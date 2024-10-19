using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class Level : NetworkBehaviour
{
    [SyncVar(hook =(nameof(CheckLevel)))] public int current = 1;
    public int max = 1;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        UIPlayerInformation.singleton.Open();
    }

    public void CheckLevel(int oldLevel, int newLevel)
    {
        UIPlayerInformation.singleton.Open();
    }

    void OnValidate()
    {
        current = Mathf.Clamp(current, 1, max);
    }
}