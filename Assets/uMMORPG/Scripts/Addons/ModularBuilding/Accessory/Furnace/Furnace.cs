using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [Command]
    public void CmdAddWood(int inventoryIndex, NetworkIdentity identity)
    {
        Furnace furnace = identity.gameObject.GetComponent<Furnace>();
        int add = 0;
        if (furnace.CanAddWood(inventory.slots[inventoryIndex].item, inventory.slots[inventoryIndex].amount))
        {
            add = furnace.AddWood(inventory.slots[inventoryIndex].item, inventory.slots[inventoryIndex].amount);
        }
        if (add == -10000) inventory.slots[inventoryIndex] = new ItemSlot();
    }

    [Command]
    public void CmdAddElements(int inventoryIndex, NetworkIdentity identity)
    {
        Furnace furnace = identity.gameObject.GetComponent<Furnace>();
        int add = 0;
        if (furnace.CanAddElements(inventory.slots[inventoryIndex].item, inventory.slots[inventoryIndex].amount))
        {
            add = furnace.AddElements(inventory.slots[inventoryIndex].item, inventory.slots[inventoryIndex].amount);
        }
        if (add == -10000) inventory.slots[inventoryIndex] = new ItemSlot();
    }

    [Command]
    public void CmdAddWoodToInventory(NetworkIdentity identity)
    {
        Furnace furnace = identity.gameObject.GetComponent<Furnace>();
        if (furnace.wood[0].amount == 0) return;
        if (inventory.CanAddItem(furnace.wood[0].item, furnace.wood[0].amount))
        {
            inventory.AddItem(furnace.wood[0].item, furnace.wood[0].amount);
            furnace.wood[0] = new ItemSlot();
        }
    }
    [Command]
    public void CmdAddElementsToInventory(int elementIndex, NetworkIdentity identity)
    {
        Furnace furnace = identity.gameObject.GetComponent<Furnace>();
        if (furnace.elements[elementIndex].amount == 0) return;
        if (inventory.CanAddItem(furnace.elements[elementIndex].item, furnace.elements[elementIndex].amount))
        {
            inventory.AddItem(furnace.elements[elementIndex].item, furnace.elements[elementIndex].amount);
            furnace.elements[elementIndex] = new ItemSlot();
        }
    }

    [Command]
    public void CmdAddResultsToInventory(int resultsIndex, NetworkIdentity identity)
    {
        Furnace furnace = identity.gameObject.GetComponent<Furnace>();
        if (furnace.results[resultsIndex].amount == 0) return;
        if (inventory.CanAddItem(furnace.results[resultsIndex].item, furnace.results[resultsIndex].amount))
        {
            inventory.AddItem(furnace.results[resultsIndex].item, furnace.results[resultsIndex].amount);
            furnace.results[resultsIndex] = new ItemSlot();
        }
    }

    [Command]
    public void CmdManageFurnace(NetworkIdentity identity)
    {
        Furnace furnace = identity.gameObject.GetComponent<Furnace>();
        if (furnace.on == false && furnace.wood[0].amount > 0)
            furnace.on = !furnace.on;
        else if (furnace.on == true)
            furnace.on = !furnace.on;

        if (furnace.on)
        {
            furnace.LaunchJob();
        }
        else
        {
            furnace.StopJob();
        }
    }
}

public partial class Database
{
    class furnace_status
    {
        public int buildingIndex { get; set; }
        public int status { get; set; }
    }
    class furnace_elements
    {
        public int buildingIndex { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
    }

    class furnace_results
    {
        public int buildingIndex { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
    }

    class furnace_wood
    {
        public int buildingIndex { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
    }

    public void SaveFurnace(int index)
    {
        SaveFurnaceStatus(index);
        SaveElements(index);
    }

