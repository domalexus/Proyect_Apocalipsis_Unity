using UnityEngine;


public class healthbar_controller_component : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image BarImage;

    public void UpdateBar(float MaxHealth, float health)
    {
        BarImage.fillAmount = health / MaxHealth;
        
    }
}
