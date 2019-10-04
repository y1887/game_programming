using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveSatellite : MonoBehaviour
{
    public GameObject satellite;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if(collision.CompareTag("Player"))
        {
            collision.gameObject.GetComponentInParent<PlayerController>().AddSatellite(satellite);
            Destroy(this);
        }*/
    }
}
