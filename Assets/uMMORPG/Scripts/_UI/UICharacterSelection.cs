// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too.
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UICharacterSelection : MonoBehaviour
{
    public static UICharacterSelection singleton;
    public UICharacterCreationCustom uiCharacterCreation;
    public UIConfirmation uiConfirmation;
    public CameraMMO2D cameraMMO;
    public NetworkManagerMMO manager; // singleton is null until update
    public GameObject panel;
    public Button startButton;
    public Button deleteButton;
    public Button createButton;
    public Button quitButton;
    public GameObject loginImage;
    public GameObject entryInfo;
    public Button entryButton;
    public Image entryPanelImage;
    public Button entryPanelButton;


    private CharactersAvailableMsg.CharacterPreview[] characters = new CharactersAvailableMsg.CharacterPreview[0];


    public void Start()
    {
        if (!singleton) singleton = this;
        entryButton.onClick.RemoveAllListeners();
        entryButton.onClick.AddListener(() => {
            entryInfo.SetActive(false);
            entryPanelImage.enabled = false;
            JoinTheGame();
        });

        entryPanelButton.onClick.AddListener(() => {
            entryInfo.SetActive(false);
            entryPanelImage.enabled = false;
        });
    }

        void Update()
        {
            // show while in lobby and while not creating a character
            if (manager.state == NetworkState.Lobby && !uiCharacterCreation.panel.activeInHierarchy)
            {
                panel.SetActive(true);
                loginImage.SetActive(false);

                // characters available message received already?
                if (manager.charactersAvailableMsg.characters != null)
                {
                    characters = manager.charactersAvailableMsg.characters;
                    if (cameraMMO.target == null && characters.Length > 0)
                    {
                        cameraMMO.target = CharacterSelector.singleton.playerPlacement[0];
                        //CharacterSelector.singleton.currentIndex = 0;
                        //manager.selection = 0;
                    }

                    // start button: calls AddPLayer which calls OnServerAddPlayer
                    // -> button sends a request to the server
                    // -> if we press button again while request hasn't finished
                    //    then we will get the error:
                    //    'ClientScene::AddPlayer: playerControllerId of 0 already in use.'
                    //    which will happen sometimes at low-fps or high-latency
                    // -> internally ClientScene.AddPlayer adds to localPlayers
                    //    immediately, so let's check that first
                    startButton.gameObject.SetActive(manager.selection != -1);
                    startButton.onClick.SetListener(() => {
                        entryInfo.SetActive(true);
                        entryPanelImage.enabled = true;
                    });

                    // delete button
                    deleteButton.gameObject.SetActive(manager.selection != -1);
                    deleteButton.onClick.SetListener(() => {
                        uiConfirmation.Show(
                            "Do you really want to delete <b>" + characters[manager.selection].name + "</b>?",
                            () => {
                                NetworkClient.Send(new CharacterDeleteMsg { index = manager.selection });
                                UIConfirmation.singleton.closeButton.onClick.Invoke();
                            }
                        );
                    });

                    // create button
                    createButton.interactable = characters.Length < manager.characterLimit;
                    createButton.onClick.SetListener(() => {
                        panel.SetActive(false);
                        uiCharacterCreation.ManageVisibility(true);
                    });

                    // quit button
                    quitButton.onClick.SetListener(() => { NetworkManagerMMO.Quit(); });
                }
            }
            else panel.SetActive(false);
        }

        public void JoinTheGame()
        {
            // set client "ready". we will receive world messages from
            // monsters etc. then.
            NetworkClient.Ready();

            // send CharacterSelect message (need to be ready first!)
            NetworkClient.Send(new CharacterSelectMsg { index = manager.selection });

            // clear character selection previews
            manager.ClearPreviews();

            // make sure we can't select twice and call AddPlayer twice
            panel.SetActive(false);
            Camera.main.targetTexture = null;
            RenderTextureManager.singleton.creationCamera.enabled = false;
        }
}
