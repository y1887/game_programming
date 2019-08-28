using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyDamage : MonoBehaviour
{
    [Header("擊退力度"), Range(10f,50f)]
    public float forceMultiplier = 15f;
    public GameObject hitText;
    [Header("字體跳躍高度"), Range(1f, 5f)]
    public float jumpMultiplier = 1.5f;
    [Header("字體跳躍時間"), Range(0.5f, 2f)]
    public float jumpTime = 0.8f;
    [Header("跳躍曲線")]
    public Ease jumpEase;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Material hitMaterial, original;
    private BoxCollider2D pushBox;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponentInParent<Rigidbody2D>();
        sr = this.GetComponent<SpriteRenderer>();
        hitMaterial = Resources.Load<Material>("Materials/EnemyGetHitEffect");
        pushBox = this.GetComponentInParent<BoxCollider2D>();
        original = sr.material;
    }

    public IEnumerator getHit(Vector2 playerPos, float time)
    {
        Vector2 direction = (new Vector2(this.transform.position.x, this.transform.position.y) - playerPos).normalized;
        Vector2 force = direction * forceMultiplier;
        yield return new WaitForSeconds(time);
        GameObject thisHitText = GameObject.Instantiate(hitText, this.transform.position, Quaternion.identity);
        thisHitText.transform.DOJump(thisHitText.transform.position + new Vector3(direction.x, direction.y, 0), jumpMultiplier, 1, jumpTime).SetEase(jumpEase);
        rb.AddForce(force, ForceMode2D.Impulse);
        sr.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        sr.material = original;
        yield return new WaitForSeconds(jumpTime - 0.1f);
        Destroy(thisHitText);
    }
}
