using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager singleton;
    public Sprite completed;
    public Sprite notCompleted;

    void Start()
    {
        if (!singleton) singleton = this;
    }

}
