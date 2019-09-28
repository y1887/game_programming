using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathTest : MonoBehaviour
{
    public GameObject testDummy;
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnDummy", 1, 0.5f);
    }

    void SpawnDummy()
    {
        GameObject newGO = Instantiate(testDummy, this.transform.position, Quaternion.identity);
        newGO.GetComponent<AIDestinationSetter>().target = player;
    }
}
