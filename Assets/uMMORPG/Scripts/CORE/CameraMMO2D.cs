// Simple MMO camera that always follows the player.
using UnityEngine;

public class CameraMMO2D : MonoBehaviour
{
    public static CameraMMO2D singleton;
    [Header("Target Follow")]
    public Transform target;
    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector2 offset = Vector2.zero;
    public Vector2 offsetSelection = Vector2.zero;

    public float cameraSizeSelector;
    public float cameraSizePlayer;

    // smooth the camera movement
    [Header("Dampening")]
    public float damp = 2;
    public float dampSelection = 2;

    [Header("Max zoom")]
    public float originalZoom = -1.0f;
    public float maxAdd = 6.0f;

    public void Start()
    {
        if (!singleton) singleton = this;
    }

    void LateUpdate()
    {
        if (!target) return;

        if (Player.localPlayer)
        {
            Camera.main.orthographicSize = cameraSizePlayer;
            if (originalZoom == -1.0f) originalZoom = cameraSizePlayer;
            // calculate goal position
            Vector2 goal = (Vector2)target.position + offset;

            // interpolate
            Vector2 position = Vector2.Lerp(transform.position, goal, Time.deltaTime * damp);

            // convert to 3D but keep Z to stay in front of 2D plane
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
        else
        {
            Camera.main.orthographicSize = cameraSizeSelector;
            Vector2 goal = (Vector2)target.position + offsetSelection;
            Vector2 position = Vector2.Lerp(transform.position, goal, Time.deltaTime * dampSelection);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }
    }

    public void ManageZoom(float amount)
    {
        if (Camera.main.orthographicSize + amount <= originalZoom)
        {
            cameraSizePlayer = originalZoom; Camera.main.orthographicSize = originalZoom;
        }
        else if (Camera.main.orthographicSize + amount > originalZoom + maxAdd)
        {
            cameraSizePlayer = originalZoom + maxAdd;
            Camera.main.orthographicSize = originalZoom + maxAdd;
        }
        else
        {
            cameraSizePlayer = cameraSizePlayer + amount;
            Camera.main.orthographicSize = cameraSizePlayer + amount;
        }

        this.enabled = false;
    }
}
