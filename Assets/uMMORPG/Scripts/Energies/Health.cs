using UnityEngine;
using Mirror;
using ScriptBoy.Fly2D;

public partial class Database
{
    class cleaning
    {
        public string characterName { get; set; }
        public int cleaningAmount { get; set; }
    }

    public void SavePlayerCleaning(Player player)
    {
        connection.Execute("DELETE FROM cleaning WHERE characterName=?", player.name);

        connection.InsertOrReplace(new cleaning
        {
            characterName = player.name,
            cleaningAmount = player.health.cleaningState
            
        });
    }

    public void LoadPlayerCleaning(Player player)
    {
        foreach (cleaning row in connection.Query<cleaning>("SELECT * FROM cleaning WHERE characterName=?", player.name))
        {
            player.health.cleaningState = row.cleaningAmount;
        }
    }

}

// inventory, attributes etc. can influence max health
public interface IHealthBonus
{
    int GetHealthBonus(int baseHealth);
    int GetHealthRecoveryBonus();
}

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Health : Energy
{
    public Level level;

    public LinearInt baseHealth = new LinearInt{baseValue=100};
    public int baseRecoveryRate = 1;
    [SyncVar(hook =nameof(ManageFlyAmount))]
    public int cleaningState = 0;
    public F2DFlyZone flyzone;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InvokeDirtyPlayer();
    }

    public void InvokeStartClean()
    {
        Invoke(nameof(CleanPlayer), 0.3f);
    }

    public void InvokeDirtyPlayer()
    {
        Invoke(nameof(DirtyPlayer), 20.0f);
    }

    public void ManageFlyAmount(int oldValue, int newValue)
    {
        if(newValue >= 70)
        {
            flyzone.m_FlyCount = (newValue - 70); 
        }
    }

    public void CleanPlayer()
    {
        cleaningState--;
        if (cleaningState < 0) cleaningState = 0;
        else
        {
            InvokeStartClean();
        }       
    }

    public void DirtyPlayer()
    {
        if (TemperatureManager.singleton.isRainy)
        {
            cleaningState -= 5;
            if (cleaningState < 0) cleaningState = 0;
        }
        else
        {
            cleaningState++;
            if (cleaningState > 100) cleaningState = 100;
            else
            {
                InvokeDirtyPlayer();
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Player.localPlayer.playerAccessoryInteraction.RemoveInteraction();
    }

    [Command]
    public void CmdCallHealthReachZero()
    {
        Player.localPlayer.playerAccessoryInteraction.RemoveInteraction();
        Player.localPlayer.playerAdditionalState.SetState("", false, 0.1f, 30, null);
    }

    public void InvokeStopClean()
    {
        CancelInvoke(nameof(CleanPlayer));
    }

    // cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IHealthBonus[] _bonusComponents;
    IHealthBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<IHealthBonus>());

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            int baseThisLevel = baseHealth.Get(level.current);
            foreach (IHealthBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetHealthBonus(baseThisLevel);
            return baseThisLevel + bonus;
        }
    }

    public override int recoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (IHealthBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetHealthRecoveryBonus();
            return baseRecoveryRate + bonus;
        }
    }
}