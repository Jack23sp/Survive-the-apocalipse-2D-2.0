using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public partial class Player
{
    [Command]
    public void CmdSetMessage(string messageText, NetworkIdentity identity, NetworkIdentity acc)
    {
        Player pl = identity.GetComponent<Player>();
        Billboard billboard = acc.GetComponent<Billboard>();

        if (ModularBuildingManager.singleton.CanDoOtherActionForniture(billboard, pl))
            billboard.message = messageText;
    }

}

public partial class Database
{
    class billboard
    {
        public int ind { get; set; }
        public string message { get; set; }
    }

    public void SaveBillboard(int ind)
    {
        connection.InsertOrReplace(new billboard
        {
            ind = ind,
            message = ((Billboard)ModularBuildingManager.singleton.buildingAccessories[ind]).message
        });
    }

    public void LoadBillboard(int ind, Billboard billboard)
    {
        foreach (billboard row in connection.Query<billboard>("SELECT * FROM billboard WHERE ind=?", ind))
        {
            billboard.message = row.message;
        }
    }
}

public class Billboard : BuildingAccessory
{
    public TextMeshProUGUI messageText;
    [SyncVar(hook = (nameof(SetMessage)))] public string message;

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.billboards.Contains(this)) ModularBuildingManager.singleton.billboards.Add(this);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetMessage(message, message);
    }

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (isServer || isClient)
        {
            if (ModularBuildingManager.singleton.billboards.Contains(this)) ModularBuildingManager.singleton.billboards.Remove(this);
        }
    }

    public void SetMessage(string oldValue, string newValue)
    {
        messageText.text = newValue;
        UIBillboard.singleton.setButton.GetComponentInChildren<TextMeshProUGUI>().text = "Setted!";
    }

}
