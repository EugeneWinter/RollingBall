using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Настройки камеры")]
    public Vector3 offset = new Vector3(0f, 18f, -12f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(player.position);
    }
}