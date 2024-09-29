using UnityEngine;
using System.Collections;


namespace ScriptBoy.Fly2D
{
    [AddComponentMenu(" Script Boy/Fly 2D/Explosion")]
    public class F2DExplosion : MonoBehaviour
    {
        public float radius = 2;
        public float forceMagnitude = 2;
        public float delay;

        public bool autoExplode = true;
        public bool autoDestroy = true;


        private void Start()
        {
            if (autoExplode)
            {
                if (delay == 0)
                {
                    Explode();
                    if(autoDestroy) Destroy();
                }
                else
                {
                    StartCoroutine(WaitForSeconds());
                }
            }
        }

        private IEnumerator WaitForSeconds()
        {
            yield return new WaitForSeconds(delay);
            Explode();
            if (autoDestroy) Destroy();
        }

        public void Explode()
        {
            float localToWorld = Mathf.Max(transform.localScale.x, transform.localScale.y);
            Explode(transform.position, radius * localToWorld, forceMagnitude);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public static void Explode(Vector2 center,float radius,float forceMagnitude)
        {
            float sqrRadius = radius * radius;

            foreach (var zone in F2DFlyZoneManager.FlyZoneArray)
            {
                FlyParticle[] flies = zone.flies;
                for (int i = 0; i < zone.flyCount; i++)
                {
                    Vector3 position = flies[i].Position;

                    float delta_x = position.x - center.x;
                    float delta_y = position.y - center.y;
                    float sqrMagnitude = delta_x * delta_x + delta_y * delta_y;

                    if (sqrMagnitude < sqrRadius)
                    {
                        if (flies[i].landing) flies[i].StopLanding();

                        float magnitude = Mathf.Sqrt(sqrMagnitude);
                        float f = (1 - (magnitude / radius)) * forceMagnitude;

                        flies[i].velocity.x += (delta_x / magnitude) * f;
                        flies[i].velocity.y += (delta_y / magnitude) * f;
                    }
                }
            }
        }

        public static IEnumerator Explode(Vector2 center, float radius, float forceMagnitude, float delay)
        {
            yield return new WaitForSeconds(delay);
            Explode(center, radius, forceMagnitude);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.CircleHandleCap(0, position, Quaternion.identity, radius, EventType.Repaint);
        }
#endif
    }
}