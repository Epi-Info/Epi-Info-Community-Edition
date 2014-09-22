Option Strict Off
Option Explicit On
Option Compare Text

Public Class EIKaplanMeierSurvival
    Implements EpiInfo.Plugin.IAnalysisStatistic
    'VERSION 1.0 CLASS
    'BEGIN
    '  MultiUse = -1  'True
    '  Persistable = 0  'NotPersistable
    '  DataBindingBehavior = 0  'vbNone
    '  DataSourceBehavior = 0   'vbNone
    '  MTSTransactionMode = 0   'NotAnMTSObject
    'End
    Private dataTable As DataTable
    Public context As EpiInfo.Plugin.IAnalysisStatisticContext

    Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
        dataTable = New DataTable

        For Each column As DataColumn In context.Columns
            dataTable.Columns.Add(column.ColumnName, column.DataType)
        Next

        For Each row As DataRow In context.GetDataRows(Nothing)
            dataTable.ImportRow(row)
        Next

    End Sub

    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Kaplan-Meier Survival"
        End Get
    End Property

    Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)

    Private Const ErrStart As Integer = &H3000
    Private Const conConnStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="

    Private WithEvents mMatrixlikelihood As EIMatrix

    Public ReadOnly Property ResultArray() As Object
        Get
            ResultArray = VB6.CopyArray(StatisticsRepository.Results)
        End Get
    End Property


    Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute
        'Formerly Function EIKaplanSurv() As String

        CreateSettingsFromContext()

        Dim strKaplanResults As String

        Dim ldblaSProb() As Double
        Dim lintaRisk() As Integer
        Dim lintaFailed() As Integer
        Dim lintaCensored() As Integer
        Dim ldblaSTimes() As Double
        Dim k, i, j, f As Integer
        Dim lSVarArray() As StrataVariable
        Dim lStrataA() As Strata
        Dim lS2() As Strata
        Dim lvaraOutTable(,) As Object
        Dim lvaraStringTable() As Object
        Dim lintrows As Integer
        Dim lintcols As Integer
        Dim p As Integer
        Dim NumRows As Integer
        Dim lintStartPos As Integer
        Dim lintEndPos As Integer
        Dim lstrdebug As String
        Dim lintTotalRisk As Integer
        Dim ldblpeto, ldbllogrank As Double
        Dim ldblSI() As Double

        Dim d As Date

        Dim args As Dictionary(Of String, String)
        args = New Dictionary(Of String, String)

        On Error GoTo erroRHandler
        '''''''''''''''''''''''''''

        'DISABLE ERROR HANDLING
        On Error Resume Next
        NumRows = context.GetDataRows(Nothing).Count

        If NumRows <> 0 Then
            p = 0
            d = TimeOfDay

            On Error GoTo erroRHandler
            If Len(mstrWeightVar) > 0 Then
                mintWeight = 1
            End If
            'Setup the number of columns in the result table
            '1 for the survival time, 3 for the expected,
            lintcols = 1 + UBound(mstraStrataVar) + 1 + 3 + 2
            lintrows = 0
            'For Kaplan, the Covariates are the strata vars
            mstraStrataVar = VB6.CopyArray(mstraCovariates)
            'Load covariates into array
            SetStratified(mstraStrataVar, lSVarArray)
            LoadPlotTable(mstraStrataVar, lSVarArray, lStrataA)
            'Done with loading

            ReDim lS2(UBound(lStrataA))
            ReDim ldblSI(UBound(lStrataA) + 1)

            ' go through the strata, compute the survival curves
            For k = 0 To UBound(lStrataA)
                With lStrataA(k)
                    'f is the strata indexer.
                    f = 0
                    If (.lintrows < 1) Then Err.Raise(vbObjectError + 12345, , "<tlt>Strata has no data: " & lStrataA(k).strName & "</tlt>")

                    'Get the expected curves
                    ldblaSProb = Expected(.dblaData, .lintrows, .lintcols, 3, ldblaSTimes, lintaRisk, lintaFailed, lintaCensored)

                    ReDim Preserve lvaraOutTable(lintcols, lintrows + UBound(ldblaSProb))
                    ReDim lS2(k).dblaData(UBound(ldblaSProb), 3)
                    ldblSI(k) = lintrows + 1

                    'Dump Table in outtable..
                    For j = 0 To UBound(ldblaSTimes)

                        lvaraOutTable(0, lintrows + j) = ldblaSTimes(j)
                        'Set up column names for this strata thingy
                        For i = 0 To UBound(mstraStrataVar)
                            lvaraOutTable(i + 1, lintrows + j) = .strName
                        Next i
                        i = i + 1
                        lvaraOutTable(i, lintrows + j) = lintaFailed(j)
                        lvaraOutTable(i + 1, lintrows + j) = lintaCensored(j)
                        lvaraOutTable(i + 2, lintrows + j) = lintaRisk(j)
                        'Survival curve
                        lvaraOutTable(i + 3, lintrows + j) = ldblaSProb(j)
                        'Plop in Expected Values
                        'lvaraOutTable(i + 4, lintrows + j) = LDBLAEXPECTED FORMULAE
                        lS2(k).dblaData(f, 0) = ldblaSTimes(j)
                        lS2(k).dblaData(f, 1) = lintaCensored(j)
                        lS2(k).dblaData(f, 2) = lintaFailed(j)
                        lS2(k).dblaData(f, 3) = lintaRisk(j)
                        f = f + 1
                    Next j

                    lintrows = lintrows + UBound(ldblaSProb) + 1
                End With

            Next k
            ldblSI(UBound(ldblSI)) = lintrows + 1

            Dim lvarastat(,) As Object
            strKaplanResults = logrank2(lS2, lStrataA, ldblpeto, ldbllogrank, lvaraOutTable, ldblSI, lvarastat)
            ReDim Results(1, 1)
            Results(0, 0) = "outtable"
            Results(1, 0) = VB6.CopyArray(lvaraOutTable)
            Results(0, 1) = "tests"
            Results(1, 1) = VB6.CopyArray(lvarastat)
            Debug.Print("RESULT TABLE")

            Debug.Print("STime" & Chr(9) & "PVars" & Chr(9) & "Died " & Chr(9) & "Censor" & Chr(9) & "Riskset" & Chr(9) & "ActualS" & Chr(9) & "ExpectS" & Chr(9) & "Hazard")
            For i = 0 To UBound(lvaraOutTable, 2)
                lstrdebug = ""
                For j = 0 To UBound(lvaraOutTable, 1)
                    If VarType(lvaraOutTable(j, i)) = VariantType.Null Then
                        lstrdebug = lstrdebug & Chr(9) & Chr(9)
                    Else
                        lstrdebug = lstrdebug & VB6.Format(lvaraOutTable(j, i), "0.0000") & Chr(9)
                    End If
                Next j
                Debug.Print(lstrdebug)
            Next i

            args.Add("COMMANDNAME", "KMSURVIVAL")
            args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
            args.Add("HTMLRESULTS", strKaplanResults)
            context.Display(args)

        End If

cleanup:
        Exit Sub

erroRHandler:

        strKaplanResults = "<TLT>ERROR:</TLT> " & Err.Description
        ReDim Results(2, 1)
        Results(1, 1) = "ERROR"
        Results(2, 1) = Err.Description

        strKaplanResults = "<br clear=""all"" /><p align=""left""><b><tlt> " & Err.Description & "</tlt></b></p>"

        args = New Dictionary(Of String, String)
        args.Add("COMMANDNAME", "KMSURVIVAL")
        args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
        args.Add("HTMLRESULTS", strKaplanResults)
        context.Display(args)
        'Resume
        ReDim Results(1, 0)


        Exit Sub
        Resume
    End Sub

    Private Sub CreateSettingsFromContext()
        'Called from Execute(), this routine sets several constants for Kaplan Meier Survival Analysis and
        '  extracts settings and properties from the data held in the context
        Dim i As Integer
        Dim dist1 As New statlib
        dist1 = New statlib()
        'Initialize Default Values
        mstrC = "95"
        mdblC = 0.05
        mdblP = dist1.ZFROMP(mdblC * 0.5)
        mlngIter = 15

        mdblConv = 0.000001
        mdblToler = 0.000001
        mintTimeDepCount = 0
        ReDim DataArray(0, 0)

        Dim mstraboolean(2) As String
        mstrWeightVar = ""
        mintWeight = 0
        mstraboolean(0) = "False"
        mstraboolean(1) = "True"
        mstraboolean(2) = "Missing"

        mstraBLabels(0) = "False"
        mstraBLabels(1) = "True"

        'Extract data from context.SetProperties

        If context.SetProperties.ContainsKey("Database") Then
            mstrConnString = conConnStr & context.SetProperties("Database") & ";"
        End If

        If context.SetProperties.ContainsKey("ConnectionString") Then
            mstrConnString = context.SetProperties("ConnectionString")
        End If

        If context.SetProperties.ContainsKey("TableName") Then
            mstrTableName = context.SetProperties("TableName")
        End If

        If context.SetProperties.ContainsKey("BLabels") Then
            Dim booleanLabels() As String
            booleanLabels = context.SetProperties("BLabels").ToString().Split(";")

            For i = 0 To UBound(mstraboolean)
                mstraboolean(i) = booleanLabels(i)
            Next
        End If

        'If context.SetProperties.ContainsKey("ShowBaseline") Then
        '    Dim success As Boolean
        '    success = Boolean.TryParse(context.SetProperties("ShowBaseline"), mboolShowBaseline)
        'End If

        'If context.SetProperties.ContainsKey("Iterations") Then
        '    Dim success As Boolean
        '    success = Boolean.TryParse(context.SetProperties("Iterations"), mlngIter)
        'End If

        'If context.SetProperties.ContainsKey("Convergence") Then
        '    Dim success As Boolean
        '    success = Boolean.TryParse(context.SetProperties("Convergence"), mdblConv)
        'End If

        'If context.SetProperties.ContainsKey("Tolerance") Then
        '    Dim success As Boolean
        '    success = Boolean.TryParse(context.SetProperties("Tolerance"), mdblToler)
        'End If

        'If context.SetProperties.ContainsKey("P") Then
        '    Dim success As Boolean
        '    success = Double.TryParse(context.SetProperties("P"), mdblP)
        '    If success = True Then
        '        mdblC = 1 - mdblP
        '        mdblP = dist1.ZFROMP((1 - mdblP) * 0.5)
        '        mstrC = Str(context.SetProperties("P") * 100)
        '    End If
        'End If

        ReDim mstraTerms(0)
        Dim terms As Integer
        Dim discrete As Integer
        Dim covariate As Integer

        terms = 0
        discrete = 0
        covariate = 0

        'Extract data from context.InputVariableList
        'Currently, only one group variable is accepted for "covariates"
        'In KMSurvival, Covariates and StrataVar are used interchangeably

        mstrGroupVar = context.InputVariableList("group_variable").ToString()
        ReDim mstraCovariates(0)
        mstraCovariates(0) = mstrGroupVar

        ReDim mstraStrataVar(0)
        mstraStrataVar(0) = mstrGroupVar

        mstrTimeVar = context.InputVariableList("time_variable").ToString()
        'ToDo: den4: eventually, remove next ReDim when ExtendVarList added to context
        ReDim mstraTimeDependentVar(0)
        'mstraTimeDependentVar = context.InputVariableList("ExtendVarList").ToString().Split(";")
        mstrCensoredVar = context.InputVariableList("censor_variable").ToString()
        Select Case (context.InputVariableList("uncensored_value").ToString())
            Case "(+)"
                mstrUncensoredVal = "1"
            Case "(-)"
                mstrUncensoredVal = "0"
            Case "(.)"
                mstrUncensoredVal = String.Empty
            Case Else
                mstrUncensoredVal = context.InputVariableList("uncensored_value").ToString()
        End Select
        If Not context.InputVariableList("weight_variable") Is Nothing Then
            mstrWeightVar = context.InputVariableList("weight_variable").ToString()
        Else
            mstrWeightVar = String.Empty
        End If

        If Not context.InputVariableList("graph_type") Is Nothing Then
            mstrGraphType = context.InputVariableList("graph_type").ToString()
        End If



    End Sub

    Public Sub SetStratified(ByRef lstraStrata() As String, ByRef lSVarArray() As StrataVariable)
        'Dim lconRS As ADODB.Recordset
        Dim i As Integer
        Dim k As Integer
        Dim lvarNullTest As Object
        Dim lboolNull As Boolean



        ReDim lSVarArray(UBound(lstraStrata))

        If Len(lstraStrata(0)) = 0 Then
            lSVarArray(0).iTerms = 1
            lSVarArray(0).strName = "ALL"
            ReDim lSVarArray(0).straTerms(0)
            lSVarArray(0).straTerms(0) = "ALL"
        Else
            'Set up Strata Variable Array
            For k = 0 To UBound(lstraStrata)
                With lSVarArray(k)
                    .iTerms = 0
                    .strName = lstraStrata(k)
                    ReDim .straTerms(0)
                    'Open the record as a distinct set and count the non missing values
                    Dim tempTable As DataTable
                    tempTable = New DataTable("output")
                    tempTable.Columns.Add(lstraStrata(k), context.Columns(lstraStrata(k)).DataType)

                    Dim lastValue As Object
                    lastValue = VariantType.Null

                    ' We must be able to do a SELECT DISTINCT on the data table; this code will replicate
                    ' that functionality using .NET code, since we can't do that operating directly against
                    ' a DataTable object using SQL.
                    For Each row As DataRow In dataTable.Select("", lstraStrata(k))

                        Dim columnEqual As Boolean
                        columnEqual = False

                        If lastValue Is Nothing And row(lstraStrata(k)) Is Nothing Then
                            columnEqual = True
                        ElseIf lastValue Is Nothing Or row(lstraStrata(k)) Is Nothing Then
                            columnEqual = False
                        Else
                            columnEqual = lastValue.Equals(row(lstraStrata(k)))
                        End If

                        If lastValue Is Nothing Or columnEqual = False Then
                            lastValue = row(lstraStrata(k))
                            tempTable.Rows.Add(row(lstraStrata(k)))
                        End If
                    Next
                    '============================================================================================


                    'Set the null value counter to 0
                    lboolNull = False

                    For i = 0 To tempTable.Rows.Count - 1
                        lvarNullTest = tempTable.Rows(i)(lstraStrata(k))
                        If VarType(lvarNullTest) = VariantType.Null Then
                            If lboolNull = False Then
                                ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                                .straTerms(UBound(.straTerms) - 1) = System.DBNull.Value
                                .iTerms = .iTerms + 1
                            End If
                            lboolNull = True
                        ElseIf VarType(lvarNullTest) = VariantType.String Then
                            If Len(lvarNullTest) = 0 Then
                                If lboolNull = False Then
                                    ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                                    .straTerms(UBound(.straTerms) - 1) = String.Empty
                                    .iTerms = .iTerms + 1
                                End If
                                lboolNull = True

                            Else
                                'If it is a normal number, add it to the array
                                ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                                .straTerms(UBound(.straTerms) - 1) = lvarNullTest
                                .iTerms = .iTerms + 1
                            End If
                        Else
                            'If it is a normal number, add it to the array
                            ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                            .straTerms(UBound(.straTerms) - 1) = lvarNullTest
                            .iTerms = .iTerms + 1
                        End If
                    Next i
                    ReDim Preserve .straTerms(.iTerms - 1)
                End With
            Next k
        End If
    End Sub

    Public Sub LoadPlotTable(ByRef lstraGroups() As String, ByRef lSVarArray() As StrataVariable, ByRef lStrataA() As Strata)
        'called from EIKaplanMeierSurvival.vb  LoadPlotTable(mstraStrataVar, lSVarArray, lStrataA)
        ' routine copied from EICoxGraph.vb
        Dim lintStrata As Integer
        Dim i As Integer
        Dim lstrbase As String
        Dim lstrSortOrder As String
        Dim lintTempFields As Integer
        lintStrata = 1
        'for KMSurv, lstraGroups (aka mstraStrataVar) is always a count of 1
        For i = 0 To lstraGroups.Count - 1
            lintStrata = lintStrata * lSVarArray(i).iTerms
            Debug.Print("Strata " & lSVarArray(i).strName & " Levels " & lSVarArray(i).iTerms)
        Next
        Debug.Print("Number of Tables to be loaded = " & lintStrata)
        ReDim lStrataA(lintStrata - 1)

        lstrbase = String.Empty
        mintRealFields = 2
        If mintWeight > 0 Then
            lstrbase = lstrbase & ", [" & mstrWeightVar & "] "
            mintRealFields = mintRealFields + 1
        End If

        Debug.Print("QueryString")
        Debug.Print(lstrbase)
        Debug.Print("Real Amount of Covariates + Censored +STime " & mintRealFields)
        lintTempFields = 0
        lstrSortOrder = String.Empty

        If lintStrata = 1 Then
            lstrSortOrder = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC"
            lStrataA(1).strName = lSVarArray(1).strName & " " & lSVarArray(1).straTerms(1)
            LoadPlotData(lstrbase, lstrSortOrder, 1, lStrataA)
        Else
            StrataRecurseWOC(lstrbase, lstrSortOrder, 1, 0, lSVarArray, lStrataA)
        End If

    End Sub

    Private Sub LoadPlotData(ByRef lstrQuery As String, ByRef lstrSortOrder As String, ByRef lintIndex As Integer, ByRef lStrataA() As Strata)

        Dim debugstring As String
        Dim lvarCurData As Object
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim lintOffset As Integer
        Dim ldblatemp() As Double

        Dim table As DataTable
        Dim rows() As DataRow
        ReDim ldblatemp(mintRealFields)

        Debug.Print(" Loading data with")
        Debug.Print(lstrQuery & " order by " & lstrSortOrder)


        table = dataTable
        rows = table.Select(lstrQuery, lstrSortOrder)
        k = rows.Count
        If k = 0 Then Err.Raise(vbObjectError + 2131, , "<tlt>No data in table</tlt>")
        'The data that is important, is the weight var, the  time var, the censored var
        lStrataA(lintIndex).lintcols = 2 + mintWeight
        lStrataA(lintIndex).lintrows = k

        Debug.Print("Continuing to Load Strata " & lStrataA(lintIndex).strName)
        Debug.Print("Columns = " & lStrataA(lintIndex).lintcols)
        Debug.Print("Rows = " & lStrataA(lintIndex).lintrows)

        ReDim lStrataA(lintIndex).dblaData(k - 1, lStrataA(lintIndex).lintcols - 1)
        Dim currentRow As Integer
        For currentRow = 0 To rows.Count - 1
            '   SELECT TimeVar, CensoredVar, WeightVar (if applicable), So...
            '   RS.Fields(0) = mstrTimeVar
            '   RS.Fields(1) = mstrCensoredVar 
            '   RS.Fields(2) = mstrWeightVar


            lvarCurData = rows(currentRow)(mstrTimeVar) 'lvarCurData = lconRS.Fields(0).Value
            'This is a missing value.. then..
            If VarType(lvarCurData) = VariantType.Null Then
                lintOffset = lintOffset + 1
                GoTo MISSING
            ElseIf lvarCurData <= 0 Then
                Err.Raise(vbObjectError + 2333, , "<tlt>Cannot accept survival times less than or equal to zero.</tlt>")
            Else
                lStrataA(lintIndex).dblaData(currentRow - lintOffset, 0) = lvarCurData
            End If
            If EICoxLoadData.isNumber(lvarCurData) Then
                If VarType(lvarCurData) = VariantType.String Then
                    lvarCurData = Val(lvarCurData)
                End If
            Else
                Err.Raise(vbObjectError + 2333, , "<tlt>Survival times must be a number greater than zero.</tlt>")
            End If

            'In Epi3, the temp table is constructed with the SQL Select where
            ' SELECT iif(" & taVariable(0).sExpression & "=" & s & ",1,0)
            '   .sExpression is the Censored var and s is the value for uncensored.
            '   so, when Censored_var = Uncensored_value then it returns a 1, otherwise, including this
            '   condition when Censored_var is VariantType.Null, then it returns 0.
            'As long as the value for uncensored is not null, then when it is null, return a zero.

            'lStrataA(lintIndex).dblaData(i - lintOffset, 2) = lconRS.Fields(1).Value
            lStrataA(lintIndex).dblaData(currentRow - lintOffset, 1) = IIf(StrComp(mstrUncensoredVal.ToUpper, rows(currentRow)(mstrCensoredVar).ToString.ToUpper) = 0, 1, 0)
            If mintWeight = 1 Then
                'lvarCurData = lconRS.Fields(2).Value
                lvarCurData = rows(currentRow)(mstrWeightVar)
                If VarType(lvarCurData) = VariantType.Null Then
                    lintOffset = lintOffset + 1
                    'lconRS.Fields(0).value = vbNull
                    'lconRS.Fields(0).Value = System.DBNull.Value
                    'lconRS.Update()
                    GoTo MISSING
                End If

                If rows(currentRow)(mstrWeightVar) <= 0 Then Err.Raise(vbObjectError, "", "<tlt>Weight must be greater than 0. Weight value found as: </tlt>" & rows(currentRow)(mstrWeightVar))
                lStrataA(lintIndex).dblaData(currentRow - lintOffset, 2) = rows(currentRow)(mstrWeightVar)
            End If

MISSING:
        Next currentRow
        lStrataA(lintIndex).lintrows = lStrataA(lintIndex).lintrows - lintOffset

        'For i = 1 To lStrataA(lintIndex).lintrows
        For i = 0 To lStrataA(lintIndex).lintrows - 1
            debugstring = "Loaded "
            For j = 0 To lStrataA(lintIndex).lintcols - 1
                debugstring = debugstring & lStrataA(lintIndex).dblaData(i, j) & " "
            Next
            Debug.Print(debugstring)
        Next
    End Sub

    Private Sub StrataRecurseWOC(ByVal lstrbase As String, ByVal lstrSortOrder As String, ByVal lintLevel As Object, ByRef lintSIndex As Integer, ByRef lSVarArray() As StrataVariable, ByRef lStrataA() As Strata)

        Dim j As Integer
        Dim lstrOther As String
        Dim lstrQuery As String
        lstrOther = ""
        lstrQuery = ""
        'If the level is the last strata var
        If lintLevel = UBound(lSVarArray) + 1 Then
            With lSVarArray(UBound(lSVarArray))
                'loop through each value of the strata
                For j = 0 To .iTerms - 1
                    If lintLevel = 1 Then
                        If IsDBNull(.straTerms(j)) Then
                            lstrQuery = "[" & .strName & "] is Null"
                            lstrSortOrder = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC"
                        Else
                            lstrQuery = "[" & .strName & "] = " & Wrap(.straTerms(j))
                            lstrSortOrder = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC"
                        End If
                    Else
                        If .straTerms(j) = "" Then
                            lstrQuery = "AND [" & .strName & "] = " & System.DBNull.Value & " AND [" & .strName & "] = " & Chr(34) & Chr(34) '   
                            lstrSortOrder = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC"
                        Else
                            lstrQuery = " AND [" & .strName & "] = " & Wrap(.straTerms(j))
                            lstrSortOrder = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC"
                        End If
                    End If
                    lStrataA(lintSIndex).strName = lStrataA(lintSIndex).strName & " " & .strName & " " & .straTerms(j)
                    LoadPlotData(lstrQuery, lstrSortOrder, lintSIndex, lStrataA)
                    lintSIndex = lintSIndex + 1
                Next
            End With
            Exit Sub
        End If

        With lSVarArray(lintLevel)
            For j = 1 To .iTerms
                If lintLevel = 1 Then
                    If .straTerms(j) = "" Then
                        lstrQuery = "[" & .strName & "] = " & System.DBNull.Value & " AND [" & .strName & "] = " & Chr(34) & Chr(34)
                    Else
                        lstrOther = "[" & .strName & "] = " & Wrap(.straTerms(j)) & " "
                    End If
                Else
                    If .straTerms(j) = "" Then
                        lstrQuery = "AND [" & .strName & "] = " & System.DBNull.Value & " AND [" & .strName & "] = " & Chr(34) & Chr(34)
                    Else
                        lstrOther = " AND [" & .strName & "] = " & Wrap(.straTerms(j)) & " "
                    End If
                End If
                lStrataA(lintSIndex).strName = lStrataA(lintSIndex).strName & " " & .strName & " " & .straTerms(j)
                StrataRecurseWOC(lstrbase & lstrOther, lstrSortOrder, lintLevel + 1, lintSIndex, lSVarArray, lStrataA)
            Next
        End With
    End Sub

    Private Sub mMatrixlikelihood_CalcLikelihood(ByRef lintOffset As Integer, ByRef ldblA As System.Array, ByRef ldblaB As System.Array, ByRef ldblaJacobian As System.Array, ByRef ldblaF As System.Array, ByRef nRows As Integer, ByRef likelihood As Double, ByRef strError As String, ByRef booStartAtZero As Boolean) Handles mMatrixlikelihood.CalcLikelihood
        Dim i As Integer
        likelihood = 0

        ' added by Eric Fontaine 07/28/03: a value of "" means no error
        strError = ""

        If UBound(mstraTimeDependentVar) > 0 Then
            For i = 0 To UBound(mStrataA)
                With mStrataA(i)
                    likelihood = likelihood + TimeDependentLikelihood(lintOffset, .dblaData, ldblaJacobian, ldblaB, ldblaF, .lintrows, .lintcols, .mdblaTime, .intaDataColumns, .intaTimeSelectors)
                End With
            Next i
        Else
            For i = 0 To UBound(mStrataA)
                'ToDo: den4: Resolve potential error in Epi 3 code...BreslowLikelihood(...Jacobian, ldblaB(), ...)
                '  does not agree with BreslowLikelihood definition which calls for (...ldblaB(), Jacobian, ...)
                '  Jacobian and B() are switched in order.
                likelihood = likelihood + BreslowLikelihood(lintOffset, mStrataA(i).dblaData, ldblaJacobian, ldblaB, ldblaF, mStrataA(i).lintrows, mStrataA(i).lintcols)
            Next i
        End If

    End Sub

End Class