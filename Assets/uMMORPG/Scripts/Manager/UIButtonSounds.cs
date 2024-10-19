using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ButtonSounds
{
    public string type;
    public AudioClip sound;
}

public class UIButtonSounds : MonoBehaviour
{
    public static UIButtonSounds singleton;
    
    public List<ButtonSounds> audioClips = new List<ButtonSounds>();

    public AudioSource audioSource;

    public void Start()
    {
        if(!singleton) singleton = this;
    }

    public void ButtonPress(int sounds)
    {
        if (Player.localPlayer)
        {
            if(!Player.localPlayer.playerOptions.blockButtonSounds)
            {
                if (audioClips[sounds].sound)
                {
                    audioSource.clip = audioClips[sounds].sound;
                    audioSource.Play();
                }
            }
        }
    }
}
