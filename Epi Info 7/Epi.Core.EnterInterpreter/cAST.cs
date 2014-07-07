using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
namespace Epi.Core.EnterInterpreter
{
    public interface IVisitor
    {
       void visit(HeteroAST concreteElementA);
    }

    public class cAST
    {
        Token token; // node is derived from which token?
        List<cAST> children; // operands

        public cAST(Token token) { this.token = token; }
        public void addChild(cAST t)
        {
            if (children == null) children = new List<cAST>();
            children.Add(t);
        }
    }



    // define
    // assign
    //


    public class HeteroAST
    {// Heterogeneous AST node type
        protected Token token;        // This node created from which token?
        public HeteroAST() { ; }
        public HeteroAST(Token token) { this.token = token; }
        public String toString() { return token.ToString(); }

        public void visit(IVisitor visitor) { visitor.visit(this); }

    }

    /** A generic heterogeneous tree node used in our vector math trees */
    public abstract class VecMathNode : HeteroAST 
    {
        public VecMathNode() {;}
        public VecMathNode(Token token) : base(token) { }
        public void print() 
        {
            // generic print tree-walker method
            System.Console.Write((token != null)? token.ToString() : "<null>" );
        }

    }

    public abstract class StatementNode : HeteroAST
    {
        public HeteroAST Statement;
        public HeteroAST Statements;
        public StatementNode() { ;}
        public StatementNode(Token token) : base(token) { }
        public void print()
        {
            // generic print tree-walker method
            System.Console.Write((token != null) ? token.ToString() : "<null>");
        }


    }




    // Visitor pattern -- Structural example

    /// <summary>
    /// MainApp startup class for Structural
    /// Visitor Design Pattern.
    /// </summary>
    class MainApp
    {
        static void Main2()
        {
            // Setup structure
            ObjectStructure o = new ObjectStructure();
            o.Attach(new ConcreteElementA());
            o.Attach(new ConcreteElementB());
 
            // Create visitor objects
            ConcreteVisitor1 v1 = new ConcreteVisitor1();
            ConcreteVisitor2 v2 = new ConcreteVisitor2();
 
            // Structure accepting visitors
            o.Accept(v1);
            o.Accept(v2);
 
            // Wait for user
            Console.ReadKey();

            cGraph g = new cGraph();

            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            g.AddEdge(2, 3);
            //g.AddEdge(3, 4);

            Console.WriteLine("Breadth-First:");
            g.BreadthFirstSearch();
            Console.WriteLine();
            Console.WriteLine("Depth-First:");
            g.DepthFirstSearch();
            Console.WriteLine();
            g.TopologicalSort();
            Console.WriteLine();

            g = new cGraph();

            g.AddVertex("CS1");
            g.AddVertex("CS2");
            g.AddVertex("Data Structures");
            g.AddVertex("Operating Systems");
            g.AddVertex("Algorithms");
            g.AddVertex("Assembly Language");

            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            g.AddEdge(2, 3);

            g.AddEdge(1, 5);
            g.AddEdge(2, 4);

            Console.WriteLine("Breadth-First:");
            g.BreadthFirstSearch();
            Console.WriteLine();
            Console.WriteLine("Depth-First:");
            g.DepthFirstSearch();
            Console.WriteLine();
            g.TopologicalSort();
            Console.WriteLine();

            g = new cGraph();
            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddVertex("E");
            g.AddVertex("F");
            g.AddVertex("G");
            g.AddVertex("H");
            g.AddVertex("I");
            g.AddVertex("J");
            g.AddVertex("K");
            g.AddVertex("L");
            g.AddVertex("M");

            g.AddEdge(0, 1);
            g.AddEdge(1, 2);
            g.AddEdge(2, 3);
            g.AddEdge(0, 4);
            g.AddEdge(4, 5);
            g.AddEdge(5, 6);
            g.AddEdge(0, 7);
            g.AddEdge(7, 8);
            g.AddEdge(8, 9);
            g.AddEdge(0, 10);
            g.AddEdge(10, 11);
            g.AddEdge(11, 12);


            Console.WriteLine("Breadth-First:");
            g.BreadthFirstSearch();
            Console.WriteLine();
            Console.WriteLine("Depth-First:");
            g.DepthFirstSearch();
            Console.WriteLine();
            g.TopologicalSort();
            Console.WriteLine();


            g = new cGraph(cGraph.cGraphTypeEnum.undirected);
            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddVertex("E");
            g.AddVertex("F");
            g.AddVertex("G");


            g.AddEdge(0, 1);
            g.AddEdge(0, 2);
            g.AddEdge(0, 3);
            g.AddEdge(1, 2);
            g.AddEdge(1, 3);
            g.AddEdge(1, 4);
            g.AddEdge(2, 3);
            g.AddEdge(2, 5);
            g.AddEdge(3, 5);
            g.AddEdge(3, 4);
            g.AddEdge(3, 6);
            g.AddEdge(4, 5);
            g.AddEdge(4, 6);
            g.AddEdge(5, 6);


            Console.WriteLine("Minimum Spanning Tree:");
            Console.WriteLine();
            g.Mst();
            Console.WriteLine();


            g = new cGraph(cGraph.cGraphTypeEnum.directed | cGraph.cGraphTypeEnum.weigthed);
            g.AddVertex("A");
            g.AddVertex("B");
            g.AddVertex("C");
            g.AddVertex("D");
            g.AddVertex("E");
            g.AddVertex("F");
            g.AddVertex("G");

            g.AddEdge(0, 1, 2);
            g.AddEdge(0, 3, 1);
            g.AddEdge(1, 3, 3);
            g.AddEdge(1, 4, 10);
            g.AddEdge(2, 5, 5);
            g.AddEdge(2, 0, 4);
            g.AddEdge(3, 2, 2);
            g.AddEdge(3, 5, 8);
            g.AddEdge(3, 4, 2);
            g.AddEdge(3, 6, 4);
            g.AddEdge(4, 6, 6);
            g.AddEdge(6, 5, 1);

            Console.WriteLine("Shortest paths:");
            Console.WriteLine();
            g.Path();
            Console.WriteLine();
            Console.WriteLine("Breadth-First:");
            g.BreadthFirstSearch();
            Console.WriteLine();
            Console.WriteLine("Depth-First:");
            g.DepthFirstSearch();
            Console.WriteLine();
            g.TopologicalSort();
            Console.WriteLine("Finished.");
            Console.ReadKey();
        }
    }
 