    public void SaveFurnaceStatus(int index)
    {
        connection.InsertOrReplace(new furnace_status
        {
            buildingIndex = index,
            status = Convert.ToInt32(((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).on)
        });
    }

    public void SaveElements(int index)
    {
        for (int i = 0; i < ((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).elements.Count; i++)
        {
            ItemSlot slot = ((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).elements[i];
            if (slot.amount > 0)
            {
                connection.InsertOrReplace(new furnace_elements
                {
                    buildingIndex = index,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount
                });
            }
        }

        for (int i = 0; i < ((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).results.Count; i++)
        {
            ItemSlot slot = ((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).results[i];
            if (slot.amount > 0)
            {
                connection.InsertOrReplace(new furnace_results
                {
                    buildingIndex = index,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount
                });
            }
        }

        for (int i = 0; i < ((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).wood.Count; i++)
        {
            ItemSlot slot = ((Furnace)ModularBuildingManager.singleton.buildingAccessories[index]).wood[i];
            if (slot.amount > 0)
            {
                connection.InsertOrReplace(new furnace_wood
                {
                    buildingIndex = index,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount
                });
            }
        }
    }

    public void LoadFurnace(int index, Furnace furnace)
    {
        for (int i = 0; i < 7; i++)
        {
            furnace.elements.Add(new ItemSlot());
        }

        for (int i = 0; i < 7; i++)
        {
            furnace.results.Add(new ItemSlot());
        }

        furnace.wood.Add(new ItemSlot());

        foreach (furnace_status row in connection.Query<furnace_status>("SELECT * FROM furnace_status WHERE buildingIndex=?", index))
        {
            furnace.on = Convert.ToBoolean(row.status);
        }

        foreach (furnace_elements row in connection.Query<furnace_elements>("SELECT * FROM furnace_elements WHERE buildingIndex=?", index))
        {
            if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                furnace.elements[row.slot] = new ItemSlot(item, row.amount);
            }
        }

        foreach (furnace_results row in connection.Query<furnace_results>("SELECT * FROM furnace_results WHERE buildingIndex=?", index))
        {
            if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                furnace.results[row.slot] = new ItemSlot(item, row.amount);
            }
        }

        foreach (furnace_wood row in connection.Query<furnace_wood>("SELECT * FROM furnace_wood WHERE buildingIndex=?", index))
        {
            if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                furnace.results[row.slot] = new ItemSlot(item, row.amount);
            }
        }
    }
}

public class Furnace : BuildingAccessory
{
    public readonly SyncList<ItemSlot> elements = new SyncList<ItemSlot>();
    public readonly SyncList<ItemSlot> results = new SyncList<ItemSlot>();
    public readonly SyncList<ItemSlot> wood = new SyncList<ItemSlot>();

    [SyncVar(hook = (nameof(ManageActivation)))] public bool on;

    public new void Start()
    {
        base.Start();
        if (isServer)
        {
            if (elements.Count == 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    elements.Add(new ItemSlot());
                }
            }

            if (results.Count == 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    results.Add(new ItemSlot());
                }
            }

            if (wood.Count == 0)
            {
                wood.Add(new ItemSlot());
            }

            LaunchJob();
        }
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.furnaces.Contains(this)) ModularBuildingManager.singleton.furnaces.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.furnaces.Contains(this)) ModularBuildingManager.singleton.furnaces.Remove(this);
        }
        CancelInvoke();
    }

    public void LaunchJob()
    {
        InvokeRepeating(nameof(Working), FurnaceManager.singleton.furnaceCook, FurnaceManager.singleton.furnaceCook);
    }

    public void StopJob()
    {
        CancelInvoke(nameof(Working));
    }

    public void Working()
    {
        if (wood[0].amount == 0)
        {
            on = false;
            CancelInvoke(nameof(Working));
        }
        else
        {
            if (elements.Count > 0)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    int index = i;

                    if (elements[index].amount == 0) continue;
                    for (int e = 0; e < elements[index].item.data.itemtoCraft.Count; e++)
                    {
                        int index_e = e;
                        int itmAmount = (elements[index].item.data.itemtoCraft[index_e].itemAndAmount.item.name == "Nitrate Potassium" || elements[index].item.data.itemtoCraft[index_e].itemAndAmount.item.name == "Sulfur") ? 0 : 1;
                        int random = UnityEngine.Random.Range(itmAmount, elements[index].item.data.itemtoCraft[index_e].itemAndAmount.amount + 1);
                        if (CanAddResults(new Item(elements[index].item.data.itemtoCraft[index_e].itemAndAmount.item), random))
                        {
                            AddResults(new Item(elements[index].item.data.itemtoCraft[index_e].itemAndAmount.item), random);
                            ItemSlot woodSlot = wood[0];
                            woodSlot.amount--;
                            wood[0] = woodSlot;

                            if (elements[index].amount - 1 <= 0)
                            {
                                elements[index] = new ItemSlot();
                            }
                            else
                            {
                                ItemSlot slot = elements[index];
                                slot.DecreaseAmount(1);
                                elements[index] = slot;
                            }

                            if (wood[0].amount <= 0)
                            {
                                on = false;
                                CancelInvoke(nameof(Working));
                            }
                        }
                    }
                    int rand = UnityEngine.Random.Range(1, 3);
                    if (CanAddResults(new Item(FurnaceManager.singleton.coal), rand))
                    {
                        AddResults(new Item(FurnaceManager.singleton.coal), rand);
                    }
                }
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        elements.Callback += OnElementsChanged;
        results.Callback += OnResultsChanged;
        wood.Callback += OnWoodChanged;
    }

    public void ManageActivation(bool oldBool, bool newBool)
    {
        if (UIFurnace.singleton && UIFurnace.singleton.panel.activeInHierarchy)
        {
            if (ModularBuildingManager.singleton.buildingAccessory && UIFurnace.singleton.panel.activeInHierarchy)
            {
                UIFurnace.singleton.Open(UIFurnace.singleton.furnace);
            }
        }
    }

