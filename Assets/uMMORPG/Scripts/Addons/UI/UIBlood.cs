using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBlood : MonoBehaviour
{
    public static UIBlood singleton;
    public Animator animator;
    public Image image;

    void Start()
    {
        if (!singleton) singleton = this;
    }


    public void ManageAnimator(bool condition)
    {
        if (condition)
        {
            animator.enabled = true;
        }
        else
        {
            animator.enabled = false;
            image.color = new Color(1,1,1,0);
        }
    }
}