    /// <summary>
    /// The 'Visitor' abstract class
    /// </summary>
    abstract class Visitor
    {
        public abstract void VisitConcreteElementA(ConcreteElementA concreteElementA);
        public abstract void VisitConcreteElementB(ConcreteElementB concreteElementB);
    }
 
    /// <summary>
    /// A 'ConcreteVisitor' class
    /// </summary>
    class ConcreteVisitor1 : Visitor
    {
        public override void VisitConcreteElementA(ConcreteElementA concreteElementA)
        {
            Console.WriteLine("{0} visited by {1}",
            concreteElementA.GetType().Name, this.GetType().Name);
        }
 
        public override void VisitConcreteElementB(ConcreteElementB concreteElementB)
        {
            Console.WriteLine("{0} visited by {1}",
            concreteElementB.GetType().Name, this.GetType().Name);
        }
    }
 
    /// <summary>
    /// A 'ConcreteVisitor' class
    /// </summary>
    class ConcreteVisitor2 : Visitor
    {
        public override void VisitConcreteElementA(ConcreteElementA concreteElementA)
        {
            Console.WriteLine("{0} visited by {1}",
            concreteElementA.GetType().Name, this.GetType().Name);
        }
 
        public override void VisitConcreteElementB(ConcreteElementB concreteElementB)
        {
            Console.WriteLine("{0} visited by {1}",
            concreteElementB.GetType().Name, this.GetType().Name);
        }
    }
 
    /// <summary>
    /// The 'Element' abstract class
    /// </summary>
    abstract class Element
    {
        public abstract void Accept(Visitor visitor);
    }
 
    /// <summary>
    /// A 'ConcreteElement' class
    /// </summary>
    class ConcreteElementA : Element
    {
        public override void Accept(Visitor visitor)
        {
            visitor.VisitConcreteElementA(this);
        }
 
        public void OperationA()
        {
        }
    }
 
    /// <summary>
    /// A 'ConcreteElement' class
    /// </summary>
    class ConcreteElementB : Element
    {
        public override void Accept(Visitor visitor)
        {
            visitor.VisitConcreteElementB(this);
        }
 
        public void OperationB()
        {
        }
    }
 
    /// <summary>
    /// The 'ObjectStructure' class
    /// </summary>
    class ObjectStructure
    {
        private List<Element> _elements = new List<Element>();
 
        public void Attach(Element element)
        {
            _elements.Add(element);
        }
 
        public void Detach(Element element)
        {
            _elements.Remove(element);
        }
 
