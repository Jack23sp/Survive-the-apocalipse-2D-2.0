using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickManager : MonoBehaviour
{
    public static JoystickManager singleton;
    public VariableJoystick attackJoystick;
    public VariableJoystick moveJoystick;
    public LayerMask notPersonalPlayer;
    public LayerMask personalPlayer;
    public LayerMask meleeDetector;
    public LayerMask monster;

    void Start()
    {
        if(!singleton) singleton = this;
    }

    public void SetAttack(bool condition)
    {
        if(Player.localPlayer.playerMove.canAttack != condition)
        {
            Player.localPlayer.playerMove.CmdSetCanAttack(condition);
        }
    }
}
