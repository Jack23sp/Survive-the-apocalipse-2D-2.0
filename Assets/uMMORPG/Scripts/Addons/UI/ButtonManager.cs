using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager singleton;
    public Button closeButton;
    public Image raycastImage;

    public GameObject mainPanel;
    public List<ScrollRect> contents;

    public Button statsButton;
    public GameObject statsPanel;

    public Button abilityButton;
    public UIAbilities abilityPanel;

    public Button groupButton;
    public UIGroup groupPanel;

    public Button boostsButton;
    public UIBoost boostPanel;

    public Button questsButton;
    public GameObject questPanel;

    public Button friendsButton;
    public GameObject friendPanel;

    public Button optionsButton;
    public GameObject optionPanel;

    [SerializeField] UISelectedItem uISelectedItem;

    private void Awake()
    {
        if (!singleton) singleton = this;
    }

    void Start()
    {
        statsButton.onClick.AddListener(() =>
        {
            foreach(ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(true);
            abilityPanel.gameObject.SetActive(false);
            abilityPanel.Reset();

            groupPanel.gameObject.SetActive(false);
            groupPanel.Reset();

            boostPanel.gameObject.SetActive(false);
            boostPanel.Reset();

            questPanel.SetActive(false);
            friendPanel.SetActive(false);
            optionPanel.SetActive(false);
        });
        abilityButton.onClick.AddListener(() =>
        {
            foreach (ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(false);
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);

            abilityPanel.gameObject.SetActive(true);
            groupPanel.gameObject.SetActive(false);
            groupPanel.Reset();

            boostPanel.gameObject.SetActive(false);
            boostPanel.Reset();

            questPanel.SetActive(false);
            friendPanel.SetActive(false);
            optionPanel.SetActive(false);
        });

        groupButton.onClick.AddListener(() =>
        {
            foreach (ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(false);
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);

            abilityPanel.gameObject.SetActive(false);
            abilityPanel.Reset();

            groupPanel.gameObject.SetActive(true);
            boostPanel.gameObject.SetActive(false);
            boostPanel.Reset();

            questPanel.SetActive(false);
            friendPanel.SetActive(false);
            optionPanel.SetActive(false);
        });
        boostsButton.onClick.AddListener(() =>
        {
            foreach (ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(false);
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);

            abilityPanel.gameObject.SetActive(false);
            abilityPanel.Reset();

            groupPanel.gameObject.SetActive(false);
            groupPanel.Reset();

            boostPanel.gameObject.SetActive(true);
            boostPanel.Reset();

            questPanel.SetActive(false);
            friendPanel.SetActive(false);
            optionPanel.SetActive(false);
        });
        questsButton.onClick.AddListener(() =>
        {
            foreach (ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(false);
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);

            abilityPanel.gameObject.SetActive(false);
            abilityPanel.Reset();

            groupPanel.gameObject.SetActive(false);
            groupPanel.Reset();

            boostPanel.gameObject.SetActive(false);
            boostPanel.Reset();

            questPanel.SetActive(true);
            friendPanel.SetActive(false);
            optionPanel.SetActive(false);
        });
        friendsButton.onClick.AddListener(() =>
        {
            foreach (ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(false);
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);

            abilityPanel.gameObject.SetActive(false);
            abilityPanel.Reset();

            groupPanel.gameObject.SetActive(false);
            groupPanel.Reset();

            boostPanel.gameObject.SetActive(false);
            boostPanel.Reset();

            questPanel.SetActive(false);
            friendPanel.SetActive(true);
            optionPanel.SetActive(false);
        });
        optionsButton.onClick.AddListener(() =>
        {
            foreach (ScrollRect tr in contents)
            {
                AutoScroll(tr);
            }
            statsPanel.SetActive(false);
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);

            abilityPanel.gameObject.SetActive(false);
            abilityPanel.Reset();

            groupPanel.gameObject.SetActive(false);
            groupPanel.Reset();

            boostPanel.gameObject.SetActive(false);
            boostPanel.Reset();

            questPanel.SetActive(false);
            friendPanel.SetActive(false);
            optionPanel.SetActive(true);
        });

        closeButton.onClick.AddListener(() =>
        {
            statsButton.onClick.Invoke();
            if (GameObjectSpawnManager.singleton.spawnedSelectedItem != null) Destroy(GameObjectSpawnManager.singleton.spawnedSelectedItem);
            uISelectedItem.gameObject.SetActive(false);
            mainPanel.SetActive(false);
            abilityPanel.Reset();
            groupPanel.Reset();
            boostPanel.Reset();

            raycastImage.raycastTarget = false;
        });
    }

    void AutoScroll(ScrollRect scrollRect)
    {
        scrollRect.verticalNormalizedPosition = 1;
    }
}
