using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Reflection;
using SQLite;

public partial class Player
{
    [HideInInspector] public PlayerBelt playerBelt;
}

public partial class EquipmentItem
{
    public bool excludePants;
    public int indexHat = -1;
    public int indexAccessory = -1;
    public int indexShirt = -1;
    public int indexPants = -1;
    public int indexShoes = -1;
    public int indexBag = -1;
    public int sex = -1;
}

public partial class Database
{
    class character_belt_weapon_accessories
    {
        public int myIndex { get; set; }
        public string character { get; set; }
        public string accessoryName { get; set; }
        public int bulletsRemaining { get; set; }
        public int skin { get; set; }
    }

    class character_belt
    {
        public int myIndex { get; set; }
        public string character { get; set; }
        public int slot { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public float summonedHealth { get; set; }
        public int summonedLevel { get; set; }
        public long summonedExperience { get; set; } // TODO does long work?
        public int currentArmor { get; set; }
        public int currentUnsanity { get; set; }
        public int radioCurrentBattery { get; set; }
        public int torchCurrentBattery { get; set; }
        public int currentDurability { get; set; }
        public int skin { get; set; }
        public int accuracyLevel { get; set; }
        public int durabilityLevel { get; set; }
        public int armorLevel { get; set; }
        public int bagLevel { get; set; }
        public int batteryLevel { get; set; }
        public int currentBlood { get; set; }
        public int gasContainer { get; set; }
        public int honeyContainer { get; set; }
        public int waterContainer { get; set; }
        public float weight { get; set; }
        public float wet { get; set; }
        public int bulletsRemaining { get; set; }
    }

    public void Connect_Belt()
    {
        connection.CreateTable<character_belt>();
    }

    public void SaveBelt(Player player)
    {
        if (!player.playerBelt) return;
        connection.Execute("DELETE FROM character_belt WHERE character=?", player.name);
        connection.Execute("DELETE FROM character_belt_weapon_accessories WHERE character=?", player.name);
        for (int i = 0; i < player.playerBelt.belt.Count; i++)
        {
            ItemSlot slot = player.playerBelt.belt[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_belt
                {
                    myIndex = i,
                    character = player.name,
                    slot = i,
                    name = slot.item.name,
                    amount = slot.amount,
                    summonedHealth = slot.item.summonedHealth,
                    summonedLevel = slot.item.summonedLevel,
                    summonedExperience = slot.item.summonedExperience,
                    currentArmor = slot.item.currentArmor,
                    currentUnsanity = slot.item.currentUnsanity,
                    radioCurrentBattery = slot.item.radioCurrentBattery,
                    torchCurrentBattery = slot.item.torchCurrentBattery,
                    currentDurability = slot.item.currentDurability,
                    skin = slot.item.skin,
                    accuracyLevel = slot.item.accuracyLevel,
                    durabilityLevel = slot.item.durabilityLevel,
                    armorLevel = slot.item.armorLevel,
                    bagLevel = slot.item.bagLevel,
                    batteryLevel = slot.item.batteryLevel,
                    currentBlood = slot.item.currentBlood,
                    gasContainer = slot.item.gasContainer,
                    honeyContainer = slot.item.honeyContainer,
                    waterContainer = slot.item.waterContainer,
                    weight = slot.item.weight,
                    bulletsRemaining = slot.item.bulletsRemaining,
                    wet = slot.item.wet
                });

                if (slot.item.data is WeaponItem && slot.item.accessories.Length > 0)
                {
                    for (int e = 0; e < slot.item.accessories.Length; e++)
                    {
                        connection.InsertOrReplace(new character_belt_weapon_accessories
                        {
                            myIndex = i,
                            character = player.name,
                            accessoryName = slot.item.accessories[e].name,
                            bulletsRemaining = slot.item.accessories[e].bulletsRemaining,
                            skin = slot.item.skin
                        });
                    }
                }
            }
        }

    }

