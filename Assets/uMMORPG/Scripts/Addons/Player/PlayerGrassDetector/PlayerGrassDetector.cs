using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrassDetector : MonoBehaviour
{
    public Player player;
    public Collider2D thisCollider;
    public List<Player> nearPlayer = new List<Player>();
    private string returnString;
    public BoxCollider2D zoneInside = null;
    public List<InteractableAction> interactableActions = new List<InteractableAction>();

    public void Start()
    {
        if (player.netIdentity.isLocalPlayer || player.GetComponent<SelectableCharacter>())
        {
            player.gameObject.GetComponent<PlayerCharacterCreation>().playerChildObject.gameObject.SetActive(true);
        }
        Invoke(nameof(Caller), 3.0f);
    }

    public void Caller()
    {
        if(player.netIdentity.isLocalPlayer)
        {
            CheckNear();
        }
    }

    public void CheckNear()
    {
        for (int i = 0; i < nearPlayer.Count; i++)
        {
            if (nearPlayer != null)
            {
                nearPlayer[i].playerOverlays.otherOverlay.text = ManageAssociationWithOtherPlayer(player, nearPlayer[i]);
                nearPlayer[i].gameObject.GetComponent<FollowPlayer>().Check();
            }
        }
        Invoke(nameof(CheckNear), 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (player.isServer)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Spawn")
            {
                SpawnManager.singleton.SpawnObject(collision, player.collider);
                zoneInside = ((BoxCollider2D)collision);
            }
        }

        if (LayerMask.LayerToName(collision.gameObject.layer) == "Grass" && player.isLocalPlayer)
        {
            if (!collision.GetComponent<Grass>().isOverlayed)
            {
                collision.GetComponent<Grass>().Manage(true);
            }
        }

        if (player.netIdentity.isLocalPlayer)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && !collision.gameObject.GetComponent<Player>().netIdentity.isLocalPlayer)
            {
                collision.gameObject.GetComponent<PlayerCharacterCreation>().playerChildObject.gameObject.SetActive(true);
                collision.gameObject.transform.GetChild(3).gameObject.SetActive(true);

                if(collision.gameObject.GetComponent<Player>())
                {
                    if(!nearPlayer.Contains(collision.gameObject.GetComponent<Player>()))
                    {
                        nearPlayer.Add(collision.gameObject.GetComponent<Player>());
                        collision.gameObject.GetComponent<PlayerOverlays>().otherOverlay.text = ManageAssociationWithOtherPlayer(player, collision.gameObject.GetComponent<Player>());
                        collision.gameObject.GetComponent<FollowPlayer>().Check();
                    }
                }
            }

            if(collision.CompareTag("InteractSlot"))
            {
                if (!interactableActions.Contains(collision.GetComponent<InteractableAction>()))
                    interactableActions.Add(collision.GetComponent<InteractableAction>());
            }

            if (collision.CompareTag("Basement"))
            {
                ModularBuilding modularBuilding = collision.GetComponent<ModularBuilding>();
                if (modularBuilding)
                {
                    modularBuilding.RefreshWallOptions();
                    for(int i = 0; i < modularBuilding.sortByDepths.Count; i++)
                    {
                        modularBuilding.enabled = true;
                        modularBuilding.sortByDepths[i].SetOrder();
                    }
                }
            }
        }
    }

    public string ManageAssociationWithOtherPlayer(Player myPlayer, Player otherPlayer)
    {
        returnString = string.Empty;
        if(myPlayer.party.InParty() && otherPlayer.party.InParty() && otherPlayer.party.party.partyId == myPlayer.party.party.partyId)
        {
            returnString = "[Party]";
        }
        if(myPlayer.guild.InGuild() && otherPlayer.guild.InGuild() && otherPlayer.playerAlliance.guildAlly.Contains(myPlayer.guild.guild.name))
        {
            returnString += returnString == string.Empty ? "[Ally]" : "/[Ally]";
        }
        return returnString;
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (player.netIdentity.isServer)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Spawn")
            {
                SpawnManager.singleton.DespawnObject(collision, player.collider);
            }
        }

        if (LayerMask.LayerToName(collision.gameObject.layer) == "Grass" && player.isLocalPlayer)
        {
            if (!collision.GetComponent<Grass>().isOverlayed)
            {
                collision.GetComponent<Grass>().Manage(false);
            }
        }

        if (player.netIdentity.isLocalPlayer)
        {
            if (LayerMask.LayerToName(collision.gameObject.layer) == "Player" && !collision.gameObject.GetComponent<Player>().netIdentity.isLocalPlayer)
            {
                collision.gameObject.GetComponent<PlayerCharacterCreation>().playerChildObject.gameObject.SetActive(false);
                collision.gameObject.transform.GetChild(3).gameObject.SetActive(false);

                if (collision.gameObject.GetComponent<Player>())
                {
                    if (nearPlayer.Contains(collision.gameObject.GetComponent<Player>()))
                    {
                        nearPlayer.Remove(collision.gameObject.GetComponent<Player>());
                        collision.gameObject.GetComponent<Player>().playerOverlays.otherOverlay.text = string.Empty;
                    }
                }
            }

            if (collision.CompareTag("InteractSlot"))
            {
                if (interactableActions.Contains(collision.GetComponent<InteractableAction>()))
                    interactableActions.Remove(collision.GetComponent<InteractableAction>());
            }

        }
    }
}
