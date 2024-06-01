using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    private Transform thisTransform;
    public float rotateAmount;
    public Transform pivot;

    public void Start()
    {
        thisTransform = this.transform;
    }

    public void Update()
    {
        //thisTransform.Rotate(new Vector3(thisTransform.transform.eulerAngles.x + rotateAmount.x, thisTransform.transform.eulerAngles.y + rotateAmount.y, thisTransform.transform.eulerAngles.z + rotateAmount.z),Space.Self);
        thisTransform.RotateAround(pivot.position, Vector3.up, rotateAmount * Time.deltaTime);
    }
}
