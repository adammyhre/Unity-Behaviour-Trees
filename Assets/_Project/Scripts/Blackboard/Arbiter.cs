using System;
using System.Collections.Generic;

namespace BlackboardSystem {
    public class Arbiter {
        readonly List<IExpert> experts = new();

        public void RegisterExpert(IExpert expert) {
            Preconditions.CheckNotNull(expert);
            experts.Add(expert);
        }

        public void DeregisterExpert(IExpert expert) {
            Preconditions.CheckNotNull(expert);
            experts.Remove(expert);
        }

        public List<Action> BlackboardIteration(Blackboard blackboard) {
            IExpert bestExpert = null;
            int highestInsistence = 0;

            foreach (IExpert expert in experts) {
                int insistence = expert.GetInsistence(blackboard);
                if (insistence > highestInsistence) {
                    highestInsistence = insistence;
                    bestExpert = expert;
                }
            }
            
            bestExpert?.Execute(blackboard);
            
            var actions = new List<Action>(blackboard.PassedActions);
            blackboard.ClearActions();
            
            // Return or execute the actions here
            return actions;
        }
    }
}