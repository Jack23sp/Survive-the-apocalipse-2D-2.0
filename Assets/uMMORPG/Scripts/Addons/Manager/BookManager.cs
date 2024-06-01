using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager singleton;
    public RuntimeAnimatorController animator;

    void Awake()
    {
        if(!singleton) singleton = this;
    }


}
