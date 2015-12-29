using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using com.calitha.goldparser;
using Epi;
using Epi.Collections;
using Epi.Data;
using Epi.Data.Services;
using Epi.Fields;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
using Epi.Core.EnterInterpreter.Rules;
using EpiInfo.Plugin;

namespace Epi.Core.EnterInterpreter
{
    public class Rule_Context : ICommandContext
    {
        public IEnterInterpreterHost EnterCheckCodeInterface;
        public StringBuilder ProgramText;

        public Dictionary<string, string> StandardVariables;
        public Dictionary<string, string> CommandVariableCheck;

        private List<string> _parsedUndefinedVariables;
        private List<string> _parsedFieldNames;
        private List<string> _parsedPageNames;

        private string _expectedTokens;
        private string _nextToken;
        private string _unexpectedToken;

        private IScope currentScope;

        public Rule_DefineVariables_Statement DefineVariablesCheckcode;
        public Rule_View_Checkcode_Statement View_Checkcode;
        public Rule_Record_Checkcode_Statement Record_Checkcode;

        public Dictionary<string, IDLLClass> DLLClassList;
        public Dictionary<string, EnterRule> Page_Checkcode;
        public Dictionary<string, EnterRule> Field_Checkcode;
        public Dictionary<string, EnterRule> BeforeCheckCode;
        public Dictionary<string, EnterRule> AfterCheckCode;
        public Dictionary<string, EnterRule> PageBeforeCheckCode;
        public Dictionary<string, EnterRule> PageAfterCheckCode;
        public Dictionary<string, EnterRule> FieldBeforeCheckCode;
        public Dictionary<string, EnterRule> FieldAfterCheckCode;
        public Dictionary<string, EnterRule> FieldClickCheckCode;
        public Dictionary<string, EnterRule> Subroutine;
        public List<String> SelectCommandList = new List<String>();
        public List<String> CommandButtonFieldList = new List<String>();
        private string[] parseGetCommandSearchText(string pSearchText)
        {
            string[] result = null;
            string[] temp = pSearchText.Split('&');
            result = new string[temp.Length];

            for (int i = 0; i < temp.Length; i++)
            {
                result[i] = temp[i].Split('=')[1];
            }

            return result;
        }

        public IScope Scope 
        {
            get 
            { 
                return currentScope; 
            } 
        }

        public List<string> ParsedUndefinedVariables
        {
            get
            {
                return _parsedUndefinedVariables;
            }
        }

        public List<string> ParsedFieldNames
        {
            get
            {
                return _parsedFieldNames;
            }
        }

        public List<string> ParsedPageNames
        {
            get
            {
                return _parsedPageNames;
            }
        }

        public string ExpectedTokens
        {
            get
            {
                if (_expectedTokens == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _expectedTokens.ToLower();
                }
            }
            set
            {
                _expectedTokens = value;
            }
        }

        public string NextToken
        {
            get
            {
                if (_nextToken == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _nextToken.ToLower();
                }
            }
            set
            {
                _nextToken = value;
            }
        }

        public string UnexpectedToken
        {
            get
            {
                if (_unexpectedToken == null)
                {
                    return string.Empty;
                }
                else
                {
                    return _unexpectedToken.ToLower();
                }
            }
            set
            {
                _unexpectedToken = value;
            }
        }

