using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerUIPrefab;
    [SerializeField] private RectTransform uiCanvas;  
    [SerializeField] private float xOffset = 100f;     

    private int uiCount = 0;

    public void AssignPlayerUI(Player player)
    {
        if (playerUIPrefab == null || uiCanvas == null)
        {
            Debug.LogWarning("Prefab UI ou Canvas non défini !");
            return;
        }

        GameObject uiInstance = Instantiate(playerUIPrefab, uiCanvas);
        uiInstance.SetActive(true); 

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
}
