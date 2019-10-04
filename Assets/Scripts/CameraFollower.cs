using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private LevelGenerator generator = null;
    private Camera cam;
    public GameObject player = null;
    private PlayerController controller = null;
    [Header("攝影機跟隨強度"),Range(0f,1f)]
    public float lerp = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        cam = this.GetComponent<Camera>();
        generator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<LevelGenerator>();
        if (generator == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform.Find("Player").gameObject;
            controller = player.GetComponentInParent<PlayerController>();
        }
        else
            StartCoroutine(FindPlayer());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player != null && controller != null)
        {
            Vector2 previousPos = cam.transform.position;
            Vector2 newPos = Vector2.Lerp(previousPos, player.transform.position, lerp);
            cam.transform.position = new Vector3(newPos.x, newPos.y, cam.transform.position.z);
        }
        else
            Debug.LogWarning("Player was not found by the camera.");
    }

    IEnumerator FindPlayer()
    {
        enabled = false;
        yield return new WaitUntil(() => (int)generator.currentStatus >= (int)LevelGenerator.GeneratingStatus.Final_Makeup);
        while(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform.Find("Player").gameObject;
            yield return null;
        }
        cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, cam.transform.position.z);
        controller = player.GetComponentInParent<PlayerController>();
        enabled = true;
    }
}
