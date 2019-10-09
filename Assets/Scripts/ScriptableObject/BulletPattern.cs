using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;

[CreateAssetMenu]
public class BulletPattern : ScriptableObject
{
    public AnimationCurve vRotation = new AnimationCurve();
    public AnimationCurve velocity = new AnimationCurve();
    public AnimationCurve rotation = new AnimationCurve();
    public AnimationCurve scale = new AnimationCurve();
    public List<SpawnData> spawns = new List<SpawnData>();
}
