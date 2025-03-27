using System;
using UnityEngine;
using UnityEngine.Rendering;

public class GrenadeProjectile : MonoBehaviour
{
    public static event EventHandler OnAnyGrendeExploded;

    [SerializeField] private TrailRenderer trailRenderer;

    [SerializeField] private Transform grenadeExplodedVFXPrefab;
    [SerializeField] private AnimationCurve arcYAnimationCurve;

    private float totalDistance;

    private Vector3 targetPosition;
    private Action onComplete;
    private int damage;
    private Vector3 positionXZ;

    private void Update()
    {
        
        Vector3 moveDir = (targetPosition - positionXZ).normalized;

        float moveSpeed = 15f;

        positionXZ += moveDir * moveSpeed * Time.deltaTime;
        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 4f;
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

        float reachedTargetDistance = .2f;
        if (Vector3.Distance(positionXZ, targetPosition) < reachedTargetDistance)
        {
            float damageRadius = 2f;
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);

            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.Damage(damage);
                }
            }

            OnAnyGrendeExploded?.Invoke(this, EventArgs.Empty);

            trailRenderer.transform.parent = null;

            Instantiate(grenadeExplodedVFXPrefab, targetPosition + Vector3.up * 1f, Quaternion.identity);

            Destroy(gameObject);

            onComplete();
        }
    }

    public void Setup(GridPosition targetGridPosition, int damage, Action onComplete)
    {
        this.onComplete = onComplete;
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        this.damage = damage;

        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }
}
