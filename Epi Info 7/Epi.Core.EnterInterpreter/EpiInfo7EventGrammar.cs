using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using com.calitha.goldparser.lalr;
using com.calitha.commons;
using com.calitha.goldparser;
using Epi.Core.EnterInterpreter.Rules;
using EpiInfo.Plugin;

namespace Epi.Core.EnterInterpreter
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

        public RuleException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RuleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class EpiInterpreterParser : IEnterInterpreter
    {
        private const string name = "EpiEnterInterpreter";
        private bool IsExecuteMode = false;
        private IEnterInterpreterHost host = null;
        private LALRParser parser;
        private IEnterInterpreterHost EnterCheckCodeInterface = null;
        public Rule_Context RuleContext;
        public EnterRule ProgramStart = null;
        private string commandText = String.Empty;
        private Stack<Token> tokenStack = new Stack<Token>();      

        public ICommandContext Context
        {
            get { return RuleContext; }
            set { RuleContext = (Rule_Context)value; }
        }

        public string Name
        {
            get { return this.Name; }
        }

        public IEnterInterpreterHost Host
        {
            get { return this.host; }
            set{ this.host = value;}

        }
        public bool IsExecuteError
        {
            get;
            set;
        }

        const string RESOURCES_LANGUAGE_RULES = "Epi.Core.EnterInterpreter.grammar.EpiInfoGrammar.cgt";
        const string RESOURCES_LANGUAGE_RULES_ENTER = "Epi.Core.EnterInterpreter.grammar.EpiInfo.Enter.Grammar.cgt";

        /// <summary>
        /// Returns Epi Info compiled grammar table as stream
        /// </summary>
        /// <returns></returns>
        public static Stream GetCompiledGrammarTable()
        {
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCES_LANGUAGE_RULES);
            return resourceStream;
        }

        /// <summary>
        /// Returns Epi Info compiled grammar table as stream
        /// </summary>
        /// <returns></returns>
        public static Stream GetEnterCompiledGrammarTable()
        {
            Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCES_LANGUAGE_RULES_ENTER);
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

        public EpiInterpreterParser(string filename, IEnterInterpreterHost pEnterCheckCodeInterface)
        {
            this.EnterCheckCodeInterface = pEnterCheckCodeInterface;

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

        private void Init(Stream stream, IScope pScope = null)
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
            if (pScope == null)
            {
                RuleContext = new Rule_Context();
            }
            else
            {
                RuleContext = new Rule_Context(pScope);
            }
        }


        public EpiInterpreterParser(IEnterInterpreterHost pEnterCheckCodeInterface)
        {
            Stream stream = EpiInterpreterParser.GetEnterCompiledGrammarTable(); 
            Init(stream);
            pEnterCheckCodeInterface.Register(this);
            this.EnterCheckCodeInterface = pEnterCheckCodeInterface;
        }


        public EpiInterpreterParser(IEnterInterpreterHost pEnterCheckCodeInterface, IScope pScope)
        {
            Stream stream = EpiInterpreterParser.GetEnterCompiledGrammarTable();
            Init(stream, pScope);
            pEnterCheckCodeInterface.Register(this);
            this.EnterCheckCodeInterface = pEnterCheckCodeInterface;
            //this.Context
        }

        public void Parse(string source)
        {
            try
            {
                this.RuleContext.CommandVariableCheck = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                this.commandText = source;
                this.IsExecuteMode = false;
                parser.TrimReductions = true;
                parser.Parse(source);

                this.RuleContext.CommandVariableCheck = null;
            }
            catch(InvalidOperationException ex)
            {
                if (!ex.Message.ToUpper().Contains("STACK EMPTY"))
                {
                    throw ex;
                }
            }
        }

        public void Execute(string source)
        {
            if (this.host.IsExecutionEnabled)
            {
                try
                {
                    IsExecuteError = false;
                    this.RuleContext.CommandVariableCheck = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    this.commandText = source;
                    this.IsExecuteMode = true;
                    parser.TrimReductions = true;
                    parser.Parse(source);
                    this.RuleContext.CommandVariableCheck = null;

                }
                catch (InvalidOperationException ex)
                {
                    if (!ex.Message.ToUpper().Contains("STACK EMPTY"))
                    {
                        IsExecuteError = true;
                        if (this.host.IsSuppressErrorsEnabled)
                        {
                            Logger.Log(string.Format("{0} - EnterInterpreter Parse Error. : source [{1}]\n message:\n{2}", DateTime.Now, ex.Source, ex.Message));
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    IsExecuteError = true;
                    if (this.host.IsSuppressErrorsEnabled)
                    {
                        Logger.Log(string.Format("{0} - EnterInterpreter Execute : source [{1}]\n message:\n{2}", DateTime.Now, ex.Source, ex.Message));
                    }
                    else
                    {
                        throw ex;
                    }
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
            NonterminalToken nonterminalToken = (NonterminalToken)args.Token;
            
            RuleContext.EnterCheckCodeInterface = this.EnterCheckCodeInterface;

            RuleContext.ClearState();
            EnterRule program = EnterRule.BuildStatments(RuleContext, nonterminalToken);

            program.Execute();
            
            RuleContext.CheckCommandVariables();
        }

        private void TokenErrorEvent(LALRParser parser, TokenErrorEventArgs args)
        {
            throw new TokenException(args);
        }

        private void ParseErrorEvent(LALRParser parser, ParseErrorEventArgs args)
        {
            Context.ExpectedTokens = args.ExpectedTokens.ToString();
            
            if (args.NextToken != null)
            {
                Context.NextToken = args.NextToken.ToString();
            }
            
            if (args.UnexpectedToken != null)
            {
                Context.UnexpectedToken = args.UnexpectedToken.ToString();
            }

            throw new ParseException(args);
        }
    }
}
