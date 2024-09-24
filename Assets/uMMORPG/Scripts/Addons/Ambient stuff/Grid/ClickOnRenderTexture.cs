using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public struct ListWithBackgroundShadow
{
    public Image image;
    public Button button;
    public Button seeButton;
}

public class ClickOnRenderTexture : MonoBehaviour
{
    public static ClickOnRenderTexture singleton;
    public GameObject prefabToInstantiate;
    public GameObject prefabToInstantiateDestination;
    public GameObject prefabToInstantiateLocaion;
    public RectTransform rectTransform;
    public BoxCollider2D boxCollider;
    public Transform anchor;
    public Vector3 size = new Vector3(2937.828f,1351.968f);
    public List<GameObject> pin = new List<GameObject>();
    public List<Vector3> pinPlaces = new List<Vector3>();
    public int maxPin = 5;
    public Button addButton;
    public Image addImage;
    public bool add;
    public bool move;
    public int indexMove;
    public Transform contentPin;
    public Transform contentButtons;

    #region Pin part
    public List<Button> pinButtons = new List<Button>();
    public List<Button> modifyButtons = new List<Button>();
    public List<Button> deleteButtons = new List<Button>();
    #endregion

    #region Spawnpoint
    public List<SpawnpointSlot> spawnpoints = new List<SpawnpointSlot>();
    #endregion

    public List<ListWithBackgroundShadow> selectedButton = new List<ListWithBackgroundShadow>();
    public bool seePin = true, seeSpawnpoint = true, seePath = false;
    public GameObject destination;

    public List<GameObject> pinUIObjectToManage = new List<GameObject>();
    public List<GameObject> spawnpointUIObjectToManage = new List<GameObject>();
    public List<GameObject> destinationUIObjectToManage = new List<GameObject>();
    private int selectedIndex;

