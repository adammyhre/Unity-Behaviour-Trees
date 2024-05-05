using System.Collections.Generic;
using Pathfinding.BehaviourTrees;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HeroAnimationController))]
public class Hero : MonoBehaviour {
    [SerializeField] InputReader input;
    [SerializeField] List<Transform> waypoints = new();
    [SerializeField] GameObject treasure;
    [SerializeField] GameObject treasure2;
    [SerializeField] GameObject safeSpot;
    [SerializeField] bool inDanger;
    
    NavMeshAgent agent;
    AnimationController animations;
    BehaviourTree tree;
    
    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        
        tree = new BehaviourTree("Hero");
        
        PrioritySelector actions = new PrioritySelector("Agent Logic");
        
        Sequence runToSafetySeq = new Sequence("RunToSafety", 100);
        bool IsSafe() {
            if (!inDanger) {
                runToSafetySeq.Reset();
                return false;
            }

            return true;
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
    }

    void OnClick(RaycastHit hit) {
        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas)) {
            agent.SetDestination(navHit.position);
        }
    }
}
