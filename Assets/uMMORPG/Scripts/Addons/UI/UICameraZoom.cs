using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICameraZoom : MonoBehaviour
{
    public static UICameraZoom singleton;
    public Button zoomIN;
    public Button zoomOut;
    public CameraMMO2D cameraMMO2D;
    public GameObject panel;

    void Start()
    {
        if (!singleton) singleton = this;

        //cameraMMO2D = CameraMMO2D.singleton;

        zoomIN.onClick.RemoveAllListeners();
        zoomIN.onClick.AddListener(() =>
        {
            CameraMMO2D.singleton.ManageZoom(1);
            cameraMMO2D.enabled = true;
        });

        zoomOut.onClick.RemoveAllListeners();
        zoomOut.onClick.AddListener(() =>
        {
            CameraMMO2D.singleton.ManageZoom(-1);
            cameraMMO2D.enabled = true;
        });
    }

}
