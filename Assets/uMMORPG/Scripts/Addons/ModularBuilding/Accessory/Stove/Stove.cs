using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Stove : CraftAccessory
{
    public List<GameObject> objectToManageActivation = new List<GameObject>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        craftingItem.Callback += OnItemChanged;
        CheckItems();
    }

    public void OnItemChanged(SyncList<CraftinItemSlot>.Operation op, int index, CraftinItemSlot oldSlot, CraftinItemSlot newSlot)
    {
        base.OnBeltChanged(op, index, oldSlot, newSlot);
        CheckItems();
    }

    public void CheckItems()
    {
        for (int i = 0; i < objectToManageActivation.Count; i++)
        {
            objectToManageActivation[i].SetActive(false);
        }

        int objectsToActivate = Mathf.Min(craftingItem.Count / 2, objectToManageActivation.Count);

        for (int i = 0; i < objectsToActivate; i++)
        {
            objectToManageActivation[i].SetActive(true);
        }
    }
}
