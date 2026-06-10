using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Componente modular para manejar daño y salud en cualquier objeto del juego.
/// Puede ser usado en enemigos, jugador, objetos destructibles, etc.
/// </summary>
public class damageable_component : MonoBehaviour
{
    [Header("Configuración de Salud")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canBeHealed = true;
    [SerializeField] private float maxHealthRegeneration = 0f; // 0 = sin regeneración
    [SerializeField] private float regenRate = 1f; // salud por segundo
    [SerializeField] private float regenDelay = 3f; // segundos sin recibir daño para empezar a regenerar

    [Header("Sistema de Defensa")]
    [SerializeField] private float damageResistance = 0f; // 0-1 (0 = sin resistencia, 0.5 = 50% menos daño)
    [SerializeField] private bool isInvulnerable = false;

    [Header("Efectos")]
    [SerializeField] private bool enableKnockback = false;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private Rigidbody2D rb2D;

    
    private healthbar_controller_component healthBar;


    // Eventos
    public event Action<float> OnDamageTaken; // Envía la cantidad de daño
    public event Action<float> OnHealed; // Envía la cantidad sanada
    public event Action<float> OnHealthChanged; // Envía la salud actual
    public event Action OnDeath; // Cuando la salud llega a 0
    public event Action OnFullHealth; // Cuando llega a salud máxima

    // Propiedades públicas
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool IsDead => currentHealth <= 0f;
    public bool IsAtFullHealth => currentHealth >= maxHealth;

    private void Awake()
    {
        healthBar = GetComponentInChildren<healthbar_controller_component>();
        currentHealth = maxHealth;
        
        UpdateHealthBar();
    }


    /// <summary>
    /// Aplica daño al objeto. Retorna la cantidad de daño real aplicado.
    /// </summary>
    public float TakeDamage(float damageAmount)
    {
        if (!IsAlive || isInvulnerable || damageAmount <= 0f)
            return 0f;

        // Aplicar resistencia al daño
        float actualDamage = damageAmount * (1f - damageResistance);
        
        currentHealth -= actualDamage;

        // Evitar salud negativa
        if (currentHealth < 0f)
            currentHealth = 0f;

        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();

        // Aplicar knockback si está habilitado
        if (enableKnockback && rb2D != null)
        {
            ApplyKnockback();
        }

        // Verificar muerte
        if (IsDead)
        {
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// Sana al objeto. Retorna la cantidad de salud real regenerada.
    /// </summary>
    public float Heal(float healAmount)
    {
        if (!IsAlive || !canBeHealed || healAmount <= 0f)
            return 0f;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        
        float actualHealed = currentHealth - previousHealth;
        OnHealed?.Invoke(actualHealed);
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();

        if (IsAtFullHealth)
        {
            OnFullHealth?.Invoke();
        }

        return actualHealed;
    }

    /// <summary>
    /// Restaura la salud al máximo.
    /// </summary>
    public void FullHeal()
    {
        Heal(maxHealth);
    }

    /// <summary>
    /// Establece la salud actual a un valor específico.
    /// </summary>
    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();

        if (IsAtFullHealth)
            OnFullHealth?.Invoke();

        if (IsDead)
            Die();
    }

    /// <summary>
    /// Establece el máximo de salud y ajusta la salud actual si es necesario.
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.Max(newMaxHealth, 1f);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Habilita o deshabilita la invulnerabilidad temporal o permanente.
    /// </summary>
    public void SetInvulnerable(bool state)
    {
        isInvulnerable = state;
    }

    /// <summary>
    /// Aplica un knockback al objeto (requiere Rigidbody2D).
    /// </summary>
    public void ApplyKnockback(Vector2 direction)
    {
        if (rb2D == null)
            return;

        rb2D.linearVelocity = direction.normalized * knockbackForce;
    }

    private void ApplyKnockback()
    {
        if (rb2D == null)
            return;

        // Knockback aleatorio en dirección aleatoria
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        ApplyKnockback(randomDirection);
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateBar(maxHealth, currentHealth);
        }
    }

  
    private void Die()
    {
        OnDeath?.Invoke();
        
        // Opcional: Desactivar el GameObject después de morir
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// Revive el objeto restaurando su salud.
    /// </summary>
    public void Revive(float reviveHealth = -1f)
    {
        if (reviveHealth < 0f)
            reviveHealth = maxHealth;

        SetHealth(reviveHealth);
        

    }

    // Métodos auxiliares para debugging
    private void OnGUI()
    {
        // Descomenta para ver información de debug en pantalla
        // GUILayout.Label($"Salud: {currentHealth:F1}/{maxHealth} ({HealthPercentage*100:F1}%)");
    }
}