    public void LoadBelt(Player player)
    {
        for (int i = 0; i < 4; ++i)
            player.GetComponent<PlayerBelt>().belt.Add(new ItemSlot());

        foreach (character_belt row in connection.Query<character_belt>("SELECT * FROM character_belt WHERE character=?", player.name))
        {
            if (ScriptableItem.All.TryGetValue(row.name.GetStableHashCode(), out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                item.summonedHealth = row.summonedHealth;
                item.summonedLevel = row.summonedLevel;
                item.summonedExperience = row.summonedExperience;
                item.currentArmor = row.currentArmor;
                item.currentUnsanity = row.currentUnsanity;
                item.radioCurrentBattery = row.radioCurrentBattery;
                item.torchCurrentBattery = row.torchCurrentBattery;
                item.currentDurability = row.currentDurability;
                item.skin = row.skin;
                item.accuracyLevel = row.accuracyLevel;
                item.durabilityLevel = row.durabilityLevel;
                item.armorLevel = row.armorLevel;
                item.bagLevel = row.bagLevel;
                item.batteryLevel = row.batteryLevel;
                item.currentBlood = row.currentBlood;
                item.gasContainer = row.gasContainer;
                item.honeyContainer = row.honeyContainer;
                item.waterContainer = row.waterContainer;
                item.weight = row.weight;
                item.wet = row.wet;

                int am = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM character_belt_weapon_accessories WHERE character=? AND myIndex=?", player.name, row.slot);
                item.accessories = new Item[am];
                int count = 0;
                foreach (character_belt_weapon_accessories row2 in connection.Query<character_belt_weapon_accessories>("SELECT * FROM character_belt_weapon_accessories WHERE character=? AND myIndex=?", player.name, row.slot))
                {
                    if (ScriptableItem.All.TryGetValue(row2.accessoryName.GetStableHashCode(), out ScriptableItem accessory))
                    {
                        Item item2 = new Item(accessory);
                        item2.bulletsRemaining = row2.bulletsRemaining;
                        item.accessories[count] = item2;
                        count++;
                    }
                }

                player.GetComponent<PlayerBelt>().belt[row.slot] = new ItemSlot(item, row.amount);
            }
            else Debug.LogWarning("LoadInventory: skipped item " + row.name + " for " + player.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }

    }
}

public partial class UsableItem
{
    public virtual bool CanUseBelt(Player player, int inventoryIndex)
    {
        // check level etc. and make sure that cooldown buff elapsed (if any)
        return player.level.current >= minLevel;
    }
    public virtual void UseBelt(Player player, int inventoryIndex)
    {
        // start cooldown (if any)
        // -> no need to set sync dict dirty if we have no cooldown
        if (cooldown > 0)
            player.SetItemCooldown(cooldownCategory, cooldown);
    }

}

public partial class EquipmentItem
{
    public bool CanEquipBelt(Player player, int inventoryIndex, int equipmentIndex)
    {
        string requiredCategory = ((PlayerEquipment)player.equipment).slotInfo[equipmentIndex].requiredCategory;
        return base.CanUseBelt(player, inventoryIndex) &&
               requiredCategory != "" &&
               category.StartsWith(requiredCategory);
    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        // find a slot that accepts this category, then equip it
        int slot = FindEquipableSlotFor(player, inventoryIndex);
        if (slot != -1)
        {
            // reuse Player.SwapInventoryEquip function for validation etc.
            ((PlayerBelt)player.playerBelt).SwapBeltEquip(inventoryIndex, slot);
        }
    }
}

public partial class PotionItem
{
    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        // increase health/mana/etc.
        player.health.current += usageHealth;
        player.mana.current += usageMana;
        player.experience.current += usageExperience;
        if (player.petControl.activePet != null) player.petControl.activePet.health.current += usagePetHealth;

        // decrease amount
        ItemSlot slot = player.playerBelt.belt[inventoryIndex];
        slot.DecreaseAmount(1);
        player.playerBelt.belt[inventoryIndex] = slot;
    }

}

public partial class MountItem
{
    // usage
    public override bool CanUseBelt(Player player, int inventoryIndex)
    {
        // summonable checks if we can summon it already,
        // we just need to check if we have no active mount summoned yet
        // OR if this is the active mount, so we unsummon it
        return base.CanUseBelt(player, inventoryIndex) &&
               (player.mountControl.activeMount == null ||
                player.mountControl.activeMount.netIdentity == player.playerBelt.belt[inventoryIndex].item.summoned);
    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        // summon
        if (player.mountControl.activeMount == null)
        {
            // summon at player position
            ItemSlot slot = player.playerBelt.belt[inventoryIndex];
            GameObject go = Instantiate(summonPrefab.gameObject, player.transform.position, player.transform.rotation);
            Mount mount = go.GetComponent<Mount>();
            mount.name = summonPrefab.name; // avoid "(Clone)"
            mount.owner = player;
            mount.health.current = (int)slot.item.summonedHealth;

            // spawn with owner connection so the owner can call [Command]s on it
            NetworkServer.Spawn(go, player.connectionToClient);
            player.mountControl.activeMount = go.GetComponent<Mount>(); // set syncvar to go after spawning

            // set item summoned pet reference so we know it can't be sold etc.
            slot.item.summoned = go.GetComponent<NetworkIdentity>();
            player.playerBelt.belt[inventoryIndex] = slot;
        }
        // unsummon
        else
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(player.mountControl.activeMount.gameObject);
        }
    }
}
public partial class MonsterScrollItem
{
    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        foreach (SpawnInfo spawn in spawns)
        {
            if (spawn.monster != null)
            {
                for (int i = 0; i < spawn.amount; ++i)
                {
                    // summon in random circle position around the player
                    Vector2 circle2D = UnityEngine.Random.insideUnitCircle * spawn.distanceMultiplier;
                    Vector3 position = player.transform.position + new Vector3(circle2D.x, 0, circle2D.y);
                    GameObject go = Instantiate(spawn.monster.gameObject, position, Quaternion.identity);
                    go.name = spawn.monster.name; // avoid "(Clone)"
                    NetworkServer.Spawn(go);
                }
            }
        }

