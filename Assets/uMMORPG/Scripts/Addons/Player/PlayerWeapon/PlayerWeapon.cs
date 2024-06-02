using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerWeapon playerWeapon;
}

public class PlayerWeapon : NetworkBehaviour
{
    private Player player;
    [SyncVar] public int shooted;
    [SyncVar] public int chargedMunition;
    public LineRenderer lineRenderer;
    public List<Monster> nearMonster = new List<Monster>();


    private Vector3 destination;

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

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerWeapon = this;
    }

    public void TargetNearest()
    {
        if (player.isServer &&
           player.equipment.slots[0].amount > 0 &&
           player.equipment.slots[0].item.data is WeaponItem &&
           ((WeaponItem)player.equipment.slots[0].item.data).distanceToGrab > 0)
        {
            for (int i = 0; i < player.equipment.slots[0].item.accessories.Length; i++)
            {
                if (player.equipment.slots[0].item.accessories[i].name == "Silencer")
                {
                    return;
                }
            }

            Collider2D[] mon = Physics2D.OverlapBoxAll(player.transform.GetChild(0).transform.position, new Vector2(((WeaponItem)player.equipment.slots[0].item.data).distanceToGrab, ((WeaponItem)player.equipment.slots[0].item.data).distanceToGrab), transform.localEulerAngles.z, JoystickManager.singleton.monster);
            List<Monster> monsters = mon.Select(go => go.GetComponent<Monster>()).Where(m => m.health.current > 0).ToList();
            nearMonster = monsters.OrderBy(m => Vector2.Distance(transform.position, m.transform.position)).ToList();

            for (int i = 0; i < nearMonster.Count; i++)
            {
                nearMonster[i].monsterComponentManager.ForceAble(((WeaponItem)player.equipment.slots[0].item.data).distanceToGrab, player);
            }
        }
    }

    public void FixedUpdate()
    {
        if (player && Player.localPlayer)
        {
            if (Player.localPlayer == player)
            {
                destination = (Vector3)JoystickManager.singleton.attackJoystick.input;
                destination *= (player.playerEquipment.slots[0].amount > 0 ? player.playerEquipment.slots[0].item.data.requiredSkill.lineCast : 1f);

                if (player.playerMove.states.Contains("AIM") ||
                   player.playerMove.states.Contains("SHOOT") &&
                   JoystickManager.singleton.attackJoystick.input != Vector2.zero)
                {
                    lineRenderer.SetPosition(1, player.transform.position);
                    lineRenderer.SetPosition(0, lineRenderer.GetPosition(1) + destination);
                }
                else
                {
                    lineRenderer.SetPosition(1, player.transform.position);
                    lineRenderer.SetPosition(0, lineRenderer.GetPosition(1));
                }
            }
        }
    }

    [Command]
    public void CmdChargeMunition(string weaponName)
    {
        ChargeMunition(weaponName);
    }

    [TargetRpc]
    public void TargetStopMunitionAnimation()
    {
        UIMobileControl.singleton.animate = false;
    }

    public int CheckMunitionInMagazine()
    {
        if (player.playerEquipment.slots[0].amount == 0) return -2;
        if (player.playerEquipment.slots[0].item.data.needMunitionInMagazine)
        {
            for (int i = 0; i < player.playerEquipment.slots[0].item.accessories.Length; i++)
            {
                if (player.playerEquipment.slots[0].item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
                {
                    return player.playerEquipment.slots[0].item.accessories[i].bulletsRemaining;
                }
            }
        }
        return -1;
    }


    public bool CheckMagazine(string weaponName)
    {
        ScriptableItem inv = null;
        bool belt = false;
        int inventoryIndex = -1;
        int maxBullets = 0;

        List<string> itms = new List<string>();


        for (int i = 0; i < SkinManager.singleton.WeaponAccessories.Count; i++)
        {
            int index = i;
            if (SkinManager.singleton.WeaponAccessories[index].mainWeapon.name == weaponName)
            {
                for (int e = 0; e < SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories.Count; e++)
                {
                    int index_e = e;
                    if (SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories[index_e].accessoriesType == AccessoriesType.magazine)
                    {
                        if (!itms.Contains(SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories[index_e].name))
                        {
                            itms.Add(SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories[index_e].name);
                        }
                    }
                }
            }
        }

        for (int a = 0; a < player.playerBelt.belt.Count; a++)
        {
            int index_a = a;
            if (player.playerBelt.belt[index_a].amount > 0)
            {
                if (itms.Contains(player.playerBelt.belt[index_a].item.data.name))
                {
                    if (player.playerBelt.belt[index_a].item.bulletsRemaining > 0 && maxBullets < player.playerBelt.belt[index_a].item.bulletsRemaining)
                    {
                        inv = player.playerBelt.belt[index_a].item.data;
                        maxBullets = player.playerBelt.belt[index_a].item.bulletsRemaining;
                        inventoryIndex = index_a;
                        belt = true;
                    }
                }
            }
        }

        if (!belt)
        {
            for (int a = 0; a < player.inventory.slots.Count; a++)
            {
                int index_a = a;
                if (player.inventory.slots[index_a].amount > 0)
                {
                    if (itms.Contains(player.inventory.slots[index_a].item.data.name))
                    {
                        if (player.inventory.slots[index_a].item.bulletsRemaining > 0 && maxBullets < player.inventory.slots[index_a].item.bulletsRemaining)
                        {
                            inv = player.inventory.slots[index_a].item.data;
                            maxBullets = player.inventory.slots[index_a].item.bulletsRemaining;
                            inventoryIndex = index_a;
                        }
                    }
                }
            }
        }
        return inv != null;

    }

    [TargetRpc]
    public void TargetPlayReloadSound()
    {
        player.playerSounds.PlaySounds(((MunitionSkill)player.playerEquipment.slots[0].item.data.requiredSkill).projectile.type, "2");
    }

    public void ChargeMunition(string weaponName)
    {
        if (player.equipment.slots[0].amount > 0)
        {
            ItemSlot currentWeapon = player.equipment.slots[0];
            ScriptableItem inv = null;
            bool belt = false;
            int inventoryIndex = -1;
            int maxBullets = 0;

            List<string> itms = new List<string>();

            for (int i = 0; i < SkinManager.singleton.WeaponAccessories.Count; i++)
            {
                int index = i;
                if (SkinManager.singleton.WeaponAccessories[index].mainWeapon.name == weaponName)
                {
                    for (int e = 0; e < SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories.Count; e++)
                    {
                        int index_e = e;
                        if (SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories[index_e].accessoriesType == AccessoriesType.magazine)
                        {
                            if (!itms.Contains(SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories[index_e].name))
                            {
                                itms.Add(SkinManager.singleton.WeaponAccessories[index].weaponItemAccessories[index_e].name);
                            }
                        }
                    }
                }
            }

            for (int a = 0; a < player.playerBelt.belt.Count; a++)
            {
                int index_a = a;
                if (player.playerBelt.belt[index_a].amount > 0)
                {
                    if (itms.Contains(player.playerBelt.belt[index_a].item.data.name))
                    {
                        if (player.playerBelt.belt[index_a].item.bulletsRemaining > 0 && maxBullets < player.playerBelt.belt[index_a].item.bulletsRemaining)
                        {
                            inv = player.playerBelt.belt[index_a].item.data;
                            maxBullets = player.playerBelt.belt[index_a].item.bulletsRemaining;
                            inventoryIndex = index_a;
                            belt = true;
                        }
                    }
                }
            }

            if (!belt)
            {
                for (int a = 0; a < player.inventory.slots.Count; a++)
                {
                    int index_a = a;
                    if (player.inventory.slots[index_a].amount > 0)
                    {
                        if (itms.Contains(player.inventory.slots[index_a].item.data.name))
                        {
                            if (player.inventory.slots[index_a].item.bulletsRemaining > 0 && maxBullets < player.inventory.slots[index_a].item.bulletsRemaining)
                            {
                                inv = player.inventory.slots[index_a].item.data;
                                maxBullets = player.inventory.slots[index_a].item.bulletsRemaining;
                                inventoryIndex = index_a;
                            }
                        }
                    }
                }
            }

            if (inv != null)
            {
                TargetPlayReloadSound();
                StartCoroutine(CallEffectiveChargeAfterDelay(inv, maxBullets, belt, inventoryIndex));
            }
        }
    }

    private IEnumerator CallEffectiveChargeAfterDelay(ScriptableItem inv, int maxBullets, bool belt, int inventoryIndex)
    {
        float delay = player.playerSounds.RetrieveSoundLength(((MunitionSkill)player.playerEquipment.slots[0].item.data.requiredSkill).projectile.type, "2") - 0.5f;
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        EffectiveCharge(inv, maxBullets, belt, inventoryIndex);
    }

    public void EffectiveCharge(ScriptableItem Sitem, int bullets, bool isBelt, int index)
    {
        ItemSlot currentWeapon = player.equipment.slots[0];
        ItemSlot itemSlot = isBelt ? player.playerBelt.belt[index] : player.inventory.slots[index];
        if (itemSlot.amount == 0) return;
        bool stopAnimation = false;

        for (int i = 0; i < currentWeapon.item.accessories.Length; i++)
        {
            int index_i = i;
            if (currentWeapon.item.accessories[index_i].data.accessoriesType == AccessoriesType.magazine)
            {
                Item item = currentWeapon.item.accessories[index_i];
                Item item2 = currentWeapon.item.accessories[index_i];
                item = itemSlot.item;
                player.playerWeapon.chargedMunition = item.bulletsRemaining;
                player.playerWeapon.shooted = 0;
                currentWeapon.item.accessories[index_i] = item;
                player.playerEquipment.slots[0] = currentWeapon;
                stopAnimation = true;
                if(isBelt)
                    player.playerBelt.belt[index] = new ItemSlot(item2, 1);
                else
                    player.inventory.slots[index] = new ItemSlot(item2, 1);
                return;
            }
        }

        List<Item> acc = currentWeapon.item.accessories.ToList();
        //Item itm2 = new Item(Sitem);
        //itm2.bulletsRemaining = bullets;
        acc.Add(itemSlot.item);
        currentWeapon.item.accessories = acc.ToArray();
        player.playerWeapon.chargedMunition = itemSlot.item.bulletsRemaining;
        player.playerWeapon.shooted = 0;
        player.playerEquipment.slots[0] = currentWeapon;
        stopAnimation = true;

        if (isBelt)
            player.playerBelt.belt[index] = new ItemSlot();
        else
            player.inventory.slots[index] = new ItemSlot();

        if (stopAnimation || !currentWeapon.item.data.needMunitionInMagazine) TargetStopMunitionAnimation();
    }

    [Command]
    public void CmdRemoveMagazine(string weaponName)
    {
        if (player.playerEquipment.slots[0].amount > 0)
        {
            ItemSlot equip = player.playerEquipment.slots[0];

            for (int i = 0; i < equip.item.accessories.Length; i++)
            {
                if (equip.item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
                {
                    if (player.inventory.CanAddItem(equip.item.accessories[i], 1))
                    {
                        player.inventory.AddItem(equip.item.accessories[i], 1);

                        List<Item> acc = equip.item.accessories.ToList();
                        acc.RemoveAt(acc.IndexOf(equip.item.accessories[i]));
                        equip.item.accessories = acc.ToArray();
                        player.playerEquipment.slots[0] = equip;
                        return;
                    }
                }
            }
        }
    }

    public bool CheckAbleMunitionButton()
    {
        if (player.playerEquipment.slots[0].amount > 0)
        {
            ItemSlot equip = player.playerEquipment.slots[0];

            for (int i = 0; i < equip.item.accessories.Length; i++)
            {
                if (equip.item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            return false;
        }
        return false;
    }

}
