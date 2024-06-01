using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsManager : MonoBehaviour
{
    public static FriendsManager singleton;
    public int maxFriendRequest;
    public int maxFriends;
    public Color onlineColor;
    public Color offlineColor;


    void Start()
    {
        if (!singleton) singleton = this;    
    }
}
