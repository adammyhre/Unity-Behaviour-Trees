using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pathfinding.BehaviourTrees {
    // UntilSuccess
    // Repeat
    public class UntilFail : Node {
        public UntilFail(string name) : base(name) { }
        
        public override Status Process() {
            if (children[0].Process() == Status.Failure) {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }
    
    public class Inverter : Node {
        public Inverter(string name) : base(name) { }
        
        public override Status Process() {
            switch (children[0].Process()) {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }
    
    public class RandomSelector : PrioritySelector {
        protected override List<Node> SortChildren() => children.Shuffle().ToList();
        
        public RandomSelector(string name, int priority = 0) : base(name, priority) { }
    }
    
    public class PrioritySelector : Selector {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();
        
        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();
        
        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }
        
        public override void Reset() {
            base.Reset();
            sortedChildren = null;
        }
        
        public override Status Process() {
            foreach (var child in SortedChildren) {
                switch (child.Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        continue;
                }
            }

            Reset();
            return Status.Failure;
        }
    }
    
    public class Selector : Node {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }
            
            Reset();
            return Status.Failure;
        }
    }
    
    public class Sequence : Node {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        currentChild = 0;
                        return Status.Failure;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Success;
        }
    }
    
    public class Leaf : Node {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority) {
            // Preconditions.CheckNotNull(strategy);
            this.strategy = strategy;
        }
        
        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }
    
    public class Node {
        public enum Status { Success, Failure, Running }
        
        public readonly string name;
        public readonly int priority;
        
        public readonly List<Node> children = new();
        protected int currentChild;
        
        public Node(string name = "Node", int priority = 0) {
            this.name = name;
            this.priority = priority;
        }
        
        public void AddChild(Node child) => children.Add(child);
        
        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset() {
            currentChild = 0;
            foreach (var child in children) {
                child.Reset();
            }
        }
    }
    
    public interface IPolicy {
        bool ShouldReturn(Node.Status status);
    }

    public static class Policies {
        public static readonly IPolicy RunForever = new RunForeverPolicy();
        public static readonly IPolicy RunUntilSuccess = new RunUntilSuccessPolicy();
        public static readonly IPolicy RunUntilFailure = new RunUntilFailurePolicy();
        
        class RunForeverPolicy : IPolicy {
            public bool ShouldReturn(Node.Status status) => false;
        }
        
        class RunUntilSuccessPolicy : IPolicy {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Success;
        }
        
        class RunUntilFailurePolicy : IPolicy {
            public bool ShouldReturn(Node.Status status) => status == Node.Status.Failure;
        }
    }
    
    public class BehaviourTree : Node {
        readonly IPolicy policy;
        
        public BehaviourTree(string name, IPolicy policy = null) : base(name) {
            this.policy = policy ?? Policies.RunForever;
        }

        public override Status Process() {
            Status status = children[currentChild].Process();
            if (policy.ShouldReturn(status)) {
                return status;
            }
            
            currentChild = (currentChild + 1) % children.Count;
            return Status.Running;
        }

        public void PrintTree() {
            StringBuilder sb = new StringBuilder();
            PrintNode(this, 0, sb);
            Debug.Log(sb.ToString());
        }

        static void PrintNode(Node node, int indentLevel, StringBuilder sb) {
            sb.Append(' ', indentLevel * 2).AppendLine(node.name);
            foreach (Node child in node.children) {
                PrintNode(child, indentLevel + 1, sb);
            }
        }
    }
}