    public void OnResultsChanged(SyncList<ItemSlot>.Operation op, int itemIndex, ItemSlot oldItem, ItemSlot newItem)
    {
        if (!(op == SyncList<ItemSlot>.Operation.OP_ADD))
        {
            if (UIFurnace.singleton && UIFurnace.singleton.panel.activeInHierarchy)
            {
                if (ModularBuildingManager.singleton.buildingAccessory && UIFurnace.singleton.panel.activeInHierarchy)
                {
                    UIFurnace.singleton.Open(UIFurnace.singleton.furnace);
                }
            }
        }
    }

    public void OnElementsChanged(SyncList<ItemSlot>.Operation op, int itemIndex, ItemSlot oldItem, ItemSlot newItem)
    {
        if (!(op == SyncList<ItemSlot>.Operation.OP_ADD))
        {
            if (UIFurnace.singleton && UIFurnace.singleton.panel.activeInHierarchy)
            {
                if (ModularBuildingManager.singleton.buildingAccessory && UIFurnace.singleton.panel.activeInHierarchy)
                {
                    UIFurnace.singleton.Open(UIFurnace.singleton.furnace);
                }
            }
        }
    }

    public void OnWoodChanged(SyncList<ItemSlot>.Operation op, int itemIndex, ItemSlot oldItem, ItemSlot newItem)
    {
        if (!(op == SyncList<ItemSlot>.Operation.OP_ADD))
        {
            if (UIFurnace.singleton && UIFurnace.singleton.panel.activeInHierarchy)
            {
                if (ModularBuildingManager.singleton.buildingAccessory && UIFurnace.singleton.panel.activeInHierarchy)
                {
                    UIFurnace.singleton.Open(UIFurnace.singleton.furnace);
                }
            }
        }
    }


    public bool CanAddWood(Item item, int amount)
    {
        for (int i = 0; i < wood.Count; ++i)
        {
            if (wood[i].amount == 0)
                amount -= item.maxStack;
            else if (wood[i].item.name == item.name)
                amount -= (wood[i].item.maxStack - wood[i].amount);

            if (amount <= 0) return true;
        }

        return false;
    }

    public int AddWood(Item item, int amount)
    {
        for (int i = 0; i < wood.Count; ++i)
        {
            if (wood[i].amount > 0 && wood[i].item.name == item.name)
            {
                ItemSlot temp = wood[i];
                amount -= temp.IncreaseAmount(amount);
                wood[i] = temp;
            }

            if (amount <= 0) return -10000;
        }

        for (int i = 0; i < wood.Count; ++i)
        {
            if (wood[i].amount == 0)
            {
                int add = Mathf.Min(amount, item.maxStack);
                wood[i] = new ItemSlot(item, add);
                amount -= add;
            }

            if (amount <= 0) return -10000;
        }
        if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        return amount;

    }

    public bool CanAddElements(Item item, int amount)
    {
        for (int i = 0; i < elements.Count; ++i)
        {
            if (elements[i].amount == 0)
                amount -= item.maxStack;
            else if (elements[i].item.name == item.name)
                amount -= (elements[i].item.maxStack - elements[i].amount);

            if (amount <= 0) return true;
        }

        return false;
    }

    public int AddElements(Item item, int amount)
    {
        for (int i = 0; i < elements.Count; ++i)
        {
            if (elements[i].amount > 0 && elements[i].item.name == item.name)
            {
                ItemSlot temp = elements[i];
                amount -= temp.IncreaseAmount(amount);
                elements[i] = temp;
            }

            if (amount <= 0) return -10000;
        }

        for (int i = 0; i < elements.Count; ++i)
        {
            if (elements[i].amount == 0)
            {
                int add = Mathf.Min(amount, item.maxStack);
                elements[i] = new ItemSlot(item, add);
                amount -= add;
            }

            if (amount <= 0) return -10000;
        }
        if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        return amount;

    }

    public bool CanAddResults(Item item, int amount)
    {
        for (int i = 0; i < results.Count; ++i)
        {
            if (results[i].amount == 0)
                amount -= item.maxStack;
            else if (results[i].item.name == item.name)
                amount -= (results[i].item.maxStack - results[i].amount);

            if (amount <= 0) return true;
        }

        return false;
    }

    public int AddResults(Item item, int amount)
    {
        for (int i = 0; i < results.Count; ++i)
        {
            if (results[i].amount > 0 && results[i].item.name == item.name)
            {
                ItemSlot temp = results[i];
                amount -= temp.IncreaseAmount(amount);
                results[i] = temp;
            }

            if (amount <= 0) return -10000;
        }

        for (int i = 0; i < results.Count; ++i)
        {
            if (results[i].amount == 0)
            {
                int add = Mathf.Min(amount, item.maxStack);
                results[i] = new ItemSlot(item, add);
                amount -= add;
            }

            if (amount <= 0) return -10000;
        }
        if (amount != 0) Debug.LogError("inventory add failed: " + item.name + " " + amount);
        return amount;

    }

}
