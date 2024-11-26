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
    [SerializeField] private SelectAllies mSelectAllies;
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

    private bool isSelectingAlly = false; 
    private bool isSelectingEnemy = true; 
    private Player mCurrentSelectedAlly;
    private Enemy mCurrentSelectedEnemy;

    // Variables pour le mode ultimate
    private bool isInUltimateMode = false;
    private bool isWaitingForConfirmation = false;

    //Comp�tence 
    private int maxSkillPoints = 5; 
    private int currentSkillPoints;

    #endregion

    private void Start()
    {
        currentSkillPoints = 3;
        mTurnQueue = new();
        mActivePlayers = new();
        mActiveEnemies = new();
        mCurrentSelectedEnemy = null;
        SetUpGame();
        WatchForActive();
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

    #region SelectionTarget
   
    private IEnumerator WaitForAllySelection(Character character, string actionType)
    {
        Debug.Log($"S�lection d'un alli� pour l'action : {actionType}");

        while (mCurrentSelectedAlly == null)
        {
            yield return null;
        }

        if (actionType == "competence")
        {
            Debug.Log("Comp�tence lanc�e sur l'alli� : " + mCurrentSelectedAlly.name);
            character.Competence(mCurrentSelectedAlly);
        }
        else if (actionType == "ultimate")
        {
            Debug.Log("Ultime lanc� sur l'alli� : " + mCurrentSelectedAlly.name);
            character.Ultimate(mCurrentSelectedAlly);
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

    private void UpdateSelectionMode(RectTransform selectedAction)
    {
        if (selectedAction == mRotatingSelection.GetAttackCircle())
        {
            // Si l'action est une attaque, on passe en mode ennemis
            mSelectEnemy.SetAllEnemies(mActiveEnemies);
            Debug.Log("Mode s�lection d'ennemis activ�.");
        }
        else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
        {
            // Si l'action est une comp�tence
            if (currentSkillPoints > 0)
            {
                if (mTurnQueue[0].isCompetenceTargetOnAllies())
                {
                    mSelectAllies.SetAllAllies(mActivePlayers);
                    Debug.Log("Mode s�lection d'alli�s activ�.");
                }
                else
                {
                    mSelectEnemy.SetAllEnemies(mActiveEnemies);
                    Debug.Log("Mode s�lection d'ennemis activ�.");
                }
            }
            else
            {
                Debug.LogWarning("Pas assez de points de comp�tence !");
            }
        }
    }

    private IEnumerator WaitForTargetSelection(Character character, string actionType)
    {
        Debug.Log($"Mode {actionType} activ�. S�lectionnez une cible.");

        while (!actionConfirmed)
        {
            RectTransform newSelection = mRotatingSelection.GetSelected();

            if (newSelection != null && newSelection != selectedAction)
            {
                selectedAction = newSelection;
                UpdateSelectionMode(selectedAction); // Basculer entre alli�s et ennemis
            }

            if (selectedAction == mRotatingSelection.GetAttackCircle() && mCurrentSelectedEnemy != null)
            {
                Debug.Log($"Cible ennemie potentielle : {mCurrentSelectedEnemy.name}");
            }
            else if (selectedAction == mRotatingSelection.GetCompetenceCircle() && mCurrentSelectedAlly != null)
            {
                Debug.Log($"Cible alli�e potentielle : {mCurrentSelectedAlly.name}");
            }

            yield return null;
        }

        // Confirmation de l'action une fois la cible valid�e
        if (actionType == "ultimate")
        {
            if (selectedAction == mRotatingSelection.GetAttackCircle() && mCurrentSelectedEnemy != null)
            {
                Debug.Log($"Ultime lanc� sur l'ennemi : {mCurrentSelectedEnemy.name}");
                character.Ultimate(mCurrentSelectedEnemy);
            }
            else if (selectedAction == mRotatingSelection.GetCompetenceCircle() && mCurrentSelectedAlly != null)
            {
                Debug.Log($"Ultime lanc� sur l'alli� : {mCurrentSelectedAlly.name}");
                character.Ultimate(mCurrentSelectedAlly);
            }
        }

        EndUltimateMode();
    }

    #endregion

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
                            if (mCurrentSelectedEnemy == null)
                            {
                                mCurrentSelectedEnemy = enemy;
                                mSelectEnemy.SetEnemyTarget(mCurrentSelectedEnemy);
                            }
                        }
                    }
                }
            }
        }

        //mSelectAllies.SetAllAllies(mActivePlayers);
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

        UpdateSkillPointsUI();

        if (!isTurnCycleRunning)
        {
            StartTurnCycle();
        }
        else if (!isRunningCoroutine)
        {
            StartCoroutine(RunTurnCycle());
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

                // Initialisation de la s�lection en fonction de la cible par d�faut
                RectTransform initialAction = mRotatingSelection.GetCompetenceCircle(); // Par exemple, comp�tence
                if (!character.IsUltimateTargetOnAllies())
                {
                    initialAction = mRotatingSelection.GetAttackCircle(); // Sinon attaque
                }

                selectedAction = initialAction;
                UpdateSelectionMode(selectedAction); // D�termine la cible initiale (alli� ou ennemi)

                // Attente de confirmation dynamique
                StartCoroutine(WaitForTargetSelection(character, "ultimate"));
            }
        }
    }



    #region SkillPoint
    private void UpdateSkillPointsUI()
    {
        mUIManager.UpdateSkillPointsUI(currentSkillPoints); 
    }

    private bool UseSkillPoint()
    {
        if (currentSkillPoints > 0)
        {
            currentSkillPoints--;
            UpdateSkillPointsUI();
            return true;
        }
        return false;
    }

    private void GainSkillPoint()
    {
        if (currentSkillPoints < maxSkillPoints)
        {
            currentSkillPoints++;
            UpdateSkillPointsUI();
        }
    }
    #endregion

    private void ExecuteAction(Character character, RectTransform selectedAction)
    {
        if (selectedAction == mRotatingSelection.GetAttackCircle())
        {
            Debug.Log("Action confirm�e : Attaque sur " + mCurrentSelectedEnemy.name);
            character.Attack(mCurrentSelectedEnemy);
            GainSkillPoint();
        }
        else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
        {
            if (UseSkillPoint())
            {
                if (character.isCompetenceTargetOnAllies())
                {
                    Debug.Log("Action confirm�e : Comp�tence sur " + mCurrentSelectedAlly.name);
                    character.Competence(mCurrentSelectedAlly);
                }
                else
                {
                    Debug.Log("Action confirm�e : Comp�tence sur " + mCurrentSelectedEnemy.name);
                    character.Competence(mCurrentSelectedEnemy);
                }
            }
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
                UpdateSelectionMode(selectedAction); // Mise � jour du mode
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
            UpdateSelectionMode(selectedAction);
        }

        while (!actionConfirmed)
        {
            RectTransform newSelection = mRotatingSelection.GetSelected();

            if (newSelection != null && newSelection != selectedAction)
            {
                selectedAction = newSelection;
                UpdateSelectionMode(selectedAction); // Basculer la cible en fonction de la nouvelle action
            }

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
