using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionPanel : MonoBehaviour
{
    public static UIInteractionPanel singleton;
    public Button actionButton;

    void Start()
    {
        if (!singleton) singleton = this;        
    }

}
