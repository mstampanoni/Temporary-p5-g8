using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

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

    [Header("Param�tres Attaque")]

    [SerializeField]
    private float mAttackSpeed;

    [SerializeField]
    private Transform mAttackSpawnPoint;

    private string mAttackAnimName;
    private string mUltimateAnimName;

    private CinemachineVirtualCamera mCameraUltimate;
    private Transform mUltPlaceholder;


    private void Start()
    {
        animator.SetBool("idle_combat", true);

        mAttackAnimName = "attack_short_001";
        mUltimateAnimName = "zoom";

        mCameraUltimate = GameObject.FindWithTag("SKUltCam").GetComponent<CinemachineVirtualCamera>();
        mUltPlaceholder = GameObject.FindWithTag("UltPlaceholder").GetComponent<Transform>();
    }

    private void Update()
    {
    }

    private IEnumerator SyncUltimate(Character target)
    {
        Vector3 oldPos = target.gameObject.transform.position;

        target.gameObject.transform.position.Set(mUltPlaceholder.position.x, mUltPlaceholder.position.y, mUltPlaceholder.position.z);

        mCameraUltimate.LookAt = target.gameObject.transform;

        mCameraUltimate.Priority = 10;

        mCameraUltimate.GetComponent<Animator>().SetTrigger("Begin");

        CinemachineTrackedDolly dolly = mCameraUltimate.GetCinemachineComponent<CinemachineTrackedDolly>();

        while (dolly.m_PathPosition < 2.5)
        {
            yield return null; 
        }

        LaunchUltimate(target);

        yield return new WaitForSeconds(3);

        mCameraUltimate.Priority = 0;

        target.gameObject.transform.position.Set(oldPos.x, oldPos.y , oldPos.z);
    }

    private IEnumerator SyncAttack(int attackId, Character target)
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
                LaunchAttack(target);
                break;
            case 1:
                LaunchCompetence(target);
                break;
            case 2:
                StartCoroutine(SyncUltimate(target));
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
        StartCoroutine(SyncAttack(0, target));       
    }

    private void LaunchAttack(Character target)
    {
        GameObject InstanceAttack = Instantiate(mAttackPrefab, mAttackSpawnPoint.position, mAttackSpawnPoint.rotation, null);

        Rigidbody rb = InstanceAttack.GetComponent<Rigidbody>();

        Vector3 direction = (target.gameObject.transform.position - InstanceAttack.transform.position).normalized;

        rb.velocity = direction * mAttackSpeed;

        AddMana(25);
    }

    public override void Competence(Character target)
    {
        StartCoroutine(SyncAttack(1, target));
    }

    private void LaunchCompetence(Character target)
    {
        GameObject InstanceCompetence = Instantiate(mCapacityPrefab, target.gameObject.transform.position, target.gameObject.transform.rotation, null);
        Destroy(InstanceCompetence, InstanceCompetence.GetComponent<VisualEffect>().GetFloat("Lifetime"));

    }

    public override void Ultimate(Character target)
    {
        StartCoroutine(SyncAttack(2, target));
    }
    private void LaunchUltimate(Character target)
    {
        GameObject InstanceUltimate = Instantiate(mUltimatePrefab, target.gameObject.transform.position, target.gameObject.transform.rotation, null);
        Destroy(InstanceUltimate, InstanceUltimate.GetComponent<VisualEffect>().GetFloat("Lifetime"));

    }
}
