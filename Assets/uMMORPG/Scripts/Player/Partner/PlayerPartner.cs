using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using AdvancedPeopleSystem;

public partial class Player
{
    [HideInInspector] public PlayerPartner playerPartner;
}

[System.Serializable]
public struct Partner
{
    public string guild;
    public string partnerName;
    public int level;
    public float currentHealth;
    public int maxHealth;
    public float currentStamina;
    public int staminaMax;
    public float accuracy;
    public float currentArmor;
    public float maxArmor;
    public List<Ability> abilities;
    public List<Boost> boosts;
    public List<string> alliance;
    public int sex;
    public int hair;
    public int beard;
    public string hairColor;
    public int underPants;
    public string underPantsColor;
    public string eyesColor;
    public string skinColor;
    public float fat;
    public float height;
    public float thin;
    public float muscles;
    public float breast;
    public string underwearColor;
    public int hat;
    public int accessory;
    public int upper;
    public int down;
    public int shoes;
    public int bag;
}

public partial class Database
{
    class partner
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public string partnerName { get; set; }
    }

    public void Connect_Partner()
    {
        connection.CreateTable<partner>();
    }

    public void SavePartner(Player player)
    {
        PlayerPartner partner = player.GetComponent<PlayerPartner>();
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        connection.Execute("DELETE FROM partner WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.
        connection.InsertOrReplace(new partner
        {
            characterName = player.name,
            partnerName = partner.partnerName
        });

    }
    public void LoadPartner(Player player)
    {
        PlayerPartner partner = player.GetComponent<PlayerPartner>();

        foreach (partner row in connection.Query<partner>("SELECT * FROM partner WHERE characterName=?", player.name))
        {
            partner.partnerName = row.partnerName;
        }

    }



    public Partner LoadPartnerStats(string partnerName)
    {
        Partner partner = new Partner();

        GameObject g = Instantiate(GameObjectSpawnManager.singleton.playerDummy);
        Player pl = g.GetComponent<Player>();

        partner.abilities = new List<Ability>();
        partner.alliance = new List<string>();
        partner.boosts = new List<Boost>();
        partner.guild = string.Empty;
        foreach (ability row in connection.Query<ability>("SELECT * FROM ability WHERE characterName=?", partnerName))
        {
            partner.abilities.Add(new Ability{
                name = row.abilityName,
                level = row.level,
                maxLevel = row.maxLevel,
                baseValue = row.baseValue
            });
        }

        foreach (characters row in connection.Query<characters>("SELECT * FROM characters WHERE name=?", partnerName))
        {
            partner.level = row.level;
            partner.currentHealth = row.health;
            partner.maxHealth = pl.health.baseHealth.Get(partner.level);

            partner.currentStamina = row.mana;
            partner.staminaMax = pl.mana.baseMana.Get(partner.level);
        }

        foreach (character_guild row in connection.Query<character_guild>("SELECT * FROM character_guild WHERE character=?", partnerName))
        {
            partner.guild = row.guild; 
        }

        foreach (partner row in connection.Query<partner>("SELECT * FROM partner WHERE characterName=?", partnerName))
        {
            partner.partnerName = row.partnerName;
        }


        partner.boosts = new List<Boost>();
        foreach (boosts row in connection.Query<boosts>("SELECT * FROM boosts WHERE character=?", partnerName))
        {
            partner.boosts.Add(new Boost
            {
                boostType = row.boostType,
                timeStart = row.timeStart,
                timeEnd = row.timeEnd,
                serverTimeBegin = row.serverTimeBegin,
                serverTimeEnd = row.serverTimeEnd,
                perc = row.perc
            });
        }

        float equipmentBonus = 0;
        foreach (Boost slot in partner.boosts)
            if (slot.boostType == "Precision")
                equipmentBonus += slot.perc;

        foreach (Ability slot in partner.abilities)
            if (slot.name == "Precision")
                equipmentBonus += slot.level * AbilityManager.singleton.FindAbility("Precision").bonus;

        partner.accuracy = pl.GetComponent<PlayerAccuracy>().linearAccuracy.Get(partner.level) + equipmentBonus;

        Database.singleton.LoadEquipment(((PlayerEquipment)pl.equipment));

        equipmentBonus = 0;
        foreach (ItemSlot slot in ((PlayerEquipment)pl.equipment).slots)
            if (slot.amount > 0)
                equipmentBonus += slot.item.currentArmor;

        partner.currentArmor = equipmentBonus;

        equipmentBonus = 0;
        foreach (ItemSlot slot in ((PlayerEquipment)pl.equipment).slots)
            if (slot.amount > 0)
                equipmentBonus += ((EquipmentItem)slot.item.data).armor.Get(slot.item.armorLevel);

        partner.maxArmor = equipmentBonus;


        foreach (guildAlly row in connection.Query<guildAlly>("SELECT * FROM guildAlly WHERE guildName=?", partnerName))
        {
            partner.alliance.Add(row.ally);
        }

        partner = LoadPartenerCharacterCreation(partnerName, partner);

        Destroy(g);

        return partner;
    }

    public Partner LoadPartenerCharacterCreation(string playerMame, Partner partner)
    {
        foreach (characterCreation row in connection.Query<characterCreation>("SELECT * FROM characterCreation WHERE characterName=?", playerMame))
        {
            partner.sex = row.sex;
            partner.hair = row.hairType;
            partner.beard = row.beard;
            partner.hairColor = row.hairColor;
            partner.underwearColor = row.underwearColor;
            partner.eyesColor = row.eyesColor;
            partner.skinColor = row.skinColor;
            partner.fat = row.fat;
            partner.thin = row.thin;
            partner.muscles = row.muscle;
            partner.height = row.height;
            partner.breast = row.breast;
            partner.hat = row.hats;
            partner.accessory = row.accessory;
            partner.upper = row.upper;
            partner.down = row.down;
            partner.shoes = row.shoes;
            partner.bag = row.bag;
        }

        return partner;
    }



    public void DeletePartner(string partnerName)
    {
        connection.Execute("DELETE FROM partner WHERE characterName=?", partnerName);

    }
}

