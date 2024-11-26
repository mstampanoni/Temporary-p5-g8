using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using static Cinemachine.CinemachineTargetGroup;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviour
{
    private Slider healthSlider;
    private Image manaSlider;
    private Player mplayer;

    [SerializeField] private GameManager mGameManager;
    private Vector3 mOriginalScale;

    public void Start()
    {
         mOriginalScale = transform.localScale;
    }

    public Player GetPlayer()
    {
        return mplayer;
    }

    public void Initialize(Player player, Transform uiInstance)
    {
        mplayer = player;
        healthSlider = uiInstance.Find("HealthSlider").GetComponent<Slider>();
        manaSlider = uiInstance.Find("Ult").GetComponent<Image>();

        if (healthSlider == null || manaSlider == null)
        {
            Debug.LogError("Les éléments du prefab UI sont manquants !");
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
            Debug.LogError("Le bouton associé à l'image Ult n'a pas été trouvé.");
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
            Debug.LogError("GameManager non assigné !");
        }
    }

    public void SetScale(float targetScale, float animationSpeed)
    {
        StartCoroutine(AnimateScale(targetScale, animationSpeed));
    }

    private IEnumerator AnimateScale(float targetScale, float animationSpeed)
    {
        transform.localScale = mOriginalScale;
        Vector3 originalScale = transform.localScale;
        Vector3 newScale = new Vector3(originalScale.x * targetScale, originalScale.y * targetScale, originalScale.z);
        float elapsedTime = 0f;

        while (elapsedTime < animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, newScale, elapsedTime / animationSpeed);
            yield return null;
        }

    }

}
