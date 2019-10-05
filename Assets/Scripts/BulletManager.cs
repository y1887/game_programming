using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;

public class BulletManager : MonoBehaviour
{
    private GameObject dummy = new GameObject();
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
        foreach (Bullet bullet in bulletArray.enemies)
        {
            Queue<GameObject> bulletPool = new Queue<GameObject>();
            for (int i = 0; i < bullet.size; i++)
            {
                GameObject newBulet = Instantiate(bullet.gameObject);
                newBulet.SetActive(false);
                bulletPool.Enqueue(newBulet);
            }
            bulletDictionary.Add(bullet.name, bulletPool);
        }
    }

    public IEnumerator SpawnPattern(BulletPattern pattern, Vector2 pos, Quaternion quaternion)
    {
        GameObject newDummy = Instantiate(dummy, pos, quaternion);
        Transform parent = newDummy.transform;
        Rigidbody2D rb = newDummy.AddComponent<Rigidbody2D>();
        List<GameObject> bullets = new List<GameObject>();
        rb.gravityScale = 0;
        foreach(SpawnData spawn in pattern.spawns)
        {
            GameObject newBullet = bulletDictionary[spawn.name].Dequeue();
            newBullet.transform.SetParent(parent);
            newBullet.transform.localPosition = spawn.position;
            newBullet.SetActive(true);
            bullets.Add(newBullet);
            bulletDictionary[spawn.name].Enqueue(newBullet);
        }
        float timer = 0;
        float vRotate = quaternion.z * Mathf.Deg2Rad;
        float selfRotate = this.transform.rotation.z;
        float speed = 0;
        float previousScale = 1;
        float scale = 1;
        while (timer <= 10)
        {
            vRotate += pattern.vRotation.Evaluate(timer) * Mathf.Deg2Rad;
            speed = pattern.velocity.Evaluate(timer);
            rb.velocity = new Vector2(speed * Mathf.Cos(vRotate), speed * Mathf.Sin(vRotate));
            selfRotate += pattern.rotation.Evaluate(timer) * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selfRotate));
            scale = pattern.scale.Evaluate(timer);
            foreach (GameObject bullet in bullets)
            {
                if (!bullet.activeSelf)
                {
                    bullets.Remove(bullet);
                    continue;
                }
                bullet.transform.localPosition /= previousScale;
                bullet.transform.localPosition *= scale;
            }
            previousScale = scale;
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(newDummy);
    }
}
