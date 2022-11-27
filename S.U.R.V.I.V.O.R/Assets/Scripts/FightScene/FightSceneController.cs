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
    private static Queue<GameObject> CharactersQueue = new Queue<GameObject>();
    private GameObject currentCharacterNodeObj;

    void Awake()
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
        InitializeCharacters();
        NodesNav.InitializeNodesLists(graph);

        StateController.MakeAvailablePhases();
        CharacterObj = CharactersQueue.Dequeue();
        AI.CurrentCharacterObj = CharacterObj;
        DrawAreas();
        State = FightState.Sleeping;

        currentCharacterNodeObj = NodesNav.GetNearestNode(CharacterObj.transform.position);
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

    private void InitializeCharacters()
    {
        for (var i = 0; i < Characters.Count; i++)
        {
            var component = Characters[i].GetComponent<FightCharacter>();
            //component.RangeWeapon = new Rifle(10);
            //component.MeleeWeapon = new Knife(3, 0.3f);
            CharactersQueue.Enqueue(Characters[i]);
        }

        Debug.Log(CharactersQueue.Count);
    }

    private void CharacterReachTarget()
    {
        NodesNav.Path.Clear();
        currentCharacterNodeObj = NodesNav.GetNearestNode(CharacterObj.transform.position);
        if (State == FightState.Fighting)
        {
            Debug.Log("Hit");
            // CharacterObj.GetComponent<FightCharacter>().MakeHit();
            CharacterObj.GetComponent<FightCharacter>().TargetToHit = null;
            DeleteDeathCharacterFromQueue();
        }

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
        State = FightState.Sleeping;
        Debug.Log("Sleeping");
    }

    private GameObject GetNextCharacter()
    {
        var nextCharacter = CharactersQueue.Dequeue();
        while (nextCharacter == null || !nextCharacter.GetComponent<FightCharacter>().Alive)
        {
            nextCharacter = CharactersQueue.Dequeue();
            Debug.Log("OK");
        }

        return nextCharacter;
    }

    private void MoveCharacter()
    {
        if (NodesNav.Path.Count != 0)
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
        if (targetObj.tag == "Character" && targetObj != CharacterObj
                                         && targetObj.GetComponent<FightCharacter>().Type !=
                                         CharacterObj.GetComponent<FightCharacter>().Type)
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
        if (targetObj.tag == "Character" && targetObj != CharacterObj
                                         && targetObj.GetComponent<FightCharacter>().Alive)
        {
            Debug.Log("Shoot");
            var character = CharacterObj.GetComponent<FightCharacter>();
            // character.MakeShoot(targetObj, "Body");
            State = FightState.Sleeping;
            StateController.AvailablePhase[FightState.FightPhase] = false;
            StateController.AvailablePhase[FightState.ShootPhase] = false;
            DeleteDeathCharacterFromQueue();
            Debug.Log(CharactersQueue.Count);
        }
    }

    private void DeleteDeathCharacterFromQueue()
    {
        var newQueue = new Queue<GameObject>();
        while (CharactersQueue.Count > 0)
        {
            var charObj = CharactersQueue.Dequeue();
            if (charObj != null && charObj.GetComponent<FightCharacter>().Alive)
                newQueue.Enqueue(charObj);
        }

        CharactersQueue = newQueue;
    }

    public void DeleteDeathCharacterFromQueue(FightCharacter character)
    {
        var newQueue = new Queue<GameObject>();
        while (CharactersQueue.Count > 0)
        {
            var charObj = CharactersQueue.Dequeue();
            if (charObj != null && charObj.GetComponent<FightCharacter>() != character)
                newQueue.Enqueue(charObj);
        }

        CharactersQueue = newQueue;
    }

    private void DrawAreas()
    {
        NodesNav.CleanAreasLists();
        foreach (var otherCharacterObj in CharactersQueue)
            NodesNav.FindObstacleNode(otherCharacterObj);
        NodesNav.FindAvailableArea(CharacterObj);
    }

    private void CalculateAvailalePathToPoint(RaycastHit hit, bool isForFighting)
    {
        if (hit.transform != null &&
            ((hit.transform.gameObject.tag == "Character" && isForFighting) || !isForFighting))
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


    public void Initialization(FightData data)
    {
        
    }
}