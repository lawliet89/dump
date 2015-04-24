using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Graph
{
    public class Graph<TValue, TWeight> where TWeight : IComparable
    {
        public IEnumerable<Vertex<TValue>> Vertices { get; set; }
        public IEnumerable<Edge<TValue, TWeight>> Edges { get; set; }

        public static Graph<TValue, TWeight> MakeGraph(IEnumerable<TValue> vertices,
            IEnumerable<Edge<TValue, TWeight>.EdgeFactory> edgeFactories)
        {
            var edges = new List<Edge<TValue, TWeight>>();
            var graph = new Graph<TValue, TWeight>
            {
                Vertices = vertices.Select(v => new Vertex<TValue> {Value = v}),
                Edges = edges
            };

            edges.AddRange(edgeFactories.Select(factory => new Edge<TValue, TWeight>
            {
                A = graph.Vertices.First(v => v.Value.Equals(factory.A)),
                B = graph.Vertices.First(v => v.Value.Equals(factory.B)),
                Direction = factory.Direction,
                Weight = factory.Weight
            }));
            return graph;
        }
    }
}
