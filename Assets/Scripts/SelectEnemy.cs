using System.Collections.Generic;
using UnityEngine;

public class SelectEnemy : MonoBehaviour
{
    #region Init value
    private Enemy selectedEnemy;
    private Enemy confirmedSelectedEnemy;

    [SerializeField] private List<Enemy> allEnemies;
    #endregion

    #region Getter
    public Enemy GetSelectedEnemy()
    {
        return selectedEnemy;
    }

    public Enemy GetConfirmed()
    {
        return confirmedSelectedEnemy;
    }
    #endregion

    public void SetAllEnemies(List<Enemy> activeEnemyList)
    {
        allEnemies = activeEnemyList;
    }

    void Update()
    {
        HandleEnemySelection();

        UpdateOutlines();
    }

    private void HandleEnemySelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                SetEnemyTarget(enemy);
            }
        }
    }

    private void UpdateOutlines()
    {
        foreach (Enemy enemy in allEnemies)
        {
            Renderer enemyRenderer = enemy.GetComponent<Renderer>();

            if (enemy == selectedEnemy)
            {
                EnableOutline(enemyRenderer);
            }
            else
            {
                DisableOutline(enemyRenderer);
            }
        }
    }

    private void EnableOutline(Renderer renderer)
    {
        if (renderer != null)
        {
            foreach (var material in renderer.materials)
            {
                if (material.name.Contains("Outline Test Mat"))
                {
                    material.SetFloat("_OutlineEnabled", 1);
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
                if (material.name.Contains("Outline Test Mat"))
                {
                    material.SetFloat("_OutlineEnabled", 0);
                    break;
                }
            }
        }
    }

    public void SetEnemyTarget(Enemy target)
    {
        if (target != null)
        {
            if (target == selectedEnemy && confirmedSelectedEnemy == null)
            {
                confirmedSelectedEnemy = target;
            }
            else if (target != selectedEnemy)
            {
                confirmedSelectedEnemy = null;
                selectedEnemy = target;

                Debug.Log("Ennemi sélectionné : " + target.name);
            }
        }
    }
}
