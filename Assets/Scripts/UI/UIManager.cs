using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private RectTransform uiCanvas;  
    [SerializeField] private float xOffset = 100f;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float deselectedScale = 1f;
    [SerializeField] private float animationSpeed = 0.2f;

    private int uiCount = 0;

    private List<PlayerUIManager> playerUIs;

    private void Start()
    {
        playerUIs = new();
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
