using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncBookRotation : MonoBehaviour
{
    public Quaternion rotation;

    void Update()
    {
        if(transform.localRotation != rotation)
        {
            transform.localRotation = rotation;
            Destroy(this);
        }
    }
}
