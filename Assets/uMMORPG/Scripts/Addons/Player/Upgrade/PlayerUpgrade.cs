using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;
using System;

public partial class Database
{
    class purchasedSkin
    {
        public string characterName { get; set; }
        public string itemName { get; set; }
        public string skins { get; set; }
    }

    public void Connect_Skin()
    {
        connection.CreateTable<purchasedSkin>();
    }

    public void SaveSkin(Player player)
    {
        PlayerUpgrade skin = player.GetComponent<PlayerUpgrade>();

        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM purchasedSkin WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        for (int i = 0; i < skin.weaponSkins.Count; i++)
        {
            string sk = string.Empty;
            for(int e = 0; e < skin.weaponSkins[i].buyedSkin.Length;e++)
            {
                if(sk == string.Empty)
                {
                    sk = skin.weaponSkins[i].buyedSkin[e].ToString() + ";";
                }
                else
                {
                    sk = sk + skin.weaponSkins[i].buyedSkin[e].ToString() + ";";
                }
            }
            connection.InsertOrReplace(new purchasedSkin
            {
                characterName = player.name,
                itemName = skin.weaponSkins[i].itemName,
                skins = sk
            });
        }

    }
    public void LoadSkin(Player player)
    {
        PlayerUpgrade skins = player.GetComponent<PlayerUpgrade>();

        foreach (purchasedSkin row in connection.Query<purchasedSkin>("SELECT * FROM purchasedSkin WHERE characterName=?", player.name))
        {
            WeaponSkin sp = new WeaponSkin();
            sp.itemName = row.itemName;
            sp.buyedSkin = new int[0];
            int[] buyedSkinInt = new int[0];
            string[] buyedSkin = row.skins.Split(";");
            if (buyedSkin.Length > 0)
            {
                for (int i = 0; i < buyedSkin.Length - 1; i++)
                {
                    sp.buyedSkin = ConvertIntArray(buyedSkin, buyedSkinInt);
                }
                skins.weaponSkins.Add(sp);
            }
        }
    }

    int[] ConvertIntArray(string[] arrayDiStringhe, int[] arrayInteri)
    {
        arrayInteri = new int[arrayDiStringhe.Length];

        for (int i = 0; i < arrayDiStringhe.Length -1; i++)
        {
            arrayInteri[i] = int.Parse(arrayDiStringhe[i]);
        }

        return arrayInteri;
    }

}


[System.Serializable]
public partial struct WeaponSkin
{
    public string itemName;
    public int[] buyedSkin;
}

public partial class Player
{
    [HideInInspector] public PlayerUpgrade playerUpgrade;
}


public class PlayerUpgrade : NetworkBehaviour
{
    private Player player;
    public readonly SyncList<WeaponSkin> weaponSkins = new SyncList<WeaponSkin>();

    [HideInInspector] public WeaponObject playerWeapon;

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerUpgrade = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    [Command]
    public void CmdRepairItem(int index, string itemName)
    {
        if (player.inventory.slots[index].item.data.name == itemName)
        {
            ItemSlot slot = player.inventory.slots[index];
            for (int e = 0; e < slot.item.data.repairItems.Count; e++)
            {
                int index_e = e;
                if (player.inventory.CountItem(new Item(slot.item.data.repairItems[index_e].items)) < slot.item.data.repairItems[index_e].amount)
                {
                    return;
                }
            }


            for (int e = 0; e < slot.item.data.repairItems.Count; e++)
            {
                int index_e = e;
                player.inventory.RemoveItem(new Item(slot.item.data.repairItems[index_e].items), slot.item.data.repairItems[index_e].amount);
            }

            slot.item.currentDurability = slot.item.data.maxDurability.Get(slot.item.durabilityLevel);
            player.inventory.slots[index] = slot;
        }
    }


