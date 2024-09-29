using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptBoy.Fly2D
{
    [AddComponentMenu(" Script Boy/Fly 2D/Follow Path")]
    public sealed class F2DFollowPath : MonoBehaviour
    {
        [SerializeField] private Transform m_Transform;
        public F2DFlyPath path;

        public float speed = 1;
        public float waitTime;
        private float m_Time;

        private int m_CurrentIndex;
        private int m_IndexDirection = 1;

        public bool looping;
        public bool randomSelection;

        private void Awake()
        {
            if (m_Transform == null)
            {
                m_Transform = base.transform;
            }

            if (path == null || path.localPositions == null) return;

            m_Transform.position = path.GetPosition(0);
        }

        private void Update()
        {
            if (path == null || path.localPositions == null) return;

            if (m_Time < waitTime)
            {
                m_Time += Time.deltaTime;
            }
            else
            {
                int positionCount = path.localPositions.Count;
                if (positionCount > 0)
                {
                    Vector3 target = path.GetPosition(m_CurrentIndex);
                    Vector3 position = m_Transform.position;
                    position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
                    m_Transform.position = position;

                    if (Vector3.Distance(position, target) < speed * Time.deltaTime)
                    {
                        m_Time = 0;
                        Next();
                    }
                }
            }
        }

        private void Next()
        {
            if (randomSelection)
            {
                int nextIndex = m_CurrentIndex;

                while (m_CurrentIndex == nextIndex)
                {
                    nextIndex = Random.Range(0, path.PositionCount - 1);
                }

                m_CurrentIndex = nextIndex;
            }
            else 
            if (looping)
            {
                m_CurrentIndex = Mod.get(m_CurrentIndex + 1, path.PositionCount);
            }
            else
            {
                if (m_CurrentIndex == path.PositionCount - 1)
                {
                    m_IndexDirection = -1;
                }
                else if (m_CurrentIndex == 0)
                {
                    m_IndexDirection = 1;
                }

                m_CurrentIndex += m_IndexDirection;
            }
        }
    }
}