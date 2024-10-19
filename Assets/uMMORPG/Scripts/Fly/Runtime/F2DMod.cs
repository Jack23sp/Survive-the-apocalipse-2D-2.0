using UnityEngine;

namespace ScriptBoy.Fly2D
{
    public static class Mod
    {
        public static int get(int a, int b)
        {
            return a - Mathf.FloorToInt((float)a / b) * b;
        }

        public static float get(float a, float b)
        {
            return a - Mathf.Floor(a / b) * b;
        }
    }
}