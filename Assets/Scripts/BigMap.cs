using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigMap : MonoBehaviour
{
    private LevelGenerator generator = null;

    // Start is called before the first frame update
    void Start()
    {
        generator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<LevelGenerator>();
        if (generator != null)
            StartCoroutine(FindPosition());
    }

    IEnumerator FindPosition()
    {
        yield return new WaitUntil(() => (int)generator.currentStatus >= (int)LevelGenerator.GeneratingStatus.Final_Makeup);
        Vector2 pos = generator.BigMapPos();
        this.transform.position = new Vector3(pos.x - 500, pos.y, this.transform.position.z);
    }
}
