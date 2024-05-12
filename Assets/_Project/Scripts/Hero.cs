using System.Collections.Generic;
using BehaviourTrees;
using BlackboardSystem;
using UnityEngine;
using UnityEngine.AI;
using UnityServiceLocator;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HeroAnimationController))]
public class Hero : MonoBehaviour, IExpert {
    [SerializeField] InputReader input;
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] GameObject treasure;
    [SerializeField] GameObject treasure2;
    [SerializeField] GameObject safeSpot;
    
    NavMeshAgent agent;
    AnimationController animations;
    BehaviourTree tree;
    
    Blackboard blackboard;
    BlackboardKey foundAllTreasuresKey;
    BlackboardKey isSafeKey;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
    }
    
    public int GetInsistence(Blackboard blackboard) {
        // Found all the treasures
        return treasure.activeSelf || treasure2.activeSelf ? 0 : 80;
    }

    public void Execute(Blackboard blackboard) {
        blackboard.AddAction(() =>  {
            if (blackboard.TryGetValue(foundAllTreasuresKey, out bool isSafe)) {
                blackboard.SetValue(foundAllTreasuresKey, !treasure.activeSelf && !treasure2.activeSelf);
            }
        });
    }
    
    void Start() {
        blackboard = ServiceLocator.For(this).Get<BlackboardController>().GetBlackboard();
        ServiceLocator.For(this).Get<BlackboardController>().RegisterExpert(this);
        
        // blackboardData.SetValuesOnBlackboard(blackboard);
        isSafeKey = blackboard.GetOrRegisterKey("IsSafe");
        // blackboard.SetValue(isSafeKey, false);
        foundAllTreasuresKey = blackboard.GetOrRegisterKey("FoundAllTreasures");
        blackboard.SetValue(foundAllTreasuresKey, false);
        
        tree = new BehaviourTree("Hero");
        
        PrioritySelector actions = new PrioritySelector("Agent Logic");
        
        Sequence runToSafetySeq = new Sequence("RunToSafety", 100);
        bool IsSafe() {
            if (blackboard.TryGetValue(isSafeKey, out bool isSafe)) {
                if (!isSafe) {
                    runToSafetySeq.Reset();
                    return true;
                }
            }

            return false;
        }
        runToSafetySeq.AddChild(new Leaf("isSafe?", new Condition(IsSafe)));
        runToSafetySeq.AddChild(new Leaf("Go To Safety", new MoveToTarget(transform, agent, safeSpot.transform)));
        actions.AddChild(runToSafetySeq);
        
        Selector goToTreasure = new RandomSelector("GoToTreasure", 50);
        Sequence getTreasure1 = new Sequence("GetTreasure1");
        getTreasure1.AddChild(new Leaf("isTreasure1?", new Condition(() => treasure.activeSelf)));
        getTreasure1.AddChild(new Leaf("GoToTreasure1", new MoveToTarget(transform, agent, treasure.transform)));
        getTreasure1.AddChild(new Leaf("PickUpTreasure1", new ActionStrategy(() => treasure.SetActive(false))));
        goToTreasure.AddChild(getTreasure1);
        
        Sequence getTreasure2 = new Sequence("GetTreasure2");
        getTreasure2.AddChild(new Leaf("isTreasure2?", new Condition(() => treasure2.activeSelf)));
        getTreasure2.AddChild(new Leaf("GoToTreasure2", new MoveToTarget(transform, agent, treasure2.transform)));
        getTreasure2.AddChild(new Leaf("PickUpTreasure2", new ActionStrategy(() => treasure2.SetActive(false))));
        goToTreasure.AddChild(getTreasure2);
        
        actions.AddChild(goToTreasure);
        
        Leaf patrol = new Leaf("Patrol", new PatrolStrategy(transform, agent, waypoints));
        actions.AddChild(patrol);
        
        tree.AddChild(actions);
    }

    void OnEnable() {
        input.EnablePlayerActions();
        input.Click += OnClick;
    }

    void OnDisable() {
        input.Click -= OnClick;
    }
    
    void Update() {
        animations.SetSpeed(agent.velocity.magnitude);
        tree.Process();
        
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (blackboard.TryGetValue(isSafeKey, out bool isSafe)) {
                blackboard.SetValue(isSafeKey, !isSafe);
                Debug.Log($"IsSafe: {isSafe}");
            }
        }
    }

    void OnClick(RaycastHit hit) {
        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas)) {
            agent.SetDestination(navHit.position);
        }
    }
}