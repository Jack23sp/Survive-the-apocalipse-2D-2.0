using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public partial class Player
{
    [HideInInspector] public PlayerSounds playerSounds;
}

[System.Serializable]
public partial class WhereToPlaySounds
{
    public string layerIndicator;
    public AudioSource source;
    public AudioSource sourceAim;
    public AudioClip aim;
    public AudioClip normal;
    public AudioClip silenced;
    public AudioClip recharge;
    public AudioClip empty;
    public float volume = 0.5f;
}

public class PlayerSounds : MonoBehaviour
{
    private Player player;
    public AudioSource weaponAimAudioSource;
    public AudioSource weaponAudioSource;
    public AudioSource resourceAudioSource;
    public List<WhereToPlaySounds> whereToPlaySounds = new List<WhereToPlaySounds>();

    void Awake()
    {
        player = GetComponent<Player>();
        player.playerSounds = this;
    }

    public void PlaySounds(string types, string parameters)
    {
        int type = Convert.ToInt32(types);
        int param = Convert.ToInt32(parameters);
        if (type == -1) return;
        if (!player.playerOptions.blockSound)
        {
            if (player.playerEquipment.slots[0].amount > 0)
            {
                if (((WeaponItem)player.playerEquipment.slots[0].item.data).needMunitionInMagazine)
                {
                    if (param == 0) // no ammo
                    {
                        whereToPlaySounds[type].source.volume = whereToPlaySounds[type].sourceAim.volume = whereToPlaySounds[type].volume;
                        whereToPlaySounds[type].sourceAim.clip = whereToPlaySounds[type].empty;
                        whereToPlaySounds[type].sourceAim.Play();
                    }
                    else if (param == 1)
                    {
                        for (int i = 0; i < player.playerEquipment.slots[0].item.accessories.Length; i++)
                        {
                            if (player.playerEquipment.slots[0].item.accessories[i].name == "Silencer")
                            {
                                whereToPlaySounds[type].source.clip = whereToPlaySounds[type].silenced;
                                whereToPlaySounds[type].source.Play();
                                return;
                            }
                        }
                        whereToPlaySounds[type].source.clip = whereToPlaySounds[type].normal;
                        whereToPlaySounds[type].source.Play();
                    }
                    else if (param == 2)
                    {
                        whereToPlaySounds[type].source.volume = whereToPlaySounds[type].sourceAim.volume = whereToPlaySounds[type].volume;
                        whereToPlaySounds[type].source.clip = whereToPlaySounds[type].recharge;
                        whereToPlaySounds[type].source.Play();
                    }
                    else if (param == 3)
                    {
                        whereToPlaySounds[type].source.volume = whereToPlaySounds[type].sourceAim.volume = whereToPlaySounds[type].volume;
                        whereToPlaySounds[type].source.clip = whereToPlaySounds[type].empty;
                        whereToPlaySounds[type].source.Play();
                    }
                    else if (param == 4)
                    {
                        whereToPlaySounds[type].source.volume = whereToPlaySounds[type].sourceAim.volume = whereToPlaySounds[type].volume;
                        whereToPlaySounds[type].sourceAim.clip = whereToPlaySounds[type].aim;
                        whereToPlaySounds[type].sourceAim.Play();
                    }
                }
                else
                {
                    if (param == 0) // no ammo
                    {
                        whereToPlaySounds[type].source.volume = whereToPlaySounds[type].sourceAim.volume = whereToPlaySounds[type].volume;
                        whereToPlaySounds[type].sourceAim.clip = whereToPlaySounds[type].normal;
                        whereToPlaySounds[type].sourceAim.Play();
                    }
                    else
                    {
                        whereToPlaySounds[type].sourceAim.clip = whereToPlaySounds[type].aim;
                        whereToPlaySounds[type].sourceAim.Play();
                    }
                }
            }
        }
    }


    public float RetrieveSoundLength(string types, string parameter)
    {
        int type = Convert.ToInt32(types);
        int param = Convert.ToInt32(parameter);
        if (type == -1) return 0;

        if (player.playerEquipment.slots[0].amount > 0)
        {
            if (((WeaponItem)player.playerEquipment.slots[0].item.data).needMunitionInMagazine)
            {
                if (param == 0) // no ammo
                {
                    return whereToPlaySounds[type].empty.length;
                }
                else if (param == 1)
                {
                    for (int i = 0; i < player.playerEquipment.slots[0].item.accessories.Length; i++)
                    {
                        if (player.playerEquipment.slots[0].item.accessories[i].name == "Silencer")
                        {
                            return whereToPlaySounds[type].silenced.length;
                        }
                    }
                    return whereToPlaySounds[type].normal.length;
                }
                else if (param == 2)
                {
                    return whereToPlaySounds[type].recharge.length;
                }
                else
                {
                    return whereToPlaySounds[type].empty.length;
                }
            }
            else
            {
                if (param == 0) // no ammo
                {
                    return whereToPlaySounds[type].normal.length;
                }
                else
                {
                    return whereToPlaySounds[type].aim.length;
                }
            }
        }
        return 0;
    }
}
