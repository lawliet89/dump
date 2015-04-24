using System;
using System.Collections.Generic;
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
