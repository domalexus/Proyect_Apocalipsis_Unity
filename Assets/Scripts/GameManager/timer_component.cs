using TMPro;
using UnityEngine;

public class timer_component : MonoBehaviour
{
    [Header("Tiempo")]
    [SerializeField] private int initialTime = 60;


    public TextMeshProUGUI TextMesh;
    private int currentTime;
    private bool isRunning = true;

    private void Start()
    {
        
        currentTime = initialTime;

        if (TextMesh != null)
            TextMesh.text = currentTime.ToString("0");

        StartCoroutine(TimerRoutine());
    }

    private System.Collections.IEnumerator TimerRoutine()
    {
        while (isRunning && currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;

            if (TextMesh != null)
                TextMesh.text = currentTime.ToString("0");

            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                Debug.Log("Perdiste");
            }
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void AddTime(int amount)
    {
        currentTime += Mathf.Max(amount, 0);
    }

    public int GetCurrentTime()
    {
        return currentTime;
    }
}
