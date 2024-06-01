using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.Playables;
using System.Collections.Generic;

[Serializable]
public partial struct EquipmentInfo
{
    public string requiredCategory;
    public SubAnimation location;
    public ScriptableItemAndAmount defaultItem;
}

public partial class Player
{
    [HideInInspector] public PlayerEquipment playerEquipment;
}

[RequireComponent(typeof(PlayerInventory))]
public class PlayerEquipment : Equipment
{
    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;
    public WeaponIK weaponIK;

    // avatar Camera is only enabled while Equipment UI is active
    [Header("Avatar")]
    public Camera avatarCamera;
    public RawImage rawImage;

    [SyncVar]
    public Item magazineItem;
    private bool assigned;

    [Header("Equipment Info")]
    public EquipmentInfo[] slotInfo = {
        new EquipmentInfo{requiredCategory="Weapon", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Head", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Chest", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Legs", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shield", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Shoulders", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Hands", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Feet", location=null, defaultItem=new ScriptableItemAndAmount()}
    };

    public void Awake()
    {
        avatarCamera.targetTexture = new RenderTexture(RenderTextureManager.singleton.playerRenderTexure);
        rawImage.texture = avatarCamera.targetTexture;
    }

    public override void OnStartClient()
    {
        // setup synclist callbacks on client. no need to update and show and
        // animate equipment on server
        slots.Callback += OnEquipmentChanged;
        player.playerEquipment = this;
        // refresh all locations once (on synclist changed won't be called
        // for initial lists)
        // -> needs to happen before ProximityChecker's initial SetVis call,
        //    otherwise we get a hidden character with visible equipment
        //    (hence OnStartClient and not Start)
        for (int i = 0; i < slots.Count; ++i)
            RefreshLocation(i);


    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        slots.Callback += OnEquipmentChangedLocal;
        Invoke(nameof(ManageMunition), 2.0f);
    }

    void OnEquipmentChangedLocal(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        ManageMunition();
    }

    public int FindMunitionInMagazine()
    {
        int tot = 0;

        if (slots[0].amount > 0)
        {
            for (int i = 0; i < slots[0].item.accessories.Length; i++)
            {
                if (slots[0].item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
                {
                    tot = slots[0].item.accessories[i].bulletsRemaining;
                }
            }

        }
        return tot;
    }

    public void AssignMagazineItem()
    {
        if ((!player || slots[0].amount == 0))
        {
            if (!assigned)
            {
                Item itm = new Item(PremiumItemManager.singleton.Instantresurrect);
                magazineItem = itm;
                assigned = true;
            }
        }
        else
        {
            if (magazineItem.data)
            {
                for (int i = 0; i < slots[0].item.accessories.Length; i++)
                {
                    if (slots[0].item.accessories[i].data.accessoriesType == AccessoriesType.magazine)
                    {
                        if (magazineItem.name != slots[0].item.accessories[i].name
                           || magazineItem.bulletsRemaining != slots[0].item.accessories[i].bulletsRemaining)
                            magazineItem = slots[0].item.accessories[i];
                        assigned = false;
                    }
                }
            }
        }
    }

    public void ManageDurability()
    {
        if (UIMobileControl.singleton)
        {
            UIMobileControl uIMobileControl = UIMobileControl.singleton;

            if (slots[0].amount > 0)
            {
                uIMobileControl.durabilitySlider.gameObject.SetActive(slots[0].item.data.maxDurability.baseValue > 0);
                if (uIMobileControl.durabilitySlider.gameObject.activeInHierarchy)
                    uIMobileControl.durabilitySlider.fillAmount = (float)slots[0].item.currentDurability / (float)slots[0].item.data.maxDurability.Get(slots[0].item.durabilityLevel);
                uIMobileControl.munitionImage.gameObject.SetActive(false);
                uIMobileControl.munitionCountText.gameObject.SetActive(false);
            }
        }
    }

    public void ManageMunition()
    {
        if (UIMobileControl.singleton)
        {
            UIMobileControl uIMobileControl = UIMobileControl.singleton;
            if (!player.playerWeapon)
            {
                player.playerWeapon = player.GetComponent<PlayerWeapon>();
                player.playerWeapon.Assign();
            }

            uIMobileControl.prevMunitionCount = 0;
            if (slots[0].amount > 0)
            {
                uIMobileControl.durabilitySlider.gameObject.SetActive(slots[0].item.data.maxDurability.baseValue > 0);
                if (uIMobileControl.durabilitySlider.gameObject.activeInHierarchy)
                    uIMobileControl.durabilitySlider.fillAmount = (float)slots[0].item.currentDurability / (float)slots[0].item.data.maxDurability.Get(slots[0].item.durabilityLevel);
                uIMobileControl.munitionImage.gameObject.SetActive(false);
                uIMobileControl.munitionCountText.gameObject.SetActive(false);

                if (uIMobileControl.weaponImage)
                {
                    uIMobileControl.weaponImage.gameObject.SetActive(true);
                    uIMobileControl.weaponImage.sprite = slots[0].item.data.image;
                    uIMobileControl.weaponImage.preserveAspect = true;
                }
                if (slots[0].item.data.needMunitionInMagazine)
                {
                    uIMobileControl.munitionCountText.gameObject.SetActive(true);
                    uIMobileControl.munitionCountText.text = " - ";
                    uIMobileControl.durabilityGameobject.SetActive(true);
                    uIMobileControl.munitionImage.gameObject.SetActive(magazineItem.bulletsRemaining > 0);

                    if (uIMobileControl.munitionImage.gameObject.activeInHierarchy)
                        uIMobileControl.munitionImage.sprite = magazineItem.data.image;

                    uIMobileControl.munitionImage.preserveAspect = true;
                    //uIMobileControl.munitionCountText.text = (player.playerWeapon.chargedMunition - player.playerWeapon.shooted).ToString();
                    uIMobileControl.munitionCountText.text = magazineItem.bulletsRemaining.ToString();
                    uIMobileControl.prevMunitionCount = magazineItem.bulletsRemaining;
                }
                else
                {
                    if (((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo)
                    {
                        int totalMunition = uIMobileControl.TotalMunition(((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo);
                        if (uIMobileControl.prevMunitionCount != totalMunition)
                        {
                            uIMobileControl.durabilityGameobject.SetActive(true);
                            uIMobileControl.munitionImage.gameObject.SetActive(true);
                            uIMobileControl.munitionCountText.gameObject.SetActive(true);
                            uIMobileControl.munitionImage.sprite = ((WeaponItem)player.playerEquipment.slots[0].item.data).requiredAmmo.image;
                            uIMobileControl.munitionImage.preserveAspect = true;
                            uIMobileControl.munitionCountText.text = totalMunition.ToString();

                            uIMobileControl.prevMunitionCount = totalMunition;
                        }
                    }
                    else
                    {
                        uIMobileControl.durabilityGameobject.SetActive(false);
                    }
                }
            }
            else
            {
                if (uIMobileControl.weaponImage)
                {
                    uIMobileControl.weaponImage.gameObject.SetActive(false);
                    //uIMobileControl.durabilityGameobject.SetActive(false);
                    //uIMobileControl.durabilitySlider.gameObject.SetActive(false);
                    //uIMobileControl.munitionImage.gameObject.SetActive(false);
                    //uIMobileControl.munitionCountText.gameObject.SetActive(false);
                    uIMobileControl.durabilitySlider.fillAmount = 0;
                }

                uIMobileControl.durabilityGameobject.SetActive(false);
            }
        }
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        magazineItem = new Item(PremiumItemManager.singleton.Instantresurrect);
        AssignMagazineItem();
        player.playerWeight = player.GetComponent<PlayerWeight>();
        player.playerWeight.max = player.playerWeight.defaultCarryWeight;
        player.playerEquipment = this;
        float max = player.playerWeight.defaultCarryWeight;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].amount > 0)
            {
                max += ((EquipmentItem)slots[i].item.data).maxWeight.Get(slots[i].item.bagLevel);
            }
        }
        player.playerWeight.max = max;
    }

    void OnEquipmentChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        // update the equipment
        RefreshLocation(index);
        if(!player.playerArmor)
        {
            player.GetComponent<PlayerArmor>().Assign();
            player.playerArmor.GetCurrentArmor();
            player.playerArmor.GetMaxArmor();
        }
        else
        {
            player.playerArmor.GetCurrentArmor();
            player.playerArmor.GetMaxArmor();
        }
    }

    public void RefreshLocation(int index)
    {
        ItemSlot slot = slots[index];
        EquipmentInfo info = slotInfo[index];

        // valid cateogry and valid location? otherwise don't bother
        if (info.requiredCategory != "" && info.location != null)
            info.location.spritesToAnimate = slot.amount > 0 ? ((EquipmentItem)slot.item.data).sprites : null;

        if (index == 0)
        {
            player.GetComponent<PlayerUpgrade>().Assign();
            SetInitialPositionOfWeapon();
            player.playerUpgrade.SetSkin(slots[0]);


            if (slot.amount > 0)
            {
                player.animator.runtimeAnimatorController = ((EquipmentItem)slot.item.data).animatorToSet;
                UIMobileControl.singleton.enableAttackButtonObject.gameObject.SetActive(true);
            }
            else
            {
                player.animator.runtimeAnimatorController = AnimatorManager.singleton.noWeaponRuntimeController;
                UIMobileControl.singleton.enableAttackButtonObject.gameObject.SetActive(false);
            }
            ChangeWeaponLayer();
        }
        if (index == 6)
        {
            player.GetComponent<PlayerTorch>().Assign();
            player.playerTorch.CheckTorch();
        }
    }

    public void SetInitialPositionOfWeapon()
    {
        if (player.state == "IDLE")
        {
            player.GetComponent<PlayerWeaponIK>().Assign();
            if (player.playerWeaponIK && player.playerEquipment.weaponIK)
            {
                for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
                {
                    if (player.playerWeaponIK.weaponsHolder[i].parent.name.Replace("(Clone)", "") == player.playerEquipment.weaponIK.weaponObject.name)
                    {
                        player.playerWeaponIK.weaponsHolder[i].parent.transform.localPosition = player.playerWeaponIK.weaponsHolder[i].weaponHolder.idle.pos;
                        Utilities.ApplyEulerRotation(player.playerWeaponIK.weaponsHolder[i].parent.transform, player.playerWeaponIK.weaponsHolder[i].weaponHolder.idle.rot);
                    }
                }
            }
        }
    }

    // swap inventory & equipment slots to equip/unequip. used in multiple places
    [Server]
    public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (inventory.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // item slot has to be empty (unequip) or equipabable
            ItemSlot slot = inventory.slots[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem itemData &&
                itemData.CanEquip(player, inventoryIndex, equipmentIndex))
            {
                // swap them
                if (equipmentIndex == 7 && player.playerEquipment.slots[equipmentIndex].amount > 0)
                    if (!CanUnequip(equipmentIndex))
                        return;

                if (player.inventory.slots[inventoryIndex].amount > 0 && player.inventory.slots[inventoryIndex].item.data is EquipmentItem && !(player.inventory.slots[inventoryIndex].item.data is WeaponItem))
                {
                    if (((EquipmentItem)player.inventory.slots[inventoryIndex].item.data).sex != -1 && ((EquipmentItem)player.inventory.slots[inventoryIndex].item.data).sex != player.playerCharacterCreation.sex) return;
                }

                ItemSlot temp = slots[equipmentIndex];

                if (temp.amount > 0)
                {
                    player.GetComponent<PlayerCharacterCreation>().Assign();
                    if (((EquipmentItem)temp.item.data).indexHat > -1)
                    {
                        player.playerCharacterCreation.hats = -1;
                    }
                    if (((EquipmentItem)temp.item.data).indexAccessory > -1)
                    {
                        player.playerCharacterCreation.accessory = -1;
                    }
                    if (((EquipmentItem)temp.item.data).indexShirt > -1)
                    {
                        player.playerCharacterCreation.upper = -1;
                    }
                    if (((EquipmentItem)temp.item.data).indexPants > -1)
                    {
                        player.playerCharacterCreation.down = -1;
                    }
                    if (((EquipmentItem)temp.item.data).indexShoes > -1)
                    {
                        player.playerCharacterCreation.shoes = -1;
                    }
                    if (((EquipmentItem)temp.item.data).indexBag > -1)
                    {
                        player.playerCharacterCreation.bag = -1;
                        player.inventory.bagsize = 0;
                    }
                }

                slots[equipmentIndex] = slot;

                if (equipmentIndex == 0)
                {
                    player.GetComponent<PlayerWeapon>().Assign();
                    player.playerWeapon.shooted = 0;
                    player.playerWeapon.chargedMunition = 0;
                }

                inventory.slots[inventoryIndex] = temp;

                if (slots[equipmentIndex].amount > 0)
                {
                    player.GetComponent<PlayerCharacterCreation>().Assign();
                    if (((EquipmentItem)slots[equipmentIndex].item.data).indexHat > -1)
                    {
                        player.playerCharacterCreation.hats = ((EquipmentItem)slots[equipmentIndex].item.data).indexHat;
                    }
                    if (((EquipmentItem)slots[equipmentIndex].item.data).indexAccessory > -1)
                    {
                        player.playerCharacterCreation.accessory = ((EquipmentItem)slots[equipmentIndex].item.data).indexAccessory;
                    }
                    if (((EquipmentItem)slots[equipmentIndex].item.data).indexShirt > -1)
                    {
                        player.playerCharacterCreation.upper = ((EquipmentItem)slots[equipmentIndex].item.data).indexShirt;
                        if (((EquipmentItem)slots[equipmentIndex].item.data).excludePants)
                        {
                            player.playerCharacterCreation.down = -1;
                        }
                    }
                    if (((EquipmentItem)slots[equipmentIndex].item.data).indexPants > -1)
                    {
                        player.playerCharacterCreation.down = ((EquipmentItem)slots[equipmentIndex].item.data).indexPants;
                    }
                    if (((EquipmentItem)slots[equipmentIndex].item.data).indexShoes > -1)
                    {
                        player.playerCharacterCreation.shoes = ((EquipmentItem)slots[equipmentIndex].item.data).indexShoes;
                    }
                    if (((EquipmentItem)slots[equipmentIndex].item.data).indexBag > -1)
                    {
                        player.playerCharacterCreation.bag = ((EquipmentItem)slots[equipmentIndex].item.data).indexBag;
                        player.inventory.bagsize = ((EquipmentItem)slots[equipmentIndex].item.data).additionalSlot.Get(slots[equipmentIndex].item.bagLevel);
                    }

                    if (equipmentIndex == 7)
                    {
                        int result = (player.inventory.defaultSize + player.inventory.abilitySize + player.inventory.bagsize) - (player.inventory.slots.Count);

                        if (result > 0)
                        {
                            for (int i = 0; i < result; i++)
                            {
                                player.inventory.slots.Add(new ItemSlot());
                            }
                        }
                        else
                        {
                            for (int i = Mathf.Abs(result); i > 0; i--)
                            {
                                FirstFree();
                            }
                        }
                    }
                }
                else
                {
                    if (equipmentIndex == 7)
                    {
                        int result = (player.inventory.defaultSize + player.inventory.abilitySize + player.inventory.bagsize) - (player.inventory.slots.Count);
                        for (int i = Mathf.Abs(result); i > 0; i--)
                        {
                            FirstFree();
                        }
                    }
                }
            }
            player.inventory.size = player.inventory.defaultSize + player.inventory.abilitySize + player.inventory.bagsize;
        }
    }

    public bool CanUnequip(int index)
    {
        if (player.playerEquipment.slots[index].amount > 0 && player.playerEquipment.slots[index].item.data.additionalSlot.baseValue > 0)
        {
            return player.inventory.SlotsFree() >= player.playerEquipment.slots[index].item.data.additionalSlot.Get(player.playerEquipment.slots[index].item.bagLevel);
        }
        return false;
    }

    public void FirstFree()
    {
        for (int e = player.inventory.slots.Count - 1; e > 0; e--)
        {
            int index = e;
            if (player.inventory.slots[index].amount == 0)
            {
                player.inventory.slots.RemoveAt(index);
                return;
            }
        }
    }

    public void ChangeWeaponLayer()
    {
        player.GetComponent<PlayerWeaponIK>().Assign();
        if (player.playerWeaponIK)
        {
            for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
            {
                int index_i = i;
                if (player.playerWeaponIK.weaponsHolder[index_i].parent)
                {
                    WeaponObject weaponObject = player.playerWeaponIK.weaponsHolder[index_i].parent.GetComponent<WeaponObject>();

                    player.playerWeaponIK.weaponsHolder[index_i].parent.gameObject.layer = player.isLocalPlayer ? LayerMask.NameToLayer("PersonalPlayer") : LayerMask.NameToLayer("NotPersonalPlayer");
                    if (player.playerWeaponIK.weaponsHolder[index_i].parent.GetComponent<WeaponObjectRenderer>())
                        player.playerWeaponIK.weaponsHolder[index_i].parent.GetComponent<WeaponObjectRenderer>().SetLayer(player.isLocalPlayer ? "PersonalPlayer" : "NotPersonalPlayer");

                    player.playerWeaponIK.weaponsHolder[index_i].parent.SetActive(false);

                    player.playerWeaponIK.weaponsHolder[index_i].parent.gameObject.SetActive(player.playerEquipment.slots[0].amount > 0 &&
                                                                                                player.playerEquipment.slots[0].item.data.weaponToSpawn &&
                                                                                                player.playerWeaponIK.weaponsHolder[index_i].parent.name.Replace("(Clone)", "") == player.playerEquipment.slots[0].item.data.weaponToSpawn.name);

                    if (weaponObject)
                    {
                        for (int e = 0; e < weaponObject.weaponPieces.Count; e++)
                        {
                            int index_e = e;
                            if (weaponObject.weaponPieces[index_e].renderer != null)
                            {
                                weaponObject.weaponPieces[index_e].renderer.gameObject.layer = player.isLocalPlayer ? LayerMask.NameToLayer("PersonalPlayer") : LayerMask.NameToLayer("NotPersonalPlayer");
                            }
                        }
                    }
                }
            }
            player.ManageState("", "");
        }
    }

    public void Reassign()
    {
        ItemSlot equip;

        equip = player.equipment.slots[0];
        player.equipment.slots[0] = new ItemSlot();
        player.equipment.slots[0] = equip;
    }

    [Command]
    public void CmdSwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        SwapInventoryEquip(inventoryIndex, equipmentIndex);
    }

