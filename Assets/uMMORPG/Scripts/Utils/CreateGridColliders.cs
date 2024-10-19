using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class CreateGridColliders : MonoBehaviour
{
    public int numberOfColliders = 5;
    public bool isTrigger = true;

    private void Start()
    {
        CreateColliders();
    }

    public void CreateColliders()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Bounds spriteBounds = spriteRenderer.bounds;

        float width = spriteBounds.size.x / numberOfColliders;
        float height = spriteBounds.size.y / numberOfColliders;

        for (int i = 0; i < numberOfColliders; i++)
        {
            for (int j = 0; j < numberOfColliders; j++)
            {
                float offsetX = spriteBounds.min.x + (width * (i + 0.5f));
                float offsetY = spriteBounds.min.y + (height * (j + 0.5f));

                GameObject colliderObject = new GameObject($"Collider_{i}_{j}");
                colliderObject.transform.position = new Vector3(offsetX, offsetY, transform.position.z);
                colliderObject.transform.SetParent(transform);

                BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
                collider.isTrigger = isTrigger;
                collider.size = new Vector2(width, height);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CreateGridColliders))]
public class CreateGridCollidersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CreateGridColliders script = (CreateGridColliders)target;
        if (GUILayout.Button("Create Colliders"))
        {
            script.CreateColliders();
        }
    }
}
#endif
