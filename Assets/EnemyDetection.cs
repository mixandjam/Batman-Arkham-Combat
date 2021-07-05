using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private MovementInput movementInput;
    private CombatScript combatScript;

    [Header("Targets in radius")]
    public List<EnemyScript> targets;
    public int targetIndex;

    public LayerMask layerMask;

    Vector3 desiredMoveDirection;
    [SerializeField] private EnemyScript currentTarget;

    public GameObject cam;

    private void Start()
    {
        movementInput = GetComponentInParent<MovementInput>();
        combatScript = GetComponentInParent<CombatScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targets.Add(other.GetComponent<EnemyScript>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (targets.Contains(other.GetComponent<EnemyScript>()))
                targets.Remove(other.GetComponent<EnemyScript>());
        }
    }

    public void RemoveEnemy(EnemyScript enemy)
    {
        targets.Remove(enemy);
    }

    private void Update()
    {
        var camera = Camera.main;
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * movementInput.moveAxis.y + right * movementInput.moveAxis.x;
        desiredMoveDirection = desiredMoveDirection.normalized;

        if (movementInput.moveAxis.magnitude < .5f)
            desiredMoveDirection = transform.forward;

        RaycastHit info;

        if (Physics.SphereCast(transform.position, 3f, desiredMoveDirection,out info, 10,layerMask))
        {
            currentTarget = info.collider.transform.GetComponent<EnemyScript>();
        }
    }

    public EnemyScript CurrentTarget()
    {
        return currentTarget;
    }

    public void SetCurrentTarget(EnemyScript target)
    {
        currentTarget = target;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, desiredMoveDirection);
        Gizmos.DrawWireSphere(transform.position, 1);
        if(CurrentTarget() != null)
            Gizmos.DrawSphere(CurrentTarget().transform.position, .5f);
    }
}
