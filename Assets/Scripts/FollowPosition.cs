using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    private PlayerController player;
    private Rigidbody2D rb;
    private CircleCollider2D circle;
    private Vector3 previousPos;

    // Start is called before the first frame update
    void Start()
    {
        player = this.GetComponentInParent<PlayerController>();
        rb = this.GetComponent<Rigidbody2D>();
        circle = this.GetComponent<CircleCollider2D>();
        previousPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = player.movePos;
    }
}