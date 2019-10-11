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
        public Vector3 parentPos;
        public float parentAngle;
        public float scale;

        public void Execute(int i, TransformAccess transform)
        {
            float newAngle = parentAngle + angle[i];
            Vector3 newDisance = new Vector3(distance[i] * Mathf.Cos(newAngle), distance[i] * Mathf.Sin(newAngle));
            transform.position = parentPos + (newDisance * scale); 
        }
    }

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
        Debug.Log(pattern.spawns.Count);
        JobHandle PositionJobHandle;
        Transform[] temp = new Transform[length];
        NativeArray<float> distance = new NativeArray<float>(length, Allocator.Persistent), angle = new NativeArray<float>(length, Allocator.Persistent);
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
            j++;
        }
        TransformAccessArray transforms = new TransformAccessArray(temp);
        float tempRotate = vRotate;
        while (timer <= 10)
        {
            if (bullets.Count == 0)
                break; 
            vRotate = tempRotate + pattern.vRotation.Evaluate(timer) * Mathf.Deg2Rad;
            speed = pattern.velocity.Evaluate(timer);
            rb.velocity = new Vector2(speed * Mathf.Cos(vRotate), speed * Mathf.Sin(vRotate));
            selfRotate += pattern.rotation.Evaluate(timer) * Time.deltaTime;
            newDummy.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selfRotate));
            PositionUpdateJob m_Job = new PositionUpdateJob()
            {
                distance = distance,
                angle = angle,
                scale = pattern.scale.Evaluate(timer),
                parentPos = parent.position,
                parentAngle = parent.rotation.eulerAngles.z * Mathf.Deg2Rad
            };
            PositionJobHandle = m_Job.Schedule(transforms);
            for (int i = 0; i < bullets.Count; i ++)
            {
                if (!bullets[i].activeSelf)
                {
                    bullets.Remove(bullets[i]);
                    currentBulletAmount--;
                    continue;
                }
            }
            timer += Time.deltaTime;
            yield return null;
            PositionJobHandle.Complete();
        }
        for (int i = 0; i < bullets.Count; i++)
        {
            bullets[i].SetActive(false);
            currentBulletAmount--;
        }
        Destroy(newDummy);
        transforms.Dispose();
        distance.Dispose();
        angle.Dispose();
    }
}
