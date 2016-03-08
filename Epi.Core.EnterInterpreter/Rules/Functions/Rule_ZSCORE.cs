using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Abs reduction.
    /// </summary>
    public partial class Rule_ZSCORE : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_ZSCORE(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            AddCommandVariableCheckValue(ParameterList, "zscore");
            //if (ParameterList.Count > 0)
            //{
            //    foreach (var item in ParameterList)
            //    {
            //        if (item is Rule_Value)
            //        {
            //            var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
            //            if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
            //            {
            //                this.Context.CommandVariableCheck.Add(id.ToLower(), "zscore");
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            object result = null;

            // example: ASSIGN BMIZ = ZSCORE("CDC 2000", "BMI", BMI, Age, Sex)

            string p1 = this.ParameterList[0].Execute().ToString(); // reference, e.g. "CDC 2000"
            string p2 = this.ParameterList[1].Execute().ToString(); // metric, e.g. you want to calculate BMI, "BMI"
            string p3 = this.ParameterList[2].Execute().ToString(); // the actual measurement, e.g. the BMI field in the database
            string p4 = this.ParameterList[3].Execute().ToString(); // the corresponding metric, such as Age
            string p5 = this.ParameterList[4].Execute().ToString(); // the gender, coded as 1= Male and 2= Female

            if (string.IsNullOrEmpty(p3.Trim()) || string.IsNullOrEmpty(p4.Trim()))
            {
                // no x-axis and y-axis measurements detected
                return result;
            }

            double measurement = double.Parse(p3);
            double age = double.Parse(p4);

            if (p5 != "1" && p5 != "2")
            {
                // no valid gender detected
                return result;
            }

            short gender = short.Parse(p5);
            double flag = 0; // unused

            switch (p1.ToLower())
            {
                case "cdc 2000":
                    switch (p2.ToLower())
                    {
                        case "bodymassindex":
                        case "bmi":
                            if (age < 24 || age > 240)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_BMI(measurement, age, gender, ref flag);
                            }
                            break;
                        case "heightage":
                        case "htage":
                            if (age < 24 || age > 240)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_HtAge(measurement, age, gender, ref flag);
                            }
                            break;
                        case "weightage":
                        case "wtage":
                            if (age < 0 || age > 240)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_WtAge(measurement, age, gender, ref flag);
                            }
                            break;
                        case "weightheight":
                        case "wtht":
                            if (measurement < 77 || measurement >= 121.5)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_WtHt(measurement, age, gender, ref flag);
                            }
                            break;
                        case "weightlength":
                        case "wtlgth":
                            if (measurement < 45 || measurement >= 103.5)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_WtLgth(measurement, age, gender, ref flag);
                            }
                            break;
                        case "headcircumference":
                        case "headcircum":
                        case "head":
                            if (age < 0 || age > 36)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_Head(measurement, age, gender, ref flag);
                            }
                            break;
                        case "lengthage":
                        case "lgthage":
                            if (age < 0 || age > 37)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_CDC2000_LgthAge(measurement, age, gender, ref flag);
                            }
                            break;
                    }
                    break;
                case "who cgs": // 0-5 years
                case "who child growth standards": // 0-5 years
                case "who growth standards": // 0-5 years
                case "who 2006": // 0-5 years
                    switch (p2.ToLower())
                    {
                        case "bodymassindex":
                        case "bmi":
                            if (age < 0 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 0, ref flag);
                            }
                            break;
                        case "heightage": // note: Does length-for-age when age = 0 to 24 months; the WHO cutoff assumes length for 0-2 years and height for 2-5 years. If you want to specifically do length for age, use that metric instead and it will only calculate 0-2 years.
                        case "htage":
                            if (age < 0 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 2, ref flag);
                            }
                            break;
                        case "weightage":
                        case "wtage":
                            if (age < 0 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 4, ref flag);
                            }
                            break;
                        case "weightheight":
                        case "wtht":
                            if (age < 0 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 5, ref flag);
                            }
                            break;
                        case "weightlength":
                        case "wtlgth":
                            if (age < 45 || age > 110)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 6, ref flag);
                            }
                            break;
                        case "mid-upper arm circumference":
                        case "muac":
                            //if (age < 3 || age > 60)
                            //{
                            return result;
                            //}
                            //else
                            //{
                            //    result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 6, ref flag);
                            //}
                            break;
                        case "headcircumference":
                        case "headcircum":
                        case "head":
                            if (age < 0 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 1, ref flag);
                            }
                            break;
                        case "ssf":
                            if (age < 3 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 7, ref flag);
                            }
                            break;
                        case "tsf":
                            if (age < 3 || age > 60)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 8, ref flag);
                            }
                            break;
                        case "lengthage":
                        case "lgthage":
                            if (age < 0 || age >= 24)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2006(measurement, age, gender, 2, ref flag);
                            }
                            break;
                    }
                    break;
                case "who growth reference": // 5-19 years
                case "who 2007": // 5-19 years
                    switch (p2.ToLower())
                    {
                        case "bodymassindex":
                        case "bmi":
                            if (age < 60 || age > 228)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2007(measurement, age, gender, 0, ref flag);
                            }
                            break;
                        case "heightage":
                        case "htage":
                            if (age < 60 || age > 228)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2007(measurement, age, gender, 2, ref flag);
                            }
                            break;
                        case "weightage":
                        case "wtage":
                            if (age < 61 || age > 120)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZ_WHO2007(measurement, age, gender, 4, ref flag);
                            }
                            break;
                    }
                    break;
                case "nchs 1977":
                case "who 1977":
                case "who 1978":
                case "cdc/who 1978":
                    switch (p2.ToLower())
                    {
                        case "heightage":
                        case "htage":
                            if (age < 24 || age > 119)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZScoreNCHS(measurement, age, gender, 2, ref flag);
                            }
                            break;
                        case "lengthage":
                        case "lgthage":
                            if (age < 0 || age > 23)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZScoreNCHS(measurement, age, gender, 3, ref flag);
                            }
                            break;
                        case "weightage":
                        case "wtage":
                            if (age < 0 || age > 119)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZScoreNCHS(measurement, age, gender, 4, ref flag);
                            }
                            break;
                        case "weightheight":
                        case "wtht":
                            if (age < 65 || age > 137)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZScoreNCHS(measurement, age, gender, 5, ref flag);
                            }
                            break;
                        case "weightlength":
                        case "wtlgth":
                            if (age < 49 || age > 100)
                            {
                                return result;
                            }
                            else
                            {
                                result = AnthStat.NutriDataCalc.GetZScoreNCHS(measurement, age, gender, 6, ref flag);
                            }
                            break;
                    }
                    break;
            }

            return result;
        }        
    }
}