    [Command]
    public void CmdEquipAccessory(int inventoryIndex, int accessorytype, int accessoryIndex, string accessoryName, string itemName, int type)
    {
        ItemSlot accessoryItem = new ItemSlot();
        ItemSlot weaponItem = new ItemSlot();
        switch (type)
        {
            case 0:
                accessoryItem = player.inventory.slots[accessoryIndex];
                weaponItem = player.inventory.slots[inventoryIndex];
                break;
            case 1:
                accessoryItem = player.inventory.slots[accessoryIndex];
                weaponItem = player.equipment.slots[inventoryIndex];
                break;
            case 2:
                accessoryItem = player.inventory.slots[accessoryIndex];
                weaponItem = player.playerBelt.belt[inventoryIndex];
                break;
        }

        int presentItem = -1;

        if (weaponItem.amount > 0 &&
            accessoryItem.amount > 0 &&
            weaponItem.item.data.name == itemName &&
            accessoryItem.item.data.name == accessoryName)
        {

            //Check if present
            for (int i = 0; i < weaponItem.item.accessories.Length; i++)
            {
                int index_i = i;
                if ((Convert.ToInt32(weaponItem.item.accessories[index_i].data.accessoriesType) == accessorytype))
                {
                    presentItem = index_i;
                }
            }

        }           
        ManageAccesssory(inventoryIndex, accessoryIndex, presentItem, type);
    }

    [Server]
    public void ManageAccesssory(int inventoryIndex, int accessoryIndex, int alreadyPresent, int type)
    {
        ItemSlot slot = new ItemSlot();
        ItemSlot newAccessory = player.inventory.slots[accessoryIndex];

        switch (type)
        {
            case 0:
                slot = player.inventory.slots[inventoryIndex];
                break;
            case 1:
                slot = player.equipment.slots[inventoryIndex];
                break;
            case 2:
                slot = player.playerBelt.belt[inventoryIndex];
                break;
        }

        if (slot.amount == 0 || newAccessory.amount == 0) return;

        if (alreadyPresent > -1)
        {

            Item oldAccessory = slot.item.accessories[alreadyPresent];
            slot.item.accessories[alreadyPresent] = newAccessory.item;

            switch (type)
            {
                case 0:
                    //player.inventory.slots[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[accessoryIndex] = new ItemSlot();
                    player.inventory.slots[inventoryIndex] = slot;
                    player.inventory.AddItem(oldAccessory, 1);
                    break;
                case 1:
                    //player.equipment.slots[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[accessoryIndex] = new ItemSlot();
                    player.equipment.slots[inventoryIndex] = slot;
                    player.inventory.AddItem(oldAccessory, 1);
                    break;
                case 2:
                    //player.playerBelt.belt[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[accessoryIndex] = new ItemSlot();
                    player.playerBelt.belt[inventoryIndex] = slot;
                    player.inventory.AddItem(oldAccessory, 1);
                    break;
            }
        }
        else
        {
            switch (type)
            {
                case 0:
                    List<Item> item = slot.item.accessories.ToList();
                    item.Add(newAccessory.item);
                    slot.item.accessories = item.ToArray();
                    //player.inventory.slots[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[accessoryIndex] = new ItemSlot();
                    player.inventory.slots[inventoryIndex] = slot;
                    break;
                case 1:
                    List<Item> item1 = slot.item.accessories.ToList();
                    item1.Add(newAccessory.item);
                    slot.item.accessories = item1.ToArray();
                    //player.equipment.slots[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[accessoryIndex] = new ItemSlot();
                    player.equipment.slots[inventoryIndex] = slot;
                    break;
                case 2:
                    List<Item> item2 = slot.item.accessories.ToList();
                    item2.Add(newAccessory.item);
                    slot.item.accessories = item2.ToArray();
                    //player.playerBelt.belt[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[accessoryIndex] = new ItemSlot();
                    player.playerBelt.belt[inventoryIndex] = slot;
                    break;
            }
        }
    }

    [Command]
    public void CmdSelectSkin (int inventoryIndex, int selectedSkin, string selectedInventoryName, int type)
    {
        ItemSlot slot = player.inventory.slots[inventoryIndex];

        switch (type)
        {
            case 0:
                slot = player.inventory.slots[inventoryIndex];
                break;
            case 1:
                slot = player.equipment.slots[inventoryIndex];
                break;
            case 2:
                slot = player.playerBelt.belt[inventoryIndex];
                break;
        }

        if (slot.item.name == selectedInventoryName)
        {
            slot.item.skin = selectedSkin;

            switch (type)
            {
                case 0:
                    //player.inventory.slots[inventoryIndex] = new ItemSlot();
                    player.inventory.slots[inventoryIndex] = slot;
                    break;
                case 1:
                    //player.equipment.slots[inventoryIndex] = new ItemSlot();
                    player.equipment.slots[inventoryIndex] = slot;
                    break;
                case 2:
                    //player.playerBelt.belt[inventoryIndex] = new ItemSlot();
                    player.playerBelt.belt[inventoryIndex] = slot;
                    break;
            }
        }
    }

