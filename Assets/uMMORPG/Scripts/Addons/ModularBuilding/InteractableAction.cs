using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

public enum AnimationType
{
    Sit = 1,
    MoveTo = 2,
    Lay = 3
}

public struct ActionPlayerSlot
{
    public NetworkIdentity player;
    public int slot;
}

public class InteractableAction : MonoBehaviour
{
    public int index;
    public AnimationType animationType;
    public List<AnimationType> additionalActionToDo = new List<AnimationType>();
    public BuildingAccessory mainAccessory;
    public Vector2 direction;
    public Transform moveToTransform;
    public float overrideSortByDepth;
    public SpriteRenderer spriteToManage;
}
