using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    #region Init value
    [SerializeField] private List<GameObject> mPlayerPrefabs;
    [SerializeField] private List<GameObject> mEnemyPrefabs;
    [SerializeField] private Transform[] mPlayerPositions;
    [SerializeField] private Transform[] mEnemyPositions;

    [SerializeField] private RotatingSelection mRotatingSelection;

    private List<Character> mTurnQueue;
    private float mGlobalMin = 0f;
    private bool isTurnCycleRunning = false;
    private bool isRunningCoroutine = false;

    private List<Player> mActivePlayers;
    private List<Enemy> mActiveEnemies;

    //Li� au choix des actions
    private RectTransform selectedAction = null;
    private RectTransform confirmedAction = null;
    private bool actionConfirmed = false;
    #endregion

    private void Start()
    {
        mTurnQueue = new ();
        mActivePlayers = new ();
        mActiveEnemies = new ();
        SetUpGame();
    }

    private void SetUpGame()
    {
        GameObject pcContainer = GameObject.Find("Character/pc");
        GameObject npcContainer = GameObject.Find("Character/npc");

        for (int i = 0; i < mPlayerPrefabs.Count && i < mPlayerPositions.Length; i++)
        {
            GameObject player = Instantiate(mPlayerPrefabs[i], mPlayerPositions[i].position, Quaternion.identity);
            player.GetComponent<Player>().isInGame(true);
            player.transform.SetParent(pcContainer.transform);
            AddToTurnQueue(player);
        }

        for (int i = 0; i < mEnemyPrefabs.Count && i < mEnemyPositions.Length; i++)
        {
            GameObject enemy = Instantiate(mEnemyPrefabs[i], mEnemyPositions[i].position, Quaternion.identity);
            enemy.transform.SetParent(npcContainer.transform);
            enemy.GetComponent<Enemy>().isInGame(true);
            AddToTurnQueue(enemy);
        }
    }

    private void WatchForActive()
    {
        foreach (Character character in mTurnQueue) 
        {
            if (character.isInGame())
            {
                if (character.gameObject.TryGetComponent<Player>(out Player player))
                {
                    if (mActivePlayers.Contains(player))
                    {
                        if (!player.GetLifeSystem().IsAlive())
                        {
                            player.isInGame(false);
                            mActivePlayers.Remove(player);
                        }
                    }
                    else
                    {
                        if (player.GetLifeSystem().IsAlive())
                        {
                            mActivePlayers.Add(player);
                        }
                    }
                }
                else if (character.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    if (mActiveEnemies.Contains(enemy))
                    {
                        if (!enemy.GetLifeSystem().IsAlive())
                        {
                            enemy.isInGame(false);
                            mActiveEnemies.Remove(enemy);
                        }
                    }
                    else
                    {
                        if (enemy.GetLifeSystem().IsAlive())
                        {
                            mActiveEnemies.Add(enemy);
                        }
                    }
                }
            }
        }
    }

    private void AddToTurnQueue(GameObject characterObject)
    {
        Character character = characterObject.GetComponent<Character>();
        mTurnQueue.Add(character);
    }

    private float GetIncrementStep()
    {
        return Mathf.Max(
            0.1f,
            Mathf.Min(
                mTurnQueue
                    .Where(data => data != null && data.gameObject != null)
                    .Select(data => data.gameObject.GetComponent<Character>().GetSpeed())
                    .DefaultIfEmpty(1f)
                    .Min(),
                1f
            )
        );
    }

    private void Update()
    {
        WatchForActive();
        if (!isTurnCycleRunning)
        {
            StartTurnCycle();
        }
        else if (!isRunningCoroutine) 
        {
            StartCoroutine(RunTurnCycle());
        }
    }

    private Enemy GetRandomEnemy()
    {
        if (mActiveEnemies.Count > 0)
        {
            return mActiveEnemies[Random.Range(0, mActiveEnemies.Count)];
        }
        return null;
    }

    private void ExecuteAction(Character character, RectTransform selectedAction)
    {
        if (selectedAction == mRotatingSelection.GetAttackCircle())
        {
            Debug.Log("Action confirm�e : Attaque.");
            character.Attack(GetRandomEnemy());
        }
        else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
        {
            Debug.Log("Action confirm�e : Comp�tence.");
            character.Competence(GetRandomEnemy());
        }
        else
        {
            Debug.LogWarning("Aucune action valide n'a �t� s�lectionn�e.");
        }

        actionConfirmed = false;
        mRotatingSelection.setConfirmedActionNull();
    }

    public void OnClickAction()
    {
        confirmedAction = mRotatingSelection.GetConfirmed();

        if (confirmedAction != null)
        {
            actionConfirmed = true;
        }
        else
        {
            RectTransform tempAction = mRotatingSelection.GetSelected();
            if (tempAction != null && tempAction != selectedAction)
            {
                selectedAction = tempAction;
            }
        }
    }

    private IEnumerator WaitForActionSelection(Character character)
    {
        Debug.Log(character.GetName() + " peut jouer. S�lectionnez une action.");

        if (mRotatingSelection != null)
        {
            selectedAction = mRotatingSelection.GetSelected();
        }
            
        while (!actionConfirmed)
        {
            if (selectedAction != null)
            {
                Debug.Log("Action potentielle s�lectionn�e : " +
                          (selectedAction == mRotatingSelection.GetAttackCircle() ? "Attaque" : "Comp�tence"));
            }
            yield return null;
        }
        ExecuteAction(character, confirmedAction);
    }


    private void StartTurnCycle()
    {
        Debug.Log("D�but du cycle de tours.");
        mGlobalMin = 0f;
        isTurnCycleRunning = true;
        isRunningCoroutine = false;
    }

    private IEnumerator RunTurnCycle()
    {
        isRunningCoroutine = true;

        while (true)
        {
            float incrementStep = GetIncrementStep();
            mGlobalMin += incrementStep;
            bool anyonePlayed = false;

            mTurnQueue = mTurnQueue
                .Where(data => data != null && data.gameObject != null)
                .OrderByDescending(data => data.GetSpeed())
                .ToList();

            foreach (var character in mTurnQueue)
            {
                if (character == null || character.gameObject == null)
                    continue;

                if (character.GetSpeed() >= mGlobalMin)
                {
                    Debug.Log("Personnage pr�t � jouer : " + character.GetName());
                    anyonePlayed = true;

                    yield return StartCoroutine(WaitForActionSelection(character));
                }
            }

            if (!anyonePlayed)
            {
                Debug.Log("Fin du cycle, aucun personnage ne peut jouer. R�initialisation.");
                mGlobalMin = 0f;
            }

            yield return null; 
        }
    }
}
