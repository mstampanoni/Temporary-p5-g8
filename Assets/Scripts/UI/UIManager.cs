using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private RectTransform uiCanvas;  
    [SerializeField] private float xOffset = 75f;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float deselectedScale = 1f;
    [SerializeField] private float animationSpeed = 0.2f;

    [Header("Skill Points UI")]
    [SerializeField] private List<GameObject> skillPointObjects;
    [SerializeField] private AnimationCurve bounceAnimationCurve;

    private int uiCount = 0;

    private List<PlayerUIManager> playerUIs;

    private void Start()
    {
        playerUIs = new();
        for (int i = 0; i < skillPointObjects.Count; i++)
        {
            skillPointObjects[i].GetComponent<SkillPointAnimation>().setCurve(bounceAnimationCurve);
        }
    }

    public void AssignPlayerUI(Player player)
    {
        if (playerUIPrefab == null || uiCanvas == null)
        {
            Debug.LogWarning("Prefab UI ou Canvas non défini !");
            return;
        }

        GameObject uiInstance = Instantiate(playerUIPrefab, uiCanvas);
        uiInstance.SetActive(true); 
        playerUIs.Add(uiInstance.GetComponent<PlayerUIManager>());

        RectTransform rectTransform = uiInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += new Vector2(uiCount * xOffset, 0);
        }

        PlayerUIManager uiManager = uiInstance.GetComponent<PlayerUIManager>();
        if (uiManager != null)
        {
            uiManager.Initialize(player, uiInstance.transform);
        }

        uiCount++; 
    }

    public void UpdateSkillPointsUI(int currentPoints)
    {
        for (int i = 0; i < skillPointObjects.Count; i++)
        {
            if (i < currentPoints)
            {
                if (!skillPointObjects[i].activeSelf)
                {
                    skillPointObjects[i].SetActive(true);
                    
                }
                else
                {
                    skillPointObjects[i].GetComponent<SkillPointAnimation>().setCurve(bounceAnimationCurve);
                }
            }
            else
            {
                skillPointObjects[i].SetActive(false);
            }
        }
    }

    public void UIvisibility(bool isActive)
    {
        uiCanvas.gameObject.SetActive(isActive);
    }

    public void HighlightActivePlayer(Player activePlayer)
    {
        foreach (var playerUI in playerUIs)
        {
            if (playerUI.GetPlayer() == activePlayer)
            {
                playerUI.SetScale(selectedScale, animationSpeed);
            }
            else
            {
                playerUI.SetScale(deselectedScale, animationSpeed);
            }
        }
    }
}
