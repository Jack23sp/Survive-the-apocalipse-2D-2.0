using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player
{
    [HideInInspector] public PlayerTutorial playerTutorial;
}

public class PlayerTutorial : MonoBehaviour
{
    public bool openTutorial;
    private Player player;
    private int numberOfTry = 30;

    void Start()
    {
        player = GetComponent<Player>();
        player.playerTutorial = this;
        Invoke(nameof(Open), 0.1f);
    }

    void Open()
    {
        if(!openTutorial)
        {
            if(player.netIdentity.isLocalPlayer)
                TutorialManager.singleton.Setup();
            else
            {
                if(numberOfTry > 0)
                {
                    numberOfTry--;
                    Invoke(nameof(Open), 0.1f);
                }
            }
        }
    }
}
