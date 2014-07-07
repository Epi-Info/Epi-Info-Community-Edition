Option Strict Off
Option Explicit On
Option Compare Text
Public Class ComplexSampleMeans
    Implements EpiInfo.Plugin.IAnalysisStatistic

    Private strataVar As String
    Private mainVar As String
    Private crosstabVar As String
    Private domainVal As String
    Private psuVar As String
    Private weightVar As String
    Private columnNames As List(Of String)
    Private domain1 As String
    Private domain2 As String

    Private validCases As Int32

    Private tableName As String
    Private booleanLabels As String
    Private outputLevel As Int32
    Private percents As Boolean
    Private booleanValues As Int32

    Private sortedTable As DataTable
    Private distinctTable As DataTable

    Private mis As Int32
    Private first As CSMeansTotal
    Private last As CSMeansTotal
    Private com As Boolean

    Private row As Int32

    Private outcome As CSField
    Private domain As CSField
    Private strata As CSField
    Private psu As CSField
    Private weight As CSField
    Private crossTab As CSField

    Private varT As Object
    Private csOutputBuffer As String
    Private cnOutputLevel As Short
    Private cbIncludePercents As Boolean
    Private cbStandalone As Boolean

    Private isDeleted As Boolean
    Private isVerified As Boolean

    Private varianceMultiplier As Double
    Private errorMessage As String
    Private numErrors As Short

    Private meansResults As CSMeansResults

    Private currentTable As DataTable

    Public Structure MeansRow
        Public Label As String
        Public Count As Nullable(Of Double)
        Public Mean As Nullable(Of Double)
        Public StdErr As Nullable(Of Double)
        Public LCL As Nullable(Of Double)
        Public UCL As Nullable(Of Double)
        Public Min As Nullable(Of Decimal)
        Public Max As Nullable(Of Decimal)
    End Structure

    Public Structure CSMeansResults
        Public Rows As List(Of MeansRow)
        Public ErrorMessage As String
    End Structure

    Private Const ErrStart As Integer = &H3000
    Private context As EpiInfo.Plugin.IAnalysisStatisticContext

    Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
    End Sub

    Public Function ComplexSampleMeans(ByVal inputVariableList As Dictionary(Of String, String), ByVal dataTable As DataTable) As CSMeansResults

        currentTable = dataTable

        CreateSettings(inputVariableList)

        numErrors = 0
        errorMessage = String.Empty

        On Error GoTo ERROR_PROC

        meansResults.ErrorMessage = String.Empty

        If Init() = False Then
            Throw New ApplicationException("There was a problem initializing the statistics.")
            Exit Function
        End If

        mis = 0
        first = NewTot("TOTAL")
        last = first

        first.NextTotal = Nothing
        Dim result As Integer

        If com Then

            GetNextRow()
            If Domain1 < Domain2 Then
                first.NextTotal = NewTot(Domain1)
                last = NewTot(Domain2)
            Else
                first.NextTotal = NewTot(Domain2)
                last = NewTot(Domain1)
            End If
            first.NextTotal.NextTotal = last
            ResetReader()

        End If

        result = FirstPass()

        If Not String.IsNullOrEmpty(errorMessage) Or numErrors > 0 Then
            Throw New ApplicationException(errorMessage)
            Exit Function
        End If

        If Not first.NextTotal Is Nothing Then

            If (first.NextTotal.NextTotal Is last) Then
                com = True
            End If

        End If

        ResetReader()

        If result = MsgBoxResult.Ok Then
            errorMessage = String.Empty
            result = SecondPass()

            If Not String.IsNullOrEmpty(errorMessage) Or numErrors > 0 Then
                Throw New ApplicationException(errorMessage)
                Exit Function
            End If
        End If

        If result = MsgBoxResult.Ok Then

            If cnOutputLevel > 0 Then
                errorMessage = String.Empty
                PrintValues(errorMessage)
                If Not String.IsNullOrEmpty(errorMessage) Or numErrors > 0 Then
                    Throw New ApplicationException(errorMessage)
                    Exit Function
                End If
            End If

            '
            ' Removed the resetting of First and Last as this
            ' affected the ResultsArray function.  First and Last
            ' are cleaned up when the class terminates.
            '


        End If

        Return meansResults
ERROR_PROC:
        numErrors = numErrors + 1
        errorMessage = Err.Description
        If Not String.IsNullOrEmpty(errorMessage) Or numErrors > 0 Then
            Throw New ApplicationException(errorMessage)
            Exit Function
        End If

        Exit Function
        Resume
    End Function

    Private Sub CreateSettings(ByVal inputVariableList As Dictionary(Of String, String))

        On Error GoTo Errorhandler

        com = False
        outputLevel = 3
        booleanLabels = "Yes;No;Missing"
        percents = True
        booleanValues = False
        domain1 = String.Empty
        domain2 = String.Empty

        For Each kvp As KeyValuePair(Of String, String) In inputVariableList
            If kvp.Key.ToLower().Equals("percents") Then
                percents = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("stratavar") Then
                strataVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("stratvarlist") Then
                strataVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("numeric_variable") Or kvp.Key.ToLower().Equals("mainvar") Or kvp.Key.ToLower().Equals("identifier") Then
                mainVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("psuvar") Then
                psuVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("cross_tabulation_variable") Or kvp.Key.ToLower().Equals("crosstabvar") Or kvp.Key.ToLower().Equals("identifier2") Then
                crosstabVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("weightvar") Then
                weightVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("tablename") Then
                tableName = kvp.Value
            End If
        Next

        cnOutputLevel = 3 'outputLevel
        cbIncludePercents = percents

        If String.IsNullOrEmpty(psuVar) Then
            errorMessage = "PSU variable is missing"
            numErrors = numErrors + 1
        End If

        If String.IsNullOrEmpty(mainVar) Then
            errorMessage = "Main variable is missing"
            numErrors = numErrors + 1
        End If

