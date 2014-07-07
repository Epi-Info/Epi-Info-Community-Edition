using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using com.calitha.goldparser;
using System.Data;
using Epi.Core.AnalysisInterpreter;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    //!***			Run Statement			***!
    //Simple_Run_Statement ::= RUNPGM Identifier
    //Run_String_Statement ::= RUNPGM "String"
    //Run_File_PGM_Statement ::= RUNPGM 'File'
    //Run_PGM_In_Db_Statement ::= RUNPGM 'File':Identifier
    public class Rule_RunPGM : AnalysisRule
    {
        bool HasRun = false;
        #region protected members
        protected string identifier = string.Empty;
        protected string statement = string.Empty;
        protected string file = string.Empty;
        protected StringBuilder commandText = new StringBuilder();
        #endregion

        public Rule_RunPGM(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
        }


        public override object Execute()
        {
            object results = null;

            if (!this.HasRun)
            {
                bool resetOneCommand = false;
                if (this.Context.RunOneCommand)
                {
                    resetOneCommand = true;
                    this.Context.RunOneCommand = false;
                }
                this.Context.AnalysisCheckCodeInterface.RunProgram(commandText.ToString());
                this.Context.RunOneCommand = resetOneCommand;
                this.HasRun = true;
            }
            return results;
        }
    }

    /// <summary>
    /// Run_File_PGM_Statement ::= RUNPGM 'File'
    /// </summary>
    public class Rule_Run_File_PGM_Statement : Rule_RunPGM
    {
        public Rule_Run_File_PGM_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken)
        {
            this.file = this.GetCommandElement(pToken.Tokens, 1).Trim("'".ToCharArray());
        }

        public override object Execute()
        {
            object results = null;
            if (File.Exists(file))
            {
                StreamReader fileStream = new StreamReader(file);
                while (!fileStream.EndOfStream)
                {
                    commandText.AppendLine(fileStream.ReadLine());
                }
                fileStream.Dispose();
                base.Execute();
            }
            return results;
        }
    }

    public class Rule_Run_String_Statement : Rule_RunPGM
    {
        public Rule_Run_String_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken)
        {
            this.identifier = this.GetCommandElement(pToken.Tokens, 1).Trim('"');
        }

        public override object Execute()
        {
            object results = null;
            if (!Util.IsEmpty(this.Context.CurrentProject))
            {
                DataRow row = null;
                DataTable programs = this.Context.CurrentProject.GetPgms();
                if (programs.Rows.Count > 0)
                {
                    DataRow[] rows;
                    rows = programs.Select("[" + ColumnNames.PGM_NAME + "] = '" + this.identifier + "'");
                    if (rows.GetUpperBound(0) >= 0)      // Existing pgm
                    {
                        row = rows[0];
                    }
                }
                commandText.Append(row[ColumnNames.PGM_CONTENT].ToString());
                base.Execute();
            }
            return results;
        }
    }

    /// <summary>
    /// Run_PGM_In_Db_Statement ::= RUNPGM 'File':"Identifier"
    /// </summary>
    public class Rule_Run_PGM_In_Db_Statement : Rule_RunPGM
    {
        public Rule_Run_PGM_In_Db_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken)
        {
            this.file = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] { '\'', '"' });
            this.identifier = this.GetCommandElement(pToken.Tokens, 3).Trim('"');
        }

        public override object Execute()
        {
            Project pgmProject = new Project(file);
            DataRow row = null;
            DataTable programs = pgmProject.GetPgms();
            if (programs.Rows.Count > 0)
            {
                DataRow[] rows;
                rows = programs.Select("[" + ColumnNames.PGM_NAME + "] = '" + this.identifier + "'");
                if (rows.GetUpperBound(0) >= 0)      // Existing pgm
                {
                    row = rows[0];
                }
            }
            commandText.Append(row[ColumnNames.PGM_CONTENT].ToString());
            return base.Execute();
        }
    }
}