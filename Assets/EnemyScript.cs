using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    Animator animator;
    CombatScript playerCombat;

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

    void OnHit(Transform target)
    {
        if(transform == target)
        {
            animator.SetTrigger("Hit");
        }
    }
}
