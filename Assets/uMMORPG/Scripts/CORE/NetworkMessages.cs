// Contains all the network messages that we need.
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

// client to server ////////////////////////////////////////////////////////////
public partial struct LoginMsg : NetworkMessage
{
    public string account;
    public string password;
    public string version;
}

public partial struct CharacterCreateMsg : NetworkMessage
{
    public string name;
    public int classIndex;

    public int hairIndex;
    public string hairColor;
    public int bearIndex;
    public string skinColor;
    public string eyesColor;
    public string underwearColor;
    public float fatValue;
    public float thinValue;
    public float heightValue;
    public float muscleValue;
    public float breastValue;
    public int gender;
    public int hats;
    public int accessory;
    public int upper;
    public int down;
    public int shoes;
    public int bag;
}

public partial struct CharacterSelectMsg : NetworkMessage
{
    public int index;
}

public partial struct CharacterDeleteMsg : NetworkMessage
{
    public int index;
}

// server to client ////////////////////////////////////////////////////////////
// we need an error msg packet because we can't use TargetRpc with the Network-
// Manager, since it's not a MonoBehaviour.
public partial struct ErrorMsg : NetworkMessage
{
    public string text;
    public bool causesDisconnect;
}

public partial struct LoginSuccessMsg : NetworkMessage
{
}

public partial struct CharactersAvailableMsg : NetworkMessage
{
    public partial struct CharacterPreview
    {
        public string name;
        public string className; // = the prefab name
        public sbyte hairIndex;
        public string hairColor;
        public sbyte bearIndex;
        public string skinColor;
        public string eyesColor;
        public string underwearColor;
        public float fatValue;
        public float thinValue;
        public float heightValue;
        public float muscleValue;
        public float breastValue;
        public sbyte gender;
        public sbyte hats;
        public sbyte accessory;
        public sbyte upper;
        public sbyte down;
        public sbyte shoes;
        public sbyte bag;
    }
    public CharacterPreview[] characters;
}