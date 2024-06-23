using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSmokeParticles : MonoBehaviour
{
    public GameObject leftFoodSmokePlacer;
    public GameObject rightFoodSmokePlacer;
    public GameObject effectToSpawn;

    private GameObject instantiateObject;

    public AudioClip wetFootstep;
    public AudioClip snowyFootstep;
    public AudioClip normalFootstep;

    public float sneakVolume;
    public float runVolume;
    public float normalVolume;

    [HideInInspector] public Player player;

    public void SpawnEffect(int position)
    {
        player.audioSource.volume = player.playerMove.states.Contains("SNEAK") ? sneakVolume : player.playerMove.states.Contains("RUN") ? runVolume : normalVolume;

        if (TemperatureManager.singleton.isRainy)
        {
            if (wetFootstep != null)
            {
                player.audioSource.clip = wetFootstep;
            }
        }
        else if (TemperatureManager.singleton.isSnowy)
        {
            if (snowyFootstep != null)
            {
                player.audioSource.clip = snowyFootstep;
            }
        }
        else
        {
            if (normalFootstep != null)
            {
                player.audioSource.clip = normalFootstep;
            }
        }

        if (position == 0)
        {
            if (player.playerMove.states.Contains("RUN"))
            {
                instantiateObject = Instantiate(effectToSpawn);
                instantiateObject.transform.position = leftFoodSmokePlacer.transform.position;
            }
            if(!Player.localPlayer.playerOptions.blockFootstep && !Player.localPlayer.playerOptions.blockSound) player.audioSource.Play();
        }
        else
        {
            if (player.playerMove.states.Contains("RUN"))
            {
                instantiateObject = Instantiate(effectToSpawn);
                instantiateObject.transform.position = rightFoodSmokePlacer.transform.position;
            }
            if (!Player.localPlayer.playerOptions.blockFootstep && !Player.localPlayer.playerOptions.blockSound) player.audioSource.Play();
        }
        if(instantiateObject) instantiateObject.gameObject.layer = player.isLocalPlayer ? LayerMask.NameToLayer("PersonalPlayer") : LayerMask.NameToLayer("NotPersonalPlayer");
    }

    public void CallPlayerSounds(string state)
    {
        var substr = state.Split(",");
        player.playerSounds.PlaySounds(substr[0], substr[1]);
    }

}
