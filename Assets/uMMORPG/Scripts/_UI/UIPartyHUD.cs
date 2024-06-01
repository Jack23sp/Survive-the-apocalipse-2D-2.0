using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIPartyHUD : MonoBehaviour
{
    public static UIPartyHUD singleton;
    public GameObject panel;
    public UIPartyHUDMemberSlot slotPrefab;
    public Transform memberContent;


    public void Start()
    {
        if(!singleton) singleton = this;
    }

    public void RefreshPartyMember()
    {
        Player player = Player.localPlayer;
        if (player && player.party && player.party.InParty())
        {
            panel.SetActive(true);
            Party party = player.party.party;

            // get party members without self. no need to show self in HUD too.
            List<string> members = player.party.InParty() ? party.members.Where(m => m != player.name).ToList() : new List<string>();

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, members.Count, memberContent);

            // refresh all members
            for (int i = 0; i < members.Count; i++)
            {
                UIPartyHUDMemberSlot slot = memberContent.GetChild(i).GetComponent<UIPartyHUDMemberSlot>();
                string memberName = members[i];
                float visRange = player.VisRange();

                slot.nameText.text = memberName;
                slot.masterIndicatorText.gameObject.SetActive(party.master == memberName);

                // pull health, mana, etc. from observers so that party struct
                // doesn't have to send all that data around. people will only
                // see health of party members that are near them, which is the
                // only time that it's important anyway.
                if (Player.onlinePlayers.ContainsKey(memberName))
                {
                    Player member = Player.onlinePlayers[memberName];
                    slot.icon.sprite = member.classIcon;
                    slot.healthSlider.value = member.health.Percent();
                    slot.manaSlider.value = member.mana.Percent();
                }
            }
        }
        else
        {
            panel.SetActive(false);
        }

    }
}