cleanup:
        Exit Sub

Errorhandler:
        errorMessage = Err.Description
        numErrors = numErrors + 1
    End Sub

    ''' <summary>
    ''' Checks for errors, and if any are found, returns true. Returns false if no errors are found.
    ''' </summary>
    ''' <param name="args">Display parameters</param>
    ''' <returns>Boolean (true for errors found, false for no errors)</returns>
    ''' <remarks></remarks>
    Public Function CheckForErrors(ByRef args As Dictionary(Of String, String)) As Boolean

        If Not String.IsNullOrEmpty(errorMessage) Or numErrors > 0 Then

            Dim output As String
            output = String.Empty

            ReDim Results(1, 0)
            Results(0, 0) = "ERROR"
            Results(1, 0) = errorMessage
            output = "<br clear=""all"" /><p align=""left""><b><tlt>" + errorMessage + "</tlt></b></p>"

            args.Add("COMMANDNAME", "MEANS")
            args.Add("COMMANDTEXT", context.InputVariableList("commandText"))
            args.Add("HTMLRESULTS", output)
            context.Display(args)
            Return True
        Else
            Return False
        End If

    End Function

    Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute

        numErrors = 0
        errorMessage = String.Empty

        On Error GoTo ERROR_PROC

        CreateSettingsFromContext()

        Dim output As String

        Dim args As Dictionary(Of String, String)
        args = New Dictionary(Of String, String)

        If Init() = False And CheckForErrors(args) = True Then
            Exit Sub
        End If

        mis = 0
        first = NewTot("TOTAL")
        last = first

        first.NextTotal = Nothing
        Dim result As Integer

        If com Then

            GetNextRow()
            If Domain1 < Domain2 Then
                first.NextTotal = NewTot(Domain1)
                last = NewTot(Domain2)
            Else
                first.NextTotal = NewTot(Domain2)
                last = NewTot(Domain1)
            End If
            first.NextTotal.NextTotal = last
            ResetReader()

        End If

        result = FirstPass()

        If CheckForErrors(args) = True Then
            Exit Sub
        End If

            If Not first.NextTotal Is Nothing Then

                If (first.NextTotal.NextTotal Is last) Then
                    com = True
                End If

            End If

            ResetReader()

        If result = MsgBoxResult.Ok Then
            errorMessage = String.Empty
            result = SecondPass()

            If CheckForErrors(args) = True Then
                Exit Sub
            End If
        End If

        If result = MsgBoxResult.Ok Then

            If cnOutputLevel > 0 Then
                errorMessage = String.Empty
                PrintValues(errorMessage)
                If CheckForErrors(args) = True Then
                    Exit Sub
                End If
            End If

            '
            ' Removed the resetting of First and Last as this
            ' affected the ResultsArray function.  First and Last
            ' are cleaned up when the class terminates.
            '

        End If


        output = csOutputBuffer

        args.Add("COMMANDNAME", "MEANS")

        args.Add("COMMANDTEXT", context.InputVariableList("commandText"))
        args.Add("HTMLRESULTS", output)

        context.Display(args)

        Exit Sub
