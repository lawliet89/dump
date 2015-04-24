using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BinarySearchTree.Tests
{
    [TestFixture(typeof(int), new[] { 5, 6, 3, 2, 4, 7, 13, 9, 18, 17, 20 })]
    public class NodeTests<T> where T : IComparable
    {
        private readonly T[] values;
        private readonly Node<T> root;

        public NodeTests(T[] values)
        {
            this.values = values;
            root = Node<T>.MakeTree(values);
        }

        [Test]
        public void InsertionRespectsBinaryTreeProperty()
        {
            Assert.That(BinaryTreePropertyRespected(root));
        }

        [Test]
        public void MaximumValueReturnedIsCorrect()
        {
            Assert.AreEqual(values.Max(), root.MaximumValue());
        }

        [Test]
        public void MinimumValueReturndIsCorrect()
        {
            Assert.AreEqual(values.Min(), root.MinimumValue());
        }

        [Test]
        public void NodesAreWalkedInOrder()
        {
            Assert.That(values.OrderBy(n => n).SequenceEqual(root.Walk()));
        }

        [Test]
        public void CanFindNodes()
        {
            var randomValue = values.Skip(new Random().Next(0, values.Count() - 1))
                .First();
            Assert.IsNotNull(root.FindNode(randomValue));
        }

        [Test]
        public void SuccessorsAreReturnedCorrectly()
        {
            var sortedValues = values.OrderBy(x => x).ToArray();
            for (var i = 0; i < sortedValues.Length - 1; ++i)
            {
                Assert.AreEqual(sortedValues[i + 1], root.Successor(sortedValues[i]));
            }
            Assert.IsNull(root.SuccessorNode(sortedValues[sortedValues.Length - 1]));
        }

        [Test]
        public void DeleteAndTransplantsAreDoneCorrectly()
        {
            var sortedValues = values.OrderBy(x => x).ToArray();
            foreach (var value in sortedValues)
            {
                var treeValues = values.Except(value.Yield());
                var tree = Node<T>.MakeTree(treeValues);
                tree.Delete(value, ref tree);
                Assert.That(sortedValues.Except(value.Yield()).SequenceEqual(tree.Walk()));
            }
        }

        public static bool BinaryTreePropertyRespected(Node<T> root, T minimumValue = default(T),
            T maximumValue = default(T)) 
        {
            if (root == null) return true;
            var result = true;
            if (root.LeftChild != null)
            {
                result &= root.LeftChild.Value.CompareTo(root.Value) <= 0;
                result &= BinaryTreePropertyRespected(root.LeftChild, minimumValue, root.Value);
            }

            if (root.RightChild != null)
            {
                result &= root.RightChild.Value.CompareTo(root.Value) >= 0;
                result &= BinaryTreePropertyRespected(root.RightChild, root.Value, maximumValue);
            }
            // The following lines are fragile if the node tree contains zero because default(int) is zero!
            if (!EqualityComparer<T>.Default.Equals(minimumValue, default(T)))
            {
                result &= root.Value.CompareTo(minimumValue) >= 0;
            }
            if (!EqualityComparer<T>.Default.Equals(maximumValue, default(T)))
            {
                result &= root.Value.CompareTo(maximumValue) <= 0;
            }

            return result;
        }
    }
}
