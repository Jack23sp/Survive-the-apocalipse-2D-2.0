using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public interface IUIScript
{
    void Close();

    void Assign();

    void RemovePlayerFromBuildingAccessory(NetworkIdentity identity);

}


public interface IUIScriptNoBuildingRelated
{
    void Close();

    void Assign();

}