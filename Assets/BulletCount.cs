using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BulletCount : MonoBehaviour
{
    private TextMeshProUGUI text;
    private BulletManager manager;
    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();
        manager = BulletManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Bullets: " + manager.currentBulletAmount;
    }
}
