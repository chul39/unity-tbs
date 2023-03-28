using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public static event EventHandler OnAnyGrenadeExploded;
    private Vector3 targetPosition;
    private Action onGrenadeComplete;
    [SerializeField] private float moveSpeed = 15f;

    [SerializeField] private Transform grenadeExplodeVfxPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private AnimationCurve arcYAnimationCurve;
    private float totalDistance;
    private Vector3 positionXZ;

    private void Update() 
    {
        Vector3 moveDirection = (targetPosition - positionXZ).normalized;
        positionXZ += moveDirection * moveSpeed * Time.deltaTime;
        
        float distance = Vector3.Distance(positionXZ, targetPosition);
        float normalizedDistance = 1 - (distance / totalDistance);

        float maxHeight = totalDistance / 3f;
        float positionY = arcYAnimationCurve.Evaluate(normalizedDistance) * maxHeight;
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);
        
        float reachedTargetDistance = .2f;
        if (Vector3.Distance(transform.position, targetPosition) < reachedTargetDistance)
        {
            float damageRadius = 4f;
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.Damage(30);
                }
                if (collider.TryGetComponent<DestructibleCrate>(out DestructibleCrate destructibleCrate))
                {
                    destructibleCrate.Damage();
                }
            }
            OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);
            trailRenderer.transform.parent = null;
            Instantiate(grenadeExplodeVfxPrefab, targetPosition + Vector3.up * 1f, Quaternion.identity);
            Destroy(gameObject);
            onGrenadeComplete();
        }
    }
    
    public void Setup(GridPosition targetGridPosition, Action onGrenadeComplete)
    {
        this.onGrenadeComplete = onGrenadeComplete;
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }
}
