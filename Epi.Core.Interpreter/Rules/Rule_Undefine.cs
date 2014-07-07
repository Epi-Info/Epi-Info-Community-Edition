using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using System.Data;
using Epi;
using Epi.Data;
using Epi.Data.Services;
using Epi.DataSets;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    class Rule_Undefine : AnalysisRule
    {
        bool HasRun = false;
        private string identifier = string.Empty;

        public Rule_Undefine(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.identifier = this.GetCommandElement(pToken.Tokens, 1);
        }


        /// <summary>
        /// performs execution of the UNDEFINE command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object results = null;
            if (!this.HasRun)
            {
                if (this.Context.MemoryRegion.IsVariableInScope(identifier))
                {
                    this.Context.MemoryRegion.UndefineVariable(identifier);
                }

                if (this.Context.DataSet.Tables.Contains("variables"))
                {
                    if (this.Context.DataSet.Tables["variables"].Columns.Contains(identifier))
                    {
                        this.Context.DataSet.Tables["variables"].Columns.Remove(identifier);
                    }
                }

                if (this.Context.DataSet.Tables.Contains("Output"))
                {
                    if (this.Context.DataSet.Tables["Output"].Columns.Contains(identifier))
                    {
                        this.Context.DataSet.Tables["Output"].Columns.Remove(identifier);
                    }
                }

                this.Context.SyncVariableAndOutputTable();
                this.HasRun = true;
            }
            return results;
        }
    }
}
