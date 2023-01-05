using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEditor.SceneTemplate;

public class FightSceneController : MonoBehaviour
{
    public static FightSceneController Instance { get; private set; }
    public List<GameObject> Characters = new List<GameObject>();
    public GameObject CharacterObj;

    public SceneTemplateAsset templateAsset;
    public GameObject Sign;
    public static FightState State;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject graph;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private List<GameObject> spawnPointsObjects;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameObject ratPrefab;
    public static Queue<GameObject> CharactersQueue {get; private set;}
    private GameObject currentCharacterNodeObj;
    private List<Vector3> allySpawnPoints;
    private List<Vector3> enemySpawnPoints;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance == this)
            Destroy(gameObject); 
    }

    private void Start() 
    {
        Init();
    }

    private void Init()
    {
        CreateSpawnPointsLists();
        CreateCharactersList();
        InitializeCharacters();
        NodesNav.InitializeNodesLists(graph);
        Debug.Log(UIController.Instance);
        UIController.Instance.CreateUI();

        StateController.MakeAvailablePhases();
        CharacterObj = CharactersQueue.Dequeue();
        AI.CurrentCharacterObj = CharacterObj;
        Debug.Log(CharactersQueue.Count);
        DrawAreas();
        State = FightState.Sleeping;
        SetNearestNodeToCurrentCharacter();
        
        Sign.transform.position = CharacterObj.transform.position + new Vector3(0, 1.3f, 0);
        Sign.transform.parent = CharacterObj.transform;
    }

    private void Update()
    {
        lineRenderer.positionCount = 0;

        if (CharacterObj.GetComponent<FightCharacter>().Type == CharacterType.Ally)
        {
            var point = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            var cameraRay = mainCamera.ScreenPointToRay(point);
            Physics.Raycast(cameraRay, out var hit);
            switch (State)
            {
                case FightState.MovePhase:
                    CalculateAvailalePathToPoint(hit, false);
                    break;
                case FightState.FightPhase:
                    CalculateAvailalePathToPoint(hit, true);
                    break;
                case FightState.EndTurnPhase:
                    EndTurn();
                    break;
            }

            if (Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject())
                MakeAction(State, hit.transform.gameObject);
        }
        else
        {
            var decision = AI.MakeDecision(CharactersQueue);
            MakeAction(decision.State, decision.Target);
        }
        
        if (State == FightState.Moving || State == FightState.Fighting)
        {
            if (NodesNav.IsEndMoveCharacter())
            {
                CharacterReachTarget();
            }
            else
                NodesNav.MoveCharacterByPath(CharacterObj);
        }
    }

    private void MakeAction(FightState state, GameObject targetObj)
    {
        switch (state)
        {
            case FightState.MovePhase:
                MoveCharacter();
                break;
            case FightState.FightPhase:
                Fight(targetObj);
                break;
            case FightState.ShootPhase:
                Shoot(targetObj);
                break;
            case FightState.EndTurnPhase:
                EndTurn();
                break;
        }
    }

    private void CreateSpawnPointsLists()
    {
        allySpawnPoints = spawnPointsObjects
                            .Where(p => p.GetComponent<FightSpawnPoint>().Type == CharacterType.Ally)
                            .Select(p => p.transform.position)
                            .ToList();
        enemySpawnPoints = spawnPointsObjects
                            .Where(p => p.GetComponent<FightSpawnPoint>().Type == CharacterType.Enemy)
                            .Select(p => p.transform.position)
                            .ToList();
    }

    private void CreateCharactersList()
    {
        var data = FightSceneLoader.CurrentData;
        foreach (var entity in data.ally)
        {
            var obj = Instantiate(characterPrefab, new Vector3(0,0,0), Quaternion.identity);
            var objHeight = obj.GetComponent<MeshRenderer>().bounds.size.y;
            obj.transform.position = allySpawnPoints[allySpawnPoints.Count - 1] + new Vector3(0, objHeight / 2, 0);
            allySpawnPoints.RemoveAt(allySpawnPoints.Count - 1);
            obj.AddComponent<FightCharacter>().ApplyProperties(entity, CharacterType.Ally);
            obj.GetComponent<Renderer>().material.color = Color.green;
            Characters.Add(obj);
        }

        foreach (var entity in data.enemies)
        {
            var entityObj = Instantiate(entity, new Vector3(0,0,0), Quaternion.identity);
            var obj = entityObj.gameObject;
            var objHeight = obj.GetComponent<MeshRenderer>().bounds.size.y;
            obj.transform.position = enemySpawnPoints[enemySpawnPoints.Count - 1] + new Vector3(0, objHeight / 2, 0);
            enemySpawnPoints.RemoveAt(enemySpawnPoints.Count - 1);
            obj.AddComponent<FightCharacter>().ApplyProperties(entityObj, CharacterType.Enemy);
            obj.GetComponent<Renderer>().material.color = Color.red;
            Characters.Add(obj);
        }

        Characters = Characters
            .OrderByDescending(c => c.GetComponent<FightCharacter>().Initiative)
            .ToList();
    }

    private void InitializeCharacters()
    {
        CharactersQueue = new Queue<GameObject>();
        for (var i = 0; i < Characters.Count; i++)
        {
            var component = Characters[i].GetComponent<FightCharacter>();
            //component.RangeWeapon = new Rifle(10);
            //component.MeleeWeapon = new Knife(3, 0.3f);
            CharactersQueue.Enqueue(Characters[i]);
        }

        Debug.Log("CharacterQueue Count: " + CharactersQueue.Count);
    }

    private void CharacterReachTarget()
    {
        NodesNav.Path.Clear();
        currentCharacterNodeObj = NodesNav.GetNearestNode(CharacterObj.transform.position);
        if (State == FightState.Fighting)
        {
            Debug.Log("Hit");
            // CharacterObj.GetComponent<FightCharacter>().MakeHit();
            var character = CharacterObj.GetComponent<FightCharacter>();
            character.Attack();
            character.TargetToHit = null;
            //DeleteDeathCharacterFromQueue();
        }
        DrawAreas();
        UIController.Instance.RedrawGroupCharacterCards();
        FightSceneController.State = FightState.Sleeping;
        Debug.Log("Sleeping Phase");
    }

    private void EndTurn()
    {
        State = FightState.Completion;
        CharactersQueue.Enqueue(CharacterObj);
        CharacterObj = GetNextCharacter();
        AI.CurrentCharacterObj = CharacterObj;

        Sign.transform.position = new Vector3(CharacterObj.transform.position.x,
            Sign.transform.position.y, CharacterObj.transform.position.z);
        Sign.transform.parent = CharacterObj.transform;

        StateController.MakeAvailablePhases();
        currentCharacterNodeObj = NodesNav.GetNearestNode(CharacterObj.transform.position);
        DrawAreas();

        UIController.Instance.ChangeActiveCard();
        State = FightState.Sleeping;
        Debug.Log("Sleeping");
    }

    private GameObject GetNextCharacter()
    {
        var fightCharacter = CharacterObj.GetComponent<FightCharacter>();
        fightCharacter.ResetEnergy();
        UIController.Instance.RedrawGroupCharacterCards();
        
        var nextCharacter = CharactersQueue.Dequeue();
        while (!nextCharacter || !nextCharacter.GetComponent<FightCharacter>().Alive)
        {
            nextCharacter = CharactersQueue.Dequeue();
        }

        return nextCharacter;
    }

    private void MoveCharacter()
    {
        if (NodesNav.Path.Count != 0
            && StateController.AvailablePhase[FightState.MovePhase])
        {
            State = FightState.Moving;
            NodesNav.StartMoveCharacter(CharacterObj);
            StateController.AvailablePhase[FightState.MovePhase] = false;
        }
        else
            Debug.Log("Недоступная зона или Слишком близко к другому персонажу / объекту");
    }

    private void Fight(GameObject targetObj)
    {
        if (targetObj.GetComponent<FightCharacter>() && targetObj != CharacterObj
            && targetObj.GetComponent<FightCharacter>().Type !=
            CharacterObj.GetComponent<FightCharacter>().Type
            && StateController.AvailablePhase[FightState.FightPhase]
            && NodesNav.Path.Count != 0
            && NodesNav.Path.Count <= CharacterObj.GetComponent<FightCharacter>().RemainingEnergy + 1)
        {
            CharacterObj.GetComponent<FightCharacter>().TargetToHit = targetObj;
            NodesNav.StartMoveCharacter(CharacterObj);
            State = FightState.Fighting;
            StateController.AvailablePhase[FightState.FightPhase] = false;
            StateController.AvailablePhase[FightState.ShootPhase] = false;
        }
        else
            Debug.Log("Недоступная зона или Необходимо указать на врага");
    }

    private void Shoot(GameObject targetObj)
    {
        if (targetObj != CharacterObj && targetObj.GetComponent<FightCharacter>()
            && targetObj.GetComponent<FightCharacter>().Alive
            && StateController.AvailablePhase[FightState.ShootPhase])
        {
            Debug.Log("Shoot");
            var character = CharacterObj.GetComponent<FightCharacter>();
            character.MakeShoot(targetObj);
            // character.MakeShoot(targetObj, "Body");
            State = FightState.Sleeping;
            StateController.AvailablePhase[FightState.FightPhase] = false;
            StateController.AvailablePhase[FightState.ShootPhase] = false;
            //DeleteDeathCharacterFromQueue();
            //Debug.Log(CharactersQueue.Count);
        }
    }


    public void DeleteDeathCharacterFromQueue(FightCharacter character)
    {
        Debug.Log("Delete");
        var newQueue = new Queue<GameObject>();
        while (CharactersQueue.Count > 0)
        {
            var charObj = CharactersQueue.Dequeue();
            if (charObj != null && charObj.GetComponent<FightCharacter>() != character)
                newQueue.Enqueue(charObj);
        }

        CharactersQueue = newQueue;
    }

    public void SetNearestNodeToCurrentCharacter()
    {
        currentCharacterNodeObj = NodesNav.GetNearestNode(CharacterObj.transform.position);
    }

    public void DrawAreas()
    {
        NodesNav.CleanAreasLists();
        if(StateController.AvailablePhase[FightState.MovePhase] 
            || StateController.AvailablePhase[FightState.FightPhase])
        {
            foreach (var otherCharacterObj in CharactersQueue)
            {
                NodesNav.FindObstacleNode(otherCharacterObj);
            }
            NodesNav.FindAvailableArea(CharacterObj);
        }
    }

    private void CalculateAvailalePathToPoint(RaycastHit hit, bool isForFighting)
    {
        if (hit.transform != null &&
            ((hit.transform.gameObject.GetComponent<FightCharacter>() && isForFighting) || !isForFighting))
        {
            if (isForFighting)
                NodesNav.TryFindPath(currentCharacterNodeObj.GetComponent<FightNode>(),
                    NodesNav.GetNearestNodeNearEnemy(hit.transform.gameObject, hit.point));
            else
                NodesNav.TryFindPath(currentCharacterNodeObj.GetComponent<FightNode>(),
                    NodesNav.GetNearestNode(hit.point).GetComponent<FightNode>());

            if (NodesNav.Path.Count != 0)
                DrawPath(isForFighting);
        }
    }

    private void DrawPath(bool isForFighting)
    {
        var pathPoints = NodesNav.Path;
        var energy = CharacterObj.GetComponent<FightCharacter>().Energy;
        if (NodesNav.Path.Count != 0)
        {
            lineRenderer.positionCount = pathPoints.Count;
            lineRenderer.SetPositions(pathPoints.ToArray());
        }
    }
    
}