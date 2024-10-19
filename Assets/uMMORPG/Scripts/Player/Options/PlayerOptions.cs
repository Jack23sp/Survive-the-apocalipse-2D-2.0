using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEditor;

public partial class Player
{
    [HideInInspector] public PlayerOptions playerOptions;
}

public partial class Database
{
    class options
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int blockMarriage { get; set; }
        public int blockParty { get; set; }
        public int blockGroup { get; set; }
        public int blockAlly { get; set; }
        public int blockTrade { get; set; }
        public int blockFriend { get; set; }
        public int blockFootstep { get; set; }
        public int blockSound { get; set; }
        public int blockButtonSound { get; set; }
        public int postProcessing { get; set; }
        public float buildingSensibility { get; set; }
        public int showZoomButtonsOnScreen { get; set; }
    }

    class issue
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public int index { get; set; }
        public string characterName { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string operatorID { get; set; }
        public string closed { get; set; }
    }

    public void Connect_Options()
    {
        connection.CreateTable<issue>();
        connection.CreateTable<options>();
    }


    public void SaveOptions(Player player)
    {
        PlayerOptions options = player.GetComponent<PlayerOptions>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM options WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new options
        {
            characterName = player.name,
            blockMarriage = Convert.ToInt32(options.blockMarriage),
            blockParty = Convert.ToInt32(options.blockParty),
            blockGroup = Convert.ToInt32(options.blockGroup),
            blockAlly = Convert.ToInt32(options.blockAlly),
            blockTrade = Convert.ToInt32(options.blockTrade),
            blockFriend = Convert.ToInt32(options.blockFriend),
            blockFootstep = Convert.ToInt32(options.blockFootstep),
            blockSound = Convert.ToInt32(options.blockSound),
            blockButtonSound = Convert.ToInt32(options.blockButtonSounds),
            postProcessing = Convert.ToInt32(options.postProcessing),
            buildingSensibility = options.buildingSensibility,
            showZoomButtonsOnScreen = Convert.ToInt32(options.showZoomButtonOnScreen)
        });

    }
    public void LoadOptions(Player player)
    {
        PlayerOptions options = player.GetComponent<PlayerOptions>();

        foreach (options row in connection.Query<options>("SELECT * FROM options WHERE characterName=?", player.name))
        {
            options.blockMarriage = Convert.ToBoolean(row.blockMarriage);
            options.blockParty = Convert.ToBoolean(row.blockParty);
            options.blockGroup = Convert.ToBoolean(row.blockGroup);
            options.blockAlly = Convert.ToBoolean(row.blockAlly);
            options.blockTrade = Convert.ToBoolean(row.blockTrade);
            options.blockFriend = Convert.ToBoolean(row.blockFriend);
            options.blockFootstep = Convert.ToBoolean(row.blockFootstep);
            options.blockSound = Convert.ToBoolean(row.blockSound);
            options.blockButtonSounds = Convert.ToBoolean(row.blockButtonSound);
            options.postProcessing = Convert.ToBoolean(row.postProcessing);
            options.buildingSensibility = row.buildingSensibility;
            options.showZoomButtonOnScreen = Convert.ToBoolean(row.showZoomButtonsOnScreen);
        }

    }

    public void SaveIssue(Player player, string Type, string description)
    {
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new issue
        {
            characterName = player.name,
            positionX = player.transform.position.x,
            positionY = player.transform.position.y,
            description = description,
            type = Type,
            operatorID = "",
            closed = ""
        });
    }
}

public class PlayerOptions : NetworkBehaviour
{
    private Player player;


    [SyncVar(hook = nameof(ManageMarriage))]
    public bool blockMarriage;
    [SyncVar(hook = nameof(ManageParty))]
    public bool blockParty;
    [SyncVar(hook = nameof(ManageGroup))]
    public bool blockGroup;
    [SyncVar(hook = nameof(ManageAlly))]
    public bool blockAlly;
    [SyncVar(hook = nameof(ManageTrade))]
    public bool blockTrade;
    [SyncVar(hook = nameof(ManageFriend))]
    public bool blockFriend;
    [SyncVar(hook = nameof(ManageFootstep))]
    public bool blockFootstep;
    [SyncVar(hook = nameof(ManageSound))]
    public bool blockSound;
    [SyncVar(hook = nameof(ManageButtonSounds))]
    public bool blockButtonSounds;
    [SyncVar(hook = nameof(ManagePostProcessing))]
    public bool postProcessing;
    [SyncVar(hook = nameof(ManageBuildingSensibility))]
    public float buildingSensibility;
    [SyncVar(hook = nameof(ShowZoomButtonOnScreen))]
    public bool showZoomButtonOnScreen;

