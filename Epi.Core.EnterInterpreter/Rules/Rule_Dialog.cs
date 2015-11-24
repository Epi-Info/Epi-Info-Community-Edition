using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using com.calitha.goldparser;

using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Dialog : EnterRule
    {
        EnterRule Dialog = null;

        public Rule_Dialog(Rule_Context pContext, NonterminalToken token) : base(pContext)
        {
            switch (token.Rule.Lhs.ToString())
            {
                case "<Simple_Dialog_Statement>":
                    this.Dialog = new Rule_Simple_Dialog_Statement(pContext, token);
                    break;
                case "<Numeric_Dialog_Implicit_Statement>":
                    this.Dialog = new Rule_Numeric_Dialog_Implicit_Statement(pContext, token);
                    break;
                case "<TextBox_Dialog_Statement>":
                    this.Dialog = new Rule_TextBox_Dialog_Statement(pContext, token);
                    break;
                case "<Numeric_Dialog_Explicit_Statement>":
                    this.Dialog = new Rule_Numeric_Dialog_Explicit_Statement(pContext, token);
                    break;
                case "<Db_Values_Dialog_Statement>":
                    this.Dialog = new Rule_Db_Values_Dialog_Statement(pContext, token);
                    break;
                case "<YN_Dialog_Statement>":
                    this.Dialog = new Rule_YN_Dialog_Statement(pContext, token);
                    break;
                case "<Db_Views_Dialog_Statement>":
                    this.Dialog = new Rule_Db_Views_Dialog_Statement(pContext, token);
                    break;
                case "<Databases_Dialog_Statement>":
                    this.Dialog = new Rule_Databases_Dialog_Statement(pContext, token);
                    break;
                case "<Db_Variables_Dialog_Statement>":
                    this.Dialog = new Rule_Db_Variables_Dialog_Statement(pContext, token);
                    break;
                case "<Multiple_Choice_Dialog_Statement>":
                    this.Dialog = new Rule_Multiple_Choice_Dialog_Statement(pContext, token);
                    break;
                case "<Dialog_Read_Statement>":
                    this.Dialog = new Rule_Dialog_Read_Statement(pContext, token);
                    break;
                case "<Dialog_Write_Statement>":
                    this.Dialog = new Rule_Dialog_Write_Statement(pContext, token);
                    break;
                case "<Dialog_Read_Filter_Statement>":
                    this.Dialog = new Rule_Dialog_Read_Filter_Statement(pContext, token);
                    break;
                case "<Dialog_Write_Filter_Statement>":
                    this.Dialog = new Rule_Dialog_Write_Filter_Statement(pContext, token);
                    break;
                case "<Dialog_Date_Statement>":
                    this.Dialog = new Rule_Dialog_Date_Statement(pContext, token);
                    break;
                case "<Dialog_Time_Statement>":
                    this.Dialog = new Rule_Dialog_Time_Statement(pContext, token);
                    break;
                case "<Dialog_DateTime_Statement>":
                    this.Dialog = new Rule_Dialog_DateTime_Statement(pContext, token);
                    break;
                case "<Dialog_Date_Mask_Statement>":
                    this.Dialog = new Rule_Dialog_Date_Mask_Statement(pContext, token);
                    break;
                default:
                    this.Dialog = new Rule_TextBox_Dialog_Statement(pContext, token);
                    break;

            }
        }

        public override object Execute()
        {
            return this.Dialog.Execute();
        }
    }

    public class Rule_Simple_Dialog_Statement : RuleDialogBase
    {
        public Rule_Simple_Dialog_Statement(Rule_Context pContext, NonterminalToken token) : base(pContext,token)
        {
            //<Simple_Dialog_Statement>	::= DIALOG String <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            if (token.Tokens.Length > 2)
            {
                TitleText = this.GetCommandElement(token.Tokens, 2);
            }
            else
            {
                TitleText = "";
            }
        }
        
        public override object Execute()
        {
            return base.Execute();
        }
    }

    public class Rule_Numeric_Dialog_Implicit_Statement : RuleDialogBase 
    {
        public Rule_Numeric_Dialog_Implicit_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Numeric_Dialog_Implicit_Statement> ::= DIALOG String Identifier <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            TitleText = this.GetCommandElement(token.Tokens, 3);
        }

        public override object Execute()
        {
            this.DialogThenAssign(new Double());
            return null;
        }
    }

    public class Rule_TextBox_Dialog_Statement : RuleDialogBase 
    {
        public Rule_TextBox_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<TextBox_Dialog_Statement> ::= DIALOG String Identifier TEXTINPUT <MaskOpt> <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            MaskOpt = this.GetCommandElement(token.Tokens, 4);
            TitleText = this.GetCommandElement(token.Tokens, 5);
        }
        
        public override object Execute()
        {
            this.DialogThenAssign("");
            return null;
        }
    }

    public class Rule_Numeric_Dialog_Explicit_Statement : RuleDialogBase 
    {
        public Rule_Numeric_Dialog_Explicit_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Numeric_Dialog_Explicit_Statement> ::= DIALOG String Identifier NUMERIC <MaskOpt> <TitleOpt>
            this.Prompt = GetCommandElement(token.Tokens, 1);
            this.Identifier = GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            this.MaskOpt = GetCommandElement(token.Tokens, 4);
            this.TitleText = GetCommandElement(token.Tokens, 5);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new Double(), EpiInfo.Plugin.DataType.Number);
            return null;
        }
    }

    public class Rule_Db_Values_Dialog_Statement : RuleDialogBase 
    {
        public Rule_Db_Values_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Db_Values_Dialog_Statement> ::= DIALOG String Identifier DBVALUES Identifier Identifier <TitleOpt>
            Prompt = GetElement(token, 1);
            Identifier = GetElement(token, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = GetElement(token, 3);
            TitleText = GetElement(token, 6);
            tableName = GetElement(token, 4);
            variableName = GetElement(token, 5);
        }
        public override object Execute()
        {
             //db values is the listing of all values in one or more databases
            List<string> list = new List<string>();
            list = this.Context.EnterCheckCodeInterface.GetDbListValues(tableName, variableName);           
            DialogThenAssign(list);
            return null;
        }
           
    }

    public class Rule_YN_Dialog_Statement : RuleDialogBase 
    {
        public Rule_YN_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<YN_Dialog_Statement>	::= DIALOG String Identifier YN <TitleOpt>
            this.Prompt = this.GetCommandElement(token.Tokens, 1);
            this.Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            this.TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new Boolean());
            return null;
        }
    }

    public class Rule_Db_Views_Dialog_Statement : RuleDialogBase 
    {
        public Rule_Db_Views_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Db_Views_Dialog_Statement> ::= DIALOG String Identifier DBVIEWS <titleOpt>
            Prompt = GetElement(token, 1);
            Identifier = GetElement(token, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = GetElement(token, 3);
            TitleText = GetElement(token, 4);
        }
        public override object Execute()
        {
            return null;


            /*
            Epi.Data.IDbDriver driver = null;
            if (this.Context.CurrentRead != null)
            {
                driver = Epi.Data.DBReadExecute.GetDataDriver(this.Context.CurrentRead.File);
            }
            else
            {
                return null;
            }

            List<string> tableNames = driver.GetTableNames();
            List<string> viewNames = new List<string>();

            foreach (string name in tableNames)
            {
                if (name.ToUpper().StartsWith("VIEW"))
                {
                    viewNames.Add(name);
                }
            }
            this.DialogThenAssign(viewNames);
            return null;*/
        }
    }

    public class Rule_Databases_Dialog_Statement : RuleDialogBase 
    {
        public Rule_Databases_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Databases_Dialog_Statement> ::= DIALOG String Identifier DATABASES <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = GetElement(token, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = this.GetCommandElement(token.Tokens, 3);
            TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        public override object Execute()
        {
            // show the open file dialog and filter on databases
            return null;
        }
    }

    public class Rule_Db_Variables_Dialog_Statement : RuleDialogBase 
    {
        public Rule_Db_Variables_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Db_Variables_Dialog_Statement> ::= DIALOG String Identifier DBVARIABLES <TitleOpt>
            Prompt = GetElement(token, 1);
            Identifier = GetElement(token, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = GetElement(token, 3);
            TitleText = GetElement(token, 4);
        }

        public override object Execute()
        {
            List<string> list = new List<string>();
            List<EpiInfo.Plugin.IVariable> vars = this.Context.CurrentScope.FindVariables(EpiInfo.Plugin.VariableScope.Permanent | EpiInfo.Plugin.VariableScope.Global | EpiInfo.Plugin.VariableScope.Standard);
            foreach (EpiInfo.Plugin.IVariable v in vars)
            {
                list.Add(v.Name);
            }
            this.DialogThenAssign(list);
            return null;
        }
    }

    public class Rule_Multiple_Choice_Dialog_Statement : RuleDialogBase 
    {
        //<Multiple_Choice_Dialog_Statement> ::= DIALOG String Identifier <StringList> <TitleOpt>

        public Rule_Multiple_Choice_Dialog_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            this.Prompt = this.GetCommandElement(token.Tokens, 1);
            this.Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            this.StringList = this.GetCommandElement(token.Tokens, 3);
            this.TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        public override object Execute()
        {
            List<string> list = new List<string>();
            string[] strings = StringList.Split(',');
            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = strings[i].Trim();
                strings[i] = strings[i].Replace("\"", "");
            }
            list.AddRange(strings);
            this.DialogThenAssign(list);
            return null;
        }
    }

    public class Rule_Dialog_Read_Statement : RuleDialogBase 
    {
        //<Dialog_Read_Statement> ::= DIALOG String Identifier READ <TitleOpt>

        public Rule_Dialog_Read_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            Filter = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = this.GetCommandElement(token.Tokens, 3);
            TitleText = this.GetCommandElement(token.Tokens, 4);
        }

        public override object Execute()
        {
            string obj = Filter;
            this.DialogThenAssign(obj);
            return null;
        }
    }

    public class Rule_Dialog_Read_Filter_Statement : RuleDialogBase
    {
        //<Dialog_Read_Filter_Statement> ::= DIALOG String Identifier READ String <TitleOpt>

        public Rule_Dialog_Read_Filter_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            Filter = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = this.GetCommandElement(token.Tokens, 3);
            TitleText = this.GetCommandElement(token.Tokens, 5);
        }

        public override object Execute()
        {
            string obj = Filter;
            this.DialogThenAssign(obj);
            return null;
        }
    }

    public class Rule_Dialog_Write_Statement : RuleDialogBase 
    {
        //<Dialog_Write_Statement> ::= DIALOG String Identifier WRITE <TitleOpt>

        public Rule_Dialog_Write_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            Filter = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = this.GetCommandElement(token.Tokens, 3);
            TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        
        public override object Execute()
        {
            string obj = Filter;
            this.DialogThenAssign(obj);
            return null;
        }
    }

    public class Rule_Dialog_Write_Filter_Statement : RuleDialogBase 
    {
        //<Dialog_Write_Filter_Statement> ::= DIALOG String Identifier WRITE String <TitleOpt>

        public Rule_Dialog_Write_Filter_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            Filter = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            Modifier = this.GetCommandElement(token.Tokens, 3);
            TitleText = this.GetCommandElement(token.Tokens, 5);
        }
        public override object Execute()
        {
            string obj = Filter;
            this.DialogThenAssign(obj);
            return null;
        }
    }

    public class Rule_Dialog_Date_Statement : RuleDialogBase 
    {
        public Rule_Dialog_Date_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Dialog_Date_Statement> ::= DIALOG String Identifier DATEFORMAT <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new DateTime(), EpiInfo.Plugin.DataType.Date);
            return null;
        }
    }

    public class Rule_Dialog_Time_Statement : RuleDialogBase
    {
        public Rule_Dialog_Time_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Dialog_Date_Statement> ::= DIALOG String Identifier TIMEFORMAT <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new DateTime(), EpiInfo.Plugin.DataType.Time);
            return null;
        }
    }

    public class Rule_Dialog_DateTime_Statement : RuleDialogBase
    {
        public Rule_Dialog_DateTime_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Dialog_Date_Statement> ::= DIALOG String Identifier DATETIMEFORMAT <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            TitleText = this.GetCommandElement(token.Tokens, 4);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new DateTime(), EpiInfo.Plugin.DataType.DateTime);
            return null;
        }
    }

    public class Rule_Dialog_Date_Mask_Statement : RuleDialogBase 
    {
        public Rule_Dialog_Date_Mask_Statement(Rule_Context pContext, NonterminalToken token)
            : base(pContext, token)
        {
            //<Dialog_Date_Mask_Statement> ::= DIALOG String Identifier DATEFORMAT String <TitleOpt>
            Prompt = this.GetCommandElement(token.Tokens, 1);
            Identifier = this.GetCommandElement(token.Tokens, 2);
            if (!string.IsNullOrEmpty(Identifier))
            {

                if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                {
                    this.Context.CommandVariableCheck.Add(Identifier, "dialog");
                }

            }
            MaskOpt = this.GetCommandElement(token.Tokens, 4);
            TitleText = this.GetCommandElement(token.Tokens, 5);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new DateTime());
            return null;
        }
    }

    /// <summary>
    /// DIALOG Command base class.
    /// </summary>
    public class RuleDialogBase : EnterRule 
    {
        protected string mask = null;
        protected string titleText = null;
        protected string prompt = null;

        protected string Filter { get; set; }
        protected string Identifier { get; set; }
        protected string Modifier { get; set; }
        protected string StringList { get; set; }
        protected string TitleOpt { get; set; }
        protected string tableName { get; set; }
        protected string variableName { get; set; }
        protected string MaskOpt
        {
            get { return mask; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Contains("\""))
                {
                    mask = value.Substring(value.IndexOf("\"") + 1).Trim().TrimEnd('\"');
                }
                else
                {
                    mask = value;
                }
            }
        }
        protected string Prompt 
        {
            get { return prompt; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Contains("\""))
                {
                    prompt = value.Substring(value.IndexOf("\"") + 1).Trim().TrimEnd('\"');
                }
                else
                {
                    prompt = value;
                }
            }
        }
        protected string TitleText
        {
            get { return titleText; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Contains("\""))
                {
                    titleText = value.Substring(value.IndexOf("\"") + 1).Trim().TrimEnd('\"');
                }
                else
                {
                    titleText = value;
                }
            }
        }

        #region Constructors

        public RuleDialogBase(Rule_Context pContext, NonterminalToken token)
            : base(pContext)
        {

        }
        #endregion Constructors

        #region Private Methods
        #endregion  Private Methods

        #region Protected methods
        protected bool DialogThenAssign(object obj, EpiInfo.Plugin.DataType dataType = EpiInfo.Plugin.DataType.Unknown)
        {
            MaskOpt = MaskOpt == null ? string.Empty : MaskOpt;
            MaskOpt = MaskOpt == "None" ? string.Empty : MaskOpt;
            Modifier = Modifier == null ? string.Empty : Modifier;
            
            if (this.Context.EnterCheckCodeInterface.Dialog(Prompt, TitleText, MaskOpt, Modifier, ref obj, dataType))
            {
                if (obj is StringBuilder)
                {
                    Assign.AssignValue(this.Context, Identifier, obj.ToString());
                }
                else
                {
                    Assign.AssignValue(this.Context, Identifier, obj);
                }

                return true;
            }

            return false;
        }
        protected string GetElement(NonterminalToken token, int index)
        {
            string returnToken = string.Empty;

            if (token.Tokens.Length > index )
            {
                returnToken = this.GetCommandElement(token.Tokens, index);
            }
            
            return returnToken;
        }
        
        #endregion Protected methods

        #region Public Methods

        /// <summary>
        /// Executes the DIALOG command
        /// </summary>
        /// <returns> CommandProcessorResults </returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.Dialog(this.Prompt, this.TitleText);
            return null;
        }
        #endregion Public Methods
    }

    /// <summary>
    /// Assign
    /// </summary>
    public class Assign
    {
        public static bool AssignValue(Rule_Context pContext, string varName, object value)
        {
            EpiInfo.Plugin.IVariable var;
            string dataValue = string.Empty;
            var = pContext.CurrentScope.Resolve(varName);

            if (var != null)
            {
                if (var.VariableScope ==  EpiInfo.Plugin.VariableScope.DataSource)
                {
                    //var.Expression = value.ToString();
                    pContext.EnterCheckCodeInterface.Assign(varName, value);
                }
                else
                {
                    if (value != null)
                    {
                        var.Expression = value.ToString();
                    }
                    else
                    {
                        var.Expression = "Null";
                    }
                }
            }
            else
            {
                if (value != null)
                {
                    pContext.EnterCheckCodeInterface.Assign(varName, value);
                }
            }

            return false;
        }

        private static Epi.DataType GuessDataTypeFromExpression(string expression)
        {
            double d = 0.0;
            DateTime dt;
            if (double.TryParse(expression, out d))
            {
                return DataType.Number;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(expression, "([+,-])"))
            {
                return DataType.Boolean;
            }
            if (DateTime.TryParse(expression, out dt))
            {
                return DataType.Date;
            }
            return DataType.Unknown;
        }

    }

}



