using UnityEngine;

public class Player : Character
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
        Debug.Log(GetName() + "utilise son ultime sur" + target.GetName() + "!");
    }
}