using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerAdditionalState playerAdditionalState;
}

public class PlayerAdditionalState : NetworkBehaviour
{
    private Player player;
    public Animator animator;
    private RuntimeAnimatorController previousAnimatorController;
    private GameObject previousWeapon;
    private GameObject book;
    private GameObject dumbbell1;
    private GameObject dumbbell2;
    private ScriptableAbility ability;
    private float amountToAdd;

    [SyncVar (hook = nameof(ManageadditionState))] public string additionalState;
    [SyncVar] public string bookTitle = string.Empty;

    void Awake()
    {
        Assign();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerAdditionalState = this;
        previousAnimatorController = animator.runtimeAnimatorController;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ManageadditionState(additionalState, additionalState);
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
                bookTitle = player.inventory.slots[index].item.data.name;
                book.Use(player, index);
            }
        }
        else
        {
            if (player.playerBelt.InventoryOperationsAllowed() &&
                0 <= index && index < player.playerBelt.belt.Count && player.playerBelt.belt[index].amount > 0 &&
                player.playerBelt.belt[index].item.data is ScriptableBook book)
            {
                bookTitle = player.playerBelt.belt[index].item.data.name;
                book.Use(player, index);
            }
        }
    }

    [Command]
    public void CmdUseDumbbell(Player player,NetworkIdentity dumbbellIdentity)
    {
        ScriptableDumbbell dumbbell = ((ScriptableDumbbell)dumbbellIdentity.gameObject.GetComponent<BuildingAccessory>().craftingAccessoryItem);
        SetState("EXERCISE", true, dumbbell.amountToIncreaseAbility, dumbbell.timerIncreasePerPointAbility, dumbbell.abilityType);
    }

    [Command]
    public void CmdSetAnimation(string animationName,string abilityName)
    {
        SetState(animationName, true, 0.1f, 30, AbilityManager.singleton.FindAbility(abilityName));
    }

    public void SetState(string state,bool condition, float amount, float timer, ScriptableAbility scriptableAbility)
    {
        additionalState = ((condition && state == "READING") || 
                           (condition && state == "EXERCISE") ||
                           (condition && state == "ABS") ||
                           (condition && state == "JUMPINGJACK") ||
                           (condition && state == "SLEEP") ||
                           (condition && state == "PUSHUPS")) ? state : "";
        
        CancelInvoke(nameof(IncreaseAbility));
        ability = scriptableAbility;
        amountToAdd = amount;
            
        if(condition)
        {
            InvokeRepeating(nameof(IncreaseAbility), timer, timer);
        }
    }

    public void ManageadditionState(string oldValue, string newValue)
    {
        if (!player.playerWeaponIK) player.GetComponent<PlayerWeaponIK>().Assign();
        player.playerWeaponIK.Spawn();
        if (!player.playerEquipment) player.GetComponent<PlayerEquipment>().Assign();

        if (oldValue != "READING" && 
           oldValue != "EXERCISE" &&
           oldValue != "ABS" &&
           oldValue != "JUMPINGJACK" &&
           oldValue != "PUSHUPS" && 
           oldValue != "SLEEP")
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

        if(newValue != string.Empty)
        {
            for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
            {
                int index_i = i;
                if (player.playerWeaponIK.weaponsHolder[index_i].parent.activeInHierarchy)
                {
                    previousWeapon = player.playerWeaponIK.weaponsHolder[index_i].parent;
                    player.playerWeaponIK.weaponsHolder[index_i].parent.SetActive(false);
                }
            }

            if (newValue == "READING")
            {
                for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
                {
                    int index_i = i;
                    if (player.playerWeaponIK.weaponsHolder[index_i].parent.name.Contains("Book"))
                    {
                        book = player.playerWeaponIK.weaponsHolder[index_i].parent;
                        book.SetActive(true);
                    }
                }
                animator.runtimeAnimatorController = BookManager.singleton.animator;
            }

            if (newValue == "EXERCISE")
            {
                for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
                {
                    int index_i = i;
                    if (player.playerWeaponIK.weaponsHolder[index_i].parent.name.Contains("Dumb"))
                    {
                        if (dumbbell1 == null)
                        {
                            dumbbell1 = player.playerWeaponIK.weaponsHolder[index_i].parent;
                            dumbbell1.SetActive(true);
                        }
                        else
                        {
                            dumbbell2 = player.playerWeaponIK.weaponsHolder[index_i].parent;
                            dumbbell2.SetActive(true);
                        }                                
                    }
                }
                animator.runtimeAnimatorController = ExerciseManager.singleton.dumbbellAnimator;
            }

            if(newValue == "PUSHUPS") animator.runtimeAnimatorController = ExerciseManager.singleton.pushUpAnimator;
            if(newValue == "JUMPINGJACK") animator.runtimeAnimatorController = ExerciseManager.singleton.jumpinJackAnimator;
            if(newValue == "ABS") animator.runtimeAnimatorController = ExerciseManager.singleton.sitUpAnimator;
            if(newValue == "SLEEP") animator.runtimeAnimatorController = AnimatorManager.singleton.sleepRuntimeController;



            for (int i = 0; i < player.playerWeaponIK.weaponsHolder.Count; i++)
            {
                if (player.playerWeaponIK.weaponsHolder[i].parent.name.Replace("(Clone)", "").Contains("Dumbbell"))
                {
                    player.playerWeaponIK.weaponsHolder[i].parent.transform.localPosition = player.playerWeaponIK.weaponsHolder[i].weaponHolder.idle.pos;
                    Utilities.ApplyEulerRotation(player.playerWeaponIK.weaponsHolder[i].parent.transform, player.playerWeaponIK.weaponsHolder[i].weaponHolder.idle.rot);
                }
            }

        }
        else
        {

            if (dumbbell1) dumbbell1.SetActive(false);
            if (dumbbell2) dumbbell2.SetActive(false);

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

        if(newValue == "READING")
        {
            if(UIBookPanel.singleton.title == string.Empty || (UIBookPanel.singleton.title != string.Empty && UIBookPanel.singleton.title != bookTitle))
                UIBookPanel.singleton.Monitoring(bookTitle);
        }
        else
        {
            UIBookPanel.singleton.ClosePanel();
        }
    }

    public void IncreaseAbility()
    {
        if (ability)
        {
            Ability ab = player.playerAbility.networkAbilities[AbilityManager.singleton.FindNetworkAbility(ability.name, player.name)];
            ab.level += amountToAdd;
            player.playerAbility.networkAbilities[AbilityManager.singleton.FindNetworkAbility(ability.name, player.name)] = ab;
            if(additionalState == "READING") player.playerNotification.TargetSpawnBookNotification(bookTitle, "Ability " + ab.name + " level increased of " + amountToAdd);
            else if(additionalState == "EXERCISE") player.playerNotification.TargetSpawnDumbbellNotification("Dumbbell", "Ability " + ab.name + " level increased of " + amountToAdd);
            else player.playerNotification.TargetSpawnNotificationFullBodyExercise("Ability " + ab.name + " level increased of " + amountToAdd);
        }
    }

}
