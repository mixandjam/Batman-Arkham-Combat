using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{

    CombatScript combatScript;

    [Header("Targets in radius")]
    public List<Transform> targets;
    public int targetIndex;

    public LayerMask layerMask;

    Vector3 desiredMoveDirection;
    private Transform currentTarget;

    private void Start()
    {
        combatScript = GetComponentInParent<CombatScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (targets.Contains(other.transform))
                targets.Remove(other.transform);
        }
    }

    private void Update()
    {
        //Input
        float InputX = Input.GetAxis("Horizontal");
        float InputZ = Input.GetAxis("Vertical");

        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * InputZ + right * InputX;
        desiredMoveDirection = desiredMoveDirection.normalized;

        RaycastHit info;

        if (Physics.SphereCast(transform.position, 1.5f, desiredMoveDirection,out info, 10,layerMask))
        {
            currentTarget = info.collider.transform;
        }
    }

    public Transform CurrentTarget()
    {
        return currentTarget;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, desiredMoveDirection);
        Gizmos.DrawWireSphere(transform.position, 1);
        if(CurrentTarget() != null)
            Gizmos.DrawSphere(CurrentTarget().position, .5f);
    }
}