ERROR_PROC:
        numErrors = numErrors + 1
        errorMessage = Err.Description
        CheckForErrors(args)

        Exit Sub
        Resume
    End Sub

    Public Sub AddToOutput(ByRef sText As String)

        csOutputBuffer = csOutputBuffer & sText & vbCrLf

    End Sub

    Public Sub PrintValues(ByRef errorMessage As String)

        Dim Ptr As CSMeansTotal
        Dim Lo As Object
        Dim Up As Object
        Dim Diff As Object
        Dim i As Integer
        Dim sOutline As String
        Dim nOutfile As Integer

        On Error GoTo ErrorHandler

        csOutputBuffer = ""

        If cnOutputLevel > 0 Then


            If cbStandalone Then

                AddToOutput("<html>")
                AddToOutput("<head>")
                AddToOutput("<title>" & "COMPLEX SAMPLE DESIGN ANALYSIS" & "</title>")
                AddToOutput("</head>")
                AddToOutput("<center>")
                AddToOutput("<h1>" & "COMPLEX SAMPLE DESIGN ANALYSIS" & "</h1>")
                AddToOutput("</center>")
                AddToOutput("<br/>")

                If (Not domain Is Nothing) Then
                    AddToOutput("<h4><tlt>Analysis of</tlt> " & outcome.FieldLabel & " : " & domain.FieldLabel & "</h4>")
                Else
                    AddToOutput("<h4><tlt>Analysis of</tlt> " & outcome.FieldLabel & "</h4>")
                End If
                AddToOutput("<hr />")
                AddToOutput("<br />")

            End If

            '
            ' Create the main means result table
            '
            AddToOutput("<table border=""1"">")

            AddToOutput("<TR><TH>&nbsp;</TH>")
            If cnOutputLevel < 2 Then
                AddToOutput("<TH>")
            ElseIf cnOutputLevel = 2 Then
                AddToOutput("<TH COLSPAN=6>")
            Else
                AddToOutput("<TH COLSPAN=7>")
            End If
            AddToOutput(outcome.FieldLabel & "</TH></TR>")

            AddToOutput("<TR>")
            If cnOutputLevel > 1 And cbIncludePercents Then
                AddToOutput("<TH ROWSPAN=""2"">")
            Else
                AddToOutput("<TH>")
            End If
            If (Not domain Is Nothing) Then
                AddToOutput((domain.FieldLabel))
            Else
                AddToOutput("&nbsp;")
                '            AddToOutput Outcome.FieldLabel
            End If
            AddToOutput("</TH>")

            If cnOutputLevel > 1 And cbIncludePercents Then
                AddToOutput("<TH ROWSPAN=""2""><TLT>Count</TLT></TH>")
            ElseIf cnOutputLevel > 1 Then
                AddToOutput("<TH><TLT>Count</TLT></TH>")
            End If

            If cnOutputLevel > 1 And cbIncludePercents Then
                AddToOutput("<TH ROWSPAN=""2""><TLT>Mean</TLT></TH>")
            ElseIf cnOutputLevel > 0 Then
                AddToOutput("<TH><TLT>Mean</TLT></TH>")
            End If

            If cnOutputLevel > 2 And cbIncludePercents Then
                AddToOutput("<TH ROWSPAN=""2""><TLT>Std Error</TLT></TH>")
            ElseIf cnOutputLevel > 2 Then
                AddToOutput("<TH><TLT>Std Error</TLT></TH>")
            End If

            If cnOutputLevel > 1 And cbIncludePercents Then
                AddToOutput("<TH ALIGN=""right"" ROWSPAN=""1"" COLSPAN=""2""><TLT>Confidence Limits</TLT></TH>")
            End If

            If cnOutputLevel > 1 And cbIncludePercents Then
                AddToOutput("<TH ROWSPAN=""2""><TLT>Minimum</TLT></TH>")
                AddToOutput("<TH ROWSPAN=""2""><TLT>Maximum</TLT></TH>")
            ElseIf cnOutputLevel > 1 Then
                AddToOutput("<TH><TLT>Minimum</TLT></TH>")
                AddToOutput("<TH><TLT>Maximum</TLT></TH>")
            End If

            AddToOutput("</TR>")

            If cnOutputLevel > 1 And cbIncludePercents Then
                AddToOutput("<TR>")
                AddToOutput("<TH><TLT>Lower</TLT></TH>")
                AddToOutput("<TH><TLT>Upper</TLT></TH>")
                AddToOutput("</TR>")
            End If


            If (Not domain Is Nothing) Then
                Ptr = first.NextTotal
            Else
                Ptr = first
            End If

            While Not Ptr Is Nothing

                Dim mRow As MeansRow

                '
                ' Beginning of table row.
                '
                AddToOutput("<TR>")

                '
                ' Get the domain name.
                '
                AddToOutput("<TD ALIGN=""right""><B>")
                AddToOutput((Ptr.Domain))
                AddToOutput("</B></TD>")
                mRow.Label = Ptr.Domain

                '
                ' Get the count (observations).
                '
                If cnOutputLevel > 1 Then
                    AddToOutput("<TD ALIGN=""right"">")
                    AddToOutput(Str(Ptr.N))
                    mRow.Count = Convert.ToDouble(Ptr.N)
                    AddToOutput("</TD>")
                End If

                '
                ' Get the mean value.
                '
                If cnOutputLevel > 0 Then
                    AddToOutput("<TD ALIGN=""right"">")
                    If Ptr.SumW > 0 Then
                        AddToOutput(VB6.Format(Ptr.YE / Ptr.SumW, "0.000"))
                        mRow.Mean = Ptr.YE / Ptr.SumW
                    Else
                        AddToOutput("----")
                        mRow.Mean = Nothing
                    End If
                    AddToOutput("</TD>")
                End If

                If cnOutputLevel > 2 Then

                    '
                    ' Get the standard error value.
                    '
                    AddToOutput("<TD ALIGN=""right"">")
                    If Ptr.VarT > 0 Then
                        'AddToOutput(VB6.Format(SquareRootOf((Ptr.VarT)), "0.000"))
                        AddToOutput(VB6.Format(Math.Sqrt(Ptr.VarT), "0.000"))
                        mRow.StdErr = Math.Sqrt(Ptr.VarT)
                    Else
                        AddToOutput("----")
                        mRow.StdErr = Nothing
                    End If
                    AddToOutput("</TD>")

                End If

                If cnOutputLevel > 1 And cbIncludePercents Then

                    '
                    ' Get the upper and lower confidence limit values.
                    '
                    If (Ptr.SumW > 0) And (Ptr.VarT > 0) Then

                        AddToOutput("<TD ALIGN=""right"">")
                        Lo = (Ptr.YE / Ptr.SumW) - (varianceMultiplier * Math.Sqrt(Ptr.VarT))
                        mRow.LCL = Lo
                        AddToOutput(VB6.Format(Lo, "0.000"))
                        AddToOutput("</TD>")

                        AddToOutput("<TD ALIGN=""right"">")
                        Up = (Ptr.YE / Ptr.SumW) + (varianceMultiplier * Math.Sqrt(Ptr.VarT))
                        mRow.UCL = Up
                        AddToOutput(VB6.Format(Up, "0.000"))
                        AddToOutput("</TD>")

                    Else

                        AddToOutput("<TD ALIGN=""right"">")
                        AddToOutput("----")
                        AddToOutput("</TD>")
                        AddToOutput("<TD ALIGN=""right"">")
                        AddToOutput("----")
                        AddToOutput("</TD>")

                    End If

                End If

                If cnOutputLevel > 1 Then
                    AddToOutput("<TD ALIGN=""right"">")
                    AddToOutput(VB6.Format(Ptr.Min, "0.000"))
                    mRow.Min = Ptr.Min
                    AddToOutput("</TD>")
                    AddToOutput("<TD ALIGN=""right"">")
                    AddToOutput(VB6.Format(Ptr.Max, "0.000"))
                    mRow.Max = Ptr.Max
                    AddToOutput("</TD>")
                End If

                If Ptr Is first Then
                    'UPGRADE_NOTE: Object Ptr may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
                    Ptr = Nothing
                Else
                    Ptr = Ptr.NextTotal
                    If Ptr Is Nothing Then
                        Ptr = first
                    End If
                End If

                AddToOutput("</TR>")

                meansResults.Rows.Add(mRow)

            End While


            If com And cnOutputLevel > 2 Then

                Dim dRow As MeansRow

                AddToOutput("<TR>")

                AddToOutput("<TD ALIGN=""right""><B><TLT>Difference</TLT></B></TD>")
                dRow.Label = "Difference"

                '
                ' Blank cell in table.
                '
                AddToOutput("<TD ALIGN=""right"">&nbsp;</TD>")

                AddToOutput("<TD ALIGN=""right"">")
                If (first.NextTotal.SumW > 0) And (last.SumW > 0) Then
                    Diff = (first.NextTotal.YE / first.NextTotal.SumW) - (last.YE / last.SumW)
                    AddToOutput(VB6.Format(Diff, "0.000"))
                    dRow.Mean = Diff
                Else
                    AddToOutput("----")
                    dRow.Mean = Nothing
                End If
                AddToOutput("</TD>")

                AddToOutput("<TD ALIGN=""right"">")
                If (first.NextTotal.SumW > 0) And (last.SumW > 0) And (varT > 0) Then
                    AddToOutput(VB6.Format(Math.Sqrt(varT), "0.000"))
                    dRow.StdErr = Math.Sqrt(varT)
                Else
                    AddToOutput("----")
                    dRow.StdErr = Nothing
                End If
                AddToOutput("</TD>")

                If (first.NextTotal.SumW > 0) And (last.SumW > 0) And (varT > 0) Then

                    AddToOutput("<TD ALIGN=""right"">")
                    Lo = (Diff) - (varianceMultiplier * Math.Sqrt(varT))
                    dRow.LCL = (Diff) - (varianceMultiplier * Math.Sqrt(varT))
                    AddToOutput(VB6.Format(Lo, "0.000"))
                    AddToOutput("</TD>")
                    AddToOutput("<TD ALIGN=""right"">")
                    Up = (Diff) + (varianceMultiplier * Math.Sqrt(varT))
                    dRow.UCL = (Diff) + (varianceMultiplier * Math.Sqrt(varT))
                    AddToOutput(VB6.Format(Up, "0.000"))
                    AddToOutput("</TD>")

                Else

                    AddToOutput("<TD ALIGN=""right"">")
                    AddToOutput("----")
                    AddToOutput("</TD>")
                    AddToOutput("<TD ALIGN=""right"">")
                    AddToOutput("----")
                    AddToOutput("</TD>")

                End If

                '
                ' Two more blank cells in table for Min/Max that do not have differences displayed.
                '
                AddToOutput("<TD ALIGN=""right"">&nbsp;</TD>")
                AddToOutput("<TD ALIGN=""right"">&nbsp;</TD>")

                AddToOutput("</TR>")

                meansResults.Rows.Add(dRow)

            End If

            AddToOutput("</TABLE>")

            AddToOutput("<BR>")
            AddToOutput("<HR>")
            AddToOutput("<BR>")

            PrintDesign()

            If cbStandalone Then AddToOutput("</HTML>")

        End If

        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub PrintDesign()

        On Error GoTo ErrorHandler

        AddToOutput("<H4><TLT>Sample Design Included</TLT></H4>")

        If (Not weight Is Nothing) Then
            AddToOutput("<TLT>Weight Variable</TLT>:  " & Trim(weight.FieldLabel) & "<BR>")
        Else
            AddToOutput("<TLT>Weight Variable</TLT>:  <TLT>None</TLT>" & "<BR>")
        End If
        If (Not psu Is Nothing) Then
            AddToOutput("<TLT>PSU Variable</TLT>:  " & Trim(psu.FieldLabel) & "<BR>")
        Else
            AddToOutput("<TLT>PSU Variable</TLT>:  <TLT>None</TLT>" & "<BR>")
        End If
        If (Not strata Is Nothing) Then
            AddToOutput("<TLT>Stratification Variable</TLT>:  " & Trim(strata.FieldLabel) & "<BR>")
        Else
            AddToOutput("<TLT>Stratification Variable</TLT>:  <TLT>None</TLT>" & "<BR>")
        End If

        AddToOutput("<BR>")
        AddToOutput("<TLT>Records with missing values:</TLT>" & " " & mis & "<BR>")
        AddToOutput("<BR>")

        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Complex Sample Means"
        End Get
    End Property

    Public ReadOnly Property ResultArray() As Object
        Get
            ResultArray = VB6.CopyArray(EIRegressGlobals.Results)
        End Get
    End Property

    Private Sub CreateSettingsFromContext()

        On Error GoTo Errorhandler

        com = False
        outputLevel = 3
        booleanLabels = "Yes;No;Missing"
        percents = True
        booleanValues = False
        domain1 = String.Empty
        domain2 = String.Empty

        If context.SetProperties.ContainsKey("TableName") Then
            tableName = context.SetProperties("TableName")
        End If

        If context.SetProperties.ContainsKey("BLabels") Then
            booleanLabels = context.SetProperties("BLabels")
        End If

        If context.SetProperties.ContainsKey("IsBoolean") Then
            booleanValues = context.SetProperties("IsBoolean")
        End If

        For Each kvp As KeyValuePair(Of String, String) In context.InputVariableList
            If kvp.Key.ToLower().Equals("percents") Then
                percents = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("stratavar") Then
                strataVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("stratvarlist") Then
                strataVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("numeric_variable") Or kvp.Key.ToLower().Equals("mainvar") Or kvp.Key.ToLower().Equals("identifier") Then
                mainVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("psuvar") Then
                psuVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("cross_tabulation_variable") Or kvp.Key.ToLower().Equals("crosstabvar") Or kvp.Key.ToLower().Equals("identifier2") Then
                crosstabVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("weightvar") Then
                weightVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("tablename") Then
                tableName = kvp.Value
            End If
        Next

        cnOutputLevel = 3 'outputLevel
        cbIncludePercents = percents

        If String.IsNullOrEmpty(psuVar) Then
            errorMessage = "PSU variable is missing"
            numErrors = numErrors + 1
        End If

        If String.IsNullOrEmpty(mainVar) Then
            errorMessage = "Main variable is missing"
            numErrors = numErrors + 1
        End If

