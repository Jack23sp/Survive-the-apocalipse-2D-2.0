using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptBoy.Fly2D
{
    public static class Quad
    {
        private static Mesh m_Mesh;

        public static Mesh mesh
        {
            get
            {
                if (m_Mesh == null)
                {
                    m_Mesh = new Mesh();
                    m_Mesh.vertices = vertices;
                    m_Mesh.uv = uv;
                    m_Mesh.triangles = triangles;
                }

                return m_Mesh;
            }
        }

        // v1_________v2
        //  |         |
        //  |         |
        //  |         |
        //  |_________|
        // v0         v3

        public static Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f),
            new Vector3(0.5f, 0.5f),
            new Vector3(0.5f, -0.5f)
        };

        public static Vector2[] uv = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        public static int[] triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
    }
}