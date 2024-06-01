using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector singleton;
    public NetworkManagerMMO manager;

    public CameraMMO2D cameraMMO;

    public int currentIndex;

    public List<Transform> playerPlacement;
    public int currentPlayers;

    public Button previousPlayer;
    public Button nextPlayer;
    public GameObject nameObject;
    public TextMeshProUGUI nameText;

    void Start()
    {
        if (!singleton) singleton = this;

        previousPlayer.gameObject.SetActive(manager.characterLimit > 1);
        nextPlayer.gameObject.SetActive(manager.characterLimit > 1);

        previousPlayer.onClick.RemoveAllListeners();
        previousPlayer.onClick.AddListener(() =>
        {
            currentPlayers = manager.charactersAvailableMsg.characters.Length;

            if (currentIndex > 0)
            {
                currentIndex--;
                cameraMMO.target = playerPlacement[currentIndex];
                manager.selection = currentIndex;
            }

            if (currentIndex == 0)
            {
                nextPlayer.interactable = currentPlayers > 1 ? true : false;
                previousPlayer.interactable = false;
            }
        });

        nextPlayer.onClick.RemoveAllListeners();
        nextPlayer.onClick.AddListener(() =>
        {
            currentPlayers = manager.charactersAvailableMsg.characters.Length;

            if (currentIndex + 1 < currentPlayers)
            {
                currentIndex++;
                cameraMMO.target = playerPlacement[currentIndex];
                manager.selection = currentIndex;
            }

            if (currentIndex + 1 == currentPlayers)
            {
                nextPlayer.interactable = false;
                previousPlayer.interactable = true;
            }
        });

    }

    public void Update()
    {
        if (manager.state == NetworkState.Lobby)
        {
            currentPlayers = manager.charactersAvailableMsg.characters.Length;

            nextPlayer.interactable = currentPlayers > 1 && currentIndex < (currentPlayers -1);
            previousPlayer.interactable = currentPlayers > 0 && currentIndex > 0;

            nameObject.SetActive(UICharacterSelection.singleton.panel.activeInHierarchy && currentPlayers > 0 && currentIndex >= 0);
            nameText.text = currentPlayers > 0 && currentIndex >= 0 ? manager.charactersAvailableMsg.characters[currentIndex].name : string.Empty;
            cameraMMO.target = currentIndex == -1 ? playerPlacement[0] : playerPlacement[currentIndex];
        }
    }

    public void CheckPreview()
    {
        currentPlayers = manager.charactersAvailableMsg.characters.Length;

        if (currentPlayers == 1)
        {
            manager.selection = 0;
            currentIndex = 0;
        }

        if (currentPlayers >= currentIndex) 
        {
            manager.selection = currentPlayers - 1;
            currentIndex = currentPlayers - 1;
        }
    }
}
