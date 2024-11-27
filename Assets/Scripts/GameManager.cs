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

    [Header("Sous-Systèmes")]
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

    //Lié au choix des actions
    private RectTransform selectedAction = null;
    private RectTransform confirmedAction = null;
    private bool actionConfirmed = false;

    private bool isSelectingAlly = false; 
    private bool isSelectingEnemy = true; 
    private Player mCurrentSelectedAlly;
    private Enemy mCurrentSelectedEnemy;

    private Character currentCharacter;
    private bool haveToReset;

    // Variables pour le mode ultimate
    private bool isInUltimateMode = false;
    private bool isWaitingForConfirmation = false;

    //Compétence 
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
        mCurrentSelectedAlly = null;
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
    private void UpdateSelectionMode()
    {
        if (selectedAction == mRotatingSelection.GetAttackCircle())
        {
            mSelectEnemy.SetAllEnemies(mActiveEnemies);
            if (mCurrentSelectedEnemy == null)
            {
                GiveFirstEnemy();
            }
            Debug.Log("Mode sélection d'ennemis activé.");
        }

        else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
        {
            if (currentSkillPoints > 0)
            {
                if (currentCharacter.isCompetenceTargetOnAllies())
                {
                    mSelectAllies.SetAllAllies(mActivePlayers);
                    mSelectEnemy.resetEnnemySelection();
                    if (mCurrentSelectedAlly == null)
                    {
                        GiveFirstAlly();
                    }
                    Debug.Log("Mode sélection d'alliés activé.");
                }
                else
                {
                    mSelectEnemy.SetAllEnemies(mActiveEnemies);
                    mSelectAllies.resetAllySelection();
                    if (mCurrentSelectedEnemy == null)
                    {
                        GiveFirstEnemy();
                    }
                    Debug.Log("Mode sélection d'ennemis activé.");
                }
            }
            else
            {
                Debug.LogWarning("Pas assez de points de compétence !");
            }
        }
    }

    private void GiveFirstAlly()
    {
        if (mActivePlayers != null && mActivePlayers.Count > 0)
        {
            Player first = mActivePlayers[0];
            mCurrentSelectedAlly = first;
            mSelectAllies.SetAllyTarget(first);
        }
        else
        {
            Debug.LogWarning("Aucun ennemi actif disponible pour la sélection.");
        }
    }

    private void GiveFirstEnemy()
    {
        if (mActiveEnemies!= null && mActiveEnemies.Count > 0)
        {
            Enemy first = mActiveEnemies[0];
            mCurrentSelectedEnemy = first;
            mSelectEnemy.SetEnemyTarget(first); 
        }
        else
        {
            Debug.LogWarning("Aucun ennemi actif disponible pour la sélection.");
        }
    }
    #endregion

    private IEnumerator WaitForTargetSelection()
    {
        Character target = null;

        Debug.Log("Mode sélection activé. Veuillez choisir une cible.");

        while (!actionConfirmed)
        {
            RectTransform newSelection = mRotatingSelection.GetSelected();

            if (newSelection != null && newSelection != selectedAction)
            {
                selectedAction = newSelection;

                if (!isInUltimateMode)
                {
                    if (!currentCharacter.IsUltimateTargetOnAllies())
                    {
                        if (mCurrentSelectedEnemy != null)
                        {
                            Debug.Log($"Cible ennemie potentielle : {mCurrentSelectedEnemy.name}");
                            target = mCurrentSelectedEnemy;
                        }
                    }
                    else
                    {
                        if (mCurrentSelectedAlly != null)
                        {
                            Debug.Log($"Cible alliée potentielle : {mCurrentSelectedAlly.name}");
                            target = mCurrentSelectedAlly;
                        }
                    }
                }
                else
                {
                    if (selectedAction == mRotatingSelection.GetAttackCircle() && mCurrentSelectedEnemy != null)
                    {
                        Debug.Log($"Cible ennemie potentielle : {mCurrentSelectedEnemy.name}");
                        target = mCurrentSelectedEnemy;
                    }
                    else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
                    {
                        if (currentCharacter.isCompetenceTargetOnAllies() && mCurrentSelectedAlly != null)
                        {
                            Debug.Log($"Cible alliée potentielle : {mCurrentSelectedAlly.name}");
                            target = mCurrentSelectedAlly;
                        }
                        else if (mCurrentSelectedEnemy != null)
                        {
                            Debug.Log($"Cible ennemie potentielle : {mCurrentSelectedEnemy.name}");
                            target = mCurrentSelectedEnemy;
                        }
                    }
                }
            }
            yield return null;
        }

        if (isInUltimateMode)
        {
            if (target != null)
            {
                Debug.Log($"Ultime lancé sur : {target.GetName()}");
                currentCharacter.Ultimate(target);
                currentCharacter.ResetMana();
                EndUltimateMode();
            }
        }
        else
        {
            ExecuteAction(currentCharacter, target);
        }

        actionConfirmed = false;
        mRotatingSelection.setConfirmedActionNull();
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
        mCurrentSelectedAlly = mSelectAllies.GetSelectedAlly();
        mCurrentSelectedEnemy = mSelectEnemy.GetSelectedEnemy();
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

        UpdateSelectionMode();
    }

    #region Ultimate stuff
    private void StartUltimateMode()
    {
        isInUltimateMode = true;
        isWaitingForConfirmation = true;
        Debug.Log("Mode ultimate activé. Veuillez confirmer l'ennemi.");

        mUIManager.UIvisibility(false);
    }

    private void EndUltimateMode()
    {
        isInUltimateMode = false;
        isWaitingForConfirmation = false;
        Debug.Log("Mode ultimate desactivé.");

        mUIManager.UIvisibility(true);
    }

    public void LaunchUltimate(GameObject goCharacter)
    {
        if (goCharacter.TryGetComponent<Character>(out Character character))
        {
            if (character.CanLaunchUltimate() && !isInUltimateMode)
            {
                StartUltimateMode();

                RectTransform initialAction = mRotatingSelection.GetCompetenceCircle(); 
                if (!character.IsUltimateTargetOnAllies())
                {
                    initialAction = mRotatingSelection.GetAttackCircle(); 
                }

                selectedAction = initialAction;
                //UpdateSelectionMode(selectedAction, character); 

                StartCoroutine(WaitForTargetSelection());
            }
        }
    }
    #endregion

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

    private void ExecuteAction(Character character, Character target)
    {
        if (confirmedAction == mRotatingSelection.GetAttackCircle())
        {
            if (target != null)
            {
                Debug.Log($"Action confirmée : Attaque sur {target.name}");
                character.Attack(target);
                GainSkillPoint(); 
            }
        }
        else if (confirmedAction == mRotatingSelection.GetCompetenceCircle())
        {
            if (UseSkillPoint())
            {
                if (character.isCompetenceTargetOnAllies())
                {
                    if (target != null)
                    {
                        Debug.Log($"Action confirmée : Compétence sur l'allié {target.name}");
                        character.Competence(target);
                    }
                }
                else
                {
                    if (target != null)
                    {
                        Debug.Log($"Action confirmée : Compétence sur l'ennemi {target.name}");
                        character.Competence(target);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Pas assez de points de compétence !");
            }
        }
        else
        {
            Debug.LogWarning("Aucune action valide n'a été sélectionnée.");
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
                    Debug.Log("Personnage prêt à jouer : " + character.GetName());
                    currentCharacter = character;
                    anyonePlayed = true;

                    yield return StartCoroutine(WaitForTargetSelection());
                }
            }

            if (!anyonePlayed)
            {
                currentCharacter = null;
                Debug.Log("Fin du cycle, aucun personnage ne peut jouer. Réinitialisation.");
                mGlobalMin = 0f;
            }

            yield return null;
        }
    }

}
