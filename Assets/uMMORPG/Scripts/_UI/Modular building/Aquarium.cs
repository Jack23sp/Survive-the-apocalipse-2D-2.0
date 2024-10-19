using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class Database
{
    class aquarium
    {
        public int ind { get; set; }
        public float dirt { get; set; }
    }

    public void SaveAquarium(int ind)
    {
        connection.InsertOrReplace(new aquarium
        {
            ind = ind,
            dirt = Convert.ToSingle(((Aquarium)ModularBuildingManager.singleton.aquarium[ind]).dirt.ToString("F2"))
        });
    }

    public void LoadAquarium(int ind, Aquarium aquarium)
    {
        foreach (aquarium row in connection.Query<aquarium>("SELECT * FROM aquarium WHERE ind=?", ind))
        {
            aquarium.dirt = row.dirt;
        }
    }
}

public partial class Player
{
    [Command]
    public void CmdCleanAquarium(NetworkIdentity identity)
    {
        Aquarium aquarium = identity.GetComponent<Aquarium>();

        if (ModularBuildingManager.singleton.CanDoOtherActionForniture(aquarium, this))
            aquarium.dirt = 0;
    }

    [Command]
    public void CmdLevelUp(NetworkIdentity identity)
    {
        Fence fence = identity.GetComponent<Fence>();
        Gate gate = identity.GetComponent<Gate>();

        if (!gate && !fence) return;
        if(fence)
        {
            fence.LevelDoAction(this);
        }
        if(gate)
        {
            gate.LevelDoAction(this);
        }
    }

    [Command]
    public void CmdlClaim(NetworkIdentity identity)
    {
        Fence fence = identity.GetComponent<Fence>();
        Gate gate = identity.GetComponent<Gate>();

        if (!gate && !fence) return;
        if (fence)
        {
            fence.ClaimDoAction(this);
        }
        if (gate)
        {
            gate.ClaimDoAction(this);
        }
    }
}

public class Aquarium : BuildingAccessory
{
    [SyncVar(hook = (nameof(ManageDirt)))] public float dirt;
    public float maxDirty = 0.35f;
    public SpriteRenderer dirtSprite;
    private Color col;

    public new void Start()
    {
        base.Start();
        if (isServer || isClient)
        {
            if (!ModularBuildingManager.singleton.aquarium.Contains(this)) ModularBuildingManager.singleton.aquarium.Add(this);
            ManageDirt(dirt,dirt);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Invoke(nameof(CheckDirt), 180.0f);
    }

    public void CheckDirt()
    {
        if (dirt < maxDirty)
        {
            dirt += 0.01f;
        }
        Invoke(nameof(CheckDirt), 180.0f);
    }  

    public void ManageDirt(float oldValue, float newValue)
    {
        col = dirtSprite.color;
        col.a = newValue;
        dirtSprite.color = col;
    }
}
