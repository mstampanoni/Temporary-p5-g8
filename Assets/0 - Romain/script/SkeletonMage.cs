using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SkeletonMage : Player
{
    [Header("Animator")]

    [SerializeField]
    private Animator animator;

    [Header("Camera Ultimate")]

    [SerializeField]
    private CinemachineVirtualCamera mCameraUltimate;

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

    private string mAttackAnimName;
    private string mUltimateAnimName;

    private void Start()
    {
        base.Start();
        animator.SetBool("idle_combat", true);

        mAttackAnimName = "attack_short_001";
        mUltimateAnimName = "zoom";
}

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {       
            StartCoroutine(SyncAttack(0));
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            StartCoroutine(SyncAttack(1));
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            StartCoroutine(SyncAttack(2));
        }
    }

    private IEnumerator SyncUltimate()
    {
        mCameraUltimate.Priority = 10;

        mCameraUltimate.GetComponent<Animator>().SetTrigger("Begin");

        CinemachineTrackedDolly dolly = mCameraUltimate.GetCinemachineComponent<CinemachineTrackedDolly>();

        while (dolly.m_PathPosition < 2.5)
        {
            yield return null; 
        }

        Ultimate(enemy);

        yield return new WaitForSeconds(3);

        mCameraUltimate.Priority = 10;

    }

    private IEnumerator SyncAttack(int attackId)
    {
        Animator animator = GetComponent<Animator>();

        AnimationClip clip = FindAnimationClip(animator, mAttackAnimName);

        animator.Play(mAttackAnimName, -1, 0);

        if (clip != null)
        {
            yield return new WaitForSeconds(clip.length/1.75f);
        }

        switch (attackId)
        {
            case 0:
                Attack(enemy);
                break;
            case 1:
                Competence(enemy);
                break;
            case 2:
                StartCoroutine(SyncUltimate());
                break;
            default:
                break;
        }
    }

    private AnimationClip FindAnimationClip(Animator animator, string animationName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
                return clip;
        }
        return null;
    }

    public override void Attack(Character target)
    {
        GameObject InstanceAttack = Instantiate(mAttackPrefab, mAttackSpawnPoint.position, mAttackSpawnPoint.rotation, null);


        Rigidbody rb = InstanceAttack.GetComponent<Rigidbody>();

        Vector3 direction = (target.gameObject.transform.position - InstanceAttack.transform.position).normalized;

        rb.velocity = direction * mAttackSpeed;
        
    }

    public override void Competence(Character target)
    {
        GameObject InstanceCompetence = Instantiate(mCapacityPrefab, target.gameObject.transform.position, target.gameObject.transform.rotation, null);
        Destroy(InstanceCompetence, InstanceCompetence.GetComponent<VisualEffect>().GetFloat("Lifetime"));

    }

    public override void Ultimate(Character target)
    {
        GameObject InstanceUltimate = Instantiate(mUltimatePrefab, target.gameObject.transform.position, target.gameObject.transform.rotation, null);
        Destroy(InstanceUltimate, InstanceUltimate.GetComponent<VisualEffect>().GetFloat("Lifetime"));

    }
}
