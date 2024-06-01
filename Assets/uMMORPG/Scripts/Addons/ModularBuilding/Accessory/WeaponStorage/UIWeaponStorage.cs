using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class ScriptableItem
{
    public int weaponType = -1;
}

public class UIWeaponStorage : MonoBehaviour
{
    public static UIWeaponStorage singleton;
    public GameObject panel;

    public List<UIInventorySlot> weapon = new List<UIInventorySlot>();
    public WeaponStorage weaponStorage;
    private Player player;
    public GameObject objectToSpawn;
    public Transform inventoryContainer;

    public Button closeButton;
    public Button manageButton;

    private Image img;

    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void Open(WeaponStorage storageWeapon)
    {
        player = Player.localPlayer;
        if(!player) return;

        weaponStorage = storageWeapon;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.SetListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
            closeButton.image.raycastTarget = false;
            Close();
            closeButton.image.enabled = false;
            BlurManager.singleton.Show();
        });

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(weaponStorage, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(weaponStorage.netIdentity, weaponStorage.craftingAccessoryItem, closeButton);
        });


        closeButton.image.enabled = true;

        panel.SetActive(true);
        closeButton.image.raycastTarget = true;

        UIUtils.BalancePrefabs(objectToSpawn, player.inventory.slots.Count, inventoryContainer);
        for (int i = 0; i < player.inventory.slots.Count; i++)
        {
            int icopy = i;
            UIInventorySlot slot = inventoryContainer.GetChild(icopy).GetComponent<UIInventorySlot>();
            ItemSlot itemSlot = player.inventory.slots[icopy];
            slot.image.preserveAspect = true;
            slot.dragAndDropable.enabled = false;

            if (itemSlot.amount > 0)
            {
                if (player.inventory.slots[icopy].item.data.canUseWeaponStorage)
                {
                    slot.button.interactable = true;
                }
                else
                {
                    slot.button.interactable = false;
                }

                slot.button.onClick.SetListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.CmdAddWeaponToWeaponStorage(icopy, player.inventory.slots[icopy].item.data.weaponType, weaponStorage.GetComponent<NetworkIdentity>());
                });
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.data.skinImages.Count > 0 && itemSlot.item.skin > -1 ?
                                    itemSlot.item.data.skinImages[itemSlot.item.skin] :
                                    itemSlot.item.data.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
                slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = player.inventory.slots[icopy].item.data.maxDurability.baseValue > 0 ? ((float)player.inventory.slots[icopy].item.currentDurability / (float)player.inventory.slots[icopy].item.data.maxDurability.Get(player.inventory.slots[icopy].item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = player.inventory.slots[icopy].item.data.maxUnsanity > 0 ? ((float)player.inventory.slots[icopy].item.currentUnsanity / (float)player.inventory.slots[icopy].item.data.maxUnsanity) : 0;
            }
            else
            {
                slot.button.onClick.RemoveAllListeners();
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
                slot.registerItem.index = icopy;
                slot.durabilitySlider.fillAmount = 0;
                slot.unsanitySlider.fillAmount = 0;
            }
        }

        for (int i = 0; i < weaponStorage.weapon.Count; i++)
        {
            int index = i;
            UIInventorySlot slot = weapon[index];
            if (weaponStorage.weapon[index].amount > 0)
            {
                slot.gameObject.transform.parent.GetComponent<Image>().enabled = false;
                slot.gameObject.SetActive(true);
                slot.image.color = Color.white;
                slot.image.sprite = weaponStorage.weapon[index].item.data.skinImages.Count > 0 && weaponStorage.weapon[index].item.skin > -1 ?
                                    weaponStorage.weapon[index].item.data.skinImages[weaponStorage.weapon[index].item.skin] :
                                    weaponStorage.weapon[index].item.data.image;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(weaponStorage.weapon[index].amount > 1);
                slot.amountText.text = weaponStorage.weapon[index].amount.ToString();
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    player.CmdAddToInventoryFromWeaponStorage(index,weaponStorage.GetComponent<NetworkIdentity>());
                });
                slot.registerItem.index = index;
                slot.durabilitySlider.fillAmount = weaponStorage.weapon[index].item.data.maxDurability.baseValue > 0 ? ((float)weaponStorage.weapon[index].item.currentDurability / (float)weaponStorage.weapon[index].item.data.maxDurability.Get(weaponStorage.weapon[index].item.durabilityLevel)) : 0;
                slot.unsanitySlider.fillAmount = weaponStorage.weapon[index].item.data.maxUnsanity > 0 ? ((float)weaponStorage.weapon[index].item.currentUnsanity / (float)weaponStorage.weapon[index].item.data.maxUnsanity) : 0;
            }
            else
            {
                slot.button.onClick.RemoveAllListeners();
                img = slot.gameObject.transform.parent.GetComponent<Image>();
                img.enabled = true;
                slot.gameObject.SetActive(false);
                img.color = new Color(img.color.r,img.color.g,img.color.b,0.5f);
                slot.registerItem.index = -1;
                slot.durabilitySlider.fillAmount = 0;
                slot.unsanitySlider.fillAmount = 0;
            }
        }
    }
}
