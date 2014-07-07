using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{

    public partial class Rule_RecordCount : AnalysisRule
    {
        bool HasRun = false;
        object StoredResult = 0.0;

        public Rule_RecordCount(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //RECORDCOUNT()

        }


        /// <summary>
        /// returns the number of records in the current data set
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = 0.0;
            if (this.HasRun)
            {
                result = this.StoredResult;
            }
            else
            {
                if (this.Context.DataSet != null & this.Context.DataSet.Tables.Contains("Output"))
                {
                    List<System.Data.DataRow> DR = this.Context.GetOutput();
                    result = (double)DR.Count;
                }
                this.HasRun = true;
                this.StoredResult = result;
            }

            return result;
        }
    }
}
