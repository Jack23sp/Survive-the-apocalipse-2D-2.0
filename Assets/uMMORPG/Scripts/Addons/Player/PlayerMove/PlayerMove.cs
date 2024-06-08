using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using AdvancedPeopleSystem;
using System;

public partial class Player
{
    [HideInInspector] public PlayerMove playerMove;
    public PlayerNavMeshMovement2D playerNavMeshMovement2D;
}

public class PlayerMove : NetworkBehaviour
{
    [HideInInspector] public Player player;
    [SyncVar (hook=(nameof(ChangeRotation)))]public Vector2 tempVector = new Vector2();

    public readonly SyncList<string> states = new SyncList<string>();
    public GameObject playerObject;
    [SyncVar (hook = (nameof(ChangeAttackMode)))]
    public bool canAttack;
    [SyncVar] public long position;
    public GameObject torch;

    private PlayerSkills playerSkills;

    public void Awake()
    {
        playerSkills = GetComponent<PlayerSkills>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        SetPosition();
        StateSpeedManager.singleton.originalSpeed = player.agent2D.speed;
        player.agent2D.SetSpeed(StateSpeedManager.singleton.originalSpeed);
        InvokeRepeating(nameof(CheckRunConsumeStamina), 1.0f,1.0f);
        InvokeRepeating(nameof(CheckBloodDropOnIdle), 30.0f,30.0f);
        InvokeRepeating(nameof(CheckBloodDropOnMovement), 5.0f,5.0f);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (UIMobileControl.singleton && AttackManager.singleton)
        {
            UIMobileControl.singleton.enableAttackButton.image.sprite = player.equipment.slots[0].amount > 0 ? player.equipment.slots[0].item.data.image : null;
        }
    }

    public void ChangeAttackMode(bool oldValue, bool newValue)
    {
        if(UIMobileControl.singleton)
        {
            UIMobileControl.singleton.placeOver.SetActive(!newValue);
        }
    }

    public void Update()
    {
        if(isServer)
        {
            playerSkills._currentSkillDirection = tempVector;
            playerSkills.entity.lookDirection = tempVector;
        }
    }

    public void CheckBloodDropOnIdle()
    {
        DropBlood(0);
    }

    public void CheckBloodDropOnMovement()
    {
        DropBlood(1);
    }

    public void DropBlood(int type)
    {
        switch (type)
        {
            case 0:
                {
                    if (player.state == "IDLE" && player.health.Percent() <= 0.5f)
                        player.playerBlood.Spawn(1.0f);
                }
                break;
            case 1:
                {
                    if (player.state == "MOVING" && player.health.Percent() <= 0.5f)
                        player.playerBlood.Spawn(1.0f);
                }
                break;
        }
    }

    public void CheckRunConsumeStamina()
    {
        if(states.Contains("RUN") && player.agent2D.velocity != Vector2.zero)
        {
            player.mana.current--;
            if(player.mana.current <= 0 ) 
            {
                if (states.Contains("RUN"))
                    states.Remove("RUN");
            }
        }
    }

    public void SetPosition()
    {
        RegistrablePrefabManager.singleton.lastPosition++;
        position = RegistrablePrefabManager.singleton.lastPosition;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        states.Callback += OnstatesChanged;
        ChangeRotation(tempVector, tempVector);
        StateSpeedManager.singleton.originalSpeed = player.agent2D.speed;
        player.agent2D.SetSpeed(StateSpeedManager.singleton.originalSpeed);
    }

    private void OnstatesChanged(SyncList<string>.Operation op, int itemIndex, string oldItem, string newItem)
    {
        player.ManageState(oldItem, newItem);
    }

    [Command]
    public void CmdSetState(string state, string[] removeStates)
    {
        if (!string.IsNullOrEmpty(state) && !states.Contains(state)) states.Add(state);
        for (int i = 0; i < removeStates.Length; i++)
        {
            if (states.Contains(removeStates[i])) states.Remove(removeStates[i]);
        }
    }

    [Command]
    public void CmdSetCanAttack(bool condition)
    {
        canAttack = condition;
    }