        public ICommand GetCommand(string pSearchText)
        {
            ICommand result = null;
            string Level = null;
            string Event = null;
            string Identifier = null;

            string[] Parameters = parseGetCommandSearchText(pSearchText);
            Level = Parameters[0].ToLower();
            Event = Parameters[1].ToLower();
            Identifier = Parameters[2].ToLower();

            switch (Level.ToLower())
            {
                case "view":
                case "form":
                    if (Event.ToLower() == "")
                    {
                        result = this.View_Checkcode;
                    }
                    else
                    if (Event.ToLower() == "before")
                    {
                        if (this.BeforeCheckCode.ContainsKey("view"))
                        {
                            result = this.BeforeCheckCode["view"];
                        }
                    }
                    else if (Event.ToLower() == "after")
                    {
                        if (this.AfterCheckCode.ContainsKey("view"))
                        {
                            result = this.AfterCheckCode["view"];
                        }
                    }
                    break;
                case "record":
                    if (Event.ToLower() == "")
                    {
                        result = this.Record_Checkcode;
                    }
                    else
                    if (Event.ToLower() == "before")
                    {
                        if (this.BeforeCheckCode.ContainsKey("record"))
                        {
                            result = this.BeforeCheckCode["record"];
                        }
                    }
                    else if (Event.ToLower() == "after")
                    {
                        if (this.AfterCheckCode.ContainsKey("record"))
                        {
                            result = this.AfterCheckCode["record"];
                        }
                    }
                    break;
                case "page":
                    if (Event.ToLower() == "")
                    {
                        if (this.Page_Checkcode.ContainsKey(Identifier))
                        {
                            result = this.Page_Checkcode[Identifier];
                        }
                    }
                    else
                    if (Event.ToLower() == "before")
                    {
                        if (this.PageBeforeCheckCode.ContainsKey(Identifier))
                        {
                            result = this.PageBeforeCheckCode[Identifier];
                        }
                    }
                    else if (Event.ToLower() == "after")
                    {
                        if (this.PageAfterCheckCode.ContainsKey(Identifier))
                        {
                            result = this.PageAfterCheckCode[Identifier];
                        }
                    }
                    break;
                case "field":
                    if (Event.ToLower() == "")
                    {
                        if (this.Field_Checkcode.ContainsKey(Identifier))
                        {
                            result = this.Field_Checkcode[Identifier];
                        }
                    }
                    else
                    if (Event.ToLower() == "before")
                    {
                        if (this.FieldBeforeCheckCode.ContainsKey(Identifier))
                        {
                            result = this.FieldBeforeCheckCode[Identifier];
                        }
                    }
                    else if (Event.ToLower() == "after")
                    {
                        if (this.FieldAfterCheckCode.ContainsKey(Identifier))
                        {
                            result = this.FieldAfterCheckCode[Identifier];
                        }
                    }
                    else if (Event.ToLower() == "click")
                    {
                        if (this.FieldClickCheckCode.ContainsKey(Identifier))
                        {
                            result = this.FieldClickCheckCode[Identifier];
                        }
                    }
                    break;
                case "sub":
                    if (this.Subroutine.ContainsKey(Event))
                    {
                        result = this.Subroutine[Event];
                    }
                    break;
                case "definevariables":
                    result = this.DefineVariablesCheckcode;
                    break;
            }

            return result;
        }

        public IScope CurrentScope
        {
            get { return this.currentScope; }
        }

        public Rule_Context()
        {
            this.currentScope = new SymbolTable("global",null);
            Rule_Context.LoadPermanentVariables(this.currentScope);
            this.Initialize();
        }
        
        public Rule_Context(IScope pScope)
        {
            this.currentScope = new SymbolTable(pScope);
            this.Initialize();
        }
        
        private void Initialize()
        {
            IScope scope = this.currentScope;

            while (scope.GetEnclosingScope() != null)
            {
                if (scope.Name == "global")
                {
                    LoadPermanentVariables(scope);
                    break;
                }
                scope = scope.GetEnclosingScope();
            }

            ProgramText = new StringBuilder();
            DLLClassList = new Dictionary<string, IDLLClass>(StringComparer.OrdinalIgnoreCase);

            _parsedUndefinedVariables = new List<string>();
            _parsedFieldNames = new List<string>();
            _parsedPageNames = new List<string>();

            Page_Checkcode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            Field_Checkcode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);

            BeforeCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            AfterCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            PageBeforeCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            PageAfterCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            FieldBeforeCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            FieldAfterCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            FieldClickCheckCode = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            Subroutine = new Dictionary<string, EnterRule>(StringComparer.OrdinalIgnoreCase);
            SelectCommandList.Add(CommandNames.ABS);
            SelectCommandList.Add(CommandNames.AND);
            SelectCommandList.Add(CommandNames.ALWAYS);
            SelectCommandList.Add(CommandNames.APPEND);
            SelectCommandList.Add(CommandNames.MOD);
            SelectCommandList.Add(CommandNames.LIKE);
            SelectCommandList.Add(CommandNames.OR);
            SelectCommandList.Add(CommandNames.XOR);
            SelectCommandList.Add(CommandNames.NOT);
            SelectCommandList.Add(CommandNames.EXP);
            SelectCommandList.Add(CommandNames.LN);
            SelectCommandList.Add(CommandNames.LOG);
            SelectCommandList.Add(CommandNames.NUMTODATE);
            SelectCommandList.Add(CommandNames.NUMTOTIME);
            SelectCommandList.Add(CommandNames.RECORDCOUNT);
            SelectCommandList.Add(CommandNames.RND);
            SelectCommandList.Add(CommandNames.ROUND);
            SelectCommandList.Add(CommandNames.STEP);
            SelectCommandList.Add(CommandNames.SIN);
            SelectCommandList.Add(CommandNames.COS);
            SelectCommandList.Add(CommandNames.TAN);
            SelectCommandList.Add(CommandNames.TRUNC);
            SelectCommandList.Add(CommandNames.PFROMZ);
            SelectCommandList.Add(CommandNames.ZSCORE);
            SelectCommandList.Add(CommandNames.YEARS);
            SelectCommandList.Add(CommandNames.MONTHS);
            SelectCommandList.Add(CommandNames.DAYS);
            SelectCommandList.Add(CommandNames.YEAR);
            SelectCommandList.Add(CommandNames.MONTH);
            SelectCommandList.Add(CommandNames.DAY);
            SelectCommandList.Add(CommandNames.CURRENTUSER);
            SelectCommandList.Add(CommandNames.EXISTS);
            SelectCommandList.Add(CommandNames.FILEDATE);
            SelectCommandList.Add(CommandNames.SYSTEMDATE);
            SelectCommandList.Add(CommandNames.SYSTEMTIME);
            SelectCommandList.Add(CommandNames.HOURS);
            SelectCommandList.Add(CommandNames.MINUTES);
            SelectCommandList.Add(CommandNames.SECONDS);
            SelectCommandList.Add(CommandNames.HOUR);
            SelectCommandList.Add(CommandNames.MINUTE);
            SelectCommandList.Add(CommandNames.SECOND);
            SelectCommandList.Add(CommandNames.FINDTEXT);
            SelectCommandList.Add(CommandNames.FORMAT);
            SelectCommandList.Add(CommandNames.LINEBREAK);
            SelectCommandList.Add(CommandNames.STRLEN);
            SelectCommandList.Add(CommandNames.SUBSTRING);
            SelectCommandList.Add(CommandNames.TXTTONUM);
            SelectCommandList.Add(CommandNames.TXTTODATE);
            SelectCommandList.Add(CommandNames.UPPERCASE);
            SelectCommandList.Add(CommandNames.ISUNIQUE);
            SelectCommandList.Add(CommandNames.EPIWEEK);
            SelectCommandList.Add("(");
            SelectCommandList.Add(",");
           
        }

