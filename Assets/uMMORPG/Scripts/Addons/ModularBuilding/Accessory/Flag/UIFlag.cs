using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class UIFlag : MonoBehaviour, IUIScript
{
    public static UIFlag singleton;

    public GameObject panel;

    public TMP_InputField flagInputText;
    public Button setButton;
    public TextMeshProUGUI setButtonText;
    public Transform content;
    public Button closeButton;
    public Button manageButton;

    public Flag flag;

    public GameObject objectToInstantiate;
    public int selectedFlag = 0;

    public TextMeshProUGUI flagName;
    public List<int> searchedFlags = new List<int>();


    void Start()
    {
        if (!singleton) singleton = this;
    }

    public void Open(Flag Flag)
    {
        flag = Flag;
        Assign();

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            Close();
        });

        setButton.onClick.RemoveAllListeners();
        setButton.onClick.AddListener(() =>
        {
            if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
            if (flagName.text != string.Empty)
            {
                Player.localPlayer.CmdSetFlag(FindFlagIndex(flagName.text), flag.netIdentity);
                closeButton.onClick.Invoke();
            }
        });

        manageButton.gameObject.SetActive(ModularBuildingManager.singleton.CanDoOtherActionForniture(flag, Player.localPlayer));
        manageButton.onClick.RemoveAllListeners();
        manageButton.onClick.AddListener(() =>
        {
            GameObject g = Instantiate(GameObjectSpawnManager.singleton.confirmManagerAccessory, GameObjectSpawnManager.singleton.canvas);
            g.GetComponent<UIBuildingAccessoryManager>().Init(flag.netIdentity, flag.craftingAccessoryItem, closeButton);
        });

        flagInputText.onValueChanged.RemoveAllListeners();
        flagInputText.onValueChanged.AddListener(delegate { CheckChange(); });


        closeButton.image.raycastTarget = true;
        panel.SetActive(true);
        closeButton.image.enabled = true;
        setButtonText.text = "Set!";
        Spawn();
    }

    public void Spawn()
    {
        UIUtils.BalancePrefabs(objectToInstantiate, FlagManager.singleton.flags.Count, content);
        for (int i = 0; i < FlagManager.singleton.flags.Count; i++)
        {
            int index = i;
            FlagSlot slot = content.GetChild(index).GetComponent<FlagSlot>();
            slot.image.sprite = FlagManager.singleton.flags[index];
            slot.flagName.text = FlagManager.singleton.flags[index].name;
            slot.button.onClick.RemoveAllListeners();
            slot.button.onClick.AddListener(() =>
            {
                if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                flagName.text = slot.flagName.text;
            });
        }
    }

    public void CheckChange()
    {
        searchedFlags = new List<int>();
        for(int i = 0; i < FlagManager.singleton.flags.Count; i++)
        {
            if (FlagManager.singleton.flags[i].name.ToLower().Contains(flagInputText.text.ToLower()))
            {
                if (!searchedFlags.Contains(i)) searchedFlags.Add(i);
            }
        }
        if(flagInputText.text != string.Empty)
        {
            UIUtils.BalancePrefabs(objectToInstantiate, searchedFlags.Count, content);
            for(int i = 0; i < searchedFlags.Count; i++)
            {
                int index = i;
                FlagSlot slot = content.GetChild(index).GetComponent<FlagSlot>();
                slot.image.sprite = FlagManager.singleton.flags[searchedFlags[index]];
                slot.flagName.text = FlagManager.singleton.flags[searchedFlags[index]].name;
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    flagName.text = slot.flagName.text;
                });
            }
        }
        else
        {
            UIUtils.BalancePrefabs(objectToInstantiate, FlagManager.singleton.flags.Count, content);
            for (int i = 0; i < FlagManager.singleton.flags.Count; i++)
            {
                int index = i;
                FlagSlot slot = content.GetChild(index).GetComponent<FlagSlot>();
                slot.image.sprite = FlagManager.singleton.flags[index];
                slot.flagName.text = FlagManager.singleton.flags[index].name;
                slot.button.onClick.RemoveAllListeners();
                slot.button.onClick.AddListener(() =>
                {
                    if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(0);
                    flagName.text = slot.flagName.text;
                });
            }
        }
    }

    public int FindFlagIndex (string flagName)
    {
        for(int i = 0; i < FlagManager.singleton.flags.Count; i++)
        {
            if (FlagManager.singleton.flags[i].name == flagName) return i;
        }

        return 0;
    }


    public void Close()
    {
        if (UIButtonSounds.singleton) UIButtonSounds.singleton.ButtonPress(1);
        panel.SetActive(false);
        closeButton.image.raycastTarget = false;
        closeButton.image.enabled = false;
        RemovePlayerFromBuildingAccessory(flag.netIdentity);
        BlurManager.singleton.Show();
    }

    public void Assign()
    {
        if (!ModularBuildingManager.singleton.UIToCloseOnDeath.Contains(this)) ModularBuildingManager.singleton.UIToCloseOnDeath.Add(this);
    }

    public void RemovePlayerFromBuildingAccessory(NetworkIdentity identity)
    {
        Player.localPlayer.playerModularBuilding.CmdRemovePlayerInteractWithAccessory(identity);
    }
}
