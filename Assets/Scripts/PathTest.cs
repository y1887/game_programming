using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathTest : MonoBehaviour
{
    public GameObject testDummy;
    public Transform player;
    public int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnDummy", 1, 0.5f);
    }

    void SpawnDummy()
    {
        GameObject newGO = Instantiate(testDummy, this.transform.position, Quaternion.identity);
        //newGO.GetComponent<AIDestinationSetter>().target = player;
        i++;
        if (i >= 20)
            Destroy(this);
    }
}
