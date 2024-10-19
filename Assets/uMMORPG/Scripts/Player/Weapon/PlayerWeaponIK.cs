using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerWeaponIK playerWeaponIK;
}

[System.Serializable]
public partial class WeaponIKContainer
{
    public string boneName;
    //public Transform bone;
    public WeaponIK weaponHolder;
    public GameObject parent;
}

public class PlayerWeaponIK : NetworkBehaviour
{
    private Player player;

    public List<WeaponIKContainer> weaponsHolder;
    public List<WeaponIKContainer> feetPlacer;

    private bool spawn = false;
    private bool spawnFeet = false;

    private PlayerCharacterCreation playerCharacterCreation;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerWeaponIK = this;
        playerCharacterCreation = GetComponent<PlayerCharacterCreation>();
    }

    public void Spawn()
    {
        if (!spawn && GetComponent<NetworkIdentity>().isClient)
        {

            for (int i = 0; i < weaponsHolder.Count; i++)
            {
                weaponsHolder[i].parent = Instantiate(weaponsHolder[i].weaponHolder.weaponObject);
                weaponsHolder[i].parent.name.Replace("(Clone)", string.Empty);
                weaponsHolder[i].parent.transform.parent = Utilities.FindChildRecursive(GetComponent<PlayerCharacterCreation>().playerChildObject.transform, weaponsHolder[i].boneName);
                weaponsHolder[i].parent.transform.localPosition = weaponsHolder[i].weaponHolder.idle.pos;
                weaponsHolder[i].parent.transform.localRotation = new Quaternion(weaponsHolder[i].weaponHolder.idle.rot.x,
                                                      weaponsHolder[i].weaponHolder.idle.rot.y,
                                                      weaponsHolder[i].weaponHolder.idle.rot.z, 0);
                weaponsHolder[i].parent.gameObject.SetActive(false);
            }
            spawn = true;
            player.ManageState(player.state, player.state);
        }
    }

    public void SpawnFeet()
    {
        if (!spawnFeet && GetComponent<NetworkIdentity>().isClient)
        {
            if(!playerCharacterCreation) playerCharacterCreation = GetComponent<PlayerCharacterCreation>();

            for (int i = 0; i < feetPlacer.Count; i++)
            {
                feetPlacer[i].parent = new GameObject();
                feetPlacer[i].parent.name.Replace("(Clone)", string.Empty);
                feetPlacer[i].parent.transform.parent = Utilities.FindChildRecursive(playerCharacterCreation.playerChildObject.transform, feetPlacer[i].boneName);
                feetPlacer[i].parent.transform.localPosition = Vector3.zero;
                feetPlacer[i].parent.transform.localRotation = playerCharacterCreation.playerChildObject.transform.rotation;

                //feetPlacer[i].parent.layer = player.isLocalPlayer ? LayerMask.NameToLayer("PersonalPlayer") : LayerMask.NameToLayer("NotPersonalPlayer");

                if (i == 0)
                {
                    playerCharacterCreation.playerChildObject.GetComponent<PlayerSmokeParticles>().leftFoodSmokePlacer = feetPlacer[i].parent;
                }
                else
                {
                    playerCharacterCreation.playerChildObject.GetComponent<PlayerSmokeParticles>().rightFoodSmokePlacer = feetPlacer[i].parent;
                }
                feetPlacer[i].parent.gameObject.SetActive(true);
            }
            spawnFeet = true;
        }
    }

}
