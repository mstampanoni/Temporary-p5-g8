using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    private Slider healthSlider;
    private Image manaSlider;
    private Player mplayer;

    [SerializeField] private GameManager mGameManager; 

    public void Initialize(Player player, Transform uiInstance)
    {
        mplayer = player;
        healthSlider = uiInstance.Find("HealthSlider").GetComponent<Slider>();
        manaSlider = uiInstance.Find("Ult").GetComponent<Image>();

        if (healthSlider == null || manaSlider == null)
        {
            Debug.LogError("Les �l�ments du prefab UI sont manquants !");
            return;
        }

        healthSlider.maxValue = mplayer.GetLifeSystem().GetMaxHealth();
        healthSlider.value = mplayer.GetLifeSystem().GetCurrentHealth();

        UpdateManaSlider(mplayer.GetMana());

        mplayer.GetLifeSystem().OnHealthChanged += UpdateHealthSlider;
        mplayer.OnManaChanged += UpdateManaSlider;

        Button ultimateButton = manaSlider.GetComponent<Button>();
        if (ultimateButton != null)
        {
            ultimateButton.onClick.AddListener(() => OnUIButtonClick()); 
        }
        else
        {
            Debug.LogError("Le bouton associ� � l'image Ult n'a pas �t� trouv�.");
        }
    }

    private void UpdateHealthSlider(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }

    private void UpdateManaSlider(float currentMana)
    {
        if (manaSlider != null)
        {
            manaSlider.fillAmount = currentMana / mplayer.GetMaxMana();
        }
    }

    private void Update()
    {
        if (mplayer == null) return;

        healthSlider.value = mplayer.GetLifeSystem().GetCurrentHealth();
        manaSlider.fillAmount = mplayer.GetMana() / mplayer.GetMaxMana();
    }

    private void OnDestroy()
    {
        if (healthSlider != null && manaSlider != null)
        {
            if (mplayer != null)
            {
                mplayer.GetLifeSystem().OnHealthChanged -= UpdateHealthSlider;
                mplayer.OnManaChanged -= UpdateManaSlider;
            }
        }
    }

    private void OnUIButtonClick()
    {
        if (mGameManager != null)
        {
            mGameManager.LaunchUltimate(mplayer.gameObject); 
        }
        else
        {
            Debug.LogError("GameManager non assign� !");
        }
    }
}
