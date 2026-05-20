using System;
using System.Collections;
using UnityEngine;

public class damageable_component : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100f;
    [SerializeField] private float CurrentHealth = 100f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float Damage)
    {
        if (isDead) return;

        CurrentHealth -= Damage;
        StartCoroutine(DamageFlash());
        StartCoroutine(ShakeScale());
        
        if (CurrentHealth <= 0)
            DestroyObject();
    }

    public void DestroyObject()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(FadeOut());
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ShakeScale()
    {
        Vector3 originalScale = transform.localScale;
        for (int i = 0; i < 4; i++)
        {
            transform.localScale = originalScale * 1.1f;
            yield return new WaitForSeconds(0.05f);
            transform.localScale = originalScale * 0.95f;
            yield return new WaitForSeconds(0.05f);
        }
        transform.localScale = originalScale;
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        float duration = 0.3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, timer / duration);
                spriteRenderer.color = color;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Calcular daño basado en velocidad del impacto
        Rigidbody2D otherRb = collision.rigidbody;
        float impactDamage = 15f;

        if (otherRb != null)
        {
            // Daño escalado por velocidad del objeto que choca
            float velocityMagnitude = otherRb.linearVelocity.magnitude;
            impactDamage = 15f + (velocityMagnitude * 2f);
        }

        TakeDamage(impactDamage);
        
        // Knockback automático basado en dirección de colisión
        Rigidbody2D myRb = GetComponent<Rigidbody2D>();
        if (myRb != null)
        {
            Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
            myRb.linearVelocity = pushDirection * 3f;
        }
    }
}