        public void Accept(Visitor visitor)
        {
            foreach (Element element in _elements)
            {
                element.Accept(visitor);
            }
        }
    }
    public class BinaryTree
    {
        private System.Collections.Generic.Stack<TreeNode> NodeStack = new System.Collections.Generic.Stack<TreeNode>();
        private BinaryTree TreeList = new BinaryTree();

        public TreeNode root;

        public BinaryTree()
        {
            this.root = new TreeNode("start");
        }
        public TreeNode GetLast()
        {
            TreeNode Current = this.root;
            while (Current.Right != null)
            {
                Current = Current.Right;
            }

            return Current;
        }

        public void Print()
        {
            this.root.Print();
        }

        private void ReductionEvent(ReduceEventArgs pValue)
        {

            switch (pValue.Token.Symbol.ToString())
            {
                case "<Value>":
                    string Value = "";//pValue.Token.Tokens[0].ToString().Trim(new char[] { '{', '}' });
                    //System.Console.Write(Value);
                    this.NodeStack.Push(new TreeNode(Value));
                    break;
                case "<Negate Exp>":
                    System.Collections.Generic.LinkedListNode<string> Node = new System.Collections.Generic.LinkedListNode<string>("-");
                    //System.Console.Write("-");
                    TreeNode NegateNode = new TreeNode("-");
                    NegateNode.Right = this.NodeStack.Pop();
                    this.NodeStack.Push(NegateNode);
                    break;
                case "<Add Exp>":
                    //System.Console.Write("+");
                    TreeNode AddExpNode = new TreeNode("+");
                    AddExpNode.Right = this.NodeStack.Pop();
                    AddExpNode.Left = this.NodeStack.Pop();
                    this.NodeStack.Push(AddExpNode);
                    break;
                case "<Assign_Statement>":
                    string Identifier = "";//args.Token.Tokens[0].ToString().Trim(new char[] { '[', ']' });
                    //System.Console.Write(Identifier);
                    //System.Console.Write("=");
                    TreeNode AssignNode = new TreeNode("=");
                    this.TreeList.GetLast().Right = AssignNode;
                    AssignNode.Right = this.NodeStack.Pop();
                    AssignNode.Left = new TreeNode(Identifier);
                    this.NodeStack.Push(AssignNode);
                    break;
                case "<Statement>":
                    TreeNode Statement = new TreeNode("statement\n");
                    Statement.Right = this.NodeStack.Pop();
                    Statement.Left = this.NodeStack.Pop();
                    this.NodeStack.Push(Statement);
                    break;

                case "<StatementList>":
                    TreeNode StatementList = new TreeNode("sl\n");
                    StatementList.Right = this.NodeStack.Pop();
                    StatementList.Left = this.NodeStack.Pop();
                    this.NodeStack.Push(StatementList);
                    break;
                case "<Program>":
                    TreeNode Program = new TreeNode("program");
                    Program.Right = this.NodeStack.Pop();
                    Program.Left = this.NodeStack.Pop();
                    this.NodeStack.Push(Program);
                    break;
                default:
                    System.Console.WriteLine(pValue.Token.Symbol.ToString());
                    break;
            }


        }
    }
    public class TreeNode
    {
        public TreeNode Left, Right;
        public string Value;

        public TreeNode() { }
        public TreeNode(string pValue) { this.Value = pValue; }

        public void Print()
        {

            if (this.Left != null || this.Right != null)
            {
                System.Console.Write("(");
            }

            System.Console.Write(this.Value);
            System.Console.Write(" ");
            if (this.Left != null)
            {
                this.Left.Print();
            }


            /*
            if (grammar.IdentifierList.ContainsKey(this.Value))
            {
                System.Console.Write(grammar.IdentifierList[this.Value]);
            }
            else
            {

                System.Console.Write(this.Value);
            }*/

            if (this.Right != null)
            {
                System.Console.Write(" ");
                this.Right.Print();
            }
            if (this.Left != null || this.Right != null)
            {
                System.Console.Write(") ");
            }

        }

  
    }

    public class cVertex
    {
        public bool wasVisited;
        public bool isInTree;
        public string label;

        public cVertex(string pLabel)
        {
            this.label = pLabel;
            this.wasVisited = false;
            this.isInTree = false;
        }
    }


    public class cDistOriginal
    {
        public int distance;
        public int parentVertex;

        public cDistOriginal(int pParentVertex, int pDistance)
        {
            this.distance = pDistance;
            this.parentVertex = pParentVertex;
        }


    }
    public class cGraph
    {
        public enum cGraphTypeEnum { undirected = 0x0, directed = 0x1, weigthed = 0x2 }