    [SyncVar]
    public double nextRiskyActionTime;

    public GameObject uiRestoreCredential;

    public AudioSource[] audioSource;
    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerOptions = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (!player) Assign();
        ManageMarriage(blockMarriage, blockMarriage);
        ManageParty(blockParty, blockParty);
        ManageGroup(blockGroup, blockGroup);
        ManageAlly(blockAlly, blockAlly);
        ManageTrade(blockTrade, blockTrade);
        ManageFriend(blockFriend, blockFriend);
        ManageFootstep(blockFootstep, blockFootstep);
        ManageButtonSounds(blockButtonSounds, blockButtonSounds);
        ManageSound(blockSound, blockSound);
        ManageBuildingSensibility(buildingSensibility, buildingSensibility);
        ShowZoomButtonOnScreen(showZoomButtonOnScreen, showZoomButtonOnScreen);
    }

    public void ManageBuildingSensibility(float oldValue, float newValue)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.sensibilityBuildingPlacement.value = newValue;
            }
            ModularBuildingManager.singleton.sensibility = newValue;
        }
    }


    public void ManageMarriage(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if(UIOptions.singleton)
            {
                UIOptions.singleton.marriageObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageParty(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.partyObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageGroup(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.groupObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageAlly(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.allyObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageTrade(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.tradeObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageFriend(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.friendsObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageFootstep(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.footstepObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManageButtonSounds(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.buttonSoundsObject.EnableOnObject(newBool);
            }
        }
    }

    public void ManagePostProcessing(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UIOptions.singleton)
            {
                UIOptions.singleton.buttonPostProcessing.EnableOnObject(newBool);
                PostProcessingManager.singleton.postEffect.SetActive(postProcessing);
            }
        }
    }

    public void ManageSound(bool oldBool, bool newBool)
    {
        if(!player) Assign();
        if (player.isLocalPlayer)
        {
            audioSource = FindObjectOfType<SoundManager>().GetComponents<AudioSource>();
            TemperatureManager.singleton.ChangeVolume(newBool);
            for (int i = 0; i < audioSource.Length; i++)
            {
                int index = i;
                audioSource[index].enabled = !newBool;
            }

            if (UIOptions.singleton)
            {
                UIOptions.singleton.soundsObject.EnableOnObject(newBool);
            }
        }
    }

    public void ShowZoomButtonOnScreen(bool oldBool, bool newBool)
    {
        if (!player) Assign();
        if (player.isLocalPlayer)
        {
            if (UICameraZoom.singleton) UICameraZoom.singleton.panel.gameObject.SetActive(newBool);
            if (UIOptions.singleton)
            {
                UIOptions.singleton.toogleZoom.isOn = newBool;
            }
        }
    }

    [Command]
    public void CmdSaveIssue(string playerName, string Type, string description)
    {
        Player onlinePlayer;
        if (Player.onlinePlayers.TryGetValue(playerName, out onlinePlayer))
        {
            Database.singleton.SaveIssue(onlinePlayer, Type, description);
        }
    }

    [Command]
    public void CmdBlockAlly()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockAlly = !blockAlly;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockFootstep()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockFootstep = !blockFootstep;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockFriends()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockFriend = !blockFriend;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockGroup()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockGroup = !blockGroup;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockMarriage()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockMarriage = !blockMarriage;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockParty()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockParty = !blockParty;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockSound()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockSound = !blockSound;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockButtonSound()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockButtonSounds = !blockButtonSounds;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdBlockTrade()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        blockTrade = !blockTrade;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdManagePostProcessing()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        postProcessing = !postProcessing;
        nextRiskyActionTime = NetworkTime.time + 1;
    }

    [Command]
    public void CmdChangeSensibility(float sensibility)
    {
        buildingSensibility = sensibility;
    }

    [Command]
    public void CmdShowZoomButtonsOnScreen()
    {
        if (NetworkTime.time < nextRiskyActionTime) return;
        showZoomButtonOnScreen = !showZoomButtonOnScreen;
        nextRiskyActionTime = NetworkTime.time + 1;
    }
}
