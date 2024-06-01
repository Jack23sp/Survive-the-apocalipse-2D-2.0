using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundSlot
{
    public string label;
    [Range(0f, 1f)]
    public float volume;
    public AudioClip sounds;
}


public class SoundManager : MonoBehaviour
{
    public static SoundManager singleton;
    public List<SoundSlot> ambientObjectSounds = new List<SoundSlot>();

    public int FindSoundByLabel(string soundToSearch)
    {
        for(int i = 0; i < ambientObjectSounds.Count; i++)
        {
            int index = i;
            if (ambientObjectSounds[index].label == soundToSearch)
                return index;

        }
        return -1;
    }

    void Start()
    {
        if (!singleton) singleton = this; 
    }

}
