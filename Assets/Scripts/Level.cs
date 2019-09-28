using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public GameObject map, enemy, obstacle;
    [Header("關卡大小")]
    public int X, Y;
    [Header("關卡接口")]
    public Direction[] directions;
    [HideInInspector]
    public Vector2Int PosInArray = new Vector2Int(-1, -1);
    [HideInInspector]
    public int connection = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
