using UnityEngine;

public class spawn_manager_component : MonoBehaviour//probablemente este componente no usaremos
{
    public SpriteRenderer SpriteRendererComp;//usaremos esto para cuando se instancie pasarle el sprite rendered
    private damageable_component DamageableComp; // probablemente no usar   
    void Awake()
    {
        SpriteRendererComp = GetComponent<SpriteRenderer>();
    }
    
}
