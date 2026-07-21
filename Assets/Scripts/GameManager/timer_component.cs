using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class timer_component : MonoBehaviour
{
    [Header("Tiempo")]
    private int initialTime;

    public TextMeshProUGUI TextMesh;
    private int currentTime;
    private bool isRunning = true;
    private level_info_component levelInfo;

    private void Start()
    {
        levelInfo = GetComponent<level_info_component>();

        if (levelInfo != null)
        {
            initialTime = levelInfo.GameDuration;
            Debug.Log($"[Timer] Tiempo inicial configurado desde level_info_component: {initialTime}");
        }

        currentTime = initialTime;

        if (TextMesh != null && isRunning)
        {
            TextMesh.text = currentTime.ToString("0");
            Debug.Log($"[Timer] Texto inicial del temporizador: {TextMesh.text}");
        }
        else if (TextMesh != null)
        {
            Debug.Log("[Timer] Temporizador detenido en inicio, conservando texto existente.");
        }

        StartCoroutine(TimerRoutine());
    }

    private System.Collections.IEnumerator TimerRoutine()
    {
        while (currentTime > 0)
        {
            if (!isRunning)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(1f);
            currentTime--;

            if (TextMesh != null)
                TextMesh.text = currentTime.ToString("0");

            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                Ganar();
            }
        }
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
        Debug.Log("[Timer] ResumeTimer() invoked: isRunning=true");
        if (TextMesh != null)
            TextMesh.text = currentTime.ToString("0");
    }

    public void SetText(string text)
    {
        if (TextMesh != null)
        {
            TextMesh.text = text;
            Debug.Log($"[Timer] SetText(): {text}");
        }
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
        Debug.Log($"[Timer] SetRunning(): {running}");
        if (running && TextMesh != null)
            TextMesh.text = currentTime.ToString("0");
    }

    public void AddTime(int amount)
    {
        currentTime += Mathf.Max(amount, 0);
    }

    private void Ganar()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex == 3 ? 5 : currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[Timer] Cargando escena {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("[Timer] No hay una escena siguiente disponible.");
        }
    }

    public int GetCurrentTime()
    {
        return currentTime;
    }
}
