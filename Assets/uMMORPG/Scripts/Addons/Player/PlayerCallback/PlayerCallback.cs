using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class Player
{
    [HideInInspector] public PlayerCallback playerCallback;
}

public class PlayerCallback : NetworkBehaviour
{
    public Player player;
    UIStatSlot searchedSlot;
    public ItemSlot searchedItem;
    public AudioSource heartBeatAudioSource;

    public void Start()
    {
        player = GetComponent<Player>();
        player.playerCallback = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        player.inventory.slots.Callback += OnInventoryChanged;
        player.equipment.slots.Callback += OnEquipmentChanged;

        player.playerFriends.friends.Callback += OnFriendsChanged;
        player.playerFriends.request.Callback += OnFriendsChanged;

        player.playerBelt.belt.Callback += OnBeltChanged;
        player.playerMove.states.Callback += OnStatesChanged;

        player.playerSpawnpoint.spawnpoint.Callback += OnSpawnpointChanged;
        player.playerScreenNotification.invitation.Callback += OnInvitationAdded;
        player.playerMove.ManageSneakAnimation();
        player.ManageState(player.state, player.state);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        player.inventory.slots.Callback += OnInventoryChangedOnServer;
        player.equipment.slots.Callback += OnEquipmentChangedOnServer;
        player.playerScreenNotification.invitation.Callback += OnInvitationAddedToServer;
    }

    void OnAllianceChanged(SyncList<string>.Operation op, int index, string oldGuildAlly, string newGuildAlly)
    {
        if (player.isLocalPlayer)
        {
            if (MenuButton.singleton)
            {
                if (MenuButton.singleton.panels[2].gameObject.activeInHierarchy)
                {
                    MenuButton.singleton.group.RefreshGuild();
                }
            }
        }
    }


    void OnInventoryChangedOnServer(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (newSlot.amount == 0 && oldSlot.amount > 0)
        {
            player.playerWeight.current -= (oldSlot.item.weight * oldSlot.amount);
        }
        else if (newSlot.amount > 0 && oldSlot.amount == 0)
        {
            player.playerWeight.current += (newSlot.item.weight * newSlot.amount);
        }
        else
        {
            player.playerWeight.current -= (oldSlot.item.weight * oldSlot.amount);
            player.playerWeight.current += (newSlot.item.weight * newSlot.amount);
        }

        if (player.playerWeight.current > player.playerWeight.max)
        {
            player.playerMove.ManageWeight();
        }
    }

    void OnInvitationAddedToServer(SyncList<InviteRequest>.Operation op, int index, InviteRequest oldSlot, InviteRequest newSlot)
    {
        if (player.playerScreenNotification.invitation.Count > 0)
        {
            player.playerScreenNotification.actualInviteRequest = player.playerScreenNotification.invitation[0];
        }
    }

    void OnInvitationAdded(SyncList<InviteRequest>.Operation op, int index, InviteRequest oldSlot, InviteRequest newSlot)
    {
        if (op == SyncList<InviteRequest>.Operation.OP_ADD)
        {

        }
        if (op == SyncList<InviteRequest>.Operation.OP_REMOVEAT)
        {

        }
    }

    public void Update()
    {
        if (player.isServer)
            player.playerEquipment.AssignMagazineItem();
    }

    void OnEquipmentChangedOnServer(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (index == 0)
        {
            player.playerEquipment.AssignMagazineItem();
        }

        if (newSlot.amount == 0 && oldSlot.amount > 0)
        {
            player.playerWeight.max -= oldSlot.item.data.maxWeight.Get(oldSlot.item.bagLevel);
        }
        else if (newSlot.amount > 0 && oldSlot.amount == 0)
        {
            player.playerWeight.max += newSlot.item.data.maxWeight.Get(newSlot.item.bagLevel);
        }
        else
        {
            player.playerWeight.max -= oldSlot.item.data.maxWeight.Get(oldSlot.item.bagLevel);
            player.playerWeight.max += newSlot.item.data.maxWeight.Get(newSlot.item.bagLevel);
        }

        if (player.playerWeight.current > player.playerWeight.max)
        {
            player.playerMove.ManageWeight();
        }
    }


