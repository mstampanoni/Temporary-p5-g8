using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonMage : Player
{
    [Header("Animator")]

    [SerializeField]
    private Animator animator;

    [Header("Prefab")]

    [SerializeField]
    private GameObject mAttackPrefab;

    [SerializeField]
    private GameObject mCapacityPrefab;

    [SerializeField]
    private GameObject mUltimatePrefab;

    [Header("Paramètres Attaque")]

    [SerializeField]
    private float mAttackSpeed;

    [SerializeField]
    private Transform mAttackSpawnPoint;

    [Header("Temp")]
    [SerializeField]
    private Character enemy;

    private void Start()
    {
        animator.SetBool("idle_combat", true);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Attack(enemy);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Competence(enemy);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Ultimate(enemy);
        }
    }

    public override void Attack(Character target)
    {
        GameObject InstanceAttack = Instantiate(mAttackPrefab, mAttackSpawnPoint);

        Rigidbody rb = InstanceAttack.GetComponent<Rigidbody>();

        Vector3 direction = (target.gameObject.transform.position - InstanceAttack.transform.position).normalized;

        rb.velocity = direction * mAttackSpeed;

    }

    public override void Competence(Character target)
    {
        Instantiate(mCapacityPrefab, target.gameObject.transform.position, target.gameObject.transform.rotation, null);
    }

    public override void Ultimate(Character target)
    {
        Instantiate(mUltimatePrefab, target.gameObject.transform.position, target.gameObject.transform.rotation, null);
    }
}
