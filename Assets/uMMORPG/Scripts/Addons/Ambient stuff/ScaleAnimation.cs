using UnityEngine;

public class ScaleAnimation : MonoBehaviour
{
    public Vector2 fixedScale;
    public Vector2 targetScale = new Vector2(2f, 2f); // The target scale for the animation
    public float duration = 1f; // The duration of the animation

    private Vector3 originalScale; // The original scale of the object
    private Vector2 currentScale; // The current scale of the object during animation
    private float startTime; // The time the animation started
    private float endTime; // The time the animation will end
    [HideInInspector] public bool isAnimating = false; // Whether the animation is currently playing
    private Transform myTransform;
    private bool completed;

    void Start()
    {
        myTransform = this.transform;
        originalScale = myTransform.localScale;
    }

    void Update()
    {
        if (isAnimating)
        {
            completed = false;
            float timeRatio = (Time.time - startTime) / duration;
            currentScale = Vector2.Lerp(originalScale, targetScale, timeRatio);
            myTransform.localScale = new Vector3(currentScale.x, currentScale.y, originalScale.z);

            if (Time.time >= endTime)
            {
                completed = true;
                isAnimating = false; // stop animating
                startTime = Time.time; // reset start time
                endTime = startTime + duration; // set end time for returning to original scale
            }
        }
        else
        {
            if (completed)
            {
                if (myTransform.localScale != originalScale)
                {
                    myTransform.localScale = Vector3.Lerp(currentScale, originalScale, (Time.time - startTime) / duration);
                    completed = !(myTransform.localScale == originalScale);
                }
            }
        }
    }

    public void StartAnimation()
    {
        startTime = Time.time;
        endTime = startTime + duration;
        isAnimating = true;
    }
}
