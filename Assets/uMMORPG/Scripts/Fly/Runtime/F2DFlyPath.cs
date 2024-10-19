using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptBoy.Fly2D
{
    [AddComponentMenu(" Script Boy/Fly 2D/Fly Path")]
    public class F2DFlyPath : MonoBehaviour
    {
        public List<Vector2> localPositions;
        public int PositionCount
        {
            get
            {
                return localPositions.Count;
            }
        }

        public Vector3 GetPosition(int index)
        {
            return transform.TransformPoint(localPositions[index]);
        }
    }
}
