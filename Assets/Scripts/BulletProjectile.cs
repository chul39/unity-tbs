using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Transform bulletHitVfxPrefab;
    private Vector3 targetPosition;
    float moveSpeed = 200f;

    public void Setup(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private void Update()
    {
        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        float distnaceAfterMoving = Vector3.Distance(transform.position, targetPosition);
        if (distanceBeforeMoving < distnaceAfterMoving)
        {
            transform.position = targetPosition;
            trailRenderer.transform.parent = null;
            Destroy(gameObject);
            Instantiate(bulletHitVfxPrefab, targetPosition, Quaternion.identity);
        }
    }

}
