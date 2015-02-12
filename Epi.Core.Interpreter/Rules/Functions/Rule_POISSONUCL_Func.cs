using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Abs reduction.
    /// </summary>
    public partial class Rule_POISSONUCL_Func : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_POISSONUCL_Func(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            object result = null;
            
            object p1 = this.ParameterList[0].Execute().ToString();
            double param1;

            if(double.TryParse(p1.ToString(), out param1))
            {
                if (param1 < 0.0)
                    result = Double.NaN;
                else
                {
                    result = LimitFromChiSq(0.025, 2 * (param1 + 1.0));
                }
            }

            return result;
        }

        private static double LimitFromChiSq(double p, double df)
        {
            double limit = 0;
            double a = (df / 2.0 - 1.0) * 2.0;
            double b = 100 * Math.Sqrt(df / 2.0);

            double pVal = 0.0;

            while (Math.Abs(pVal - p) > 0.00000001)
            {
                limit = (a + b) / 2.0;
                pVal = PValFromChiSq(limit, df);
                if (Double.IsNaN(pVal))
                    return Double.NaN;
                if (pVal < p)
                    b = limit;
                else
                    a = limit;
            }

            return limit / 2.0;
        }

        private static double PValFromChiSq(double _x, double _df)
        {
            double _j;
            double _k;
            double _l;
            double _m;
            double _pi = 3.1416;

            if (_x < 0.000000001 || _df < 1.0)
                return 1.0;

            double _rr = 1.0;
            int _ii = (int)_df;

            while (_ii >= 2)
            {
                _rr = _rr * (double)_ii;
                _ii = _ii - 2;
            }

            _k = Math.Exp(Math.Floor((_df + 1.0) * 0.5) * Math.Log(Math.Abs(_x)) - _x * 0.5) / _rr;

            if (_k <= 0.0)
                return 0.0;

            if (Math.Floor(_df * 0.5) == _df * 0.5)
                _j = 1.0;
            else
                _j = Math.Sqrt(2.0 / _x / _pi);

            _l = 1.0;
            _m = 1.0;

            if (!double.IsNaN(_x) && !double.IsInfinity(_x))
            {
                while (_m >= 0.00000001)
                {
                    _df = _df + 2.0;
                    _m = _m * _x / _df;
                    _l = _l + _m;
                }
            }
            double PfX2 = 1 - _j * _k * _l;
            return PfX2;
        }        
    }
}
