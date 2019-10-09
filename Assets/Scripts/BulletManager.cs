using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;

public class BulletManager : MonoBehaviour
{
    private GameObject dummy;
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

    public IEnumerator SpawnPattern(BulletPattern pattern, Vector2 pos, Quaternion quaternion)
    {
        GameObject newDummy = Instantiate(dummy, pos, Quaternion.identity);
        newDummy.transform.rotation = Quaternion.Euler(new Vector3(0, 0, quaternion.eulerAngles.z));
        Transform parent = newDummy.transform;
        Rigidbody2D rb = newDummy.AddComponent<Rigidbody2D>();
        List<GameObject> bullets = new List<GameObject>();
        rb.gravityScale = 0;
        float timer = 0;
        float vRotate = quaternion.eulerAngles.z * Mathf.Deg2Rad;
        float selfRotate = newDummy.transform.rotation.eulerAngles.z;
        float speed = 0;
        foreach (SpawnData spawn in pattern.spawns)
        {
            float newRotate = vRotate + Vector2.SignedAngle(Vector2.right,spawn.position) * Mathf.Deg2Rad;
            GameObject newBullet = bulletDictionary[spawn.name].Dequeue();
            newBullet.transform.localScale = new Vector2(1, 1);
            newBullet.transform.SetParent(parent);
            newBullet.transform.position = new Vector2(spawn.position.magnitude * Mathf.Cos(newRotate), spawn.position.magnitude * Mathf.Sin(newRotate)) + pos;
            newBullet.transform.localRotation = Quaternion.identity;
            newBullet.SetActive(true);
            bullets.Add(newBullet);
            bulletDictionary[spawn.name].Enqueue(newBullet);
        }
        while (timer <= 10)
        {
            if (bullets.Count == 0)
                break;
            Debug.Log(vRotate);
            vRotate += pattern.vRotation.Evaluate(timer) * Mathf.Deg2Rad;
            speed = pattern.velocity.Evaluate(timer);
            rb.velocity = new Vector2(speed * Mathf.Cos(vRotate), speed * Mathf.Sin(vRotate));
            selfRotate += pattern.rotation.Evaluate(timer) * Time.deltaTime;
            newDummy.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selfRotate));
            newDummy.transform.localScale = new Vector2(1,1) * pattern.scale.Evaluate(timer);
            for(int i = 0; i < bullets.Count; i ++)
            {
                if (!bullets[i].activeSelf)
                {
                    bullets[i].transform.SetParent(null);
                    bullets.Remove(bullets[i]);
                    continue;
                }
                bullets[i].transform.localScale = new Vector2(1, 1) / pattern.scale.Evaluate(timer);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(newDummy);
    }
}
