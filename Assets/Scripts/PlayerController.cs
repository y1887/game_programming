using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D circle;
    private Animator anim;
    private SpriteRenderer sr;
    private Camera cam;

    private bool isSlashing = false;
    private Vector3 previousPos;

    [Header("走路速度"), Range(1f,10f)]
    public float speed = 5;
    [Header("突刺最大距離"), Range(5f,15f)]
    public float slashDis = 8;
    [HideInInspector]
    public Vector2 direction = Vector2.zero;
    [HideInInspector]
    public Vector3 movePos;
    public GameObject slashAnim;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        circle = this.GetComponent<CircleCollider2D>();
        anim = this.GetComponent<Animator>();
        sr = this.GetComponent<SpriteRenderer>();
        cam = Camera.main;
        previousPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        movePos = RayCasting();
        if (Input.GetMouseButtonDown(0) && isSlashing == false)
            StartCoroutine(Slash(movePos));
        if (isSlashing == false)
            rb.velocity = Moving();
        previousPos = this.transform.position;
        SetAnimAndFlip();
    }

    Vector2 Moving()
    {
        direction = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            direction += Vector2.up;
        if (Input.GetKey(KeyCode.S))
            direction += Vector2.down;
        if (Input.GetKey(KeyCode.A))
            direction += Vector2.left;
        if (Input.GetKey(KeyCode.D))
            direction += Vector2.right;
        return direction.normalized * speed;
    }

    void SetAnimAndFlip()
    {
        if (rb.velocity.x != 0)
            sr.flipX = ((rb.velocity.x > 0) ? false : true);
        anim.SetFloat("Velocity", rb.velocity.magnitude);
    }

    Vector3 RayCasting()
    {
        Vector3 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 realWorldPos = new Vector3(worldPos.x, worldPos.y, 0);
        Vector3 dir = (realWorldPos - circle.bounds.center).normalized;
        sr.flipX = ((dir.x > 0) ? false : true);
        float dis = Mathf.Min((realWorldPos - circle.bounds.center).magnitude, slashDis);
        RaycastHit2D hit = Physics2D.Raycast(circle.bounds.center, dir, dis, 1 << 8);
        if (hit.collider != null)
            dis = Mathf.Max(hit.distance - circle.radius * Mathf.Abs(1f / Mathf.Cos(Mathf.Deg2Rad * Vector2.Angle(dir, hit.normal))), 0);
        Vector3 pos = (rb.transform.position + dir * dis) - new Vector3(0, 0.5f, 0);
        return pos;
    }

    IEnumerator Slash(Vector3 pos)
    {
        pos += new Vector3(0, 0.5f, 0);
        float time = 0.1f, angle = (((pos - rb.transform.position).y >= 0)? Vector2.Angle(Vector2.right, (pos - rb.transform.position)) : -Vector2.Angle(Vector2.right, (pos - rb.transform.position)));
        isSlashing = true;
        time = 0.1f * Mathf.Min((pos - rb.transform.position).magnitude / slashDis, 1);
        GameObject slash = GameObject.Instantiate(slashAnim, (pos - new Vector3(0, 0.3f, 0) + rb.transform.position) / 2, Quaternion.Euler(0,0,135 + angle));
        rb.transform.DOMove(pos, time);
        yield return new WaitForSeconds(time);
        rb.velocity = Vector2.zero;
        isSlashing = false;
        yield return new WaitForSeconds(0.2f);
        Destroy(slash);
    }
}
