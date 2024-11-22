using System;
using UnityEngine;

public class Enemy : Character
{
    public enum BehaviorType
    {
        LowDefense,
        HighAttack,
        LowHealth,
        RandomAttack
    }

    [Header("Comportement IA")]
    [SerializeField] private BehaviorType behaviorType;

    public void DecideAction(Player[] pc)
    {
        Player target = null;

        switch (behaviorType)
        {
            case BehaviorType.LowDefense:
                target = FindOptimalTarget(pc, c => c.GetDefense(), false);
                break;

            case BehaviorType.HighAttack:
                target = FindOptimalTarget(pc, c => c.GetAttack(), true);
                break;

            case BehaviorType.LowHealth:
                target = FindOptimalTarget(pc, c => c.GetLifeSystem().GetCurrentHealth(), false);
                break;

            case BehaviorType.RandomAttack:
                target = ChooseRandomTarget(pc);
                break;
        }

        int randomAttackRoll = UnityEngine.Random.Range(1, 3);
        if (randomAttackRoll == 3)
        {
            target = ChooseRandomTarget(pc);
        }

        Player killingTarget = CheckForKillingBlow(pc);
        if (killingTarget != null)
        {
            target = killingTarget;
            Debug.Log($"{GetName()} cible {target.GetName()} pour l'achever !");
        }

        Attack(target);
    }

    /// <summary>
    /// Recherche la cible optimale dans une liste en fonction d'un critère.
    /// </summary>
    /// <param name="pc">Liste des personnages.</param>
    /// <param name="selector">Fonction qui retourne la valeur à comparer.</param>
    /// <param name="findMax">True pour trouver le maximum, False pour le minimum.</param>
    /// <returns>Le personnage optimal.</returns>
    private Player FindOptimalTarget(Player[] pc, Func<Player, float> selector, bool findMax)
    {
        Player optimalTarget = pc[0];
        float optimalValue = selector(optimalTarget);

        foreach (var thisPlayer in pc)
        {
            float value = selector(thisPlayer);
            if ((findMax && value > optimalValue) || (!findMax && value < optimalValue))
            {
                optimalTarget = thisPlayer;
                optimalValue = value;
            }
        }

        return optimalTarget;
    }

    private Player ChooseRandomTarget(Player[] pc)
    {
        int randomIndex = UnityEngine.Random.Range(0, pc.Length);
        return pc[randomIndex];
    }

    private Player CheckForKillingBlow(Player[] pc)
    {
        foreach (var thisPlayer in pc)
        {
            if (thisPlayer.GetLifeSystem().CouldGetKill(GetAttack()))
            {
                return thisPlayer;
            }
        }
        return null;
    }
}