    void OnEquipmentChangedOnLocalPlayer(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (index == 0)
        {
            if (UIMobileControl.singleton && AttackManager.singleton)
            {
                UIMobileControl.singleton.enableAttackButton.image.sprite = player.equipment.slots[0].amount > 0 ? player.equipment.slots[0].item.data.image : null;
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CheckConsume();

        if (TemperatureManager.singleton.isRainy)
            TemperatureManager.singleton.CheckRainParticle(TemperatureManager.singleton.isRainy, TemperatureManager.singleton.isRainy);

        if (TemperatureManager.singleton.isSnowy)
            TemperatureManager.singleton.CheckSnowParticle(TemperatureManager.singleton.isSnowy, TemperatureManager.singleton.isSnowy);

        player.equipment.slots.Callback += OnEquipmentChangedOnLocalPlayer;
        player.playerAlliance.guildAlly.Callback += OnAllianceChanged;

        if (UIFrontStats.singleton)
        {
            UIFrontStats.singleton.panel.SetActive(true);
            for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
            {
                if (UIFrontStats.singleton.stats[i].torch)
                {
                    searchedSlot = UIFrontStats.singleton.stats[i];
                    searchedItem = player.equipment.slots[6];
                    if (searchedItem.amount > 0)
                    {
                        searchedSlot.gameObject.SetActive(true);
                        searchedSlot.image.sprite = searchedItem.item.image;
                        searchedSlot.SpawnRestAmount(searchedItem.item.torchCurrentBattery - searchedSlot.intAmount);
                        searchedSlot.amount.text = searchedItem.item.torchCurrentBattery + " / " + ((EquipmentItem)searchedItem.item.data).battery.Get(searchedItem.item.batteryLevel);
                        searchedSlot.intAmount = searchedItem.item.torchCurrentBattery;
                    }
                    else
                    {
                        searchedSlot.image.sprite = ImageManager.singleton.torchImage;
                        searchedSlot.amount.text = "No torch!";
                        searchedSlot.intAmount = 0;
                    }
                }

                if (UIFrontStats.singleton.stats[i].bag)
                {
                    searchedSlot = UIFrontStats.singleton.stats[i];
                    searchedItem = player.equipment.slots[7];
                    if (searchedItem.amount > 0)
                    {
                        searchedSlot.gameObject.SetActive(true);
                        searchedSlot.amount.text = player.playerWeight.current + " / " + player.playerWeight.max;
                        searchedSlot.intAmount = (int)player.playerWeight.current;
                        searchedSlot.image.sprite = searchedItem.item.data.image;
                    }
                    else
                    {
                        searchedSlot.image.sprite = ImageManager.singleton.weightImage;
                        searchedSlot.amount.text = "No Bag (" + player.playerWeight.current + " / " + player.playerWeight.max + ")";
                        searchedSlot.intAmount = 0;
                    }
                }

            }
        }
        if (UITime.singleton)
        {
            UITime.singleton.Open();
        }
    }

    void OnStatesChanged(SyncList<string>.Operation op, int index, string oldState, string newState)
    {
        player.playerMove.ManageSneakAnimation();
    }

    void OnFriendsChanged(SyncList<string>.Operation op, int index, string oldSlot, string newSlot)
    {
        if (UIFriends.singleton)
        {
            UIFriends.singleton.Open();
        }
    }

    void OnSpawnpointChanged(SyncListSpawnPoint.Operation op, int index, Spawnpoint oldSlot, Spawnpoint newSlot)
    {
        if (UISpawnpoint.singleton) UISpawnpoint.singleton.RefreshSpawnpoint();
    }

    public void PartyChanged(Party oldParty, Party newParty)
    {
        if (UIPartyHUD.singleton)
            UIPartyHUD.singleton.RefreshPartyMember();

        if (UIPartyCustom.singleton)
        {
            if (UIPartyCustom.singleton.gameObject.activeInHierarchy)
            {
                UIPartyCustom.singleton.Open();
            }
        }

        if (MenuButton.singleton)
        {
            if (MenuButton.singleton.panels[2].gameObject.activeInHierarchy)
            {
                MenuButton.singleton.group.RefreshGuild();
            }
        }
    }

    public void GroupChanged(Guild oldGuild, Guild newGuild)
    {
        if (Player.localPlayer && Player.localPlayer.name == player.name)
        {
            if (UIGuildCustom.singleton &&
               UIGuildCustom.singleton.gameObject.activeInHierarchy &&
               UIGuildCustom.singleton.currentGuild.name == newGuild.name)
                UIGuildCustom.singleton.Open(newGuild);

            if (MenuButton.singleton)
            {
                if (MenuButton.singleton.panels[2].gameObject.activeInHierarchy)
                {
                    MenuButton.singleton.group.RefreshGuild();
                }
            }
        }
    }

    void OnBeltChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (!player.isLocalPlayer || (Player.localPlayer && Player.localPlayer.name != player.name)) return;

        if (UIMobileControl.singleton)
        {
            UIMobileControl.singleton.Refresh(player);
        }

        if (UISelectedItem.singleton && UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
        {
            if (UISelectedItem.singleton.ItemSlot.amount > 0) UISelectedItem.singleton.Setup(UISelectedItem.singleton.ItemSlot, UISelectedItem.singleton.delete, UISelectedItem.singleton.use, UISelectedItem.singleton.equip);
        }

        if (UIWaterContainer.singleton && UIWaterContainer.singleton.panel.gameObject.activeInHierarchy)
        {
            UIWaterContainer.singleton.Open(UIWaterContainer.singleton.waterCont);
        }

        if (ConfirmDropItem.singleton && ConfirmDropItem.singleton.panel.activeInHierarchy)
        {
            RefreshSelectedItem();
            bool isInventory = ConfirmDropItem.singleton.inventory;
            int am = isInventory ? player.inventory.slots[ConfirmDropItem.singleton.indexSlot].amount : player.playerBelt.belt[ConfirmDropItem.singleton.indexSlot].amount;
            if (am > 0)
            {
                ConfirmDropItem.singleton.Manage(isInventory);
            }
            else
            {
                ConfirmDropItem.singleton.closeButton.onClick.Invoke();
            }
        }
        else
        {
            RefreshSelectedItem();
        }

        if (UIScreenMagazine.singleton)
        {
            UIScreenMagazine.singleton.Refresh();
        }
    }


    void OnInventoryChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (!player.isLocalPlayer || (Player.localPlayer && Player.localPlayer.name != player.name)) return;

        if (UIWeaponStorage.singleton && UIWeaponStorage.singleton.panel.activeInHierarchy)
        {
            UIWeaponStorage.singleton.Open(UIWeaponStorage.singleton.weaponStorage, UIWeaponStorage.singleton.isReadOnly);
        }
        if (UIFridge.singleton.panel.activeInHierarchy)
        {
            UIFridge.singleton.Open(UIFridge.singleton.fridge, UIFridge.singleton.isReadOnly);
        }
        if (UIWarehouse.singleton.panel.activeInHierarchy)
        {
            UIWarehouse.singleton.Open(UIWarehouse.singleton.warehouse, UIWarehouse.singleton.isReadOnly);
        }
        if (UIInventoryCustom.singleton)
        {
            UIInventoryCustom.singleton.Open();
        }
        if (UICabinet.singleton && UICabinet.singleton.panel.activeInHierarchy)
        {
            UICabinet.singleton.Open(UICabinet.singleton.cabinet, UICabinet.singleton.isReadOnly);
        }
        if (UIFurnace.singleton && UIFurnace.singleton.panel.activeInHierarchy)
        {
            UIFurnace.singleton.Open(UIFurnace.singleton.furnace);
        }
        if (UILibrary.singleton.panel.activeInHierarchy)
        {
            UILibrary.singleton.Open(UILibrary.singleton.library, UILibrary.singleton.isReadOnly);
        }
        if (UIFrontStats.singleton && UIFrontStats.singleton.panel.activeInHierarchy)
        {
            for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
            {
                if (UIFrontStats.singleton.stats[i].bag)
                {
                    searchedSlot = UIFrontStats.singleton.stats[i];
                    searchedItem = player.equipment.slots[7];
                    if (searchedItem.amount > 0)
                    {
                        searchedSlot.gameObject.SetActive(true);
                        searchedSlot.image.sprite = searchedItem.item.image;
                        searchedSlot.amount.text = player.playerWeight.current + " / " + player.playerWeight.max;
                        searchedSlot.intAmount = (int)player.playerWeight.current;
                    }
                    else
                    {
                        searchedSlot.image.sprite = ImageManager.singleton.weightImage;
                        searchedSlot.amount.text = "No Bag (" + player.playerWeight.current + " / " + player.playerWeight.max + ")";
                        searchedSlot.intAmount = 0;
                    }
                }
            }
        }

        if (UIPetStatusManagement.singleton && UIPetStatusManagement.singleton.panel.activeInHierarchy)
            UIPetStatusManagement.singleton.Open();

        if (UICentralManager.singleton && UICentralManager.singleton.panel.activeInHierarchy)
        {
            UICentralManager.singleton.RefreshPanelView(UICentralManager.singleton.selectedPanel);
        }

        if (UISelectedItem.singleton && UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
        {
            if(UISelectedItem.singleton.ItemSlot.amount > 0) 
                UISelectedItem.singleton.Setup(UISelectedItem.singleton.ItemSlot, UISelectedItem.singleton.delete, UISelectedItem.singleton.use, UISelectedItem.singleton.equip);
        }

        if (UIWaterContainer.singleton && UIWaterContainer.singleton.panel.gameObject.activeInHierarchy)
        {
            UIWaterContainer.singleton.Open(UIWaterContainer.singleton.waterCont);
        }
              
        if(UIInventoryCustom.singleton && UIInventoryCustom.singleton.operationType != -1)
        {
            UIInventoryCustom.singleton.SearchItemToManage(UIInventoryCustom.singleton.operationType,true);
        }


        player.playerEquipment.ManageMunition();
        if (ConfirmDropItem.singleton && ConfirmDropItem.singleton.panel.activeInHierarchy)
        {
            RefreshSelectedItem();
            UISelectedItem.singleton.dropButton.onClick.Invoke();
        }
        else
        {
            RefreshSelectedItem();
        }

        if (UIScreenMagazine.singleton)
        {
            UIScreenMagazine.singleton.Refresh();
        }
    }

    public void RefreshSelectedItem()
    {
        if (UISelectedItem.singleton && UISelectedItem.singleton.panel.gameObject.activeInHierarchy)
        {
            if (UISelectedItem.singleton.ItemSlot.amount > 0) UISelectedItem.singleton.CheckSpawnItem();
        }
    }

    void OnEquipmentChanged(SyncList<ItemSlot>.Operation op, int index, ItemSlot oldSlot, ItemSlot newSlot)
    {
        if (!player.isLocalPlayer || (Player.localPlayer && Player.localPlayer.name != player.name)) return;

        if (PanelEquipment.singleton)
        {
            PanelEquipment.singleton.RefreshEquipment();
        }
        if (UIKitchenSink.singleton)
        {
            UIKitchenSink.singleton.SearchWaterBottle();
        }
        if (UIWaterContainer.singleton)
        {
            UIWaterContainer.singleton.SearchWaterBottle();
        }
        if (UIMobileControl.singleton)
        {
            player.playerEquipment.ManageMunition();
            if (UIMobileControl.singleton.weaponImage)
            {
                if (index == 0)
                {
                    UIMobileControl.singleton.weaponImage.gameObject.SetActive(newSlot.amount > 0);

                    if (UIMobileControl.singleton.weaponImage.gameObject.activeInHierarchy)
                    {
                        UIMobileControl.singleton.weaponImage.sprite = newSlot.item.data.image;
                        UIMobileControl.singleton.weaponImage.preserveAspect = true;
                    }
                }
            }
        }

        if (UIEquipment.singleton)
        {
            UIEquipment.singleton.Open();
        }

        if (UIFrontStats.singleton)
        {
            if (index == 6)
            {
                for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
                {
                    if (UIFrontStats.singleton.stats[i].torch)
                    {
                        searchedSlot = UIFrontStats.singleton.stats[i];
                        searchedItem = player.equipment.slots[6];
                        if (searchedItem.amount > 0)
                        {
                            searchedSlot.image.gameObject.SetActive(true);
                            searchedSlot.image.sprite = searchedItem.item.image;
                            searchedSlot.SpawnRestAmount(searchedItem.item.torchCurrentBattery - searchedSlot.intAmount);
                            searchedSlot.amount.text = searchedItem.item.torchCurrentBattery + " / " + ((EquipmentItem)searchedItem.item.data).battery.Get(searchedItem.item.batteryLevel);
                            searchedSlot.intAmount = searchedItem.item.torchCurrentBattery;
                        }
                        else
                        {
                            searchedSlot.image.sprite = ImageManager.singleton.torchImage;
                            searchedSlot.image.gameObject.SetActive(false);
                            searchedSlot.amount.text = "No torch!";
                            searchedSlot.intAmount = 0;
                        }
                    }
                }
            }
            if (index == 6)
            {
                player.playerTorch.CheckTorch();
            }
            if (index == 7)
            {
                for (int i = 0; i < UIFrontStats.singleton.stats.Count; i++)
                {
                    if (UIFrontStats.singleton.stats[i].bag)
                    {
                        searchedSlot = UIFrontStats.singleton.stats[i];
                        searchedItem = player.equipment.slots[7];
                        if (searchedItem.amount > 0)
                        {
                            searchedSlot.gameObject.SetActive(true);
                            searchedSlot.image.sprite = searchedItem.item.image;
                            searchedSlot.amount.text = player.playerWeight.current + " / " + player.playerWeight.max;
                            searchedSlot.intAmount = (int)player.playerWeight.current;
                        }
                        else
                        {
                            searchedSlot.image.sprite = ImageManager.singleton.weightImage;
                            searchedSlot.amount.text = "No Bag (" + player.playerWeight.current + " / " + player.playerWeight.max + ")";
                            searchedSlot.intAmount = 0;
                        }
                    }
                }

            }
        }

    }

    public void CheckConsume()
    {
        if (player.isLocalPlayer || (Player.localPlayer && Player.localPlayer.name == player.name))
        {
            if (player.party.InParty() && player.party.party.members.ToList().Contains(player.name))
            {
                UIPartyHUD.singleton.RefreshPartyMember();
            }

            if (player.health.Percent() <= 0.3f && player.health.Percent() > 0.0f)
            {
                UIBlood.singleton.ManageAnimator(true);
                heartBeatAudioSource.volume = player.playerOptions.blockSound ? 0.0f : 1.0f;
                heartBeatAudioSource.Play();
            }
            else
            {
                UIBlood.singleton.ManageAnimator(false);
                heartBeatAudioSource.Stop();
            }

            if (player.health.current == 0)
            {
                ModularBuildingManager.singleton.CancelBuildingMode();
                player.health.CmdCallHealthReachZero();
                Invoke(nameof(OpenSpawnpointPanelOnDeath), 2.0f);
            }
            UIPlayerInformation.singleton.Open();
        }
    }

    public void OpenSpawnpointPanelOnDeath()
    {
        MenuButton.singleton.openPanel.onClick.Invoke();
        MenuButton.singleton.spawnpointButton.onClick.Invoke();
        MenuButton.singleton.SpawnpointManageButtonForSpawnpoint(false);
    }

    [TargetRpc]
    public void TargetManageGrass(NetworkIdentity grassIdentity, bool condition)
    {
        grassIdentity.gameObject.SetActive(condition);
    }
}