using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> mPlayerPrefabs; // Liste des prefabs des joueurs
    [SerializeField] private List<GameObject> mEnemyPrefabs;  // Liste des prefabs des ennemis
    [SerializeField] private Transform[] mPlayerPositions;   // Positions prédéfinies pour les joueurs
    [SerializeField] private Transform[] mEnemyPositions;    // Positions prédéfinies pour les ennemis

    private List<GameObject> mActivePlayers = new List<GameObject>(); 
    private List<GameObject> mActiveEnemies = new List<GameObject>(); 

    private void Start()
    {
        SetUpGame();
    }

    private void SetUpGame()
    {
        for (int i = 0; i < mPlayerPrefabs.Count && i < mPlayerPositions.Length; i++)
        {
            GameObject player = Instantiate(mPlayerPrefabs[i], mPlayerPositions[i].position, Quaternion.identity);
            mActivePlayers.Add(player);
        }

        for (int i = 0; i < mEnemyPrefabs.Count && i < mEnemyPositions.Length; i++)
        {
            GameObject enemy = Instantiate(mEnemyPrefabs[i], mEnemyPositions[i].position, Quaternion.identity);
            mActiveEnemies.Add(enemy);
        }
    }

    public List<GameObject> GetActivePlayers()
    {
        return mActivePlayers;
    }

    public List<GameObject> GetActiveEnemies()
    {
        return mActiveEnemies;
    }
}
