using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphsDFS
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = new Vertex { Name = "A" };
            var b = new Vertex { Name = "B" };
            var c = new Vertex { Name = "C" };
            var d = new Vertex { Name = "D" };
            var e = new Vertex { Name = "E" };
            var f = new Vertex { Name = "F" };
            var g = new Vertex { Name = "G" };
            var h = new Vertex { Name = "H" };
            var i = new Vertex { Name = "I" };

            var graph = new Graph
            {
                Edges = new List<Edge>
                {
                    new Edge(a,b),
                    new Edge(b,d),
                    new Edge(b,e),
                    new Edge(a,c),
                    new Edge(c,f),
                    new Edge(f,e),
                    new Edge(c,g),
                    new Edge(h,i),
                    new Edge(i,g),
                    new Edge(g,d)
                }
            };

            DFS(graph);
            PrintTree(graph);

            WriteToDotFile(graph);

            //Console.WriteLine("Topologia");
            //var vertexs = TopologySort(graph);
            //foreach (var vertex in vertexs)
            //{
            //    Console.WriteLine(vertex);
            //}



            //graph = new Graph
            //{
            //    Edges = new List<Edge>
            //    {
            //        new Edge(a,b),
            //        new Edge(b,d),
            //        new Edge(b,e),
            //        new Edge(a,c),
            //        new Edge(c,f),
            //        new Edge(f,e),
            //        new Edge(f,b),
            //        new Edge(c,g),
            //        new Edge(g,d),
            //        new Edge(h,i),
            //        new Edge(i,g),
            //        new Edge(i,h),
            //        new Edge(g,h),
            //        new Edge(e,a),
            //        }
            //};
            
            
            //GetStrongConnectedComponents(graph);

           
        }

        const string anStatus = "status";
        const string anParent = "pi";
        const string anD = "d";
        const string anF = "f";

        public static int Counter = 0;
        public static void DFS(Graph graph, bool fillCounter= true)
        {
            foreach (var u in graph.GetVertex())
            {
                u.Attributes[anStatus] = VertexStatus.Nowy;
                u.Attributes[anParent] = null;
            }
            var vertexes = graph.GetVertex();
            for (int i = 0; i < vertexes.Count; i++)
            {
                var u = vertexes[i];
                if (u.Attributes[anStatus].Cast<VertexStatus>() == VertexStatus.Nowy)
                {
                    DFS_rec(graph, u, fillCounter);
                }    
            }
        }
        public static void DFS_rec( Graph graph,  Vertex u, bool fillCounter=true)
        {
            Counter++;
            if(fillCounter)
                u.Attributes[anD] = Counter;
            
            u.Attributes[anStatus] = VertexStatus.Odwiedzony;

            var toVertex = graph.GetOutFrom(u);
            for (int i = 0; i < toVertex.Count; i++)
            {
                var v = toVertex[i];
                if (v.Attributes[anStatus].Cast<VertexStatus>() == VertexStatus.Nowy)
                {
                    v.Attributes[anParent] = u;
                    DFS_rec( graph,  v);
                }
            }

            u.Attributes[anStatus] = VertexStatus.Przetworzony;
            Counter++;

            if(fillCounter)
                u.Attributes[anF] = Counter;
        }

        public static IList<Vertex> TopologySort(Graph graph)
        {
            DFS(graph);

            return graph
                .GetVertex()
                .OrderByDescending(v => v.Attributes[anF])
                .ToList();
        }

        public static void GetStrongConnectedComponents(Graph graph)
        {
            DFS(graph);
            
            var graphT = graph.GetTranspose();

            Counter = 0;
            graphT.GetVertexOrderFunction= x => -1 * (int)x.Attributes[anF];

            DFS(graphT, false);

            PrintTree(graphT);
            
        }

        public static void PrintTree(Graph grap, string atributeNameParent = anParent)
        {
            var roots = grap.GetVertex().Where(x => x.Attributes[atributeNameParent] == null);
            foreach (var root in roots)
            {
                Console.WriteLine("Root :{0}", root.Name);
                PrintNextNode(grap, root);
            }
        }
        public static void PrintNextNode(Graph graph, Vertex parent, string atributeNameParent = anParent)
        {
            var childs = graph
                .GetOutFrom(parent)
                .Where(x => x.Attributes[atributeNameParent] == parent);

            foreach (var child in childs)
            {
                Console.WriteLine("{0} -> {1}", parent, child);
                PrintNextNode(graph, child);
            }
        }

        public static void WriteToDotFile(Graph graph)
        {
            #region formating
            string begining = @"digraph G {
rankdir=LR;";
            string ending = @"fontsize=12;
}";
            string edgeFormat = "{0} -> {1} ;";
            string vertexFormat = "{0} [label={1}-{2}];";
            #endregion

            var fileLocation =@"C:\Users\arekb_000\Desktop\lessons\graph\";
            using (TextWriter vTextWriter = new StreamWriter(fileLocation + "GraphViz.dot", false))
            {
                vTextWriter.WriteLine(begining);

                foreach (var vertex in graph.GetVertex())
                {
                    vTextWriter.WriteLine(string.Format(vertexFormat,vertex.Name, vertex.Attributes[anD], vertex.Attributes[anF]));
                }


                foreach (var edge in graph.Edges)
                {
                    vTextWriter.WriteLine(string.Format(edgeFormat, edge.From, edge.To));
                }

                vTextWriter.WriteLine(ending);
                vTextWriter.Close();
            }
        
        
        }
    }


    public enum VertexStatus { Nowy, Odwiedzony, Przetworzony }

    public static class ObjectExt
    {
        [DebuggerStepThrough]
        public static T Cast<T>(this Object obj)
        {
            return (T)obj;
        }
    }


    public class Vertex
    {
        public string Name { get; set; }
        public Dictionary<string,object> Attributes { get; set; }


        public Vertex()
        {
            Attributes = new Dictionary<string,object>();
        }
        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class Edge
    {
        public const string WeightAttributeName = "weight";

        public Vertex From { get; set; }
        public Vertex To { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public Edge(Vertex from, Vertex to, int weight=0)
        {
            From = from;
            To = to;
            Attributes = new Dictionary<string, object>();
            Attributes.Add(WeightAttributeName, weight);
        }
        public override string ToString()
        {
            return From +" -> "+To;
        }
    }

    public class Graph
    {
        public IList<Edge> Edges { get; set; }
        public IList<Vertex> GetVertex()
        {
            var froms =Edges.Select(x => x.From);
            var tos = Edges.Select(x => x.To);
            var dic = new Dictionary<string, Vertex>();
            foreach (var vertex in froms.Union(tos))
            {
                if(!dic.ContainsKey(vertex.Name))
                {
                    dic.Add(vertex.Name, vertex);
                }
            }

            var vertexes = dic.Select(x=>x.Value);
            if (GetVertexOrderFunction != null)
            {
                vertexes = vertexes.OrderBy(GetVertexOrderFunction);
            }

            return vertexes.ToList();
        }
        public IList<Vertex> GetOutFrom(Vertex u)
        {
            return Edges
               .Where(edge => edge.From == u)
               .Select(x => x.To)
               .ToList()
             ;
        }



        public Graph()
        {
            Edges = new List<Edge>();

        }
        
        public Graph GetTranspose()
        {
            var graphT = new Graph();
            foreach (var edge in Edges)
            {
                graphT.Edges.Add(new Edge(edge.To, edge.From));
            }
            return graphT;
        }
        public Func<Vertex, int> GetVertexOrderFunction;

    }
}
