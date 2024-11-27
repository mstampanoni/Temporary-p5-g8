using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrkPlayer : Player
{

    public override void Attack(Character target)
    {
        Debug.Log(GetName() + "attaque" + target.GetName() + "!");


    }

    public override void Competence(Character target)
    {
        Debug.Log(GetName() + "utilise une compétence spéciale sur" + target.GetName() + "!");
    }

    public override void Ultimate(Character target)
    {
        Debug.Log(GetName() + "utilise son ultime sur");
    }
}