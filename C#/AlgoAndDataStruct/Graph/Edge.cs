using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph
{
    public class Edge<TValue, TWeight> where TWeight : IComparable
    {
        public Vertex<TValue> A { get; set; }
        public Vertex<TValue> B { get; set; }
        public TWeight Weight { get; set; }
        public EdgeDirection Direction { get; set; }

        public enum EdgeDirection { Forward, Backward }

        public struct EdgeFactory
        {
            public TValue A { get; set; }
            public TValue B { get; set; }
            public TWeight Weight { get; set; }
            public EdgeDirection Direction { get; set; }
        }
    }
}
