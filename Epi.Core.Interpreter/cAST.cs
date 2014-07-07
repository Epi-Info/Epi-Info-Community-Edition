using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using Epi.Core.AnalysisInterpreter.Rules;

namespace Epi.Core.AnalysisInterpreter
{

    public class cAST
    {
        public TreeNode root;

        public cAST()
        {
            //this.root = new TreeNode("start");
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


        

    }
    public class TreeNode
    {
        public TreeNode Left, Right;
        public AnalysisRule Value;

        public TreeNode() { }
        public TreeNode(AnalysisRule pValue) { this.Value = pValue; }


        public object Execute()
        {
            object result = null;

            if (this.Left != null)
            {
                result = this.Left.Execute();
            }


            result = this.Value;


            if (this.Right != null)
            {
                result = this.Right.Execute();
            }

            return result;

        }


        public void Print()
        {
            if (this.Left != null)
            {
                this.Left.Print();
            }


            System.Console.Write(this.Value);
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
                this.Right.Print();
            }
        }

    }
}
