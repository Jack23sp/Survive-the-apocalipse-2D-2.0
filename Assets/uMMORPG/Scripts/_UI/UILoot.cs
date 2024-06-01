// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class UILoot : MonoBehaviour
{
    public static UILoot singleton;
    public GameObject panel;
    public Button goldButton;
    public TextMeshProUGUI goldText;
    public Color hasGoldColor = Color.yellow;
    public Color emptyGoldColor = Color.gray;
    public UILootSlot itemSlotPrefab;
    public Transform content;
    private Player player;
    private Monster monster;
    public Button closeButton;

    public UILoot()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    public void Start()
    {
        goldButton.onClick.RemoveAllListeners();
        goldButton.onClick.AddListener(() => {
            player.looting.CmdTakeGold(monster.netIdentity);
        });

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => {
            Close();
        });
    }
    public void Close()
    {
        panel.SetActive(false);
    }

    public void Show(Monster zombie) 
    {
        monster = zombie;
        player = Player.localPlayer;
        panel.SetActive(true);

        goldButton.interactable = monster.gold > 0;
        goldText.text = monster.gold.ToString();
        goldText.color = monster.gold > 0 ? hasGoldColor : emptyGoldColor;


        UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, monster.inventory.slots.Count, content);

        // refresh all valid items
        for (int i = 0; i < monster.inventory.slots.Count; ++i)
        {
            ItemSlot itemSlot = monster.inventory.slots[i];

            UILootSlot slot = content.GetChild(i).GetComponent<UILootSlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop index

            if (itemSlot.amount > 0)
            {
                // refresh valid item
                slot.button.interactable = player.inventory.CanAdd(itemSlot.item, itemSlot.amount);
                int icopy = i;
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() => {
                    player.looting.CmdTakeItem(icopy, monster.netIdentity);
                });
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = itemSlot.ToolTip();
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                slot.nameText.text = itemSlot.item.name;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                // refresh invalid item
                slot.button.interactable = false;
                slot.button.onClick.RemoveAllListeners();
                slot.tooltip.enabled = false;
                slot.tooltip.text = "";
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.nameText.text = "";
                slot.amountOverlay.SetActive(false);
            }
        }
    }
}
