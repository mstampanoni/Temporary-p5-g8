using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SelectEnemy : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.red;

    private Enemy selectedEnemy;
    private Color originalColor;
    private Renderer lastRenderer;

    public Enemy GetSelectedEnemy()
    {
        return selectedEnemy;
    }

    void Update()
    {
        HandleEnemySelection();
    }

    private void HandleEnemySelection()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                setEnemyTarget(enemy);
            }
        }
    }

    public void setEnemyTarget(Enemy target)
    {
        if (target != null)
        {
            if (lastRenderer != null)
            {
                lastRenderer.material.color = originalColor;
            }

            Renderer enemyRenderer = target.GetComponent<Renderer>();
            if (enemyRenderer != null)
            {
                lastRenderer = enemyRenderer;
                originalColor = enemyRenderer.material.color;
                enemyRenderer.material.color = highlightColor;
            }

            selectedEnemy = target;
            Debug.Log("Ennemi sélectionné : " + target.name);
        }
    }
}
