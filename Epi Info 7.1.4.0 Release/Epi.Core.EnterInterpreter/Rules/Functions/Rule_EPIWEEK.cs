using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Abs reduction.
    /// </summary>
    public partial class Rule_EPIWEEK : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();
        private DateTime _dateTimeGiven;
        private int _firstDayOfWeek = 0;

        public Rule_EPIWEEK(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            // Program code and logic by David Nitschke
            // Updated by Erik Knudsen
            // Updated by David Brown

            object result = null;

            if (this.ParameterList.Count >= 1)
            {
                if (this.ParameterList[0] != null && this.ParameterList[0].Execute() != null)
                {
                    if (DateTime.TryParse(this.ParameterList[0].Execute().ToString(), out _dateTimeGiven) == false)
                    {
                        return null;
                    }
                }
            }

            if (this.ParameterList.Count >= 2)
            {
                if (this.ParameterList[1].Execute().ToString() != null)
                {
                    string firstDayString = this.ParameterList[1].Execute().ToString();
                    int parsedInt = 0;
                    if (int.TryParse(firstDayString, out parsedInt))
                    {
                        _firstDayOfWeek = parsedInt - 1;
                    }
                }
            }

            if (_dateTimeGiven != null)
            {
                DateTime MMWR__Start;
                MMWR__Start = GetMMWRStart(_dateTimeGiven, _firstDayOfWeek);

                TimeSpan MMWR__DayCount = _dateTimeGiven.Subtract(MMWR__Start);
                int MMWR__Week = ((int)(MMWR__DayCount.Days / 7)) + 1;
                
                return MMWR__Week;
            }
            else
            {
                return result;
            }
        }
        
        /// <summary>
        /// GetMMWRStart returns the date of the start of the MMWR year closest to Jan 01
        /// of the year passed in.  It finds 01/01/yyyy first then moves forward or back
        /// the correct number of days to be the start of the MMWR year.  MMWR Week #1 is 
        /// always the first week of the year that has a minimum of 4 days in the new year.
        /// If Jan. first falls on a Thurs, Fri, or Sat, the MMWRStart date returned could be
        /// greater than the date passed in so this must be checked for by the calling Sub.
        ///
        /// If Jan. first is a Mon, Tues, or Wed, the MMWRStart goes back to the last
        /// Sunday in Dec of the previous year which is the start of MMWR Week 1 for the
        /// current year.
        ///
        /// If the first of January is a Thurs, Fri, or Sat, the MMWRStart goes forward to 
        /// the first Sunday in Jan of the current year which is the start of
        /// MMWR Week 1 for the current year.  For example, if the year passed
        /// in was 01/02/1998, a Friday, the MMWRStart that is returned is 01/04/1998, a Sunday.  
        /// Since 01/04/1998 > 01/02/1998, we must subract a year and pass Jan 1 of the new
        /// year into this function as in GetMMWRStart("01/01/1997").
        /// The MMWRStart date would then be returned as the date of the first
        /// MMWR Week of the previous year. 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns></returns>
        private DateTime GetMMWRStart(int gregorianYear, int firstDayOfWeek = 0)
        {
            DateTime dateResult;
            DateTime dateYearBegin = new DateTime(gregorianYear, 1, 1);
            DayOfWeek firstDayOfYear = (DayOfWeek)dateYearBegin.DayOfWeek;

            if (firstDayOfYear <= (DayOfWeek)firstDayOfWeek + 3)
            {
                dateResult = dateYearBegin.AddDays(firstDayOfWeek - (int)firstDayOfYear);
            }
            else
            {
                dateResult = dateYearBegin.AddDays(7 - firstDayOfWeek - (int)firstDayOfYear);
            }

            return dateResult;
        }

        private DateTime GetMMWRStart(DateTime dateTime, int firstDayOfWeek = 0)
        {
            DateTime MMWRStart_YearMinusOne = GetMMWRStart(dateTime.Year - 1, firstDayOfWeek);
            DateTime MMWRStart              = GetMMWRStart(dateTime.Year,     firstDayOfWeek);
            DateTime MMWRStart_YearPlusOne  = GetMMWRStart(dateTime.Year + 1, firstDayOfWeek);
            
            int delta = MMWRStart.Subtract(dateTime).Days;
            int delta_PlusOne = MMWRStart_YearPlusOne.Subtract(dateTime).Days;

            if (delta > 0)
            {
                return MMWRStart_YearMinusOne;
            }
            else if (delta_PlusOne > 0)
            {
                return MMWRStart;
            }
            else
            {
                return MMWRStart_YearPlusOne;
            }
        }
    }
}
