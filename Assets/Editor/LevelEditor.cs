using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    private Level level;
    private GameObject[] tiles, monsters;
    private string[] tileNames, monsterNames, categories = {"None", "Tiles", "Monsters"};

    private int category = 0, preCate = 0, index = 0, degree = 0;
    private GameObject selectedPrefab;

    private void OnEnable()
    {
        level = (Level)target;
        category = 0;
        preCate = 0;
        index = 0;
        degree = 0;
        selectedPrefab = null;
        tiles = Resources.LoadAll<GameObject>("Prefab(LE)/Tiles");
        monsters = Resources.LoadAll<GameObject>("Prefab(LE)/Monsters");
        tileNames = new string[tiles.Length + 1];
        monsterNames = new string[monsters.Length + 1];
        tileNames[0] = "None";
        monsterNames[0] = "None";
        for (int i = 0; i < tiles.Length; i++)
            tileNames[i + 1] = tiles[i].name;
        for (int i = 0; i < monsters.Length; i++)
            monsterNames[i + 1] = monsters[i].name;
    }

    public void OnSceneGUI()
    {
        level = (Level)target;
        
        float halfX = ((float)level.X) * 2f;
        float halfY = ((float)level.Y) * 2f;
        float posX = level.transform.position.x;
        float posY = level.transform.position.y;
        DrawRect(posX, posY, halfX, halfY, Color.red);
        foreach(Direction dir in level.directions)
        {
            switch(dir.direction)
            {
                case Direction.DIR.Up:
                    posX = level.transform.position.x + (4f * dir.position + 2f) - ((float)level.X) * 2f;
                    posY = level.transform.position.y + ((float)level.Y) * 2f + 0.5f;
                    halfX = 2f;
                    halfY = 0.5f;
                    break;
                case Direction.DIR.Down:
                    posX = level.transform.position.x + (4f * dir.position + 2f) - ((float)level.X) * 2f;
                    posY = level.transform.position.y - ((float)level.Y) * 2f - 0.5f;
                    halfX = 2f;
                    halfY = 0.5f;
                    break;
                case Direction.DIR.Left:
                    posX = level.transform.position.x - ((float)level.X) * 2f - 0.5f;
                    posY = level.transform.position.y - (4f * dir.position + 2f) + ((float)level.Y) * 2f;
                    halfX = 0.5f;
                    halfY = 2f;
                    break;
                case Direction.DIR.Right:
                    posX = level.transform.position.x + ((float)level.X) * 2f + 0.5f;
                    posY = level.transform.position.y - (4f * dir.position + 2f) + ((float)level.Y) * 2f;
                    halfX = 0.5f;
                    halfY = 2f;
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
            DrawRect(posX, posY, halfX, halfY, Color.blue);
        }
        /*此段以上負責畫關卡邊界*/

        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.BeginHorizontal();
        category = EditorGUILayout.Popup(category, categories);
        if (preCate != category)
        {
            index = 0;
            preCate = category;
        }
        switch (category)
        {
            case 1:
                index = EditorGUILayout.Popup(index, tileNames);
                selectedPrefab = ((index > 0) ? tiles[index - 1] : null);
                break;
            case 2:
                index = EditorGUILayout.Popup(index, monsterNames);
                selectedPrefab = ((index > 0) ? monsters[index - 1] : null);
                break;
            default:
                break;
        }
        GUILayout.EndHorizontal();
        string boxstr = "Level Edit Mode : " + ((selectedPrefab == null) ? "No prefab selected" : selectedPrefab.name);
        GUILayout.Box(boxstr);
        string rotatestr = "Rotation : " + degree;
        GUILayout.Box(rotatestr);
        GUILayout.EndArea();
        /*此段以上負責生成SceneGUI*/

        level.map.transform.localScale = new Vector3(4 * level.X, 4 * level.Y);
        Vector3 spawnPosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        Vector3 gridPosition = new Vector3(Mathf.Floor(spawnPosition.x) + 0.5f, Mathf.Floor(spawnPosition.y) + 0.5f, 0);

        GameObject inLayer; Transform pointed = null;
        if(category != 0)
        {
            if (category == 1)
                inLayer = level.obstacle;
            else
                inLayer = level.enemy;
            foreach (Transform obj in inLayer.transform)
            {
                if (obj.position == gridPosition)
                {
                    pointed = obj;
                }
            }
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Z)
            degree = (degree == 0 ? 270 : degree - 90);
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.C)
            degree = (degree == 270 ? 0 : degree + 90);

        if (pointed == null && Event.current.type == EventType.MouseDown && Event.current.button == 0 && selectedPrefab != null) //滑鼠左鍵點擊生成
        {
            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            newObject.transform.position = gridPosition;
            if (category == 1)
            {
                newObject.transform.SetParent(level.obstacle.transform);
                newObject.transform.rotation = Quaternion.Euler(0, 0, degree);
            }
            else
                newObject.transform.SetParent(level.enemy.transform);
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 1) //滑鼠右鍵點擊退出生成
        {
            index = 0;
            selectedPrefab = null;
        }
        else if (pointed != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.X) //按X把滑鼠指到的東西刪掉(如果有指到東西的話)
            DestroyImmediate(pointed.gameObject);
            
        /*此段以上負責在Scene內生成Prefab*/

        SceneView.RepaintAll();
    }

    private void OnDisable()
    {
        level = (Level)target;
        EditorUtility.SetDirty(level.gameObject);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        level = (Level)target;
        foreach (Direction dir in level.directions)
        {
            dir.direction = (Direction.DIR)EditorGUILayout.EnumPopup(dir.direction);
            if (dir.direction == Direction.DIR.Up || dir.direction == Direction.DIR.Down)
                dir.position = EditorGUILayout.IntSlider(dir.position, 0, level.X - 1);
            else
                dir.position = EditorGUILayout.IntSlider(dir.position, 0, level.Y - 1);
        }
    }

    void DrawRect(float posX, float posY, float halfX, float halfY, Color color)
    {
        Vector3[] verts = new Vector3[]
        {
            new Vector3(posX - halfX, posY + halfY, 0),
            new Vector3(posX - halfX, posY - halfY, 0),
            new Vector3(posX + halfX, posY - halfY, 0),
            new Vector3(posX + halfX, posY + halfY, 0)
        };
        Handles.DrawSolidRectangleWithOutline(verts, new Color(color.r, color.g, color.b, 0.1f), color);
    }
}
