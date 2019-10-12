using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTAI;
using Pathfinding;
using System.Linq;

public class Enemy : MonoBehaviour
{
    Root aiRoot = BT.Root();
    public PlayerController player;
    public Transform playerTransform;
    public Transform enemyTransform;
    protected AIDestinationSetter destination;

    [SerializeField,Header("生命值"),Range(1,1000)]
    protected int hp = 50;
    [SerializeField, Header("冷卻時間"), Range(0.5f, float.MaxValue)]
    protected float coolDown = 4f;
    public float timer = 0;
    [SerializeField]
    public List<System.Action> Attacks = new List<System.Action>();
    protected int current = 0;
    public int atkPatternLen = 0;
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
        if(Attacks[pattern[current]] != null)
            Attacks[pattern[current]].Invoke();
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

    protected virtual void Start()//需要override以獲取繼承此class的script所涵蓋的資料，關於如何override請參考底下的註解
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerTransform = player.gameObject.transform.Find("Player");
        enemyTransform = this.transform;
        destination = enemyTransform.gameObject.GetComponent<AIDestinationSetter>();
        destination.target = playerTransform;
        timer = coolDown;
    }
    /* override範例
     
        public class test : Enemy <--- 這裡要改成繼承Enemy而不是MonoBehaviour
        {
            protected override void Start() <--- 這個start是在其他繼承Enemy這個class的script裡
            {
                base.Start();                       <--- base代表使用原本的code，只是後面會再額外加東西
                Attacks.Add(attack1);               <--- attack1是在test這個script裡的function，有幾種攻擊模式就要add幾次
                atkPatternLen = Attacks.Count;
                current = 0;
                pattern = new int[atkPatternLen];
                PatternShuffle();
            }

            void attack1()
            {
                //這裡寫攻擊模式
            }
            .
            .
            .
            下略
        }
     
    */

    protected void PatternShuffle() //洗牌
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(collision.gameObject);
            hp -= 10;
        }
    }

    public Root GetAIRoot()
    {
        return aiRoot;
    }
}
