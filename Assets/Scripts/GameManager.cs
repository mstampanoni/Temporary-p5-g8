using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    #region Init value
    [Header("Prefab et position")]
    [SerializeField] private List<GameObject> mPlayerPrefabs;
    [SerializeField] private List<GameObject> mEnemyPrefabs;
    [SerializeField] private Transform[] mPlayerPositions;
    [SerializeField] private Transform[] mEnemyPositions;

    [Header("Sous-Syst�mes")]
    [SerializeField] private RotatingSelection mRotatingSelection;
    [SerializeField] private SelectEnemy mSelectEnemy;
    [SerializeField] private UIManager mUIManager;

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

    private Enemy mCurrentSelectedEnnemy;

    // Variables pour le mode ultimate
    private bool isInUltimateMode = false;
    private bool isWaitingForConfirmation = false;

    // cam link
    private LogicCam mLogicCam;
    #endregion

    private void Start()
    {
        mTurnQueue = new();
        mActivePlayers = new();
        mActiveEnemies = new();
        mCurrentSelectedEnnemy = null;
        SetUpGame();
        WatchForActive();
    }

    private void SetUpGame()
    {
        mLogicCam = GameObject.FindWithTag("LogicCam").GetComponent<LogicCam>();

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

        mLogicCam.RegisterCharcater(mTurnQueue);
        mLogicCam.Init();

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
                            mUIManager.AssignPlayerUI(player);
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
                            if (mCurrentSelectedEnnemy == null)
                            {
                                mCurrentSelectedEnnemy = enemy;
                                mSelectEnemy.SetEnemyTarget(mCurrentSelectedEnnemy);
                            }
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
        mSelectEnemy.SetAllEnemies( mActiveEnemies );
        WatchForActive();
        if (!isTurnCycleRunning)
        {
            StartTurnCycle();
        }
        else
        {
            mCurrentSelectedEnnemy = mSelectEnemy.GetSelectedEnemy();
            if (!isRunningCoroutine)
            {
                StartCoroutine(RunTurnCycle());
            }
        }
    }

    private void StartUltimateMode()
    {
        isInUltimateMode = true;
        isWaitingForConfirmation = true;
        Debug.Log("Mode ultimate activ�. Veuillez confirmer l'ennemi.");

        mUIManager.UIvisibility(false);
    }

    private void EndUltimateMode()
    {
        isInUltimateMode = false;
        isWaitingForConfirmation = false;
        Debug.Log("Mode ultimate desactiv�.");

        mUIManager.UIvisibility(true);
    }

    public void LaunchUltimate(GameObject goCharacter)
    {
        if (goCharacter.TryGetComponent<Character>(out Character character))
        {
            if (character.CanLaunchUltimate() && !isInUltimateMode)
            {
                StartUltimateMode();
                StartCoroutine(WaitForEnemySelection(character)); 
            }
        }
    }

    private IEnumerator WaitForEnemySelection(Character character)
    {
        Debug.Log("Mode Ultimate activ�. S�lectionnez un ennemi.");

        while (isWaitingForConfirmation)
        {
            if (mSelectEnemy.GetConfirmed() != null) 
            {
                Debug.Log("Ultimate lanc� sur " + mSelectEnemy.GetConfirmed().name);
                character.Ultimate(mSelectEnemy.GetConfirmed());
                character.ResetMana();
                EndUltimateMode();
            }
            yield return null;
        }
    }

    private void ExecuteAction(Character character, RectTransform selectedAction)
    {
        
        if (selectedAction == mRotatingSelection.GetAttackCircle())
        {
            Debug.Log("Action confirm�e : Attaque sur " + mCurrentSelectedEnnemy.name);
            character.Attack(mCurrentSelectedEnnemy);
        }
        else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
        {
            Debug.Log("Action confirm�e : Comp�tence sur " + mCurrentSelectedEnnemy.name);
            character.Competence(mCurrentSelectedEnnemy);
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
        mUIManager.HighlightActivePlayer(character.GetComponent<Player>());

        if (mRotatingSelection != null)
        {
            selectedAction = mRotatingSelection.GetSelected();
        }

        while (!actionConfirmed)
        {
            if (selectedAction != null)
            {
/*                Debug.Log("Action potentielle s�lectionn�e : " +
                          (selectedAction == mRotatingSelection.GetAttackCircle() ? "Attaque" : "Comp�tence"));*/
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
                    mLogicCam.SwitchTarget(character.name);

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
