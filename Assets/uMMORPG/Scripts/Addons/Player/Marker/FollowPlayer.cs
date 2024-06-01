using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class Player
{
    [HideInInspector] public FollowPlayer followPlayer;
}

public class FollowPlayer : MonoBehaviour
{
    public Player player;
    public GameObject marker;
    public SpriteRenderer markerRenderer;

    void Start()
    {
        player.followPlayer = this;
        if (player.isClient)
        {
            //marker.transform.parent = null;
            marker.transform.position = player.transform.position;
            markerRenderer.color = Color.blue;
            Invoke(nameof(Check), 1.0f);
        }
    }

    public void Check()
    {
        if (Player.localPlayer && Player.localPlayer.name == player.name)
        {
            marker.SetActive(true);
            markerRenderer.color = Color.white;
        }
        else 
        {
            markerRenderer.color = Color.white;
            if (player.party.InParty() && 
                Player.localPlayer.party.InParty() && 
                player.party.party.partyId == Player.localPlayer.party.party.partyId)
            {
                marker.SetActive(true);
                markerRenderer.color = Color.yellow;
                return;
            }
            if (player.guild.InGuild() && 
                Player.localPlayer.guild.InGuild() && 
                player.guild.guild.name == Player.localPlayer.guild.guild.name)
            {
                marker.SetActive(true);
                markerRenderer.color = Color.green;
                return;
            }
            if (Player.localPlayer.guild.InGuild() && 
                player.guild.InGuild() &&  
                player.playerAlliance.guildAlly.Contains(Player.localPlayer.guild.guild.name))
            {
                marker.SetActive(true);
                markerRenderer.color = Color.cyan;
                return;
            }

            marker.SetActive(false);
        }
    }

    //void Update()
    //{
    //    marker.transform.position = player.transform.position;
    //}
}
