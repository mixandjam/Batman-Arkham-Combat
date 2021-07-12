using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyScript : MonoBehaviour
{
    Animator animator;
    CombatScript playerCombat;
    EnemyDetection enemyDetection;
    CharacterController characterController;

    [Header("Stats")]
    public int health = 3;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isRecovering;

    [Header("Polish")]
    [SerializeField] private ParticleSystem counterParticle;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        playerCombat = FindObjectOfType<CombatScript>();
        enemyDetection = FindObjectOfType<EnemyDetection>();
        playerCombat.OnHit.AddListener((x) => OnHit(x));
        playerCombat.OnCounterAttack.AddListener((x) => OnCounter(x));

        StartCoroutine(PrepareAttackCoroutine());
    }

    void Update()
    {

        Vector3 dir = (playerCombat.transform.position - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction
        Vector3 movedir = Vector3.zero;

        movedir += pDir * Time.deltaTime;

        characterController.Move(movedir);

        transform.LookAt(new Vector3(playerCombat.transform.position.x, transform.position.y, playerCombat.transform.position.z));
    }

    void OnHit(EnemyScript target)
    {
        if(target == this && health > 0)
        {
            health--;

            if(health <= 0)
            {
                animator.SetTrigger("Death");

                enemyDetection.RemoveEnemy(this);
                enemyDetection.SetCurrentTarget(null);
                characterController.enabled = false;
                this.enabled = false;
                return;
            }

            animator.SetTrigger("Hit");
            transform.DOMove(transform.position - (transform.forward/2), .3f).SetDelay(.1f);

            if (isPreparingAttack)
                PrepareAttack(false);
        }

        IEnumerator HitCoroutine()
        {
            yield return new WaitForSeconds(1);

        }
    }

    void OnCounter(EnemyScript target)
    {
        if(target == this)
        {
            PrepareAttack(false);
        }
    }

    IEnumerator PrepareAttackCoroutine()
    {
        yield return new WaitForSeconds(1);
        PrepareAttack(true);
        yield return new WaitForSeconds(1);

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
            counterParticle.Clear();
            counterParticle.Stop();
        }
    }

    public bool IsAttackable()
    {
        return !isRecovering;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

}
