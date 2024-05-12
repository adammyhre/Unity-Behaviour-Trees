using System.Collections.Generic;
using BlackboardSystem;
using UnityEngine;
using UnityServiceLocator;

public class Scout : MonoBehaviour, IExpert {
    Blackboard blackboard;
    BlackboardKey isSafeKey;
    
    bool dangerSensor;

    void Start() {
        blackboard = ServiceLocator.For(this).Get<BlackboardController>().GetBlackboard();
        ServiceLocator.For(this).Get<BlackboardController>().RegisterExpert(this);
    }

    public int GetInsistence(Blackboard blackboard) {
        return dangerSensor ? 100 : 0;
    }

    public void Execute(Blackboard blackboard) {
        blackboard.AddAction(() => {
            if (blackboard.TryGetValue(isSafeKey, out bool isSafe)) {
                blackboard.SetValue(isSafeKey, !isSafe);
            }
        });
    }
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (blackboard.TryGetValue(isSafeKey, out bool isSafe)) {
                blackboard.SetValue(isSafeKey, !isSafe);
                Debug.Log($"IsSafe: {isSafe}");
            }
        }
    }
}