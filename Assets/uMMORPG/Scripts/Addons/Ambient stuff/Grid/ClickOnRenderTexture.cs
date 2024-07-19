using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClickOnRenderTexture : MonoBehaviour
{
    public static ClickOnRenderTexture singleton;
    public GameObject prefabToInstantiate;
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

    public List<Button> pinButtons = new List<Button>();
    public List<Button> modifyButtons = new List<Button>();
    public List<Button> deleteButtons = new List<Button>();



    public void OnEnable()
    {
        if (!singleton) singleton = this;

        if (pin.Count == 0)
        {
            if (!Player.localPlayer.playerSpawnpoint)
            {
                Player.localPlayer.playerSpawnpoint = Player.localPlayer.GetComponent<PlayerSpawnpoint>();
                Player.localPlayer.playerSpawnpoint.Assign();
            }
            else
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
        }

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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = Input.mousePosition;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, clickPosition, null, out localPoint);
            
            float percentX = (localPoint.x - (- rectTransform.rect.width /2)) / ((rectTransform.rect.width / 2) - (- rectTransform.rect.width / 2)) * 100f;

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
