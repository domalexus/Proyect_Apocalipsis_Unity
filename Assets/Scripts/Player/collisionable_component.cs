using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Componente de colisión del jugador.
/// Maneja las interacciones con enemigos y objetos destructibles.
/// </summary>
public class collisionable_component : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private float damageFromEnemy = 10f; // Daño que recibe del enemigo
    [SerializeField] private float damageToProp = 5f; // Daño que aplica a props al colisionar
    [SerializeField] private float collisionDamageCooldown = 0.5f; // Cooldown entre daños de colisión

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength = 3f; // Fuerza del knockback al colisionar
    [SerializeField] private bool enableKnockbackFromEnemies = true;

    [Header("Agarre")]
    [SerializeField] private float maxGrabDistance = 2f; // Distancia máxima para agarrar un prop

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Variables internas
    private damageable_component playerDamageComponent;
    private Rigidbody2D playerRb;
    private float lastCollisionDamageTime = -Mathf.Infinity;
    private GameObject carriedProp;
    private InputAction grabAction;

    private void Awake()
    {
        // Obtener referencias del jugador
        playerDamageComponent = GetComponent<damageable_component>();
        playerRb = GetComponent<Rigidbody2D>();
        grabAction = InputSystem.actions.FindAction("Grab");
        grabAction?.Enable();
    }

    private void Update()
    {
        if (grabAction != null && grabAction.WasPressedThisFrame())
        {
            if (carriedProp != null)
            {
                DropProp();
            }
            else
            {
                TryGrabProp();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar colisión con enemigos
        if (collision.gameObject.CompareTag("Enemy"))
        {
            HandleEnemyCollision(collision.gameObject);

        }
        // Detectar colisión con props
        //else if (collision.gameObject.CompareTag("Prop"))
        //{
        //    HandlePropCollision(collision.gameObject);
        //}
    }

    private void TryGrabProp()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, maxGrabDistance);
        GameObject nearestProp = null;
        float nearestDistance = maxGrabDistance;

        foreach (Collider2D hit in nearbyColliders)
        {
            if (!hit.CompareTag("Prop"))
                continue;

            float distanceToProp = Vector2.Distance(transform.position, hit.transform.position);
            if (distanceToProp < nearestDistance)
            {
                nearestDistance = distanceToProp;
                nearestProp = hit.gameObject;
            }
        }

        if (nearestProp == null)
        {
            if (showDebugInfo)
                Debug.Log("No hay props cerca para agarrar.");
            return;
        }

        carriedProp = nearestProp;
        carriedProp.transform.SetParent(transform);

        if (showDebugInfo)
            Debug.Log($"¡Prop agarrado! {carriedProp.name}");
    }

    private void DropProp()
    {
        if (carriedProp == null)
            return;

        carriedProp.transform.SetParent(null);
        carriedProp = null;

        if (showDebugInfo)
            Debug.Log("Prop soltado.");
    }

    /// <summary>
    /// Maneja la colisión con un enemigo.
    /// </summary>
    private void HandleEnemyCollision(GameObject enemy)
    {
        Debug.Log("Collision enemy");
        // Aplicar daño al jugador
        if (playerDamageComponent != null)
        {
            playerDamageComponent.TakeDamage(damageFromEnemy);
            Debug.Log("Collision enemy take damage");
            if (showDebugInfo)
                Debug.Log($"¡Golpeado por enemigo! Daño recibido: {damageFromEnemy}");
        }

        // Aplicar knockback
        if (enableKnockbackFromEnemies && playerRb != null)
        {
            Vector2 knockbackDirection = (transform.position - enemy.transform.position).normalized;
            playerRb.linearVelocity = knockbackDirection * knockbackStrength;
        }
    }

    /// <summary>
    /// Maneja la colisión con un objeto destructible (Prop).
    /// </summary>
    //private void HandlePropCollision(GameObject prop)
    //{
        // Buscar el componente damageable del prop
        //damageable_component propDamage = prop.GetComponent<damageable_component>();
        //Debug.Log("collision con prop");
        //if (propDamage != null)
        //{
        //    propDamage.TakeDamage(damageToProp);
        //    if (showDebugInfo)
        //        Debug.Log($"¡Golpeaste el prop '{prop.name}'! Daño aplicado: {damageToProp}");
        //}
        //else if (showDebugInfo)
        //{
        //    Debug.LogWarning($"El prop '{prop.name}' no tiene damageable_component");
        //}
    //}

    /// <summary>
    /// Configura el daño que recibe del enemigo.
    /// </summary>
    public void SetEnemyDamage(float newDamage)
    {
        damageFromEnemy = Mathf.Max(newDamage, 0f);
    }

    /// <summary>
    /// Configura el daño que aplica a los props.
    /// </summary>
    public void SetPropDamage(float newDamage)
    {
        damageToProp = Mathf.Max(newDamage, 0f);
    }

}