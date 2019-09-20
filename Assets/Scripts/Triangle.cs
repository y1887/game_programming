using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Triangle : Weapon
{
    private Color srColor;
    private SpriteRenderer sr;
    private Animator anim;
    private Camera cam;
    private float fireTime;
    public GameObject bullet;
    [Range(10f, 50f)]
    public float bulletSpeed = 20;
    [Range(3, 7)]
    public int bulletAmount = 4;
    [Range(5f, 10f)]
    public float spreadDeg = 7.5f;
    [HideInInspector]
    public int currentAmmo;
    [HideInInspector]
    public bool canAttack = true;

    private void Start()
    {
        sr = this.GetComponent<SpriteRenderer>();
        srColor = sr.color;
        anim = this.GetComponent<Animator>();
        currentAmmo = ammoCapacity;
        cam = Camera.main;
        if (fireRate > 0)
            fireTime = 1 / fireRate;
        else
            Debug.LogWarning("The fire rate should always be positive");
    }

    private void Update()
    {
        Vector3 mousePos = new Vector3(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
        Vector3 dir = mousePos - transform.position;
        transform.rotation = Quaternion.Euler(0, 0, (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir)));
    }

    public override void Attack()
    {
        if (fireRate > 0 && canAttack && currentAmmo > 0)
            StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        canAttack = false;
        Vector3 mousePos = new Vector3(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
        Vector3 dir = mousePos - transform.position;
        GameObject[] toDestroy = new GameObject[bulletAmount];
        float totalDeg = spreadDeg * (float)(bulletAmount - 1);
        float Deg = -90 + (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir)) - (0.5f * totalDeg); 
        for (int i = 0; i < bulletAmount; i ++)
        {
            GameObject newBullet = Instantiate(bullet, transform.position + dir.normalized * 0.6f, Quaternion.Euler(0, 0, Deg));
            Vector2 dirVect = new Vector2(Mathf.Cos((90 + Deg) * Mathf.Deg2Rad), Mathf.Sin((90 + Deg) * Mathf.Deg2Rad));
            newBullet.GetComponent<Rigidbody2D>().velocity = dirVect.normalized * bulletSpeed;
            Deg += spreadDeg;
            toDestroy[i] = newBullet;
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(fireTime);
        canAttack = true;
        yield return new WaitForSeconds(2);
        foreach(GameObject theBullet in toDestroy)
            Destroy(theBullet);
    }

    public override void SwitchIn()
    {
        sr.DOColor(srColor, 0.5f);
        anim.SetTrigger("In");
    }

    public override void SwitchOut()
    {
        sr.DOColor(Color.gray, 0.5f);
        anim.SetTrigger("Out");
    }

    public override void Special()
    {
        enabled = false;
        StartCoroutine(SpecialAtk());
        return;
    }

    IEnumerator SpecialAtk()
    {
        canAttack = false;
        Vector3 mousePos = new Vector3(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
        Vector3 dir = mousePos - transform.position;
        float totalDeg = spreadDeg * (float)(bulletAmount - 1);
        float Deg = -90 + (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir)) - (0.5f * totalDeg);
        float tempDeg = Deg;
        this.transform.DOMove(this.transform.position + dir.normalized * 6, 1).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1);
        for (int j = 0; j < 3; j ++)
        {
            Deg = tempDeg + (j % 2 == 0? 0 : totalDeg);
            for (int i = 0; i < bulletAmount + j; i++)
            {
                GameObject newBullet = Instantiate(bullet, transform.position + dir.normalized * 0.6f, Quaternion.Euler(0, 0, Deg));
                Vector2 dirVect = new Vector2(Mathf.Cos((90 + Deg) * Mathf.Deg2Rad), Mathf.Sin((90 + Deg) * Mathf.Deg2Rad));
                
                Deg += (j % 2 == 0 ? spreadDeg : -spreadDeg);
                if(j == 2)
                {
                    this.transform.DOShakePosition(0.2f, 0.3f).SetEase(Ease.OutCubic);
                    newBullet.transform.localScale = new Vector3(0.8f, 2);
                    newBullet.GetComponent<Rigidbody2D>().velocity = dirVect.normalized * bulletSpeed * 1.5f;
                }
                else
                    newBullet.GetComponent<Rigidbody2D>().velocity = dirVect.normalized * bulletSpeed;
                yield return new WaitForEndOfFrame();
            }
            this.transform.DOMove(this.transform.position - dir.normalized * 0.5f, 0.2f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(0.2f);
        }
        Destroy(this.gameObject);
    }
}