    [Command]
    public void CmdSetSkin(int inventoryIndex, int selectedSkin, string selectedInventoryName, int type)
    {
        if (SkinManager.singleton.gold)
        {
            if (player.gold < SkinManager.singleton.costToBuySkin)
            {
                return;
            }
        }
        else
        {
            if (player.itemMall.coins < SkinManager.singleton.costToBuySkin)
            {
                return;
            }
        }

        ItemSlot slot = player.inventory.slots[inventoryIndex];

        switch (type)
        {
            case 0:
                slot = player.inventory.slots[inventoryIndex];
                break;
            case 1:
                slot = player.equipment.slots[inventoryIndex];
                break;
            case 2:
                slot = player.playerBelt.belt[inventoryIndex];
                break;
        }

        if (slot.item.name == selectedInventoryName)
        {
            bool containSkin = false;

            for (int i = 0; i < weaponSkins.Count; i++)
            {
                int index_i = i;
                if (weaponSkins[index_i].itemName == slot.item.name)
                {
                    if (!weaponSkins[index_i].buyedSkin.Contains(selectedSkin))
                    {
                        containSkin = false;
                    }
                }
            }

            if (!containSkin)
            {
                for (int e = 0; e < weaponSkins.Count; e++)
                {
                    int index_e = e;
                    if (weaponSkins[index_e].itemName == player.inventory.slots[inventoryIndex].item.name)
                    {
                        WeaponSkin weaponSkin = weaponSkins[index_e];
                        List<int> weaponSkinL = weaponSkin.buyedSkin.ToList();
                        weaponSkinL.Add(selectedSkin);
                        weaponSkin.buyedSkin = weaponSkinL.ToArray();
                        weaponSkins[index_e] = weaponSkin;
                        containSkin = true;
                    }
                }
                if (!containSkin)
                {
                    WeaponSkin weaponSkin1 = new WeaponSkin();
                    weaponSkin1.buyedSkin = new int[0];
                    weaponSkin1.itemName = player.inventory.slots[inventoryIndex].item.name;
                    List<int> weaponSkinL1 = weaponSkin1.buyedSkin.ToList();
                    weaponSkinL1.Add(selectedSkin);
                    weaponSkin1.buyedSkin = weaponSkinL1.ToArray();
                    if (!weaponSkins.Contains(weaponSkin1)) weaponSkins.Add(weaponSkin1);
                }
                slot.item.skin = selectedSkin;
                switch (type)
                {
                    case 0:
                        //player.inventory.slots[inventoryIndex] = new ItemSlot();
                        player.inventory.slots[inventoryIndex] = slot;
                        break;
                    case 1:
                        //player.equipment.slots[inventoryIndex] = new ItemSlot();
                        player.equipment.slots[inventoryIndex] = slot;
                        break;
                    case 2:
                        //player.playerBelt.belt[inventoryIndex] = new ItemSlot();
                        player.playerBelt.belt[inventoryIndex] = slot;
                        break;
                }

            }
            else if (containSkin == true)
            {
                slot.item.skin = selectedSkin;
                switch (type)
                {
                    case 0:
                        //player.inventory.slots[inventoryIndex] = new ItemSlot();
                        player.inventory.slots[inventoryIndex] = slot;
                        break;
                    case 1:
                        //player.equipment.slots[inventoryIndex] = new ItemSlot();
                        player.equipment.slots[inventoryIndex] = slot;
                        break;
                    case 2:
                        //player.playerBelt.belt[inventoryIndex] = new ItemSlot();
                        player.playerBelt.belt[inventoryIndex] = slot;
                        break;
                }
            }

            if (SkinManager.singleton.gold)
            {
                player.gold -= SkinManager.singleton.costToBuySkin;
            }
            else
            {
                player.itemMall.coins -= SkinManager.singleton.costToBuySkin;
            }
        }
    }

