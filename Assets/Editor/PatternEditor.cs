using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BulletCreator;

public enum BulletType
{
    none = 0,
    pistol = 1,
    rifle = 2,
    shotgun = 3,
    laser = 4,
    enemies = 5
}

[CustomEditor(typeof(BulletPattern))]
public class PatternEditor : Editor
{
    private int bCount;
    private int eCount;
    private float radius;
    [SerializeField]
    private Vector2 from, to;

    private readonly BulletArray bulletAR = BulletArray.instance;
    private Dictionary<GameObject, SpawnData> dict;
    private List<GameObject> list;
    private BulletType index = 0;
    private int bulletNum = 0;
    private GameObject selectedPrefab;
    private string[] P, R, S, L, E;
    private bool showCircle, showPolygon, showLine;
    BulletPattern Inspecting;

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
        index = 0;
        Inspecting = (BulletPattern)target;
        dict = new Dictionary<GameObject, SpawnData>();
        list = new List<GameObject>();
        selectedPrefab = null;
        showCircle = false; showPolygon = false; showLine = false;
        foreach (SpawnData data in Inspecting.spawns)
        {
            GameObject newBullet = Instantiate(bulletAR.bulletDictionary[data.name], data.position, Quaternion.identity);
            dict.Add(newBullet, data);
            list.Add(newBullet);
        }
        P = new string[bulletAR.pistol.Count + 1];
        P[0] = "None";
        for (int i = 0; i < bulletAR.pistol.Count; i++)
            P[i + 1] = bulletAR.pistol[i].name;
        R = new string[bulletAR.rifle.Count + 1];
        R[0] = "None";
        for (int i = 0; i < bulletAR.rifle.Count; i++)
            R[i + 1] = bulletAR.rifle[i].name;
        S = new string[bulletAR.shotgun.Count + 1];
        S[0] = "None";
        for (int i = 0; i < bulletAR.shotgun.Count; i++)
            S[i + 1] = bulletAR.shotgun[i].name;
        L = new string[bulletAR.laser.Count + 1];
        L[0] = "None";
        for (int i = 0; i < bulletAR.laser.Count; i++)
            L[i + 1] = bulletAR.laser[i].name;
        E = new string[bulletAR.enemies.Count + 1];
        E[0] = "None";
        for (int i = 0; i < bulletAR.enemies.Count; i++)
            E[i + 1] = bulletAR.enemies[i].name;
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        selectedPrefab = null;
        while(list.Count != 0)
        {
            GameObject toDestroy = list[0];
            list.RemoveAt(0);
            DestroyImmediate(toDestroy);
        }
        dict.Clear();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.BeginHorizontal();
        index = (BulletType)EditorGUILayout.EnumPopup("Bullet Type : ", index);
        switch (index)
        {
            case BulletType.none:
                selectedPrefab = null;
                break;
            case BulletType.pistol:
                bulletNum = EditorGUILayout.Popup(bulletNum, P);
                selectedPrefab = bulletNum > 0 ? bulletAR.pistol[bulletNum - 1].gameObject : null;
                break;
            case BulletType.rifle:
                bulletNum = EditorGUILayout.Popup(bulletNum, R);
                selectedPrefab = bulletNum > 0 ? bulletAR.rifle[bulletNum - 1].gameObject : null;
                break;
            case BulletType.shotgun:
                bulletNum = EditorGUILayout.Popup(bulletNum, S);
                selectedPrefab = bulletNum > 0 ? bulletAR.shotgun[bulletNum - 1].gameObject : null;
                break;
            case BulletType.laser:
                bulletNum = EditorGUILayout.Popup(bulletNum, L);
                selectedPrefab = bulletNum > 0 ? bulletAR.laser[bulletNum - 1].gameObject : null;
                break;
            case BulletType.enemies:
                bulletNum = EditorGUILayout.Popup(bulletNum, E);
                selectedPrefab = bulletNum > 0 ? bulletAR.enemies[bulletNum - 1].gameObject : null;
                break;
            default:
                Debug.LogWarning("Undefined category.");
                selectedPrefab = null;
                break;
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        bCount = EditorGUILayout.IntSlider("Bullets", bCount, 1, 60);
        eCount = EditorGUILayout.IntSlider("Edges", eCount, 3, 8);
        radius = EditorGUILayout.Slider("Radius",radius, 1, 10);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        from = EditorGUILayout.Vector2Field("From", from);
        to = EditorGUILayout.Vector2Field("To", to);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("DrawCircle"))
            DrawCircle();
        showCircle = GUILayout.Toggle(showCircle, "Show?");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("DrawPolygon"))
            DrawPolygon();
        showPolygon = GUILayout.Toggle(showPolygon, "Show?");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("DrawLine"))
            DrawLine(from,to);
        showLine = GUILayout.Toggle(showLine, "Show?");
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Undo"))
            Undo.PerformUndo();
        if (GUILayout.Button("Redo"))
            Undo.PerformRedo();
    }

    public void DrawCircle()
    {
        if (selectedPrefab == null)
            return;
        for (int i = 0; i < bCount; i++)
        {
            float degree = 360 * ((float)i / (float)bCount) * Mathf.Deg2Rad;
            Vector2 pos = new Vector2(radius * Mathf.Cos(degree), radius * Mathf.Sin(degree));
            GameObject bullet = Instantiate(selectedPrefab, pos, Quaternion.identity);
            SpawnData newSpawn = new SpawnData();
            newSpawn.name = selectedPrefab.name;
            newSpawn.position = pos;
            dict.Add(bullet, newSpawn);
            Inspecting.spawns.Add(newSpawn);
            list.Add(bullet);
        }
    }

    public void DrawPolygon()
    {
        if (selectedPrefab == null)
            return;
        Vector2 a = new Vector2(radius, 0), b;
        for (int i = 1; i <= eCount; i++)
        {
            float degree = 360 * ((float)i / (float)eCount) * Mathf.Deg2Rad;
            b = new Vector2(radius * Mathf.Cos(degree), radius * Mathf.Sin(degree));
            DrawLine(a, b);
            a = b;
        }
    }

    public void DrawLine(Vector2 a, Vector2 b)
    {
        if (selectedPrefab == null)
            return;
        for (int i = 0; i < bCount; i++)
        {
            float lerpValue = (float)i / (float)(bCount - 1);
            Vector2 pos = Vector2.Lerp(a,b,lerpValue);
            GameObject bullet = Instantiate(selectedPrefab, pos, Quaternion.identity);
            SpawnData newSpawn = new SpawnData();
            newSpawn.name = selectedPrefab.name;
            newSpawn.position = pos;
            dict.Add(bullet, newSpawn);
            Inspecting.spawns.Add(newSpawn);
            list.Add(bullet);
        }
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        
        Inspecting = (BulletPattern)target;
        if(showCircle)
        {
            Handles.color = new Color(1, 1, 1, 0.4f);
            Handles.DrawWireDisc(Vector2.zero, Vector3.back, radius);
            Handles.color = new Color(0,1,1,0.4f);
            for (int i = 0; i < bCount; i++)
            {
                float degree = 360 * ((float)i / (float)bCount) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(radius * Mathf.Cos(degree), radius * Mathf.Sin(degree));
                Handles.DrawSolidDisc(pos, Vector3.back, 0.35f);
            }
        }
        if (showPolygon)
        {
            Vector2 a = new Vector2(radius, 0), b;
            for (int i = 1; i <= eCount; i++)
            {
                Handles.color = new Color(1, 1, 1, 0.4f);
                float degree = 360 * ((float)i / (float)eCount) * Mathf.Deg2Rad;
                b = new Vector2(radius * Mathf.Cos(degree), radius * Mathf.Sin(degree));
                Handles.DrawLine(a, b);
                Handles.color = new Color(0, 1, 1, 0.4f);
                for (int j = 0; j < bCount; j++)
                {
                    float lerpValue = (float)j / (float)(bCount);
                    Vector2 pos = Vector2.Lerp(a, b, lerpValue);
                    Handles.DrawSolidDisc(pos, Vector3.back, 0.35f);
                }
                a = b;
            }
        }
        if (showLine)
        {
            Handles.color = new Color(1, 1, 1, 0.4f);
            Handles.DrawLine(from, to);
            Handles.color = new Color(0, 1, 1, 0.4f);
            for (int i = 0; i < bCount; i++)
            {
                float lerpValue = (float)i / (float)(bCount - 1);
                Vector2 pos = Vector2.Lerp(from, to, lerpValue);
                Handles.DrawSolidDisc(pos, Vector3.back, 0.35f);
            }
        }
        SceneView.RepaintAll();
    }
}
