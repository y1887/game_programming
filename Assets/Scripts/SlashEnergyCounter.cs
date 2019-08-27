using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlashEnergyCounter : MonoBehaviour
{
    public Color fullColor, emptyColor;
    public Slider s1, s2, s3, s4;
    public Image i1, i2, i3, i4;
    private PlayerController player;
    private float counter;
        
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        counter = player.slashNum;
    }

    // Update is called once per frame
    void Update()
    {
        counter = player.slashEnergy / 15f;
        s1.value = Mathf.Min(counter, 1);
        s2.value = Mathf.Min(counter - 1f, 1);
        s3.value = Mathf.Min(counter - 2f, 1);
        s4.value = Mathf.Min(counter - 3f, 1);
        Color blend = Color.Lerp(emptyColor, fullColor, Mathf.Floor(counter) / 4f);
        i1.color = blend;
        i2.color = blend;
        i3.color = blend;
        i4.color = blend;
    }
}
