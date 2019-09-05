using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    private LevelGenerator generator = null;
    private GameObject player = null;

    // Start is called before the first frame update
    void Start()
    {
        generator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<LevelGenerator>();
        if (generator != null)
            StartCoroutine(FindPlayer());
        enabled = false;
    }

    private void LateUpdate()
    {
        Vector2 playerPos = player.transform.position;
        this.transform.position = new Vector3(playerPos.x - 500, playerPos.y, this.transform.position.z);
    }

    IEnumerator FindPlayer()
    {
        yield return new WaitUntil(() => (int)generator.currentStatus >= (int)LevelGenerator.GeneratingStatus.Final_Makeup);
        while (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }
        enabled = true;
    }
}
