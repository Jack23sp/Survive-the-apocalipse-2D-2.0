
using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using AdvancedPeopleSystem;

public partial class Player
{
    [HideInInspector] public PlayerCharacterCreation playerCharacterCreation;
}

public partial class Database
{
    class characterCreation
    {
        //[PrimaryKey] // important for performance: O(log n) instead of O(n)
        //[Collation("NOCASE")] // [COLLATE NOCASE for case insensitive compare. this way we can't both create 'Archer' and 'archer' as characters]
        public string characterName { get; set; }
        public int sex { get; set; }
        public int hairType { get; set; }
        public int beard { get; set; }
        public string hairColor { get; set; }
        public string underwearColor { get; set; }
        public string eyesColor { get; set; }
        public string skinColor { get; set; }
        public float fat { get; set; }
        public float thin { get; set; }
        public float muscle { get; set; }
        public float height { get; set; }
        public float breast { get; set; }
        public int hats { get; set; }
        public int accessory { get; set; }
        public int upper { get; set; }
        public int down { get; set; }
        public int shoes { get; set; }
        public int bag { get; set; }
        public int alreadyAssigned { get; set; }
    }

    public void Connect_CharacterCreation()
    {
        connection.CreateTable<characterCreation>();
    }

    public void SaveCharacterCreation(Player player)
    {
        player.playerCharacterCreation = player.GetComponent<PlayerCharacterCreation>();

        connection.Execute("DELETE FROM characterCreation WHERE characterName=?", player.name);
        // note: .Insert causes a 'Constraint' exception. use Replace.

        if (!player.playerCharacterCreation.alreadyAssigned)
        {
            if (player.playerCharacterCreation.sex == 0)
            {
                player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Shirt"))] = new ItemSlot(new Item(CharacterCreationManager.singleton.maleUp), 1);
                player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Pant"))] = new ItemSlot(new Item(CharacterCreationManager.singleton.maleDown), 1);
                player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Shoes"))] = new ItemSlot(new Item(CharacterCreationManager.singleton.maleShoes), 1);
            }                                                                               
            else                                                                            
            {                                                                               
                player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Shirt"))] = new ItemSlot(new Item(CharacterCreationManager.singleton.femaleUp), 1);
                player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Pant"))] = new ItemSlot(new Item(CharacterCreationManager.singleton.femaleDown), 1);
                player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Shoes"))] = new ItemSlot(new Item(CharacterCreationManager.singleton.femaleShoes), 1);
            }
            player.playerCharacterCreation.alreadyAssigned = true;
        }

        connection.InsertOrReplace(new characterCreation
        {
            characterName = player.name,
            sex = player.playerCharacterCreation.sex,
            hairType = player.playerCharacterCreation.hairType,
            beard = player.playerCharacterCreation.beard,
            hairColor = player.playerCharacterCreation.hairColor,
            underwearColor = player.playerCharacterCreation.underwearColor,
            eyesColor = player.playerCharacterCreation.eyesColor,
            skinColor = player.playerCharacterCreation.skinColor,
            fat = player.playerCharacterCreation.fat,
            thin = player.playerCharacterCreation.thin,
            height = player.playerCharacterCreation.height,
            breast = player.playerCharacterCreation.breast,
            muscle = player.playerCharacterCreation.muscle,
            hats = player.playerCharacterCreation.hats,
            accessory = player.playerCharacterCreation.accessory,
            upper = player.playerCharacterCreation.upper,
            down = player.playerCharacterCreation.down,
            shoes = player.playerCharacterCreation.shoes,
            bag = player.playerCharacterCreation.bag,
            alreadyAssigned = Convert.ToInt32(player.playerCharacterCreation.alreadyAssigned)
        });
    }

