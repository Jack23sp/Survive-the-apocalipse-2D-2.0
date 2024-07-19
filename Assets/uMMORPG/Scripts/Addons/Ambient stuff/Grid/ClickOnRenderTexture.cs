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



    public void Start()
    {
        if (!singleton) singleton = this;

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
