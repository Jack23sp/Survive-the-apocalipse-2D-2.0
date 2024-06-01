using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmojiSlot : MonoBehaviour
{
    public Button manageEmojiButton;
    public GameObject padLock;
    public Animator animator;


    public void CheckEmoji(bool activateAnimator)
    {
        animator.enabled = activateAnimator;
    }
}