        // decrease amount
        ItemSlot slot = player.playerBelt.belt[inventoryIndex];
        slot.DecreaseAmount(1);
        player.playerBelt.belt[inventoryIndex] = slot;
    }
}

public partial class SummonableItem : UsableItem
{
    // usage
    public override bool CanUseBelt(Player player, int inventoryIndex)
    {
        // summon only if:
        //  summonable not dead (dead summonable item has to be revived first)
        //  not while fighting, trading, stunned, dead, etc
        //  player level at least summonable level to avoid power leveling
        //    with someone else's high level summonable
        //  -> also use riskyActionTime to avoid spamming. we don't want someone
        //     to spawn and destroy a pet 1000x/second
        return base.CanUseBelt(player, inventoryIndex) &&
               (player.state == "IDLE" || player.state == "MOVING") &&
               NetworkTime.time >= player.nextRiskyActionTime &&
               summonPrefab != null &&
               player.playerBelt.belt[inventoryIndex].item.summonedHealth > 0 &&
               player.playerBelt.belt[inventoryIndex].item.summonedLevel <= player.level.current;
    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.UseBelt(player, inventoryIndex);

        // set risky action time (1s should be okay)
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }
}

public partial class PetItem
{
    public override bool CanUseBelt(Player player, int inventoryIndex)
    {
        // summonable checks if we can summon it already,
        // we just need to check if we have no active pet summoned yet
        return base.CanUse(player, inventoryIndex) && player.petControl.activePet == null;
    }

    public override void UseBelt(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // summon right next to the player
        ItemSlot slot = player.playerBelt.belt[inventoryIndex];
        GameObject go = Instantiate(summonPrefab.gameObject, player.petControl.petDestination, Quaternion.identity);
        Pet pet = go.GetComponent<Pet>();
        pet.name = summonPrefab.name; // avoid "(Clone)"
        pet.owner = player;
        pet.level.current = slot.item.summonedLevel;
        pet.experience.current = slot.item.summonedExperience;
        // set health AFTER level, otherwise health is always level 1 max health
        pet.health.current = (int)slot.item.summonedHealth;

        // spawn with owner connection so the owner can call [Command]s on it
        NetworkServer.Spawn(go, player.connectionToClient);
        player.petControl.activePet = go.GetComponent<Pet>(); // set syncvar to go after spawning

        // set item summoned pet reference so we know it can't be sold etc.
        slot.item.summoned = go.GetComponent<NetworkIdentity>();
        player.playerBelt.belt[inventoryIndex] = slot;
    }
}