    [Server]
    public void MergeInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        if (inventory.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // both items have to be valid
            // note: no 'is EquipmentItem' check needed because we already
            //       checked when equipping 'slotTo'.
            ItemSlot slotFrom = inventory.slots[inventoryIndex];
            ItemSlot slotTo = slots[equipmentIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    inventory.slots[inventoryIndex] = slotFrom;
                    slots[equipmentIndex] = slotTo;
                }
            }
        }
    }

    [Command]
    public void CmdMergeInventoryEquip(int equipmentIndex, int inventoryIndex)
    {
        MergeInventoryEquip(equipmentIndex, inventoryIndex);
    }

    [Command]
    public void CmdMergeEquipInventory(int equipmentIndex, int inventoryIndex)
    {
        if (inventory.InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // both items have to be valid
            ItemSlot slotFrom = slots[equipmentIndex];
            ItemSlot slotTo = inventory.slots[inventoryIndex];
            if (slotFrom.amount > 0 && slotTo.amount > 0)
            {
                // make sure that items are the same type
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slotFrom.item.Equals(slotTo.item))
                {
                    // merge from -> to
                    // put as many as possible into 'To' slot
                    int put = slotTo.IncreaseAmount(slotFrom.amount);
                    slotFrom.DecreaseAmount(put);

                    // put back into the list
                    slots[equipmentIndex] = slotFrom;
                    inventory.slots[inventoryIndex] = slotTo;
                }
            }
        }
    }

    // durability //////////////////////////////////////////////////////////////
    /*
    public void OnDamageDealtTo(Entity victim)
    {
        // reduce weapon durability by one each time we attacked someone
        int weaponIndex = GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            ItemSlot slot = slots[weaponIndex];
            slot.item.durability = Mathf.Clamp(slot.item.durability - 1, 0, slot.item.maxDurability);
            slots[weaponIndex] = slot;
        }
    }

    public void OnReceivedDamage(Entity attacker, int damage)
    {
        // reduce durability by one in each equipped item
        for (int i = 0; i < slots.Count; ++i)
        {
            if (slots[i].amount > 0)
            {
                ItemSlot slot = slots[i];
                slot.item.durability = Mathf.Clamp(slot.item.durability - 1, 0, slot.item.maxDurability);
                slots[i] = slot;
            }
        }
    }
    */

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            CmdMergeInventoryEquip(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? check Equals because name AND dynamic variables matter (petLevel etc.)
        // => merge is important when dragging more arrows into an arrow slot!
        if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
        {
            CmdMergeEquipInventory(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]); // reversed
        }
    }

    // validation
    void OnValidate()
    {
        // it's easy to set a default item and forget to set amount from 0 to 1
        // -> let's do this automatically.
        for (int i = 0; i < slotInfo.Length; ++i)
            if (slotInfo[i].defaultItem.item != null && slotInfo[i].defaultItem.amount == 0)
                slotInfo[i].defaultItem.amount = 1;
    }
}
