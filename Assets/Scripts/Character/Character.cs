using UnityEngine;

public abstract class Character : MonoBehaviour
{
    #region Init Variable
    [Header("Identité du Personnage")]
    [SerializeField] private string mName;         

    [Header("Statistiques Principales")]
    [SerializeField] private float mMaxHealth;
    [SerializeField] private float mAttack;          
    [SerializeField] private float mDefense;         
    [SerializeField] private float mSpeed;           

    [Header("Statistiques Critiques")]
    [SerializeField] private float mCriticalChance; 
    [SerializeField] private float mCriticalMultiplier; 

    [Header("Statistiques pour l'ultimate")]
    [SerializeField] private float mMaxMana;            
    [SerializeField] private float mCurrentMana;
    [SerializeField] private float mChargingMana;

    [Header("Sous-Systèmes")]
    [SerializeField] private LifeSystem mLifeSystem;
    #endregion

    #region Getter
    public string GetName() { return mName; }
    public float GetAttack() { return mAttack; }
    public float GetDefense() { return mDefense; }
    public float GetSpeed() { return mSpeed; }
    public float GetCriticalChance() { return mCriticalChance; }
    public float GetCriticalMultiplier() { return mCriticalMultiplier; }
    public float GetMaxMana() { return mMaxMana; }
    public float GetMana() { return mCurrentMana; }
    public float GetChargingMana() { return mChargingMana; }
    public LifeSystem GetLifeSystem() { return mLifeSystem; } 
    #endregion

    private void Start()
    {
        mLifeSystem.Init(mMaxHealth , mDefense);
    }

    private float CalculateTotalDamage()
    {
        float critRoll = UnityEngine.Random.Range(0f, 100f);
        bool isACriticalAttack = critRoll <= mCriticalChance;

        float baseDamage = mAttack;
        if (isACriticalAttack)
        {
            baseDamage = Mathf.RoundToInt(baseDamage * mCriticalMultiplier);
            Debug.Log("Coup critique ! Dégâts" + baseDamage);
        }
        return baseDamage;
    }

    public virtual void Attack(Character target)
    {
        Debug.Log(mName + "attaque" + target.GetName() + "!");
    }

    public virtual void Competence(Character target)
    {
        Debug.Log(mName + "utilise une compétence spéciale sur" + target.GetName() + "!");
    }

    public virtual void Ultimate(Character target)
    {
        Debug.Log(mName + "utilise son ultime sur" + target.GetName() + "!");
    }

    public bool CanLaunchUltimate()
    {
        return mCurrentMana >= mMaxMana;
    }


    public void RestartMana()
    {
        mCurrentMana = 0;
    }
}
