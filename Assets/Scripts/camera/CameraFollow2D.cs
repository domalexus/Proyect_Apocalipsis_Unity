using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Referencia del jugador")]
    [SerializeField] private Transform target; // Jugador

    [Header("Configuración del seguimiento")]
    [SerializeField] private float smoothSpeed = 5f; // Velocidad de suavizado
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f); // Posición relativa de la cámara

    [Header("Límites de movimiento (opcional)")]
    public bool useLimits = false;
    public Vector2 minPosition; // Límite inferior izquierdo
    public Vector2 maxPosition; // Límite superior derecho

    private void LateUpdate()
    {
        if (target == null) return;

        // Calcula la posición deseada
        Vector3 desiredPosition = target.position + offset;

        // Suaviza el movimiento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Aplica límites si están activos
        if (useLimits)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minPosition.x, maxPosition.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minPosition.y, maxPosition.y);
        }

        // Mantiene la posición final
        transform.position = smoothedPosition;
    }
}
