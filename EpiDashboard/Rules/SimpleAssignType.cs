using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard.Rules
{
    public enum SimpleAssignType
    {
        CheckboxesMarkedYes = 0,
        YesNoMarkedYes = 1,
        AllBooleanMarkedYes = 2,
        YearsElapsed = 3,
        MonthsElapsed = 4,
        DaysElapsed = 5,
        HoursElapsed = 6,
        MinutesElapsed = 7,
        TextToNumber = 8,
        TextToDate = 9,
        Substring = 10,
        StringLength = 11,
        FindText = 12,
        Round = 13,
        Uppercase = 14,
        Lowercase = 15,
        AddDays = 16,
        DetermineNonExistantListValues = 17,
        CountCheckedCheckboxesInGroup = 18,
        CountYesMarkedYesNoFieldsInGroup = 19,
        DetermineCheckboxesCheckedInGroup = 20,
        DetermineYesMarkedYesNoFieldsInGroup = 21,
        CountNumericFieldsBetweenValuesInGroup = 22,
        CountNumericFieldsOutsideValuesInGroup = 23,
        FindSumNumericFieldsInGroup = 24,
        FindMeanNumericFieldsInGroup = 25,
        FindMaxNumericFieldsInGroup = 26,
        FindMinNumericFieldsInGroup = 27,
        CountFieldsWithMissingInGroup = 28,
        CountFieldsWithoutMissingInGroup = 29,
        DetermineFieldsWithMissingInGroup = 30,
        NumberToText = 31,
        StripDate = 32,
        NumberToDate = 33
    }
}
