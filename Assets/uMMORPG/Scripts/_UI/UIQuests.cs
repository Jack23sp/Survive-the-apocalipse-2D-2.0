// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UIQuests : MonoBehaviour
{
    public static UIQuests singleton;

    public Transform content;
    public QuestSlot slotPrefab;
    public Transform detailContent;
    public QuestSlotDetails slotPrefabDetails;

    public Button getReward;
    public Transform rewardContent;
    public GameObject rewardSlot;

    public GameObject rewardExperienceObject;
    public TextMeshProUGUI experienceText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI coinText;


    [HideInInspector] public int questIndex = -1;

    private Player player;

    public void Start()
    {
        if (!singleton) singleton = this;

        getReward.onClick.RemoveAllListeners();
        getReward.onClick.AddListener(() =>
        {
            if(questIndex > -1)
            {
                Player.localPlayer.quests.CmdClaimQuest(questIndex);
                questIndex = -1;
                Open();
            }
        });
    }

    public void Open()
    {
        questIndex = 0;
        rewardExperienceObject.SetActive(false);
        player = Player.localPlayer;
        if (player != null)
        {               
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.quests.MissionToAccomplish.Count, content);
           
            for (int i = 0; i < player.quests.MissionToAccomplish.Count; ++i)
            {
                int index = i;
                QuestSlot slot = content.GetChild(index).GetComponent<QuestSlot>();
                Missions quest = player.quests.MissionToAccomplish[index];

                // name button
                slot.title.text = quest.name;
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => {
                    questIndex = index;
                    SpawnQuestDetail(index);
                });
            }
            if (player.quests.MissionToAccomplish.Count > 0) content.GetChild(0).GetComponent<QuestSlot>().button.onClick.Invoke();
        }
    }

    public void SpawnQuestDetail(int questIndex)
    {
        Missions quest = player.quests.MissionToAccomplish[questIndex];
        rewardExperienceObject.SetActive(true);
        experienceText.text = "Experience : \n" + quest.rewardExperience.ToString();
        goldText.text = quest.rewardGold.ToString();
        coinText.text = quest.rewardCoins.ToString();

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in detailContent)
        {
            children.Add(child.gameObject);
        }
        foreach (GameObject child in children)
        {
            DestroyImmediate(child);
        }

        bool canTakeRewards = true;
        int nextIndex = 0;

        nextIndex = detailContent.childCount > 0 ? detailContent.childCount -1 : 0;

        UIUtils.BalancePrefabsNoDelete(slotPrefabDetails.gameObject, quest.craft.Count, detailContent);
        for(int i = 0; i < quest.craft.Count; i++)
        {
            int index = i;
            QuestSlotDetails slot = detailContent.GetChild(nextIndex).GetComponent<QuestSlotDetails>();
            slot.title.text = "Craft " +  quest.craft[index].actual + "/" + quest.craft[index].amountRequest + " " + quest.craft[index].item;
            slot.image.sprite = quest.craft[index].actual < quest.craft[index].amountRequest ? QuestManager.singleton.notCompleted : QuestManager.singleton.completed;
            if (quest.craft[index].actual < quest.craft[index].amountRequest) canTakeRewards = false;
            nextIndex++;
        }

        //nextIndex = detailContent.childCount > 0 ? detailContent.childCount -1 : 0;

        UIUtils.BalancePrefabsNoDelete(slotPrefabDetails.gameObject, quest.building.Count, detailContent);
        for (int i = 0; i < quest.building.Count; i++)
        {
            int index = i;
            QuestSlotDetails slot = detailContent.GetChild(nextIndex).GetComponent<QuestSlotDetails>();
            slot.title.text = "Build " + quest.building[index].actual + "/" + quest.building[index].amountRequest + " " + quest.building[index].item;
            slot.image.sprite = quest.building[index].actual < quest.building[index].amountRequest ? QuestManager.singleton.notCompleted : QuestManager.singleton.completed;
            if (quest.building[index].actual < quest.building[index].amountRequest) canTakeRewards = false;
            nextIndex++;
        }

        //nextIndex = detailContent.childCount > 0 ? detailContent.childCount -1 : 0;

        UIUtils.BalancePrefabsNoDelete(slotPrefabDetails.gameObject, quest.kills.Count, detailContent);
        for (int i = 0; i < quest.kills.Count; i++)
        {
            int index = i;
            QuestSlotDetails slot = detailContent.GetChild(nextIndex).GetComponent<QuestSlotDetails>();
            if (quest.kills[index].name.Contains("Animals_")) 
                slot.title.text = "Kill " + quest.kills[index].actual + "/" + quest.kills[index].amountRequest + " " + quest.kills[index].name.Replace("Animals_","");
            if (quest.kills[index].name.Contains("Zombie_")) 
                slot.title.text = "Kill " + quest.kills[index].actual + "/" + quest.kills[index].amountRequest + " " + quest.kills[index].name.Replace("Zombie_","") + " zombies";
            slot.image.sprite = quest.kills[index].actual < quest.kills[index].amountRequest ? QuestManager.singleton.notCompleted : QuestManager.singleton.completed;
            if (quest.kills[index].actual < quest.kills[index].amountRequest) canTakeRewards = false;
            nextIndex++;
        }

        //nextIndex = detailContent.childCount > 0 ? detailContent.childCount -1 : 0;

        UIUtils.BalancePrefabsNoDelete(slotPrefabDetails.gameObject, quest.pick.Count, detailContent);
        for (int i = 0; i < quest.pick.Count; i++)
        {
            int index = i;
            QuestSlotDetails slot = detailContent.GetChild(nextIndex).GetComponent<QuestSlotDetails>();
            slot.title.text = "Pick " + quest.pick[index].actual + "/" + quest.pick[index].amountRequest + " " + quest.pick[index].item;
            slot.image.sprite = quest.pick[index].actual < quest.pick[index].amountRequest ? QuestManager.singleton.notCompleted : QuestManager.singleton.completed;
            if (quest.pick[index].actual < quest.pick[index].amountRequest) canTakeRewards = false;
            nextIndex++;
        }

        //nextIndex = detailContent.childCount > 0 ? detailContent.childCount -1 : 0;

        UIUtils.BalancePrefabsNoDelete(slotPrefabDetails.gameObject, quest.players.Count, detailContent);
        for (int i = 0; i < quest.players.Count; i++)
        {
            int index = i;
            QuestSlotDetails slot = detailContent.GetChild(nextIndex).GetComponent<QuestSlotDetails>();
            slot.title.text = "Kill " + quest.players[index].actual + "/" + quest.players[index].amountRequest + " players";
            slot.image.sprite = quest.players[index].actual < quest.players[index].amountRequest ? QuestManager.singleton.notCompleted : QuestManager.singleton.completed;
            if (quest.players[index].actual < quest.players[index].amountRequest) canTakeRewards = false;
            nextIndex++;
        }

        UIUtils.BalancePrefabs(rewardSlot.gameObject, player.quests.MissionToAccomplish[questIndex].data.rewards.Count, rewardContent);
        for (int i = 0; i < player.quests.MissionToAccomplish[questIndex].data.rewards.Count; i++)
        {
            int index = i;
            BuyBoostSlot slot = rewardContent.GetChild(index).GetComponent<BuyBoostSlot>();
            slot.boostButton.enabled = false;
            //slot.boostButton.gameObject.SetActive(false);
            slot.boostButton.onClick.RemoveAllListeners();
            if (ScriptableItem.All.TryGetValue(player.quests.MissionToAccomplish[questIndex].data.rewards[index].item.GetStableHashCode(), out ScriptableItem itemData))
            {
                slot.boostImage.sprite = itemData.image;
                if (UIUtils.FindWhereTheItemIsCrafted(itemData) != null)
                {
                    slot.boostButton.enabled = true;
                    slot.boostButton.gameObject.SetActive(true);
                    slot.boostButton.onClick.AddListener(() =>
                    {
                        UIItemDetails.singleton.Open(itemData, UIUtils.FindWhereTheItemIsCrafted(itemData));
                    });
                }
            }          
            slot.title.text = player.quests.MissionToAccomplish[questIndex].data.rewards[index].item + " (" + player.quests.MissionToAccomplish[questIndex].data.rewards[index].amount + ")";
            //slot.boostImage.sprite = player.quests.quests[questIndex].data.rewards[index].item.image;
            slot.coinImage.gameObject.SetActive(false);
            slot.coins.gameObject.SetActive(false);
        }
        getReward.interactable = canTakeRewards;
    }
}
