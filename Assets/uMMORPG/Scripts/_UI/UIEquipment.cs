// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public partial class UIEquipment : MonoBehaviour
{
    public static UIEquipment singleton;
    public KeyCode hotKey = KeyCode.U; // 'E' is already used for rotating
    public UIEquipmentSlot slotPrefab;
    public Transform content;
    public Button dropMagazine;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    public void Awake()
    {
        if (!singleton) singleton = this;
    }

    public void Start()
    {
        Player player = Player.localPlayer;

        dropMagazine.onClick.RemoveAllListeners();
        dropMagazine.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(10);
            if (player.playerEquipment.slots[0].amount > 0)
                player.playerWeapon.CmdRemoveMagazine(player.playerEquipment.slots[0].item.data.name);
        });
    }

    public void Open()
    {
        Player player = Player.localPlayer;

        if (player)
        {
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.equipment.slots.Count, content);
            dropMagazine.gameObject.SetActive(player.playerWeapon.CheckAbleMunitionButton());
            // refresh all
            for (int i = 0; i < player.equipment.slots.Count; ++i)
            {
                UIEquipmentSlot slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                ItemSlot itemSlot = player.equipment.slots[i];

                // set category overlay in any case. we use the last noun in the
                // category string, for example EquipmentWeaponBow => Bow
                // (disabled if no category, e.g. for archer shield slot)
                EquipmentInfo slotInfo = ((PlayerEquipment)player.equipment).slotInfo[i];
                slot.categoryOverlay.SetActive(slotInfo.requiredCategory != "");
                string overlay = Utils.ParseLastNoun(slotInfo.requiredCategory);
                slot.categoryText.text = overlay != "" ? overlay : "?";

                if (itemSlot.amount > 0)
                {
                    // refresh valid item

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = false;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = false;
                    slot.registerItem.use = false;
                    slot.registerItem.equip = true;
                    slot.registerItem.delete = false;
                    slot.registerItem.equipmentSlot = true;
                    slot.registerItem.index = i;
                    slot.durabilitySlider.fillAmount = itemSlot.item.data.maxDurability.baseValue > 0 ? ((float)itemSlot.item.currentDurability / (float)itemSlot.item.data.maxDurability.Get(itemSlot.item.durabilityLevel)) : 0;
                    slot.unsanitySlider.fillAmount = 0;
                    // use durability colors?
                    /*if (itemSlot.item.maxDurability > 0)
                    {
                        if (itemSlot.item.durability == 0)
                            slot.image.color = brokenDurabilityColor;
                        else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                            slot.image.color = lowDurabilityColor;
                        else
                            slot.image.color = Color.white;
                    }
                    else*/
                    slot.image.color = Color.white; // reset for no-durability items
                    slot.image.preserveAspect = true;
                    slot.image.sprite = itemSlot.item.image;

                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                    slot.registerItem.use = false;
                    slot.registerItem.equip = false;
                    slot.registerItem.delete = false;
                    slot.registerItem.equipmentSlot = false;
                    slot.registerItem.index = i;
                    slot.durabilitySlider.fillAmount = 0;
                    slot.unsanitySlider.fillAmount = 0;

                }
            }
        }
    }
}