    public void LoadCharacterCreation(Player player)
    {
        player.playerCharacterCreation = player.GetComponent<PlayerCharacterCreation>();

        foreach (characterCreation row in connection.Query<characterCreation>("SELECT * FROM characterCreation WHERE characterName=?", player.name))
        {
            player.playerCharacterCreation.sex = row.sex;
            player.playerCharacterCreation.hairType = row.hairType;
            player.playerCharacterCreation.beard = row.beard;
            player.playerCharacterCreation.hairColor = row.hairColor;
            player.playerCharacterCreation.underwearColor = row.underwearColor;
            player.playerCharacterCreation.eyesColor = row.eyesColor;
            player.playerCharacterCreation.skinColor = row.skinColor;
            player.playerCharacterCreation.fat = row.fat;
            player.playerCharacterCreation.thin = row.thin;
            player.playerCharacterCreation.muscle = row.muscle;
            player.playerCharacterCreation.height = row.height;
            player.playerCharacterCreation.breast = row.breast;
            player.playerCharacterCreation.hats = row.hats;
            player.playerCharacterCreation.accessory = row.accessory;
            player.playerCharacterCreation.upper = row.upper;
            player.playerCharacterCreation.down = row.down;
            player.playerCharacterCreation.shoes = row.shoes;
            player.playerCharacterCreation.bag = row.bag;
            player.playerCharacterCreation.alreadyAssigned = Convert.ToBoolean(row.alreadyAssigned);
        }
    }

    public CharactersAvailableMsg LoadCharacterCreationForPreview(string playerName, CharactersAvailableMsg msg)
    {
        List<CharactersAvailableMsg.CharacterPreview> chL = new List<CharactersAvailableMsg.CharacterPreview>();
        foreach (characterCreation row in connection.Query<characterCreation>("SELECT * FROM characterCreation WHERE characterName=?", playerName))
        {
            CharactersAvailableMsg.CharacterPreview ch = new CharactersAvailableMsg.CharacterPreview();
            ch.name = row.characterName;
            ch.className = "Warrior";
            ch.gender = (sbyte)row.sex;
            ch.hairIndex = (sbyte)row.hairType;
            ch.bearIndex = (sbyte)row.beard;
            ch.hairColor = row.hairColor;
            ch.underwearColor = row.underwearColor;
            ch.eyesColor = row.eyesColor;
            ch.skinColor = row.skinColor;
            ch.fatValue = row.fat;
            ch.thinValue = row.thin;
            ch.heightValue = row.height;
            ch.heightValue = row.height;
            ch.muscleValue = row.muscle;
            ch.muscleValue = row.muscle;
            ch.breastValue = row.breast;
            ch.hats = (sbyte)row.hats;
            ch.accessory = (sbyte)row.accessory;
            ch.upper = (sbyte)row.upper;
            ch.down = (sbyte)row.down;
            ch.shoes = (sbyte)row.shoes;
            ch.bag = (sbyte)row.bag;
            Array.Resize(ref msg.characters, msg.characters.Length + 1);
            msg.characters[msg.characters.Length - 1] = ch;
        }
        return msg;
    }
}

public class PlayerCharacterCreation : NetworkBehaviour
{
    private Player player;

    [SyncVar]
    public int sex;
    [SyncVar(hook = nameof(ChangeWeigth))]
    public float fat;
    [SyncVar(hook = nameof(ChangeThin))]
    public float thin;
    [SyncVar(hook = nameof(ChangeMuscle))]
    public float muscle;
    [SyncVar(hook = nameof(ChangeHeight))]
    public float height;
    [SyncVar(hook = nameof(ChangeBreastSize))]
    public float breast;

    [SyncVar(hook = nameof(ChangeHairType))]
    public int hairType;
    [SyncVar(hook = nameof(ChangeBeardType))]
    public int beard;
    [SyncVar(hook = nameof(ChangeHairColor))]
    public string hairColor;
    [SyncVar(hook = nameof(ChangeUnderpantsColor))]
    public string underwearColor;
    [SyncVar(hook = nameof(ChangeEyesColor))]
    public string eyesColor;
    [SyncVar(hook = nameof(ChangeSkinColor))]
    public string skinColor;


    [SyncVar(hook = nameof(ChangeHats))]
    public int hats;
    [SyncVar(hook = nameof(ChangeAccessory))]
    public int accessory;
    [SyncVar(hook = nameof(ChangeUpper))]
    public int upper;
    [SyncVar(hook = nameof(ChangeDown))]
    public int down;
    [SyncVar(hook = nameof(ChangeShoes))]
    public int shoes;
    [SyncVar(hook = nameof(ChangeBag))]
    public int bag;


    public ItemSlot accessorySlot;
    public ItemSlot hatsSlot;

    public CharacterCustomization characterCustomization;
    public JoystickManager joystickManager;

    public CharactersAvailableMsg.CharacterPreview message;

    public PlayerChildObject playerChildObject;
    Color newCol;
    public bool alreadyAssigned;

