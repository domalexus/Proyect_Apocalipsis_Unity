using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class menu_system_component : MonoBehaviour
{
    [SerializeField] bool EsMenuPrincipal = false;
    public void Jugar()
    {
        int Multiplicador = 1;

        if (EsMenuPrincipal)
            Multiplicador = 1;

        if(EsMenuPrincipal == false)
            Multiplicador = 2;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + Multiplicador);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }
}