        public static void DeletePermanentVariable(string variableName)
        {
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.PermanentVariables.Select("Name='" + variableName + "'");
            if (result.Length != 1)
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.ContentsInvalid);
            }
            result[0].Delete();
            Configuration.Save(config);
        }

        public static void UpdatePermanentVariable(EpiInfo.Plugin.IVariable variable)
        {
            Configuration config = Configuration.GetNewInstance();
            DataRow[] result = config.PermanentVariables.Select("Name='" + variable.Name + "'");
            if (result.Length < 1)
            {
                config.PermanentVariables.AddPermanentVariableRow(
                   variable.Name,
                   variable.Expression ?? "",
                    (int)variable.DataType,
                   config.ParentRowPermanentVariables);
            }
            else if (result.Length == 1)
            {
                ((DataSets.Config.PermanentVariableRow)result[0]).DataValue = variable.Expression ?? "";
                ((DataSets.Config.PermanentVariableRow)result[0]).DataType = (int)variable.DataType;
            }
            else
            {
                throw new ConfigurationException(ConfigurationException.ConfigurationIssue.ContentsInvalid, "Duplicate permanent variable rows encountered.");
            }

            Configuration.Save(config);
        }

        public static void LoadPermanentVariables(IScope pScope)
        {
            Configuration config = Configuration.GetNewInstance();
            foreach (Epi.DataSets.Config.PermanentVariableRow row in config.PermanentVariables)
            {
                EpiInfo.Plugin.IVariable var = new PluginVariable(row.Name, (EpiInfo.Plugin.DataType)row.DataType, VariableScope.Permanent, row.DataValue);
                pScope.Define(var);
            }
        }

        public object GetVariable(string name)
        {
            object result = null;
            result = currentScope.Resolve(name);
            return result;
        }

        public bool SetVariable(string name, object setValue)//, Epi.VariableType pType)
        {
            bool result = false;
            string value = setValue.ToString();
            if (StandardVariables.ContainsKey(name))
            {
                StandardVariables[name] = value;
            }
            else
            {
                StandardVariables.Add(name, setValue.ToString());
            }

            return result;
        }


        /// <summary>
        /// Clears the session state
        /// </summary>
        public void ClearState()
        {
            currentScope.RemoveVariablesInScope(EpiInfo.Plugin.VariableScope.Standard, Scope.Name);

            ParsedFieldNames.Clear();
            ParsedPageNames.Clear();

            DefineVariablesCheckcode = null;
            View_Checkcode = null;
            Record_Checkcode = null;
            Page_Checkcode.Clear();
            Field_Checkcode.Clear();

            BeforeCheckCode.Clear();
            AfterCheckCode.Clear();
            PageBeforeCheckCode.Clear();
            PageAfterCheckCode.Clear();
            FieldBeforeCheckCode.Clear();
            FieldAfterCheckCode.Clear();
            FieldClickCheckCode.Clear();
            Subroutine.Clear();
        }


        public List<EpiInfo.Plugin.IVariable> GetVariablesInScope()
        {
            return this.currentScope.FindVariables(VariableScope.DataSource | VariableScope.DataSourceRedefined | VariableScope.Global | VariableScope.Permanent | VariableScope.Standard | VariableScope.System | VariableScope.Undefined);
        }

        public List<EpiInfo.Plugin.IVariable> GetVariablesInScope(VariableScope scopeCombination)
        {
            return this.currentScope.FindVariables(scopeCombination);
        }

        public bool TryGetVariable(string p, out EpiInfo.Plugin.IVariable var)
        {
            bool result = false;
            var = null;

            var = this.currentScope.Resolve(p);

            if (var != null)
            {
                result = true;
            }
            return result;
        }


        public void RemoveVariablesInScope(VariableScope varTypes)
        {
            this.currentScope.RemoveVariablesInScope(varTypes);
        }
        public void AddToCommandButtonFieldList(List<string> List)
        {
            this.CommandButtonFieldList = List;
        }

        public void DefineVariable(EpiInfo.Plugin.IVariable variable)
        {
            this.currentScope.Define(variable);
        }

        public void UndefineVariable(string varName)
        {
            this.currentScope.Undefine(varName);
        }

        private NonterminalToken FindBlock(NonterminalToken pT, string pSearchRule, string pIdentifier)
        {
            NonterminalToken result = null;
            NonterminalToken currentToken = pT;

            while (currentToken != null)
            {
                if (currentToken.Rule.Lhs.ToString().ToLower() == pSearchRule.ToLower())
                {
                    switch (pSearchRule)
                    {
                        case "<Field_Checkcode_Statement>":
                        case "<Page_Checkcode_Statement>":
                        case "<Subroutine_Statement>":
                            if (currentToken.Tokens[1].ToString().ToLower() == pIdentifier.ToLower())
                            {
                                result = currentToken;
                                return result;
                            }
                            break;
                        default:
                            result = currentToken;
                            return result;
                    }
                }

                if (currentToken.Tokens[0] is NonterminalToken)
                {
                    switch (currentToken.Rule.Rhs[0].ToString())
                    {
                        case "<Statements>":
                            currentToken = (NonterminalToken)currentToken.Tokens[1];
                            break;
                        case "<Statement>":
                        default:
                            currentToken = (NonterminalToken)currentToken.Tokens[0];
                            break;
                    }
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        public void CheckCommandVariables()
        {
            _parsedUndefinedVariables.Clear();

            if (currentScope.SymbolList.Count > 0)
            { 
                foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in this.CommandVariableCheck)
                {
                    var _CurrentScope = this.currentScope.Resolve(kvp.Key);
                    if (_CurrentScope == null )
                    {
                        _parsedUndefinedVariables.Add(kvp.Key);
                    }
                };

                if (_parsedUndefinedVariables.Count > 0)
                {
                  //  string exceptionMessage = SharedStrings.ERROR_VARIABLE_NOT_DEFINED;
                    string exceptionSource = null;
                    string Message = null;
                    foreach (string name in _parsedUndefinedVariables)
                    {


                     if (!name.Contains("\"") && !SelectCommandList.Contains(name.ToUpper()) && !this.Subroutine.ContainsKey(name) && !this.CommandButtonFieldList.Contains(name))
                      {
                        string exceptionMessage = SharedStrings.ERROR_VARIABLE_NOT_DEFINED;
                        if (name != Constants.VARIABLE_NAME_TEST_TOKEN)
                         Message +=  exceptionMessage + " " + name + "\n" ;
                        exceptionSource = name;
                      }
                    }
                    if (Message != null)
                    {
                    Exception exception = new Exception(Message);
                    exception.Source = exceptionSource;
                    throw exception;
                    }
                    
                }
            }
        }
        public IScope GetNewScope(string pName, IScope pParent)
        {
            return new SymbolTable(pName, pParent);
        }
    }
}