        private const int MAX_VERTICES = 20;
        private cVertex[] verticies;
        cDistOriginal[] sPath;
        private int[,] adMatrix;
        int numVerts;
        int nTree;
        const int infinity = 1000000;
        int currentVert;
        int startToCurrent;


        private cGraphTypeEnum graphType;

        public cGraph(cGraphTypeEnum pGraphType = cGraphTypeEnum.directed)
        {
            this.graphType = pGraphType;
            this.verticies = new cVertex[MAX_VERTICES];
            this.adMatrix = new int[MAX_VERTICES, MAX_VERTICES];
            numVerts = 0;
            nTree = 0;
            for (int j = 0; j < MAX_VERTICES; j++)
            {
                if ((pGraphType & cGraphTypeEnum.weigthed) == cGraphTypeEnum.weigthed)
                {
                    for (int k = 0; k < MAX_VERTICES; k++)
                    {
                        this.adMatrix[j, k] = infinity;
                    }
                }
                else
                {
                    for (int k = 0; k < MAX_VERTICES; k++)
                    {
                        this.adMatrix[j, k] = 0;
                    }
                }

            }

            sPath = new cDistOriginal[MAX_VERTICES];
        }

        public void AddVertex(string pLabel)
        {
            this.verticies[numVerts] = new cVertex(pLabel);
            numVerts++;
        }

        public void AddEdge(int pStart, int pEnd)
        {
            this.adMatrix[pStart, pEnd] = 1;
            if (this.graphType == cGraphTypeEnum.undirected)
            {
                this.adMatrix[pEnd, pStart] = 1;
            }
        }

        public void AddEdge(int pStart, int pEnd, int pWeight)
        {
            this.adMatrix[pStart, pEnd] = pWeight;
            if (this.graphType == cGraphTypeEnum.undirected)
            {
                this.adMatrix[pEnd, pStart] = pWeight;
            }
        }

        public void ShowVertex(int pValue)
        {
            Console.Write(verticies[pValue].label + " ");
        }

        #region Topological Sort Methods

        public int NoSuccessors()
        {
            bool hasFoundAnEdge;
            for (int row = 0; row < this.numVerts; row++)
            {
                hasFoundAnEdge = false;
                for (int col = 0; col < this.numVerts; col++)
                {
                    if (this.adMatrix[row, col] > 0)
                    {
                        hasFoundAnEdge = true;
                        break;
                    }
                }

                if (!hasFoundAnEdge)
                {
                    return row;
                }
            }


            return -1;
        }


        private void MoveRow(int row, int length)
        {
            for (int col = 0; col < length; col++)
            {
                this.adMatrix[row, col] = this.adMatrix[row + 1, col];
            }
        }


        private void MoveCol(int col, int length)
        {
            for (int row = 0; row < length; row++)
            {
                this.adMatrix[row, col] = this.adMatrix[row, col + 1];
            }
        }

        public void DelVertex(int pVert)
        {
            if (pVert != numVerts)
            {
                for (int j = pVert; j < numVerts; j++)
                {
                    this.verticies[j] = this.verticies[j + 1];
                }

                for (int row = pVert; row < numVerts; row++)
                {
                    MoveRow(row, numVerts);

                }

                for (int col = pVert; col < numVerts; col++)
                {
                    MoveCol(col, numVerts);
                }

                numVerts--;
            }
            else
            {

            }
        }

        public void TopologicalSort()
        {
            int origVerts = this.numVerts;
            Stack<string> gStack = new Stack<string>();
            while (this.numVerts > 0)
            {
                int currVertex = NoSuccessors();
                if (currVertex == -1 && this.numVerts > 1)
                {
                    Console.WriteLine("Error: graph has cycles.");
                    return;
                }
                if (this.numVerts > 1)
                {

                    gStack.Push(this.verticies[currVertex].label);
                    DelVertex(currVertex);
                }
                else
                {
                    gStack.Push(this.verticies[0].label);
                    DelVertex(0);
                }
            }

            Console.Write("Topological sorting order: ");
            while (gStack.Count > 0)
            {
                Console.Write(gStack.Pop() + " ");
            }
        }
        #endregion


        #region Depth-First Searh
        private int GetAdjUnvisitedVertex(int v)
        {
            for (int j = 0; j < numVerts; j++)
            {
                ////if (this.adMatrix[v, j] == 1 && this.verticies[j].wasVisited == false)
                if ((this.adMatrix[v, j] > 0 && this.adMatrix[v,j] < infinity) && this.verticies[j].wasVisited == false)
                {
                    return j;
                }
            }

            return -1;
        }

