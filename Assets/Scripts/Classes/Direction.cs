using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Direction
{
    public enum DIR
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }
    [Header("接口方向")]
    public DIR direction = 0;
    [Header("接口位置")]
    public int position = 0;
}