    public void Awake()
    {
        Assign();
    }

    public void SetProperty()
    {
        if (sex != -1)
        {
            NewSetup();
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        TemperatureManager.singleton.ChangeLightColor(TemperatureManager.singleton.colorSync, TemperatureManager.singleton.colorSync);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Assign();
        SetProperty();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Assign();
        SetProperty();
        characterCustomization.transform.parent.transform.parent = null;
        characterCustomization.transform.parent.transform.position = new Vector3(5 * player.GetComponent<PlayerMove>().position, 1000.0f, 1000.0f);
        GetComponent<PlayerWeaponIK>().Spawn();
        GetComponent<PlayerWeaponIK>().SpawnFeet();
        for (int i = 0; i < player.playerEquipment.slots.Count; ++i)
            player.playerEquipment.RefreshLocation(i);

    }

    public void Assign()
    {
        player = GetComponent<Player>();
        player.playerCharacterCreation = this;

        joystickManager = FindObjectOfType<JoystickManager>();

        player.playerEquipment = player.GetComponent<PlayerEquipment>();
        player.playerEquipment.avatarCamera.cullingMask = player.isLocalPlayer ? joystickManager.personalPlayer : joystickManager.notPersonalPlayer;
    }

    public void SetGender(int oldSex, int newSex)
    {
        player = GetComponent<Player>();

        if (!characterCustomization) return;
            characterCustomization.SwitchCharacterSettings(newSex);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeHairType(int oldHair, int newHair)
    {
        player = GetComponent<Player>();

        if (!characterCustomization) return;
        characterCustomization.SetElementByIndex(CharacterElementType.Hair, newHair);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeHairColor(string oldHairColor, string newHairColor)
    {
        player = GetComponent<Player>();
        if (!characterCustomization) return;
        if (ColorUtility.TryParseHtmlString("#" + newHairColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeUnderpantsColor(string oldUnderpantsColor, string newUnderpantsColor)
    {
        player = GetComponent<Player>();
        if (!characterCustomization) return;
        if (ColorUtility.TryParseHtmlString("#" + newUnderpantsColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeEyesColor(string oldEyesColor, string newEyesColor)
    {
        player = GetComponent<Player>();
        if (!characterCustomization) return;
        if (ColorUtility.TryParseHtmlString("#" + newEyesColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeSkinColor(string oldSkinColor, string newSkinColor)
    {
        player = GetComponent<Player>();
        if (!characterCustomization) return;
        if (ColorUtility.TryParseHtmlString("#" + newSkinColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeBeardType(int oldHair, int newHair)
    {
        player = GetComponent<Player>();
        if (!characterCustomization) return;
        characterCustomization.SetElementByIndex(CharacterElementType.Beard, newHair);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeWeigth(float oldWeigth, float newWeigth)
    {
        player = GetComponent<Player>();
        if (!characterCustomization) return;
        characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, newWeigth);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        if (MenuButton.singleton) MenuButton.singleton.RefreshFatText();
    }

    public void ChangeThin(float oldThin, float newThin)
    {
        if (sex == 0)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, newThin);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeMuscle(float oldMuscle, float newMuscle)
    {
        if (sex == 0)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, newMuscle);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeHeight(float oldHeight, float newHeight)
    {

        player = GetComponent<Player>();
        if (!characterCustomization) return;
        characterCustomization.SetHeight(newHeight);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
    }

    public void ChangeBreastSize(float oldBrestSize, float newBrestSize)
    {
        if (sex == 1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, newBrestSize);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeHats(int oldHats, int newHats)
    {
        if (newHats != -1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Hat, newHats);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
        else
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Hat, -1);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeAccessory(int oldAccessory, int newAccessory)
    {
        if (newAccessory != -1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Accessory, newAccessory);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
        else
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Accessory, -1);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeUpper(int oldUpper, int newUpper)
    {
        if (newUpper != -1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Shirt, newUpper);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
        else
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Shirt, -1);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeDown(int oldDown, int newDown)
    {
        if (newDown != -1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Pants, newDown);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
        else
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Pants, -1);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeShoes(int oldShoes, int newShoes)
    {
        if (newShoes != -1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Shoes, newShoes);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
        else
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Shoes, -1);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void ChangeBag(int oldBag, int newBag)
    {
        if (newBag != -1)
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Item1, newBag);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
        else
        {
            player = GetComponent<Player>();
            if (!characterCustomization) return;
            characterCustomization.SetElementByIndex(CharacterElementType.Item1, -1);
            characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);
        }
    }

    public void NewSetup()
    {
        player = GetComponent<Player>();
        SetGender(sex, sex);
        ChangeHairColor(hairColor, hairColor);
        ChangeHairType(hairType, hairType);
        ChangeUnderpantsColor(underwearColor, underwearColor);
        ChangeEyesColor(eyesColor, eyesColor);
        ChangeSkinColor(skinColor, skinColor);
        ChangeBeardType(beard, beard);
        ChangeWeigth(fat, fat);
        ChangeThin(thin, thin);
        ChangeMuscle(muscle, muscle);
        ChangeHeight(height, height);
        ChangeBreastSize(breast, breast);
        ChangeHats(hats, hats);
        ChangeAccessory(accessory, accessory);
        ChangeUpper(upper, upper);
        ChangeDown(down, down);
        ChangeShoes(shoes, shoes);
        ChangeBag(bag, bag);
        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);

    }

    public void DressSelectablePlayer(Player player, CharacterCustomization characterCustomization)
    {
        if (ColorUtility.TryParseHtmlString("#" + player.playerCharacterCreation.underwearColor, out newCol))
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
        characterCustomization.SetElementByIndex(CharacterElementType.Hat, player.playerCharacterCreation.hats);
        characterCustomization.SetElementByIndex(CharacterElementType.Accessory, player.playerCharacterCreation.accessory);
        characterCustomization.SetElementByIndex(CharacterElementType.Shirt, player.playerCharacterCreation.upper);
        characterCustomization.SetElementByIndex(CharacterElementType.Pants, player.playerCharacterCreation.down);
        characterCustomization.SetElementByIndex(CharacterElementType.Shoes, player.playerCharacterCreation.shoes);
        characterCustomization.SetElementByIndex(CharacterElementType.Item1, player.playerCharacterCreation.bag);
    }

    public void SetupPreview(CharactersAvailableMsg.CharacterPreview message)
    {
        player = GetComponent<Player>();

        characterCustomization.SwitchCharacterSettings(message.gender);

        if (ColorUtility.TryParseHtmlString("#" + message.hairColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Hair, newCol);
        }

        characterCustomization.SetElementByIndex(CharacterElementType.Hair, message.hairIndex);

        if (ColorUtility.TryParseHtmlString("#" + message.underwearColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Underpants, newCol);
        }
        if (ColorUtility.TryParseHtmlString("#" + message.eyesColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Eye, newCol);
        }
        if (ColorUtility.TryParseHtmlString("#" + message.skinColor, out newCol))
        {
            characterCustomization.SetBodyColor(BodyColorPart.Skin, newCol);
        }
        if (sex == 0)
        {
            characterCustomization.SetElementByIndex(CharacterElementType.Beard, message.bearIndex);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, message.fatValue);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Thin, message.thinValue);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Muscles, message.muscleValue);
        }

        characterCustomization.SetHeight(height);

        if (sex == 1)
        {
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.Fat, message.fatValue);
            characterCustomization.SetBlendshapeValue(CharacterBlendShapeType.BreastSize, message.breastValue);
        }

        characterCustomization.SetElementByIndex(CharacterElementType.Shirt, message.upper);
        characterCustomization.SetElementByIndex(CharacterElementType.Pants, message.down);
        characterCustomization.SetElementByIndex(CharacterElementType.Shoes, message.shoes);

        characterCustomization.gameObject.GetComponent<PlayerChildObject>().Manage(player.isLocalPlayer, player);

    }

    public void CheckAccessory()
    {
        if (isServer)
        {
            if (player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Accessory")) != -1)
            {
                accessorySlot = player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Accessory"))];
            }
            else
            {
                accessorySlot.amount = 0;
                accessory = -1;
            }
        }
    }

    public void CheckHat()
    {
        if (isServer)
        {
            if (player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Head")) != -1)
            {
                hatsSlot = player.equipment.slots[player.equipment.slots.FindIndex(slot => slot.amount > 0 && ((EquipmentItem)slot.item.data).category.StartsWith("Head"))];
            }
            else
            {
                hatsSlot.amount = 0;
                hats = -1;
            }
        }
    }

}
