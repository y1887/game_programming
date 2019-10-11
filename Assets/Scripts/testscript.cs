using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : Enemy
{
    private BulletManager manager;
    public BulletPattern pattern1;
    protected override void Start()
    {
        base.Start();
        manager = BulletManager.instance;
        Attacks.Add(attack1);
        atkPatternLen = Attacks.Count;
        current = 0;
        pattern = new int[atkPatternLen];
        PatternShuffle();
    }

    public void attack1()
    {
        float angle = Vector2.SignedAngle(Vector2.right, playerTransform.transform.position - enemyTransform.transform.position);
        manager.StartCoroutine(manager.SpawnPattern(pattern1, this.transform.position, Quaternion.Euler(0, 0, angle)));
    }

    protected override void Follow()
    {
        base.Follow();
        Vector3 from = enemyTransform.transform.up;
        Vector3 to = enemyTransform.transform.position - playerTransform.transform.position;
        enemyTransform.transform.up = Vector3.Lerp(from,to,0.01f);
    }

    protected override void OnDead()
    {
        Debug.Log("The OnDead method is overrided");
        Destroy(this.gameObject);
    }

}
