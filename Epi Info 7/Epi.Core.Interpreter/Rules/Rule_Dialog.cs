using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using com.calitha.goldparser;

using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Dialog : AnalysisRule
    {
        AnalysisRule Dialog = null;

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
            TitleText = this.GetCommandElement(token.Tokens, 2);
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
            MaskOpt = this.GetCommandElement(token.Tokens, 4);
            TitleText = this.GetCommandElement(token.Tokens, 5);
        }
        
        public override object Execute()
        {
            this.DialogThenAssign(string.Empty);
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
            this.MaskOpt = GetCommandElement(token.Tokens, 4);
            this.TitleText = GetCommandElement(token.Tokens, 5);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new Double());
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
            Modifier = GetElement(token, 3);
            tableName = GetElement(token, 4);
            variableName = GetElement(token, 5);
            TitleText = GetElement(token, 6);
        }
        public override object Execute()
        {
            DataTable outputTable = null;

            DataSets.Config.DataDriverDataTable dataDrivers = Configuration.GetNewInstance().DataDrivers;
            Epi.Data.IDbDriverFactory dbFactory = null;
            foreach (DataSets.Config.DataDriverRow dataDriver in dataDrivers)
            {
                dbFactory = Epi.Data.DbDriverFactoryCreator.GetDbDriverFactory(dataDriver.Type);

                if (dbFactory.CanClaimConnectionString(this.Context.CurrentRead.File))
                {
                    break;
                }
            }

            Epi.Data.IDbDriver OutputDriver = Epi.Data.DBReadExecute.GetDataDriver(this.Context.CurrentRead.File, true);

            outputTable = OutputDriver.GetTableData(this.tableName);

            if (outputTable != null)
            {
                
                Type columnType = outputTable.Columns[variableName].DataType;

                List<string> values = new List<string>();

                foreach (DataRow row in outputTable.Rows)
                {
                    string value = row[variableName].ToString();

                    if (values.Contains(value) == false && string.IsNullOrEmpty(value) == false)
                    {
                        values.Add(value);
                    }
                }

                this.DialogThenAssign(values);
            }

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
            Modifier = GetElement(token, 3);
            TitleText = GetElement(token, 4);
        }
        public override object Execute()
        {
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
                if (name.ToUpperInvariant().StartsWith("VIEW"))
                {
                    viewNames.Add(name);
                }
            }
            this.DialogThenAssign(viewNames);
            return null;
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
            Modifier = GetElement(token, 3);
            TitleText = GetElement(token, 4);
        }

        public override object Execute()
        {
            List<string> list = new List<string>();
            VariableCollection vars = this.Context.MemoryRegion.GetVariablesInScope(VariableType.Permanent | VariableType.Global | VariableType.Standard);
            list.AddRange(vars.Names);
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
            TitleText = this.GetCommandElement(token.Tokens, 4);
            Modifier = this.GetCommandElement(token.Tokens, 3).ToUpperInvariant();
            Identifier = this.GetCommandElement(token.Tokens, 2);
        }
        public override object Execute()
        {
            this.DialogThenAssign(new DateTime());
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
            MaskOpt = this.GetCommandElement(token.Tokens, 4);
            TitleText = this.GetCommandElement(token.Tokens, 5);
            Modifier = this.GetCommandElement(token.Tokens, 3).ToUpperInvariant();
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
    public class RuleDialogBase : AnalysisRule 
    {
        public bool HasDisplayedOnce = false;
        
        protected string mask = null;
        protected string titleText = null;
        protected string prompt = null;

        protected object value = null;

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
        protected bool DialogThenAssign(object obj)
        {
            MaskOpt = MaskOpt == null ? string.Empty : MaskOpt;
            Modifier = Modifier == null ? string.Empty : Modifier;
            
            if (this.Context.AnalysisCheckCodeInterface.Dialog(Prompt, TitleText, MaskOpt, Modifier, ref obj))
            {
                this.value = obj;
                IVariable var;
                if (this.Context.MemoryRegion.TryGetVariable(this.Identifier, out var))
                {
                    if (var.VarType != VariableType.Standard && var.VarType != VariableType.DataSource)
                    {
                        if (!Util.IsEmpty(this.value))
                        {
                            var.Expression = this.value.ToString();
                        }
                        else
                        {
                            var.Expression = "Null";
                        }
                    }
                }
                if (this.Modifier == "DATEFORMAT")
                {
                    
                    if (this.value != null)
                    {
                        DateTime dateTime = (DateTime)this.value;
                        DateTime dt = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
                        this.value = dt;
                    }
                }

                this.Context.GetOutput(AssignNonProjectDataFunction);
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
            if (this is Epi.Core.AnalysisInterpreter.Rules.Rule_Simple_Dialog_Statement)
            {
                if (!HasDisplayedOnce)
                {
                    this.Context.AnalysisCheckCodeInterface.Dialog(this.Prompt, this.TitleText);
                    HasDisplayedOnce = true;
                }
                
            }

            return null;
        }

        private void AssignNonProjectDataFunction()
        {
            if (Util.IsEmpty(this.value))
            {
                this.Context.CurrentDataRow[this.Identifier] = DBNull.Value;
            }
            else
            {
                try
                {
                    this.Context.CurrentDataRow[this.Identifier] = this.value;
                }
                catch (System.Exception ex)
                {
                    if (ex != null)
                    {
                        throw new System.Exception(string.Format("You have attempted to assign an incompatible value ({0}) to {1}", this.value, this.Identifier));
                    }
                }
            }
        }

        #endregion Public Methods
    }
}



