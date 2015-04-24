using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BinarySearchTree
{
    public class Node<T> where T : IComparable
    {
        public Node<T> Parent { get; set; }
        public Node<T> LeftChild { get; set; }
        public Node<T> RightChild { get; set; }

        public T Value { get; set; }

        public static Node<T> MakeNode(T value, Node<T> parent = null, Node<T> left = null, Node<T> right = null)
        {
            return new Node<T>() {Value = value, LeftChild = left, RightChild = right, Parent = parent};
        }

        public Node<T> Insert(T value)
        {
            var node = MakeNode(value);
            Insert(node);
            return node;
        }

        public void Insert(Node<T> node)
        {
            if (node == null) return;
            // Node needs to be inserted into a tree
            node.Parent = null;

            var currentNode = this;
            while (node.Parent == null)
            {
                var expression = node.Value.CompareTo(currentNode.Value) <= 0
                    ? LeftChildExpression
                    : RightChildExpression;
                var newNode = expression.Compile()(currentNode);
                // Bingo!
                if (newNode == null)
                {
                    var selector = expression.Body as MemberExpression;
                    if (selector != null)
                    {
                        var property = selector.Member as PropertyInfo;
                        if (property != null)
                        {
                            property.SetValue(currentNode, node, null);
                        }
                    }
                    node.Parent = currentNode;
                }
                else
                {
                    currentNode = newNode;
                }
            }
        }

        public T MinimumValue()
        {
            return MinimumNode().Value;
        }

        public Node<T> MinimumNode()
        {
            return LeftChild == null ? this : LeftChild.MinimumNode();
        }

        public T MaximumValue()
        {
            return MaximumNode().Value;
        }

        public Node<T> MaximumNode()
        {
            return RightChild == null ? this : RightChild.MaximumNode();
        }

        public IEnumerable<T> Walk()
        {
            return WalkNodes().Select(n => n.Value);
        }

        public IEnumerable<Node<T>> WalkNodes()
        {
            if (LeftChild != null)
            {
                foreach (var node in LeftChild.WalkNodes())
                {
                    yield return node;
                }
            }
            yield return this;
            if (RightChild != null)
            {
                foreach (var node in RightChild.WalkNodes())
                {
                    yield return node;
                }
            }
        }

        public Node<T> FindNode(T value)
        {
            if (Value.Equals(value))
                return this;
            if (value.CompareTo(Value) <= 0 && LeftChild != null)
            {
                return LeftChild.FindNode(value);
            }
            if (value.CompareTo(Value) > 0 && RightChild != null)
            {
                return RightChild.FindNode(value);
            }
            return null;
        }

        public T Successor(T value)
        {
            var node = SuccessorNode(value);
            return node != null ? node.Value : default(T);
        }

        public Node<T> SuccessorNode(T value)
        {
            var node = FindNode(value);
            if (node != null) return node.SuccessorNode();
            throw new ArgumentException("Value cannot be found in tree.");
        } 

        public T Successor()
        {
            var successorNode = SuccessorNode();
            return successorNode != null ? successorNode.Value : default(T);
        }

        public Node<T> SuccessorNode()
        {
            if (RightChild != null)
            {
                return RightChild.MinimumNode();
            }

            // Otherwise, the successor is a node that is the lowest ancestor of this 
            // and has a left child that is also an ancestor of this
            var x = this;
            var y = Parent;
            // Keep going up as long as x is the right child of y
            while (y != null && y.RightChild == x)
            {
                x = y;
                y = y.Parent;
            }
            return y;
        }

        /// <summary>
        /// Replace this as a child of its parent with v.
        /// Afterwards, this will be in a "dangling" state
        /// </summary>
        public void Transplant(Node<T> v, ref Node<T> root)
        {
            if (Parent == null)
            {
                root = v;
            }
            else if (Parent.LeftChild == this)
            {
                Parent.LeftChild = v;
            }
            else if (Parent.RightChild == this)
            {
                Parent.RightChild = v;
            }
            if (v != null)
                v.Parent = Parent;
        }

        public void Delete(ref Node<T> root)
        {
            if (LeftChild == null)
            {
                Transplant(RightChild, ref root);
            }
            else if (RightChild == null)
            {
                Transplant(LeftChild, ref root);
            }
            else // Has two children
            {
                // The successor (by virtue of BST) will be in the right sub-tree
                // and has no left-child
                var successor = SuccessorNode();
                Debug.Assert(successor != null);

                // If successor is not a right child of this
                if (successor.Parent != this)
                {
                    // Transplant successor with its right child
                    successor.Transplant(successor.RightChild, ref root);
                    // Make Successor's right child be this's original right child
                    successor.RightChild = RightChild;
                    successor.RightChild.Parent = successor;
                }
                // Transplant this with successor
                Transplant(successor, ref root);
                // Make successor's left child be this's left child
                successor.LeftChild = LeftChild;
                successor.LeftChild.Parent = successor;
            }
        }

        public Node<T> Delete(T value, ref Node<T> root)
        {
            var node = FindNode(value);
            if (node != null) node.Delete(ref root);
            return node;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static Expression<Func<Node<T>, Node<T>>> LeftChildExpression = n => n.LeftChild;
        public static Expression<Func<Node<T>, Node<T>>> RightChildExpression = n => n.RightChild;

        public static Node<T> MakeTree(params T[] values)
        {
            return MakeTree(values.AsEnumerable());
        }

        public static Node<T> MakeTree(IEnumerable<T> values)
        {
            Node<T> root = null;

            foreach (var value in values)
            {
                // Root Node
                if (root == null)
                {
                    root = MakeNode(value);
                }
                else
                {
                    root.Insert(value);
                }
            }

            return root;
        }
    }
}
