using System.Collections;
using System.Collections.Generic;
//using Unity.Android.Types;
using UnityEngine;

public class OrkPlayer : Player
{
    public Animator animator;

    public override void Attack(Character target)
    {
        Debug.Log(GetName() + "attaque" + target.GetName() + "!");

        animator.Play("ComboAttack");
    }

    public override void Competence(Character target)
    {
        Debug.Log(GetName() + "utilise une comp�tence sp�ciale sur" + target.GetName() + "!");

        animator.Play("Cast");

    }

    public override void Ultimate(Character target)
    {
        Debug.Log(GetName() + "utilise son ultime sur");

        animator.Play("TauntCry");
    }

    public void GenerateSlash()
    {
        if (gameObject.GetComponent<Transform>().Find("weapon2_left")== null)
        {
            Debug.Log("WeaponLeft Failed find");
        }

        if (gameObject.GetComponent<Transform>().Find("weapon2_right") == null)
        {
            Debug.Log("WeaponRight Failed find" );
        }

    }

}