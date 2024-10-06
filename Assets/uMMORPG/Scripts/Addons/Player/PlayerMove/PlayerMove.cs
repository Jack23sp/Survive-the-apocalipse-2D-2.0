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
    [SyncVar(hook = nameof(ManageUITiredness))] public int tired = 200;
    public int tiredLimitForAim = 30;
    public int maxTiredness = 100;
    public readonly SyncList<string> states = new SyncList<string>();
    public GameObject playerObject;
    [SyncVar (hook = (nameof(ChangeAttackMode)))]
    public bool canAttack;
    [SyncVar] public long position;
    public GameObject torch;

    private PlayerSkills playerSkills;
    [HideInInspector] public PlayerGrassDetector grassDetector;
    [SyncVar(hook = (nameof(ChangeSorthByDepth)))]
    public NetworkIdentity whereActionIsGoing;
    private float previousSorthByDepth = 0.0f;
    [SyncVar(hook = (nameof(ChangePositionOfSprite)))]
    public Vector3 movePosition;
    [HideInInspector] public Vector3 originalCanvasPosition;

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
        InvokeRepeating(nameof(ManageTiredness), 7.0f,7.0f);
        InvokeRepeating(nameof(BurnFat), 5.0f,5.0f);
    }

    public void ManageTiredness()
    {
        if(tired > 0 && player.playerAdditionalState.additionalState != "SLEEP") tired--;
        
        if (tired == 0 && player.playerAdditionalState.additionalState != "SLEEP")
        {
            player.playerAdditionalState.additionalState = "SLEEP";
            return;
        }

        if (player.playerAdditionalState.additionalState == "SLEEP")
        {
            tired++;
        }

    }

    public void ChangeSorthByDepth (NetworkIdentity oldIdentity, NetworkIdentity newIdentity)
    {
        if(newIdentity)
        {
            BuildingAccessory acc = newIdentity.gameObject.GetComponent<BuildingAccessory>();
            for(int i = 0; i < acc.actionPlayerSlot.Count; i++)
            {
                if (acc.actionPlayerSlot[i].player && acc.actionPlayerSlot[i].player == player.netIdentity)
                {
                    InteractableAction interactable = acc.actionObjects[acc.actionPlayerSlot[i].slot].GetComponent<InteractableAction>();
                    if (!interactable) continue;
                    if (interactable.overrideSortByDepth != 0.0f)
                    {
                        player.transform.GetChild(0).gameObject.GetComponentInChildren<SortByDepth>().precision = interactable.overrideSortByDepth;
                    }
                    if (interactable.spriteToManage)
                        interactable.spriteToManage.enabled = true;
                }
            }
        }
        else
        {
            player.transform.GetChild(0).gameObject.GetComponentInChildren<SortByDepth>().precision = previousSorthByDepth;
            if (oldIdentity)
            {
                BuildingAccessory acc = oldIdentity.gameObject.GetComponent<BuildingAccessory>();
                int count = 0;
                for (int i = 0; i < acc.actionPlayerSlot.Count; i++)
                {
                    if (acc.actionPlayerSlot[i].player)
                    {
                        count ++;
                    }
                }
                if(count == 0)
                {
                    for (int i = 0; i < acc.actionObjects.Count; i++)
                    {
                        if (acc.actionObjects[i].gameObject.GetComponent<InteractableAction>().spriteToManage)
                            acc.actionObjects[i].gameObject.GetComponent<InteractableAction>().spriteToManage.enabled = false;
                    }
                }
            }
        }
    }

    public void ManageUITiredness(int oldValue, int newValue)
    {
        UIPlayerInformation.singleton.Tired();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (UIMobileControl.singleton && AttackManager.singleton)
        {
            UIMobileControl.singleton.enableAttackButton.image.sprite = player.equipment.slots[0].amount > 0 ? player.equipment.slots[0].item.data.image : null;
        }

        ManageUITiredness(tired, tired);
        grassDetector = player.GetComponentInChildren<PlayerGrassDetector>();
        Invoke(nameof(CheckNearInteractableObject), 0.3f);
    }

    public void ChangePositionOfSprite(Vector3 oldValue, Vector3 newValue)
    {
        if(newValue == Vector3.zero)
        {
            player.playerBlood.playerImage.transform.parent.localPosition = originalCanvasPosition;
        }
        else
        {
            player.playerBlood.playerImage.transform.parent.position = newValue;
        }
    }

    public new void CancelInvoke()
    {
        CancelInvoke(nameof(CheckNearInteractableObject));
    }

    public void InitializeInvoke()
    {
        Invoke(nameof(CheckNearInteractableObject), 0.3f);
    }

    public void CheckNearInteractableObject()
    {
        try
        {
            if (whereActionIsGoing)
            {
                UIInteractionPanel.singleton.actionButton.gameObject.SetActive(false);
                Invoke(nameof(CheckNearInteractableObject), 0.3f);
                return;
            }

            UIInteractionPanel.singleton.actionButton.gameObject.SetActive(false);

            SortInteractableActionsByDistance();
            for (int i = 0; i < grassDetector.interactableActions.Count; i++)
            {
                int index = i;
                if (grassDetector.interactableActions[index] &&
                    grassDetector.interactableActions[index].mainAccessory.gameObject.activeInHierarchy &&
                    grassDetector.interactableActions[index].mainAccessory.actionPlayerSlot[grassDetector.interactableActions[index].index].player == null)
                {
                    if (Vector3.Distance(grassDetector.interactableActions[index].transform.position, player.transform.position) < 0.3f)
                    {

                        UIInteractionPanel.singleton.actionButton.gameObject.SetActive(true);
                        UIInteractionPanel.singleton.actionButton.onClick.RemoveAllListeners();
                        UIInteractionPanel.singleton.actionButton.onClick.AddListener(() =>
                        {
                            CmdSetInteractionPlayer(grassDetector.interactableActions[index].mainAccessory.netIdentity, grassDetector.interactableActions[index].index);
                            if (grassDetector.interactableActions[index].animationType == AnimationType.Sit)
                            {
                                CmdSyncRotation(grassDetector.interactableActions[index].direction, false);
                                player.playerAdditionalState.CmdSetAnimation(grassDetector.interactableActions[index].animationType.ToString().ToUpper(), "");
                            }
                            else if (grassDetector.interactableActions[index].animationType == AnimationType.MoveTo)
                            {
                                CmdSyncRotation(grassDetector.interactableActions[index].direction, false);
                                player.playerAdditionalState.CmdSetAnimation(grassDetector.interactableActions[index].additionalActionToDo[0].ToString().ToUpper(), "");
                                CmdSetMovePosition(grassDetector.interactableActions[index].moveToTransform.position);
                            }
                            UIInteractionPanel.singleton.actionButton.gameObject.SetActive(false);
                        });
                        Invoke(nameof(CheckNearInteractableObject), 0.3f);
                        return;
                    }
                }
            }
        }
        catch
        {
            CancelInvoke();
            InitializeInvoke();
        }
        finally
        {
            CancelInvoke();
            InitializeInvoke();

        }
    }

    void SortInteractableActionsByDistance()
    {
        grassDetector.interactableActions.Sort((a, b) =>
        {
            float distanceASqr = (player.transform.position - a.transform.position).sqrMagnitude;
            float distanceBSqr = (player.transform.position - b.transform.position).sqrMagnitude;

            return distanceASqr.CompareTo(distanceBSqr);
        });
    }

    [Command]
    public void CmdSetInteractionPlayer(NetworkIdentity identity, int index)
    {
        BuildingAccessory acc = identity.gameObject.GetComponent<BuildingAccessory>();
        if (!acc) return;
        ActionPlayerSlot plSlot = acc.actionPlayerSlot[index];
        plSlot.player = player.netIdentity;
        acc.actionPlayerSlot[index] = plSlot;
        whereActionIsGoing = acc.netIdentity;
    }

    [Command]
    public void CmdSetMovePosition(Vector3 newPosition)
    {
        movePosition = newPosition;
    }


    [Command]
    public void CmdRemoveInteraction()
    {
        RemoveInteraction();
    }

    public void RemoveInteraction()
    {
        movePosition = Vector3.zero;

        if (whereActionIsGoing)
        {
            BuildingAccessory acc = whereActionIsGoing.gameObject.GetComponent<BuildingAccessory>();
            if (!acc) return;
            for (int i = 0; i < acc.actionPlayerSlot.Count; i++)
            {
                if (acc.actionPlayerSlot[i].player != null && acc.actionPlayerSlot[i].player.netId == player.netIdentity.netId)
                {
                    ActionPlayerSlot plSlot = acc.actionPlayerSlot[i];
                    plSlot.player = null;
                    acc.actionPlayerSlot[i] = plSlot;
                    whereActionIsGoing = null;
                }
            }
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
        previousSorthByDepth = player.transform.GetChild(0).gameObject.GetComponentInChildren<SortByDepth>().precision;
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

    public void BurnFat()
    {
        if (player.state == "MOVING")
        {
            switch (states.Contains("RUN"))
            {
                case true:
                    player.playerCharacterCreation.fat -= 0.025f;
                    break;

                case false:
                    player.playerCharacterCreation.fat -= 0.01f;
                    break;
            }
        }
    }

    [Command]
    public void CmdSetRun()
    {
        if (player.playerWeight.current <= player.playerWeight.max)
        {
            if (!states.Contains("RUN"))
            {
                states.Add("RUN");
            }
            else
            {
                states.Remove("RUN");
            }
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
           player.playerAdditionalState.SetState("",false, 0, 0, null);
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
