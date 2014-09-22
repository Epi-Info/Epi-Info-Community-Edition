Option Strict Off
Option Explicit On
Option Compare Text
<System.Runtime.InteropServices.ProgId("EILSFit_NET.EILSFit")> Public Class LogisticRegression
    Implements EpiInfo.Plugin.IAnalysisStatistic

    Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)

	Private WithEvents mMatrixlikelihood As EIMatrix

    Private Const ErrStart As Integer = &H3000
    Private Const conConnStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
    Private context As EpiInfo.Plugin.IAnalysisStatisticContext
    Private currentTable As DataTable

    Public Structure VariableRow
        Public variableName As String
        Public oddsRatio As Double
        Public ninetyFivePercent As Double
        Public ci As Double
        Public coefficient As Double
        Public se As Double
        Public Z As Double
        Public P As Double
    End Structure

    Public Structure InteractionRow
        Public interactionName As String
        Public oddsRatio As String
        Public ninetyFivePercent As String
        Public ci As String
    End Structure

    Public Structure LogisticRegressionResults
        Public variables As List(Of VariableRow)
        Public interactionOddsRatios As List(Of InteractionRow)
        Public convergence As String
        Public iterations As Integer
        Public finalLikelihood As Double
        Public casesIncluded As Integer
        Public scoreStatistic As Double
        Public scoreDF As Double
        Public scoreP As Double
        Public LRStatistic As Double
        Public LRDF As Double
        Public LRP As Double
        Public errorMessage As String
    End Structure

    Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
    End Sub

    Public Function LogisticRegression(ByVal inputVariableList As Dictionary(Of String, String), ByVal dataTable As DataTable) As LogisticRegressionResults

        currentTable = dataTable

        CreateSettings(inputVariableList)

        Dim logistic As String
        Dim errorMessage As String
        errorMessage = String.Empty

        Dim regressionResults As New LogisticRegressionResults
        regressionResults.errorMessage = String.Empty

        If GetRawData(errorMessage) = False Then
            regressionResults.errorMessage = errorMessage
            Return regressionResults 'Exit Function
        End If

        Dim lintConditional As Integer
        Dim lintweight As Integer
        Dim lstrError As String

        Dim ldblFirstLikelihood As Double
        Dim ldblScore As Double

        lstrError = String.Empty

        mMatrixlikelihood = New EIMatrix()
        If Len(mstrWeightVar) > 0 Then
            lintweight = 1
        End If
        If Len(mstrMatchVar) > 0 Then
            lintConditional = 1
        End If
        mboolFirst = True

        mMatrixlikelihood.MaximizeLikelihood(0 + NumRows, 0 + NumColumns, DataArray, lintweight + lintConditional + 1, NumColumns - (lintweight + lintConditional + 1), 0 + mlngIter, mdblToler, mdblConv, False)

        If mMatrixlikelihood.GetConvergence = True Then
            If (mMatrixlikelihood.getError(lstrError) = False) Then
                logistic = CreateOutputString(mMatrixlikelihood.getFirstLikelihood, mMatrixlikelihood.getLastLikelihood, mMatrixlikelihood.getIters, mMatrixlikelihood.GetCoefficients, mMatrixlikelihood.GetInverseMatrix, mMatrixlikelihood.GetConvergence, mMatrixlikelihood.getScore)
                'TODO: Re-enable later? 'Residuals(1 + lintweight + lintConditional, mMatrixlikelihood.GetCoefficients)
            Else
                '            Err.Raise vbObjectError, , lstrError
                regressionResults.errorMessage = lstrError
                Return regressionResults 'Exit Function
            End If
        Else
            ' if convergence with inital estimate fails, then
            ' try with initial estimate of 0

            ' remember these so we can calculate the score and
            ' the likelihood ratio later...
            ldblFirstLikelihood = mMatrixlikelihood.getFirstLikelihood
            ldblScore = mMatrixlikelihood.getScore

            mMatrixlikelihood = New EIMatrix
            If Len(mstrWeightVar) > 0 Then
                lintweight = 1
            End If
            If Len(mstrMatchVar) > 0 Then
                lintConditional = 1
            End If
            mboolFirst = True

            ' redo regression with inital estimate of 0
            mMatrixlikelihood.MaximizeLikelihood(0 + NumRows, 0 + NumColumns, DataArray, lintweight + lintConditional + 1, NumColumns - (lintweight + lintConditional + 1), 0 + mlngIter, mdblToler, mdblConv, True)

            'Check the error status
            If (mMatrixlikelihood.getError(lstrError) = False) Then
                'output results
                logistic = CreateOutputString(ldblFirstLikelihood, mMatrixlikelihood.getLastLikelihood, mMatrixlikelihood.getIters, mMatrixlikelihood.GetCoefficients, mMatrixlikelihood.GetInverseMatrix, mMatrixlikelihood.GetConvergence, ldblScore)
                'Residuals(1 + lintweight + lintConditional, mMatrixlikelihood.GetCoefficients)
            Else
                regressionResults.errorMessage = lstrError
                Return regressionResults 'Exit Function
            End If

        End If

        'Results(0,0)

        regressionResults.variables = New List(Of VariableRow)

        Dim lvarCoeff(8, UBound(mMatrixlikelihood.GetCoefficients)) As Object
        lvarCoeff = VB6.CopyArray(Results(1, 0))

        For i = 0 To UBound(mMatrixlikelihood.GetCoefficients)

            Dim variableRow As VariableRow
            variableRow.variableName = lvarCoeff(0, i).ToString()
            If lvarCoeff(1, i).ToString().Equals("*") = True Then
                variableRow.oddsRatio = -99999
                variableRow.ninetyFivePercent = -99999
                variableRow.ci = -99999
            Else
                variableRow.oddsRatio = lvarCoeff(1, i)
                variableRow.ninetyFivePercent = lvarCoeff(2, i).ToString()
                variableRow.ci = lvarCoeff(3, i).ToString()
            End If
            variableRow.coefficient = lvarCoeff(4, i).ToString()
            variableRow.se = lvarCoeff(5, i).ToString()
            variableRow.Z = lvarCoeff(6, i).ToString()
            variableRow.P = lvarCoeff(7, i).ToString()

            regressionResults.variables.Add(variableRow)

        Next

        Dim lvarStats(2, 4) As Object
        lvarStats = VB6.CopyArray(Results(1, 1))

        regressionResults.iterations = lvarStats(1, 1)
        regressionResults.convergence = lvarStats(0, 0)
        regressionResults.finalLikelihood = lvarStats(1, 2)
        regressionResults.casesIncluded = lvarStats(1, 3)

        Dim lvarTests(3, 1) As Object
        lvarTests = VB6.CopyArray(Results(1, 2))

        regressionResults.scoreStatistic = lvarTests(1, 0)
        regressionResults.scoreDF = lvarTests(2, 0)
        regressionResults.scoreP = lvarTests(3, 0)

        regressionResults.LRStatistic = lvarTests(1, 1)
        regressionResults.LRDF = lvarTests(2, 1)
        regressionResults.LRP = lvarTests(3, 1)

        Dim interactionindex As Int16
        interactionindex = logistic.IndexOf("Interaction</strong>")
        If (interactionindex > 0) Then
            regressionResults.interactionOddsRatios = New List(Of InteractionRow)
            Dim parsingString As String
            parsingString = logistic.Substring(interactionindex + 20)
            parsingString = parsingString.Substring(parsingString.IndexOf("Confidence Limits</strong>") + 36)
            Dim trindex As Int16
            trindex = parsingString.IndexOf("<tr>")
            While (trindex >= 0)
                Dim interactionRow As InteractionRow
                parsingString = parsingString.Substring(parsingString.IndexOf("<strong>") + 8)
                interactionRow.interactionName = parsingString.Substring(0, parsingString.IndexOf("</strong>"))
                parsingString = parsingString.Substring(parsingString.IndexOf("</td><td") + 39)
                interactionRow.oddsRatio = parsingString.Substring(0, parsingString.IndexOf("</td>"))
                parsingString = parsingString.Substring(parsingString.IndexOf("</td><td") + 38)
                interactionRow.ninetyFivePercent = parsingString.Substring(0, parsingString.IndexOf("</td>"))
                parsingString = parsingString.Substring(parsingString.IndexOf("</td><td") + 38)
                interactionRow.ci = parsingString.Substring(0, parsingString.IndexOf("</td>"))
                regressionResults.interactionOddsRatios.Add(interactionRow)
                trindex = parsingString.IndexOf("<tr>")
            End While
        End If

        Return regressionResults

    End Function

    Private Sub CreateSettings(ByVal inputVariableList As Dictionary(Of String, String))

        Dim dist As New statlib
        dist = New statlib()

        mstrC = "95"
        mdblC = 0.05
        mdblP = dist.ZFROMP(mdblC * 0.5)
        mlngIter = 15
        mdblConv = 0.00001
        mdblToler = 0.00001
        mboolIntercept = True
        ReDim mStrADiscrete(0)
        ReDim DataArray(0, 0)
        ReDim mstraBoolean(2)
        mstraBoolean(0) = "No"
        mstraBoolean(1) = "Yes"
        mstraBoolean(2) = "Missing"
        mstrMatchVar = ""
        mstrWeightVar = ""

        ReDim mstraTerms(0)
        Dim terms As Integer
        Dim discrete As Integer

        terms = 0
        discrete = 0

        For Each kvp As KeyValuePair(Of String, String) In inputVariableList
            If kvp.Value.ToLower().Equals("term") Then
                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1
            End If

            If kvp.Value.ToLower().Equals("discrete") Then
                ReDim Preserve mStrADiscrete(discrete)
                mStrADiscrete(discrete) = kvp.Key
                discrete = discrete + 1

                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1
            End If

            If kvp.Value.ToLower().Equals("matchvar") Then
                mstrMatchVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("weightvar") Then
                mstrWeightVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("dependvar") Then
                mstrDependVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("unsorted") Then
                Dim type As String
                type = currentTable.Columns(kvp.Key).DataType.ToString()
                If Not MoreThanTwoValues(currentTable.Columns(kvp.Key)) Or type.Equals("System.String") Then
                    ReDim Preserve mStrADiscrete(discrete)
                    mStrADiscrete(discrete) = kvp.Key
                    discrete = discrete + 1
                End If
                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1

            End If

            If kvp.Key.ToLower().Equals("intercept") Then
                Dim success As Boolean
                success = Boolean.TryParse(kvp.Value, mboolIntercept) ' TODO: Test
            End If

            If kvp.Key.ToLower().Equals("p") Then
                Dim success As Boolean
                success = Double.TryParse(kvp.Value.ToLower(), mdblP)
                If success = True Then
                    mdblC = 1 - mdblP
                    mstrC = Str(mdblP * 100)
                    mdblP = dist.ZFROMP(mdblC * 0.5)
                End If
            End If
        Next
    End Sub

    Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute

        CreateSettingsFromContext()

        Dim logistic As String
        Dim errorMessage As String
        errorMessage = String.Empty

        Dim args As Dictionary(Of String, String)
        args = New Dictionary(Of String, String)

        If GetRawData(errorMessage) = False Then
            ReDim Results(1, 0)
            Results(0, 0) = "ERROR"
            Results(1, 0) = errorMessage
            logistic = "<br clear=""all"" /><p align=""left""><b><tlt>" + errorMessage + "</tlt></b></p>"

            args.Add("COMMANDNAME", "REGRESS")
            args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
            args.Add("HTMLRESULTS", logistic)
            context.Display(args)
            Exit Sub
        End If

        args.Add("COMMANDNAME", "LOGISTIC")

        args.Add("COMMANDTEXT", context.SetProperties("CommandText"))

        Dim lintConditional As Integer
        Dim lintweight As Integer
        Dim lstrError As String

        Dim ldblFirstLikelihood As Double
        Dim ldblScore As Double

        lstrError = String.Empty

		mMatrixlikelihood = New EIMatrix()
        If Len(mstrWeightVar) > 0 Then
            lintweight = 1
        End If
        If Len(mstrMatchVar) > 0 Then
            lintConditional = 1
        End If
        mboolFirst = True

        ' first calculate with intial estimate - Eric Fontaine 06/14/04
        mMatrixlikelihood.MaximizeLikelihood(0 + NumRows, 0 + NumColumns, DataArray, lintweight + lintConditional + 1, NumColumns - (lintweight + lintConditional + 1), 0 + mlngIter, mdblToler, mdblConv, False)

        If mMatrixlikelihood.GetConvergence = True Then
            'Check the error status and output the string
            If (mMatrixlikelihood.getError(lstrError) = False) Then
                logistic = CreateOutputString(mMatrixlikelihood.getFirstLikelihood, mMatrixlikelihood.getLastLikelihood, mMatrixlikelihood.getIters, mMatrixlikelihood.GetCoefficients, mMatrixlikelihood.GetInverseMatrix, mMatrixlikelihood.GetConvergence, mMatrixlikelihood.getScore)
                'TODO: Re-enable later? 'Residuals(1 + lintweight + lintConditional, mMatrixlikelihood.GetCoefficients)
            Else
                '            Err.Raise vbObjectError, , lstrError
                ReDim Results(1, 0)
                Results(0, 0) = "ERROR"
                Results(1, 0) = lstrError
                logistic = "<BR CLEAR=ALL><p align=""left""><b><tlt>" & lstrError & "</tlt></b></p>"

                args.Add("HTMLRESULTS", logistic)
                context.Display(args)
                Exit Sub
            End If
        Else
            ' if convergence with inital estimate fails, then
            ' try with initial estimate of 0

            ' remember these so we can calculate the score and
            ' the likelihood ratio later...
            ldblFirstLikelihood = mMatrixlikelihood.getFirstLikelihood
            ldblScore = mMatrixlikelihood.getScore

            mMatrixlikelihood = New EIMatrix
            If Len(mstrWeightVar) > 0 Then
                lintweight = 1
            End If
            If Len(mstrMatchVar) > 0 Then
                lintConditional = 1
            End If
            mboolFirst = True

            ' redo regression with inital estimate of 0
            mMatrixlikelihood.MaximizeLikelihood(0 + NumRows, 0 + NumColumns, DataArray, lintweight + lintConditional + 1, NumColumns - (lintweight + lintConditional + 1), 0 + mlngIter, mdblToler, mdblConv, True)

            'Check the error status
            If (mMatrixlikelihood.getError(lstrError) = False) Then
                'output results
                logistic = CreateOutputString(ldblFirstLikelihood, mMatrixlikelihood.getLastLikelihood, mMatrixlikelihood.getIters, mMatrixlikelihood.GetCoefficients, mMatrixlikelihood.GetInverseMatrix, mMatrixlikelihood.GetConvergence, ldblScore)
                'Residuals(1 + lintweight + lintConditional, mMatrixlikelihood.GetCoefficients)
            Else
                '            Err.Raise vbObjectError, , lstrError
                ReDim Results(1, 0)
                Results(0, 0) = "ERROR"
                Results(1, 0) = lstrError
                logistic = "<BR CLEAR=ALL><p align=""left""><b><tlt>" & lstrError & "</tlt></b></p>"

                args.Add("HTMLRESULTS", logistic)
                context.Display(args)
                Exit Sub
            End If

        End If
        
        args.Add("HTMLRESULTS", logistic)

        context.Display(args)

    End Sub

    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Logistic Regression"
        End Get
    End Property

    Public ReadOnly Property ResultArray() As Object
        Get
            ResultArray = VB6.CopyArray(EIRegressGlobals.Results)
        End Get
    End Property

    Private Function MoreThanTwoValues(column As DataColumn) As Boolean

        Dim values As List(Of String)
        values = New List(Of String)

        For Each row As DataRow In currentTable.Rows
            Dim value = row(column.ColumnName).ToString()
            If Not values.Contains(value) And Not String.IsNullOrEmpty(value) Then
                values.Add(value)
            End If

            If values.Count > 2 Then
                Return True
            End If
        Next

        If values.Count <= 2 Then
            Return False
        Else
            Return True
        End If

    End Function

    Private Sub CreateSettingsFromContext()

        Dim i As Integer
        Dim j As Integer
        '   Set default values

        Dim dist As New statlib
        dist = New statlib()

        mstrC = "95"
        mdblC = 0.05
        mdblP = dist.ZFROMP(mdblC * 0.5)
        mlngIter = 15
        mdblConv = 0.00001
        mdblToler = 0.000001
        mboolIntercept = True
        ReDim mStrADiscrete(0)
        ReDim DataArray(0, 0)
        ReDim mstraBoolean(2)
        mstraBoolean(0) = "No"
        mstraBoolean(1) = "Yes"
        mstraBoolean(2) = "Missing"
        mstrMatchVar = ""
        mstrWeightVar = ""

        Dim includeMissing As Boolean
        includeMissing = False

        If context.SetProperties.ContainsKey("TableName") Then
            mstrTableName = context.SetProperties("TableName")
        End If

        If context.SetProperties.ContainsKey("INCLUDE-MISSING") Then
            includeMissing = Boolean.Parse(context.SetProperties("INCLUDE-MISSING"))
        End If

        If context.SetProperties.ContainsKey("Title") Then
            mstrTitle = context.SetProperties("Title")
        End If

        If context.SetProperties.ContainsKey("BLabels") Then

            Dim booleanLabels() As String
            booleanLabels = context.SetProperties("BLabels").ToString().Split(";")

            For i = 0 To UBound(mstraBoolean)
                mstraBoolean(i) = booleanLabels(i)
            Next

            mstrConnString = context.SetProperties("BLabels")
        End If

        If context.SetProperties.ContainsKey("Intercept") Then
            Dim success As Boolean
            success = Boolean.TryParse(context.SetProperties("Intercept"), mboolIntercept)
        End If

        If context.SetProperties.ContainsKey("Iterations") Then
            Dim success As Boolean
            success = Boolean.TryParse(context.SetProperties("Iterations"), mlngIter)
        End If

        If context.SetProperties.ContainsKey("Convergence") Then
            Dim success As Boolean
            success = Boolean.TryParse(context.SetProperties("Convergence"), mdblConv)
        End If

        If context.SetProperties.ContainsKey("Tolerance") Then
            Dim success As Boolean
            success = Boolean.TryParse(context.SetProperties("Tolerance"), mdblToler)
        End If

        If context.SetProperties.ContainsKey("P") Then
            Dim success As Boolean
            success = Double.TryParse(context.SetProperties("P"), mdblP)
            If success = True Then
                'mdblC = 1 - mdblP
                'mdblP = 0 'dist1.ZFROMP((1 - mdblP) * 0.5)
                'mstrC = Str(context.SetProperties("P") * 100)
                mdblC = 1 - mdblP
                mstrC = Str(mdblP * 100)
                mdblP = dist.ZFROMP(mdblC * 0.5)
            End If
        End If
        ReDim mstraTerms(0)
        Dim terms As Integer
        Dim discrete As Integer

        terms = 0
        discrete = 0

        Dim columnNames As List(Of String)
        columnNames = New List(Of String)
        Dim customFilter As String
        customFilter = String.Empty

        For Each kvp As KeyValuePair(Of String, String) In context.InputVariableList
            If kvp.Value.ToLower().Equals("term") Then
                If Not kvp.Key.ToLower().Contains("*") Then
                    columnNames.Add(kvp.Key.ToLower())
                End If
            End If

            If kvp.Value.ToLower().Equals("discrete") Or kvp.Value.ToLower().Equals("matchvar") Or kvp.Value.ToLower().Equals("weightvar") Or kvp.Value.ToLower().Equals("dependvar") Then
                columnNames.Add(kvp.Key.ToLower())
            End If

            If kvp.Value.ToLower().Equals("unsorted") Then
                columnNames.Add(kvp.Key.ToLower())
            End If
        Next

        Dim columnNamesArray() As String
        columnNamesArray = columnNames.ToArray()

        currentTable = context.GetDataRows(Nothing).CopyToDataTable().DefaultView.ToTable("REGRESS", False, columnNamesArray)

        columnNames = New List(Of String)

        For Each kvp As KeyValuePair(Of String, String) In context.InputVariableList
            If kvp.Value.ToLower().Equals("term") Then

                If Not kvp.Key.ToLower().Contains("*") Then
                    columnNames.Add(kvp.Key.ToLower())
                End If

                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1
            End If

            If kvp.Value.ToLower().Equals("discrete") Then
                columnNames.Add(kvp.Key.ToLower())
                ReDim Preserve mStrADiscrete(discrete)
                mStrADiscrete(discrete) = kvp.Key
                discrete = discrete + 1

                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1
            End If

            If kvp.Value.ToLower().Equals("matchvar") Then
                columnNames.Add(kvp.Key.ToLower())
                customFilter = customFilter + "(" + "[" + kvp.Key + "]" + " " + "is not null" + ")" + " AND "
                mstrMatchVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("weightvar") Then
                columnNames.Add(kvp.Key.ToLower())
                customFilter = customFilter + "(" + "[" + kvp.Key + "]" + " " + "is not null" + ")" + " AND "
                mstrWeightVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("dependvar") Then
                columnNames.Add(kvp.Key.ToLower())
                mstrDependVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("unsorted") Then
                Dim type As String
                type = context.Columns(kvp.Key).DataType.ToString()
                If Not MoreThanTwoValues(currentTable.Columns(kvp.Key)) Or type.Equals("System.String") Then
                    'If type.Equals("System.Boolean") Or type.Equals("System.String") Then
                    ReDim Preserve mStrADiscrete(discrete)
                    mStrADiscrete(discrete) = kvp.Key
                    discrete = discrete + 1
                End If
                customFilter = customFilter + "(" + "[" + kvp.Key + "]" + " " + "is not null" + ")" + " AND "
                columnNames.Add(kvp.Key.ToLower())
                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1
            End If

            If kvp.Key.ToLower().Equals("intercept") Then
                Dim success As Boolean
                success = Boolean.TryParse(kvp.Value, mboolIntercept) ' TODO: Test
            End If

            If kvp.Value.ToLower().Equals("p") Then
                Dim success As Boolean
                success = Double.TryParse(kvp.Key.ToLower(), mdblP)
                If success = True Then
                    'mdblC = 1 - mdblP
                    'mstrC = Str(mdblP * 100)
                    'mdblP = 0 'dist1.ZFROMP((1 - mdblP) * 0.5)
                    mdblP = mdblP / 100
                    mdblC = 1 - mdblP
                    mstrC = Str(mdblP * 100)
                    mdblP = dist.ZFROMP(mdblC * 0.5)
                End If
            End If
        Next

        'Dim columnNamesArray() As String
        columnNamesArray = ColumnNames.ToArray()

        currentTable = context.GetDataRows(Nothing).CopyToDataTable().DefaultView.ToTable("REGRESS", False, columnNamesArray)

        If Not String.IsNullOrEmpty(customFilter) And includeMissing = False Then
            customFilter = customFilter.Remove(customFilter.Length - 4, 4)
            currentTable = currentTable.Select(customFilter).CopyToDataTable().DefaultView.ToTable("REGRESS", False)
        End If

    End Sub

   
    'Get Raw Data
    '---From the settings , will load data into the global data array
    ' Outcome , [MatchVar], [WeightVar], [Covariates]
    Private Function GetRawData(ByRef errorMessage As String) As Boolean
        'ERROR HANDLING
        Dim lstrFunction As String
        Dim lstrError As String
        lstrFunction = "GetRawData()"
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim d As Date
        Dim p As Integer
        Dim llngstime As Integer
        Dim llngetime As Integer
        Dim lstrQuery As String
        Dim lStrAVarNames() As String

        Dim lIntRealFields As Integer
        Dim lIntVirtualFields As Integer
        Dim lVarCurData As Object

        Dim lIntTempFields As Integer
        Dim ldblATemp() As Double
        Dim lintOffset As Integer
        Dim lintnull As Integer
        Dim lIntIsMatch As Integer
        Dim lintIntercept As Integer
        Dim lintweight As Integer

        Dim dumdum As String
        Dim llngMatcho As Integer

        If Len(mstrMatchVar) > 0 Then
            mboolIntercept = False
        End If

        If mboolIntercept = True Then
            lintIntercept = 1
        Else
            lintIntercept = 0
        End If

        If Len(mstrWeightVar) > 0 Then
            lintweight = 1
        Else
            lintweight = 0
        End If

        'Gauranteed valid data
        k = 0
        d = TimeOfDay
        NumRows = 0

        On Error Resume Next
        While NumRows = 0

            'Dim tableName As String
            'tableName = context.SetProperties("TableName")
            'NumRows = Int32.Parse(context.DataSource.GetScalar("select count(*) from " + tableName)) ' TODO: Parameterize?
            'NumRows = context.CurrentDataTable.Rows.Count
            'NumRows = context.DataRows.Count
            NumRows = currentTable.Rows.Count

            If NumRows <= 0 Then
                If System.DateTime.FromOADate(TimeOfDay.ToOADate - d.ToOADate).ToOADate * 1000 > 0.1 Then
                    On Error GoTo ErrorhandlEr
                    Err.Raise(435 + vbObjectError, , "<tlt>Waited 10 seconds for data to be set in database</tlt>")
                Else
                    Sleep((100))
                End If
            End If
        End While
        On Error GoTo ErrorhandlEr
        If NumRows = 0 Then Err.Raise(vbObjectError, , "<tlt>No Data available to load</tlt>")
        'Get the Individual Term names by parsing the terms
        lStrAVarNames = VB6.CopyArray(GetStrNames)
        'Dummyfy
        'if there is a match, set up the match variable
        'if there is a weight, set it up too
        'On Error err.Raise vbobjecterrror,,""
        lIntIsMatch = 0

        'Dim reader As IDataReader

        If Len(mstrMatchVar) > 0 Then
            i = 0
            lintnull = 0

            Dim lastValue As Object
            lastValue = VariantType.Null

            Dim tempTable As DataTable
            'tempTable = New DataTable(context.CurrentDataTable.TableName)
            'tempTable.Columns.Add(mstrMatchVar, context.CurrentDataTable.Columns(mstrMatchVar).DataType)
            tempTable = New DataTable(currentTable.TableName)
            tempTable.Columns.Add(mstrMatchVar, currentTable.Columns(mstrMatchVar).DataType)

            For Each row As DataRow In currentTable.Select("", mstrMatchVar)

                Dim columnEqual As Boolean
                columnEqual = False

                If lastValue Is Nothing And row(mstrMatchVar) Is Nothing Then '= VariantType.Null And row(lStrName) = VariantType.Null Then
                    columnEqual = True
                ElseIf lastValue Is Nothing Or row(mstrMatchVar) Is Nothing Then '= VariantType.Null Or row(lStrName) = VariantType.Null Then
                    columnEqual = False
                Else
                    columnEqual = lastValue.Equals(row(mstrMatchVar))
                End If

                If lastValue Is Nothing Or columnEqual = False Then '= VariantType.Null Or columnEqual = False Then
                    lastValue = row(mstrMatchVar)
                    tempTable.Rows.Add(lastValue)
                End If

            Next

            For Each row As DataRow In tempTable.Rows 'context.CurrentDataTable.Rows
                i = i + 1
                If row(mstrMatchVar) Is Nothing Then
                    lintnull = 1
                End If
            Next

            mLngStrata = i - lintnull
            lIntIsMatch = 1
            If mLngStrata < 2 Then Err.Raise(vbObjectError, , "<tlt>Match variable defines only 1 strata</tlt>")
        End If
        'Resume

        ReDim mVarArray(UBound(lStrAVarNames))
        '        On Error GoTo Error_Dummy
        For i = 0 To UBound(lStrAVarNames)
            mVarArray(i) = DummyfyLogistic(lStrAVarNames(i), currentTable)
        Next
        'Resume

        'On Error GoTo Error_Interaction
        setInteractionTerms()
        'Resume

        Dim fieldCount As Int16
        fieldCount = 1
        If (lIntIsMatch = 1) Then
            fieldCount = fieldCount + 1
        End If
        If lintweight = 1 Then
            fieldCount = fieldCount + 1
        End If

        fieldCount = fieldCount + 1
        For i = 1 To UBound(lStrAVarNames)
            fieldCount = fieldCount + 1
        Next



        'lstrQuery = "SELECT [" & mstrDependVar & "], ["
        'If (lIntIsMatch = 1) Then
        '    lstrQuery = lstrQuery & mstrMatchVar & "], ["
        'End If
        'If lintweight = 1 Then
        '    lstrQuery = lstrQuery & mstrWeightVar & "], ["
        'End If

        'lstrQuery = lstrQuery & lStrAVarNames(0)
        'For i = 1 To UBound(lStrAVarNames)
        '    lstrQuery = lstrQuery & "], [" & lStrAVarNames(i)
        'Next
        'lstrQuery = lstrQuery & "] from [" & mstrTableName
        'If lIntIsMatch = 1 Then
        '    lstrQuery = lstrQuery & "] ORDER BY [" & mstrMatchVar & "] ASC"
        'Else
        '    lstrQuery = lstrQuery & "] "
        'End If

        'Dim reader As IDataReader
        Dim table As DataTable
        table = New DataTable

        'reader = context.DataSource.GetDataTableReader(lstrQuery)
        'table.Load(reader)
        'table = context.CurrentDataTable
        'table = currentTable

        Dim rows() As DataRow
        ReDim rows(currentTable.Rows.Count)

        If lIntIsMatch = 1 Then
            rows = currentTable.Select(String.Empty, mstrMatchVar & " ASC, " & mstrDependVar & " ASC")
        Else
            rows = currentTable.Select(String.Empty)
        End If

        'reader = context.DataSource.GetDataTableReader(lstrQuery)
        lIntRealFields = fieldCount 'reader.FieldCount 'lconRS.Fields.Count
        lIntTempFields = 0
        For i = 0 To UBound(lStrAVarNames)
            mVarArray(i).iColumn = lIntTempFields + 1
            lIntTempFields = lIntTempFields + mVarArray(i).iSize
        Next

        'count the number of virtual fields, for all dummies
        lIntVirtualFields = 1 + lIntIsMatch + lintweight + lintIntercept
        For i = 0 To UBound(mstraTerms)
            k = 1
            For j = 0 To UBound(mIVarArray(i).Variables)
                k = k * mVarArray(mIVarArray(i).Variables(j) - 1).iSize
            Next
            lIntVirtualFields = lIntVirtualFields + k
        Next

        ReDim mStrAMatrixLabels(lIntVirtualFields - 1)

        mStrAMatrixLabels(lIntVirtualFields - 2 - lintweight) = "CONSTANT"


        p = 0
        For i = 0 To UBound(mstraTerms)
            makeMatrixLabels(String.Empty, i, 0, p)
        Next


        ReDim DataArray(NumRows - 1, lIntVirtualFields - 1)

        ReDim ldblATemp(lIntTempFields - 1)
        'Main data collection loop
        'If the outcome is missing, match is missing, weight is missing, exclude that row
        'If a continuous variable is missing, exclude it to.
        'Add Some Error Checking
        'On Error GoTo DataError
        llngMatcho = 0
        dumdum = String.Empty
        For i = 0 To NumRows - 1
            'reader.Read()

            lVarCurData = rows(i)(mstrDependVar) 'lVarCurData = table.Rows(i)(0) 'lVarCurData = reader(0) 'lconRS.Fields(0).Value

            If VarType(lVarCurData) = VariantType.Null Then
                lintOffset = lintOffset + 1
                GoTo MISSING

            ElseIf VarType(lVarCurData) = VariantType.String Then
                Err.Raise(3 + vbObjectError, "GET RAW DATA", "<tlt>Please Recode Dependent Variable to be numeric</tlt>")

            End If
            DataArray(i - lintOffset, 0) = rows(i)(mstrDependVar) 'DataArray(i - lintOffset, 0) = table.Rows(i)(0) 'reader(0) 'lconRS.Fields(0).Value

            If lIntIsMatch = 1 Then
                lVarCurData = rows(i)(mstrMatchVar) 'lVarCurData = table.Rows(i)(1) 'reader(1).Value 'lconRS.Fields(1).Value
                If VarType(lVarCurData) = VariantType.Null Then
                    lintOffset = lintOffset + 1
                    rows(i)(mstrDependVar) = Nothing 'table.Rows(i)(0) = VariantType.Null 'reader(0).Value = VariantType.Null 'lconRS.Fields(0).Value = VariantType.Null

                    GoTo MISSING
                Else
                    If dumdum <> lVarCurData.ToString() Then
                        dumdum = lVarCurData
                        llngMatcho = llngMatcho + 1
                    End If
                    DataArray(i - lintOffset, 1) = llngMatcho
                End If

            End If

            If lintweight = 1 Then
                lVarCurData = rows(i)(mstrWeightVar) 'lVarCurData = table.Rows(i)(1 + lIntIsMatch) 'reader(1 + lIntIsMatch)
                If VarType(lVarCurData) = VariantType.Null Then
                    lintOffset = lintOffset + 1
                    rows(i)(mstrDependVar) = VariantType.Null 'table.Rows(i)(0) = VariantType.Null 'reader(0) = VariantType.Null 'lconRS.Fields(0).Value = VariantType.Null 'TODO: FIX!!!

                    GoTo MISSING
                End If
                If rows(i)(mstrWeightVar) <= 0 Then Err.Raise(vbObjectError, , "<tlt>Weight variable must be greater than 0</tlt>") 'If table.Rows(i)(1 + lIntIsMatch) <= 0 Then Err.Raise(vbObjectError, , "<tlt>Weight variable must be greater than 0</tlt>") 'If reader(1 + lIntIsMatch).Value <= 0 Then Err.Raise(vbObjectError, , "<tlt>Weight variable must be greater than 0</tlt>")
                DataArray(i - lintOffset, 1 + lIntIsMatch) = rows(i)(mstrWeightVar) 'DataArray(i - lintOffset, 1 + lIntIsMatch) = table.Rows(i)(1 + lIntIsMatch) 'DataArray(i - lintOffset, 1 + lIntIsMatch) = reader(1 + lIntIsMatch).Value
            End If

            For j = 1 + lIntIsMatch + lintweight To lIntRealFields - 1
                lVarCurData = rows(i)(mVarArray(j - 1 - lIntIsMatch - lintweight).strName) 'lVarCurData = table.Rows(i)(j) 'reader(j)
                If VarType(lVarCurData) = VariantType.Null Then
                    lVarCurData = ""
                End If

                'If the variable is of type 0, it is a boolean with no missing values
                If mVarArray(j - 1 - lIntIsMatch - lintweight).iType = 0 Then
                    'Since it is a boolean with no missings, the data is correct
                    ldblATemp(mVarArray(j - 1 - lIntIsMatch - lintweight).iColumn - 1) = lVarCurData
                    'Otherwise, the variable is type one, Then it has a base string
                    'There will be no Null Values in this field
                ElseIf mVarArray(j - 1 - lIntIsMatch - lintweight).iType = 1 Then
                    If Not (StrComp(mVarArray(j - 1 - lIntIsMatch - lintweight).strBase, lVarCurData) = 0) Then
                        ldblATemp(mVarArray(j - 1 - lIntIsMatch - lintweight).iColumn - 1) = 1
                    End If
                    'For Variables of type two, it is a dummy null with string values
                ElseIf mVarArray(j - 1 - lIntIsMatch - lintweight).iType = 2 Or mVarArray(j - 1 - lIntIsMatch - lintweight).iType = 4 Then
                    'If the variable has a base value, it will not be in the string array
                    'If a variable is found, it will not be a base variable.
                    'if lvarcurdata contains a Null, the loop will end, without finding
                    'The Null Variable, causing every element in this dummy to be 0
                    For k = 0 To mVarArray(j - 1 - lIntIsMatch - lintweight).iSize - 1
                        If StrComp(mVarArray(j - 1 - lIntIsMatch - lintweight).strNames(k), lVarCurData) = 0 Then
                            ldblATemp(mVarArray(j - 1 - lIntIsMatch - lintweight).iColumn + k - 1) = 1
                        End If
                    Next
                    'If the variable is of type 3, Null values must be rejected
                ElseIf mVarArray(j - 1 - lIntIsMatch - lintweight).iType = 3 Then
                    If Len(lVarCurData) = 0 Then
                        lintOffset = lintOffset + 1
                        rows(i)(mstrDependVar) = VariantType.Null 'table.Rows(i)(0) = VariantType.Null 'reader(0) = VariantType.Null '' TODO: FIX!!!!!

                        GoTo MISSING
                    Else
                        'otherwise we can put the value in the array
                        ldblATemp(mVarArray(j - 1 - lIntIsMatch - lintweight).iColumn - 1) = lVarCurData
                    End If
                End If
            Next j
            RecursiveFactorialize(ldblATemp, i - lintOffset)
            If lintIntercept = 1 Then
                DataArray(i - lintOffset, lIntVirtualFields - 1) = 1
            End If
