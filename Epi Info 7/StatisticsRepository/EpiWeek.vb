Public Class EpiWeek
    ' Program code and logic by David Nitschke
    ' Updated by Erik Knudsen

    Private Function GetMMWRStart(dteDateIn As Date)

        '   GetMMWRStart returns the date of the start of the MMWR year closest to Jan 01
        '   of the year passed in.  It finds 01/01/yyyy first then moves forward or back
        '   the correct number of days to be the start of the MMWR year.  MMWR Week #1 is 
        '   always the first week of the year that has a minimum of 4 days in the new year.
        '   If Jan. first falls on a Thurs, Fri, or Sat, the MMWRStart date returned could be
        '   greater than the date passed in so this must be checked for by the calling Sub.

        '   If Jan. first is a Mon, Tues, or Wed, the MMWRStart goes back to the last
        '   Sunday in Dec of the previous year which is the start of MMWR Week 1 for the
        '   current year.

        '   If the first of January is a Thurs, Fri, or Sat, the MMWRStart goes forward to 
        '   the first Sunday in Jan of the current year which is the start of
        '   MMWR Week 1 for the current year.  For example, if the year passed
        '   in was 01/02/1998, a Friday, the MMWRStart that is returned is 01/04/1998, a Sunday.  
        '   Since 01/04/1998 > 01/02/1998, we must subract a year and pass Jan 1 of the new
        '   year into this function as in GetMMWRStart("01/01/1997").
        '   The MMWRStart date would then be returned as the date of the first
        '   MMWR Week of the previous year.    

        Dim dteYrBegin As Date
        Dim dblDayOfWeek As Double
        dteYrBegin = CDate("01/01/" & CStr(Year(dteDateIn)))
        dblDayOfWeek = Weekday(dteYrBegin)
        If dblDayOfWeek <= vbWednesday Then
            GetMMWRStart = DateAdd("d", -(dblDayOfWeek - 1), dteYrBegin)
        Else
            GetMMWRStart = DateAdd("d", ((7 - dblDayOfWeek) + 1), dteYrBegin)
        End If
    End Function


    Function GetEpiWeek(InputDate As Nullable(Of Date)) As Integer
        Dim strAnswer As String
        Dim dteStart As Date
        Dim lngYear As Long
        Dim strYear As String
        Dim dteQDate As Date
        Dim dteQAccept As Date
        Dim dteWkStart As Date
        Dim dteWkEnd As Date
        Dim dteEndOfQYr As Date
        Dim intMmwrWk As Integer
        Dim intMmwrNow As Integer
        Dim intMmwrMax As Integer
        Dim intEndOfYrDay As Integer

        dteQDate = InputDate

        ' The following lines of code make sure that if a NULL (blank) date is passed into this function
        ' from Epi Info, that we don't cause an error to appear in Epi Info. Instead, we return a null
        ' value and exit the function.
        If InputDate Is Nothing Then
            GetEpiWeek = Nothing
            Exit Function
        End If

        dteQAccept = dteQDate

        ' get the year
        lngYear = Year(dteQAccept)

        ' convert the year to a string
        strYear = CStr(lngYear)

        Dim sdp As String
        sdp = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern

        If sdp.ToLower().StartsWith("d") Then
            dteEndOfQYr = CDate("31/12/" & strYear)
        Else
            dteEndOfQYr = CDate("12/31/" & strYear)
        End If


        intEndOfYrDay = Weekday(dteEndOfQYr)

        If intEndOfYrDay < vbWednesday Then
            If (DateDiff("d", dteQAccept, dteEndOfQYr) < intEndOfYrDay) Then
                dteQAccept = CDate("01/01/" & CStr(lngYear + 1))
            End If
        End If

        dteStart = GetMMWRStart(dteQAccept)
        If dteStart > dteQAccept Then
            dteStart = GetMMWRStart(CDate("01/01/" & CStr(lngYear - 1)))
        End If
        intMmwrWk = 1 + DateDiff("w", dteStart, dteQAccept)
        strAnswer = CStr(intMmwrWk)
        If Len(strAnswer) < 2 Then strAnswer = "0" & strAnswer

        GetEpiWeek = CInt(strAnswer)
    End Function



    Function GetEpiYearWeek(InputDate As Nullable(Of Date))
        Dim strAnswer
        Dim dteStart
        Dim lngYear
        Dim strYear
        Dim dteQDate
        Dim dteQAccept
        Dim dteWkStart
        Dim dteWkEnd
        Dim intMmwrWk
        Dim intMmwrNow
        Dim intMmwrMax

        ' The following lines of code make sure that if a NULL (blank) date is passed into this function
        ' from Epi Info, that we don't cause an error to appear in Epi Info. Instead, we return a null
        ' value and exit the function.
        If InputDate Is Nothing Then
            GetEpiYearWeek = Nothing
            Exit Function
        End If

        dteQDate = InputDate

        strAnswer = GetEpiWeek(dteQDate)

        lngYear = Year(dteQDate)
        strYear = CStr(lngYear)
        If Len(strAnswer) < 2 Then strAnswer = "0" & strAnswer

        ' the following two IF statements check to see if the year doesn't logically match the week number,
        ' and if so, does the appropriate modifications.
        If CInt(strAnswer) >= 52 And Month(dteQDate) = 1 Then
            strYear = CStr(lngYear - 1)
        End If

        If CInt(strAnswer) = 1 And Month(dteQDate) = 12 Then
            strYear = CStr(lngYear + 1)
        End If

        ' format the answer to match the EpiYearWeek function from the EIEpiWk.DLL file
        strAnswer = strYear & ":" & strAnswer

        GetEpiYearWeek = strAnswer

    End Function


End Class