cleanup:
        Exit Sub

Errorhandler:
        errorMessage = Err.Description
        numErrors = numErrors + 1
    End Sub

    'Get Raw Data
    '---From the settings , will load data into the global data array
    ' Outcome , [MatchVar], [WeightVar], [Covariates]
    Private Function Init() As Boolean

        Dim numRows As Int32
        Dim numCats As Int32
        Dim numStrata As Int32

        Dim isDeleted As Boolean
        Dim isVerified As Boolean
        Dim columnNamesArray() As String

        meansResults = New CSMeansResults()
        meansResults.Rows = New List(Of MeansRow)
        validCases = 0 ' REMOVE

        On Error GoTo ErrorhandlEr

        numRows = -1
        numCats = -1
        numStrata = -1
        columnNames = New List(Of String)
        isDeleted = False
        isVerified = False

        If Not String.IsNullOrEmpty(strataVar) Then
            columnNames.Add(strataVar)
            strata = New CSField()
            strata.FieldLabel = strataVar
        Else
            strata = New CSField()
            strata.FieldLabel = "None" '"STRATA_VAR"
            strata.FieldEntry = 1
        End If

        If Not String.IsNullOrEmpty(weightVar) Then
            columnNames.Add(weightVar)
            weight = New CSField()
            weight.FieldLabel = weightVar
        End If

        If Not String.IsNullOrEmpty(mainVar) Then
            columnNames.Add(mainVar)
            outcome = New CSField()
            outcome.FieldLabel = mainVar
        End If

        If Not String.IsNullOrEmpty(crosstabVar) Then
            columnNames.Add(crosstabVar)
            domain = New CSField()
            domain.FieldLabel = crosstabVar
        End If

        If Not String.IsNullOrEmpty(psuVar) Then
            columnNames.Add(psuVar)
            psu = New CSField()
            psu.FieldLabel = psuVar
        End If

        columnNamesArray = columnNames.ToArray()

        Dim sortClause As String
        sortClause = String.Empty

        If Not String.IsNullOrEmpty(strataVar) Then
            sortClause = strataVar
            If Not String.IsNullOrEmpty(psuVar) Then
                sortClause = sortClause & ", " & psuVar & ""
            End If
        ElseIf Not String.IsNullOrEmpty(psuVar) Then
            sortClause = psuVar
        End If

        row = 0

        Dim whereClause = mainVar + " is not null and " + psuVar + " is not null"
        If Not String.IsNullOrEmpty(weightVar) Then
            whereClause = whereClause + " and " + weightVar + " is not null"
        End If
        If Not String.IsNullOrEmpty(strataVar) Then
            whereClause = whereClause + " and " + strataVar + " is not null"
        End If

        Dim unsortedTable = New DataTable()

        If Not currentTable Is Nothing Then
            unsortedTable = currentTable
        Else
            unsortedTable = context.GetDataRows(Nothing).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)
        End If

        'sortedTable = context.CurrentDataTable.Select(whereClause, sortClause).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)
        sortedTable = unsortedTable.Select(whereClause, sortClause).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)
        numRows = sortedTable.Rows.Count

        If Not String.IsNullOrEmpty(mainVar) Then
            outcome.FieldEntry = sortedTable.Rows(row)(mainVar)
        End If

        If Not String.IsNullOrEmpty(strataVar) Then
            strata.FieldEntry = sortedTable.Rows(row)(strataVar)
        End If

        If Not String.IsNullOrEmpty(weightVar) Then
            weight.FieldEntry = sortedTable.Rows(row)(weightVar)
        End If

        If Not String.IsNullOrEmpty(psuVar) Then
            psu.FieldEntry = sortedTable.Rows(row)(psuVar)
        End If

        If Not String.IsNullOrEmpty(crosstabVar) Then
            domain.FieldEntry = sortedTable.Rows(row)(crosstabVar)
        End If


        On Error GoTo ErrorhandlEr
        If numRows <= 0 Then Err.Raise(vbObjectError, , "<tlt>No Data available to load</tlt>")

        If Not String.IsNullOrEmpty(strataVar) Then
            numCats = sortedTable.DefaultView.ToTable(True, psuVar, strataVar).Rows.Count
            numStrata = sortedTable.DefaultView.ToTable(True, strataVar).Rows.Count
        Else
            Dim psuVarArray() As String
            ReDim psuVarArray(0)
            psuVarArray(0) = psuVar
            numCats = sortedTable.DefaultView.ToTable("Output", True, psuVar).Rows.Count
            numStrata = 1
        End If

        ' TODO: Fix this problem at a later point in time. Apparently, we should be allowing CSMeans to proceed even
        ' if only PSU value is present. However, allowing it to proceed in that scenario results in an infinite loop
        ' later on. This fix is considered temporary.
        If sortedTable.DefaultView.ToTable("Output", True, psuVar).Rows.Count = 1 Then
            Err.Raise(vbObjectError, , "<tlt>Only one PSU value is present. Aborting.</tlt>")
        End If

        '#If USETFROMP Then
        If numCats <= 1 Then
            varianceMultiplier = 1.96
        Else
            Dim dist As New statlib
            dist = New statlib()
            varianceMultiplier = dist.TfromP(0.95, numCats - numStrata)
        End If
        '#Else
        '        'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression Else did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
        '        varianceMultiplier = 1.96
        '#End If

        Init = True

