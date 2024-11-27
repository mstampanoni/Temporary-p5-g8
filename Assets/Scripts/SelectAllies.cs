using System.Collections.Generic;
using UnityEngine;

public class SelectAllies : MonoBehaviour
{
    #region Init value
    private Player selectedAlly;
    private Player confirmedSelectedAlly;

    private List<Player> allAllies = new();
    #endregion

    #region Getter
    public Player GetSelectedAlly() => selectedAlly;
    public Player GetConfirmed() => confirmedSelectedAlly;
    #endregion

    #region Setter
    public void resetAllySelection()
    {
        selectedAlly = null;
        confirmedSelectedAlly = null;
    }
    #endregion

    public void SetAllAllies(List<Player> activeAllies)
    {
        allAllies = activeAllies;
        UpdateOutlines();
    }

    void Update()
    {
        HandleAllySelection();
        UpdateOutlines();
    }

    private void HandleAllySelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Player ally = hit.collider.GetComponent<Player>();
                SetAllyTarget(ally);
            }
        }
    }

    private void UpdateOutlines()
    {
        foreach (Player ally in allAllies)
        {
            Renderer allyRenderer = ally.GetComponent<Renderer>();

            if (ally == selectedAlly)
            {
                EnableOutline(allyRenderer);
            }
            else
            {
                DisableOutline(allyRenderer);
            }
        }
    }

    private void EnableOutline(Renderer renderer)
    {
        if (renderer != null)
        {
            foreach (var material in renderer.materials)
            {
                if (material.name.Contains("outlineAlliesMat"))
                {
                    material.SetFloat("_on_off", 0);
                    break;
                }
            }
        }
    }

    private void DisableOutline(Renderer renderer)
    {
        if (renderer != null)
        {
            foreach (var material in renderer.materials)
            {
                if (material.name.Contains("outlineAlliesMat"))
                {
                    material.SetFloat("_on_off", 1);
                    break;
                }
            }
        }
    }

    public void SetAllyTarget(Player target)
    {
        if (target != null)
        {
            if (target == selectedAlly && confirmedSelectedAlly == null)
            {
                confirmedSelectedAlly = target;
            }
            else if (target != selectedAlly)
            {
                confirmedSelectedAlly = null;
                selectedAlly = target;
                Debug.Log("Allié sélectionné : " + target.name);
            }
        }
    }
}
