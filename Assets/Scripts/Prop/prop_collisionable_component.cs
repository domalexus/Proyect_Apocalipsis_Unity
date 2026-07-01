using System.Collections;
using UnityEngine;

public class prop_collisionable_component : MonoBehaviour
{
    [Header("Feedback de impacto")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitScaleMultiplier = 1.35f;
    [SerializeField] private float hitDuration = 0.25f;

    private damageable_component DamageableComponent;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine feedbackCoroutine;

    void Awake()
    {
        DamageableComponent = GetComponent<damageable_component>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (DamageableComponent != null)
                DamageableComponent.TakeDamage(5f);

            if (feedbackCoroutine != null)
                StopCoroutine(feedbackCoroutine);

            feedbackCoroutine = StartCoroutine(PlayHitFeedback());
        }
    }

    private IEnumerator PlayHitFeedback()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = hitColor;

        transform.localScale = originalScale * hitScaleMultiplier;

        yield return new WaitForSeconds(hitDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        transform.localScale = originalScale;
        feedbackCoroutine = null;
    }
}