    public void OnEnable()
    {
        if (!singleton) singleton = this;

        if (!Player.localPlayer.playerSpawnpoint)
        {
            Player.localPlayer.playerSpawnpoint = Player.localPlayer.GetComponent<PlayerSpawnpoint>();
            Player.localPlayer.playerSpawnpoint.Assign();
        }

        if (pin.Count == 0)
        {
            pinPlaces = Player.localPlayer.playerSpawnpoint.pins;
            for (int i = 0; i < pinPlaces.Count; i++)
            {
                int index = i;
                GameObject g = Instantiate(prefabToInstantiate, pinPlaces[index], Quaternion.identity);
                g.GetComponent<DeathSymbol>().dontDestroyAtBegin = true;
                pin.Add(g);
                modifyButtons[index].transform.parent.transform.gameObject.SetActive(index < pin.Count);
                pinButtons[index].gameObject.SetActive(index < pin.Count);
            }
        }

        ManageDetectionOnTypeButtonClick();

        selectedButton[0].button.onClick.RemoveAllListeners();
        selectedButton[0].button.onClick.AddListener(() =>
        {
            Player.localPlayer.playerSpawnpoint.pin = true;
            Player.localPlayer.playerSpawnpoint.pointofSpawn = false;
            Player.localPlayer.playerSpawnpoint.path = false;
            selectedIndex = 0;
            ManageDetectionOnTypeButtonClick();
            ManageVisibilityofUIBasedOnCategory(selectedIndex);
        });

        selectedButton[0].seeButton.onClick.RemoveAllListeners();
        selectedButton[0].seeButton.onClick.AddListener(() =>
        {
            seePin = !seePin;
            selectedButton[0].seeButton.transform.GetChild(0).GetComponent<Image>().sprite = seePin ? ImageManager.singleton.openEyeImage : ImageManager.singleton.closeEyeImage;
            ManageVisibility(0, seePin);
        });

        selectedButton[1].button.onClick.RemoveAllListeners();
        selectedButton[1].button.onClick.AddListener(() =>
        {
            Player.localPlayer.playerSpawnpoint.pin = false;
            Player.localPlayer.playerSpawnpoint.pointofSpawn = true;
            Player.localPlayer.playerSpawnpoint.path = false;
            selectedIndex = 1;
            ManageDetectionOnTypeButtonClick();
            ManageVisibilityofUIBasedOnCategory(selectedIndex);
        });

        selectedButton[1].seeButton.onClick.RemoveAllListeners();
        selectedButton[1].seeButton.onClick.AddListener(() =>
        {
            seeSpawnpoint = !seeSpawnpoint;
            selectedButton[1].seeButton.transform.GetChild(0).GetComponent<Image>().sprite = seeSpawnpoint ? ImageManager.singleton.openEyeImage : ImageManager.singleton.closeEyeImage;
            ManageVisibility(1, seeSpawnpoint);
        });


        selectedButton[2].button.onClick.RemoveAllListeners();
        selectedButton[2].button.onClick.AddListener(() =>
        {
            Player.localPlayer.playerSpawnpoint.pin = false;
            Player.localPlayer.playerSpawnpoint.pointofSpawn = false;
            Player.localPlayer.playerSpawnpoint.path = true;
            selectedIndex = 2;
            ManageDetectionOnTypeButtonClick();
            ManageVisibilityofUIBasedOnCategory(selectedIndex);
        });

        selectedButton[2].seeButton.onClick.RemoveAllListeners();
        selectedButton[2].seeButton.onClick.AddListener(() =>
        {
            seePath = !seePath;
            selectedButton[2].seeButton.transform.GetChild(0).GetComponent<Image>().sprite = seePath ? ImageManager.singleton.openEyeImage : ImageManager.singleton.closeEyeImage;
            ManageVisibility(2, seePath);
        });


        addButton.onClick.RemoveAllListeners();
        addButton.onClick.AddListener(() =>
        {
            add = !add;
            addImage.gameObject.SetActive(add);
            move = false;
            indexMove = -1;
        });


        for (int i = 0; i < maxPin; i++)
        {
            int index = i;
            pinButtons[index].onClick.RemoveAllListeners();
            pinButtons[index].onClick.AddListener(() =>
            {
                add = false;
                addImage.gameObject.SetActive(add);
                move = false;
                GameObject g = Instantiate(prefabToInstantiateLocaion, pin[index].transform.position, Quaternion.identity);
                g.transform.SetParent(pin[index].transform);

            });

            modifyButtons[index].transform.parent.transform.gameObject.SetActive(index < pin.Count);

            modifyButtons[index].onClick.RemoveAllListeners();
            modifyButtons[index].onClick.AddListener(() =>
            {
                if (indexMove == -1) move = true;
                else if (indexMove == index) move = !move;
                else move = true;

                add = false;
                addImage.gameObject.SetActive(add);
                indexMove = index;
            });

            deleteButtons[index].onClick.RemoveAllListeners();
            deleteButtons[index].onClick.AddListener(() =>
            {
                add = false;
                move = false;
                Destroy(pin[index].gameObject);
                pin.RemoveAt(index);
                Refresh();

                pinPlaces.Clear();
                for (int i = 0; i < pin.Count; i++)
                {
                    pinPlaces.Add(new Vector3(pin[i].transform.position.x, pin[i].transform.position.y, 0.0f));
                }
                Player.localPlayer.playerSpawnpoint.CmdSyncToServerPin(pinPlaces.ToArray());
            });
        }
    }


    public void ManageDetectionOnTypeButtonClick()
    {
        selectedButton[0].image.enabled = Player.localPlayer.playerSpawnpoint.pin == true;
        selectedButton[1].image.enabled = Player.localPlayer.playerSpawnpoint.pointofSpawn == true;
        selectedButton[2].image.enabled = Player.localPlayer.playerSpawnpoint.path == true;
    }

    public void ManageVisibilityofUIBasedOnCategory(int type)
    {
        foreach(GameObject g in pinUIObjectToManage)
        {
            g.SetActive(type == 0);
        }

        foreach (GameObject g in spawnpointUIObjectToManage)
        {
            g.SetActive(type == 1);
        }

        if(type == 1)
        {
            foreach(SpawnpointSlot s in spawnpoints)
            {
                s.gameObject.SetActive(false);
            }

            for(int i = 0; i < Player.localPlayer.playerSpawnpoint.spawnpoint.Count; i++)
            {
                int index = i;
                SpawnpointSlot slot = spawnpoints[index];
                slot.gameObject.SetActive(true);
                slot.GetComponentInChildren<TextMeshProUGUI>().text = Player.localPlayer.playerSpawnpoint.spawnpoint[index].name;
                Color newColor;

                if (ColorUtility.TryParseHtmlString(Player.localPlayer.playerSpawnpoint.spawnpoint[index].color, out newColor))
                {
                    slot.GetComponent<Image>().color = newColor;
                }
            }
        }


        foreach (GameObject g in destinationUIObjectToManage)
        {
            g.SetActive(type == 2);
        }

    }

