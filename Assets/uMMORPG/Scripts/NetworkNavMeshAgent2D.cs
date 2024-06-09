// UNET's current NetworkTransform is really laggy, so we make it smooth by
// simply synchronizing the agent's destination. We could also lerp between
// the transform positions, but this is much easier and saves lots of bandwidth.
//
// Using a NavMeshAgent also has the benefit that no rotation has to be synced
// while moving.
//
// Notes:
//
// - Teleportations have to be detected and synchronized properly
// - Caching the agent won't work because serialization sometimes happens
//   before awake/start
// - We also need the stopping distance, otherwise entities move too far.
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NavMeshAgent2D))]
public class NetworkNavMeshAgent2D : NetworkBehaviour
{
    public NavMeshAgent2D agent; // assign in Inspector (instead of GetComponent)
    Vector2 requiredVelocity; // to apply received velocity in Update constanly

    // remember last serialized values for dirty bit
    Vector2 lastSerializedDestination;
    Vector2 lastSerializedVelocity;

    // had path since last time? for warp detection
    bool hadPath = false;

    // has path if 'hasPath' or while computation pending
    bool HasPath() => agent.enabled ? (agent.hasPath || agent.pathPending) : false;

    bool hasPath;
    private bool isServerObject;
    private bool isClientObject;

    public override void OnStartServer()
    {
        base.OnStartServer();
        isServerObject = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        isClientObject = true;
    }

    void Update()
    {
        if (isServerObject && agent.sync)
        {
            // detect move mode
            if (agent.enabled)
                hasPath = HasPath();

            // click movement and destination changed since last sync?
            if (hasPath && agent.enabled && agent.destination != lastSerializedDestination)
            {
                //Debug.LogWarning(name + " dirty because destination changed from: " + lastSerializedDestination + " to " + agent.destination + " hasPath=" + agent.hasPath + " pathPending=" + agent.pathPending);
                SetSyncVarDirtyBit(1);
            }
            // wasd movement and velocity changed since last sync?
            else if (!hasPath && agent.enabled && agent.velocity != lastSerializedVelocity)
            {
                //Debug.LogWarning(name + " dirty because velocity changed from: " + lastSerializedVelocity + " to " + agent.velocity);
                SetSyncVarDirtyBit(1);
            }
            // NOTE: no automatic warp detection.
            //       Entity.Warp calls RpcWarped for 100% reliable detection.
            //
            // neither of those, but had path before and not anymore now?
            // then agent.Reset must have been called
            else if (hadPath && !hasPath)
            {
                //Debug.LogWarning(name + " agent.Reset detected");
                SetSyncVarDirtyBit(1);
            }

            hadPath = hasPath;
        }
        else if (isClientObject)
        {
            // apply velocity constantly, not just in OnDeserialize
            // (not on host because server handles it already anyway)
            if (requiredVelocity != Vector2.zero && agent.enabled)
            {
                agent.ResetMovement(); // needed after click movement before we can use .velocity
                agent.velocity = requiredVelocity;
            }
        }
    }

    [ClientRpc]
    public void RpcWarp(Vector2 position)
    {
        if (agent.enabled)
            agent.Warp(position);
    }

    // server-side serialization
    public override void OnSerialize(NetworkWriter writer, bool initialState)
    {
        // always send position so client knows if he's too far off and needs warp
        writer.WriteVector2((Vector2)transform.position);

        // always send speed in case it's modified by something
        if (agent.enabled)
            writer.WriteFloat(agent.speed);

        // click or wasd movement?
        // (no need to send everything all the time, saves bandwidth)
        hasPath = HasPath();
        writer.WriteBool(hasPath);
        if (hasPath)
        {
            // destination
            if (agent.enabled)
                writer.WriteVector2(agent.destination);

            // always send stopping distance because monsters might stop early etc.
            if (agent.enabled)
                writer.WriteFloat(agent.stoppingDistance);

            // remember last serialized path so we do it again if it changed.
            // (first OnSerialize never seems to detect path yet for whatever
            //  reason, so this way we can be 100% sure that it's called again
            //  as soon as the path was detected)
            if (agent.enabled)
                lastSerializedDestination = agent.destination;
        }
        else
        {
            // velocity
            if (agent.enabled)
                writer.WriteVector2(agent.velocity);

            // remember last serialized velocity
            if (agent.enabled)
                lastSerializedVelocity = agent.velocity;
        }
    }

    // client-side deserialization
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        // read position, speed and movement type
        Vector2 position = reader.ReadVector2();
        if(agent.enabled) agent.speed = reader.ReadFloat();
        hasPath = reader.ReadBool();

        // IMPORTANT: when spawning (=initialState), always warp to position!
        //            respawning a scene object might otherwise stay at the
        //            previous position on the client, causing movement desync.
        //            => fixes https://github.com/vis2k/uMMORPG2D/issues/4
        if (initialState)
        {
            if (agent.enabled) agent.Warp(position);
        }

        // click or wasd movement?
        if (hasPath)
        {
            // read destination and stopping distance
            Vector2 destination = reader.ReadVector2();
            float stoppingDistance = reader.ReadFloat();
            //Debug.Log("OnDeserialize: click: " + destination);

            // try setting destination if on navmesh
            // (might not be while falling from the sky after joining etc.)
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.stoppingDistance = stoppingDistance;
                agent.destination = destination;
            }
            else Debug.LogWarning("NetworkNavMeshAgent.OnDeserialize: agent not on NavMesh, name=" + name + " position=" + transform.position + " destination=" + destination);

            requiredVelocity = Vector2.zero; // reset just to be sure
        }
        else
        {
            // read velocity
            Vector2 velocity = reader.ReadVector2();
            //Debug.Log("OnDeserialize: wasd: " + velocity);

            // cancel path if we are already doing click movement, otherwise
            // we will slide
            // => important if agent.Reset was called too. otherwise we it keeps
            //    sliding.
            // => ResetPath and not ResetMovement because we really only want to
            //    reset the path and not mess with velocity until Update()
            if (agent.enabled) agent.ResetPath();

            // apply required velocity in Update later
            requiredVelocity = velocity;
        }

        // rubberbanding: if we are too far off because of a rapid position
        // change or latency, then warp
        // -> agent moves 'speed' meter per seconds
        // -> if we are speed * 2 units behind, then we teleport
        //    (using speed is better than using a hardcoded value)
        // -> we use speed * 2 for update/network latency tolerance. player
        //    might have moved quit a bit already before OnSerialize was called
        //    on the server.
        if (agent.enabled && Vector2.Distance(transform.position, position) > agent.speed * 2 && agent.isOnNavMesh)
        {
            agent.Warp(position);
            //Debug.Log(name + " rubberbanding to " + position);
        }
    }
}