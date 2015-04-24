using System;
using NUnit.Framework;

namespace BinarySearchTree.Tests
{
    [TestFixture]
    public class NodeTests
    {
        private readonly int[] values = { 15, 6, 3, 2, 4, 7, 13, 9, 18, 17, 20};

        [Test]
        public void InsertionRespectsBinaryTreeProperty()
        {
            var root = Node<int>.MakeTree(values);
            Assert.That(BinaryTreePropertyRespected(root));
        }

        public static bool BinaryTreePropertyRespected<T>(Node<T> root) where T : IComparable
        {
            if (root == null) return true;
            var result = true;
            if (root.LeftChild != null)
            {
                result &= root.LeftChild.Value.CompareTo(root.Value) <= 0;
                result &= BinaryTreePropertyRespected<T>(root.LeftChild);
            }

            if (root.RightChild != null)
            {
                result &= root.RightChild.Value.CompareTo(root.Value) >= 0;
                result &= BinaryTreePropertyRespected<T>(root.RightChild);
            }

            return result;
        }
    }
}
