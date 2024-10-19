using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerResources playerResources;
}


public class PlayerResources : NetworkBehaviour
{
    private Player player;
    public AudioSource audioSourceAmbientHit;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerResources = this;
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

    [ClientRpc]
    public void RpcPlaySound(int soundType)
    {
        if (!player.playerOptions.blockSound)
        {
            audioSourceAmbientHit.volume = SoundManager.singleton.ambientObjectSounds[soundType].volume;
            audioSourceAmbientHit.clip = SoundManager.singleton.ambientObjectSounds[soundType].sounds;
            audioSourceAmbientHit.Play();
        }
    }
}
