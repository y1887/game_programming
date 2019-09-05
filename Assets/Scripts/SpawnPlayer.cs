using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    private LevelGenerator generator = null;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        generator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponentInParent<LevelGenerator>();
        if (generator != null)
            Instantiate(player, this.transform.position, Quaternion.identity);
    }
}