MISSING:
            For j = 0 To UBound(ldblATemp)
                ldblATemp(j) = 0
            Next

        Next i

        'reader.Close()
        'reader.Dispose()

        NumRows = NumRows - lintOffset
        NumColumns = lIntVirtualFields
        GetRawData = True
        'End If

cleanup:
        Exit Function

ErrorhandlEr:
        'Err.Raise(vbObjectError + 234, , Err.Description)
        errorMessage = Err.Description
        GetRawData = False
        '    Debug.Print " Error in GetRawData"
        '    Resume cleanup
        'DataError:
        '    Debug.Print "ERROR on DATALINE " & i & " trying to assign " & lVarCurData & " to variable " & mVarArray(j - 1 - lIntIsMatch).strName
        '    Debug.Print mVarArray(j - 1 - lIntIsMatch).iSize
        '    Resume cleanup
        'Error_BadQuery:
        '    Debug.Print "Bad Query " & lstrQuery
        '    Resume cleanup
        'Error_BadMatch:
        '    Debug.Print "Non existant, or incompatable MAtch DAta: MatchVar = " & mstrMatchVar
        '    Resume cleanup
        'Error_Dummy:
        '    Debug.Print "Error Creating Dummy Variables"
        '    Resume cleanup
        'Error_Interaction:
        '    Debug.Print "Error setting interaction Terms"
        '    Resume cleanup
    End Function
    'Recursive function to Factorialize dummy variables
    'Evaluate the conditional likelihood
    'formula in kleihbalms book

    Private Function Conditional(ByRef lintOffset As Integer, ByRef ldblaDataArray(,) As Double, ByRef ldblaJacobian(,) As Double, ByRef ldblaB() As Double, ByRef ldblaF() As Double, ByRef nRows As Integer) As Double
        'Conditional regression.  Very similar to the unconditional except do more iterations, and exclude a constant
        'term
        'if conditional regression was called, it will have the match var in datamatrix position 2
        'Data Matrix has it's number of columns and number of rows defined globably
        'The number of terms nTerms, has all the unique terms that are to be used, with their
        'cooresponding cutpoints, and dummy varialbes.
        'FUNCTION
        'GetTerms will put them in an array

        'Vectors and matricies
        Dim x() As Double
        Dim lDblAParamSum() As Double
        Dim t() As Double
        Dim C(,) As Double
        'Redimming them to the amount of terms we have
        ReDim x(UBound(ldblaB))
        ReDim C(UBound(ldblaB), UBound(ldblaB))
        ReDim lDblAParamSum(UBound(ldblaB))
        ReDim t(UBound(ldblaB))

        'Statistical variables
        Dim IthLikelihood As Double
        Dim LogLikelihood As Double
        'Dim ithcontribution As Double
        Dim likelihood As Double

        'Iterative variables
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim s As Integer

        Dim lIntRow As Integer
        Dim lLevels As Integer
        Dim lLeveldata As Double
        Dim lDblT0 As Double

        Dim cases As Integer
        Dim count As Integer

        Dim ldblweight As Integer

        ldblweight = 1
        likelihood = 1
        'Looop
        lIntRow = 1
        lLevels = 0
        For s = 1 To mLngStrata

            lIntRow = lIntRow + lLevels
            'count the levels
            lLevels = 1
            lLeveldata = ldblaDataArray(lIntRow - 1, 1)
            If s = mLngStrata Then
                lLevels = nRows - lIntRow + 1
            Else
                While lLeveldata = ldblaDataArray(lIntRow + lLevels - 1, 1)
                    lLevels = lLevels + 1
                End While
            End If

            'now loop
            lDblT0 = 0
            count = 0
            cases = 0
            For i = lIntRow - 1 To lLevels + lIntRow - 2

                'Weight stuff
                If (lintOffset = 3) Then
                    ldblweight = ldblaDataArray(i, lintOffset)
                End If

                count = count + ldblweight
                'Load x's
                For j = 0 To UBound(x)
                    x(j) = ldblaDataArray(i, j + lintOffset)
                Next

                'Add to the parameter sum
                If ldblaDataArray(i, 0) > 0 Then
                    For j = 0 To UBound(x)
                        lDblAParamSum(j) = lDblAParamSum(j) + x(j)
                    Next
                    cases = cases + ldblweight
                End If

                'Now X's are loaded...
                'Compute a dotproduct
                IthLikelihood = 0
                For j = 0 To UBound(x)
                    IthLikelihood = IthLikelihood + x(j) * ldblaB(j)
                Next

                IthLikelihood = System.Math.Exp(IthLikelihood)
                IthLikelihood = IthLikelihood * ldblweight

                lDblT0 = lDblT0 + IthLikelihood
                For k = 0 To UBound(x)
                    t(k) = t(k) + IthLikelihood * x(k)
                Next

                For k = 0 To UBound(x)
                    For j = 0 To UBound(x)
                        C(j, k) = C(j, k) + x(k) * x(j) * IthLikelihood
                    Next
                Next
            Next

            'Do some more junk...
            IthLikelihood = 0
            For i = 0 To UBound(ldblaB)
                IthLikelihood = IthLikelihood + lDblAParamSum(i) * ldblaB(i)
            Next

            If cases = count Or cases = 0 Then GoTo NoContrast
            Conditional = Conditional + IthLikelihood - System.Math.Log(lDblT0)

            'Compute the Function Evaulations
            For i = 0 To UBound(x)
                ldblaF(i) = ldblaF(i) + lDblAParamSum(i) - (t(i) / lDblT0)
            Next

            For i = 0 To UBound(x)
                For k = 0 To UBound(x)
                    ldblaJacobian(i, k) = ldblaJacobian(i, k) + C(i, k) / lDblT0 - t(i) * t(k) / (lDblT0 * lDblT0)
                Next
            Next