cleanup:
        Exit Function

Errorhandler:
        errorMessage = Err.Description
        numErrors = numErrors + 1
        'Err.Raise(vbObjectError + 234, , Err.Description)
        Init = False

    End Function

    Public Function FirstPass() As Integer

        Dim P As CSMeansTotal

        On Error GoTo ErrorHandler


        FirstPass = MsgBoxResult.No
        Do While (GetNextRow())

            If ValidCase() And Not isDeleted Then 'If ValidCase Then
                If (Not domain Is Nothing) Then
                    P = FindTotal((domain.FieldEntry))
                    If com Then
                        If Not P Is Nothing Then
                            AccumYE(P)
                            AccumYE(first)
                        End If
                    Else
                        If P Is Nothing Then
                            AddTot((domain.FieldEntry))
                        Else
                            AccumYE(P)
                        End If
                        AccumYE(first)
                    End If
                Else
                    AccumYE(first)
                End If
            Else
                mis = mis + 1
            End If

        Loop

        FirstPass = MsgBoxResult.Ok

        Exit Function

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1

    End Function

    Public Sub AddTot(ByRef dom As Object)

        Dim Ptr As CSMeansTotal
        Dim P As CSMeansTotal
        Dim inserted As Boolean

        On Error GoTo ErrorHandler

        inserted = False
        Ptr = NewTot(dom)
        AccumYE(Ptr)
        If first.NextTotal Is Nothing Then
            first.NextTotal = Ptr
            last = Ptr
        Else
            P = first.NextTotal
            If P.Domain > dom Then
                Ptr.NextTotal = P
                first.NextTotal = Ptr
            Else
                While (Not P.NextTotal Is Nothing) And (Not inserted)
                    If P.NextTotal.Domain > dom Then
                        Ptr.NextTotal = P.NextTotal
                        P.NextTotal = Ptr
                        inserted = True
                    Else
                        P = P.NextTotal
                    End If
                End While
                If Not inserted Then
                    last.NextTotal = Ptr
                    last = Ptr
                End If
            End If
        End If

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function FindTotal(ByRef dom As Object) As CSMeansTotal

        Const PROC_Name As String = "clsCMeans::FindTotal"

        Dim Ptr As CSMeansTotal
        Dim found As Boolean

        On Error GoTo ErrorHandler

        Ptr = first
        found = False
        While Not Ptr Is Nothing And Not found
            If Ptr.Domain = dom.ToString() Then
                found = True
            Else
                Ptr = Ptr.NextTotal
            End If
        End While

        FindTotal = Ptr

        Exit Function