public class PlayerBelt : NetworkBehaviour
{
    private Player player;
    public SyncList<ItemSlot> belt = new SyncList<ItemSlot>();

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerBelt = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        belt.Callback += OnBeltChanged;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }

    void OnBeltChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if(UIPlayerBelt.singleton)
            UIPlayerBelt.singleton.CheckSkillbar();
    }

    public int FindFirst(string itemName)
    {
        foreach (ItemSlot slot in belt)
            if (slot.amount > 0 && slot.item.name == itemName)
                return belt.IndexOf(slot);

        return -1;
    }

    [Command]
    public void CmdUseBeltItem(int index)
    {
        // validate
        if (InventoryOperationsAllowed() &&
            0 <= index && index < belt.Count && belt[index].amount > 0 &&
            belt[index].item.data is UsableItem usable)
        {
            // use item
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            if (usable.CanUseBelt(player, index))
            {
                Item item = belt[index].item;
                usable.UseBelt(player, index);
                player.inventory.RpcUsedItem(item);
            }
        }
    }

    public int CountItem(Item item)
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int amount = 0;
        foreach (ItemSlot slot in belt)
            if (slot.amount > 0 && slot.item.data.name == item.name)
                amount += slot.amount;
        return amount;
    }


    [Command]
    public void CmdDeleteBeltItem(int index)
    {
        ItemSlot beltSlot = player.playerBelt.belt[index];
        beltSlot = new ItemSlot();
        player.playerBelt.belt[index] = beltSlot;
    }


    public bool InventoryOperationsAllowed()
    {
        return player.state == "IDLE" ||
               player.state == "MOVING" ||
               player.state == "CASTING" ||
               (player.state == "TRADING" && player.trading.state == TradingState.Free);
    }

    [Server]
    public void SwapBeltEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (InventoryOperationsAllowed() &&
            0 <= inventoryIndex && inventoryIndex < belt.Count &&
            0 <= equipmentIndex && equipmentIndex < player.equipment.slots.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = belt[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem itemData &&
                itemData.CanEquip(player, inventoryIndex, equipmentIndex))
            {
                // swap them
                if (equipmentIndex == 7 && player.playerEquipment.slots[equipmentIndex].amount > 0)
                    if (!CanUnequip(equipmentIndex))
                        return;

                var temp = player.equipment.slots[equipmentIndex];
                player.equipment.slots[equipmentIndex] = belt[inventoryIndex];
                belt[inventoryIndex] = temp;

                if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexHat > -1)
                {
                    player.playerCharacterCreation.hats = ((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexHat;
                }
                if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexAccessory > -1)
                {
                    player.playerCharacterCreation.accessory = ((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexAccessory;
                }
                if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexShirt > -1)
                {
                    player.playerCharacterCreation.upper = ((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexShirt;
                    if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).excludePants)
                    {
                        player.playerCharacterCreation.down = -1;
                    }
                }
                if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexPants > -1)
                {
                    player.playerCharacterCreation.down = ((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexPants;
                }
                if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexShoes > -1)
                {
                    player.playerCharacterCreation.shoes = ((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexShoes;
                }
                if (((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexBag > -1)
                {
                    player.playerCharacterCreation.bag = ((EquipmentItem)player.equipment.slots[equipmentIndex].item.data).indexBag;
                }

                if (equipmentIndex == 7)
                {
                    if (player.equipment.slots[equipmentIndex].amount > 0 && (player.equipment.slots[equipmentIndex].item.data.additionalSlot.baseValue > 0))
                    {
                        player.inventory.bagsize = player.equipment.slots[equipmentIndex].item.data.additionalSlot.Get(player.equipment.slots[equipmentIndex].item.bagLevel);
                    }
                    else if (player.equipment.slots[equipmentIndex].amount == 0)
                    {
                        player.inventory.bagsize = 0;

                    }

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
                player.inventory.size = player.inventory.defaultSize + player.inventory.abilitySize + player.inventory.bagsize;
            }
        }
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


    public bool CanUnequip(int index)
    {
        if (player.playerEquipment.slots[index].amount > 0 && player.playerEquipment.slots[index].item.data.additionalSlot.baseValue > 0)
        {
            return player.inventory.SlotsFree() >= player.playerEquipment.slots[index].item.data.additionalSlot.Get(player.playerEquipment.slots[index].item.bagLevel);
        }
        return false;
    }

    public int SlotsFree()
    {
        // count manually. Linq is HEAVY(!) on GC and performance
        int free = 0;
        foreach (ItemSlot slot in player.inventory.slots)
            if (slot.amount == 0)
                ++free;
        return free;
    }


    public bool CanAdd(Item item, int amount)
    {
        // go through each slot
        for (int i = 0; i < belt.Count; ++i)
        {
            // empty? then subtract maxstack
            if (belt[i].amount == 0)
                amount -= item.maxStack;
            // not empty. same type too? then subtract free amount (max-amount)
            // note: .Equals because name AND dynamic variables matter (petLevel etc.)
            else if (belt[i].item.Equals(item))
                amount -= (belt[i].item.maxStack - belt[i].amount);

            // were we able to fit the whole amount already?
            if (amount <= 0) return true;
        }

        // if we got here than amount was never <= 0
        return false;
    }

    // helper function to put 'n' items of a type into the inventory, while
    // trying to put them onto existing item stacks first
    // -> this is better than always adding items to the first free slot
    // -> function will only add them if there is enough space for all of them
    public int Add(Item item, int amount)
    {
        // we only want to add them if there is enough space for all of them, so
        // let's double check
        if (CanAdd(item, amount))
        {
            // add to same item stacks first (if any)
            // (otherwise we add to first empty even if there is an existing
            //  stack afterwards)
            for (int i = 0; i < belt.Count; ++i)
            {
                // not empty and same type? then add free amount (max-amount)
                // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                if (belt[i].amount > 0 && belt[i].item.name == item.name)
                {
                    ItemSlot temp = belt[i];
                    amount -= temp.IncreaseAmount(amount);
                    belt[i] = temp;
                }

                // were we able to fit the whole amount already? then stop loop
                if (amount <= 0) return amount;
            }

            // add to empty slots (if any)
            for (int i = 0; i < belt.Count; ++i)
            {
                // empty? then fill slot with as many as possible
                if (belt[i].amount == 0)
                {
                    int add = Mathf.Min(amount, item.maxStack);
                    belt[i] = new ItemSlot(item, add);
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

    [Command]
    public void CmdSwapInventoryBelt(int inventoryIndex)
    {
        if (CanAdd(player.inventory.slots[inventoryIndex].item, player.inventory.slots[inventoryIndex].amount))
        {
            ItemSlot slot = player.inventory.slots[inventoryIndex];
            slot.amount = Add(player.inventory.slots[inventoryIndex].item, player.inventory.slots[inventoryIndex].amount);
            player.inventory.slots[inventoryIndex] = slot;
            TargetRefreshInventory();
        }
    }

    [TargetRpc]
    public void TargetRefreshInventory()
    {
        InventoryRefresh();
    }

    public void InventoryRefresh()
    {
        if (UIInventoryCustom.singleton)
        {
            UIInventoryCustom.singleton.Open();
        }
    }

    [Command]
    public void CmdSwapBeltInventory(int beltIndex)
    {
        if (player.inventory.CanAddItem(belt[beltIndex].item, belt[beltIndex].amount))
        {
            ItemSlot slot = belt[beltIndex];
            slot.amount = player.inventory.AddItemResInt(belt[beltIndex].item, belt[beltIndex].amount);
            belt[beltIndex] = slot;
            TargetRefreshBelt();
        }
    }

    [TargetRpc]
    public void TargetRefreshBelt()
    {
        if (UIPlayerBelt.singleton)
        {
            UIPlayerBelt.singleton.CheckSkillbar();
        }
        InventoryRefresh();
    }
}
