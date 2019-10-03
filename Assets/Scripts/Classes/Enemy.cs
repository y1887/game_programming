using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTAI;
using Pathfinding;
using System.Linq;

public class Enemy : MonoBehaviour
{
    Root aiRoot = BT.Root();
    protected PlayerController player;
    protected Transform playerTransform;
    protected Transform enemyTransform;
    protected AIDestinationSetter destination;

    [SerializeField,Header("生命值"),Range(1,int.MaxValue)]
    protected int hp = 50;
    [SerializeField, Header("冷卻時間"), Range(0.5f, float.MaxValue)]
    protected float coolDown = 4f;
    protected float timer = 0;
    protected List<System.Action> Attacks;
    protected int current = 0;
    protected int atkPatternLen = 0;
    protected int[] pattern;

    //AI邏輯：
    //IF(怪物血量歸零) => 死亡function(放特效、音效，死亡時攻擊之類的)
    //ELSE IF(可以攻擊) => 攻擊
    //ELSE => 跟隨玩家
    private void OnEnable()
    {
        aiRoot.OpenBranch(
                BT.If(() => hp <= 0).OpenBranch(
                    BT.Call(OnDead)
                ),
                BT.If(CanAttack).OpenBranch(
                    BT.Call(Attack)
                ),
                BT.Call(Follow)
        );
    }

    protected virtual void Follow()//可以改寫
    {
        destination.enabled = true;
    }

    protected virtual void OnSpawn()//要自己寫，如果沒有特殊需求則維持不變
    {
        return;
    }

    protected virtual void OnDead()//要自己寫，如果沒有特殊需求則維持不變
    {
        return;
    }

    protected virtual void Attack()//要自己寫攻擊模式
    {
        Attacks[pattern[current]]?.Invoke();
        current++;
        if(current == atkPatternLen)
        {
            current = 0;
            PatternShuffle();
        }
        return;
    }

    protected virtual bool CanAttack()//可以改寫
    {
        //預設判斷：如果冷卻時間歸零且玩家與敵人間沒有障礙物
        RaycastHit2D raycast = Physics2D.Raycast(enemyTransform.position, playerTransform.position - enemyTransform.position, float.PositiveInfinity, 1 << 8 | 1 << 10);
        bool canATK = (raycast.collider.gameObject.layer == 10 && timer <= 0);
        if (canATK)
            timer = coolDown;
        return canATK;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerTransform = player.gameObject.transform.Find("Player");
        enemyTransform = this.transform.Find("Enemy");
        destination = enemyTransform.gameObject.GetComponent<AIDestinationSetter>();
        destination.target = playerTransform;
        atkPatternLen = Attacks.Count;
        current = 0;
        pattern = new int[atkPatternLen];
        PatternShuffle();
    }

    private void PatternShuffle() //洗牌
    {
        for(int i = 0; i < atkPatternLen; i ++)
            pattern[i] = i;
        for (int i = 0; i < atkPatternLen; i++)
        {
            int rand = UnityEngine.Random.Range(0, atkPatternLen);
            int temp = pattern[i];
            pattern[i] = pattern[rand];
            pattern[rand] = temp;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        aiRoot.Tick();
    }

    public Root GetAIRoot()
    {
        return aiRoot;
    }
}
