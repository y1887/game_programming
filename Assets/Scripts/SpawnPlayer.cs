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
            StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        Debug.Log("Spawning");
        yield return new WaitUntil(() => (int)generator.currentStatus > (int)LevelGenerator.GeneratingStatus.Place_Levels);
        Instantiate(player, this.transform.position, Quaternion.identity);
    }
}
