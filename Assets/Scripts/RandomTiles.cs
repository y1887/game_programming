using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTiles : MonoBehaviour
{
    public TileSpriteArray tile;
    [Header("隨機貼圖材質?")]
    public bool isRand;
    private SpriteRenderer sr;

    void Start()
    {
        float gap = UnityEngine.Random.Range(0f, 1f);
        if (isRand && gap > 0.7f)
        {
            sr = this.GetComponent<SpriteRenderer>();
            int i = UnityEngine.Random.Range(0, tile.Rand.Length);
            sr.sprite = tile.Rand[i];
        }
        Destroy(this);
    }
}
