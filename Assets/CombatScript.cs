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

    [Header("Target")]
    public EnemyScript lockedTarget;

    [Header("Combat Settings")]
    [SerializeField] private float longAttackCooldown = 1.4f;

    [Header("Final Blow Settings")]
    [SerializeField] private GameObject finalBlowCamera;

    [Header("States")]
    public bool isAttackingEnemy = false;
    public bool isCountering = false;

    [Header("Public References")]
    [SerializeField] private Transform punchPosition;

    //Coroutines
    Coroutine counterCoroutine;
    Coroutine attackCoroutine;
    Coroutine damageCoroutine;

    [Space]

    //Events
    public UnityEvent<EnemyScript> OnHit;
    public UnityEvent<EnemyScript> OnCounterAttack;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyDetection = GetComponentInChildren<EnemyDetection>();
        movementInput = GetComponent<MovementInput>();
    }

    void CounterCheck()
    {
        //Initial check
        if (isCountering || isAttackingEnemy || !AnEnemyIsPreparingAttack())
            return;

        lockedTarget = ClosestCounterEnemy();
        OnCounterAttack.Invoke(lockedTarget);

        if(TargetDistance(lockedTarget) > 3)
        {
            Attack(lockedTarget, TargetDistance(lockedTarget));
            return;
        }

        animator.SetTrigger("Dodge");
        float duration = .2f;
        transform.DOLookAt(lockedTarget.transform.position,.2f);
        transform.DOMove(transform.position + lockedTarget.transform.forward, duration);

        if(counterCoroutine != null)
            StopCoroutine(counterCoroutine);
        counterCoroutine = StartCoroutine(CounterCoroutine(duration));

        IEnumerator CounterCoroutine(float duration)
        {
            isCountering = true;
            movementInput.enabled = false;
            yield return new WaitForSeconds(duration);
            Attack(lockedTarget, TargetDistance(lockedTarget));
            isCountering = false;

        }
    }

    void AttackCheck()
    {
        if (isAttackingEnemy)
            return;

        if (enemyDetection.CurrentTarget() == null)
        {
            if (enemyDetection.targets.Count == 0)
            {
                Attack(null, 0);
                return;
            }
            else
            {
                enemyDetection.PickRandomTarget();
            }
        }

        //Lock target
        lockedTarget = enemyDetection.CurrentTarget();

        //Choose Random Target
        enemyDetection.PickRandomTarget();

        //AttackTarget
        Attack(lockedTarget, TargetDistance(lockedTarget));
        return;
    }

    public void Attack(EnemyScript target, float distance)
    {
        if (GetComponentInChildren<TrailRenderer>() != null)
        {
            GetComponentInChildren<TrailRenderer>().Clear();
            GetComponentInChildren<TrailRenderer>().time = 0;
        }

        attacks = new string[] { "AirKick", "AirKick2", "AirPunch", "AirKick3" };

        if (target == null)
        {
            AttackType("GroundPunch", .2f, null, 0);
            return;
        }

        if (distance < 12)
        {
            animationCount = (int)Mathf.Repeat((float)animationCount + 1, (float)attacks.Length);
            AttackType(isLastBlow() ? attacks[Random.Range(0,attacks.Length)] : attacks[animationCount], longAttackCooldown, target, .65f);
        }

        if (GetComponentInChildren<CinemachineImpulseSource>())
            GetComponentInChildren<CinemachineImpulseSource>().m_ImpulseDefinition.m_AmplitudeGain = Mathf.Max(3,1 * distance);

    }

    void AttackType(string attackTrigger, float cooldown, EnemyScript target, float movementDuration)
    {
        animator.SetFloat("AttackType", Random.Range(0, 3));

        animator.SetTrigger(attackTrigger);

        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(isLastBlow() ? 1.5f : cooldown));

        //Check if last enemy
        if (isLastBlow())
            StartCoroutine(FinalBlowCoroutine());

        if (target == null)
        return;

        MoveTorwardsTarget(target.transform, movementDuration);

        IEnumerator AttackCoroutine(float duration)
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
        if (lockedTarget == null)
            return;

        OnHit.Invoke(lockedTarget);

        if (isLastBlow())
            lockedTarget = null;

        //Polish
        FindObjectOfType<ParticleSystemScript>().PlayParticleAtPosition(punchPosition.position);
    }

    public void DamageEvent()
    {
        animator.SetTrigger("Hit");

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);
        damageCoroutine = StartCoroutine(DamageCoroutine());

        IEnumerator DamageCoroutine()
        {
            movementInput.enabled = false;
            yield return new WaitForSeconds(.5f);
            movementInput.enabled = true;
            LerpCharacterAcceleration();
        }
    }

    bool AnEnemyIsPreparingAttack()
    {
        foreach (EnemyScript enemyScript in enemyDetection.targets)
        {
            if (enemyScript.IsPreparingAttack())
            {
                return true;
            }
        }
        return false;
    }

    EnemyScript ClosestCounterEnemy()
    {
        float minDistance = 100;
        int finalIndex = 0;

        for (int i = 0; i < enemyDetection.targets.Count; i++)
        {
            EnemyScript enemy = enemyDetection.targets[i];

            if (enemy.IsPreparingAttack())
            {
                if (Vector3.Distance(transform.position, enemy.transform.position) < minDistance)
                {
                    minDistance = Vector3.Distance(transform.position, enemy.transform.position);
                    finalIndex = i;
                }
            }
        }

        return enemyDetection.targets[finalIndex];

    }

    void LerpCharacterAcceleration()
    {
        movementInput.acceleration = 0;
        DOVirtual.Float(0, 1, .6f, ((acceleration)=> movementInput.acceleration = acceleration));
    }

    bool isLastBlow()
    {
        if (lockedTarget == null)
            return false;

        return enemyDetection.targets.Count <= 1 && lockedTarget.health <= 1;
    }

    #region Input

    private void OnCounter()
    {
        CounterCheck();
    }

    private void OnAttack()
    {
        AttackCheck();
    }

    #endregion

}
