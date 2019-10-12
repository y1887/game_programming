using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Circle : Weapon
{
    //private Color srColor;
    //private SpriteRenderer sr;
    //private Animator anim;
    private Camera cam;
    private float fireTime;
    public GameObject bullet;
    [Range(10f,50f)]
    public float bulletSpeed = 20;
    [HideInInspector]
    public int currentAmmo;
    [HideInInspector]
    public bool canAttack = true;

    private void Start()
    {
        //sr = this.GetComponent<SpriteRenderer>();
        //srColor = sr.color;
        //anim = this.GetComponent<Animator>();
        currentAmmo = ammoCapacity;
        cam = Camera.main;
        if (fireRate > 0)
            fireTime = 1 / fireRate;
        else
            Debug.LogWarning("The fire rate should always be positive");
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
        float angle = (Vector2.SignedAngle(Vector3.right, mousePos - transform.position) + UnityEngine.Random.Range(-spread, spread)) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angle),Mathf.Sin(angle));
        GameObject newBullet = Instantiate(bullet, transform.position + dir.normalized * 0.6f, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir)));
        newBullet.GetComponent<Rigidbody2D>().velocity = dir.normalized * bulletSpeed;
        yield return new WaitForSeconds(fireTime);
        canAttack = true;
    }

    /*public override void SwitchIn()
    {
        sr.DOColor(srColor, 0.5f);
        anim.SetTrigger("In");
    }

    public override void SwitchOut()
    {
        sr.DOColor(Color.gray, 0.5f);
        anim.SetTrigger("Out");
    }*/

    /*public override void Special()
    {
        StartCoroutine(SpecialAtk());
        return;
    }*/

    /*IEnumerator SpecialAtk()
    {
        Vector3 mousePos = new Vector3(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
        Vector3 dir = mousePos - transform.position;
        this.transform.DOMove(this.transform.position + dir.normalized * 15, 3).SetEase(Ease.Linear);
        float Deg = (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir));
        for(int i = 0; i < 30; i ++)
        {
            Vector2 dirVect = new Vector2(Mathf.Cos(Deg * Mathf.Deg2Rad), Mathf.Sin(Deg * Mathf.Deg2Rad));
            GameObject newBullet = Instantiate(bullet, transform.position + new Vector3(dirVect.x,dirVect.y) * 0.6f, Quaternion.Euler(0, 0, Deg));
            newBullet.GetComponent<Rigidbody2D>().velocity = dirVect * bulletSpeed;
            newBullet = Instantiate(bullet, transform.position - new Vector3(dirVect.x, dirVect.y) * 0.6f, Quaternion.Euler(0, 0, Deg));
            newBullet.GetComponent<Rigidbody2D>().velocity = -dirVect * bulletSpeed;
            Deg += 10;
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(this.gameObject);
    }*/
}
