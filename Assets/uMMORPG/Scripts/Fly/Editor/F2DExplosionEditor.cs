using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace ScriptBoy.Fly2D
{
    [CustomEditor(typeof(F2DExplosion))]
    public class F2DExplosionEditor : Editor
    {
        private F2DExplosion explosion;

        private void OnEnable()
        {
            explosion = target as F2DExplosion;
            if (explosion.name.StartsWith("GameObject")) explosion.name = "Explosion";
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Explode"))
            {
                //explosion.Explode();
            }
        }

    }
}
