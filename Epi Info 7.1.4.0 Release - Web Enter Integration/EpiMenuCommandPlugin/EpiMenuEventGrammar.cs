using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using com.calitha.goldparser.lalr;
using com.calitha.commons;
using com.calitha.goldparser;

namespace EpiMenu.CommandPlugin
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

    public class EpiMenuInterpreterParser
    {
        private LALRParser parser;
        //private IEnterCheckCode EnterCheckCodeInterface = null;
        //private IAnalysisCheckCode AnalysisCheckCodeInterface = null;
        private Rule_Context.eRunMode RunMode = Rule_Context.eRunMode.Enter;
        private Rule_Context mContext;
        public Rule ProgramStart = null;
        private string commandText = String.Empty;
        private Stack<Token> tokenStack = new Stack<Token>();

        public Rule_Context Context
        {
            get { return mContext; }
        }

        public EpiMenuInterpreterParser(string filename)
        {
            FileStream stream = new FileStream(filename,
                                               FileMode.Open, 
                                               FileAccess.Read, 
                                               FileShare.Read);
            Init(stream);
            stream.Close();
        }

        /*
        public EpiMenuInterpreterParser(string filename, IEnterCheckCode pEnterCheckCodeInterface, Rule_Context.eRunMode pRunMode)
        {
            this.EnterCheckCodeInterface = pEnterCheckCodeInterface;
            this.RunMode = pRunMode;

            FileStream stream = new FileStream(filename,
                                   FileMode.Open,
                                   FileAccess.Read,
                                   FileShare.Read);
            Init(stream);
            stream.Close();
        }

        public EpiMenuInterpreterParser(string filename, IAnalysisCheckCode pAnalysisCheckCodeInterface, Rule_Context.eRunMode pRunMode)
        {
            this.AnalysisCheckCodeInterface = pAnalysisCheckCodeInterface;
            this.RunMode = pRunMode;

            FileStream stream = new FileStream(filename,
                                   FileMode.Open,
                                   FileAccess.Read,
                                   FileShare.Read);
            Init(stream);
            stream.Close();
        }
        */

        public EpiMenuInterpreterParser(string baseName, string resourceName)
        {
            byte[] buffer = ResourceUtil.GetByteArrayResource(
                                System.Reflection.Assembly.GetExecutingAssembly(),
                                baseName,
                                resourceName);

            MemoryStream stream = new MemoryStream(buffer);
            Init(stream);
            stream.Close();
        }

        public EpiMenuInterpreterParser(Stream stream)
        {
            Init(stream);
        }

        private void Init(Stream stream)
        {
            CGTReader reader = new CGTReader(stream);
            parser = reader.CreateNewParser();
            parser.TrimReductions = false;
            parser.StoreTokens = LALRParser.StoreTokensMode.NoUserObject;

            parser.OnReduce += new LALRParser.ReduceHandler(ReduceEvent);
            parser.OnTokenRead += new LALRParser.TokenReadHandler(TokenReadEvent);
            parser.OnAccept += new LALRParser.AcceptHandler(AcceptEvent);
            parser.OnTokenError += new LALRParser.TokenErrorHandler(TokenErrorEvent);
            parser.OnParseError += new LALRParser.ParseErrorHandler(ParseErrorEvent);

            mContext = new Rule_Context();
        }
        /*
        public EpiMenuInterpreterParser(Stream stream, IEnterCheckCode pEnterCheckCodeInterface, Rule_Context.eRunMode pRunMode)
        {
            Init(stream);
            this.EnterCheckCodeInterface = pEnterCheckCodeInterface;
            this.RunMode = pRunMode;
        }

        public EpiMenuInterpreterParser(Stream stream, IAnalysisCheckCode pAnalysisCheckCodeInterface, Rule_Context.eRunMode pRunMode)
        {
            Init(stream);
            this.AnalysisCheckCodeInterface = pAnalysisCheckCodeInterface;
            this.RunMode = pRunMode;
        }*/

        public void Parse(string source)
        {
            try
            {
                this.commandText = source; 
                parser.Parse(source);
            }
            catch(InvalidOperationException ex)
            {
                if (!ex.Message.ToUpper().Contains("STACK EMPTY"))
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
                //Logger.Log(DateTime.Now + ":  " + ex.Message);
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
                //Logger.Log(DateTime.Now + ":  " + ex.Message);
                //todo: Report message to UI?
            }
        }

        public static Object CreateObject(NonterminalToken token)
        {
            return null;
        }

        private void AcceptEvent(LALRParser parser, AcceptEventArgs args)
        {
            //System.Console.WriteLine("AcceptEvent ");
            NonterminalToken T = (NonterminalToken)args.Token;


            /*
            try
            {
                Configuration.Load(Configuration.DefaultConfigurationPath);
                this.Context.module = new MemoryRegion();
            }
            catch (System.Exception ex)
            {
                Configuration.CreateDefaultConfiguration();
                Configuration.Load(Configuration.DefaultConfigurationPath);
                this.Context.module = new MemoryRegion();
            }*/

            

            if (this.RunMode == Rule_Context.eRunMode.Enter)
            {
                //mContext.EnterCheckCodeInterface = this.EnterCheckCodeInterface;
                mContext.RunMode = Rule_Context.eRunMode.Enter;
            }
            else if (this.RunMode == Rule_Context.eRunMode.Analysis)
            {
                //mContext.AnalysisCheckCodeInterface = this.AnalysisCheckCodeInterface;
                mContext.RunMode = Rule_Context.eRunMode.Analysis;
            }
            this.ProgramStart = new Rule_Statements(mContext, T);
            this.ProgramStart.Execute();
        }

        private void TokenErrorEvent(LALRParser parser, TokenErrorEventArgs args)
        {
            //throw new TokenException.TokenException(args);
        }

        private void ParseErrorEvent(LALRParser parser, ParseErrorEventArgs args)
        {
            //throw new ParseException(args);
        }
    }
}
