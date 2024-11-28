using System.Collections;
using System.Collections.Generic;
//using Unity.Android.Types;
using UnityEngine;
using UnityEngine.VFX;

public class OrkPlayer : Player
{
    public Animator animator;
    public GameObject Slash;
    public Character Target;
    public GameObject WeaponLeft;
    public GameObject WeaponRight;

    public GameObject WeaponLeftReal;
    public GameObject WeaponRightReal;

    public Material DaggerGreenMat;

    public GameObject daggerEffect;

    public override void Attack(Character target)
    {
        Debug.Log(GetName() + "attaque" + target.GetName() + "!");
        Target = target;
        animator.Play("ComboAttack");

    }

    public override void Competence(Character target)
    {
        Debug.Log(GetName() + "utilise une comp�tence sp�ciale sur" + target.GetName() + "!");


        animator.Play("PointDagger");


    }

    public override void Ultimate(Character target)
    {
        Debug.Log(GetName() + "utilise son ultime sur");

        animator.Play("TauntCry");
    }

    public void GenerateSlashFromWeaponLeft()
    {
        GenerateSlashFromWeapon(WeaponLeft);
    }

    public void GenerateSlashFromWeaponRight()
    {
        GenerateSlashFromWeapon(WeaponRight);
    }

    private void GenerateSlashFromWeapon(GameObject weapon)
    {
        if (Target == null)
        {
            Debug.Log("NoTarget");
            return;
        }

        // R�cup�rer les positions de l'arme et de la cible
        Vector3 weaponPosition = weapon.transform.position;
        Vector3 targetPosition = Target.transform.position;

        // Calculer la position du Slash
        Vector3 slashPosition = weaponPosition + (targetPosition - weaponPosition);

        // Instancier le Slash � la position calcul�e avec la rotation de l'arme
        Instantiate(Slash, slashPosition, weapon.transform.rotation);

        Debug.Log("Slash instantiated at: " + slashPosition);
    }

    public void DaggerShader()
    {
        if (daggerEffect == null)
        {
            Debug.LogError("Dagger particle effect is not assigned!");
            return;
        }

        // Positionner et lancer l'effet de particules sur WeaponLeft
        GameObject effect = Instantiate(daggerEffect, WeaponLeft.transform.position, WeaponLeft.transform.rotation);

        WeaponLeftReal.GetComponent<Renderer>().material = DaggerGreenMat;
        WeaponRightReal.GetComponent<Renderer>().material = DaggerGreenMat;
    }

}