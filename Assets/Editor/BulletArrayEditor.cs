using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BulletArray))]
public class BulletArrayEditor : Editor
{
    private void OnDisable()
    {
        BulletArray Inspecting = (BulletArray)target;
        Inspecting.DictIO();
    }
}