ErrorHandler:

        'UPGRADE_NOTE: Object FindTotal may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        FindTotal = Nothing
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub AccumYE(ByRef P As CSMeansTotal)

        Const PROC_Name As String = "clsCMeans::AccumYE"

        Dim Value As Object

        On Error GoTo ErrorHandler

        Value = outcome.FieldReal
        P.YE = P.YE + Value * GetWeight()
        P.SumW = P.SumW + GetWeight()
        P.N = P.N + 1
        If P.N = 1 Then '   Change to recognize first value -- RF 12/13/02
            P.Min = Value
            P.Max = Value
        Else
            If P.Min > Value Then
                P.Min = Value
            ElseIf P.Max < Value Then
                P.Max = Value
            End If
        End If


        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function NewTot(ByRef dom As Object) As CSMeansTotal

        Dim Ptr As CSMeansTotal

        On Error GoTo ErrorHandler

        Ptr = New CSMeansTotal
        Ptr.Domain = dom
        Ptr.YE = 0.0#
        Ptr.SumW = 0.0#
        Ptr.N = 0
        Ptr.Min = outcome.FieldReal
        Ptr.Max = Ptr.Min
        Ptr.NextTotal = Nothing
        NewTot = Ptr

        Exit Function

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Private Sub ResetReader()

        row = 0

    End Sub

    Public Function SecondPass() As Integer

        Dim P As CSMeansTotal
        Dim Valid As Boolean
        Dim ah As Integer
        Dim Rec As Boolean 'Integer
        Dim NowStrat As Object
        Dim NowPSU As Object
        Dim qha As Object
        Dim qha2 As Object
        Dim Sumqha As Object
        Dim Sumqha2 As Object
        Dim bContinue As Boolean
        Dim bHadValidPSU As Boolean

        On Error GoTo ErrorHandler

        SecondPass = MsgBoxResult.No
        Rec = True
        VarTInit()
        varT = 0
        Valid = False
        NowStrat = ""
        NowPSU = ""
        While Not Valid And Rec = True

            Rec = GetNextRow()
            If Rec = True Then
                Valid = ValidCase()
                If Valid And Not isDeleted Then
                    If (Not strata Is Nothing) Then
                        NowStrat = strata.FieldEntry
                    End If
                    If (Not psu Is Nothing) Then
                        NowPSU = psu.FieldEntry
                    End If
                End If
            End If

        End While

        Do

            SumqInit()
            Sumqha = 0.0#
            Sumqha2 = 0.0#
            ah = 0

            Do

                qha = 0.0#
                qha2 = 0.0#
                QhaInit()
                bHadValidPSU = False

                Do

                    If ValidCase() And Not isDeleted Then 'ValidCase Then

                        bHadValidPSU = True

                        If Not com Then
                            If (Not domain Is Nothing) Then
                                P = FindTotal((domain.FieldEntry))
                                Accumqha(P)
                            End If
                            Accumqha(first)
                        Else
                            P = FindTotal((domain.FieldEntry))
                            If (P Is first.NextTotal) Then
                                qha = qha + Qhab(P)
                            Else
                                If (P Is last) Then
                                    qha = qha - Qhab(P)
                                End If
                            End If
                            If (Not P Is Nothing) Then
                                Accumqha(first)
                                Accumqha(P)
                            End If
                        End If
                    End If

                    Rec = GetNextRow()

                    If Not psu Is Nothing Then
                        If psu.FieldEntry.ToString() <> NowPSU Then
                            bContinue = True
                        ElseIf strata.FieldEntry.ToString() <> NowStrat Then
                            bContinue = True
                        Else
                            bContinue = False
                        End If
                    Else
                        bContinue = True
                    End If

                Loop Until (Rec = False) Or bContinue 'Loop Until (Rec < 0) Or bContinue

                If (Not psu Is Nothing) Then
                    If (Rec = False) Or (FieldColl(psu, NowPSU) > 0) Then

                        NowPSU = psu.FieldEntry

                    ElseIf Not strata Is Nothing Then

                        If (strata.FieldEntry <> NowStrat) Then
                            NowPSU = psu.FieldEntry
                        Else
                            errorMessage = "File is not sorted!"
                            'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "File is not sorted!")
                        End If

                    Else
                        'Error (-1)
                        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "File is not sorted!")
                        SecondPass = -1
                        errorMessage = "File is not sorted!"
                        Exit Function
                    End If
                End If

                If bHadValidPSU Then '   RF Check This

                    ah = ah + 1
                    AccumSumq()
                    If com Then
                        Sumqha = Sumqha + qha
                        Sumqha2 = Sumqha2 + (qha ^ 2)
                    End If

                End If

                If Not strata Is Nothing Then
                    If strata.FieldEntry.ToString() <> NowStrat Then
                        bContinue = True
                    Else
                        bContinue = False
                    End If
                Else
                    bContinue = True
                End If

            Loop Until (Rec = False) Or bContinue

            If (Not strata Is Nothing) Then
                If (Rec = False) Or (FieldColl(strata, NowStrat) > 0) Then
                    NowStrat = strata.FieldEntry
                Else
                    'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "File is not sorted!")
                    'Error (-1)  ' error(strata, psu)
                    SecondPass = -1
                    errorMessage = "File is not sorted!"
                    numErrors = numErrors + 1
                    Exit Function
                End If
            End If

            AccumVar(ah)
            If (ah > 1) And com Then
                varT = varT + (ah * Sumqha2 - (Sumqha ^ 2)) / (ah - 1)
            End If

        Loop Until (Rec = False)
        SecondPass = MsgBoxResult.Ok

        Exit Function

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub SumqInit()

        Dim Ptr As CSMeansTotal

        On Error GoTo ErrorHandler

        Ptr = first
        While Not Ptr Is Nothing
            Ptr.Sumqha = CDec(0.0#)
            Ptr.Sumqha2 = CDec(0.0#)
            Ptr = Ptr.NextTotal
        End While

        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function ValidCase() As Boolean

        Dim bValid As Boolean

        On Error GoTo ErrorHandler

        bValid = True
        ValidCase = True

        If Not outcome Is Nothing And bValid Then
            If outcome.Missing Then
                ValidCase = False
                bValid = False
            End If
        End If
        If Not strata Is Nothing And bValid Then
            If strata.Missing Then
                ValidCase = False
                bValid = False
            End If
        End If
        If Not psu Is Nothing And bValid Then
            If psu.Missing Then
                ValidCase = False
                bValid = False
            End If
        End If
        If Not weight Is Nothing And bValid Then
            If weight.Missing Then
                ValidCase = False
                bValid = False
            End If
        End If
        If Not domain Is Nothing And bValid Then
            If domain.Missing Then
                ValidCase = False
                bValid = False
            End If
        End If

        If bValid Then
            validCases = validCases + 1
        End If
        Exit Function

ErrorHandler:

        ValidCase = False

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub VarTInit()

        Dim Ptr As CSMeansTotal

        On Error GoTo ErrorHandler

        Ptr = first
        While Not Ptr Is Nothing
            Ptr.VarT = CDec(0.0#)
            Ptr = Ptr.NextTotal
        End While

        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub




    '(*****************************************************************************
    '***  Read the next record Onto the list.  Return _EOF (-1) on error,       ***
    '***  return _DELETED (0) if the record is deleted, return _OK (1) if OK    ***
    '*****************************************************************************)
    Private Function GetNextRow() As Boolean

        If sortedTable.Rows.Count = row Then
            Return False
        End If

        On Error GoTo ErrorHandler

        If Not String.IsNullOrEmpty(strataVar) Then
            strata.FieldEntry = sortedTable.Rows(row)(strataVar) 'reader.Item(strataVar)
            If strata.FieldEntry.ToString().Length <= 0 Then
                strata.Missing = True
            Else
                strata.Missing = False
            End If
        Else
            strata.FieldEntry = 1
            strata.Missing = False
        End If

        If Not String.IsNullOrEmpty(weightVar) Then
            weight.FieldEntry = sortedTable.Rows(row)(weightVar) 'reader.Item(weightVar)
            If weight.FieldEntry.ToString().Length <= 0 Then
                weight.Missing = True
            Else
                weight.Missing = False
            End If
        End If

        If Not String.IsNullOrEmpty(mainVar) Then
            outcome.FieldEntry = sortedTable.Rows(row)(mainVar) 'reader.Item(mainVar)
            If outcome.FieldEntry.ToString().Length <= 0 Then
                outcome.Missing = True
            Else
                outcome.Missing = False
            End If
        End If

        If Not String.IsNullOrEmpty(crosstabVar) Then
            domain.FieldEntry = sortedTable.Rows(row)(crosstabVar) 'reader.Item(crosstabVar)
            If domain.FieldEntry.ToString().Length <= 0 Then
                domain.Missing = True
            Else
                domain.Missing = False
            End If
        End If

        If Not String.IsNullOrEmpty(psuVar) Then
            psu.FieldEntry = sortedTable.Rows(row)(psuVar) 'reader.Item(psuVar)
            If psu.FieldEntry.ToString().Length <= 0 Then
                psu.Missing = True
            Else
                psu.Missing = False
            End If
        End If

        If sortedTable.Columns.Contains("RECSTATUS") Then
            Dim recstatus As Int16
            recstatus = 1
            recstatus = sortedTable.Rows(row)("RECSTATUS")
            If recstatus < 1 Then
                isDeleted = True
            End If
        Else

        End If

        isDeleted = False ' TODO: Check this...
        isVerified = True

        'UPGRADE_WARNING: Couldn't resolve default property of object rsFile.MoveNext. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'reader.Read()
        row = row + 1
        'c_nCurrentRec = c_nCurrentRec + 1

        'GetNext = retOK

        'End If
        Return True

        Exit Function

ErrorHandler:

        'GetNext = retEOF
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)
        errorMessage = Err.Description
        numErrors = numErrors + 1
        Return False

    End Function


    Public Function GetWeight() As Object

        On Error GoTo ErrorHandler

        If (Not weight Is Nothing) Then
            GetWeight = weight.FieldReal
        Else
            GetWeight = CDec(1.0#)
        End If

        Exit Function

ErrorHandler:

        GetWeight = CDec(1.0#)
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function


    Private Sub QhaInit()

        Dim Ptr As CSMeansTotal

        On Error GoTo ErrorHandler

        Ptr = first
        While Not Ptr Is Nothing
            Ptr.qha = CDec(0.0#)
            Ptr.qha2 = CDec(0.0#)
            Ptr = Ptr.NextTotal
        End While

        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub AccumSumq()

        Dim Ptr As CSMeansTotal

        On Error GoTo ErrorHandler

        Ptr = first
        While Not Ptr Is Nothing
            Ptr.Sumqha = Ptr.Sumqha + Ptr.qha
            Ptr.Sumqha2 = Ptr.Sumqha2 + (Ptr.qha ^ 2)
            Ptr = Ptr.NextTotal
        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub AccumVar(ByRef ah As Integer)

        Dim Ptr As CSMeansTotal

        On Error GoTo ErrorHandler

        Ptr = first
        While Not Ptr Is Nothing
            If ah > 1 Then
                Ptr.VarT = Ptr.VarT + (ah * Ptr.Sumqha2 - (Ptr.Sumqha ^ 2)) / (ah - 1)
            Else
                Ptr.VarT = -9999999.0#
            End If
            Ptr = Ptr.NextTotal
        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub Accumqha(ByRef P As CSMeansTotal)

        Dim Qhab As Object

        On Error GoTo ErrorHandler

        If P.SumW > 0 Then
            Qhab = CDec((outcome.FieldReal) - (P.YE / P.SumW)) * (GetWeight() / P.SumW)
        Else
            Qhab = CDec(0.0#)
        End If

        P.qha = P.qha + Qhab

        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        numErrors = numErrors + 1
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function Qhab(ByRef P As CSMeansTotal) As Object

        If P.SumW > 0 Then
            Qhab = CDec((outcome.FieldReal) - (P.YE / P.SumW)) * (GetWeight() / P.SumW)
        Else
            Qhab = CDec(0.0#)
        End If

    End Function


    Public Function FieldColl(ByRef p1 As CSField, ByRef s As Object) As Short

        '   Dim ft As ADODB.DataTypeEnum
        Dim ft As Integer
        Dim i As Integer
        Dim R As Double
        Dim R2 As Double

        On Error GoTo ErrorHandler

        FieldColl = 0

        'ft = p1.FieldType

        If IsNumeric(p1.FieldEntry) Then

            If (Val(CDec(p1.FieldEntry)) Mod 1 = 0) Then

                i = Int(Val(CStr(s)))
                FieldColl = p1.FieldInt - i

            Else

                R = Val(CStr(s))
                R2 = p1.FieldReal
                If (R2 > R) Then
                    FieldColl = 1
                ElseIf (R2 < R) Then
                    FieldColl = -1
                Else
                    FieldColl = 0
                End If

            End If

        Else

            If p1.FieldEntry > s Then
                FieldColl = 1
            ElseIf p1.FieldEntry < s Then
                FieldColl = -1
            ElseIf p1.FieldEntry = s Then
                FieldColl = 0
            End If

        End If

        Exit Function

ErrorHandler:

        FieldColl = 0
        MsgBox("Error #" & Err.Number & " -- " & Err.Description)

        errorMessage = Err.Description
        numErrors = numErrors + 1

    End Function

End Class