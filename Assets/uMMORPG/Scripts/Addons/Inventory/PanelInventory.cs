using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public partial class ItemContainer
{
    public int CountItem(Item item)
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int amount = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.data.name == item.name)
                amount += slot.amount;
        return amount;
    }

    public bool RemoveItem(Item item, int amount)
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            if (slot.amount > 0 && slot.item.data.name == item.name)
            {
                // take as many as possible
                amount -= slot.DecreaseAmount(amount);
                slots[i] = slot;

                // are we done?
                if (amount == 0) return true;
            }
        }

        // if we got here, then we didn't remove enough items
        return false;
    }

    public bool CanAddItem(Item item, int amount)
    {
        // go through each slot
        for (int i = 0; i < slots.Count; ++i)
        {
            // empty? then subtract maxstack
            if (slots[i].amount == 0)
                amount -= item.maxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (slots[i].item.data.name == item.name)
                amount -= (slots[i].item.maxStack - slots[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }

    public bool AddItem(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (CanAddItem(item, amount))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < slots.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slots[i].amount > 0 && slots[i].item.data.name == item.name)
                {
                    ItemSlot temp = slots[i];
                    amount -= temp.IncreaseAmount(amount);
                    slots[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }

            // add to empty slots (if any)
            for (int i = 0; i < slots.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (slots[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    slots[i] = new ItemSlot(item, add);
                    amount -= add;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return true;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return false;
    }

    public int AddItemResInt(Item item, int amount)
    {
        if (CanAddItem(item, amount))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < slots.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (slots[i].amount > 0 && slots[i].item.name == item.name)
                {
                    ItemSlot temp = slots[i];
                    amount -= temp.IncreaseAmount(amount);
                    slots[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return amount;
            }

            // add to empty slots (if any)
            for (int i = 0; i < slots.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (slots[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    slots[i] = new ItemSlot(item, add);
                    amount -= add;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return amount;
            }
            // we should have been able to add all of them
            if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        }
        return amount;
    }

}

public partial class Inventory
{
    [Command]
    public void CmdDeleteInventoryItem(int index)
    {
        ItemSlot inventorySlot = slots[index];
        inventorySlot = new ItemSlot();
        slots[index] = inventorySlot;
    }
}

public class PanelInventory : MonoBehaviour
{
    public static PanelInventory singleton;

    private Player player;
    public UIInventorySlot slotPrefab;
    public Transform content;
    public UISelectedItem selectedItem;

    void OnEnable()
    {
        if (!singleton) singleton = this;
        player = Player.localPlayer;
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        if (!player) return;

        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.slots.Count, content);

        // refresh all items
        for (int i = 0; i < player.inventory.slots.Count; ++i)
        {
            UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop index
            ItemSlot itemSlot = player.inventory.slots[i];

            if (itemSlot.amount > 0)
            {
                // refresh valid item
                int icopy = i; // needed for lambdas, otherwise i is Count
                slot.button.interactable = true;
                slot.button.onClick.SetListener(() =>
                {
                    UISelectedItem.singleton.ItemSlot = itemSlot;
                    UISelectedItem.singleton.use = true;
                    UISelectedItem.singleton.delete = true;
                    UISelectedItem.singleton.CallInvokeToCheck();
                });
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = false;
                slot.dragAndDropable.dragable = true;

                // use durability colors?
                /*if (itemSlot.item.maxDurability > 0)
                {
                    if (itemSlot.item.durability == 0)
                        slot.image.color = brokenDurabilityColor;
                    else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                        slot.image.color = lowDurabilityColor;
                    else
                        slot.image.color = Color.white;
                }
                else*/
                slot.image.color = Color.white; // reset for no-durability items
                slot.image.sprite = itemSlot.item.data.canAddSkin && itemSlot.item.data.skinImages.Count > 0 ? itemSlot.item.data.skinImages[itemSlot.item.skin] : itemSlot.item.data.image;
                slot.image.preserveAspect = true;

                // cooldown if usable item
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                // refresh invalid item
                slot.button.interactable = false;
                slot.button.onClick.RemoveAllListeners();
                slot.tooltip.enabled = false;
                slot.dragAndDropable.dragable = false;
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }

    }
}
