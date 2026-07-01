using UnityEngine;

/// <summary>
/// Componente de spawn de enemigos.
/// Genera enemigos dentro de una zona delimitada y a una distancia mínima del jugador.
/// </summary>
public class enemy_spawn_component : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject player;
    [SerializeField] private AudioSource spawnAudio;

    [Header("Zona de Spawn")]
    [SerializeField] private float minX = -20f; // Límite mínimo en X
    [SerializeField] private float maxX = 20f; // Límite máximo en X
    [SerializeField] private float minY = -20f; // Límite mínimo en Y
    [SerializeField] private float maxY = 20f; // Límite máximo en Y

    [Header("Distancia del Jugador")]
    [SerializeField] private float minDistanceFromPlayer = 5f; // Distancia mínima de spawn respecto al jugador

    [Header("Enemigos")]
    [SerializeField] private GameObject[] enemyPrefabs; // Array de prefabs de enemigos a spawnear
    [SerializeField] private float spawnRate = 1f; // Tiempo en segundos entre cada spawn

    [Header("Oleadas")]
    [SerializeField] private float waveTimer = 30f; // Tiempo hasta la siguiente oleada
    [SerializeField] private int enemiesToSpawnPerWave = 5; // Cantidad de enemigos por oleada

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // Variables internas
    private float spawnCooldown = 0f;
    private float waveCountdown = 0f;
    private int totalEnemiesSpawned = 0;

    private void Awake()
    {
        // Obtener referencia del jugador si no está asignada
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        // Inicializar el contador de oleada
        waveCountdown = waveTimer;
    }

    private void Update()
    {
        if (player == null || enemyPrefabs.Length == 0)
            return;

        // Actualizar cooldown de spawn
        spawnCooldown -= Time.deltaTime;

        // Actualizar contador de oleada
        waveCountdown -= Time.deltaTime;

        // Hacer spawn individual si el cooldown se acabó
        if (spawnCooldown <= 0f)
        {
            SpawnEnemy();
            spawnCooldown = spawnRate;
        }

        // Activar oleada de enemigos
        if (waveCountdown <= 0f)
        {
            SpawnWave();
            waveCountdown = waveTimer;
        }
    }

    /// <summary>
    /// Spawnea un enemigo individual en una posición válida.
    /// </summary>
    private void SpawnEnemy()
    {
        Vector2 spawnPosition = GetValidSpawnPosition();
        GameObject randomEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        GameObject newEnemy = Instantiate(randomEnemyPrefab, spawnPosition, Quaternion.identity);

        if (spawnAudio != null)
        {
            spawnAudio.Play();
        }
        
        // Si el enemigo tiene el componente de comportamiento, asignar el jugador como objetivo
        enemy_behavior_component enemyBehavior = newEnemy.GetComponent<enemy_behavior_component>();
        if (enemyBehavior != null)
        {
            enemyBehavior.SetTarget(player);
        }

        totalEnemiesSpawned++;

        if (showDebugInfo)
            Debug.Log($"Enemigo spawnado en: {spawnPosition} (Total: {totalEnemiesSpawned})");
    }

    /// <summary>
    /// Spawnea múltiples enemigos (oleada) en la mapa.
    /// </summary>
    private void SpawnWave()
    {
        for (int i = 0; i < enemiesToSpawnPerWave; i++)
        {
            SpawnEnemy();
        }

        if (showDebugInfo)
            Debug.Log($"¡Oleada de {enemiesToSpawnPerWave} enemigos! Total: {totalEnemiesSpawned}");
    }

    /// <summary>
    /// Obtiene una posición válida para spawnear un enemigo.
    /// Verifica que esté dentro de la zona y a la distancia mínima del jugador.
    /// </summary>
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPosition;
        bool isValidPosition = false;
        int maxAttempts = 10; // Intentos máximos para encontrar una posición válida
        int attempts = 0;

        do
        {
            // Generar posición aleatoria dentro de la zona
            spawnPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // Verificar distancia mínima del jugador
            float distanceToPlayer = Vector2.Distance(spawnPosition, (Vector2)player.transform.position);
            if (distanceToPlayer >= minDistanceFromPlayer)
            {
                isValidPosition = true;
            }

            attempts++;
        } while (!isValidPosition && attempts < maxAttempts);

        // Si no se encontró una posición válida después de los intentos, devolver la última
        return spawnPosition;
    }

    /// <summary>
    /// Detiene todos los spawns pausando los timers.
    /// </summary>
    public void StopSpawning()
    {
        spawnCooldown = float.MaxValue;
        waveCountdown = float.MaxValue;
    }

    /// <summary>
    /// Reanuda los spawns.
    /// </summary>
    public void ResumeSpawning()
    {
        spawnCooldown = 0f;
        waveCountdown = waveTimer;
    }

    /// <summary>
    /// Obtiene el total de enemigos spawnados.
    /// </summary>
    public int GetTotalEnemiesSpawned()
    {
        return totalEnemiesSpawned;
    }

    /// <summary>
    /// Reinicia el contador de enemigos.
    /// </summary>
    public void ResetSpawnCount()
    {
        totalEnemiesSpawned = 0;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo)
            return;

        // Dibujar zona de spawn
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // Verde semi-transparente
        Vector3 zoneCenter = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 zoneSize = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawCube(zoneCenter, zoneSize);

        // Dibujar líneas del perímetro
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minX, minY, 0f), new Vector3(maxX, minY, 0f));
        Gizmos.DrawLine(new Vector3(maxX, minY, 0f), new Vector3(maxX, maxY, 0f));
        Gizmos.DrawLine(new Vector3(maxX, maxY, 0f), new Vector3(minX, maxY, 0f));
        Gizmos.DrawLine(new Vector3(minX, maxY, 0f), new Vector3(minX, minY, 0f));

        // Dibujar radio de exclusión del jugador
        if (player != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo semi-transparente
            Gizmos.DrawWireSphere(player.transform.position, minDistanceFromPlayer);
        }
    }
}
