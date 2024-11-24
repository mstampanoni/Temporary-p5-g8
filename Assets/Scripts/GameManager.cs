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

    private Enemy mCurrentSelectedEnnemy;

    // Variables pour le mode ultimate
    private bool isInUltimateMode = false;
    private bool isWaitingForConfirmation = false;

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
                                mSelectEnemy.setEnemyTarget(mCurrentSelectedEnnemy);
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

    public void StartUltimateMode()
    {
        isInUltimateMode = true;
        isWaitingForConfirmation = true;
        Debug.Log("Mode ultimate activé. Veuillez confirmer l'ennemi.");

        // Désactiver les autres actions comme l'attaque ou la compétence (si nécessaire)
        // Par exemple : Désactiver les boutons d'action dans l'UI
    }

    public void LaunchUltimate(GameObject goCharacter)
    {
        if (goCharacter.TryGetComponent<Character>(out Character character))
        {
            if (character.CanLaunchUltimate() && !isInUltimateMode)
            {
                isInUltimateMode = true;  // On entre en mode ultimate
                isWaitingForConfirmation = true;

                StartCoroutine(WaitForEnemySelection(character)); 
            }
        }
    }

    private IEnumerator WaitForEnemySelection(Character character)
    {
        Debug.Log("Mode Ultimate activé. Sélectionnez un ennemi.");

        while (isWaitingForConfirmation)
        {
            if (Input.GetMouseButtonDown(0))  
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null && enemy == mCurrentSelectedEnnemy)
                    {
                        Debug.Log("Ultimate lancé sur " + enemy.name);
                        character.Ultimate(enemy);

                        isInUltimateMode = false;
                        isWaitingForConfirmation = false;
                    }
                    else
                    {
                        Debug.Log("L'ennemi sélectionné pour l'ultimate n'est pas celui cliqué.");
                    }
                }
            }

            yield return null;
        }
    }

    private void ExecuteAction(Character character, RectTransform selectedAction)
    {
        if (selectedAction == mRotatingSelection.GetAttackCircle())
        {
            Debug.Log("Action confirmée : Attaque sur " + mCurrentSelectedEnnemy.name);
            character.Attack(mCurrentSelectedEnnemy);
        }
        else if (selectedAction == mRotatingSelection.GetCompetenceCircle())
        {
            Debug.Log("Action confirmée : Compétence sur " + mCurrentSelectedEnnemy.name);
            character.Competence(mCurrentSelectedEnnemy);
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

    private IEnumerator WaitForActionSelection(Character character)
    {
        Debug.Log(character.GetName() + " peut jouer. Sélectionnez une action.");

        if (mRotatingSelection != null)
        {
            selectedAction = mRotatingSelection.GetSelected();
        }

        while (!actionConfirmed)
        {
            if (selectedAction != null)
            {
                Debug.Log("Action potentielle sélectionnée : " +
                          (selectedAction == mRotatingSelection.GetAttackCircle() ? "Attaque" : "Compétence"));
            }
            yield return null;
        }
        ExecuteAction(character, confirmedAction);
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
                    anyonePlayed = true;

                    yield return StartCoroutine(WaitForActionSelection(character));
                }
            }

            if (!anyonePlayed)
            {
                Debug.Log("Fin du cycle, aucun personnage ne peut jouer. Réinitialisation.");
                mGlobalMin = 0f;
            }

            yield return null;
        }
    }
}
