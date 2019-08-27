using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("擊退力度"), Range(10f,50f)]
    public float forceMultiplier = 15f;
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
        Vector2 force = (new Vector2(this.transform.position.x, this.transform.position.y) - playerPos).normalized * forceMultiplier;
        yield return new WaitForSeconds(time);
        rb.AddForce(force, ForceMode2D.Impulse);
        sr.material = hitMaterial;
        yield return new WaitForSeconds(0.1f);
        sr.material = original;
    }
}
