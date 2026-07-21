using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menu_system_component : MonoBehaviour
{
    [SerializeField] int ScenaParaCargar;
    public void Jugar()
    {
    
        SceneManager.LoadScene(ScenaParaCargar);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }
}
