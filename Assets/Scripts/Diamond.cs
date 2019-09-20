using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Diamond : Weapon
{
    private Camera cam;
    private Color srColor;
    private Animator anim;
    private float fireTime;
    private GameObject blast;
    private SpriteRenderer sr;
    private ParticleSystem blastParticle;
    [HideInInspector]
    public bool canAttack = true;

    private void Start()
    {
        cam = Camera.main;
        anim = this.GetComponent<Animator>();
        if (fireRate > 0)
            fireTime = 1 / fireRate;
        else
            Debug.LogWarning("The fire rate should always be positive");
        blast = this.transform.Find("Blast").gameObject;
        sr = this.GetComponent<SpriteRenderer>();
        srColor = sr.color;
        blastParticle = GetComponentInChildren<ParticleSystem>();
        blastParticle.Stop();
    }

    public override void Attack()
    {
        if (fireRate > 0 && canAttack)
            StartCoroutine(Shoot());
    }

    IEnumerator Shoot()
    {
        canAttack = false;
        blast.transform.DOShakePosition(1,0.75f,15);
        blast.transform.DOScale(new Vector3(12, 12, 0), 0.5f).SetEase(Ease.OutExpo);
        blastParticle.Play();
        yield return new WaitForSeconds(0.5f);
        blast.transform.DOScale(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(fireTime - 0.5f);
        canAttack = true;
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
        StartCoroutine(SpecialAtk());
        return;
    }

    IEnumerator SpecialAtk()
    {
        Vector3 mousePos = new Vector3(cam.ScreenToWorldPoint(Input.mousePosition).x, cam.ScreenToWorldPoint(Input.mousePosition).y);
        Vector3 dir = mousePos - transform.position;
        this.transform.DOMove(this.transform.position + dir.normalized * 12, 1).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(0.8f);
        this.transform.Find("BlastArea").gameObject.transform.DOScale(new Vector3(30, 30), 0.5f).SetEase(Ease.OutBounce);
        this.transform.Find("Mask").gameObject.transform.DOScale(new Vector3(29.5f, 29.5f), 0.5f).SetEase(Ease.OutBounce);
        this.transform.Find("BlastParticle").gameObject.transform.DOScale(new Vector3(2.5f, 2.5f), 0.5f);
        yield return new WaitForSeconds(0.5f);
        blast.transform.DOShakePosition(1, 0.75f, 15);
        blast.transform.DOScale(new Vector3(30, 30, 0), 0.5f).SetEase(Ease.OutExpo);
        blastParticle.Play();
        yield return new WaitForSeconds(0.5f);
        blast.transform.DOScale(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(0.6f);
        Destroy(this.gameObject);
    }
}