public class PlayerPartner : NetworkBehaviour
{
    public Player player;

    [SyncVar]
    public string inviter;
    [SyncVar(hook = (nameof(SetHearth)))]
    public string partnerName;

    [Header("Partner")]
    [SyncVar]
    public Player _partner;

    public GameObject hearthRender;

    public int defaultHealth;
    public float defaultDefense;
    public int defaultMana;

    public Partner partner;

    private void Assign()
    {
        player = GetComponent<Player>();
        player.playerPartner = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        SetHearth(partnerName, partnerName);
    }

    public void SetHearth(string oldPartner, string newPartner)
    {
        if (newPartner == string.Empty)
        {
            hearthRender.SetActive(false);
            if (oldPartner != null)
            {
                if (_partner) _partner.playerPartner.hearthRender.SetActive(false);
            }
        }
        else
        {
            hearthRender.SetActive(true);
            if (_partner) _partner.playerPartner.hearthRender.SetActive(true);
        }
        if (UIStats.singleton) StatsManager.singleton.ManageStatSlot(UIStats.singleton.content.GetChild(StatsManager.singleton.FindIndexOfPartner()).GetComponent<UIStatsSlot>());
    }

    [Command]
    public void CmdLoadPartner(string partnerName)
    {
        Partner partner = new Partner();
        partner.abilities = new List<Ability>();
        if(partnerName != string.Empty)
            partner = Database.singleton.LoadPartnerStats(partnerName);
       TargetLoadPartner(partner);
    }

    [ClientRpc]
    public void TargetLoadPartner(Partner part)
    {
        partner = part;
        if (partner.abilities.Count > 0)
        {
            SetupPreview(partner, UICharacterCreationCustom.singleton.characterCustomization);
            UIPartner.singleton.playerDummy.gameObject.SetActive(true);
            UIPartner.singleton.creationCamera.enabled = true;

            UIPartner.singleton.partnerSlot.friendName.text = name;
            UIPartner.singleton.partnerSlot.guildName.text = "[" + part.guild + "] ";
            UIPartner.singleton.partnerSlot.level.text = part.level + " / " + player.level.max;
            UIPartner.singleton.partnerSlot.health.text = part.currentHealth + " / " + part.maxHealth;
            UIPartner.singleton.partnerSlot.stamina.text = part.currentStamina + " / " + part.staminaMax;
            UIPartner.singleton.partnerSlot.accuracy.text = part.accuracy + " / 100";
            UIPartner.singleton.partnerSlot.partner.text = part.partnerName;
            UIPartner.singleton.partnerSlot.armor.text = part.currentArmor + " / " + part.maxArmor;

            UIUtils.BalancePrefabs(UIPartner.singleton.partnerSlot.abilitySlot.gameObject, part.abilities.Count, UIPartner.singleton.partnerSlot.abilitiesContent);
            for (int i = 0; i < part.abilities.Count; i++)
            {
                int index = i;
                AbilitySlot slot = UIPartner.singleton.partnerSlot.abilitiesContent.GetChild(index).GetComponent<AbilitySlot>();
                slot.statName.text = part.abilities[index].name;
                slot.image.sprite = AbilityManager.singleton.FindAbility(part.abilities[index].name).image;
                slot.statAmount.text = part.abilities[index].level + " / " + part.abilities[index].maxLevel;
                slot.button.gameObject.SetActive(false);
                slot.button.onClick.RemoveAllListeners();
            }

            UIPartner.singleton.partnerSlot.guildSlot.SetActive(part.guild != string.Empty);
            if (UIPartner.singleton.partnerSlot.guildSlot.activeInHierarchy)
            {
                UIPartner.singleton.partnerSlot.personalGroupSlot.statName.text = part.guild;
                UIPartner.singleton.partnerSlot.personalGroupSlot.statAmount.text = string.Empty;//part.guild.guild.members.Length + " / " + GuildSystem.Capacity;
            }

            UIUtils.BalancePrefabs(UIPartner.singleton.partnerSlot.guildSlot, part.alliance.Count, UIPartner.singleton.partnerSlot.groupContent);
            for (int g = 0; g < part.alliance.Count; g++)
            {
                GroupSlot groupSlot = UIPartner.singleton.partnerSlot.groupContent.GetChild(g).GetComponent<GroupSlot>();
                groupSlot.statName.text = part.alliance[g];
            }


            UIPartner.singleton.partnerSlot.partnerPanel.gameObject.SetActive(true);
            UIPartner.singleton.partnerSlot.partnerNoPanel.SetActive(false);
        }
        else
        {
            UIPartner.singleton.partnerSlot.partnerPanel.gameObject.SetActive(false);
            UIPartner.singleton.partnerSlot.partnerNoPanel.gameObject.SetActive(true);
            UIPartner.singleton.playerDummy.gameObject.SetActive(false);
            UIPartner.singleton.creationCamera.enabled = false;
        }
        UIPartner.singleton.partnerSlot.gameObject.SetActive(true);
    }

