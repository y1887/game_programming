using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CanvasTransition : MonoBehaviour
{
    private LevelGenerator generator;
    private Image BG;
    private Image[] images;
    private TextMeshProUGUI[] texts;

    // Start is called before the first frame update
    void Start()
    {
        BG = this.GetComponent<Image>();
        images = this.GetComponentsInChildren<Image>();
        texts = this.GetComponentsInChildren<TextMeshProUGUI>();
        generator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<LevelGenerator>();
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitUntil(() => generator.currentStatus == LevelGenerator.GeneratingStatus.Finish);
        BG.DOColor(Color.clear, 1);
        foreach (Image image in images)
            image.DOColor(Color.clear, 1);
        foreach (TextMeshProUGUI text in texts)
            text.DOColor(Color.clear, 1);
    }
}
