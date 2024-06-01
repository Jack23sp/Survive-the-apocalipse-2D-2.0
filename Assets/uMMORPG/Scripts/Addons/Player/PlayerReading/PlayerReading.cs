using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerReading playerReading;
}

public class PlayerReading : NetworkBehaviour
{
    private Player player;
    public Animator animator;
    private RuntimeAnimatorController previousAnimatorController;
    private GameObject previousWeapon;
    private GameObject book;
    private ScriptableAbility ability;
    private float amountToAdd;

    [SyncVar (hook = nameof(ManageReadState))] public string additionalState;
    public string bookTitle = string.Empty;

    void Awake()
    {
        Assign();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerReading = this;
        previousAnimatorController = animator.runtimeAnimatorController;
    }

    [Command]
    public void CmdUseBook(Player player,int index,bool isInventory)
    {
        if (isInventory)
        {
            if (player.inventory.InventoryOperationsAllowed() &&
                0 <= index && index < player.inventory.slots.Count && player.inventory.slots[index].amount > 0 &&
                player.inventory.slots[index].item.data is ScriptableBook book)
            {
                book.Use(player, index);
                bookTitle = player.inventory.slots[index].item.data.name;
            }
        }
        else
        {
            if (player.playerBelt.InventoryOperationsAllowed() &&
                0 <= index && index < player.playerBelt.belt.Count && player.playerBelt.belt[index].amount > 0 &&
                player.playerBelt.belt[index].item.data is ScriptableBook book)
            {
                book.Use(player, index);
                bookTitle = player.playerBelt.belt[index].item.data.name;
            }
        }
    }

    public void SetState(bool condition, float amount, float timer, ScriptableAbility scriptableAbility)
    {
        additionalState = condition ? "READING" : "";
        
        CancelInvoke(nameof(IncreaseAbility));
        ability = scriptableAbility;
        amountToAdd = amount;
            
        if(condition)
        {
            InvokeRepeating(nameof(IncreaseAbility), timer, timer);
        }
    }

    public void ManageReadState(string oldValue, string newValue)
    {
        if(oldValue != "READING")
        {
            previousAnimatorController = animator.runtimeAnimatorController;
        }
        else
        {
            animator.runtimeAnimatorController = previousAnimatorController;

            if (previousWeapon)
            {
                previousWeapon.SetActive(true);
                if (book)
                    book.SetActive(false);
            }
        }

        if(newValue == "READING")
        {

            for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
            {
                int index_i = i;
                if (player.playerWeaponIK.weaponsHolder[index_i].parent.activeInHierarchy)
                {
                    previousWeapon = player.playerWeaponIK.weaponsHolder[index_i].parent;
                    player.playerWeaponIK.weaponsHolder[index_i].parent.SetActive(false);
                }
                else if (player.playerWeaponIK.weaponsHolder[index_i].parent.name.Contains("Book"))
                {
                    book = player.playerWeaponIK.weaponsHolder[index_i].parent;
                    book.SetActive(true);
                }
            }
            animator.runtimeAnimatorController = BookManager.singleton.animator;
        }
        else
        {
            if(book)
            {
                book.SetActive(false);
            }
            else
            {
                for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
                {
                    int index_i = i;
                    if (player.playerWeaponIK.weaponsHolder[index_i].parent.name.Contains("Book"))
                    {
                        book = player.playerWeaponIK.weaponsHolder[index_i].parent;
                        book.SetActive(false);
                    }
                }
            }
        }
    }

    public void IncreaseAbility()
    {
        Ability ab = player.playerAbility.networkAbilities[AbilityManager.singleton.FindNetworkAbility(ability.name, player.name)];
        ab.level += amountToAdd;
        player.playerAbility.networkAbilities[AbilityManager.singleton.FindNetworkAbility(ability.name, player.name)] = ab;
        player.playerNotification.TargetSpawnBookNotification(bookTitle,"Ability " + ab.name + " level increased of " + amountToAdd);
    }

}
