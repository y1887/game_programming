using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;

public class BulletManager : MonoBehaviour
{
    private GameObject dummy;
    [HideInInspector]
    public int currentBulletAmount;

    public static BulletManager instance = null;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    public Dictionary<string, Queue<GameObject>> bulletDictionary = new Dictionary<string, Queue<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        BulletArray bulletArray = BulletArray.instance;
        bulletArray.DictIO();
        bulletDictionary = new Dictionary<string, Queue<GameObject>>();
        dummy = new GameObject();
        foreach (Bullet bullet in bulletArray.enemies)
        {
            Queue<GameObject> bulletPool = new Queue<GameObject>();
            for (int i = 0; i < bullet.size; i++)
            {
                GameObject newBulet = Instantiate(bullet.gameObject);
                newBulet.name = bullet.name;
                newBulet.SetActive(false);
                bulletPool.Enqueue(newBulet);
            }
            bulletDictionary.Add(bullet.name, bulletPool);
        }
    }

    struct PositionUpdateJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<float> distance;
        [ReadOnly]
        public NativeArray<float> angle; //in radian format
        [ReadOnly]
        public NativeArray<bool> isActive;
        //public NativeArray<SpherecastCommand> commands;
        public Vector3 parentPos;
        public float parentAngle;
        public float scale;

        public void Execute(int i, TransformAccess transform)
        {
            if (!isActive[i])
                return;
            float newAngle = parentAngle + angle[i];
            Vector3 newDisance = new Vector3(distance[i] * Mathf.Cos(newAngle), distance[i] * Mathf.Sin(newAngle));
            transform.position = parentPos + (newDisance * scale);
            //commands[i] = new SpherecastCommand(transform.position, 0.8f, Vector3.zero, 0, 1 << 8);
        }
    }

    /*struct CollisionJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<RaycastHit> casts;
        public NativeArray<bool> isActive;

        public void Execute(int i)
        {
            Debug.Log(casts[i].point);
            if (isActive[i] == false)
                return;
            if (casts[i].point != Vector3.zero)
                isActive[i] = false;
        }
    }*/


    public IEnumerator SpawnPattern(BulletPattern pattern, Vector2 pos, Quaternion quaternion)
    {
        GameObject newDummy = Instantiate(dummy, pos, quaternion);
        Transform parent = newDummy.transform;
        Rigidbody2D rb = newDummy.AddComponent<Rigidbody2D>();
        List<GameObject> bullets = new List<GameObject>();
        rb.gravityScale = 0;
        float timer = 0;
        float vRotate = quaternion.eulerAngles.z * Mathf.Deg2Rad;
        float selfRotate = newDummy.transform.rotation.eulerAngles.z;
        float speed = 0;
        int j = 0, length = pattern.spawns.Count;
        currentBulletAmount += length;
        Transform[] temp = new Transform[length];
        JobHandle PositionJobHandle; //CastJobHandle;
        //NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(length, Allocator.Persistent);
        //NativeArray<SpherecastCommand> commands = new NativeArray<SpherecastCommand>(length, Allocator.Persistent);
        NativeArray<float> distance = new NativeArray<float>(length, Allocator.Persistent), angle = new NativeArray<float>(length, Allocator.Persistent);
        NativeArray<bool> isActive = new NativeArray<bool>(length, Allocator.Persistent);
        foreach (SpawnData spawn in pattern.spawns)
        {
            float newRotate = vRotate + Vector2.SignedAngle(Vector2.right,spawn.position) * Mathf.Deg2Rad;
            GameObject newBullet = bulletDictionary[spawn.name].Dequeue();
            newBullet.transform.position = new Vector2(spawn.position.magnitude * Mathf.Cos(newRotate), spawn.position.magnitude * Mathf.Sin(newRotate)) + pos;
            newBullet.SetActive(true);
            bullets.Add(newBullet);
            bulletDictionary[spawn.name].Enqueue(newBullet);
            distance[j] = spawn.position.magnitude;
            angle[j] = newRotate - vRotate;
            temp[j] = newBullet.transform;
            isActive[j] = true;
            j++;
        }
        TransformAccessArray transforms = new TransformAccessArray(temp);
        float tempRotate = vRotate;
        int templength = length;
        while (timer <= 10)
        {
            if (length == 0)
                break; 
            vRotate = tempRotate + pattern.vRotation.Evaluate(timer) * Mathf.Deg2Rad;
            speed = pattern.velocity.Evaluate(timer);
            rb.velocity = new Vector2(speed * Mathf.Cos(vRotate), speed * Mathf.Sin(vRotate));
            selfRotate += pattern.rotation.Evaluate(timer) * Time.deltaTime;
            newDummy.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selfRotate));
            PositionUpdateJob m_Job = new PositionUpdateJob()
            {
                isActive = isActive,
                distance = distance,
                angle = angle,
                scale = pattern.scale.Evaluate(timer),
                parentPos = parent.position,
                parentAngle = parent.rotation.eulerAngles.z * Mathf.Deg2Rad,
                //commands = commands
            };
            PositionJobHandle = m_Job.Schedule(transforms);
            PositionJobHandle.Complete();
            /*var handle = SpherecastCommand.ScheduleBatch(commands, results, templength);
            handle.Complete();
            CollisionJob m_collision = new CollisionJob()
            {
                casts = results,
                isActive = isActive
            };
            CastJobHandle = m_collision.Schedule(templength, 32);*/
            timer += Time.deltaTime;
            yield return null;
            /*CastJobHandle.Complete();*/
            for (int i = 0; i < bullets.Count; i++)
            {
                if (isActive[i] && bullets[i].activeSelf)
                {
                    isActive[i] = circleCast(bullets[i], bullets[i].transform.position, 0.3f);
                    length = (isActive[i] == false ? length - 1 : length);
                }
            }
        }
        if(length > 0)
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                if (bullets[i].activeSelf)
                {
                    bullets[i].SetActive(false);
                    currentBulletAmount = Mathf.Max(currentBulletAmount - 1, 0);
                }
            }
        }
        Destroy(newDummy);
        isActive.Dispose();
        transforms.Dispose();
        distance.Dispose();
        angle.Dispose();
        /*commands.Dispose();
        results.Dispose();*/
    }

    bool circleCast(GameObject go ,Vector2 pos, float radius)
    {
        RaycastHit2D hit = Physics2D.CircleCast(pos, radius, Vector2.zero, 0, 1 << 8);
        if (hit.collider != null)
        {
            go.SetActive(false);
            currentBulletAmount = Mathf.Max(currentBulletAmount - 1, 0);
            return false;
        }
        else
            return true;
    }
}
