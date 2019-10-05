using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletCreator;

/*public class BulletGroup : BulletPattern
{
    private Rigidbody2D rb = null;
    [SerializeField]
    public GameObject parent;
    public List<GameObject> bullets = new List<GameObject>();
    [SerializeField]
    public float damage;
    [SerializeField]
    public bool destroyAfterTime = false;
    [SerializeField]
    public float destroyTime;
    private float initialDeg;

    private void OnEnable()
    {
        parent = this.gameObject;
        rb = parent.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    private void Start()
    {
        if (rb == null)
        {
            rb = parent.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        initialDeg = parent.transform.rotation.z;
        StartCoroutine(BulletPath());
        if (destroyAfterTime)
            StartCoroutine(DelayDestroy());
    }

    public void RemoveBullet(GameObject bullet)
    {
        bullets.Remove(bullet);
        Destroy(bullet);
    }

    IEnumerator BulletPath()
    {
        velocity.postWrapMode = WrapMode.Loop;
        rotation.postWrapMode = WrapMode.Loop;
        float timer = 0;
        float vRotate = initialDeg * Mathf.Deg2Rad;
        float selfRotate = this.transform.rotation.z;
        float speed = 0;
        while (bullets.Count > 0)
        {
            vRotate += vRotation.Evaluate(timer) * Mathf.Deg2Rad;
            speed = velocity.Evaluate(timer);
            rb.velocity = new Vector2(speed * Mathf.Cos(vRotate), speed * Mathf.Sin(vRotate));
            selfRotate += rotation.Evaluate(timer) * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, selfRotate));
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(this.gameObject);
    }

    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(this.gameObject);
    }
}*/
