using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletCreator
{
    [System.Serializable]
    public class Bullet
    {
        public GameObject gameObject;
        public string name { get { return gameObject.name; } set { name = value; } }
        public int size;
    }

    [System.Serializable]
    public class SpawnData
    {
        public string name;
        public Vector2 position;
    }
}
