using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct Magazine
{
    public bool isInventory;
    public int magazineIndex;
    public int bullets;

    public Magazine(bool inv, int ind, int bull)
    {
        isInventory = inv;
        magazineIndex = ind;
        bullets = bull;
    }
}

public class UIScreenMagazine : MonoBehaviour
{
    public static UIScreenMagazine singleton;
    public GameObject panel;
    public List<MagazineSlot> magazineSlots = new List<MagazineSlot>();
    public List<Magazine> magazineInInventory = new List<Magazine>();
    public List<Magazine> magazineInBelt = new List<Magazine>();
    List<Magazine> SortedList = new List<Magazine>();

    void OnEnable()
    {
        if (!singleton) singleton = this;
        magazineInBelt.Capacity = 4;
        Refresh();
    }

    public void Refresh()
    {
        magazineInBelt.Clear();
        magazineInInventory.Clear();
        if (Player.localPlayer.equipment.slots[0].amount == 0 || !((EquipmentItem)Player.localPlayer.equipment.slots[0].item.data).needMunitionInMagazine)
        {
            panel.SetActive(false);

            for (int i = 0; i < magazineSlots.Count; i++)
            {
                magazineSlots[i].gameObject.SetActive(false);
            }
        }
        else
        {
            Search(Player.localPlayer.equipment.slots[0].item.data.name);

            for (int i = 0; i < magazineSlots.Count; i++)
            {
                magazineSlots[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < magazineInBelt.Count; i++)
            {
                magazineSlots[i].gameObject.SetActive(true);
                magazineSlots[i].image.sprite = magazineInBelt[i].isInventory ?
                                                                                Player.localPlayer.inventory.slots[magazineInBelt[i].magazineIndex].item.data.image :
                                                                                Player.localPlayer.playerBelt.belt[magazineInBelt[i].magazineIndex].item.data.image;
                magazineSlots[i].text.text = magazineInBelt[i].isInventory ?
                                                                            Player.localPlayer.inventory.slots[magazineInBelt[i].magazineIndex].item.bulletsRemaining + "/" + ((EquipmentItem)Player.localPlayer.inventory.slots[magazineInBelt[i].magazineIndex].item.data).maxMunition :
                                                                            Player.localPlayer.playerBelt.belt[magazineInBelt[i].magazineIndex].item.bulletsRemaining + "/" + ((EquipmentItem)Player.localPlayer.playerBelt.belt[magazineInBelt[i].magazineIndex].item.data).maxMunition;
            }
        }

    }

    public void Search(string weaponName)
    {
        panel.SetActive(true);
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

        for (int a = 0; a < Player.localPlayer.playerBelt.belt.Count; a++)
        {
            int index_a = a;
            if (Player.localPlayer.playerBelt.belt[index_a].amount > 0)
            {
                if (itms.Contains(Player.localPlayer.playerBelt.belt[index_a].item.data.name))
                {
                    if(!magazineInBelt.Contains(new Magazine(false, index_a, (Player.localPlayer.playerBelt.belt[index_a].item.bulletsRemaining))))
                        magazineInBelt.Add(new Magazine(false, index_a, (Player.localPlayer.playerBelt.belt[index_a].item.bulletsRemaining)));
                }
            }
        }

        for (int a = 0; a < Player.localPlayer.inventory.slots.Count; a++)
        {
            int index_a = a;
            if (Player.localPlayer.inventory.slots[index_a].amount > 0)
            {
                if (itms.Contains(Player.localPlayer.inventory.slots[index_a].item.data.name))
                {
                    if (!magazineInInventory.Contains(new Magazine(true, index_a, (Player.localPlayer.inventory.slots[index_a].item.bulletsRemaining))))
                        magazineInInventory.Add(new Magazine(true, index_a, (Player.localPlayer.inventory.slots[index_a].item.bulletsRemaining)));
                }
            }
        }
        magazineInBelt.AddRange(magazineInInventory);
        SortedList = magazineInBelt.OrderByDescending(o => o.bullets).ToList();
        magazineInBelt = SortedList;
    }
}
