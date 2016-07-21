using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using com.calitha.goldparser.lalr;
using com.calitha.commons;
using com.calitha.goldparser;
using Epi.Core.AnalysisInterpreter.Rules;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter
{
    [Serializable()]
    public class SymbolException : System.Exception
    {
        public SymbolException(string message) : base(message)
        {
        }

        public SymbolException(string message,
            Exception inner) : base(message, inner)
        {
        }

        protected SymbolException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable()]
    public class RuleException : System.Exception
    {
        public RuleException(string message) : base(message)
        {
        }

        public RuleException(string message,
                             Exception inner) : base(message, inner)
        {
        }

        protected RuleException(SerializationInfo info,
                                StreamingContext context) : base(info, context)
        {
        }
    }

    public class EpiInterpreterParser : IAnalysisInterpreter, IAnalysisInterpreterHost 
    {
        private const string name = "EpiEnterInterpreter";
        private bool IsExecuteMode = false;
        private LALRParser parser;
        private IAnalysisInterpreterHost host = null;
        private IAnalysisCheckCode AnalysisCheckCodeInterface = null;
        private Rule_Context mContext;
        public AnalysisRule ProgramStart = null;
        private string commandText = String.Empty;
        private Stack<Token> tokenStack = new Stack<Token>();

        public Rule_Context Context
        {
            get { return mContext; }
        }

        public string Name
        {
            get { return this.Name; }
        }

        public IAnalysisInterpreterHost Host
        {
            get { return this.host; }
            set
            { 
                this.host = value;
                this.mContext.AnalysisInterpreterHost = (this.host);
            }

        }

        //public event CommunicateUIEventHandler CommunicateUI;

        const string RESOURCES_LANGUAGE_RULES = "Epi.Core.AnalysisInterpreter.grammar.EpiInfo.Analysis.Grammar.cgt";

        /// <summary>
        /// Returns Epi Info compiled grammar table as stream
        /// </summary>
        /// <returns></returns>
        public static System.IO.Stream GetCompiledGrammarTable()
        {
            System.IO.Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCES_LANGUAGE_RULES);
            return resourceStream;
        }

        
        /*
        /// <summary>
        /// onCommunicateUI()
        /// </summary>
        /// <remarks>
        /// zack for communicate UI
        /// </remarks>
        /// <param name="msg"></param>
        /// <param name="msgType"></param>
        /// <returns></returns>
        public bool onCommunicateUI(EpiMessages msg, MessageType msgType)
        {
            bool b;
            b = CommunicateUI(msg, msgType);
            return b;
        }*/

        public EpiInterpreterParser(string filename)
        {
            FileStream stream = new FileStream(filename,
                                               FileMode.Open, 
                                               FileAccess.Read, 
                                               FileShare.Read);
            Init(stream);
            stream.Close();
        }



        public EpiInterpreterParser(string filename, IAnalysisCheckCode pAnalysisCheckCodeInterface)
        {
            this.AnalysisCheckCodeInterface = pAnalysisCheckCodeInterface;

            FileStream stream = new FileStream(filename,
                                   FileMode.Open,
                                   FileAccess.Read,
                                   FileShare.Read);
            Init(stream);
            stream.Close();
        }

        public EpiInterpreterParser(string baseName, string resourceName)
        {
            byte[] buffer = ResourceUtil.GetByteArrayResource(
                                System.Reflection.Assembly.GetExecutingAssembly(),
                                baseName,
                                resourceName);

            MemoryStream stream = new MemoryStream(buffer);
            Init(stream);
            stream.Close();
        }

        public EpiInterpreterParser(Stream stream)
        {
            Init(stream);
        }

        private void Init(Stream stream)
        {

            

            CGTReader reader = new CGTReader(stream);

            parser = reader.CreateNewParser();

            parser.StoreTokens = LALRParser.StoreTokensMode.NoUserObject;
            parser.TrimReductions = false;
            parser.OnReduce += new LALRParser.ReduceHandler(ReduceEvent);
            parser.OnTokenRead += new LALRParser.TokenReadHandler(TokenReadEvent);
            parser.OnAccept += new LALRParser.AcceptHandler(AcceptEvent);
            parser.OnTokenError += new LALRParser.TokenErrorHandler(TokenErrorEvent);
            parser.OnParseError += new LALRParser.ParseErrorHandler(ParseErrorEvent);

            Configuration config = null;

            // check to see if config is already loaded
            // if NOT then load default
            try
            {
                config = Configuration.GetNewInstance();

                if (config == null)
                {
                    Configuration.Load(Configuration.DefaultConfigurationPath);
                }
            }
            catch
            {
                // It may throw an error in the Configuration.GetNewInstanc()
                // so try this from the default config path
                Configuration.Load(Configuration.DefaultConfigurationPath);
            }

            mContext = new Rule_Context();

            // this is done to ensure that the encourage copying of the Epi.Analysis.Statistics dll
            // to the build folder;
            //System.Console.WriteLine(Epi.Analysis.Statistics.Frequency.Equals(null,null));
            
        }



        public EpiInterpreterParser(IAnalysisCheckCode pAnalysisCheckCodeInterface)
        {
            Stream stream = EpiInterpreterParser.GetCompiledGrammarTable(); 
            Init(stream);
            this.AnalysisCheckCodeInterface = pAnalysisCheckCodeInterface;
        }

        public void Parse(string source)
        {
            try
            {
                parser.TrimReductions = true;
                this.commandText = source;
                this.IsExecuteMode = false;
                parser.TrimReductions = true;
                parser.Parse(source);

            }
            catch(InvalidOperationException ex)
            {
                if (!ex.Message.ToUpperInvariant().Contains("STACK EMPTY"))
                {
                    throw ex;
                }
            }
        }

        public void Execute(string source)
        {
            try
            {
                string newsourcetext = null;
                parser.TrimReductions = true;
                this.commandText = source;
                this.IsExecuteMode = true;
                parser.TrimReductions = true;
                if (source.ToUpperInvariant().Contains("DISPLAY DBVARIABLES\n"))
                    newsourcetext = source.ToUpperInvariant().Replace("DISPLAY DBVARIABLES\n", "DISPLAY DBVARIABLES\nEXIT ");
                else
                    newsourcetext = source;
                parser.Parse(newsourcetext);
                
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.ToUpperInvariant().Contains("STACK EMPTY"))
                {
                    throw ex;
                }
            }
        }

        private void TokenReadEvent(LALRParser parser, TokenReadEventArgs args)
        {

            

            try
            {
                args.Token.UserObject = CreateObject(args.Token);
            }
            catch (ApplicationException ex)
            {
                args.Continue = false;
                tokenStack.Clear();
                Logger.Log(DateTime.Now + ":  " + ex.Message);
                throw ex;
            }
        }

        private Object CreateObject(TerminalToken token)
        {
            return null;
        }

        private void ReduceEvent(LALRParser parser, ReduceEventArgs args)
        {
            try
            {
                args.Token.UserObject = CreateObject(args.Token);
            }
            catch (ApplicationException ex)
            {
                args.Continue = false;
                Logger.Log(DateTime.Now + ":  " + ex.Message);
                //todo: Report message to UI?
            }
        }

        public static Object CreateObject(NonterminalToken token)
        {
            return null;
        }

        private void AcceptEvent(LALRParser parser, AcceptEventArgs args)
        {
            NonterminalToken T = (NonterminalToken)args.Token;
            mContext.AnalysisCheckCodeInterface = this.AnalysisCheckCodeInterface;
            this.Context.AnalysisInterpreterHost = this;
            AnalysisRule program = AnalysisRule.BuildStatments(mContext, T);

            if (this.IsExecuteMode)
            {
                program.Execute();
            }

            this.ProgramStart = null;
        }

        private void TokenErrorEvent(LALRParser parser, TokenErrorEventArgs args)
        {
            throw new TokenException(args);
        }

        private void ParseErrorEvent(LALRParser parser, ParseErrorEventArgs args)
        {
            throw new ParseException(args);
        }

        public List<EpiInfo.Plugin.IVariable> GetVariablesInScope()
        {
            List<EpiInfo.Plugin.IVariable> result = new List<EpiInfo.Plugin.IVariable>();
            foreach (Epi.IVariable var in this.mContext.MemoryRegion.GetVariablesInScope())
            {
                EpiInfo.Plugin.IVariable temp = (EpiInfo.Plugin.IVariable)var;

                result.Add(temp);
            }


            return result;
            
        }
        public List<EpiInfo.Plugin.IVariable> GetVariablesInScope(VariableScope scopeCombination)
        {
            List<EpiInfo.Plugin.IVariable> result = new List<EpiInfo.Plugin.IVariable>();

            Epi.VariableType VT = (Epi.VariableType) scopeCombination;

            foreach (Epi.IVariable var in this.mContext.MemoryRegion.GetVariablesInScope(VT))
            {
                EpiInfo.Plugin.IVariable temp = (EpiInfo.Plugin.IVariable)var;

                result.Add(temp);
            }

            return result;
        }
        public bool TryGetVariable(string p, out EpiInfo.Plugin.IVariable var)
        {
            bool result = false;
            var = null;
            Epi.IVariable V = null;

            result = this.mContext.MemoryRegion.TryGetVariable(p, out V);

            var = (EpiInfo.Plugin.IVariable) V;

            return result;
        }


        public void RemoveVariablesInScope(VariableScope varTypes)
        {
            this.Context.MemoryRegion.RemoveVariablesInScope((VariableType)varTypes);
        }


        public void DefineVariable(EpiInfo.Plugin.IVariable variable)
        {
            this.Context.MemoryRegion.DefineVariable((Epi.IVariable)variable);
        }

        public void UndefineVariable(string varName)
        {
            this.Context.MemoryRegion.UndefineVariable(varName);
        }

        public void CancelProcessing()
        {
            if (this.Context != null)
            {
                this.Context.isCancelRequest = true;
            }
        }

        public void PauseProcessing()
        {
            throw new Exception("Epi.Core.AnalysisInterpreter.PauseProcessing() not yet implemented");
        }
        public void ContinueProcess()
        {
            throw new Exception("Epi.Core.AnalysisInterpreter.ContinueProcessing() not yet implemented");
        }
        public bool IsOneCommandMode 
        { 
            get; 
            set; 
        }


       

        #region IAnalysisInterpreterHost Members

        public bool Register(IAnalysisInterpreter analysisInterpreter)
        {
            throw new NotImplementedException();
        }

        public void Dialog(string pTextPrompt, string pTitleText)
        {
            throw new NotImplementedException();
        }

        public void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
        {
            throw new NotImplementedException();
        }

        public bool Dialog(string text, string caption, string mask, string modifier, ref object input)
        {
            throw new NotImplementedException();
        }

        public void Display(Dictionary<string, string> pDisplayArgs)
        {
            this.AnalysisCheckCodeInterface.Display(pDisplayArgs);
        }

        public bool TryGetFileDialog(string filter, string caption, bool isReadOnly, out string filePath)
        {
            throw new NotImplementedException();
        }

        public void SetOutputFile(bool useAutomatedFileName)
        {
            throw new NotImplementedException();
        }

        public void SetOutputFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void ChangeOutput(string fileName, bool isReplace, bool useRouteOut)
        {
            throw new NotImplementedException();
        }

        public void Printout(string fileName)
        {
            throw new NotImplementedException();
        }

        public void RunProgram(string command)
        {
            throw new NotImplementedException();
        }

        public void Quit()
        {
            throw new NotImplementedException();
        }

        public void DisplayStatusMessage(Dictionary<string, string> pStatusArgs)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