    public void ManageVisibility(int type, bool condition)
    {
        if (type == 0)
        {
            foreach (GameObject g in pin)
            {
                g.SetActive(condition);
            }
        }
        else if (type == 1)
        {
            foreach (GameObject g in Player.localPlayer.playerSpawnpoint.spawnpointObjects)
            {
                g.SetActive(condition);
            }


        }
        else if (type == 2)
        {
            if(destination) destination.SetActive(condition);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!Player.localPlayer) return;
            if (!Player.localPlayer.playerSpawnpoint) return;

            if (Player.localPlayer.playerSpawnpoint.pin)
            {
                Vector2 clickPosition = Input.mousePosition;

                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, clickPosition, null, out localPoint);

                float percentX = (localPoint.x - (-rectTransform.rect.width / 2)) / ((rectTransform.rect.width / 2) - (-rectTransform.rect.width / 2)) * 100f;

                float percentY = (localPoint.y - (-rectTransform.rect.height / 2)) / ((rectTransform.rect.height / 2) - (-rectTransform.rect.height / 2)) * 100f;

                float offsetX = Mathf.Abs(percentX * (size.x / 100));
                float offsetY = Mathf.Abs(percentY * (size.y / 100));

                if (move && indexMove > -1)
                {
                    if (percentX > 100.0f || percentY > 100.0f) return;
                    pin[indexMove].transform.position = anchor.position + new Vector3(offsetX, offsetY, 0f);
                }
                else
                {
                    if (percentX > 100.0f || percentY > 100.0f) return;

                    Vector3 position = anchor.position + new Vector3(offsetX, offsetY, 0f);

                    if (pin.Count < 5)
                    {
                        GameObject g = Instantiate(prefabToInstantiate, position, Quaternion.identity);
                        g.GetComponent<DeathSymbol>().dontDestroyAtBegin = true;
                        pin.Add(g);

                        pinPlaces.Add(new Vector3(g.transform.position.x, g.transform.position.y, 0.0f));
                        Player.localPlayer.playerSpawnpoint.CmdSyncToServerPin(pinPlaces.ToArray());

                        addButton.gameObject.SetActive(pin.Count < maxPin);


                        for (int i = 0; i < maxPin; i++)
                        {
                            int index = i;
                            pinButtons[index].gameObject.SetActive(index < pin.Count);
                            modifyButtons[index].transform.parent.transform.gameObject.SetActive(index < pin.Count);
                        }
                    }
                }
            }
            else if(Player.localPlayer.playerSpawnpoint.path)
            {
                Vector2 clickPosition = Input.mousePosition;

                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, clickPosition, null, out localPoint);

                float percentX = (localPoint.x - (-rectTransform.rect.width / 2)) / ((rectTransform.rect.width / 2) - (-rectTransform.rect.width / 2)) * 100f;

                float percentY = (localPoint.y - (-rectTransform.rect.height / 2)) / ((rectTransform.rect.height / 2) - (-rectTransform.rect.height / 2)) * 100f;

                float offsetX = Mathf.Abs(percentX * (size.x / 100));
                float offsetY = Mathf.Abs(percentY * (size.y / 100));

                if (move && indexMove > -1)
                {
                    if (percentX > 100.0f || percentY > 100.0f) return;
                    pin[indexMove].transform.position = anchor.position + new Vector3(offsetX, offsetY, 0f);
                }
                else
                {
                    if (percentX > 100.0f || percentY > 100.0f) return;

                    Vector3 position = anchor.position + new Vector3(offsetX, offsetY, 0f);

                    if (destination) Destroy(destination);

                        destination = Instantiate(prefabToInstantiateDestination, position, Quaternion.identity);
                        destination.GetComponent<DeathSymbol>().dontDestroyAtBegin = true;
                }

            }
        }
    }

    public void Refresh()
    {
        addButton.gameObject.SetActive(pin.Count < maxPin);
        for (int i = 0; i < maxPin; i++)
        {
            int index = i;
            pinButtons[index].gameObject.SetActive(index < pin.Count);
            modifyButtons[index].transform.parent.transform.gameObject.SetActive(index < pin.Count);
        }
    }
}
