using UnityEngine;

namespace ScriptBoy.Fly2D
{
    public struct FlyParticle
    {
        public Transform parent;
        public Quaternion localRotation;
        public Vector3 localPosition;
        public Vector3 velocity;

        public float time;
        public bool landing;
        public bool overlaping;


        public Quaternion Rotation
        {
            get
            {
                return (landing) ? parent.rotation * localRotation : localRotation;
            }
            set
            {
                localRotation = (landing) ? Quaternion.Inverse(parent.rotation) * value : value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return (landing) ? parent.TransformPoint(localPosition) : localPosition;
            }
            set
            {
                localPosition = (landing) ? parent.InverseTransformPoint(value) : value;
            }
        }

        public void StartLanding(Transform transform)
        {
            SetParent(transform);
            landing = true;
            if (time > 0) time = 0;
        }

        public void StopLanding()
        {
            SetParent(null);
            landing = false;
            if (time > 0) time = 0;
        }

        public void SetParent(Transform transform)
        {
            velocity.x = 0;
            velocity.y = 0;

            if (transform == null)
            {
                localPosition = Position;
                localRotation = Rotation;
                localRotation.x = 0;
                localRotation.y = 0;
            }
            else
            {
                localPosition = transform.InverseTransformPoint(Position);
                localRotation = Quaternion.Inverse(transform.rotation) * Rotation;
            }
            parent = transform;
        }
    }
}