using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float constantUpwardSpeed = 2f;
    [SerializeField] private float catchUpSpeed = 5f;
    [SerializeField] private float minYPosition = 0f;

    [Tooltip("Offset applied to the target position. Use a negative value to make this object trail below the player (e.g., a kill zone).")]
    [SerializeField] private float yOffset = 0f;

    private float currentY;

    void Awake()
    {
        if (playerTransform == null)
        {
            var playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
        }

        currentY = Mathf.Max(transform.position.y, minYPosition);
    }

    void LateUpdate()
    {
        currentY += constantUpwardSpeed * Time.deltaTime;

        if (playerTransform != null)
        {
            float targetYWithOffset = playerTransform.position.y + yOffset;

            if (targetYWithOffset > currentY)
            {
                currentY = Mathf.Lerp(currentY, targetYWithOffset, catchUpSpeed * Time.deltaTime);
            }
        }

        currentY = Mathf.Max(currentY, minYPosition);
        transform.position = new Vector3(transform.position.x, currentY, transform.position.z);
    }
}