using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epi.Windows.MakeView.Excel
{
   public class Card
    {
        private string _Question;
        public string Question
        {
            get { return _Question; }
            set { _Question = value; }
        }
        private string _Variable_Name;
        public string Variable_Name
        {
            get { return _Variable_Name; }
            set { _Variable_Name = value; }
        }
        private int _Question_Type;
        public int Question_Type
        {
            get { return _Question_Type; }
            set { _Question_Type = value; }
        }
        private bool _Required;
        public bool Required
        {
            get { return _Required; }
            set { _Required = value; }
        }
        private List<string> _List_Values;
        public List<string> List_Values
        {
            get { return _List_Values; }
            set { _List_Values = value; }
        }
        private string _If_Condition;
        public string If_Condition
        {
            get { return _If_Condition; }
            set { _If_Condition = value; }
        }
        private string _Then_Question;
        public string Then_Question
        {
            get { return _Then_Question; }
            set { _Then_Question = value; }
        }

        private string _Else_Question;
        public string Else_Question
        {
            get { return _Else_Question; }
            set { _Else_Question = value; }
        }

        public int PageId { get; set; }

        public string PageName { get; set; }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }
        private int _Counter;
        public int Counter
        {
            get { return _Counter; }
            set { _Counter = value; }
        }
    }
}
