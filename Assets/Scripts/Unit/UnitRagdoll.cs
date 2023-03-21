using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollRootBone;

    public void Setup(Transform originRootBone)
    {
        MatchAllChildTransform(originRootBone, ragdollRootBone);
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f), 
            0, 
            Random.Range(-1f, 1f)
        );
        ApplyExplosionToRagdoll(ragdollRootBone, 750f, transform.position + randomDirection, 10f);
    }

    private void MatchAllChildTransform(Transform root, Transform target)
    {
        foreach (Transform rootChild in root)
        {
            Transform targetChild = target.Find(rootChild.name);
            if (targetChild != null)
            {
                targetChild.position = rootChild.position;
                targetChild.rotation = rootChild.rotation;
                MatchAllChildTransform(rootChild, targetChild);
            }
        }
    }

    private void ApplyExplosionToRagdoll(Transform root, float explosionForce, Vector3 explosionPosition, float explosionRange)
    {
        foreach (Transform rootChild in root)
        {
            if (rootChild.TryGetComponent<Rigidbody>(out Rigidbody childRigidBody))
            {
                childRigidBody.AddExplosionForce(explosionForce, explosionPosition, explosionRange);
                ApplyExplosionToRagdoll(rootChild, explosionForce, explosionPosition, explosionRange);
            }
        }
    }

}
