using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using Cinemachine;

public class CombatScript : MonoBehaviour
{
    int animationCount = 0;
    string[] attacks;

    private EnemyDetection enemyDetection;
    private MovementInput movementInput;
    private Animator animator;

    private EnemyScript lockedTarget;

    [Header("Combat Settings")]
    [SerializeField] private float longAttackCooldown = 1.4f;

    [Header("Final Blow Settings")]
    [SerializeField] private GameObject finalBlowCamera;

    [Header("States")]
    [SerializeField] private bool isAttackingEnemy = false;
    [SerializeField] private bool isCountering = false;

    [Header("Public References")]
    [SerializeField] private Transform punchPosition;

    [Space]

    //Events
    public UnityEvent<EnemyScript> OnHit;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        movementInput = GetComponent<MovementInput>();
    }

    void CounterCheck()
    {
        if (isCountering || isAttackingEnemy)
            return;

        float duration = .2f;

        transform.DOMove(transform.position - transform.forward, duration);
        animator.SetTrigger("Dodge");

        StartCoroutine(MovementDisableCoroutine(duration));

        IEnumerator MovementDisableCoroutine(float duration)
        {
            isCountering = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            movementInput.enabled = true;
            isCountering = false;
            AttackCheck();
        }
    }

    void AttackCheck()
    {
        if (isAttackingEnemy)
            return;

        if (enemyDetection.CurrentTarget() == null)
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

    public void Attack(EnemyScript target, float distance)
    {
        attacks = new string[] { "AirKick", "AirKick2", "AirPunch", "AirKick3" };

        if(target == null)
            AttackType("GroundPunch", .2f, null, 0);

        if(distance <= 4)
            AttackType("GroundPunch", .2f, target, .2f);

        if (distance > 4 && distance < 10)
        {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attacks.Length);
            AttackType(attacks[animationCount], longAttackCooldown, target, .65f);
        }

        if (GetComponentInChildren<CinemachineImpulseSource>())
            GetComponentInChildren<CinemachineImpulseSource>().m_ImpulseDefinition.m_AmplitudeGain = 1 * distance;

    }

    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetFloat("AttackType", Random.Range(0, 3));

        animator.SetTrigger(attackTrigger);

        StopAllCoroutines();
        StartCoroutine(MovementDisableCoroutine(isLastBlow() ? 1.5f : cooldown));

        //Check if last enemy
        if (isLastBlow())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null)
        return;

        MoveTorwardsTarget(target.transform, movementDuration);

        IEnumerator MovementDisableCoroutine(float duration)
        {
            movementInput.acceleration = 0;
            isAttackingEnemy = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            isAttackingEnemy = false;
            yield return new WaitForSeconds(.2f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }
    }

    void MoveTorwardsTarget(Transform target, float duration)
    {
        transform.DOLookAt(target.position, .2f);
        transform.DOMove(TargetOffset(target), duration);
    }

    IEnumerator FinalBlowCoroutine()
    {
        Time.timeScale = .5f;
        finalBlowCamera.SetActive(true);
        GameObject.Find("TargetFocus").transform.position = lockedTarget.transform.position;
        yield return new WaitForSeconds(1);
        finalBlowCamera.SetActive(false);
        Time.timeScale = 1f;
    }

    float TargetDistance(EnemyScript target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public Vector3 TargetOffset(Transform target)
    {
        Vector3 position;
        position = target.position;
        return Vector3.MoveTowards(position, transform.position, .95f);
    }

    public void HitEvent()
    {
        if (enemyDetection.CurrentTarget() == null || lockedTarget == null)
            return;

        OnHit.Invoke(lockedTarget);

        if (isLastBlow())
            lockedTarget = null;

        //Polish
        FindObjectOfType<ParticleSystemScript>().PlayParticleAtPosition(punchPosition.position);
    }

    void LerpCharacterAcceleration()
    {
        movementInput.acceleration = 0;
        DOVirtual.Float(0, 1, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
    }

    #region Input

    public void OnCounter()
    {
        CounterCheck();
    }

    public void OnAttack()
    {
        AttackCheck();
    }
    
    bool isLastBlow()
    {
        if (lockedTarget == null)
            return false; 

        return enemyDetection.targets.Count <= 1 && lockedTarget.health <= 1;
    }

    #endregion

}
