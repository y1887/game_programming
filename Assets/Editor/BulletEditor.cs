using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BulletArray))]
public class BulletEditor : Editor
{
    private BulletArray bulletAR;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        bulletAR = (BulletArray)target;
        if (GUILayout.Button("Create Pattern"))
            bulletAR.CreatePattern();
    }
}
