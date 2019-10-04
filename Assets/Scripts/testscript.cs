using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : Enemy
{
    protected override void Start()
    {
        base.Start();
        Attacks.Add(attack1);
        atkPatternLen = Attacks.Count;
        current = 0;
        pattern = new int[atkPatternLen];
        PatternShuffle();
    }

    public void attack1()
    {
        Debug.Log("Pattern 1");
        hp -= 10;
    }

    protected override void OnDead()
    {
        Debug.Log("The OnDead method is overrided");
        Destroy(this.gameObject);
    }

}
