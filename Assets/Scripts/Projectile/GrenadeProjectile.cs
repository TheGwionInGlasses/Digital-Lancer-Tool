using System;
using UnityEngine;

/// <summary>
/// This class models the behaviour of the GrenadeProjectile prefab that's instantiated during certain attack actions.
/// The logic for modelling the movement of the GrenadeProjectile and its trail is contained here. This script is
/// attaached as a component to the GrenadeProjectile prefab.
/// </summary>
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

    /// <summary>
    /// As soon as the Grenade Projectile is instantiated, begin moving in the direction of the target transform.
    /// An AnimationCurve is attached to the projectile to decide the projectile's transform's y and create an
    /// arc between the origin and the target.
    /// Once the projectile has reached its target, create a physics overlap orb and find all the agents who have
    /// collided with the orb. This determines the units that have been damaged by the grenade. The radius of the orb
    /// is measured in Unity units and can be modified here.
    /// Destroy the GrenadePojectile and then instantiate an instance of the GrenadeExplodedVfxPrefab to create
    /// an explosion effect.
    /// </summary>
    private void Update()
    {
        
        Vector3 moveDir = (targetPosition - positionXZ).normalized;

        float moveSpeed = 15f;

        // Movement
        positionXZ += moveDir * moveSpeed * Time.deltaTime;
        float distance = Vector3.Distance(positionXZ, targetPosition);
        float distanceNormalized = 1 - distance / totalDistance;

        // Arc
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

    /// <summary>
    /// When the GrenadeProjectile is setup, it is passed a function signature to store in the class as a field to run
    /// when the objective of the class has been completed as well as an int for the damage and the gridposition to
    /// target.
    /// </summary>
    /// <param name="targetGridPosition"></param>
    /// <param name="damage"></param>
    /// <param name="onComplete"></param>
    public void Setup(GridPosition targetGridPosition, int damage, Action onComplete)
    {
        this.onComplete = onComplete;
        // Convert Gridpostion into world position and store in the class as a field.
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        this.damage = damage;

        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }
}
