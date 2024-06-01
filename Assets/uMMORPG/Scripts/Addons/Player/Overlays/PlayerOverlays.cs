using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerOverlays playerOverlays;


}

public class PlayerOverlays : NetworkBehaviour
{
    private Player player;
    private PlayerParty party;
    private PlayerGuild guild;
    private PlayerAlliance alliance;
    public TextMesh nameOverlay;
    public TextMesh guildOverlay;
    public TextMesh otherOverlay;

    void Awake()
    {
        player = GetComponent<Player>();
        player.playerOverlays = this;
        nameOverlay.gameObject.SetActive(true);
        guildOverlay.gameObject.SetActive(true);
        otherOverlay.gameObject.SetActive(true);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        nameOverlay.text = player.name;
    }
}
