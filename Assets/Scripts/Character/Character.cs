using UnityEngine;

public abstract class Character : MonoBehaviour
{
    #region Init Variable
    [Header("Identit� du Personnage")]
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

    [Header("Sous-Syst�mes")]
    [SerializeField] private LifeSystem mLifeSystem;

    private bool inGame = false;
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
    public bool isInGame() { return inGame; }
    #endregion

    #region buff
    public void addAttack(float addAttack , int numberOfRound) { mAttack += addAttack; } // le faire avec des routines
    public void addSpeed(float addSpeed, int numberOfRound) { mSpeed += addSpeed; }
    public void addMana(float addMana, int numberOfRound) { mCurrentMana += addMana; }
    #endregion

    #region Setter
    public void isInGame(bool new_Active) { inGame = new_Active; }
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
            Debug.Log("Coup critique ! D�g�ts" + baseDamage);
        }
        return baseDamage;
    }

    #region Virtual Function
    public virtual void Attack(Character target){}
    public virtual void Competence(Character target){}
    public virtual void Ultimate(){}
    #endregion

    public bool CanLaunchUltimate()
    {
        return mCurrentMana >= mMaxMana;
    }


    public void RestartMana()
    {
        mCurrentMana = 0;
    }
}
