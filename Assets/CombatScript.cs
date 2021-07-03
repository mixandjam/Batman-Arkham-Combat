using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Cinemachine;

public class CombatScript : MonoBehaviour
{
    float hitAmount;
    Animator animator;
    EnemyDetection enemyDetection;
    MovementInput movementInput;

    private Transform lockedTarget;

    //Booleans
    public bool isAttackingEnemy = false;

    public Transform punchPosition;

    //Events
    public UnityEvent<Transform> OnHit;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        movementInput = GetComponent<MovementInput>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            transform.DOMove(transform.position - transform.forward, .3f);
            animator.SetTrigger("Dodge");
        }

        if (isAttackingEnemy)
        return;

        //Punch
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(enemyDetection.CurrentTarget() == null)
            {
                Attack(null, 0);
                return;
            }

            //Lock target
            lockedTarget = enemyDetection.CurrentTarget();

            //AttackTarget
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

    }

    public void Attack(Transform target, float distance)
    {
        if(target == null)
            AttackType("GroundPunch", .2f, null, 0);

        if(distance <= 4)
            AttackType("GroundPunch", .3f, target, .2f);

        if(distance > 4 && distance < 10)
            AttackType("AirPunch", .8f, target, .7f);

        if (GetComponent<CinemachineImpulseSource>())
            GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_AmplitudeGain = 1 * distance;

    }

    void AttackType(string attackTrigger, float cooldown, Transform target, float movementDuration)
    {
        animator.SetTrigger(attackTrigger);

        StopAllCoroutines();
        StartCoroutine(MovementDisableCoroutine(cooldown));

        if(target == null)
        return;

        MoveTorwardsTarget(target, movementDuration);

        IEnumerator MovementDisableCoroutine(float duration)
        {
            isAttackingEnemy = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            movementInput.enabled = true;
            isAttackingEnemy = false;
            LerpCharacterAcceleration();
        }
    }

    void MoveTorwardsTarget(Transform target, float duration)
    {
        transform.DOLookAt(target.position, .2f);
        transform.DOMove(TargetOffset(target), duration);
    }

    float TargetDistance(Transform target)
    {
        return Vector3.Distance(transform.position, target.position);
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, 1.05f);
    }

    public void HitEvent()
    {
        if (lockedTarget == null)
            return;

        isAttackingEnemy = false;
        OnHit.Invoke(lockedTarget);
        FindObjectOfType<ParticleSystemScript>().PlayParticleAtPosition(punchPosition.position);
    }

    void LerpCharacterAcceleration()
    {
        movementInput.acceleration = 0;
        DOVirtual.Float(0, 1, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
    }

}
