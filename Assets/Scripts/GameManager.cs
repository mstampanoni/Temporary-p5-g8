using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> mPlayerPrefabs;
    [SerializeField] private List<GameObject> mEnemyPrefabs;
    [SerializeField] private Transform[] mPlayerPositions;
    [SerializeField] private Transform[] mEnemyPositions;

    private List<Character> mTurnQueue;
    private float mGlobalMin = 0f;
    private bool isTurnCycleRunning = false;
    private bool isRunningCoroutine = false;

    private void Start()
    {
        mTurnQueue = new ();
        SetUpGame();
    }

    private void SetUpGame()
    {
        GameObject pcContainer = GameObject.Find("Character/pc");
        GameObject npcContainer = GameObject.Find("Character/npc");

        for (int i = 0; i < mPlayerPrefabs.Count && i < mPlayerPositions.Length; i++)
        {
            GameObject player = Instantiate(mPlayerPrefabs[i], mPlayerPositions[i].position, Quaternion.identity);
            player.transform.SetParent(pcContainer.transform);
            AddToTurnQueue(player);
        }

        for (int i = 0; i < mEnemyPrefabs.Count && i < mEnemyPositions.Length; i++)
        {
            GameObject enemy = Instantiate(mEnemyPrefabs[i], mEnemyPositions[i].position, Quaternion.identity);
            enemy.transform.SetParent(npcContainer.transform);
            AddToTurnQueue(enemy);
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
        if (!isTurnCycleRunning)
        {
            StartTurnCycle();
        }
        else if (!isRunningCoroutine) 
        {
            StartCoroutine(RunTurnCycle());
        }
    }

    private void StartTurnCycle()
    {
        Debug.Log("Début du cycle de tours.");
        mGlobalMin = 0f;
        isTurnCycleRunning = true;
        isRunningCoroutine = false;
    }

    private IEnumerator RunTurnCycle()
    {
        isRunningCoroutine = true;
        float incrementStep = GetIncrementStep();
        mGlobalMin += incrementStep;
        bool anyonePlayed = false;

        mTurnQueue = mTurnQueue
            .Where(data => data != null && data.gameObject != null)
            .OrderByDescending(data => data.gameObject.GetComponent<Character>().GetSpeed())
            .ToList();

        int checkCharacter = 0;
        foreach (var character in mTurnQueue)
        {
            checkCharacter++;
            if (character == null || character.gameObject == null)
            {
                mTurnQueue.Remove(character);
                continue;
            }

            if (character.GetSpeed() >= mGlobalMin)
            {
                Debug.Log("Personnage prêt à jouer : " + character.gameObject.name);

                yield return WaitForMouseClick();

                ExecuteTurn(character);
                anyonePlayed = true;
            }

            if (checkCharacter == mTurnQueue.Count)
            {
                StartCoroutine(RunTurnCycle());
            }
        }

        if (!anyonePlayed)
        {
            Debug.Log("Fin du cycle, aucun personnage ne peut jouer.");
            isRunningCoroutine = false;
        }
    }

    private IEnumerator WaitForMouseClick()
    {
        while (!Input.GetKeyDown("space")) 
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
    }


    private void ExecuteTurn(Character character)
    { 
        Debug.Log(character.GetName() + "Attaque !");

        // Placeholder pour gérer les actions spécifiques
        //character.PerformAction();
    }

    //void OnApplicationQuit()
    //{
    //    if (mTurnQueue != null)
    //    {
    //        foreach (var character in mTurnQueue)
    //        {
    //            if (character != null && character.gameObject != null)
    //            {
    //                Destroy(character.gameObject);
    //            }
    //        }
    //        mTurnQueue.Clear();
    //        mTurnQueue = null;
    //    }
    //}

}
