using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ScriptBoy.Fly2D
{
    [AddComponentMenu(" Script Boy/Fly 2D/Landing Mask")]
    public sealed class F2DLandingMask : MonoBehaviour
    {
        public AreaType type;
        public new Collider2D collider;
        public float radius;

        public void UpdateFlies(FlyParticle[] flies)
        {
            int flyCount = flies.Length;

            if (type == AreaType.Collider)
            {
                if (collider)
                {
                    for (int i = 0; i < flyCount; i++)
                    {
                        if (flies[i].landing)
                        {
                            Vector3 v3 = flies[i].Position;

                            Vector2 v2;
                            v2.x = v3.x;
                            v2.y = v3.y;

                            if (!collider.OverlapPoint(v2))
                            {
                                flies[i].StopLanding();
                            }
                        }
                    }
                }
            }
            else //  if (type == LandingAreaType.CircleZone)
            {
                Vector3 center = transform.position;

                float xCenter = center.x;
                float yCenter = center.y;

                float sqrRadius = radius * radius;
                for (int i = 0; i < flyCount; i++)
                {
                    if (flies[i].landing)
                    {
                        Vector3 position = flies[i].Position;

                        float xDelta = xCenter - position.x;
                        float yDelta = yCenter - position.y;
                        float sqrMagnitude = xDelta * xDelta + yDelta * yDelta;

                        if (sqrMagnitude > sqrRadius) flies[i].StopLanding();
                    }
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (type == AreaType.Circle)
            {
                Vector3 position = transform.position;
                using (new Handles.DrawingScope(Color.green))
                {
                    Handles.CircleHandleCap(0, position, Quaternion.identity, radius, EventType.Repaint);
                }
            }
        }
#endif
    }
}