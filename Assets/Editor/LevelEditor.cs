using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
#if UNITY_EDITOR
    private Level level;
    private GameObject[] tiles, monsters;
    private string[] tileNames, monsterNames, categories = {"None", "Tiles", "Monsters"};

    private int category = 0, preCate = 0, index = 0, tileIndex = 0;
    private GameObject selectedPrefab;

    private void OnEnable()
    {
        level = (Level)target;
        category = 0;
        preCate = 0;
        index = 0;
        tileIndex = 0;
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

    private void OnSceneGUI()
    {
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
        RandomTiles randomTiles = null;
        if(selectedPrefab != null)
            randomTiles = selectedPrefab.GetComponent<RandomTiles>();
        GUILayout.BeginHorizontal();
        string boxstr = "Level Edit Mode : " + ((selectedPrefab == null) ? "No prefab selected" : selectedPrefab.name);
        GUILayout.Box(boxstr);
        if (randomTiles != null)
        {
            randomTiles.isRand = EditorGUILayout.Toggle("Random Tile?",randomTiles.isRand);
            if(randomTiles.isRand == false)
            {
                string[] str = new string[randomTiles.tile.All.Length];
                for (int i = 0; i < randomTiles.tile.All.Length; i++)
                    str[i] = randomTiles.tile.All[i].name;
                tileIndex = EditorGUILayout.Popup(tileIndex, str);
                selectedPrefab.GetComponent<SpriteRenderer>().sprite = randomTiles.tile.All[tileIndex];
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        Vector3 spawnPosition = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        Vector3 gridPosition = new Vector3(Mathf.Floor(spawnPosition.x) + 0.5f, Mathf.Floor(spawnPosition.y) + 0.5f, 0);

        GameObject inLayer; Transform pointed = null;
        if(category != 0)
        {
            if (category == 1)
                inLayer = level.map;
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

        if (pointed == null && Event.current.type == EventType.MouseDown && Event.current.button == 0 && selectedPrefab != null) //滑鼠左鍵點擊生成
        {
            if (category == 1)
                Instantiate(selectedPrefab, gridPosition, Quaternion.identity, level.map.transform);
            else
                Instantiate(selectedPrefab, gridPosition, Quaternion.identity, level.enemy.transform);
        }
        else if (Event.current.type == EventType.MouseDown && Event.current.button == 1) //滑鼠右鍵點擊退出生成
        {
            index = 0;
            selectedPrefab = null;
        }
        else if (pointed != null && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.X) //按X把滑鼠指到的東西刪掉(如果有指到東西的話)
            DestroyImmediate(pointed.gameObject);
    }
#endif
}
