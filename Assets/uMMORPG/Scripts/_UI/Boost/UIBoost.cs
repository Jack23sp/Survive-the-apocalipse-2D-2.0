using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIBoost : MonoBehaviour
{
    public static UIBoost singleton;
    public GameObject panel;
    private Player player;
    public Transform boostContent;
    public GameObject objectToSpawn;

    public ScriptableBoost selectedBoost;
    public GameObject uiToSpawn;

    public DetailBoost detailBoost;
    public Transform detailBoostContent;

    private TimeSpan difference;

    void Start()
    {
        if (!singleton) singleton = this;

        if (!player) player = Player.localPlayer;
        if (!player) return;
        UpdateBoost();
    }

    private void OnEnable()
    {
        Invoke(nameof(RefreshDetails), 1.0f);
    }

    public void RefreshDetails()
    {
        if (panel.activeInHierarchy)
        {
            UIUtils.BalancePrefabs(detailBoost.gameObject, Player.localPlayer.playerBoost.boosts.Count, detailBoostContent);
            for (int i = 0; i < detailBoostContent.childCount; i++)
            {
                int index = i;
                DetailBoost slot = detailBoostContent.GetChild(index).GetComponent<DetailBoost>();
                difference = DateTime.Parse(ConvertDate(player.playerBoost.boosts[index].timeEnd)) - DateTime.Parse(ConvertDate(DateTime.UtcNow.ToString())) ;
                if (difference.TotalSeconds <= 0)
                {
                    slot.gameObject.SetActive(false);
                }
                else
                {
                    slot.gameObject.SetActive(true);
                    slot.boostImage.sprite = Player.localPlayer.playerBoost.LookAtBoostTemplateImage(Player.localPlayer.playerBoost.boosts[index].boostType);
                    slot.description.text = Player.localPlayer.playerBoost.boosts[index].boostType + (Player.localPlayer.playerBoost.boosts[index].perc == 0.0f ? "" : (" (" + Player.localPlayer.playerBoost.boosts[index].perc + "%)"));
                    slot.boostDescription.text = Player.localPlayer.playerBoost.LookAtBoostTemplateDescription(Player.localPlayer.playerBoost.boosts[index].boostType);
                    slot.timer.text = difference.TotalSeconds >= 0 ? TimeManager.singleton.ConvertToTimer(Convert.ToInt32(difference.TotalSeconds)) : TimeManager.singleton.ConvertToTimer(0);
                }
            }
        }
        Invoke(nameof(RefreshDetails), 1.0f);
    }

    public void OnDisable()
    {
        CancelInvoke(nameof(RefreshDetails));
    }

    public void Reset()
    {
        boostContent.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public void UpdateBoost()
    {
        UIUtils.BalancePrefabs(objectToSpawn, BoostManager.singleton.allBoosts.Count, boostContent);
        for (int i = 0; i < boostContent.childCount; i++)
        {
            int index = i;

            BuyBoostSlot slot = boostContent.GetChild(index).GetComponent<BuyBoostSlot>();
            slot.title.text = BoostManager.singleton.allBoosts[index].name;
            slot.coins.gameObject.SetActive(true);
            slot.gold.gameObject.SetActive(false);
            slot.coins.text = BoostManager.singleton.allBoosts[index].coin.ToString();
            slot.gold.text = "";
            slot.boostImage.sprite = BoostManager.singleton.allBoosts[index].image;
            slot.timer.text = TimeManager.singleton.ConvertToTimer(Convert.ToInt32(ManageTimer(index)));
            slot.boostButton.onClick.RemoveAllListeners();
            slot.boostButton.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                selectedBoost = BoostManager.singleton.allBoosts[index];
                var instantiated = Instantiate(uiToSpawn, GameObjectSpawnManager.singleton.canvas);
                instantiated.GetComponent<ConfirmBuyBoost>().boostMain = this;
                instantiated.GetComponent<ConfirmBuyBoost>().Refresh();
            });
        }
    }

    public double ManageTimer(int boostIndex)
    {
        if (BoostManager.singleton.allBoosts[boostIndex].velocityTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].velocityTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].dexterityTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].dexterityTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].soldierTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].soldierTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].precisionTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].precisionTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].doubleEXP > 0) return BoostManager.singleton.allBoosts[boostIndex].doubleEXP;
        if (BoostManager.singleton.allBoosts[boostIndex].doubleGold > 0) return BoostManager.singleton.allBoosts[boostIndex].doubleGold;
        if (BoostManager.singleton.allBoosts[boostIndex].doubleLeaderPoints > 0) return BoostManager.singleton.allBoosts[boostIndex].doubleLeaderPoints;
        if (BoostManager.singleton.allBoosts[boostIndex].doubleDamageToMonster > 0) return BoostManager.singleton.allBoosts[boostIndex].doubleDamageToMonster;
        if (BoostManager.singleton.allBoosts[boostIndex].doubleDamageToPlayer > 0) return BoostManager.singleton.allBoosts[boostIndex].doubleDamageToPlayer;
        if (BoostManager.singleton.allBoosts[boostIndex].doubleDamageToBuilding > 0) return BoostManager.singleton.allBoosts[boostIndex].doubleDamageToBuilding;
        if (BoostManager.singleton.allBoosts[boostIndex].healthTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].healthTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].staminaTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].staminaTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].aimTimer > 0) return BoostManager.singleton.allBoosts[boostIndex].aimTimer;
        if (BoostManager.singleton.allBoosts[boostIndex].aimPrecision > 0) return BoostManager.singleton.allBoosts[boostIndex].aimPrecision;

        return 0;
    }

    string ConvertDate(string dateToConvert)
    {
        DateTime inputDate;
        if (DateTime.TryParseExact(dateToConvert, "MM/dd/yyyy h:mm:ss tt", null, System.Globalization.DateTimeStyles.None, out inputDate))
        {
            string outputFormat = "dd/MM/yyyy HH:mm:ss";
            string outputDateString = inputDate.ToString(outputFormat);

            return outputDateString;

        }
        return dateToConvert;
    }
}
