using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float followSpeed = 6f;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPosition =
            target.position + offset;

        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
    }
}