using System.Collections;
using UnityEngine;
using DG.Tweening;

public class EnemyScript : MonoBehaviour
{
    //Declarations
    private Animator animator;
    private CombatScript playerCombat;
    private EnemyManager enemyManager;
    private EnemyDetection enemyDetection;
    private CharacterController characterController;

    [Header("Stats")]
    public int health = 3;
    private float moveSpeed = 1;
    private Vector3 moveDirection;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isRecovering;
    [SerializeField] private bool isMoving;

    [Header("Polish")]
    [SerializeField] private ParticleSystem counterParticle;

    private Coroutine PrepareAttackCoroutine;

    void Start()
    {
        enemyManager = GetComponentInParent<EnemyManager>();

        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        playerCombat = FindObjectOfType<CombatScript>();
        enemyDetection = playerCombat.GetComponentInChildren<EnemyDetection>();

        playerCombat.OnHit.AddListener((x) => OnHit(x));
        playerCombat.OnCounterAttack.AddListener((x) => OnCounter(x));
    }

    void Update()
    {
        //Constantly look at player
        transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));

        //Only moves if the direction is set
        MoveEnemy(moveDirection);
    }

    //Listened event from Player Animation
    void OnHit(EnemyScript target)
    {
        if(target == this && health > 0)
        {
            enemyDetection.SetCurrentTarget(null);

            health--;

            if(health <= 0)
            {
                Death();
                return;
            }

            animator.SetTrigger("Hit");
            transform.DOMove(transform.position - (transform.forward/2), .3f).SetDelay(.1f);

            if(PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

            if (isPreparingAttack)
                PrepareAttack(false);

            StopMoving();
        }
    }

    void OnCounter(EnemyScript target)
    {
        if(target == this)
        {
            PrepareAttack(false);
        }
    }

    void Death()
    {
        this.enabled = false;
        animator.SetTrigger("Death");
        enemyManager.SetEnemyAvailiability(this, false);
        characterController.enabled = false;
    }

    IEnumerator PrepAttack()
    {
        PrepareAttack(true);
        yield return new WaitForSeconds(.2f);
        moveDirection = Vector3.forward;
        isMoving = true;
    }

    void PrepareAttack(bool active)
    {
        isPreparingAttack = active;

        if (active)
        {
            counterParticle.Play();
        }
        else
        {
            StopMoving();
            counterParticle.Clear();
            counterParticle.Stop();
        }
    }

    void MoveEnemy(Vector3 direction)
    {
        moveSpeed = direction == Vector3.forward ? 4 : 1;

        animator.SetFloat("InputMagnitude", characterController.velocity.normalized.magnitude/(5/moveSpeed), .2f, Time.deltaTime);
        animator.SetBool("Strafe", (direction == Vector3.right || direction == Vector3.left));
        animator.SetFloat("StrafeDirection", direction.normalized.x);

        if (!isMoving)
            return;

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction
        Vector3 movedir = Vector3.zero;

        movedir += (direction == Vector3.forward ? dir : (pDir * direction.normalized.x)) * moveSpeed * Time.deltaTime;

        characterController.Move(movedir);

        if(Vector3.Distance(transform.position, playerCombat.transform.position) < 2)
        {
            StopMoving();
            if (!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
                Attack();
            else
                PrepareAttack(false);
        }
    }

    private void Attack()
    {
        transform.DOMove(transform.position + (transform.forward / 1), .5f);
        animator.SetTrigger("AirPunch");
    }

    public void HitEvent()
    {
        if(!playerCombat.isCountering && !playerCombat.isAttackingEnemy)
            playerCombat.DamageEvent();

        PrepareAttack(false);
    }

    private void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
        characterController.Move(moveDirection);
    }
    public bool IsAttackable()
    {
        return !isRecovering && health > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

}