    [Command]
    public void CmdUnequipAccessories(int inventoryIndex, int accessoryIndex, int type)
    {
        ItemSlot slot = new ItemSlot();
        switch (type)
        {
            case 0:
                slot = player.inventory.slots[inventoryIndex];
                break;
            case 1:
                slot = player.equipment.slots[inventoryIndex];
                break;
            case 2:
                slot = player.playerBelt.belt[inventoryIndex];
                break;
        }

        Item accessoryItem = slot.item.accessories[accessoryIndex];

        if (player.inventory.CanAddItem(accessoryItem, 1))
        {
            player.inventory.AddItem(accessoryItem, 1);

            List<Item> items = new List<Item>();
            items = slot.item.accessories.ToList();
            items.RemoveAt(accessoryIndex);
            slot.item.accessories = items.ToArray();

            switch (type)
            {
                case 0:
                    player.inventory.slots[inventoryIndex] = slot;
                    break;
                case 1:
                    player.equipment.slots[inventoryIndex] = slot;
                    break;
                case 2:
                    player.playerBelt.belt[inventoryIndex] = slot;
                    break;
            }

        }
    }

    public void SetSkin(ItemSlot slot)
    {
        player.GetComponent<PlayerWeaponIK>().Assign();

        if (WeaponHolder.singleton)
        {
            playerWeapon = null;
            if (slot.amount > 0)
            {
                if (((WeaponItem)slot.item.data).weaponToSpawn == false)
                {
                    Debug.Log("You probably missing assign weaponToSpawn to item : " + slot.item.name);
                    return;
                }
                for (int y = 0; y < player.playerWeaponIK.weaponsHolder.Count; y++)
                {
                    if (player.playerWeaponIK.weaponsHolder[y].parent) 
                        player.playerWeaponIK.weaponsHolder[y].parent.SetActive(player.playerWeaponIK.weaponsHolder[y].weaponHolder.weaponObject.name.Replace("(Clone)", string.Empty) == 
                                                                                ((WeaponItem)slot.item.data).weaponToSpawn.name);

                    if (player.playerWeaponIK.weaponsHolder[y].weaponHolder.weaponObject.name.Replace("(Clone)", string.Empty) == 
                                                                                                    ((WeaponItem)slot.item.data).weaponToSpawn.name)
                    {
                        player.playerEquipment.weaponIK = player.playerWeaponIK.weaponsHolder[y].weaponHolder;
                        if (player.playerWeaponIK.weaponsHolder[y].parent) 
                            playerWeapon = player.playerWeaponIK.weaponsHolder[y].parent.GetComponent<WeaponObject>();
                    }
                }
                if (playerWeapon)
                {
                    for (int i = 0; i < playerWeapon.weaponPieces.Count; i++)
                    {
                        int index_i = i;
                        playerWeapon.weaponPieces[index_i].renderer.gameObject.SetActive(false);
                    }

                    for (int e = 0; e < slot.item.accessories.Count(); e++)
                    {
                        int index_e = e;
                        for (int i = 0; i < playerWeapon.weaponPieces.Count; i++)
                        {
                            int index_i = i;
                            if (playerWeapon.weaponPieces[index_i].item.name == slot.item.accessories[index_e].name)
                            {
                                playerWeapon.weaponPieces[index_i].renderer.gameObject.SetActive(true);
                                playerWeapon.weaponPieces[index_i].renderer.material = SkinManager.singleton.weaponAccessoryMaterials[slot.item.accessories[index_e].skin];
                            }
                        }
                    }
                }
            }
            else
            {
                player.playerEquipment.weaponIK = null;
                for (int y = 0; y < player.playerWeaponIK.weaponsHolder.Count; y++)
                {
                    if (player.playerWeaponIK.weaponsHolder[y].parent) player.playerWeaponIK.weaponsHolder[y].parent.SetActive(false);
                }
            }

        }
    }


    public bool CheckIfAlreadyBuySkinForThisAccessory(int accessoryWeaponName, int skin)
    {
        bool cont = false;

        for (int i = 0; i < weaponSkins.Count; i++)
        {
            int index_i = i;
            if (weaponSkins[index_i].itemName == player.inventory.slots[accessoryWeaponName].item.name)
            {
                if (weaponSkins[index_i].buyedSkin.Contains(skin))
                {
                    cont = true;
                }
            }
        }

        return cont;
    }
}