NoContrast:
            'Must clear the matricies before we repeat
            For i = 0 To UBound(x)
                lDblAParamSum(i) = 0
                t(i) = 0
                For k = 0 To UBound(ldblaB)

                    C(i, k) = 0
                Next
            Next
        Next
    End Function

    Private Function UnConditional(ByRef lintOffset As Integer, ByRef ldblaDataArray(,) As Double, ByRef ldblaJacobian(,) As Double, ByRef ldblaB() As Double, ByRef ldblaF() As Double, ByRef nRows As Integer) As Double
        Dim x() As Double
        'UPGRADE_WARNING: Lower bound of array x was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim x(UBound(ldblaB))

        Dim ldblIthLikelihood As Double
        Dim ldblIthContribution As Double
        Dim ithcontribution As Double
        Dim ldblweight As Double

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer

        ldblweight = 1

        For i = 0 To nRows - 1
            'Load x's
            'For the constant term,  where it is always one
            For j = 0 To UBound(x)
                x(j) = ldblaDataArray(i, j + lintOffset)
            Next

            If lintOffset = 2 Then
                ldblweight = ldblaDataArray(i, 1)
            End If
            'Now X's are loaded...
            'Compute the ith likelihood function
            ldblIthLikelihood = 0
            For j = 0 To UBound(x)
                ldblIthLikelihood = ldblIthLikelihood + x(j) * ldblaB(j)
            Next
            ldblIthLikelihood = 1 / (1 + System.Math.Exp(-ldblIthLikelihood))
            If (ldblaDataArray(i, 0) = 0) Then
                ldblIthContribution = 1 - ldblIthLikelihood
            Else
                ldblIthContribution = ldblIthLikelihood
            End If

            For k = 0 To UBound(x)
                If ldblaDataArray(i, 0) > 0 Then
                    ldblaF(k) = ldblaF(k) + (1 - ldblIthLikelihood) * x(k) * ldblweight
                Else
                    ldblaF(k) = ldblaF(k) + (0 - ldblIthLikelihood) * x(k) * ldblweight
                End If

                For j = 0 To UBound(x)
                    ldblaJacobian(j, k) = ldblaJacobian(j, k) + (x(k) * x(j) * (1 - ldblIthLikelihood) * ldblIthLikelihood) * ldblweight
                Next
            Next

            'Multiply to get the likelihood
            UnConditional = UnConditional + System.Math.Log(ldblIthContribution) * ldblweight
        Next
        UnConditional = UnConditional

    End Function

    
    'Likelihood Callback
    Private Sub mMatrixlikelihood_CalcLikelihood(ByRef lintOffset As Integer, ByRef ldblA As System.Array, ByRef ldblaB As System.Array, ByRef ldblaJacobian As System.Array, ByRef ldblaF As System.Array, ByRef nRows As Integer, ByRef likelihood As Double, ByRef strError As String, ByRef booStartAtZero As Boolean) Handles mMatrixlikelihood.CalcLikelihood
        Dim i As Integer
        Dim k As Boolean

        'for the constant term
        Dim ncases As Double
        Dim nrecs As Double

        ' added by Eric Fontaine 07/28/03: a value of "" means no error
        strError = ""

        'if we are in the first likelihood loop
        'set up the constant terms
        If mboolFirst = True And mboolIntercept = True Then

            mboolFirst = False
            ncases = 0
            'count the records and observed failures
            For i = 0 To UBound(ldblA, 1)
                If Len(mstrWeightVar) > 0 Then
                    If ldblA(i, 0) = 1 Then

                        ncases = ncases + ldblA(i, lintOffset)
                    End If
                    nrecs = nrecs + ldblA(i, lintOffset)
                Else
                    If ldblA(i, 0) = 1 Then
                        ncases = ncases + 1
                    End If
                    nrecs = nrecs + 1
                End If
            Next i
            'fix the nrecs
            'nrecs = nrecs - 1
            'if it is conditional the constant term does not exist
            If Len(mstrMatchVar) > 0 Then

            Else
                'Check for log(0) and division by 0
                If ncases <> 0 And nrecs - ncases <> 0 Then

                    ' added start at zero by Eric Fontaine 06/14/04
                    If booStartAtZero Then
                        ldblaB(UBound(ldblaB)) = 0
                    Else
                        ldblaB(UBound(ldblaB)) = System.Math.Log(ncases / (nrecs - ncases))
                    End If

                Else
                    If ncases = 0 Then
                        ' modified by Eric Fontaine 07/28/03: return an error
                        strError = "<tlt>Dependent variable contains no cases</tlt>"
                        Exit Sub
                    Else
                        'mmatrixlikelihood.
                        ' modified by Eric Fontaine 07/28/03: return an error
                        strError = "<tlt>Dependent variable contains no controls</tlt>"
                        Exit Sub
                    End If
                End If
            End If
        End If

        'Choose Conditional or Unconditional
        If Len(mstrMatchVar) > 0 Then
            likelihood = Conditional(lintOffset, ldblA, ldblaJacobian, ldblaB, ldblaF, nRows)
        Else
            likelihood = UnConditional(lintOffset, ldblA, ldblaJacobian, ldblaB, ldblaF, nRows)
        End If

    End Sub
End Class