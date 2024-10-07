using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Player
{
    [HideInInspector] public PlayerAccessoryInteraction playerAccessoryInteraction;
}

public class PlayerAccessoryInteraction : NetworkBehaviour
{
    private Player player;
    [HideInInspector] public PlayerGrassDetector grassDetector;
    [SyncVar(hook = (nameof(ChangeSorthByDepth)))]
    public NetworkIdentity whereActionIsGoing;
    private float previousSorthByDepth = 0.0f;
    [SyncVar(hook = (nameof(ChangePositionOfSprite)))]
    public Vector3 movePosition;
    [HideInInspector] public Vector3 originalCanvasPosition;


    public void Start()
    {
        Assign();
    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerAccessoryInteraction = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        previousSorthByDepth = player.transform.GetChild(0).gameObject.GetComponentInChildren<SortByDepth>().precision;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        grassDetector = player.GetComponentInChildren<PlayerGrassDetector>();
        Invoke(nameof(CheckNearInteractableObject), 0.3f);
    }

    public void ChangeSorthByDepth(NetworkIdentity oldIdentity, NetworkIdentity newIdentity)
    {
        if (newIdentity)
        {
            BuildingAccessory acc = newIdentity.gameObject.GetComponent<BuildingAccessory>();
            for (int i = 0; i < acc.actionPlayerSlot.Count; i++)
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
                        count++;
                    }
                }
                if (count == 0)
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

    public void ChangePositionOfSprite(Vector3 oldValue, Vector3 newValue)
    {
        if (newValue == Vector3.zero)
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
                                player.playerMove.CmdSyncRotation(grassDetector.interactableActions[index].direction, false);
                                player.playerAdditionalState.CmdSetAnimation(grassDetector.interactableActions[index].animationType.ToString().ToUpper(), "");
                            }
                            else if (grassDetector.interactableActions[index].animationType == AnimationType.MoveTo)
                            {
                                player.playerMove.CmdSyncRotation(grassDetector.interactableActions[index].direction, false);
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


}
