using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    private GameObject player, carrier;
    private Rigidbody2D rb;
    private CircleCollider2D circle;
    private Camera cam;

    private LinkedList<GameObject> satellites = new LinkedList<GameObject>();
    private LinkedList<Weapon> weapons = new LinkedList<Weapon>();
    private int currentSatIndex, previousSatIndex;

    private bool isShifting = false, canShift = true;
    private bool canShoot = true;
    [HideInInspector]
    public Vector2 direction = Vector2.zero;
    [Header("走路速度"), Range(1f,10f)]
    public float speed = 5;
    [Header("突刺最大距離"), Range(5f,15f)]
    public float shiftDis = 8;
    private Weapon selectedWeapon;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        player = transform.Find("Player").gameObject;
        carrier = transform.Find("Satellites").gameObject;
        rb = player.GetComponent<Rigidbody2D>();
        circle = player.GetComponent<CircleCollider2D>();
        currentSatIndex = 0;
        previousSatIndex = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Special();
        if (Input.GetMouseButton(0) && canShoot)
            Attack();
        if (Input.mouseScrollDelta.y != 0)
            ChangeWeapon(Input.mouseScrollDelta.y);
        if (Input.GetKeyDown(KeyCode.Space) && canShift)
            StartCoroutine(Shift());
        if (isShifting == false)
            rb.velocity = Moving();
        player.transform.localScale = new Vector3(1 + 0.05f * (rb.velocity.magnitude / speed), 1 - 0.05f * (rb.velocity.magnitude / speed), 1);
        player.transform.rotation = Quaternion.Euler(0, 0, (rb.velocity.y < 0 ? -Vector2.Angle(Vector2.right, rb.velocity) : Vector2.Angle(Vector2.right, rb.velocity)));
    }

    private void LateUpdate()
    {
        if (satellites.Count == 0)
            return;
        int i = 0;
        float pi = Mathf.PI;
        float dis = 2f + 0.5f * Mathf.Cos(pi * Time.time * 0.7f);
        float followStr = 0.15f + 0.1f * Mathf.Sin(pi * Time.time * 0.7f);
        float phase = (360 / satellites.Count) * Mathf.Deg2Rad;
        foreach (GameObject satellite in satellites)
        {
            Vector3 followPosition = player.transform.position + new Vector3(dis * Mathf.Cos(pi * Time.time + i * phase), dis * Mathf.Sin(pi * Time.time + i * phase), 0);
            satellite.transform.position = Vector3.Lerp(satellite.transform.position, followPosition, followStr);
            i++;
        }
    }

    public void AddSatellite(GameObject satellite)
    {
        if(satellites.Count > 0)
            weapons.ElementAt(currentSatIndex).SwitchOut();
        GameObject newGO = Instantiate(satellite, player.transform.position, Quaternion.identity, carrier.transform);
        satellites.AddLast(newGO);
        weapons.AddLast(newGO.GetComponent<Weapon>());
        currentSatIndex = satellites.Count - 1;
    }

    private void Attack()
    {
        if (satellites.Count == 0)
            return;
        if(previousSatIndex != currentSatIndex)
        {
            selectedWeapon = weapons.ElementAt(currentSatIndex);
            previousSatIndex = currentSatIndex;
        }
        selectedWeapon.Attack();
    }

    private void ChangeWeapon(float scroll)
    {
        if(satellites.Count <= 1)
            return;
        weapons.ElementAt(currentSatIndex).SwitchOut();
        if (scroll > 0)
            currentSatIndex = (currentSatIndex == 0 ? satellites.Count - 1 : currentSatIndex - 1);
        else
            currentSatIndex = (currentSatIndex == satellites.Count - 1 ? 0 : currentSatIndex + 1);
        weapons.ElementAt(currentSatIndex).SwitchIn();
    }

    private void Special()
    {
        if (satellites.Count == 0)
            return;
        weapons.ElementAt(currentSatIndex).Special();
        satellites.ElementAt(currentSatIndex).transform.parent = null;
        satellites.Remove(satellites.ElementAt(currentSatIndex));
        weapons.Remove(weapons.ElementAt(currentSatIndex));
        currentSatIndex = (currentSatIndex == 0 ? 0 : currentSatIndex - 1);
        previousSatIndex = -1;
        if (satellites.Count == 0)
            return;
        weapons.ElementAt(currentSatIndex).SwitchIn();
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

    IEnumerator Shift()
    {
        isShifting = true;
        canShift = false;
        Vector3 moveVect = Moving().normalized;
        Vector3 moveTo = player.transform.position + moveVect * shiftDis;
        player.transform.localScale = new Vector3(1.3f, 0.7f, 1);
        player.transform.rotation = Quaternion.Euler(0, 0, (moveVect.y < 0? -Vector2.Angle(Vector2.right, moveVect) : Vector2.Angle(Vector2.right, moveVect)));
        player.transform.DOMove(moveTo, 0.1f);
        player.transform.DOScale(new Vector3(1, 1, 1), 0.1f).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(0.1f);
        isShifting = false;
        yield return new WaitForSeconds(0.15f);
        canShift = true;
    }
}
