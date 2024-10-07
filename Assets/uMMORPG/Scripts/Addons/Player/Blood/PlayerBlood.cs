using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public partial class Player
{
    [HideInInspector] public PlayerBlood playerBlood;
}

public partial struct Item
{
    public int currentBlood;
}
public partial class Database
{
    class blood
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int currentBlood { get; set; }
    }

    public void Connect_Blood()
    {
        connection.CreateTable<blood>();
    }

    public void SaveBlood(Player player)
    {
        PlayerBlood blood = player.GetComponent<PlayerBlood>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM blood WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new blood
        {
            characterName = player.name,
            currentBlood = blood.current
        });
    }
    public void LoadBlood(Player player)
    {
        PlayerBlood bloods = player.GetComponent<PlayerBlood>();
        blood row = connection.FindWithQuery<blood>("SELECT * FROM blood WHERE characterName=?", player.name);

        if (row != null)
        {
            bloods.current = row.currentBlood;
        }
    }

}

public class PlayerBlood : NetworkBehaviour
{
    private Player player;
    [SyncVar(hook = (nameof(CheckSelectedItem)))]
    public int current;
    public int max;
    public RawImage playerImage;
    public Color bloodSkinColor;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        CheckSelectedItem(current, current);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        InvokeRepeating(nameof(DecreaseBlood), 25.0f, 25.0f);
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerBlood = this;
        player.GetComponent<PlayerAccessoryInteraction>().originalCanvasPosition = playerImage.transform.parent.localPosition;
    }

    public void Spawn(float heigh)
    {
        ResourceManager.singleton.SpawnDropBlood(Random.Range(1,10), transform.GetChild(0), heigh);
    }

    public void DecreaseBlood()
    {
        if (current > 0)
        {
            if (TemperatureManager.singleton.isRainy)
            {
                current -= 2;
                if (current < 0) current = 0;
            }
            else
            {
                current--;
            }
        }
    }

    public void CheckSelectedItem (int oldValue, int newValue)
    {
        if(UISelectedItem.singleton && UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
        {
            UISelectedItem.singleton.Setup(UISelectedItem.singleton.ItemSlot, UISelectedItem.singleton.delete, UISelectedItem.singleton.use, UISelectedItem.singleton.equip);
        }
        if(current > 0)
        {
            playerImage.color = bloodSkinColor;
        }
        else
        {
            playerImage.color = Color.white;
        }
    }

    [Command]
    public void CmdAddBlood(bool inventory, int index, int amount)
    {
        ItemSlot slot = new ItemSlot();
        slot = inventory ? player.inventory.slots[index] : player.playerBelt.belt[index];
        if( slot.amount > 0 && slot.item.data is FoodItem)
        {
            if (slot.item.currentBlood < amount) return;
            else
            {
                slot.item.currentBlood -= amount;
                player.playerBlood.current += amount;
                if (inventory)
                {
                    player.inventory.slots[index] = slot;
                }
                else
                {
                    player.playerBelt.belt[index] = slot;
                }
            }
        }
    }
}
