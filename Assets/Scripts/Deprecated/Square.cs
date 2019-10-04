using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Square : Weapon
{
    private Color srColor;
    private SpriteRenderer sr;
    private Animator anim;
    private Camera cam;
    private float fireTime;
    public GameObject bullet;
    [HideInInspector]
    public int currentAmmo;
    [HideInInspector]
    public bool canAttack = true;

    private void Start()
    {
        sr = this.GetComponent<SpriteRenderer>();
        srColor = sr.color;
        anim = this.GetComponent<Animator>();
        cam = Camera.main;
        currentAmmo = ammoCapacity;
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
        GameObject newBullet = Instantiate(bullet, transform.position + dir.normalized * 1.5f, Quaternion.Euler(0, 0, (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir))), this.transform);
        newBullet.transform.localScale = new Vector3(1 / 0.6f, 1 / 0.6f);
        yield return new WaitForSeconds(0.5f);
        newBullet.transform.parent = null;
        yield return new WaitForSeconds(fireTime - 0.5f);
        canAttack = true;
        yield return new WaitForSeconds(2);
        Destroy(newBullet);
    }

    public override void SwitchIn()
    {
        sr.DOColor(srColor,0.5f);
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
        Vector3 mousePos = new Vector3(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
        Vector3 dir = mousePos - transform.position;
        float Deg = dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir);
        Vector3 normDir = new Vector3(Mathf.Cos((90 + Deg) * Mathf.Deg2Rad), Mathf.Sin((90 + Deg) * Mathf.Deg2Rad));
        this.transform.DOMove(this.transform.position + dir.normalized * 6, 1).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(1);
        this.transform.DOShakePosition(1,0.3f).SetEase(Ease.InCubic);
        GameObject newBullet = Instantiate(bullet, transform.position + dir.normalized * 1.5f, Quaternion.Euler(0, 0, (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir))));
        newBullet.transform.localScale = new Vector3(1.5f, 1.5f);
        newBullet = Instantiate(bullet, transform.position - normDir * 3, Quaternion.Euler(0, 0, (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir))));
        newBullet.transform.localScale = new Vector3(1.5f, 1.5f);
        newBullet = Instantiate(bullet, transform.position + normDir * 3, Quaternion.Euler(0, 0, (dir.y < 0 ? -Vector2.Angle(Vector2.right, dir) : Vector2.Angle(Vector2.right, dir))));
        newBullet.transform.localScale = new Vector3(1.5f, 1.5f);
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }
}