    public void SetupPreview(Partner partner, CharacterCustomization characterCustomization)
    {
        Color newCol;
        characterCustomization.SwitchCharacterSettings(partner.sex);

        if (ColorUtility.TryParseHtmlString("#" + partner.hairColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
        }

        characterCustomization.SetElementByIndex(CharacterElementType.Hair, partner.hair);

        if (ColorUtility.TryParseHtmlString("#" + partner.underwearColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
        }
        if (ColorUtility.TryParseHtmlString("#" + partner.eyesColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
        }
        if (ColorUtility.TryParseHtmlString("#" + partner.skinColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
        }
        if (partner.sex == 0)
        {
            characterCustomization.SetElementByIndex(CharacterElementType.Beard, partner.beard);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, partner.fat);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, partner.thin);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, partner.muscles);
        }

        characterCustomization.SetHeight(partner.height);

        if (partner.sex == 1)
        {
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, partner.fat);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, partner.breast);
        }
        characterCustomization.SetElementByIndex(CharacterElementType.Item1, partner.bag);


        characterCustomization.SetElementByIndex(CharacterElementType.Shirt, partner.upper);
        characterCustomization.SetElementByIndex(CharacterElementType.Pants, partner.down);
        characterCustomization.SetElementByIndex(CharacterElementType.Shoes, partner.shoes);
        characterCustomization.SetElementByIndex(CharacterElementType.Accessory, partner.accessory);
        characterCustomization.SetElementByIndex(CharacterElementType.Hat, partner.hat);

        //characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);

    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        if (partnerName != string.Empty)
        {
            if (_partner == null)
            {
                if (Player.onlinePlayers.ContainsKey(partnerName))
                {
                    _partner = Player.onlinePlayers[partnerName];
                    Player.onlinePlayers[partnerName].playerPartner._partner = player;
                }
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (partnerName != string.Empty)
        {
            if (_partner != null)
            {
                if (Player.onlinePlayers.ContainsKey(partnerName))
                {
                    Player.onlinePlayers[partnerName].playerPartner._partner = null;
                }
            }
        }
    }

    public void InvitePartner(NetworkIdentity identity)
    {
        Player sender = identity.GetComponent<Player>();
        if (sender is Player && sender.playerPartner.partnerName == string.Empty && !player.playerOptions.blockMarriage)
        {
            player.playerPartner.inviter = sender.name;
        }
    }

    [Command]
    public void CmdAcceptInvitePartner()
    {
        Player onlinePlayer;
        Player _Ppartner;
        if (Player.onlinePlayers.TryGetValue(inviter, out onlinePlayer))
        {
            if (onlinePlayer && onlinePlayer.playerPartner.partnerName == string.Empty && player.playerPartner.partnerName == string.Empty)
            {
                onlinePlayer.playerPartner.partnerName = name;
                partnerName = onlinePlayer.name;
                if (Player.onlinePlayers.ContainsKey(inviter))
                {
                    _Ppartner = Player.onlinePlayers[inviter];
                    _Ppartner.playerPartner._partner = player.gameObject.GetComponent<Player>();
                }
                player.playerPartner._partner = onlinePlayer.gameObject.GetComponent<Player>();
            }
        }

        player.playerPartner.inviter = string.Empty;
    }

    [Command]
    public void DeclineInvite()
    {
        player.playerPartner.inviter = string.Empty;
    }

    [Command]
    public void CmdRemovePartner()
    {
        Player myPartner;
        if (Player.onlinePlayers.TryGetValue(player.playerPartner.partnerName, out myPartner))
        {
            Debug.Log($"Before - partnerName: {myPartner.playerPartner.partnerName}, _partner: {myPartner.playerPartner._partner}");

            myPartner.playerPartner.partnerName = string.Empty;
            myPartner.playerPartner._partner = null;

            Debug.Log($"After - partnerName: {myPartner.playerPartner.partnerName}, _partner: {myPartner.playerPartner._partner}");
        }
        else
        {
            Database.singleton.DeletePartner(player.playerPartner.partnerName);
        }
        partnerName = string.Empty;
        _partner = null;
    }
}