        public void DepthFirstSearch()
        {
            Stack<int> gStack = new Stack<int>();
            this.verticies[0].wasVisited = true;
            ShowVertex(0);
            gStack.Push(0);
            int v;
            while (gStack.Count > 0)
            {
                v = GetAdjUnvisitedVertex(gStack.Peek());
                if (v == -1)
                {
                    gStack.Pop();
                }
                else
                {
                    this.verticies[v].wasVisited = true;
                    ShowVertex(v);
                    gStack.Push(v);
                }
            }

            for (int j = 0; j < this.numVerts; j++)
            {
                this.verticies[j].wasVisited = false;
            }
        }

        #endregion


        #region Breadth-First Search
        public void BreadthFirstSearch()
        {
            Queue<int> gQueue = new Queue<int>();
            this.verticies[0].wasVisited = true;
            ShowVertex(0);
            gQueue.Enqueue(0);
            int vert1, vert2;
            while (gQueue.Count > 0)
            {
                vert1 = gQueue.Dequeue();
                vert2 = GetAdjUnvisitedVertex(vert1);
                while (vert2 != -1)
                {
                    this.verticies[vert2].wasVisited = true;
                    ShowVertex(vert2);
                    gQueue.Enqueue(vert2);
                    vert2 = GetAdjUnvisitedVertex(vert1);
                }
            }

            for (int i = 0; i < numVerts; i++)
            {
                this.verticies[i].wasVisited = false;
            }


        }

        #endregion

        #region Shortest Path

        public int GetMin()
        {
            int minDist = infinity;
            int indexMin = 0;

            for (int j = 0; j < this.numVerts; j++)
            {
                if (!(this.verticies[j].isInTree) && sPath[j].distance < minDist)
                {
                    minDist = sPath[j].distance;
                    indexMin = j;
                }
            }

            return indexMin;
        }

        public void AdjustShortPath()
        {
            int column = 1;
            while (column < this.numVerts)
            {
                if (this.verticies[column].isInTree)
                {
                    column++;
                }
                else
                {
                    int currentToFring = this.adMatrix[this.currentVert, column];
                    int startToFring = this.startToCurrent + currentToFring;
                    int sPathDist = sPath[column].distance;

                    if (startToFring < sPathDist)
                    {
                        sPath[column].parentVertex = this.currentVert;
                        sPath[column].distance = startToFring;
                    }
                }
                column++;
            }
        }

        public void DisplayPaths()
        {
            for (int j = 0; j < this.numVerts; j++)
            {
                Console.Write(this.verticies[j].label + "=");
                if (sPath[j].distance == infinity)
                {
                    Console.Write("inf");
                }
                else
                {
                    Console.Write(sPath[j].distance);
                }
                string parent = this.verticies[sPath[j].parentVertex].label;
                Console.Write("(" + parent + ")");
            }
        }

        public void Path()
        {
            int startTree = 0;
            this.verticies[startTree].isInTree = true;

            this.nTree = 1;

            for (int j = 0; j < this.numVerts; j++)
            {
                int tempDist = this.adMatrix[startTree, j];
                sPath[j] = new cDistOriginal(startTree, tempDist);
            }

            while (this.nTree < this.numVerts)
            {
                int indexMin = GetMin();
                int minDist = sPath[indexMin].distance;
                this.currentVert = indexMin;
                this.startToCurrent = sPath[indexMin].distance;
                this.verticies[this.currentVert].isInTree = true;
                this.nTree++;
                this.AdjustShortPath();
            }

            this.DisplayPaths();
            this.nTree = 0;
            for (int i = 0; i < this.numVerts; i++)
            {
                this.verticies[i].isInTree = false;
            }

        }
        #endregion

        #region Minimum Spanning Tree
        public void Mst()
        {
            if ((this.graphType & cGraphTypeEnum.undirected) == cGraphTypeEnum.undirected)
            {
                Stack<int> gStack = new Stack<int>();
                this.verticies[0].wasVisited = true;
                gStack.Push(0);
                int currVertex, ver;

                while (gStack.Count > 0)
                {
                    currVertex = gStack.Peek();
                    ver = GetAdjUnvisitedVertex(currVertex);
                    if (ver == -1)
                    {
                        gStack.Pop();
                    }
                    else
                    {
                        this.verticies[ver].wasVisited = true;
                        gStack.Push(ver);
                        ShowVertex(currVertex);
                        ShowVertex(ver);
                        Console.Write(" ");
                    }
                }
                for (int j = 0; j < this.numVerts; j++)
                {
                    this.verticies[j].wasVisited = false;
                }
            }
            else
            {
                Console.Write("requires undirected graph. ");
            }
        }

        #endregion
    }

}
