using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerList : MonoBehaviour
{
    private List<Ping> serverPing = new List<Ping>();
    public Transform content;
    public GameObject serverSlot;
    public NetworkManagerMMO manager;
    public Button loginButton;

    public void Start()
    {
        Invoke(nameof(CheckPing),2.0f);
    }

    void CheckPing()
    {
        if(Player.localPlayer == null)
        {
            UIUtils.BalancePrefabs(serverSlot, manager.serverList.Count,content);
            for (int i = 0; i < manager.serverList.Count; i++)
            {
                int index = i;
                ServerSlot slot = content.GetChild(index).GetComponent<ServerSlot>();
                slot.serverName.text = manager.serverList[index].name;

                slot.buttonSelect.onClick.RemoveAllListeners();
                slot.buttonSelect.onClick.AddListener(() =>
                {
                    manager.networkAddress = manager.serverList[index].ip;
                    for (int e = 0; e < manager.serverList.Count; e++)
                    {
                        int index_e = e;
                        ServerSlot slot = content.GetChild(index_e).GetComponent<ServerSlot>();
                        slot.shadow.enabled = index_e == index;
                    }
                    loginButton.interactable = true;
                });

                if(serverPing.Count -1 >= index)
                {
                    if (serverPing[index].isDone) 
                    { 
                        slot.pingText.text = serverPing[index].time.ToString();
                        slot.pingColor.color = !serverPing[index].isDone ? Color.red : serverPing[index].time <= 100 ? Color.green : serverPing[index].time > 100 && serverPing[index].time < 150 ? Color.cyan : Color.red;
                    }
                    serverPing[index] = new Ping(manager.serverList[index].ip);
                }
                else
                {
                    serverPing.Add(new Ping(manager.serverList[index].ip));
                    if (serverPing[index].isDone) 
                    { 
                        slot.pingText.text = serverPing[index].time.ToString();
                        slot.pingColor.color = !serverPing[index].isDone ? Color.red : serverPing[index].time <= 100 ? Color.green : serverPing[index].time > 100 && serverPing[index].time < 150 ? Color.cyan : Color.red;
                    }
                }   
            }
            Invoke(nameof(CheckPing), 2.0f);
        }       
    }
}
