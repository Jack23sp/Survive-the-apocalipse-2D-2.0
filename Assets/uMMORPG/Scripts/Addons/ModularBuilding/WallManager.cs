using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WallManager : MonoBehaviour
{
    public bool up, down, left, right;
    public NetworkIdentity identity;
    public ModularBuilding modularBuilding;

    public BoxCollider2D collider;

    public bool wall;

    public Collider2D[] colliderHits = new Collider2D[0];

    public ModularDoor modularDoor;
    public SpriteRenderer doorSprite;

    public List<NavMeshObstacle2DCustom> navMeshObstacle2Ds = new List<NavMeshObstacle2DCustom>();
    public NavMeshObstacle2DCustom doorObstacle;

    public SpriteRenderer openStatus;

    public WallOptions wallOptions;

    public SpriteRenderer cracksImage;

    private ModularBuilding cacheModularBuilding;

    public void OnEnable()
    {
         CheckWall();
         GetComponent<DamagableObject>().wall = this;       
    }

    private void OnDisable()
    {
         CheckWallOnExit();
    }

    public bool CheckIfIsOpen()
    {
        if (down)
        {
            return modularBuilding.downDoorOpen;
        }
        else if (up)
        {
            return modularBuilding.upDoorOpen;
        }
        else if (left)
        {
            return modularBuilding.leftDoorOpen;
        }
        else if (right)
            return modularBuilding.rightDoorOpen;
        else
            return false;
    }

    public void ManageVisibility()
    {
        if(!Player.localPlayer)
        {
            if(openStatus) openStatus.enabled = !wall ? true : false;
            wallOptions.gameObject.SetActive(false);
            if (up && modularBuilding.serverUpBasementDecoration == 1)
                ManageOpening(modularBuilding.upDoorOpen, false);
            else if (left && modularBuilding.serverLeftBasementDecoration == 1)
                ManageOpening(modularBuilding.leftDoorOpen, false);
            else if (down && modularBuilding.serverDownBasementDecoration == 1)
                ManageOpening(modularBuilding.downDoorOpen, false);
            else if (right && modularBuilding.serverRightBasementDecoration == 1)
                ManageOpening(modularBuilding.rightDoorOpen, false);

            return;
        }
        if (!ModularBuildingManager.singleton.CanDoOtherActionFloor(modularBuilding, Player.localPlayer))
        {
            if (openStatus) openStatus.enabled = !wall ? true : false;
            wallOptions.gameObject.SetActive(false);

            if(AbilityManager.singleton.FindNetworkAbilityLevel(("Thief"), Player.localPlayer.name) >= modularBuilding.level)
            {
                if (up && modularBuilding.serverUpBasementDecoration == 1)
                    ManageOpening(modularBuilding.upDoorOpen, false);
                else if (left && modularBuilding.serverLeftBasementDecoration == 1)
                    ManageOpening(modularBuilding.leftDoorOpen, false);
                else if (down && modularBuilding.serverDownBasementDecoration == 1)
                    ManageOpening(modularBuilding.downDoorOpen, false);
                else if (right && modularBuilding.serverRightBasementDecoration == 1)
                    ManageOpening(modularBuilding.rightDoorOpen, false);
                return;
            }
            if (up && modularBuilding.serverUpBasementDecoration == 1)
                ManageOpening(modularBuilding.upDoorOpen, false);
            else if (left && modularBuilding.serverLeftBasementDecoration == 1)
                ManageOpening(modularBuilding.leftDoorOpen, false);
            else if (down && modularBuilding.serverDownBasementDecoration == 1)
                ManageOpening(modularBuilding.downDoorOpen, false);
            else if (right && modularBuilding.serverRightBasementDecoration == 1)
                ManageOpening(modularBuilding.rightDoorOpen, false);
        }
        else
        {
            if (openStatus) openStatus.enabled = !wall ? true : false;
            wallOptions.gameObject.SetActive(true);
            if (up && modularBuilding.serverUpBasementDecoration == 1)
            {
                if(modularBuilding.upDoor.GetComponent<WallManager>().openStatus) modularBuilding.upDoor.GetComponent<WallManager>().openStatus.color = modularBuilding.upDoorOpen == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
                ManageOpening(modularBuilding.upDoorOpen, false);
            }
            else if (left && modularBuilding.serverLeftBasementDecoration == 1)
            {
                if (modularBuilding.leftDoor.GetComponent<WallManager>().openStatus) modularBuilding.leftDoor.GetComponent<WallManager>().openStatus.color = modularBuilding.leftDoorOpen == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
                ManageOpening(modularBuilding.leftDoorOpen, false);
            }
            else if (down && modularBuilding.serverDownBasementDecoration == 1)
            {
                if (modularBuilding.downDoor.GetComponent<WallManager>().openStatus) modularBuilding.downDoor.GetComponent<WallManager>().openStatus.color = modularBuilding.downDoorOpen == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
                ManageOpening(modularBuilding.downDoorOpen, false);
            }
            else if (right && modularBuilding.serverRightBasementDecoration == 1)
            {
                if (modularBuilding.rightDoor.GetComponent<WallManager>().openStatus) modularBuilding.rightDoor.GetComponent<WallManager>().openStatus.color = modularBuilding.rightDoorOpen == false ? ModularBuildingManager.singleton.closeColor : ModularBuildingManager.singleton.openColor;
                ManageOpening(modularBuilding.rightDoorOpen, false);
            }
        }
    }

    public void CheckWall()
    {
        colliderHits = Physics2D.OverlapBoxAll(transform.position, new Vector2(collider.size.x, collider.size.y), transform.localEulerAngles.z, ModularBuildingManager.singleton.basementLayerMask);

        for (int i = 0; i < colliderHits.Length; i++)
        {
            int index = i;
            if (colliderHits[index] == modularBuilding.thisCollider)
            {
                cacheModularBuilding = null;
                cacheModularBuilding = colliderHits[index].GetComponent<ModularBuilding>();
                if (!cacheModularBuilding) continue;

                if (up)
                {
                    cacheModularBuilding.upBasementDecoration = wall == true ? 0 : 1;
                }
                if (left)
                {
                    cacheModularBuilding.leftBasementDecoration = wall == true ? 0 : 1;
                }
                if (down)
                {
                    cacheModularBuilding.downBasementDecoration = wall == true ? 0 : 1;
                }
                if (right)
                {
                    cacheModularBuilding.rightBasementDecoration = wall == true ? 0 : 1;
                }
            }
            else
            {
                cacheModularBuilding = null;
                cacheModularBuilding = colliderHits[index].GetComponent<ModularBuilding>();
                if (!cacheModularBuilding) continue;

                if (up)
                {
                    cacheModularBuilding.downBasementDecoration = wall == true ? 0 : 1;
                }
                if (left)
                {
                    cacheModularBuilding.rightBasementDecoration = wall == true ? 0 : 1;
                }
                if (down)
                {
                    cacheModularBuilding.upBasementDecoration = wall == true ? 0 : 1;
                }
                if (right)
                {
                    cacheModularBuilding.leftBasementDecoration = wall == true ? 0 : 1;
                }
            }
        }
    }

    public void CheckWallOnExit()
    {
        colliderHits = Physics2D.OverlapBoxAll(transform.position, new Vector2(collider.size.x, collider.size.y), transform.localEulerAngles.z, ModularBuildingManager.singleton.basementLayerMask);
        for (int i = 0; i < colliderHits.Length; i++)
        {
            int index = i;
            if (colliderHits[index] == modularBuilding.thisCollider)
            {
                cacheModularBuilding = null;
                cacheModularBuilding = colliderHits[index].GetComponent<ModularBuilding>();
                if (!cacheModularBuilding) continue;
                if (up)
                {
                    cacheModularBuilding.upBasementDecoration = -1;
                }
                if (left)
                {
                    cacheModularBuilding.leftBasementDecoration = -1;
                }
                if (down)
                {
                    cacheModularBuilding.downBasementDecoration = -1;
                }
                if (right)
                {
                    cacheModularBuilding.rightBasementDecoration = -1;
                }
            }
            else
            {
                cacheModularBuilding = null;
                cacheModularBuilding = colliderHits[index].GetComponent<ModularBuilding>();
                if (!cacheModularBuilding) continue;
                if (up)
                {
                    cacheModularBuilding.downBasementDecoration = -1;
                }
                if (left)
                {
                    cacheModularBuilding.rightBasementDecoration = -1;
                }
                if (down)
                {
                    cacheModularBuilding.upBasementDecoration = -1;
                }
                if (right)
                {
                    cacheModularBuilding.leftBasementDecoration = -1;
                }
            }
        }
    }

    public void ManageOpening(bool status, bool sounds)
    {
        if (!status)
        {
            if (up || down)
            {
                if(doorSprite) doorSprite.enabled = true;
            }
            else
            {
                if (doorSprite) doorSprite.enabled = false;
            }
            if (sounds) 
                ModularBuildingManager.singleton.ManageSound(status);
        }
        else
        {
            if (up || down)
            {
                if (doorSprite) doorSprite.enabled = false;
            }
            else
            {
                if (doorSprite) doorSprite.enabled = true;
            }
            if (sounds) 
                ModularBuildingManager.singleton.ManageSound(status);
        }
        if (doorObstacle) doorObstacle.enabled = !status;
    }
}
