using UnityEngine;

public class damageable_component : MonoBehaviour
{
    [SerializeField] private float MaxHealth = 100f;
    [SerializeField] private float CurrentHealth = 100f;


    public void TakeDamage(float Damage){
        if(CurrentHealth <= 0) Destroy();

        CurrentHealth -= Damage;
        
    }

    public void Destroy()
    {
        
    }

}
