using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyScript : MonoBehaviour
{
    public int health = 3;

    Animator animator;
    CombatScript playerCombat;

    public bool preparingAttack;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerCombat = FindObjectOfType<CombatScript>();
        playerCombat.OnHit.AddListener((x) => OnHit(x));
    }

    void Update()
    {
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

                FindObjectOfType<EnemyDetection>().RemoveEnemy(this);
                FindObjectOfType<EnemyDetection>().SetCurrentTarget(null);
                GetComponent<CharacterController>().enabled = false;
                this.enabled = false;
                return;
            }
            animator.SetTrigger("Hit");
            transform.DOMove(transform.position - (transform.forward/2), .3f).SetDelay(.1f);
        }
    }

}
