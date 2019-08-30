using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private Camera cam;
    private GameObject player;
    private PlayerController controller;
    [Header("攝影機跟隨強度"),Range(0f,1f)]
    public float lerp = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        cam = this.GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");
        controller = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 previousPos = cam.transform.position;
        Vector2 newPos = Vector2.Lerp(previousPos, player.transform.position, lerp);
        cam.transform.position = new Vector3(newPos.x, newPos.y, cam.transform.position.z);
    }
}
