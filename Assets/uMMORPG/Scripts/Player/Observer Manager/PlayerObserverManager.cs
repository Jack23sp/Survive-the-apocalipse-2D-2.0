using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Rendering.Universal;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerObserverManager playerObserver;
}

public class PlayerObserverManager : NetworkBehaviour
{
    private Player player;

    public List<Collider2D> localPlayerObserversOld = new List<Collider2D>();
    public List<Collider2D> localPlayerObserversNew = new List<Collider2D>();
    public List<GameObject> addedObservers = new List<GameObject>();
    public List<GameObject> removedObservers = new List<GameObject>();

    public float timeBetweenSearch;
    public float distanceRange = 10;
    public LayerMask targetLayer;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerObserver = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Assign();
        InvokeRepeating(nameof(ManageSyncDictionary), timeBetweenSearch, timeBetweenSearch);
    }

    public void ManageSyncDictionary()
    {
        if (player.state == "MOVING")
        {
            if (localPlayerObserversOld != localPlayerObserversNew)
            {
                FindNetworkObject();
                ManagerGameObjectAdded();
                ManagerGameObjectRemoved();
            }
        }
    }

    public void ManagerGameObjectAdded()
    {
        for(int i = 0; i < addedObservers.Count; i++)
        {
            if (addedObservers[i].GetComponent<ShadowCaster2D>())
            {
                addedObservers[i].GetComponent<ShadowCaster2D>().enabled = true;
            }
            if (addedObservers[i].GetComponentInChildren<ShadowCaster2D>())
            {
                addedObservers[i].GetComponentInChildren<ShadowCaster2D>().enabled = true;
            }
        }
    }

    public void ManagerGameObjectRemoved()
    {
        for (int i = 0; i < removedObservers.Count; i++)
        {
            if (removedObservers[i].GetComponent<ShadowCaster2D>())
            {
                removedObservers[i].GetComponent<ShadowCaster2D>().enabled = false;
            }
            if (removedObservers[i].GetComponentInChildren<ShadowCaster2D>())
            {
                removedObservers[i].GetComponentInChildren<ShadowCaster2D>().enabled = false;
            }
        }
    }

    public List <GameObject> ManageRemoved()
    {
        removedObservers.RemoveAll(item => item == null);
        return Utilities.GetRemovedConnections(localPlayerObserversNew, localPlayerObserversOld);
    }

    public List<GameObject> ManageAdded()
    {
        addedObservers.RemoveAll(item => item == null);
        return Utilities.GetNewConnections(localPlayerObserversNew, localPlayerObserversOld);
    }

    public void FindNetworkObject()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, distanceRange, targetLayer);

        localPlayerObserversNew = colliders.ToList();
        localPlayerObserversOld.RemoveAll(item => item == null);
        if (localPlayerObserversNew.OrderByDescending(o => o.gameObject.name) != localPlayerObserversOld.OrderByDescending(o => o.gameObject.name))
        {
            removedObservers = ManageRemoved();
            addedObservers = ManageAdded();

            localPlayerObserversOld.RemoveAll(item => item == null);
            localPlayerObserversOld.Clear();
            localPlayerObserversNew.CopyTo(localPlayerObserversOld);
            localPlayerObserversNew.Clear();
        }
    }
}