    public void ManageSneakAnimation()
    {
        if (player.isLocalPlayer)
        {
            if (UIMobileControl.singleton)
            {
                UIMobileControl.singleton.runButton.image.color = states.Contains("RUN") ? Color.green : Color.white;
                UIMobileControl.singleton.sneakButton.image.color = states.Contains("SNEAK") ? Color.green : Color.white;
            }
        }

        player.animator.SetBool("SNEAK", states.Contains("SNEAK"));
        player.animator.SetBool("RUN", states.Contains("RUN"));
        player.animator.SetBool("AIM", states.Contains("AIM"));
        player.animator.SetBool("SHOOT", states.Contains("SHOOT"));
        player.animator.SetFloat("Multiplier", states.Contains("SNEAK") ? (1.0f - (StateSpeedManager.singleton.sneakSpeedAmount/100) + 0.2f) : states.Contains("RUN") ? (1.0f + (StateSpeedManager.singleton.runSpeedAmount / 100)) : 1.0f);

    }

    public void SetRun()
    {
        CmdSetRun();
    }

    public void SetSneak()
    {
        CmdSetSneak();
    }

    [Command]
    public void CmdSetRun()
    {
        if (player.playerWeight.current <= player.playerWeight.max)
        {
            if (!states.Contains("RUN"))
                states.Add("RUN");
            else states.Remove("RUN");
            if (states.Contains("SNEAK")) states.Remove("SNEAK");
            CheckSpeed();
            RpcCheckSpeed();
        }
        else
        {
            player.playerScreenNotification.TargetRpcMessageInvitation(player.netIdentity, "Check your carrying weight!");
        }
    }

    public void ManageWeight()
    {
        if (states.Contains("RUN"))
            states.Remove("RUN");

    }

    [Command]
    public void CmdSetSneak()
    {
        if(!states.Contains("SNEAK")) states.Add("SNEAK"); else states.Remove("SNEAK");
        if (states.Contains("RUN")) states.Remove("RUN");
        CheckSpeed();
        RpcCheckSpeed();
    }

    [ClientRpc]
    public void RpcCheckSpeed()
    {
        CheckSpeed();
    }

    public void CheckSpeed()
    {
        if(!player) Assign();
        player.GetComponent<PlayerBoost>().Assign();
        float plus = player.playerBoost.FindBoostPercent("Sportive") + AbilityManager.singleton.FindNetworkAbilityLevel("Sportive", player.name);
        if (states.Contains("RUN"))
        {
            player.agent2D.SetSpeed((float)(StateSpeedManager.singleton.originalSpeed) + 
                                          ((StateSpeedManager.singleton.originalSpeed / 100) * 
                                            StateSpeedManager.singleton.runSpeedAmount) + 
                                          ((StateSpeedManager.singleton.originalSpeed / 100) * plus));
        }
        else if (states.Contains("SNEAK"))
        {
            player.agent2D.SetSpeed((float)(StateSpeedManager.singleton.originalSpeed) - 
                                          ((StateSpeedManager.singleton.originalSpeed / 100) * 
                                            StateSpeedManager.singleton.sneakSpeedAmount));
        }
        else if (!states.Contains("RUN") && !states.Contains("SNEAK"))
        {
            player.agent2D.SetSpeed(StateSpeedManager.singleton.originalSpeed + 
                                  ((StateSpeedManager.singleton.originalSpeed/100) * plus));
        }

    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerMove = this;
        if (!player.playerNavMeshMovement2D) player.playerNavMeshMovement2D = GetComponent<PlayerNavMeshMovement2D>();

        player.playerCharacterCreation = player.GetComponent<PlayerCharacterCreation>();
        player.playerCharacterCreation.characterCustomization = playerObject.GetComponent<CharacterCustomization>();
        player.animator = playerObject.GetComponent<Animator>();
    }

    [Command]
    public void CmdSyncRotation(Vector2 rot, bool manageReading)
    {
        tempVector = rot;
        if (manageReading)
        {
            if (player.playerAdditionalState.additionalState == "READING")
            {
                player.playerAdditionalState.SetState("READING",false, 0, 0, null);
            }
            if (player.playerAdditionalState.additionalState == "EXERCISE")
            {
                player.playerAdditionalState.SetState("EXERCISE", false, 0, 0, null);
            }
        }
    }
    
    public void ChangeRotation(Vector2 oldVector2, Vector2 newVector2)
    {
        if (player)
        {
            player.lookDirection = newVector2;
            tempVector = newVector2;
            float heading = Mathf.Atan2(newVector2.x, newVector2.y);
            playerObject.transform.localRotation = Quaternion.Euler(0f, (heading * Mathf.Rad2Deg), 0);
            torch.transform.localRotation = Quaternion.Euler(0f, 0f, -(heading * Mathf.Rad2Deg));
        }
    }

}
