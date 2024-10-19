using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Player
{
    [Command]
    public void CmdSetFlag(int flagIndex, NetworkIdentity identity)
    {
        Flag flag = identity.gameObject.GetComponent<Flag>();
        if (ModularBuildingManager.singleton.CanDoOtherActionForniture(flag, this))
        {
            flag.flag = flagIndex;
        }
    }
}

public partial class Database
{
    class flag
    {
        public int ind { get; set; }
        public int flagIndex { get; set; }
    }

    public void SaveFlag(int index)
    {
        connection.InsertOrReplace(new flag
        {
            ind = index,
            flagIndex = ((Flag)ModularBuildingManager.singleton.buildingAccessories[index]).flag
        });
    }

    public void LoadFlag(int index, Flag flag)
    {
        foreach (flag row in connection.Query<flag>("SELECT * FROM flag WHERE ind=?", index))
        {
            flag.flag = row.flagIndex;
        }
    }
}

public class Flag : BuildingAccessory
{
    [SyncVar(hook = (nameof(ManageFlag)))] public int flag;
    public SpriteRenderer flagTexture;

    public new void OnStartClient()
    {
        base.OnStartClient();
        flagTexture.sprite = FlagManager.singleton.flags[flag];
    }

    public new void OnStartServer()
    {
        base.OnStartServer();
        flagTexture.sprite = FlagManager.singleton.flags[flag];
    }

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.flags.Contains(this)) ModularBuildingManager.singleton.flags.Add(this);
        }
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.flags.Contains(this)) ModularBuildingManager.singleton.flags.Remove(this);
        }
    }

    public void ManageFlag(int oldFlag, int newFlag)
    {
        flagTexture.sprite = FlagManager.singleton.flags[newFlag];
        if (UIFlag.singleton.panel.activeInHierarchy)
        {
            UIFlag.singleton.setButtonText.text = "Setted!";
        }
    }

}
