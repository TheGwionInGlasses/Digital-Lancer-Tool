using UnityEngine;

/// <summary>
/// This class is a component attached to the BulletPorjectile prefab and serves to manage the behaviour of the bulletprojectile produced
/// during certain attack actions.
/// </summary>
public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform bulletHitVFXPrefab;

    private Vector3 targetPosition;

    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    /// <summary>
    /// As soon as the bulletprojectile game object is instantiated, begin moving quickly in the direction of its targets
    /// transform. Once it has arrived at its target, destroy itself and set its child, the tail renderer's parent
    /// to null so the trail dissappears.
    /// </summary>
    private void Update()
    {
        Vector3 moveDir = (targetPosition - transform.position).normalized;

        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);

        float moveSpeed = 200f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);

        if (distanceBeforeMoving < distanceAfterMoving)
        {
            //Stop the trail overshooting the bullet
            transform.position = targetPosition;

            trailRenderer.transform.parent = null;

            Destroy(gameObject);

            Instantiate(bulletHitVFXPrefab, targetPosition, Quaternion.identity);
        }
    }
}
