using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;

[CreateAssetMenu]
[System.Serializable]
public class BulletArray : ScriptableObject
{
    public GameObject[] pistol;
    public GameObject[] rifle;
    public GameObject[] shotgun;
    public GameObject[] laser;
    public GameObject[] enemies;
    public GameObject[] patterns;

    public void CreatePattern()
    {
        GameObject newGO = new GameObject();
        newGO.name = "New Pattern";
        newGO.AddComponent<BulletPattern>();
        Instantiate(newGO, Vector3.zero, Quaternion.identity);
        return;
    }

    public void CreateDynamic()
    {
        return;
    }
}
