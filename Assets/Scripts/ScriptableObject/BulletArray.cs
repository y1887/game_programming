using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;
using UnityEditor;

[CreateAssetMenu]
[System.Serializable]
public class BulletArray : ScriptableObject
{
    public Dictionary<string, GameObject> bulletDictionary = null;
    public static BulletArray instance = null;
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            bulletDictionary = new Dictionary<string, GameObject>();
            DictIO();
        }
        else
        {
            Debug.LogWarning("There should only be one BulletArray Asset in the project!");
            DestroyImmediate(this);
        }
    }

    public void DictIO()
    {
        foreach(Bullet bullet in pistol)
        {
            if (!bulletDictionary.ContainsKey(bullet.name))
                bulletDictionary.Add(bullet.name, bullet.gameObject);
        }
        foreach (Bullet bullet in rifle)
        {
            if (!bulletDictionary.ContainsKey(bullet.name))
                bulletDictionary.Add(bullet.name, bullet.gameObject);
        }
        foreach (Bullet bullet in shotgun)
        {
            if (!bulletDictionary.ContainsKey(bullet.name))
                bulletDictionary.Add(bullet.name, bullet.gameObject);
        }
        foreach (Bullet bullet in laser)
        {
            if (!bulletDictionary.ContainsKey(bullet.name))
                bulletDictionary.Add(bullet.name, bullet.gameObject);
        }
        foreach (Bullet bullet in enemies)
        {
            if (!bulletDictionary.ContainsKey(bullet.name))
                bulletDictionary.Add(bullet.name, bullet.gameObject);
        }
        foreach (string s in bulletDictionary.Keys)
        {
            if (bulletDictionary[s] == null)
                bulletDictionary.Remove(s);
        }
        EditorUtility.SetDirty(this);
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            bulletDictionary = null;
            instance = null;
        }
    }
    
    public List<Bullet> pistol = new List<Bullet>();
    public List<Bullet> rifle = new List<Bullet>();
    public List<Bullet> shotgun = new List<Bullet>();
    public List<Bullet> laser = new List<Bullet>();
    public List<Bullet> enemies = new List<Bullet>();
}
