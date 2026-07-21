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

    [System.Serializable]
    public struct EnemyPrefabEntry
    {
        public GameObject prefab;
        [Tooltip("Probabilidad relativa de que este prefab sea seleccionado. Valores no negativos.")]
        public float probability;
    }

    [Header("Enemigos")]
    [SerializeField] private EnemyPrefabEntry[] enemyPrefabs; // Array de prefabs de enemigos con probabilidad
    [SerializeField] private float SpawnRate = 1f; // Tiempo en segundos entre cada spawn
    [SerializeField] private int EnemiesPerWave = 1; // Cantidad de enemigos que se spawnean por oleada
    private int MaxEnemiesToSpawn;

  


    // Variables internas
    private float spawnCooldown = 0f;
    private float waveCountdown = 0f;
    private float preparationCountdown = 0f;
    private bool spawningStarted = false;
    private int totalEnemiesSpawned = 0;
    private level_info_component levelInfo;
    private timer_component timer;

    private void Awake()
    {
        // Obtener referencia del jugador si no está asignada
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        timer = GetComponent<timer_component>();
        levelInfo = GetComponent<level_info_component>();
        if (levelInfo != null)
        {
            MaxEnemiesToSpawn = levelInfo.MaxEnemies;
            preparationCountdown = Mathf.Max(0f, levelInfo.PreparationDuration);
            Debug.Log($"[EnemySpawn] Preparación configurada en {preparationCountdown} segundos.");
        }
        else
        {
            preparationCountdown = 0f;
            Debug.LogWarning("[EnemySpawn] No se encontró level_info_component en el mismo objeto.");
        }

        if (timer != null && preparationCountdown > 0f)
        {
            timer.SetRunning(false);
            timer.SetText("Preparate");
            Debug.Log("[EnemySpawn] Temporizador detenido para preparación y texto establecido en 'Preparate'.");
        }

        spawnCooldown = Mathf.Max(0.01f, SpawnRate);
        waveCountdown = Mathf.Max(0.01f, SpawnRate);
        Debug.Log($"[EnemySpawn] SpawnRate={SpawnRate}, MaxEnemiesToSpawn={MaxEnemiesToSpawn}, EnemiesPerWave={EnemiesPerWave}");
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (player == null || enemyPrefabs == null || enemyPrefabs.Length == 0)
            return;

        if (!spawningStarted)
        {
            preparationCountdown -= Time.deltaTime;
            if (preparationCountdown <= 0f)
            {
                spawningStarted = true;
                preparationCountdown = 0f;
                Debug.Log("[EnemySpawn] La preparación terminó, comienza el spawn.");

                if (timer != null)
                {
                    timer.SetRunning(true);
                    Debug.Log("[EnemySpawn] Temporizador reanudado tras preparación.");
                }
            }
            return;
        }

        // Actualizar cooldown de spawn
        spawnCooldown -= Time.deltaTime;

        // Actualizar contador de oleada
        waveCountdown -= Time.deltaTime;

        // Hacer spawn individual si el cooldown se acabó
        if (spawnCooldown <= 0f)
        {
            if (CanSpawnMoreEnemies())
            {
                SpawnEnemy();
                Debug.Log($"[EnemySpawn] Spawn individual completado. Total: {totalEnemiesSpawned}");
            }

            spawnCooldown = Mathf.Max(0.01f, SpawnRate);
        }

        // Activar oleada de enemigos
        if (waveCountdown <= 0f)
        {
            SpawnWave();
            waveCountdown = Mathf.Max(0.01f, SpawnRate);
        }
    }

    private bool CanSpawnMoreEnemies()
    {
        return MaxEnemiesToSpawn <= 0 || totalEnemiesSpawned < MaxEnemiesToSpawn;
    }

    /// <summary>
    /// Spawnea un enemigo individual en una posición válida.
    /// </summary>
    private void SpawnEnemy()
    {
        if (!CanSpawnMoreEnemies())
            return;

        Vector2 spawnPosition = GetValidSpawnPosition();
        GameObject prefabToSpawn = GetRandomEnemyPrefab();
        if (prefabToSpawn == null)
            return;

        GameObject newEnemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

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
        Debug.Log($"[EnemySpawn] Enemigo instanciado. Total actual: {totalEnemiesSpawned}");
    }

    /// <summary>
    /// Spawnea múltiples enemigos (oleada) en la mapa.
    /// </summary>
    private void SpawnWave()
    {
        int spawnedThisWave = 0;

        for (int i = 0; i < EnemiesPerWave; i++)
        {
            if (!CanSpawnMoreEnemies())
                break;

            SpawnEnemy();
            spawnedThisWave++;
        }

        if (spawnedThisWave > 0)
        {
            Debug.Log($"[EnemySpawn] Oleada completada: {spawnedThisWave} enemigo(s) spawned.");
        }
    }

    /// <summary>
    /// Selecciona un prefab de enemigo según probabilidades relativas definidas en el inspector.
    /// Devuelve null si no hay prefabs válidos.
    /// </summary>
    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;

        // Calcular suma de probabilidades
        float total = 0f;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].prefab == null)
                continue;
            total += Mathf.Max(0f, enemyPrefabs[i].probability);
        }

        // Si todas las probabilidades son cero o no hay prefabs con probabilidad, elegir uniformemente
        if (total <= 0f)
        {
            int count = 0;
            for (int i = 0; i < enemyPrefabs.Length; i++)
                if (enemyPrefabs[i].prefab != null) count++;

            if (count == 0)
                return null;

            int idx = Random.Range(0, count);
            int seen = 0;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i].prefab == null) continue;
                if (seen == idx) return enemyPrefabs[i].prefab;
                seen++;
            }
        }

        // Selección ponderada
        float r = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            var entry = enemyPrefabs[i];
            if (entry.prefab == null) continue;
            float p = Mathf.Max(0f, entry.probability);
            cumulative += p;
            if (r <= cumulative)
            {
                return entry.prefab;
            }
        }

        // Fallback: devolver el primero no nulo
        for (int i = 0; i < enemyPrefabs.Length; i++)
            if (enemyPrefabs[i].prefab != null) return enemyPrefabs[i].prefab;

        return null;
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
