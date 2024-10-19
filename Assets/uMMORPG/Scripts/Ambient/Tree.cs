using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public partial class Database
{
    class tree
    {
        public int ind { get; set; }
        public int rewardAmount { get; set; }
    }

    public void SaveTree(int ind)
    {
        connection.InsertOrReplace(new tree
        {
            ind = ind,
            rewardAmount = ((Tree)ModularBuildingManager.singleton.buildingAccessories[ind]).rewardAmount
        });
    }

    public void LoadTree(int ind, Tree tree)
    {
        foreach (tree row in connection.Query<tree>("SELECT * FROM aquarium WHERE ind=?", ind))
        {
            tree.rewardAmount = row.rewardAmount;
        }
    }
}


public class Tree : BuildingAccessory
{
    public ItemDropChance dropChance;
    //public SpriteRenderer snowLayer;
    public SpriteRenderer cracksImage;

    public List<SpriteRenderer> treeSprites;

    public DamagableObject damagableObject;

    public ScriptableItem reward;
    [SyncVar (hook = nameof(ManageAmount))]
    public int rewardAmount;
    public ScriptableItem tree;

    public new void Start()
    {
        base.Start();
        damagableObject.tree = this;
        Invoke(nameof(IncreaseReward), 10800.0f);

        if (isServer || isClient)
        {
            if (navMeshObstacle2D) navMeshObstacle2D.enabled = true;
            if (!ModularBuildingManager.singleton.trees.Contains(this) && (owner != string.Empty || group != string.Empty)) ModularBuildingManager.singleton.trees.Add(this);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        ManageAmount(rewardAmount, rewardAmount);
    }

    public void IncreaseReward()
    {
        rewardAmount = 5;
    }

    public void ManageAmount (int oldvalue,int newValue)
    {
        renderer.material = (owner != string.Empty || group != string.Empty)  && rewardAmount > 0 ? ModularBuildingManager.singleton.objectPresent : ModularBuildingManager.singleton.objectNotPresent;
    }

    public void ManageVisibility(bool condition)
    {
        for(int i = 0; i < treeSprites.Count; i++)
        {
            treeSprites[i].enabled = condition;
        }
    }
}
