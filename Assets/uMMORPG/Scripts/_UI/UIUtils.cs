using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIUtils
{
    // instantiate/remove enough prefabs to match amount
    public static void BalancePrefabs(GameObject prefab, int amount, Transform parent)
    {
        // instantiate until amount
        for (int i = parent.childCount; i < amount; ++i)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
        }

        // delete everything that's too much
        // (backwards loop because Destroy changes childCount)
        for (int i = parent.childCount-1; i >= amount; --i)
            GameObject.Destroy(parent.GetChild(i).gameObject);
    }

    public static void BalancePrefabsNoDelete(GameObject prefab, int amount, Transform parent)
    {
        // instantiate until amount
        for (int i = 0; i < amount; i++)
        {
            GameObject go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
        }
    }

    // find out if any input is currently active by using Selectable.all
    // (FindObjectsOfType<InputField>() is far too slow for huge scenes)
    public static bool AnyInputActive()
    {
        // avoid Linq.Any because it is HEAVY(!) on GC and performance
        foreach (Selectable sel in Selectable.allSelectablesArray)
            if (sel is InputField inputField && inputField.isFocused)
                return true;
        return false;
    }

    // deselect any UI element carefully
    // (it throws an error when doing it while clicking somewhere, so we have to
    //  double check)
    public static void DeselectCarefully()
    {
        if (!Input.GetMouseButton(0) &&
            !Input.GetMouseButton(1) &&
            !Input.GetMouseButton(2))
            EventSystem.current.SetSelectedGameObject(null);
    }

    public static ScriptableBuildingAccessory FindWhereTheItemIsCrafted(ScriptableItem item)
    {
        for(int i = 0; i < ModularBuildingManager.singleton.allScriptableAccessory.Count; i++)
        {
            for (int e = 0; e < ModularBuildingManager.singleton.allScriptableAccessory[i].itemtoCraft.Count; e++)
            {
                if (ModularBuildingManager.singleton.allScriptableAccessory[i].itemtoCraft[e].itemAndAmount.item.name == item.name)
                {
                    return ModularBuildingManager.singleton.allScriptableAccessory[i];
                }
            }
        }
        return null;
    }

    public static ItemCrafting FindTheItemIsCrafted(ScriptableItem item)
    {
        for (int i = 0; i < ModularBuildingManager.singleton.allScriptableAccessory.Count; i++)
        {
            for (int e = 0; e < ModularBuildingManager.singleton.allScriptableAccessory[i].itemtoCraft.Count; e++)
            {
                if (ModularBuildingManager.singleton.allScriptableAccessory[i].itemtoCraft[e].itemAndAmount.item.name == item.name)
                {
                    return ModularBuildingManager.singleton.allScriptableAccessory[i].itemtoCraft[e];
                }
            }
        }
        return new ItemCrafting();
    }

}
