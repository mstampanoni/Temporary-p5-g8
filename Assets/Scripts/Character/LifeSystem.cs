using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSystem : MonoBehaviour 
{
    private float mMaxHealth;
    private float mCurrentHealth;
    private float mDefense; 

    public void Init(float maxHealth, float defense)
    {
        mMaxHealth = maxHealth;
        mCurrentHealth = maxHealth;
        mDefense = defense;
    }

    #region Getter
    public float GetMaxHealth() { return mMaxHealth; }
    public float GetCurrentHealth() { return mCurrentHealth; }
    public float GetDefense() { return mDefense; }
    #endregion 

    public void SetDefense(float defense)
    {
        mDefense = defense;
    }

    public void TakeDamage(float damage)
    {
        float effectiveDamage = Mathf.Max(damage - mDefense, 0);
        mCurrentHealth = Mathf.Max(mCurrentHealth - effectiveDamage, 0);

        Debug.Log("Dégâts subis" + damage + "(réduction de" + mDefense + "). Dégâts effectifs" + effectiveDamage + ". PV restants" + mCurrentHealth);
    }

    public bool CouldGetKill(float damage)
    {
        float effectiveDamage = Mathf.Max(damage - mDefense, 0);
        float temporaryHealth = mCurrentHealth;
        temporaryHealth = Mathf.Max(temporaryHealth - effectiveDamage, 0);
        if (temporaryHealth == 0) 
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Heal(float amount)
    {
        mCurrentHealth = Mathf.Min(mCurrentHealth + amount, mMaxHealth);
        Debug.Log("Soins reçus" + amount + ". PV actuels" + mCurrentHealth);
    }

    public bool IsAlive()
    {
        return mCurrentHealth > 0;
    }
}
