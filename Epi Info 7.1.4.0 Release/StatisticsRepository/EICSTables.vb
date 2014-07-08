Option Strict Off
Option Explicit On
Option Compare Text
Public Class ComplexSampleTables
    Implements EpiInfo.Plugin.IAnalysisStatistic
    '************************************************************************************
    '*
    '* clsCTables.cls Source File
    '*
    '* DESCRIPTION:
    '* This class contains the functions used to perform the complex table calculations.
    '*
    '*
    '* Algorithms by W.D. Kalsbeek
    '* Written by:   Mario Chin (UNC)
    '*               D. Chris Smith (CDC)
    '*
    '* REMARKS/CHANGE HISTORY:
    '* 16-OCT-2000 - Cass Pallansch, AditTech Consultants
    '*      Ported the original Turbo Pascal code into Visual Basic.  Original
    '*      Turbo Pascal included as comments where appropriate.    
    '*
    '*
    '* Centers for Disease Control and Prevention
    '*
    '* This source is public domain.  It should be made freely available
    '* to users of any software whose creation is wholly or partly dependent
    '* on this file.
    '*
    '************************************************************************************



#Const USE_ACCUMQHACOM = False

    Private Const Heading As String = "CTABLES COMPLEX SAMPLE DESIGN ANALYSIS"
    Private Const UNAVAILABLE As String = "----"
    Private Const VERTICALPERCENT As String = "<TLT>Col %</TLT>"
    Private Const HORIZONTALPERCENT As String = "<TLT>Row %</TLT>"

    Private strataVar As String
    Private mainVar As String
    Private domainVar As String
    Private domainVal As String
    Private psuVar As String
    Private weightVar As String
    Private columnNames As List(Of String)
    Private vntLabels As List(Of String)

    Private validCases As Int32

    Private tableName As String
    Private booleanLabels As String
    Private outputLevel As Int32
    Private percents As Boolean
    Private booleanValues As Int32

    Private sortedTable As DataTable
    Private distinctTable As DataTable

    Private com As Boolean
    Private domain1 As Object
    Private domain2 As Object

    Private outcome As CSField
    Private domain As CSField
    Private strata As CSField
    Private psu As CSField
    Private weight As CSField

    Private PTotal As New CSTotal
    Private PDom As New CSDomain
    Private PCat As New CSCategory

    Private FirstDom As CSDomain
    Private LastDom As CSDomain
    Private TotalDom As CSDomain
    Private FirstCat As CSCategory
    Private LastCat As CSCategory
    Private Mis As Short

    Private vntResultsArray(,) As Object
    Private cvntLabels() As String
    Private cnIsBoolean As Short
    Private Pdm As CSDomain
    Private T22 As Boolean
    Private csOutputBuffer As String
    Private cnOutputLevel As Short
    Private cbIncludePercents As Boolean
    Private cbStandalone As Boolean
    Private currentTable As DataTable

    Dim Row As Int32
    Dim numErrors As UShort    
    Private isDeleted As Boolean
    Private isVerified As Boolean

    Private varianceMultiplier As Double

    Private context As EpiInfo.Plugin.IAnalysisStatisticContext

    Private tablesResults As New CSTablesResults

    Private columnPercents As List(Of Double) ' should not need this, but we do...

    Private identifiers As String()

    Private Outputtable As DataTable

    Dim datarow As DataRow
    Dim exposurevar As String
    Dim exposureoutvar As String
    Private OuttableName As String

    'Private Const W = 11
    'Private Const DecimalPoint = 3
#Const USE_QUERY = True

    Public Structure CSRow
        Public Value As String
        Public Domain As String
        Public Count As Double
        Public RowPercent As Double
        Public ColPercent As Double
        Public SE As Double
        Public LCL As Double
        Public UCL As Double
        Public DesignEffect As Decimal
    End Structure

    Public Structure CSFrequencyResults
        Public Rows As List(Of CSRow)
        Public ErrorMessage As String
    End Structure


    Public Structure TablesRow
        Public Cells As List(Of CSRow)
        Public RowColPercent As Nullable(Of Double)
    End Structure

    Public Structure CSTablesResults
        Public Rows As List(Of TablesRow)

        Public OddsRatio As Nullable(Of Decimal)
        Public StandardErrorOR As Nullable(Of Double)
        Public LCLOR As Nullable(Of Double)
        Public UCLOR As Nullable(Of Double)

        Public RiskRatio As Nullable(Of Decimal)
        Public StandardErrorRR As Nullable(Of Double)
        Public LCLRR As Nullable(Of Double)
        Public UCLRR As Nullable(Of Double)

        Public RiskDifference As Nullable(Of Double)
        Public StandardErrorRD As Nullable(Of Double)
        Public LCLRD As Nullable(Of Double)
        Public UCLRD As Nullable(Of Double)

        Public ErrorMessage As String
    End Structure

    Public Function ComplexSampleFrequencies(ByVal inputVariableList As Dictionary(Of String, String), ByVal dataTable As DataTable) As CSFrequencyResults

        currentTable = dataTable

        CreateSettings(inputVariableList)

        Dim errorMessage As String
        errorMessage = String.Empty

        Dim output As String

        Dim csFrequencyResults As New CSFrequencyResults
        csFrequencyResults.ErrorMessage = String.Empty
        csFrequencyResults.Rows = New List(Of CSRow)

        If Init(errorMessage) = False Then
            csFrequencyResults.ErrorMessage = errorMessage
            Return csFrequencyResults 'Exit Function
        End If

        On Error GoTo ErrorHandler

        Mis = 0
        FirstDom = Nothing
        LastDom = Nothing
        TotalDom = Nothing
        FirstCat = Nothing
        LastCat = Nothing
        Pdm = Nothing
        Dim result As Integer

        If com Then
            errorMessage = String.Empty
            result = FirstPassCom(errorMessage)
            If Not String.IsNullOrEmpty(errorMessage) Then
                csFrequencyResults.ErrorMessage = errorMessage
                Return csFrequencyResults 'Exit Function
            End If
        Else
            errorMessage = String.Empty
            result = FirstPass(errorMessage)
            If Not String.IsNullOrEmpty(errorMessage) Then
                csFrequencyResults.ErrorMessage = errorMessage
                Return csFrequencyResults 'Exit Function
            End If
        End If

        If (result = MsgBoxResult.Ok) Then

            If FirstDom.NextDom Is LastDom Then
                com = True
                domain1 = FirstDom.Domain
                domain2 = LastDom.Domain
            End If

            If (Not domain Is Nothing) Then
                ComputeTot(errorMessage)
                If Not String.IsNullOrEmpty(errorMessage) Then
                    csFrequencyResults.ErrorMessage = errorMessage
                    Return csFrequencyResults 'Exit Function
                End If
            End If

            ResetReader()

            result = SecondPass(errorMessage)
            If Not String.IsNullOrEmpty(errorMessage) Then
                csFrequencyResults.ErrorMessage = errorMessage
                Return csFrequencyResults 'Exit Function
            End If

        End If

        vntResultsArray = ResultsArray()

        Dim vntOutTable(,) As Object
        vntOutTable = vntResultsArray(1, 3)

        For i = 0 To UBound(vntOutTable, 1)
            Dim fRow As CSRow
            fRow = New CSRow()
            fRow.Value = vntOutTable(i, 0)
            fRow.Domain = vntOutTable(i, 1)
            fRow.Count = vntOutTable(i, 2)
            fRow.RowPercent = vntOutTable(i, 3)
            fRow.ColPercent = vntOutTable(i, 4)
            fRow.SE = vntOutTable(i, 5)
            fRow.LCL = vntOutTable(i, 6)
            fRow.UCL = vntOutTable(i, 7)
            fRow.DesignEffect = vntOutTable(i, 8)
            csFrequencyResults.Rows.Add(fRow)
        Next

        'output = csOutputBuffer

        Return csFrequencyResults

ErrorHandler:

        errorMessage = Err.Description
        csFrequencyResults.ErrorMessage = errorMessage
        Return csFrequencyResults 'Exit Function

    End Function

    Public Function ComplexSampleTables(ByVal inputVariableList As Dictionary(Of String, String), ByVal dataTable As DataTable) As CSTablesResults

        currentTable = dataTable

        CreateSettings(inputVariableList)

        Dim errorMessage As String
        errorMessage = String.Empty

        Dim output As String

        'Dim csTablesResults As New CSTablesResults
        columnPercents = New List(Of Double)
        tablesResults = New CSTablesResults
        tablesResults.ErrorMessage = String.Empty
        tablesResults.Rows = New List(Of TablesRow)

        If Init(errorMessage) = False Then
            tablesResults.ErrorMessage = errorMessage
            Return tablesResults 'Exit Function
        End If

        On Error GoTo ErrorHandler

        Mis = 0
        FirstDom = Nothing
        LastDom = Nothing
        TotalDom = Nothing
        FirstCat = Nothing
        LastCat = Nothing
        Pdm = Nothing
        Dim result As Integer

        If com Then
            errorMessage = String.Empty
            result = FirstPassCom(errorMessage)
            If Not String.IsNullOrEmpty(errorMessage) Then
                tablesResults.ErrorMessage = errorMessage
                Return tablesResults 'Exit Function
            End If
        Else
            errorMessage = String.Empty
            result = FirstPass(errorMessage)
            If Not String.IsNullOrEmpty(errorMessage) Then
                tablesResults.ErrorMessage = errorMessage
                Return tablesResults 'Exit Function
            End If
        End If

        If (result = MsgBoxResult.Ok) Then

            If FirstDom.NextDom Is LastDom Then
                com = True
                domain1 = FirstDom.Domain
                domain2 = LastDom.Domain
            End If

            If (Not domain Is Nothing) Then
                ComputeTot(errorMessage)
                If Not String.IsNullOrEmpty(errorMessage) Then
                    tablesResults.ErrorMessage = errorMessage
                    Return tablesResults 'Exit Function
                End If
            End If

            ResetReader()

            result = SecondPass(errorMessage)
            If Not String.IsNullOrEmpty(errorMessage) Then
                tablesResults.ErrorMessage = errorMessage
                Return tablesResults 'Exit Function
            End If

        End If

        'vntResultsArray = ResultsArrayWithTotals()

        vntResultsArray = ResultsArray()
        Dim vntOutTable(,) As Object
        vntOutTable = ResultsArrayWithTotals()

        If UBound(vntResultsArray, 1) > 8 And UBound(vntResultsArray, 1) <= 20 Then
            tablesResults.StandardErrorOR = vntResultsArray(1, 9)
            tablesResults.StandardErrorRR = vntResultsArray(1, 13)
            tablesResults.StandardErrorRD = vntResultsArray(1, 17)

            tablesResults.OddsRatio = vntResultsArray(1, 8)
            tablesResults.RiskRatio = vntResultsArray(1, 12)
            tablesResults.RiskDifference = vntResultsArray(1, 16)

            tablesResults.LCLOR = vntResultsArray(1, 10)
            tablesResults.LCLRR = vntResultsArray(1, 14)
            tablesResults.LCLRD = vntResultsArray(1, 18)

            tablesResults.UCLOR = vntResultsArray(1, 11)
            tablesResults.UCLRR = vntResultsArray(1, 15)
            tablesResults.UCLRD = vntResultsArray(1, 19)
        Else
            tablesResults.StandardErrorOR = Nothing
            tablesResults.StandardErrorRR = Nothing
            tablesResults.StandardErrorRD = Nothing

            tablesResults.OddsRatio = Nothing
            tablesResults.RiskRatio = Nothing
            tablesResults.RiskDifference = Nothing

            tablesResults.LCLOR = Nothing
            tablesResults.LCLRR = Nothing
            tablesResults.LCLRD = Nothing

            tablesResults.UCLOR = Nothing
            tablesResults.UCLRR = Nothing
            tablesResults.UCLRD = Nothing
        End If

        Dim currRow As String
        currRow = vntOutTable(0, 1).ToString()
        Dim tRow As TablesRow
        tRow = New TablesRow()
        tRow.Cells = New List(Of CSRow)
        tablesResults.Rows = New List(Of TablesRow)
        columnPercents.Add(100)

        Dim colPerCount As Int16
        colPerCount = 0

        For i = 0 To UBound(vntOutTable, 1)
            If Not currRow.Equals(vntOutTable(i, 1).ToString()) And i <> 0 Then
                tRow.RowColPercent = columnPercents(colPerCount)
                colPerCount = colPerCount + 1
                tablesResults.Rows.Add(tRow)
                tRow = New TablesRow()
                tRow.Cells = New List(Of CSRow)
            End If

            currRow = vntOutTable(i, 1).ToString()

            Dim fRow As CSRow
            fRow = New CSRow()
            fRow.Value = vntOutTable(i, 0)
            fRow.Domain = vntOutTable(i, 1)
            fRow.Count = vntOutTable(i, 2)

            If vntOutTable(i, 3) Is DBNull.Value Then
                fRow.RowPercent = -1
            Else
                fRow.RowPercent = vntOutTable(i, 3)
            End If

            If vntOutTable(i, 4) Is DBNull.Value Then
                fRow.ColPercent = -1
            Else
                fRow.ColPercent = vntOutTable(i, 4)
            End If

            fRow.SE = vntOutTable(i, 5)

            If vntOutTable(i, 6) Is DBNull.Value Then
                fRow.LCL = -1
            Else
                fRow.LCL = vntOutTable(i, 6)
            End If

            If vntOutTable(i, 7) Is DBNull.Value Then
                fRow.UCL = -1
            Else
                fRow.UCL = vntOutTable(i, 7)
            End If

            fRow.DesignEffect = vntOutTable(i, 8)
            tRow.Cells.Add(fRow)
            'tRow.RowColPercent = columnPercents(i)

            If i = UBound(vntOutTable, 1) Then
                tRow.RowColPercent = columnPercents(colPerCount)
                colPerCount = colPerCount + 1
                tablesResults.Rows.Add(tRow)
            End If
        Next

        'output = csOutputBuffer

        Return tablesResults

ErrorHandler:

        errorMessage = Err.Description
        tablesResults.ErrorMessage = errorMessage
        Return tablesResults 'Exit Function

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
        If inputVariableList.ContainsKey("IdentifierList") Then
            Dim identifiers As String()
            identifiers = inputVariableList("IdentifierList").Split(",")
            mainVar = identifiers(0)
        End If

        'If context.SetProperties.ContainsKey("TableName") Then
        '    tableName = context.SetProperties("TableName")
        'End If

        'If context.SetProperties.ContainsKey("BLabels") Then
        '    booleanLabels = context.SetProperties("BLabels")
        'End If

        'If context.SetProperties.ContainsKey("IsBoolean") Then
        '    booleanValues = context.SetProperties("IsBoolean")
        'End If

        For Each kvp As KeyValuePair(Of String, String) In inputVariableList
            If kvp.Key.ToLower().Equals("percents") Then
                percents = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("stratavar") Or kvp.Key.ToLower().Equals("stratvarlist") Then
                strataVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("exposure_variable") Or kvp.Key.ToLower().Equals("mainvar") Or kvp.Key.ToLower().Equals("identifier") Or kvp.Key.ToLower().Equals("identifier1") Or kvp.Key.ToLower().Equals("exposurevar") Then
                domainVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("outcome_variable") Or kvp.Key.ToLower().Equals("crosstabvar") Or kvp.Key.ToLower().Equals("identifier2") Or kvp.Key.ToLower().Equals("outcomevar") Then
                mainVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("psuvar") Then
                psuVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("weightvar") Then
                weightVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("tablename") Then
                tableName = kvp.Value
            End If
        Next

        vntLabels = New List(Of String)
        vntLabels.Add(String.Empty)
        vntLabels.Add(String.Empty)
        vntLabels.Add(String.Empty)

        ' TODO: Get from configuration
        vntLabels(0) = "Yes" 'context.SetProperties("RepresentationOfYes")
        vntLabels(1) = "No" 'context.SetProperties("RepresentationOfNo")
        vntLabels(2) = "Missing" 'context.SetProperties("RepresentationOfMissing")

        cnOutputLevel = outputLevel
        cbIncludePercents = percents

cleanup:
        Exit Sub

Errorhandler:
        'errorMessage = Err.Description
        'numErrors = numErrors + 1
    End Sub

    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Complex Sample Tables"
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
        'If context.InputVariableList.ContainsKey("IdentifierList") Then
        '    Dim identifiers As String()
        '    identifiers = context.InputVariableList("IdentifierList").Split(",")
        '    mainVar = identifiers(0)
        'End If

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
                Dim stratavars As String()
                stratavars = Split(kvp.Value, ",")
                Dim i As Int16
                For i = 0 To stratavars.Length - 1
                    strataVar = stratavars(i)
                Next
            End If

            If kvp.Key.ToLower().Equals("exposure_variable") Or kvp.Key.ToLower().Equals("mainvar") Or kvp.Key.ToLower().Equals("identifier") Or kvp.Key.ToLower().Equals("identifier1") Or kvp.Key.ToLower().Equals("exposurevar") Then
                domainVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("outcome_variable") Or kvp.Key.ToLower().Equals("crosstabvar") Or kvp.Key.ToLower().Equals("identifier2") Or kvp.Key.ToLower().Equals("outcomevar") Then
                mainVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("psuvar") Then
                psuVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("weightvar") Then
                weightVar = kvp.Value
            End If

            If kvp.Key.ToLower().Equals("tablename") Then
                tableName = kvp.Value
            End If
        Next

        vntLabels = New List(Of String)
        vntLabels.Add(String.Empty)
        vntLabels.Add(String.Empty)
        vntLabels.Add(String.Empty)

        vntLabels(0) = context.SetProperties("RepresentationOfYes")
        vntLabels(1) = context.SetProperties("RepresentationOfNo")
        vntLabels(2) = context.SetProperties("RepresentationOfMissing")

        cnOutputLevel = outputLevel
        cbIncludePercents = percents

cleanup:
        Exit Sub

Errorhandler:
        'errorMessage = Err.Description
        'numErrors = numErrors + 1

    End Sub

    Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
    End Sub

    Public Sub DisplayError(ByRef args As Dictionary(Of String, String), ByRef errorMessage As String)
        Dim output As String
        output = String.Empty

        ReDim Results(1, 0)
        Results(0, 0) = "ERROR"
        Results(1, 0) = errorMessage
        output = "<br clear=""all"" /><p align=""left""><b><tlt>" + errorMessage + "</tlt></b></p>"

        args.Add("COMMANDNAME", "TABLES")
        args.Add("COMMANDTEXT", context.InputVariableList("commandText"))
        args.Add("HTMLRESULTS", output)
        context.Display(args)
    End Sub



    Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute


        Dim errorMessage As String
        errorMessage = String.Empty
        Dim output As String
        Dim args As Dictionary(Of String, String)
        mainVar = String.Empty

        If context.InputVariableList.ContainsKey("IdentifierList") Then
            identifiers = context.InputVariableList("IdentifierList").Split(",")
        End If
        If context.InputVariableList.ContainsKey("OutTable") Then
            OuttableName = context.InputVariableList("OutTable")
        End If
        If identifiers Is Nothing Then
            args = New Dictionary(Of String, String)
            CreateSettingsFromContext()
            If Init(errorMessage) = False Then
                DisplayError(args, errorMessage)
                Exit Sub
            End If

            On Error GoTo ErrorHandler

            Mis = 0
            FirstDom = Nothing
            LastDom = Nothing
            TotalDom = Nothing
            FirstCat = Nothing
            LastCat = Nothing
            Pdm = Nothing
            Dim result As Integer

            If com Then
                errorMessage = String.Empty
                result = FirstPassCom(errorMessage)
                If Not String.IsNullOrEmpty(errorMessage) Then
                    DisplayError(args, errorMessage)
                    Exit Sub
                End If
            Else
                errorMessage = String.Empty
                result = FirstPass(errorMessage)
                If Not String.IsNullOrEmpty(errorMessage) Then
                    DisplayError(args, errorMessage)
                    Exit Sub
                End If
            End If

            If (result = MsgBoxResult.Ok) Then

                If FirstDom.NextDom Is LastDom Then
                    com = True
                    domain1 = FirstDom.Domain
                    domain2 = LastDom.Domain
                End If

                If (Not domain Is Nothing) Then
                    ComputeTot(errorMessage)
                    If Not String.IsNullOrEmpty(errorMessage) Then
                        DisplayError(args, errorMessage)
                        Exit Sub
                    End If
                End If

                If Not OuttableName Is Nothing Then
                    Outputtable = New DataTable
                    Outputtable.TableName = OuttableName

                    If (Not domain Is Nothing) Then
                        exposurevar = domain.FieldLabel
                    End If
                    exposureoutvar = outcome.FieldLabel

                    If Not exposureoutvar Is Nothing Then
                        Outputtable.Columns.Add(exposureoutvar, Type.GetType("System.Double"))

                    End If
                    If Not exposurevar Is Nothing Then
                        Outputtable.Columns.Add(exposurevar, Type.GetType("System.Double"))

                    End If
                    Outputtable.Columns.Add("COUNT", Type.GetType("System.Double"))
                    Outputtable.Columns.Add("RowPct", Type.GetType("System.Double"))
                    Outputtable.Columns.Add("ColPct", Type.GetType("System.Double"))
                    Outputtable.Columns.Add("StdErr", Type.GetType("System.Double"))
                    Outputtable.Columns.Add("LCL", Type.GetType("System.Double"))
                    Outputtable.Columns.Add("UCL", Type.GetType("System.Double"))
                    Outputtable.Columns.Add("DesignEff", Type.GetType("System.Double"))
                End If
                ResetReader()

                result = SecondPass(errorMessage)
                If Not String.IsNullOrEmpty(errorMessage) Then
                    DisplayError(args, errorMessage)
                    Exit Sub
                End If

            End If

            output = csOutputBuffer

            args.Add("COMMANDNAME", "MEANS")

            args.Add("COMMANDTEXT", context.InputVariableList("commandText"))
            args.Add("HTMLRESULTS", output)

            context.Display(args)
            If Not OuttableName Is Nothing And Not Outputtable Is Nothing Then
                context.OutTable(Outputtable)
            End If
        Else
            If Not OuttableName Is Nothing Then
                Outputtable = New DataTable
                Outputtable.TableName = OuttableName
                If Not identifiers Is Nothing Then
                    For Each var In identifiers
                        Outputtable.Columns.Add(var, Type.GetType("System.Double"))
                    Next
                End If
                Outputtable.Columns.Add("VARNAME", Type.GetType("System.String"))
                Outputtable.Columns.Add("COUNT", Type.GetType("System.Int32"))
                Outputtable.Columns.Add("RowPct", Type.GetType("System.Double"))
                Outputtable.Columns.Add("ColPct", Type.GetType("System.Double"))
                Outputtable.Columns.Add("StdErr", Type.GetType("System.Double"))
                Outputtable.Columns.Add("LCL", Type.GetType("System.Double"))
                Outputtable.Columns.Add("UCL", Type.GetType("System.Double"))
                Outputtable.Columns.Add("DesignEff", Type.GetType("System.Double"))

            End If
            For Each id In identifiers

                args = New Dictionary(Of String, String)
                mainVar = id
                CreateSettingsFromContext()

                If Init(errorMessage) = False Then
                    DisplayError(args, errorMessage)
                    Exit Sub
                End If

                On Error GoTo ErrorHandler

                Mis = 0
                FirstDom = Nothing
                LastDom = Nothing
                TotalDom = Nothing
                FirstCat = Nothing
                LastCat = Nothing
                Pdm = Nothing
                Dim result As Integer

                If com Then
                    errorMessage = String.Empty
                    result = FirstPassCom(errorMessage)
                    If Not String.IsNullOrEmpty(errorMessage) Then
                        DisplayError(args, errorMessage)
                        Exit Sub
                    End If
                Else
                    errorMessage = String.Empty
                    result = FirstPass(errorMessage)
                    If Not String.IsNullOrEmpty(errorMessage) Then
                        DisplayError(args, errorMessage)
                        Exit Sub
                    End If
                End If

                If (result = MsgBoxResult.Ok) Then

                    If FirstDom.NextDom Is LastDom Then
                        com = True
                        domain1 = FirstDom.Domain
                        domain2 = LastDom.Domain
                    End If

                    If (Not domain Is Nothing) Then
                        ComputeTot(errorMessage)
                        If Not String.IsNullOrEmpty(errorMessage) Then
                            DisplayError(args, errorMessage)
                            Exit Sub
                        End If
                    End If

                    ResetReader()

                    result = SecondPass(errorMessage)
                    If Not String.IsNullOrEmpty(errorMessage) Then
                        DisplayError(args, errorMessage)
                        Exit Sub
                    End If

                End If

                output = csOutputBuffer

                args.Add("COMMANDNAME", "MEANS")

                args.Add("COMMANDTEXT", context.InputVariableList("commandText"))
                args.Add("HTMLRESULTS", output)

                context.Display(args)
                If Not OuttableName Is Nothing And Not Outputtable Is Nothing Then
                    context.OutTable(Outputtable)
                End If
                csOutputBuffer = Nothing
            Next
        End If
        Exit Sub

ErrorHandler:

        errorMessage = Err.Description
        DisplayError(args, errorMessage)

    End Sub

    Private Function Init(ByRef errorMessage As String) As Boolean

        Dim numRows As Int32
        Dim numCats As Int32
        Dim numStrata As Int32

        Dim isDeleted As Boolean
        Dim isVerified As Boolean
        Dim columnNamesArray() As String

        On Error GoTo ErrorhandlEr

        validCases = 0 ' REMOVE

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
            strata.FieldLabel = "None" 'STRATA_VAR"
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

        If Not String.IsNullOrEmpty(domainVar) Then
            columnNames.Add(domainVar)
            domain = New CSField()
            domain.FieldLabel = domainVar
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

        If Not String.IsNullOrEmpty(mainVar) Then
            sortClause = sortClause & ", " & mainVar
        End If

        If Not String.IsNullOrEmpty(domainVar) Then
            sortClause = sortClause & ", " & domainVar
        End If

        If Not String.IsNullOrEmpty(weightVar) Then
            sortClause = sortClause & ", " & weightVar
        End If

        Row = 0

        Dim whereClause = mainVar + " is not null and " + psuVar + " is not null"
        If Not String.IsNullOrEmpty(weightVar) Then
            whereClause = whereClause + " and " + weightVar + " is not null"
        End If
        If Not String.IsNullOrEmpty(strataVar) Then
            whereClause = whereClause + " and " + strataVar + " is not null"
        End If

        Dim unsortedTable = New DataTable()
        If currentTable Is Nothing Then
            unsortedTable = context.GetDataRows(Nothing).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)
        Else
            unsortedTable = currentTable
        End If
        'unsortedTable = currentTable 'context.GetDataRows(Nothing).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)
        sortedTable = unsortedTable.Select(whereClause, sortClause).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)

        numRows = sortedTable.Rows.Count

        'sortedTable = context.CurrentDataTable.Select(whereClause, sortClause).CopyToDataTable().DefaultView.ToTable(tableName, False, columnNamesArray)
        'numRows = sortedTable.Rows.Count

        If Not String.IsNullOrEmpty(mainVar) Then
            outcome.FieldEntry = sortedTable.Rows(Row)(mainVar)
        End If

        If Not String.IsNullOrEmpty(strataVar) Then
            strata.FieldEntry = sortedTable.Rows(Row)(strataVar)
        End If

        If Not String.IsNullOrEmpty(weightVar) Then
            weight.FieldEntry = sortedTable.Rows(Row)(weightVar)
        End If

        If Not String.IsNullOrEmpty(psuVar) Then
            psu.FieldEntry = sortedTable.Rows(Row)(psuVar)
        End If

        If Not String.IsNullOrEmpty(domainVar) Then
            domain.FieldEntry = sortedTable.Rows(Row)(domainVar)
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

ErrorhandlEr:
        errorMessage = Err.Description
        'Err.Raise(vbObjectError + 234, , Err.Description)
        Init = False

    End Function


    Private Function GetNextRow() As Boolean

        If sortedTable.Rows.Count = row Then
            Return False
        End If

        On Error GoTo ErrorHandler

        If Not String.IsNullOrEmpty(strataVar) Then
            strata.FieldEntry = sortedTable.Rows(Row)(strataVar)
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
            weight.FieldEntry = sortedTable.Rows(Row)(weightVar)
            If weight.FieldEntry.ToString().Length <= 0 Then
                weight.Missing = True
            Else
                weight.Missing = False
            End If
        End If

        If Not String.IsNullOrEmpty(mainVar) Then
            outcome.FieldEntry = sortedTable.Rows(Row)(mainVar)
            If outcome.FieldEntry.ToString().Length <= 0 Then
                outcome.Missing = True
            Else
                outcome.Missing = False
            End If
        End If

        If Not String.IsNullOrEmpty(domainVar) Then
            domain.FieldEntry = sortedTable.Rows(Row)(domainVar)
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

        isDeleted = False
        isVerified = True

        row = row + 1

        Return True

        Exit Function

ErrorHandler:

        'GetNext = retEOF
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)
        Return False

    End Function

    Public Function ResultsArrayWithTotals() As Object

        Const PROC_Name As String = "clsCTables::ResultsArrayWithTotals"

        Dim vntOutTable(,) As Object
        Dim Pc As CSCategory
        Dim Pd As CSDomain
        Dim P As CSTotal
        Dim Total As CSTotal
        Dim Pct As Object
        Dim varDE As Object
        Dim Cl As Object
        Dim nCat As Integer
        Dim nDom As Integer
        Dim nRows As Integer
        Dim nCurrentRow As Integer

        On Error GoTo ErrorHandler

        If (outcome Is Nothing) Then

            ReDim vntOutTable(0, 0)
            'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
            'UPGRADE_WARNING: Couldn't resolve default property of object vntOutTable(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntOutTable(0, 0) = System.DBNull.Value

        ElseIf (Not weight Is Nothing) And (strata Is Nothing) And (psu Is Nothing) Then

            ReDim vntOutTable(0, 0)
            vntOutTable(0, 0) = System.DBNull.Value

        Else

            '
            ' Find out the number of rows based on number of values in the
            ' domain minus one for the "Total" values.
            '
            Pd = FirstDom
            nCat = 0 '{ count the categories }
            P = Pd.FirstCat
            While (Not P Is Nothing)
                nCat = nCat + 1
                P = P.NextCat
            End While

            'If (Pd Is TotalDom) Then
            '    nDom = 1
            'Else
            nDom = 0
            While Not Pd Is Nothing 'And Not Pd Is TotalDom
                nDom = nDom + 1
                Pd = Pd.NextDom
            End While
            'End If

            nRows = nCat * nDom

            '
            ' Dimension the results array and populate the values.
            '
            ReDim vntOutTable(nRows - 1, 8)

            Pd = FirstDom
            nCurrentRow = 0 '1

            If (Pd Is TotalDom) Then

                P = Pd.FirstCat
                While Not P Is Nothing

                    vntOutTable(nCurrentRow, 0) = P.Category
                    vntOutTable(nCurrentRow, 1) = Nothing

                    vntOutTable(nCurrentRow, 2) = P.N

                    If nDom = 1 Then ' Only have the Totals to display.  Switch Row/Col and make Row % = 100%

                        '
                        ' Row %
                        '
                        vntOutTable(nCurrentRow, 3) = 100.0#

                        '
                        ' Col %
                        '
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            vntOutTable(nCurrentRow, 4) = 100 * (P.YE / Pd.SumW)
                        Else
                            vntOutTable(nCurrentRow, 4) = System.DBNull.Value
                        End If

                    Else

                        '
                        ' Row %
                        '
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            vntOutTable(nCurrentRow, 3) = 100 * (P.YE / Pd.SumW)
                        Else
                            vntOutTable(nCurrentRow, 3) = System.DBNull.Value
                        End If

                        '
                        ' Col %
                        '
                        Total = P
                        While (Not Total.NextDom Is Nothing)
                            Total = Total.NextDom
                        End While
                        If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                            vntOutTable(nCurrentRow, 4) = 100 * (P.YE / Total.YE)
                        Else
                            vntOutTable(nCurrentRow, 4) = System.DBNull.Value
                        End If

                    End If

                    '
                    ' Standard Error
                    '
                    If P.VarT >= 0.0# Then
                        vntOutTable(nCurrentRow, 5) = 100 * System.Math.Sqrt(P.VarT)
                    Else
                        vntOutTable(nCurrentRow, 5) = System.DBNull.Value
                    End If

                    '
                    ' Lower/Upper Confidence Limits
                    '
                    Pct = 0.0#
                    If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                        Pct = 100 * (P.YE / Pd.SumW)
                    End If

                    If (Pct > 0.0#) Then
                        vntOutTable(nCurrentRow, 6) = 100 * ((P.YE / Pd.SumW) + (-varianceMultiplier * Math.Sqrt((P.VarT))))
                        vntOutTable(nCurrentRow, 7) = 100 * ((P.YE / Pd.SumW) + (varianceMultiplier * Math.Sqrt((P.VarT))))
                    Else
                        vntOutTable(nCurrentRow, 6) = System.DBNull.Value
                        vntOutTable(nCurrentRow, 7) = System.DBNull.Value
                    End If

                    '
                    ' Design Effect
                    '
                    varDE = DesignEffect(Pd)
                    If varDE <> CDec(-1.0#) Then
                        vntOutTable(nCurrentRow, 8) = DesignEffect(Pd)
                    Else
                        vntOutTable(nCurrentRow, 8) = System.DBNull.Value
                    End If

                    P = P.NextCat
                    nCurrentRow = nCurrentRow + 1

                End While

            Else

                While Not Pd Is Nothing 'And Not Pd Is TotalDom

                    P = Pd.FirstCat
                    While (Not P Is Nothing)

                        vntOutTable(nCurrentRow, 0) = P.Category
                        vntOutTable(nCurrentRow, 1) = Pd.Domain

                        vntOutTable(nCurrentRow, 2) = P.N

                        '
                        ' Row %
                        '
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            vntOutTable(nCurrentRow, 3) = 100 * (P.YE / Pd.SumW)
                        Else
                            vntOutTable(nCurrentRow, 3) = System.DBNull.Value
                        End If

                        '
                        ' Col %
                        '
                        Total = P
                        While (Not Total.NextDom Is Nothing)
                            Total = Total.NextDom
                        End While
                        If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                            vntOutTable(nCurrentRow, 4) = 100 * (P.YE / Total.YE)
                        Else
                            vntOutTable(nCurrentRow, 4) = System.DBNull.Value
                        End If

                        '
                        ' Standard Error
                        '
                        If P.VarT >= 0.0# Then
                            vntOutTable(nCurrentRow, 5) = 100 * Math.Sqrt(P.VarT)
                        Else
                            vntOutTable(nCurrentRow, 5) = System.DBNull.Value
                        End If

                        '
                        ' Lower/Upper Confidence Limits
                        '
                        Pct = 0.0#
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            Pct = 100 * (P.YE / Pd.SumW)
                        End If

                        If (Pct > 0.0#) Then
                            vntOutTable(nCurrentRow, 6) = 100 * ((P.YE / Pd.SumW) + (-varianceMultiplier * Math.Sqrt(P.VarT)))
                            vntOutTable(nCurrentRow, 7) = 100 * ((P.YE / Pd.SumW) + (varianceMultiplier * Math.Sqrt(P.VarT)))
                        Else
                            vntOutTable(nCurrentRow, 6) = System.DBNull.Value
                            vntOutTable(nCurrentRow, 7) = System.DBNull.Value
                        End If

                        '
                        ' Design Effect
                        '
                        varDE = DesignEffect(Pd)
                        If varDE <> CDec(-1.0#) Then
                            vntOutTable(nCurrentRow, 8) = DesignEffect(Pd)
                        Else
                            vntOutTable(nCurrentRow, 8) = System.DBNull.Value
                        End If

                        P = P.NextCat
                        nCurrentRow = nCurrentRow + 1

                    End While

                    Pd = Pd.NextDom

                End While

            End If

        End If

        Return vntOutTable

ErrorHandler:

        ResultsArrayWithTotals = Nothing
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function



    Public Function ResultsArray() As Object

        Const PROC_Name As String = "clsCTables::ResultsArray"

        Dim vntOutTable(,) As Object
        Dim vntVarNames(9) As Object
        Dim vntVarPrompts(9) As Object
        Dim Pc As CSCategory
        Dim Pd As CSDomain
        Dim P As CSTotal
        Dim Total As CSTotal
        Dim Pct As Object
        Dim varDE As Object
        Dim Cl As Object
        Dim nCat As Integer
        Dim nDom As Integer
        Dim nRows As Integer
        Dim nCurrentRow As Integer

        On Error GoTo ErrorHandler

        If (outcome Is Nothing) Then

            ReDim vntOutTable(0, 0)
            'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
            'UPGRADE_WARNING: Couldn't resolve default property of object vntOutTable(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntOutTable(0, 0) = System.DBNull.Value

        ElseIf (Not weight Is Nothing) And (strata Is Nothing) And (psu Is Nothing) Then

            ReDim vntOutTable(0, 0)
            vntOutTable(0, 0) = System.DBNull.Value

        Else

            '
            ' Find out the number of rows based on number of values in the
            ' domain minus one for the "Total" values.
            '
            Pd = FirstDom
            nCat = 0 '{ count the categories }
            P = Pd.FirstCat
            While (Not P Is Nothing)
                nCat = nCat + 1
                P = P.NextCat
            End While

            If (Pd Is TotalDom) Then
                nDom = 1
            Else
                nDom = 0
                While Not Pd Is Nothing And Not Pd Is TotalDom
                    nDom = nDom + 1
                    Pd = Pd.NextDom
                End While
            End If

            nRows = nCat * nDom

            '
            ' Dimension the results array and populate the values.
            '
            ReDim vntOutTable(nRows - 1, 8)

            Pd = FirstDom
            nCurrentRow = 0 '1

            If (Pd Is TotalDom) Then

                P = Pd.FirstCat
                While Not P Is Nothing

                    vntOutTable(nCurrentRow, 0) = P.Category
                    vntOutTable(nCurrentRow, 1) = Nothing

                    vntOutTable(nCurrentRow, 2) = P.N

                    If nDom = 1 Then ' Only have the Totals to display.  Switch Row/Col and make Row % = 100%

                        '
                        ' Row %
                        '
                        vntOutTable(nCurrentRow, 3) = 100.0#

                        '
                        ' Col %
                        '
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            vntOutTable(nCurrentRow, 4) = 100 * (P.YE / Pd.SumW)
                        Else
                            vntOutTable(nCurrentRow, 4) = System.DBNull.Value
                        End If

                    Else

                        '
                        ' Row %
                        '
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            vntOutTable(nCurrentRow, 3) = 100 * (P.YE / Pd.SumW)
                        Else
                            vntOutTable(nCurrentRow, 3) = System.DBNull.Value
                        End If

                        '
                        ' Col %
                        '
                        Total = P
                        While (Not Total.NextDom Is Nothing)
                            Total = Total.NextDom
                        End While
                        If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                            vntOutTable(nCurrentRow, 4) = 100 * (P.YE / Total.YE)
                        Else
                            vntOutTable(nCurrentRow, 4) = System.DBNull.Value
                        End If

                    End If

                    '
                    ' Standard Error
                    '
                    If P.VarT >= 0.0# Then
                        vntOutTable(nCurrentRow, 5) = 100 * System.Math.Sqrt(P.VarT)
                    Else
                        vntOutTable(nCurrentRow, 5) = System.DBNull.Value
                    End If

                    '
                    ' Lower/Upper Confidence Limits
                    '
                    Pct = 0.0#
                    If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                        Pct = 100 * (P.YE / Pd.SumW)
                    End If

                    If (Pct > 0.0#) Then
                        vntOutTable(nCurrentRow, 6) = 100 * ((P.YE / Pd.SumW) + (-varianceMultiplier * Math.Sqrt((P.VarT))))
                        vntOutTable(nCurrentRow, 7) = 100 * ((P.YE / Pd.SumW) + (varianceMultiplier * Math.Sqrt((P.VarT))))
                    Else
                        vntOutTable(nCurrentRow, 6) = System.DBNull.Value
                        vntOutTable(nCurrentRow, 7) = System.DBNull.Value
                    End If

                    '
                    ' Design Effect
                    '
                    varDE = DesignEffect(Pd)
                    If varDE <> CDec(-1.0#) Then
                        vntOutTable(nCurrentRow, 8) = DesignEffect(Pd)
                    Else
                        vntOutTable(nCurrentRow, 8) = System.DBNull.Value
                    End If

                    P = P.NextCat
                    nCurrentRow = nCurrentRow + 1

                End While

            Else

                While Not Pd Is Nothing And Not Pd Is TotalDom

                    P = Pd.FirstCat
                    While (Not P Is Nothing)

                        vntOutTable(nCurrentRow, 0) = P.Category
                        vntOutTable(nCurrentRow, 1) = Pd.Domain

                        vntOutTable(nCurrentRow, 2) = P.N

                        '
                        ' Row %
                        '
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            vntOutTable(nCurrentRow, 3) = 100 * (P.YE / Pd.SumW)
                        Else
                            vntOutTable(nCurrentRow, 3) = System.DBNull.Value
                        End If

                        '
                        ' Col %
                        '
                        Total = P
                        While (Not Total.NextDom Is Nothing)
                            Total = Total.NextDom
                        End While
                        If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                            vntOutTable(nCurrentRow, 4) = 100 * (P.YE / Total.YE)
                        Else
                            vntOutTable(nCurrentRow, 4) = System.DBNull.Value
                        End If

                        '
                        ' Standard Error
                        '
                        If P.VarT >= 0.0# Then
                            vntOutTable(nCurrentRow, 5) = 100 * Math.Sqrt(P.VarT)
                        Else
                            vntOutTable(nCurrentRow, 5) = System.DBNull.Value
                        End If

                        '
                        ' Lower/Upper Confidence Limits
                        '
                        Pct = 0.0#
                        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                            Pct = 100 * (P.YE / Pd.SumW)
                        End If

                        If (Pct > 0.0#) Then
                            vntOutTable(nCurrentRow, 6) = 100 * ((P.YE / Pd.SumW) + (-varianceMultiplier * Math.Sqrt(P.VarT)))
                            vntOutTable(nCurrentRow, 7) = 100 * ((P.YE / Pd.SumW) + (varianceMultiplier * Math.Sqrt(P.VarT)))
                        Else
                            vntOutTable(nCurrentRow, 6) = System.DBNull.Value
                            vntOutTable(nCurrentRow, 7) = System.DBNull.Value
                        End If

                        '
                        ' Design Effect
                        '
                        varDE = DesignEffect(Pd)
                        If varDE <> CDec(-1.0#) Then
                            vntOutTable(nCurrentRow, 8) = DesignEffect(Pd)
                        Else
                            vntOutTable(nCurrentRow, 8) = System.DBNull.Value
                        End If

                        P = P.NextCat
                        nCurrentRow = nCurrentRow + 1

                    End While

                    Pd = Pd.NextDom

                End While

            End If

        End If

        If (Not outcome Is Nothing) Then
            vntVarNames(1) = outcome.FieldLabel
        Else
            vntVarNames(1) = Nothing
        End If
        If (Not domain Is Nothing) Then
            vntVarNames(2) = domain.FieldLabel
        Else
            vntVarNames(2) = Nothing
        End If
        vntVarNames(3) = "Count"
        vntVarNames(4) = "RowPct"
        vntVarNames(5) = "ColPct"
        vntVarNames(6) = "StdErr"
        vntVarNames(7) = "LCL"
        vntVarNames(8) = "UCL"
        vntVarNames(9) = "DesignEff"

        If (Not outcome Is Nothing) Then
            vntVarPrompts(1) = outcome.FieldLabel
        Else
            vntVarPrompts(1) = Nothing
        End If
        If (Not domain Is Nothing) Then
            vntVarPrompts(2) = domain.FieldLabel
        Else
            vntVarPrompts(2) = Nothing
        End If
        vntVarPrompts(3) = "Count"
        vntVarPrompts(4) = "Row Percent"
        vntVarPrompts(5) = "Column Percent"
        vntVarPrompts(6) = "Standard Error Percent"
        vntVarPrompts(7) = "Lower Confidence Limit"
        vntVarPrompts(8) = "Upper Confidence Limit"
        vntVarPrompts(9) = "Design Effect"

        vntResultsArray(0, 0) = "Errors"
        vntResultsArray(1, 0) = String.Empty 'goLogFile.ErrorMessages ' TODO: Resolve

        vntResultsArray(0, 1) = "VarNames"
        vntResultsArray(1, 1) = VB6.CopyArray(vntVarNames)

        vntResultsArray(0, 2) = "VarPrompts"
        vntResultsArray(1, 2) = VB6.CopyArray(vntVarPrompts)

        vntResultsArray(0, 3) = "OutTable"
        vntResultsArray(1, 3) = VB6.CopyArray(vntOutTable)

        vntResultsArray(0, 4) = "Excluded"
        vntResultsArray(1, 4) = Mis

        vntResultsArray(0, 5) = "Weight Variable"
        If (Not weight Is Nothing) Then
            vntResultsArray(1, 5) = weight.FieldLabel
        Else
            vntResultsArray(1, 5) = "None"
        End If

        vntResultsArray(0, 6) = "PSU Variable"
        If (Not psu Is Nothing) Then
            vntResultsArray(1, 6) = psu.FieldLabel
        Else
            vntResultsArray(1, 6) = "None"
        End If

        vntResultsArray(0, 7) = "Stratification Variable"
        If (Not strata Is Nothing) Then
            vntResultsArray(1, 7) = strata.FieldLabel
        Else
            vntResultsArray(1, 7) = "None"
        End If

        ResultsArray = VB6.CopyArray(vntResultsArray)

        Exit Function

ErrorHandler:

        ResultsArray = Nothing
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub AddToOutput(ByRef sText As String)

        csOutputBuffer = csOutputBuffer & sText & vbCrLf

    End Sub

    Public ReadOnly Property OutputBuffer() As String
        Get

            OutputBuffer = csOutputBuffer

        End Get
    End Property

    Public Sub PrintDesign()

        Const PROC_Name As String = "clsTables::PrintDesign"

        '
        'procedure PrintDesign;
        'begin
        '  writeln(outfile);
        '  writeln(outfile, '  Sample Design Included:');
        '  writeln(outfile, '  -----------------------');
        '  If (Weight <> nil) Then
        '     writeln(outfile, '  Sampling Weights from ',weight^.fieldName,' field')
        '  Else
        '     writeln(outfile, '  Sampling Weights--None');
        '  If (PSU <> nil) Then
        '     writeln(outfile, '  Primary Sampling Units from ',PSU^.fieldName)
        '  Else
        '     writeln(outfile, '  PSU--None');
        '  If (Strata <> nil) Then
        '     writeln(outfile, '  Stratification from ',strata^.fieldName)
        '  Else
        '     writeln(outfile, '  Stratification--None');
        '  writeln(outfile);
        '  writeln(outfile, '  ',mis,' records with missing values');
        'end;
        '

        On Error GoTo ErrorHandler

        AddToOutput("<BR>")
        AddToOutput("<H4><TLT>Sample Design Included:</TLT></H4>")

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
        AddToOutput("<TLT>Records with missing values:</TLT> " & Str(Mis) & "<BR>")
        AddToOutput("<BR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowHeading()

        Const PROC_Name As String = "clsCTables::ShowHeading"

        '
        'procedure showHeading;
        'begin
        '     writeln(outfile);
        '     writeln(outfile, heading);
        '     writeln(outfile);
        '     write(outfile,' Analysis of ', Outcome^.fieldName);
        '     If (Domain <> nil) Then
        '        writeln(outfile,' by ',domain^.fieldName);
        '     If com Then
        '        writeln(outfile, ' Comparison between ',domain^.fieldName,' ',domain1,' and ',domain2);
        '     writeln(outfile);
        '     If (Domain <> nil) Then
        '        write(outfile,'³',pad(domain^.fieldName,W), '³');
        '     write(outfile,Outcome^.fieldName);
        '     writeln(outfile);
        '     write(outfile,'³',pad(' ',W));
        'end;
        '

        Dim sOutputString As String
        Dim nCat As Integer
        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal

        On Error GoTo ErrorHandler

        If cbStandalone Then

            AddToOutput("<HEAD><TITLE>" & Heading & "</TITLE></HEAD>")

            AddToOutput("<CENTER><H1><TLT>Heading</TLT></H1></CENTER>")
            AddToOutput("<BR>")

            If (Not domain Is Nothing) Then
                AddToOutput("<H4><TLT>Analysis of</TLT> " & outcome.FieldLabel & " : " & domain.FieldLabel & "</H4>")
            Else
                AddToOutput("<H4><TLT>Analysis of</TLT> " & outcome.FieldLabel & "</H4>")
            End If
            If com Then
                'UPGRADE_WARNING: Couldn't resolve default property of object Domain2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Domain1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                AddToOutput("<H4><TLT>Comparison between</TLT> " & domain.FieldLabel & " " & domain1 & " <TLT>and</TLT> " & domain2 & "</H4>")
            End If
            AddToOutput("<HR>")
            AddToOutput("<BR>")

        End If

        '
        ' Find out how many category values we have.
        '
        Pd = FirstDom
        nCat = 0
        P = Pd.FirstCat

        If (Not Pd Is TotalDom) Then
            While (Not P Is Nothing)
                nCat = nCat + 1
                P = P.NextCat
            End While
        End If

        AddToOutput("<TABLE BORDER=""1"">")
        AddToOutput("<TR>")
        If (Not domain Is Nothing) Then
            AddToOutput("<TH ROWSPAN = ""2"">" & domain.FieldLabel & "</TH>")
        End If
        If nCat > 0 Then
            AddToOutput("<TH COLSPAN = """ & Trim(Str(nCat + 1)) & """>" & outcome.FieldLabel & "</TH>")
        Else
            AddToOutput("<TH>" & outcome.FieldLabel & "</TH>")
            AddToOutput("<TH>TOTAL</TH>")
        End If
        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowCounts(ByRef Pd As CSDomain, Optional ByRef Total As Object = Nothing)

        Const PROC_Name As String = "clsCTables::ShowCounts"

        Dim sOutputString As String
        Dim Pc As CSTotal

        On Error GoTo ErrorHandler

        AddToOutput("<TR>")

        'UPGRADE_NOTE: IsMissing() was changed to IsNothing(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="8AE1CB93-37AB-439A-A4FF-BE3B6760BB23"'
        If IsNothing(Total) Then

            'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If cnIsBoolean > 0 And Pd.Domain.ToString() <> "TOTAL" Then
                If cnIsBoolean <> 2 Then ' cnIsBoolean is a "1" or a "3" -- show the labels.
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    If Pd.Domain = -1 Then
                        AddToOutput("<TD ALIGN=""left""><B>" & cvntLabels(1) & "</B></TD>")
                        'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    ElseIf Pd.Domain = 0 Then
                        AddToOutput("<TD ALIGN=""left""><B>" & cvntLabels(2) & "</B></TD>")
                    Else
                        'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        AddToOutput("<TD ALIGN=""left""><B>" & Pd.Domain & "</B></TD>")
                    End If
                Else
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput("<TD ALIGN=""left""><B>" & Pd.Domain & "</B></TD>")
                End If
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                AddToOutput("<TD ALIGN=""left""><B>" & Pd.Domain & "</B></TD>")
            End If

            Pc = Pd.FirstCat
            While (Not Pc Is Nothing)
                AddToOutput("<TD ALIGN=""right"">")
                AddToOutput(VB6.Format(Pc.N, "0"))
                AddToOutput("</TD>")
                Pc = Pc.NextCat
            End While
            AddToOutput("<TD ALIGN=""right"">")
            AddToOutput(VB6.Format(Pd.N, "0"))
            AddToOutput("</TD>")

        Else

            Pc = Total

            'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If cnIsBoolean > 0 And Pc.Category.ToString() <> "TOTAL" Then
                If cnIsBoolean <> 2 Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    If Pc.Category = -1 Then
                        AddToOutput("<TD ALIGN=""left""><B>" & cvntLabels(1) & "</B></TD>")
                        'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    ElseIf Pc.Category = 0 Then
                        AddToOutput("<TD ALIGN=""left""><B>" & cvntLabels(2) & "</B></TD>")
                    Else
                        'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        AddToOutput("<TD ALIGN=""left""><B>" & Pc.Category & "</B></TD>")
                    End If
                Else
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput("<TD ALIGN=""left""><B>" & Pc.Category & "</B></TD>")
                End If
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                AddToOutput("<TD ALIGN=""left""><B>" & Pc.Category & "</B></TD>")
            End If

            If (Not Pc Is Nothing) Then
                AddToOutput("<TD ALIGN=""right"">")
                AddToOutput(VB6.Format(Pc.N, "0"))
                AddToOutput("</TD>")
            Else
                Pc = Pd.FirstCat
                While (Not Pc Is Nothing)
                    AddToOutput("<TD ALIGN=""right"">")
                    AddToOutput(VB6.Format(Pc.N, "0"))
                    AddToOutput("</TD>")
                    Pc = Pc.NextCat
                End While
                AddToOutput("<TD ALIGN=""right"">")
                AddToOutput(VB6.Format(Pd.N, "0"))
                AddToOutput("</TD>")
            End If

        End If

        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowVerticalPercent(ByRef Pd As CSDomain)

        Const PROC_Name As String = "clsCTables::ShowVerticalPercent"

        Dim P As CSTotal
        Dim Total As CSTotal
        Dim sOutputString As String

        On Error GoTo ErrorHandler

        AddToOutput("<TR>")

        AddToOutput("<TD ALIGN=""right"">" & VERTICALPERCENT & "</TD>")

        P = Pd.FirstCat
        While (Not P Is Nothing)

            Total = P

            While (Not Total.NextDom Is Nothing)
                Total = Total.NextDom
            End While

            AddToOutput("<TD ALIGN=""right"">")
            If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                'UPGRADE_WARNING: Couldn't resolve default property of object Total.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                AddToOutput(VB6.Format(100 * (P.YE / Total.YE), "0.000"))
            Else
                AddToOutput(UNAVAILABLE)
            End If
            AddToOutput("</TD>")
            P = P.NextCat

        End While

        AddToOutput("<TD ALIGN=""right"">")
        If (Not Pd Is TotalDom) Then
            If (Pd.SumW > 0.0#) Then
                AddToOutput(VB6.Format(100 * (Pd.SumW / TotalDom.SumW), "0.000"))
                columnPercents.Add(100 * (Pd.SumW / TotalDom.SumW))
            Else
                AddToOutput(UNAVAILABLE)
            End If
        Else
            AddToOutput(VB6.Format(100 * (Pd.SumW / TotalDom.SumW), "0.000"))
            'AddToOutput "&nbsp;"
        End If
        AddToOutput("</TD>")

        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowVerticalPercentforCSF(ByRef Pd As CSDomain, Optional ByRef dr As DataRow = Nothing)

        Dim P As CSTotal
        Dim Total As CSTotal
        On Error GoTo ErrorHandler
        P = Pd.FirstCat
        While (Not P Is Nothing)
            Total = P
            While (Not Total.NextDom Is Nothing)
                Total = Total.NextDom
            End While
            If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                dr("ColPct") = VB6.Format(100 * (P.YE / Total.YE), "0.0000000000000")
            Else
            End If
            P = P.NextCat
        End While
        If (Not Pd Is TotalDom) Then
            If (Pd.SumW > 0.0#) Then
                dr("ColPct") = VB6.Format(100 * (Pd.SumW / TotalDom.SumW), "0.0000000000000")
                columnPercents.Add(100 * (Pd.SumW / TotalDom.SumW))
            Else
            End If
        Else
            dr("ColPct") = VB6.Format(100 * (Pd.SumW / TotalDom.SumW), "0.0000000000000")
        End If
        Exit Sub

ErrorHandler:

    End Sub

    Public Sub ShowVerticalPercentforCST(ByRef Pd As CSDomain, Optional ByRef P As CSTotal = Nothing, Optional ByRef dr As DataRow = Nothing)

        Dim Total As CSTotal
        On Error GoTo ErrorHandler
        Total = P
        If Not Total Is Nothing Then
            While (Not Total.NextDom Is Nothing)
                Total = Total.NextDom
            End While
            If (P.YE > 0.0#) And (Total.YE > 0.0#) Then
                dr("ColPct") = VB6.Format(100 * (P.YE / Total.YE), "0.0000000000000")
            Else
            End If
        End If
        Exit Sub

ErrorHandler:

    End Sub


    Public Sub ShowHorizontalPercent(ByRef Pd As CSDomain, Optional ByRef Pc As Object = Nothing)

        Const PROC_Name As String = "clsCTables::ShowHorizontalPercent"

        Dim P As CSTotal

        On Error GoTo ErrorHandler

        AddToOutput("<TR>")

        'UPGRADE_NOTE: IsMissing() was changed to IsNothing(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="8AE1CB93-37AB-439A-A4FF-BE3B6760BB23"'
        If IsNothing(Pc) Then

            AddToOutput("<TD ALIGN=""right"">" & HORIZONTALPERCENT & "</TD>")

            P = Pd.FirstCat
            While (Not P Is Nothing)
                AddToOutput("<TD ALIGN=""right"">")
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput(VB6.Format(100 * (P.YE / Pd.SumW), "0.000"))
                Else
                    AddToOutput(UNAVAILABLE)
                End If
                AddToOutput("</TD>")
                P = P.NextCat
            End While

            AddToOutput("<TD ALIGN=""right"">" & VB6.Format(100.0#, "0.000") & "</TD>")

        Else

            AddToOutput("<TD ALIGN=""right"">")

            If (Not Pc Is Nothing) Then
                AddToOutput(VERTICALPERCENT)
                P = Pc
            Else
                AddToOutput(HORIZONTALPERCENT)
                P = Pd.FirstCat
            End If

            AddToOutput("</TD>")

            While (Not P Is Nothing)

                AddToOutput("<TD ALIGN=""right"">")
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput(VB6.Format(100 * (P.YE / Pd.SumW), "0.000"))
                Else
                    AddToOutput(UNAVAILABLE)
                End If
                AddToOutput("</TD>")

                If (Not Pc Is Nothing) Then
                    'UPGRADE_NOTE: Object P may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
                    P = Nothing
                Else
                    P = P.NextCat
                End If

            End While

            If (Pc Is Nothing) Then
                AddToOutput("<TD ALIGN=""right"">" & VB6.Format(100.0#, "0.000") & "</TD>")
            End If

        End If

        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowHorizontalPercentforCSF(ByRef Pd As CSDomain, Optional ByRef Pc As Object = Nothing, Optional ByRef dr As DataRow = Nothing)

        Const PROC_Name As String = "clsCTables::ShowHorizontalPercent"

        Dim P As CSTotal

        On Error GoTo ErrorHandler
        If IsNothing(Pc) Then
            P = Pd.FirstCat
            While (Not P Is Nothing)
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    datarow("RowPct") = VB6.Format(100 * (P.YE / Pd.SumW), "0.0000000000000")
                Else
                End If
                P = P.NextCat
            End While
            datarow("RowPct") = VB6.Format(100.0#, "0.0000000000000")
        Else
            Dim HVpercent As String
            If (Not Pc Is Nothing) Then
                HVpercent = "ColPct"
                P = Pc
            Else
                HVpercent = "RowPct"
                P = Pd.FirstCat
            End If

            While (Not P Is Nothing)
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    datarow(HVpercent) = VB6.Format(100 * (P.YE / Pd.SumW), "0.0000000000000")
                Else
                End If
                If (Not Pc Is Nothing) Then
                    P = Nothing
                Else
                    P = P.NextCat
                End If
            End While
            If (Pc Is Nothing) Then
                datarow(HVpercent) = VB6.Format(100.0#, "0.000")
            End If
        End If

        Exit Sub

ErrorHandler:

    End Sub

    Public Sub ShowHorizontalPercentforCST(ByRef Pd As CSDomain, Optional ByRef P As CSTotal = Nothing, Optional ByRef dr As DataRow = Nothing)

        On Error GoTo ErrorHandler
        If Not P Is Nothing Then
            If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                datarow("RowPct") = VB6.Format(100 * (P.YE / Pd.SumW), "0.0000000000000")
            Else
            End If
        Else
            If (Pd.FirstCat.NextCat.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                datarow("RowPct") = VB6.Format(100 * (Pd.FirstCat.NextCat.YE / Pd.SumW), "0.0000000000000")
            Else
            End If
        End If
        Exit Sub

ErrorHandler:

    End Sub


    Public Sub ShowStdError(ByRef Pd As CSDomain, Optional ByRef Pc As Object = Nothing)

        Const PROC_Name As String = "clsCTables::ShowStdError"

        Dim P As CSTotal
        Dim f As Object
        Dim sOutputString As String

        On Error GoTo ErrorHandler

        AddToOutput("<TR>")

        'UPGRADE_NOTE: IsMissing() was changed to IsNothing(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="8AE1CB93-37AB-439A-A4FF-BE3B6760BB23"'
        If IsNothing(Pc) Then

            AddToOutput("<TD ALIGN=""right""><TLT>SE %</TLT></TD>")

            P = Pd.FirstCat
            While (Not P Is Nothing)
                AddToOutput("<TD ALIGN=""right"">")
                If P.VarT >= 0.0# Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Math.Sqrt(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object f. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    f = Math.Sqrt(P.VarT)
                    'UPGRADE_WARNING: Couldn't resolve default property of object f. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput(VB6.Format(100 * f, "0.000"))
                Else
                    AddToOutput(UNAVAILABLE)
                End If
                AddToOutput("</TD>")
                P = P.NextCat
            End While

            AddToOutput("<TD ALIGN=""right"">&nbsp;</TD>")

        Else

            AddToOutput("<TD ALIGN=""right""><TLT>SE %</TLT></TD>")

            If (Not Pc Is Nothing) Then
                P = Pc
            Else
                P = Pd.FirstCat
            End If

            While (Not P Is Nothing)

                AddToOutput("<TD ALIGN=""right"">")
                If P.VarT >= 0.0# Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Math.Sqrt(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object f. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    f = Math.Sqrt(P.VarT)
                    'UPGRADE_WARNING: Couldn't resolve default property of object f. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput(VB6.Format(100 * f, "0.000"))
                Else
                    AddToOutput(UNAVAILABLE)
                End If
                AddToOutput("</TD>")

                If (Not Pc Is Nothing) Then
                    'UPGRADE_NOTE: Object P may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
                    P = Nothing
                Else
                    P = P.NextCat
                End If

            End While

            If (Pc Is Nothing) Then
                AddToOutput("<TD ALIGN=""right"">&nbsp;</TD>")
            End If

        End If

        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowStdErrorForCSF(ByRef Pd As CSDomain, Optional ByRef Pc As Object = Nothing, Optional ByRef dr As DataRow = Nothing)

        Dim P As CSTotal
        Dim f As Object
        On Error GoTo ErrorHandler
        If IsNothing(Pc) Then
            P = Pd.FirstCat
            While (Not P Is Nothing)
                If P.VarT >= 0.0# Then
                    f = Math.Sqrt(P.VarT)
                    dr("StdErr") = VB6.Format(100 * f, "0.00000000000000")
                Else
                End If
                P = P.NextCat
            End While

        Else

            If (Not Pc Is Nothing) Then
                P = Pc
            Else
                P = Pd.FirstCat
            End If

            While (Not P Is Nothing)
                If P.VarT >= 0.0# Then
                    f = Math.Sqrt(P.VarT)
                    dr("StdErr") = VB6.Format(100 * f, "0.00000000000000")
                Else

                End If
                If (Not Pc Is Nothing) Then
                    P = Nothing
                Else
                    P = P.NextCat
                End If
            End While
            If (Pc Is Nothing) Then
            End If

        End If
        Exit Sub

ErrorHandler:

    End Sub

    Public Sub ShowStdErrorForCST(ByRef Pd As CSDomain, Optional ByRef P As CSTotal = Nothing, Optional ByRef dr As DataRow = Nothing)
        Dim f As Object
        On Error GoTo ErrorHandler
        If Not P Is Nothing Then
            If P.VarT >= 0.0# Then
                f = Math.Sqrt(P.VarT)
                dr("StdErr") = VB6.Format(100 * f, "0.00000000000000")
            Else
            End If
        Else
            If Pd.FirstCat.NextCat.VarT >= 0.0# Then
                f = Math.Sqrt(Pd.FirstCat.NextCat.VarT)
                dr("StdErr") = VB6.Format(100 * f, "0.00000000000000")
            Else
            End If
        End If
        Exit Sub

ErrorHandler:

    End Sub


    Public Sub ShowCLPct(ByRef Pd As CSDomain, ByRef factor As Object, Optional ByRef Pc As Object = Nothing)

        Const PROC_Name As String = "clsCTables::ShowCLPct"

        Dim P As CSTotal
        Dim Pct As Object
        Dim Cl As Object

        On Error GoTo ErrorHandler

        AddToOutput("<TR>")

        'UPGRADE_NOTE: IsMissing() was changed to IsNothing(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="8AE1CB93-37AB-439A-A4FF-BE3B6760BB23"'
        If IsNothing(Pc) Then

            If (factor < 0.0#) Then
                AddToOutput("<TD ALIGN=""right""><TLT>LCL %</TLT></TD>")
            Else
                AddToOutput("<TD ALIGN=""right""><TLT>UCL %</TLT></TD>")
            End If

            P = Pd.FirstCat

            While (Not P Is Nothing)
                'UPGRADE_WARNING: Couldn't resolve default property of object Pct. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Pct = 0.0#
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pct. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Pct = 100 * (P.YE / Pd.SumW)
                End If

                AddToOutput("<TD ALIGN=""right"">")
                If (Pct > 0.0#) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Math.Sqrt(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object factor. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Cl. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Cl = (P.YE / Pd.SumW) + (factor * Math.Sqrt(P.VarT))
                    'UPGRADE_WARNING: Couldn't resolve default property of object Cl. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput(VB6.Format(100 * Cl, "0.000"))
                Else
                    AddToOutput(UNAVAILABLE)
                End If
                AddToOutput("</TD>")

                P = P.NextCat

            End While

            AddToOutput("<TD>&nbsp;</TD>")

        Else

            If (factor < 0.0#) Then
                AddToOutput("<TD ALIGN=""right""><TLT>LCL %</TLT></TD>")
            Else
                AddToOutput("<TD ALIGN=""right""><TLT>UCL %</TLT></TD>")
            End If

            If (Not Pc Is Nothing) Then
                P = Pc
            Else
                P = Pd.FirstCat
            End If

            While (Not P Is Nothing)

                'UPGRADE_WARNING: Couldn't resolve default property of object Pct. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Pct = 0.0#
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pct. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Pct = 100 * (P.YE / Pd.SumW)
                End If

                AddToOutput("<TD ALIGN=""right"">")
                If (Pct > 0.0#) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Math.Sqrt(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object factor. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Cl. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Cl = (P.YE / Pd.SumW) + (factor * Math.Sqrt(P.VarT))
                    'UPGRADE_WARNING: Couldn't resolve default property of object Cl. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput(VB6.Format(100 * Cl, "0.000"))
                Else
                    AddToOutput(UNAVAILABLE)
                End If
                AddToOutput("</TD>")

                If (Not Pc Is Nothing) Then
                    'UPGRADE_NOTE: Object P may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
                    P = Nothing
                Else
                    P = P.NextCat
                End If

            End While

            If (Pc Is Nothing) Then
                AddToOutput("<TD>&nbsp;</TD>")
            End If

        End If

        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowCLPctforCST(ByRef Pd As CSDomain, ByRef factor As Object, Optional ByRef P As CSTotal = Nothing, Optional ByRef dr As DataRow = Nothing)
        Dim Pct As Object
        Dim Cl As Object
        Dim LCLUCL As String
        On Error GoTo ErrorHandler
        If (factor < 0.0#) Then
            LCLUCL = "LCL"
        Else
            LCLUCL = "UCL"
        End If
        If Not P Is Nothing Then
            Pct = 0.0#
            If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                Pct = 100 * (P.YE / Pd.SumW)
            End If
            If (Pct > 0.0#) Then
                Cl = (P.YE / Pd.SumW) + (factor * Math.Sqrt(P.VarT))
                dr(LCLUCL) = VB6.Format(100 * Cl, "0.0000000000000")

            Else
            End If
        Else
            Pct = 0.0#
            If (Pd.FirstCat.NextCat.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                Pct = 100 * (Pd.FirstCat.NextCat.YE / Pd.SumW)
            End If
            If (Pct > 0.0#) Then
                Cl = (Pd.FirstCat.NextCat.YE / Pd.SumW) + (factor * Math.Sqrt(Pd.FirstCat.NextCat.VarT))
                dr(LCLUCL) = VB6.Format(100 * Cl, "0.0000000000000")
            Else
            End If
        End If
        Exit Sub

ErrorHandler:
    End Sub

    Public Sub ShowCLPctforCSF(ByRef Pd As CSDomain, ByRef factor As Object, Optional ByRef Pc As Object = Nothing, Optional ByRef dr As DataRow = Nothing)

        Dim P As CSTotal
        Dim Pct As Object
        Dim Cl As Object
        Dim LCLUCL As String
        On Error GoTo ErrorHandler
        If IsNothing(Pc) Then
            If (factor < 0.0#) Then
                LCLUCL = "LCL"
            Else
                LCLUCL = "UCL"
            End If
            P = Pd.FirstCat
            While (Not P Is Nothing)
                Pct = 0.0#
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    Pct = 100 * (P.YE / Pd.SumW)
                End If
                If (Pct > 0.0#) Then
                    Cl = (P.YE / Pd.SumW) + (factor * Math.Sqrt(P.VarT))
                    dr(LCLUCL) = VB6.Format(100 * Cl, "0.0000000000000")
                Else
                End If
                P = P.NextCat

            End While
        Else
            If (factor < 0.0#) Then
                LCLUCL = "LCL"
            Else
                LCLUCL = "UCL"
            End If
            If (Not Pc Is Nothing) Then
                P = Pc
            Else
                P = Pd.FirstCat
            End If
            While (Not P Is Nothing)
                Pct = 0.0#
                If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then
                    Pct = 100 * (P.YE / Pd.SumW)
                End If
                If (Pct > 0.0#) Then
                    Cl = (P.YE / Pd.SumW) + (factor * Math.Sqrt(P.VarT))
                    dr(LCLUCL) = VB6.Format(100 * Cl, "0.0000000000000")
                Else
                End If
                If (Not Pc Is Nothing) Then
                    P = Nothing
                Else
                    P = P.NextCat
                End If
            End While
            If (Pc Is Nothing) Then
            End If
        End If
        Exit Sub

ErrorHandler:

    End Sub


    Public Function DesignEffect(ByRef Pd As CSDomain) As Object

        Const PROC_Name As String = "clsCTables::DesignEffect"

        Dim P As CSTotal
        Dim denominator As Object
        Dim proporation As Object

        On Error GoTo ErrorHandler

        'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        denominator = 0
        P = Pd.FirstCat

        If (P.YE > 0.0#) And (Pd.SumW > 0.0#) Then '{ weighted }
            'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            denominator = CDec(P.YE / Pd.SumW)
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        denominator = CDec(denominator * (1 - denominator))

        'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If (denominator <> 0) Then '{ denominator = denominator / N }
            '        denominator = CDec(denominator / Pd.N) dcs0 I-3084 per Kevin Sullivan, should be pq/(n-1)
            'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            denominator = CDec(denominator / (Pd.N - 1))
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If (denominator <> 0) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object denominator. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object P.VarT. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            DesignEffect = CDec(P.VarT / denominator)
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object DesignEffect. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            DesignEffect = -1.0#
        End If

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub ShowDeff(ByRef Pd As CSDomain)

        Const PROC_Name As String = "clsCTables::ShowDeff"

        Dim P As CSTotal
        Dim deff As Object
        Dim sOutputString As String

        On Error GoTo ErrorHandler

        AddToOutput("<TR>")

        AddToOutput("<TD ALIGN=""right"">&nbsp;<TLT>Design Effect</TLT></TD>")

        'UPGRADE_WARNING: Couldn't resolve default property of object DesignEffect(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object deff. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        deff = DesignEffect(Pd)
        P = Pd.FirstCat
        While (Not P Is Nothing)
            AddToOutput("<TD ALIGN=""right"">")
            If (deff < 0.0#) Then
                AddToOutput(UNAVAILABLE)
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object deff. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                AddToOutput(VB6.Format(deff, "0.000"))
            End If
            AddToOutput("</TD>")
            P = P.NextCat
        End While

        AddToOutput("<TD ALIGN=""right"">&nbsp;</TD>")

        AddToOutput("</TR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub PrintValues()

        Const PROC_Name As String = "clsCTables::PrintValues"

        Dim Pc As CSCategory
        Dim Pd As CSDomain
        Dim P As CSTotal
        Dim nCat As Short
        Dim i As Integer
        Dim sOutputString As String

        On Error GoTo ErrorHandler

        If cnOutputLevel > 0 Then

            AddToOutput("<HTML>")

            ShowHeading()

            Pd = FirstDom
            nCat = 0 '{ count the categories }
            P = Pd.FirstCat

            If (Not Pd Is TotalDom) Then
                AddToOutput("<TR>")
                While (Not P Is Nothing)
                    If cnIsBoolean > 0 Then
                        If cnIsBoolean > 1 Then ' cnIsBoolean is a "2" or a "3" -- show labels.
                            'UPGRADE_WARNING: Couldn't resolve default property of object P.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            If P.Category = -1 Then
                                AddToOutput("<TD ALIGN=""left""><B>" & cvntLabels(1) & "</B></TD>")
                                'UPGRADE_WARNING: Couldn't resolve default property of object P.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            ElseIf P.Category = 0 Then
                                AddToOutput("<TD ALIGN=""left""><B>" & cvntLabels(2) & "</B></TD>")
                            Else
                                'UPGRADE_WARNING: Couldn't resolve default property of object P.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                                AddToOutput("<TD ALIGN=""right""><B>" & P.Category & "</B></TD>")
                            End If
                        Else
                            'UPGRADE_WARNING: Couldn't resolve default property of object P.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                            AddToOutput("<TD ALIGN=""right""><B>" & P.Category & "</B></TD>")
                        End If
                    Else
                        'UPGRADE_WARNING: Couldn't resolve default property of object P.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        AddToOutput("<TD ALIGN=""right""><B>" & P.Category & "</B></TD>")
                    End If

                    nCat = nCat + 1
                    P = P.NextCat
                End While
                AddToOutput("<TD ALIGN=""left""><B><TLT>TOTAL</TLT></B></TD>")
            End If

            nCat = nCat + 1

            AddToOutput("</TR>")

            If (Pd Is TotalDom) Then
                P = Pd.FirstCat
                While (Not P Is Nothing)

                    ShowCounts(Pd, P)

                    If cbIncludePercents Then
                        ' Display 100.00 since there is only 1 value on the row.
                        AddToOutput("<TR><TD ALIGN=""right"">" & HORIZONTALPERCENT & "</TD>")
                        AddToOutput("<TD ALIGN=""right"">" & VB6.Format(100.0#, "0.000") & "</TD></TR>")
                        ShowHorizontalPercent(Pd, P) ' Displays the horizontal % for the column % (invert single tables).
                    End If

                    If cnOutputLevel > 2 Then
                        ShowStdError(Pd, P)
                        If cbIncludePercents Then
                            ShowCLPct(Pd, -varianceMultiplier, P)
                            ShowCLPct(Pd, varianceMultiplier, P)
                        End If
                    End If

                    P = P.NextCat

                End While

                AddToOutput("<TR>")
                AddToOutput("<TD ALIGN=""left""><B><TLT>TOTAL</TLT></B></TD>")
                AddToOutput("<TD ALIGN=""right"">" & VB6.Format(TotalDom.N, "0") & "</TD>")
                AddToOutput("</TR>")

                If cnOutputLevel > 2 Then
                    AddToOutput("<TR>")
                    AddToOutput("<TD ALIGN=""right"">&nbsp;<TLT>Design Effect</TLT></TD>")
                    'UPGRADE_WARNING: Couldn't resolve default property of object DesignEffect(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    AddToOutput("<TD ALIGN=""right"">" & VB6.Format(DesignEffect(Pd), "0.000") & "</TD>")
                    AddToOutput("</TR>")
                End If

            Else

                While (Not Pd Is Nothing)

                    ShowCounts(Pd)

                    If cbIncludePercents Then
                        ShowHorizontalPercent(Pd)
                        ShowVerticalPercent(Pd)
                    End If

                    If cnOutputLevel > 2 Then
                        ShowStdError(Pd)
                        If cbIncludePercents Then
                            ShowCLPct(Pd, -varianceMultiplier)
                            ShowCLPct(Pd, varianceMultiplier)
                        End If
                        ShowDeff(Pd)
                    End If

                    Pd = Pd.NextDom

                End While

            End If

            AddToOutput("</TABLE>")
            AddToOutput("</HTML>")

        End If

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub


    Public Sub PrintValuesforCSF()

        Dim Pc As CSCategory
        Dim Pd As CSDomain
        Dim P As CSTotal
        Dim nCat As Short
        On Error GoTo ErrorHandler

        If cnOutputLevel > 0 Then
            Pd = FirstDom
            nCat = 0 '{ count the categories }
            P = Pd.FirstCat

            If (Pd Is TotalDom) Then

                P = Pd.FirstCat
                While (Not P Is Nothing)
                    datarow = Outputtable.NewRow
                    datarow(outcome.FieldLabel) = nCat + 1
                    datarow("VARNAME") = outcome.FieldLabel
                    datarow("COUNT") = VB6.Format(P.N, "0")
                    If cbIncludePercents Then
                        ' Display 100.00 since there is only 1 value on the row.
                        datarow("RowPct") = VB6.Format(100.0#, "0.000")
                        '''''''''''''''' '''' ShowHorizontalPercent(Pd, P) ' Displays the horizontal % for the column % (invert single tables).
                        '  ShowHorizontalPercentforTable(Pd, P)
                        ShowHorizontalPercentforCSF(Pd, P, datarow)

                    End If

                    If cnOutputLevel > 2 Then
                        ShowStdErrorForCSF(Pd, P, datarow)
                        If cbIncludePercents Then
                            ShowCLPctforCSF(Pd, -varianceMultiplier, P, datarow)
                            ShowCLPctforCSF(Pd, varianceMultiplier, P, datarow)
                        End If
                    End If
                    datarow("DesignEff") = VB6.Format(DesignEffect(Pd), "0.00000000000000")
                    Outputtable.Rows.Add(datarow)
                    P = P.NextCat
                    nCat = nCat + 1
                End While

            Else

                While (Not Pd Is Nothing)

                    datarow = Outputtable.NewRow
                    datarow(outcome.FieldLabel) = nCat + 1
                    datarow("VARNAME") = outcome.FieldLabel
                    If Not IsNothing(P) Then
                        datarow("COUNT") = VB6.Format(P.N, "0")
                        If cnIsBoolean > 0 And Pd.Domain.ToString() <> "TOTAL" Then
                            If cnIsBoolean <> 2 Then ' cnIsBoolean is a "1" or a "3" -- show the labels.
                                If Pd.Domain = -1 Then
                                    datarow("COUNT") = cvntLabels(1)
                                ElseIf Pd.Domain = 0 Then
                                    datarow("COUNT") = cvntLabels(2)
                                Else
                                    datarow("COUNT") = P.Category
                                End If
                            Else
                                datarow("COUNT") = P.Category
                            End If
                        Else
                            datarow("COUNT") = P.Category
                        End If
                    End If

                    If cbIncludePercents Then
                        datarow("RowPct") = VB6.Format(100.0#, "0.000")
                        ShowHorizontalPercentforCSF(Pd, Nothing, datarow)
                        ShowVerticalPercentforCSF(Pd, datarow)
                    End If

                    If cnOutputLevel > 2 Then
                        ShowStdErrorForCSF(Pd, Nothing, datarow)
                        If cbIncludePercents Then
                            ShowCLPctforCSF(Pd, -varianceMultiplier, Nothing, datarow)
                            ShowCLPctforCSF(Pd, varianceMultiplier, Nothing, datarow)
                        End If
                        datarow("DesignEff") = VB6.Format(DesignEffect(Pd), "0.00000000000000")
                    End If
                    Outputtable.Rows.Add(datarow)
                    Pd = Pd.NextDom
                    nCat = nCat + 1
                End While
            End If
        End If

        Exit Sub

ErrorHandler:

    End Sub

    Public Sub PrintValuesforCST()

        Dim Pd As CSDomain
        Dim P As CSTotal
        On Error GoTo ErrorHandler
        If cnOutputLevel > 0 Then

            Pd = FirstDom
            While (Not Pd Is Nothing)
                If (Not Pd Is TotalDom) Then
                    P = Pd.FirstCat
                    While (Not P Is Nothing)
                        datarow = Outputtable.NewRow
                        If Not P Is Nothing Then
                            datarow(exposureoutvar) = DirectCast(P.Category, Double)
                            If cnIsBoolean > 0 Then
                                If cnIsBoolean > 1 Then
                                    If P.Category = -1 Then
                                        datarow(exposurevar) = cvntLabels(1)
                                    ElseIf P.Category = 0 Then
                                        datarow(exposurevar) = cvntLabels(2)
                                    Else
                                        datarow(exposurevar) = DirectCast(P.Category, Double)
                                    End If
                                Else
                                    datarow(exposurevar) = DirectCast(P.Category, Double)
                                End If
                            Else
                                datarow(exposurevar) = DirectCast(P.Domain, Double)
                            End If
                        End If

                        datarow("COUNT") = VB6.Format(P.N, "0")

                        If cbIncludePercents Then
                            ShowHorizontalPercentforCST(Pd, P, datarow)
                            ShowVerticalPercentforCST(Pd, P, datarow)
                        End If

                        If cnOutputLevel > 2 Then
                            ShowStdErrorForCST(Pd, P, datarow)
                            If cbIncludePercents Then
                                ShowCLPctforCST(Pd, -varianceMultiplier, P, datarow)
                                ShowCLPctforCST(Pd, varianceMultiplier, P, datarow)
                            End If
                            datarow("DesignEff") = VB6.Format(DesignEffect(Pd), "0.00000000000000")
                        End If

                        Outputtable.Rows.Add(datarow)

                        P = P.NextCat
                    End While
                End If
                Pd = Pd.NextDom
            End While
        End If

        Exit Sub

ErrorHandler:

    End Sub

    Public Function FirstPassCom(ByRef errorMessage As String) As Integer

        Const PROC_Name As String = "clsCTables::FirstPassCom"

        Dim Pd As CSDomain
        Dim P As CSTotal
        Dim N As Integer
        Dim Valid As Boolean
        Dim doma As Object
        Dim cate As Object

        On Error GoTo ErrorHandler

        FirstPassCom = MsgBoxResult.No

        While (GetNextRow()) 'While (MyFile.GetNext >= 0)

            Valid = ValidCase()
            'UPGRADE_WARNING: Couldn't resolve default property of object Domain.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object doma. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            doma = domain.FieldEntry
            If Not Valid Then
                Mis = Mis + 1
            End If
            'UPGRADE_WARNING: Couldn't resolve default property of object Domain2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object doma. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Domain1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If Valid And ((doma = domain1) Or (doma = domain2)) Then
                'UPGRADE_WARNING: Couldn't resolve default property of object Outcome.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object cate. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                cate = outcome.FieldEntry
                Pd = FindDom(doma)
                If Pd Is Nothing Then
                    Pd = AddDom(doma)
                Else
                    Pd.SumW = Pd.SumW + GetWeight()
                    Pd.N = Pd.N + 1
                End If
                P = FindCat((Pd.FirstCat), cate)
                If (P Is Nothing) Then
                    AddCat(cate, doma)
                Else
                    AccumYE(P)
                End If
            End If

        End While

        FirstPassCom = MsgBoxResult.Ok

        Exit Function

ErrorHandler:

        errorMessage = Err.Description
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub AccumqhaCom(ByRef P As CSTotal, ByRef Pd As CSDomain, ByRef p2 As CSTotal, ByRef Pd2 As CSDomain)

        Const PROC_Name As String = "clsCTables::AccumqhaCom"

        Dim Qhab As Object
        Dim Ptr As CSTotal
        Dim Ptr2 As CSTotal

        On Error GoTo ErrorHandler

        'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Qhab = 0.0#
        If (Pd.SumW > 0.0#) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Qhab = (1 - (P.YE / Pd.SumW)) * (GetWeight() / Pd.SumW)
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object P.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        P.qha = P.qha + Qhab
        If Pd Is FirstDom Then
            'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object p2.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            p2.qha = p2.qha + Qhab
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object p2.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            p2.qha = p2.qha - Qhab
        End If

        Ptr = Pd.FirstCat
        Ptr2 = Pd2.FirstCat

        While (Not Ptr Is Nothing)
            If (Not Ptr Is P) Then
                If Pd.SumW > 0 Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Qhab = (Ptr.YE / Pd.SumW) * (GetWeight() / Pd.SumW)
                Else
                    'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "Attempted to divide by 0 -- Pd.SumW")
                    ' TODO: Revisit this
                End If
                'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Ptr.qha = Ptr.qha - Qhab
                If (Pd Is FirstDom) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Ptr2.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Ptr2.qha = Ptr2.qha - Qhab
                Else
                    'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Ptr2.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Ptr2.qha = Ptr2.qha + Qhab
                End If
            End If
            Ptr = Ptr.NextCat
            Ptr2 = Ptr2.NextCat
        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowStat(ByRef s As String, ByRef v As Object)

        Const PROC_Name As String = "clsCTables::ShowStat"

        On Error GoTo ErrorHandler

        'UPGRADE_WARNING: Couldn't resolve default property of object v. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        AddToOutput(s & VB6.Format(v, " 0.000") & "<BR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub ShowCL(ByRef lower As Object, ByRef upper As Object, Optional ByRef bShowNeg As Boolean = False)

        Const PROC_Name As String = "clsCTables::ShowCL"

        On Error GoTo ErrorHandler

        If (lower < 0.0# And Not bShowNeg) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object lower. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            lower = 0.0#
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object upper. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object lower. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        AddToOutput("  <TLT>95% Conf. Limits</TLT>          (" & VB6.Format(lower, "0.00") & ", " & VB6.Format(upper, "0.000") & " )<BR>")

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub PrintRisk(ByRef RR As Object, ByRef ODD As Object, ByRef RD As Object, ByRef VarRR As Object, ByRef VarOD As Object, ByRef VarRD As Object, ByRef VarOR As Object, ByRef VarLnRR As Object)

        Const PROC_Name As String = "clsCTables::PrintRisk"

        Const s1 As String = "<TLT>Odds Ratio (OR)</TLT>"
        Const s2 As String = "<TLT>Standard Error (SE)</TLT>"
        Const s3 As String = "<TLT>Risk Ratio (RR)</TLT>"
        Const s4 As String = "<TLT>Risk Difference (RD%)</TLT>"

        On Error GoTo ErrorHandler

        If cnOutputLevel > 1 Then
            AddToOutput("<BR><HR>")
            AddToOutput("<H4><TLT>CTABLES COMPLEX SAMPLE DESIGN ANALYSIS OF 2 X 2 TABLE</TLT></H4>")
            AddToOutput("<BR>")
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 9). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 8) = "Odds Ratio (OR)"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 10). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 9) = "Standard Error for OR"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 11). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 10) = "Lower 95% Confidence Limit for OR"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 12). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 11) = "Upper 95% Confidence Limit for OR"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 13). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 12) = "Risk Ratio (RR)"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 14). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 13) = "Standard Error for RR"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 15). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 14) = "Lower 95% Confidence Limit for RR"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 16). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 15) = "Upper 95% Confidence Limit for RR"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 17). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 16) = "Risk Difference (RD%)"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 18). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 17) = "Standard Error for RD(%)"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 19). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 18) = "Lower 95% Confidence Limit for RD(%)"
        'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(1, 20). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        vntResultsArray(0, 19) = "Upper 95% Confidence Limit for RD(%)"

        If (ODD >= 0.0#) Then

            '
            ' Odds Ratio (OR)
            '
            If cnOutputLevel > 1 Then
                ShowStat(s1, ODD)
            End If

            vntResultsArray(1, 8) = ODD

            '
            ' Standard Error for OR
            '
            If cnOutputLevel > 1 Then
                ShowStat(s2, Math.Sqrt(VarOD))
            End If

            vntResultsArray(1, 9) = Math.Sqrt(VarOD)

            '
            ' Lower/Upper Confidence Limits
            '
            If cnOutputLevel > 1 Then
                ShowCL(ExpOf(LnOf(ODD) - (varianceMultiplier * Math.Sqrt(VarOR))), ExpOf(LnOf(ODD) + (varianceMultiplier * Math.Sqrt(VarOR))))
                AddToOutput("<BR>")

            End If

            vntResultsArray(1, 10) = ExpOf(LnOf(ODD) - (varianceMultiplier * Math.Sqrt(VarOR)))
            vntResultsArray(1, 11) = ExpOf(LnOf(ODD) + (varianceMultiplier * Math.Sqrt(VarOR)))

        Else

            If cnOutputLevel > 1 Then
                AddToOutput("<TLT>NOTE: The Odds Ratio and its SE are not computed because the cells with 0 counts make the estimation procedures used invalid</TLT><BR>")
                vntResultsArray(1, 10) = System.DBNull.Value
                vntResultsArray(1, 11) = System.DBNull.Value
            End If

        End If

        '
        ' Risk Ratio (RR)
        '
        If (RR >= 0.0#) Then

            If cnOutputLevel > 1 Then
                ShowStat(s3, RR)
            End If

            vntResultsArray(1, 12) = RR

            '
            ' Standard Error for RR
            '
            If cnOutputLevel > 1 Then
                ShowStat(s2, Math.Sqrt(VarRR))
            End If

            vntResultsArray(1, 13) = Math.Sqrt(VarRR)

            '
            ' Lower/Upper Confidence Limits
            '
            If cnOutputLevel > 1 Then
                ShowCL(ExpOf(LnOf(RR) - (varianceMultiplier * Math.Sqrt(VarLnRR))), ExpOf(LnOf(RR) + (varianceMultiplier * Math.Sqrt(VarLnRR))))
            End If

            vntResultsArray(1, 14) = ExpOf(LnOf(RR) - (varianceMultiplier * Math.Sqrt(VarLnRR)))
            vntResultsArray(1, 15) = ExpOf(LnOf(RR) + (varianceMultiplier * Math.Sqrt(VarLnRR)))

            If cnOutputLevel > 1 Then
                AddToOutput("  <TLT>RR</TLT> = (<TLT>Risk of</TLT> " & outcome.FieldLabel & "=" & FirstCat.Category & " <TLT>if</TLT> " & domain.FieldLabel & "=" & domain1 & ")" & " / (<TLT>Risk of</TLT> " & outcome.FieldLabel & "=" & FirstCat.Category & " <TLT>if</TLT> " & domain.FieldLabel & "=" & domain2 & ")<BR>")
                AddToOutput("<BR>")
            End If

            '
            ' Risk Difference (RD)
            '
            If cnOutputLevel > 1 Then
                ShowStat(s4, RD * 100.0#)
            End If

            vntResultsArray(1, 16) = RD * 100.0#

            If cnOutputLevel > 1 Then
                ShowStat(s2, Math.Sqrt(VarRD) * 100.0#)
            End If

            vntResultsArray(1, 17) = Math.Sqrt(VarRD) * 100.0#

            If cnOutputLevel > 1 Then
                ShowCL((RD - (varianceMultiplier * Math.Sqrt(VarRD))) * 100.0#, (RD + (varianceMultiplier * Math.Sqrt(VarRD))) * 100.0#, True) '   CI can go negative with RD
            End If

            vntResultsArray(1, 18) = (RD - (varianceMultiplier * Math.Sqrt(VarRD))) * 100.0#
            vntResultsArray(1, 19) = (RD + (varianceMultiplier * Math.Sqrt(VarRD))) * 100.0#

            If cnOutputLevel > 1 Then
                AddToOutput("  <TLT>RD</TLT> = (<TLT>Risk of</TLT> " & outcome.FieldLabel & "=" & FirstCat.Category & " <TLT>if</TLT> " & domain.FieldLabel & "=" & domain1 & ")" & " - (<TLT>Risk of</TLT> " & outcome.FieldLabel & "=" & FirstCat.Category & " <TLT>if</TLT> " & domain.FieldLabel & "=" & domain2 & ")<BR>")
            End If

        Else

            If cnOutputLevel > 1 Then
                AddToOutput("<TLT>NOTE: The RR, RD and their SE''s are not computed because the cells with 0 counts make the estimation procedures used invalid</TLT><BR>")
            End If

            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 13). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 12) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 14). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 13) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 15). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 14) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 16). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 15) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 17). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 16) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 18). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 17) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 19). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 18) = Nothing
            'UPGRADE_WARNING: Couldn't resolve default property of object vntResultsArray(2, 20). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            vntResultsArray(1, 19) = Nothing

        End If

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    '
    '{----------------------------------------------------------------------------
    '|               Mantel-Haenszel Chi square                                  |
    '|               ÚÄÄÄÂÄÄÄÂÄÄÄ                                                |
    '|               ³ a ³ b ³ N1                                                |
    '|               ÅÄÄÄÅÄÄÄÅÄÄÄ                                                |
    '|               ³ c ³ d ³ N0                                                |
    '|               ÅÄÄÄÅÄÄÄÅÄÄÄ                                                |
    '|               ³M1 ³M0 ³ T                                                 |
    '|---------------------------------------------------------------------------}
    '
    Public Function MHChiSqr(ByRef a As Double, ByRef b As Double, ByRef c As Double, ByRef d As Double) As Double

        Const PROC_Name As String = "clsCTables::MHChiSqr"

        Dim N0 As Double
        Dim N1 As Double
        Dim M0 As Double
        Dim M1 As Double
        Dim T As Double

        On Error GoTo ErrorHandler

        N1 = a + b
        N0 = c + d
        M1 = a + c
        M0 = b + d
        T = N1 + N0

        MHChiSqr = ((T - 1) * ((a * d) - (b * c)) ^ 2) / (M1 * M0 * N1 * N0)
        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function ChiSqr(ByRef a As Double, ByRef b As Double, ByRef c As Double, ByRef d As Double) As Double

        Const PROC_Name As String = "clsCTables::ChiSqr"

        Dim N0 As Double
        Dim N1 As Double
        Dim M0 As Double
        Dim M1 As Double
        Dim T As Double

        On Error GoTo ErrorHandler

        N1 = a + b
        N0 = c + d
        M1 = a + c
        M0 = b + d
        T = N1 + N0

        ChiSqr = (T * ((a * d) - (b * c)) ^ 2) / (M1 * M0 * N1 * N0)

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function SecondPass(ByRef errorMessage As String) As Integer

        Const PROC_Name As String = "clsCTables::SecondPass"

        Dim P As CSTotal
        Dim p2 As CSTotal
        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim Valid As Boolean
        Dim risk As Boolean
        Dim RDRRok As Boolean
        Dim ORok As Boolean
        Dim ah As Integer
        Dim Rec As Integer
        Dim N As Integer
        Dim NowStrat As Object
        Dim NowPSU As Object
        Dim doma As Object
        Dim cate As Object
        Dim RR As Object
        Dim qhaRR As Object
        Dim qha2RR As Object
        Dim SumqhaRR As Object
        Dim Sumqha2RR As Object
        Dim VarRR As Object
        Dim ODD As Object
        Dim qhaOD As Object
        Dim qha2OD As Object
        Dim SumqhaOD As Object
        Dim Sumqha2OD As Object
        Dim VarOD As Object
        Dim R1 As Object
        Dim R2 As Object
        Dim RD As Object
        Dim qhaRD As Object
        Dim qha2RD As Object
        Dim SumqhaRD As Object
        Dim Sumqha2RD As Object
        Dim VarRD As Object
        Dim qhaOR As Object
        Dim SumqhaOR As Object
        Dim Sumqha2OR As Object
        Dim VarOR As Object
        Dim qhaLnRR As Object
        Dim SumqhaLnRR As Object
        Dim Sumqha2LnRR As Object
        Dim VarLnRR As Object
        Dim a As Object
        Dim b As Object
        Dim c As Object
        Dim d As Object
        Dim bContinue As Boolean
        Dim bHadValidPSU As Boolean

        On Error GoTo ErrorHandler

        SecondPass = MsgBoxResult.No
        Rec = 0
        N = 0
        T22 = False
        risk = False
        RDRRok = False
        ORok = False
        VarTInit()

        If (LastCat Is FirstCat.NextCat) Then

            If Not FirstDom.NextDom Is Nothing Then

                If TotalDom Is FirstDom.NextDom.NextDom Then '{i.e. 2*2 Table}

                    T22 = True
                    a = FirstCat.FirstDom.YE
                    d = LastCat.FirstDom.NextDom.YE
                    b = LastCat.FirstDom.YE
                    c = FirstCat.FirstDom.NextDom.YE
                    ODD = -1
                    If (a > 0) And (b > 0) And (c > 0) And (d > 0) Then
                        risk = True
                        ORok = True
                        ODD = CDec((a * d) / (b * c))
                    End If

                    'UPGRADE_WARNING: Couldn't resolve default property of object RR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    RR = -1
                    'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    If (a > 0) And (c > 0) Then
                        risk = True
                        RDRRok = True
                        R1 = CDec(a / (a + b))
                        R2 = CDec(c / (c + d))
                        RD = CDec(R1 - R2)
                        RR = CDec((a * (c + d)) / (c * (a + b)))
                    End If

                End If

            End If

        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object VarRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        VarRR = CDec(0.0#)
        'UPGRADE_WARNING: Couldn't resolve default property of object VarOD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        VarOD = CDec(0.0#)
        'UPGRADE_WARNING: Couldn't resolve default property of object VarRD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        VarRD = CDec(0.0#)
        'UPGRADE_WARNING: Couldn't resolve default property of object VarLnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        VarLnRR = CDec(0.0#)
        'UPGRADE_WARNING: Couldn't resolve default property of object VarOR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        VarOR = CDec(0.0#)
        Valid = False
        'UPGRADE_WARNING: Couldn't resolve default property of object NowStrat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        NowStrat = ""
        'UPGRADE_WARNING: Couldn't resolve default property of object NowPSU. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        NowPSU = ""

        While Not Valid And (Rec >= 0)

            Rec = GetNextRow() 'MyFile.GetNext

            Valid = ValidCase()
            If Valid Then
                If (Not strata Is Nothing) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Strata.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object NowStrat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    NowStrat = strata.FieldEntry
                End If
                If (Not psu Is Nothing) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object PSU.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object NowPSU. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    NowPSU = psu.FieldEntry
                End If
            End If

        End While

        Do  ' { The outermost loop }

            SumqInit()
            SumqhaRR = 0
            Sumqha2RR = 0
            SumqhaOD = 0
            Sumqha2OD = 0
            SumqhaRD = 0
            Sumqha2RD = 0
            SumqhaOR = 0.0#
            Sumqha2OR = 0.0#
            SumqhaLnRR = 0.0#
            Sumqha2LnRR = 0.0#

            ah = 0

            Do  ' { Middle loop }

                QhaInit()
                qhaRR = CDec(0.0#)
                qha2RR = CDec(0.0#)
                qhaOD = CDec(0.0#)
                qha2OD = CDec(0.0#)
                qhaRD = CDec(0.0#)
                qha2RD = CDec(0.0#)
                qhaOR = CDec(0.0#)
                qhaLnRR = CDec(0.0#)
                bHadValidPSU = False

                Do  ' { Innermost loop }

                    If Not domain Is Nothing Then
                        doma = domain.FieldEntry
                        Valid = False
                        If com Then
                            If ValidCase() And ((doma = domain1) Or (doma = domain2)) Then
                                Valid = True
                            End If
                        Else
                            Valid = ValidCase()
                        End If
                    Else
                        Valid = ValidCase()
                    End If

                    If Valid Then

                        bHadValidPSU = True

                        cate = outcome.FieldEntry

                        If (Not domain Is Nothing) Then
                            Pd = FindDom(doma)
                            P = FindCat((Pd.FirstCat), cate)
                            '#If USE_ACCUMQHACOM Then
                            '							'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression USE_ACCUMQHACOM did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
                            '							If Com Then
                            '							Set p2 = FindCat(LastDom.FirstCat, cate)
                            '							AccumqhaCom P, Pd, p2, LastDom
                            '							Else
                            '							Accumqha P, Pd
                            '							End If
                            '#Else
                            Accumqha(P, Pd)
                            '#End If
                        End If

                        If T22 And risk Then ' { qhab per PSU within stratum }

                            If Pd Is FirstDom Then

                                If P Is FirstCat.FirstDom Then

                                    If RDRRok Then
                                        qhaRR = qhaRR + RR * ((1 - a / (a + b)) / a) * GetWeight()
                                        qhaLnRR = qhaLnRR + (1 / a - 1 / (a + b)) * GetWeight()
                                        qhaRD = qhaRD + R1 * ((1 - R1) / a) * GetWeight()
                                    End If

                                    If ORok Then
                                        qhaOR = qhaOR + ((1 / a) * GetWeight())
                                        qhaOD = qhaOD + (1 / a) * GetWeight()
                                    End If

                                Else

                                    If RDRRok Then
                                        qhaRR = qhaRR - RR * (1 / (a + b)) * GetWeight()
                                        qhaLnRR = qhaLnRR - (1 / (a + b)) * GetWeight()
                                        qhaRD = qhaRD - (R1 * R1 / a) * GetWeight()
                                    End If

                                    If ORok Then
                                        qhaOR = qhaOR - (1 / b) * GetWeight()
                                        qhaOD = qhaOD - ODD * (1 / b) * GetWeight()
                                    End If

                                End If

                            Else

                                If P Is FirstDom.NextDom.FirstCat Then

                                    If RDRRok Then
                                        qhaRR = qhaRR - RR * ((1 - c / (c + d)) / c) * GetWeight()
                                        qhaLnRR = qhaLnRR + (1 / (c + d) - 1 / c) * GetWeight()
                                        qhaRD = qhaRD - R2 * ((1 - R2) / c) * GetWeight()
                                    End If

                                    If ORok Then
                                        qhaOR = qhaOR - (1 / c) * GetWeight()
                                        qhaOD = qhaOD - ODD * (1 / c) * GetWeight()
                                    End If

                                Else

                                    If RDRRok Then
                                        qhaRR = qhaRR + RR * (1 / (c + d)) * GetWeight()
                                        qhaLnRR = qhaLnRR + (1 / (c + d)) * GetWeight()
                                        qhaRD = qhaRD + (R2 * R2 / c) * GetWeight()
                                    End If

                                    If ORok Then
                                        qhaOR = qhaOR + (1 / d) * GetWeight()
                                        qhaOD = qhaOD + ODD * (1 / d) * GetWeight()
                                    End If

                                End If

                            End If

                        End If

                        P = FindCat((TotalDom.FirstCat), cate)
                        Accumqha(P, TotalDom)

                    End If

                    Rec = GetNextRow() 'MyFile.GetNext

                    If psu Is Nothing Then
                        bContinue = True
                    Else
                        If psu.FieldEntry <> NowPSU Then
                            bContinue = True
                        ElseIf strata.FieldEntry <> NowStrat Then
                            bContinue = True
                        Else
                            bContinue = False
                        End If
                    End If

                Loop Until (Rec = False) Or bContinue

                If (Not psu Is Nothing) Then

                    If (Rec = False) Or (FieldColl(psu, NowPSU) > 0) Then

                        NowPSU = psu.FieldEntry

                    ElseIf Not strata Is Nothing Then

                        If strata.FieldEntry <> NowStrat Then
                            NowPSU = psu.FieldEntry
                        Else
                            'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "Not sorted by strata, psu")
                            SecondPass = -1
                            Exit Function
                        End If

                    Else
                        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "Not sorted by strata, psu")
                        SecondPass = -1
                        Exit Function
                    End If

                End If

                If bHadValidPSU Then

                    ah = ah + 1
                    AccumSumq()
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    SumqhaRR = SumqhaRR + qhaRR ' { sum of total qhab }
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2RR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Sumqha2RR = Sumqha2RR + (qhaRR ^ 2) ' { squared sum of total qhab }
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaOD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaOD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    SumqhaOD = SumqhaOD + qhaOD
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaOD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2OD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Sumqha2OD = Sumqha2OD + (qhaOD ^ 2)
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaOR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaOR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    SumqhaOR = SumqhaOR + qhaOR
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaOR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2OR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Sumqha2OR = Sumqha2OR + (qhaOR ^ 2)
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaLnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaLnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    SumqhaLnRR = SumqhaLnRR + qhaLnRR
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaLnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2LnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Sumqha2LnRR = Sumqha2LnRR + (qhaLnRR ^ 2)
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaRD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaRD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    SumqhaRD = SumqhaRD + qhaRD
                    'UPGRADE_WARNING: Couldn't resolve default property of object qhaRD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2RD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Sumqha2RD = Sumqha2RD + (qhaRD ^ 2)

                End If

                If Not strata Is Nothing Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object NowStrat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Strata.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    If strata.FieldEntry <> NowStrat Then
                        bContinue = True
                    Else
                        bContinue = False
                    End If
                Else
                    bContinue = False
                End If

            Loop Until (Rec = False) Or bContinue

            If (Not strata Is Nothing) Then

                If (Rec = False) Or (FieldColl(strata, NowStrat) > 0) Then

                    'UPGRADE_WARNING: Couldn't resolve default property of object Strata.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object NowStrat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    NowStrat = strata.FieldEntry

                Else
                    'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "Not sorted by strata, psu")
                    SecondPass = -1
                    Exit Function
                End If

            End If

            AccumVar(ah)
            If ah > 1 Then ' { compute variances }
                'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2RR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object VarRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                VarRR = VarRR + (ah * Sumqha2RR - (SumqhaRR ^ 2)) / (ah - 1)
                'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaOD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2OD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object VarOD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                VarOD = VarOD + (ah * Sumqha2OD - (SumqhaOD ^ 2)) / (ah - 1)
                'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaRD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2RD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object VarRD. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                VarRD = VarRD + (ah * Sumqha2RD - (SumqhaRD ^ 2)) / (ah - 1)
                'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaOR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2OR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object VarOR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                VarOR = VarOR + (ah * Sumqha2OR - (SumqhaOR ^ 2)) / (ah - 1) ' { var(ln(Tef)) }
                'UPGRADE_WARNING: Couldn't resolve default property of object SumqhaLnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Sumqha2LnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object VarLnRR. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                VarLnRR = VarLnRR + (ah * Sumqha2LnRR - (SumqhaLnRR ^ 2)) / (ah - 1) ' { var(ln(Tef)) }
            End If

        Loop Until (Rec = False)

        '
        ' Print the HTML output file.
        '
        PrintValues()
        If Not OuttableName Is Nothing Then
            If Not exposurevar Is Nothing And Not exposureoutvar Is Nothing Then
                PrintValuesforCST()
            Else
                PrintValuesforCSF()
            End If
        End If
        If T22 And risk And cnOutputLevel > 1 Then
            'UPGRADE_WARNING: Lower bound of array vntResultsArray was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            'UPGRADE_ISSUE: As Variant was removed from ReDim vntResultsArray(1 To 2, 1 To 20) statement. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="19AFCB41-AA8E-4E6B-A441-A3E802E5FD64"'
            ReDim vntResultsArray(20, 19)
            PrintRisk(RR, ODD, RD, VarRR, VarOD, VarRD, VarOR, VarLnRR)
        Else
            'UPGRADE_WARNING: Lower bound of array vntResultsArray was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            'UPGRADE_ISSUE: As Variant was removed from ReDim vntResultsArray(1 To 2, 1 To 8) statement. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="19AFCB41-AA8E-4E6B-A441-A3E802E5FD64"'
            ReDim vntResultsArray(1, 7)
        End If

        PrintDesign()

        '
        ' If the output level is "0" then toss the HTML output.
        '
        If cnOutputLevel = 0 Then
            csOutputBuffer = ""
        End If

        SecondPass = MsgBoxResult.Ok

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    '    Public Function DoTables() As Integer

    '        Const PROC_Name As String = "clsCTables::DoTables"

    '        '
    '        'function doTables : word;
    '        'const
    '        '     Title    : string[22] = 'CLUSTER TABLE ANALYSIS';
    '        'Var
    '        '   TerminalWindow : PTerminalWindow;
    '        '   D              : PDialog;
    '        '   result         : word;
    '        'begin
    '        '{$IFDEF CONFUSE_THE_PROGRAMMER}
    '        '  If (Domain <> nil) Then
    '        '     swapVariables(outcome, domain);
    '        '{$ENDIF}
    '        '   terminalWindow = nil;
    '        '   doTables = cmClose;
    '        '   mis=0;
    '        '   firstDom= nothing;
    '        '   lastDom= nothing;
    '        '   totalDom= nothing;
    '        '   firstCat= nothing;
    '        '   lastCat= nothing;
    '        '   pdm = nil;
    '        '   status('FIRST_DB');
    '        '   If com Then
    '        '      result = firstPassCom
    '        '   Else
    '        '      result = firstPass;
    '        '   status('');
    '        '   If (result = cmOK) Then
    '        '      begin
    '        '      If (OutType = TOSCREEN) Then
    '        '         TerminalWindow = openTerminalWindow(@Title, outfile)
    '        '      Else
    '        '         begin
    '        '         assign(outfile, filename);
    '        '         If (overWrite) Then
    '        '            rewrite (Outfile)
    '        '         Else
    '        '            append(outfile);
    '        '         end;
    '        '{$IFDEF CONFUSE_THE_PROGRAMMER}
    '        '      If (FirstCat ^ .NextCat = LastCat) Then
    '        '         begin
    '        '         com = true;
    '        '         domain1=firstcat^.category;
    '        '         domain2= lastcat^.category;
    '        '         end;
    '        '{$ELSE}
    '        '      If FirstDom ^ .NextDom = LastDom Then
    '        '         begin
    '        '         com=true;
    '        '         Domain1=firstDom^.Domain;
    '        '         Domain2=lastDom^.Domain;
    '        '         end;
    '        '{$ENDIF}
    '        '      If (Domain <> nil) Then
    '        '         ComputeTot;
    '        '{$IFNDEF REVERSE_THE_OUTPUT}
    '        '      If com Then
    '        '         AddDom('Difference',Pdm);
    '        '{$ENDIF}
    '        '      MyFile.restart;
    '        '      ClusterApp.Progress(1,1);
    '        '      status('SECOND_DB');
    '        '      result = secondPass;
    '        '      status('');
    '        '      ClusterApp.Progress(1,1);
    '        '      If (result = cmOK) Then
    '        '         begin
    '        '         If (OutType = TOSCREEN) Then
    '        '            result = ClusterApp.execute
    '        '         Else
    '        '            close(outfile);
    '        '         end;
    '        '      if (result <> cmClose) and         { because App would close it }
    '        '         (TerminalWindow <> nil) then
    '        '      dispose(TerminalWindow, done);
    '        '      end;
    '        '{$IFDEF CONFUSE_THE_PROGRAMMER}
    '        '  If (Domain <> nil) Then
    '        '     swapVariables(outcome, domain);
    '        '{$ENDIF}
    '        'end;
    '        '
    '        Const Title As String = "CLUSTER TABLE ANALYSIS"
    '        Dim result As Integer

    '        On Error GoTo ErrorHandler

    '        DoTables = MsgBoxResult.No
    '        Mis = 0
    '        'UPGRADE_NOTE: Object FirstDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '        FirstDom = Nothing
    '        'UPGRADE_NOTE: Object LastDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '        LastDom = Nothing
    '        'UPGRADE_NOTE: Object TotalDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '        TotalDom = Nothing
    '        'UPGRADE_NOTE: Object FirstCat may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '        FirstCat = Nothing
    '        'UPGRADE_NOTE: Object LastCat may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '        LastCat = Nothing
    '        'UPGRADE_NOTE: Object Pdm may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '        Pdm = Nothing

    '        If com Then
    '            result = FirstPassCom()
    '        Else
    '            result = FirstPass()
    '        End If

    '        If (result = MsgBoxResult.Ok) Then

    '            If FirstDom.NextDom Is LastDom Then
    '                com = True
    '                'UPGRADE_WARNING: Couldn't resolve default property of object FirstDom.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain1 = FirstDom.Domain
    '                'UPGRADE_WARNING: Couldn't resolve default property of object LastDom.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain2 = LastDom.Domain
    '            End If

    '            If (Not domain Is Nothing) Then
    '                ComputeTot()
    '            End If

    '            ResetReader() 'MyFile.Restart()
    '            result = SecondPass()

    '        End If

    '        DoTables = 0

    '        Exit Function

    'ErrorHandler:

    '        DoTables = -1
    '        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    '    End Function

    Public Function GetWeight() As Object

        Const PROC_Name As String = "clsCTables::GetWeight"

        On Error GoTo ErrorHandler

        If (Not weight Is Nothing) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object Weight.FieldReal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object GetWeight. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            GetWeight = weight.FieldReal
        Else
            GetWeight = CDec(1.0#)
        End If

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function ValidCase() As Boolean

        Const PROC_Name As String = "clsCTables::ValidCase"

        On Error GoTo ErrorHandler

        ValidCase = True

        If Not outcome Is Nothing And ValidCase Then
            If outcome.Missing Then
                ValidCase = False
            End If
        End If
        If Not strata Is Nothing And ValidCase Then
            If strata.Missing Then
                ValidCase = False
            End If
        End If
        If Not psu Is Nothing And ValidCase Then
            If psu.Missing Then
                ValidCase = False
            End If
        End If
        If Not weight Is Nothing And ValidCase Then
            If weight.Missing Then
                ValidCase = False
            End If
        End If
        If Not domain Is Nothing And ValidCase Then
            If domain.Missing Then
                ValidCase = False
            End If
        End If

        Exit Function

ErrorHandler:

        ValidCase = False
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function


    Public Function NewTot(ByRef dom As Object, ByRef cat As Object) As CSTotal

        Const PROC_Name As String = "clsCTables::NewTot"

        Dim Ptr As CSTotal

        On Error GoTo ErrorHandler

        Ptr = New CSTotal
        'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Ptr.Domain = dom
        'UPGRADE_WARNING: Couldn't resolve default property of object cat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Ptr.Category = cat
        'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Ptr.YE = 0
        Ptr.N = 0
        'UPGRADE_NOTE: Object Ptr.NextDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Ptr.NextDom = Nothing
        'UPGRADE_NOTE: Object Ptr.NextCat may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Ptr.NextCat = Nothing
        NewTot = Ptr

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function NewCat(ByRef cat As Object) As CSCategory

        Const PROC_Name As String = "clsCTables::NewCat"

        Dim Pc As CSCategory

        On Error GoTo ErrorHandler

        Pc = New CSCategory
        'UPGRADE_WARNING: Couldn't resolve default property of object cat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Pc.Category = cat
        'UPGRADE_NOTE: Object Pc.NextCat may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Pc.NextCat = Nothing
        'UPGRADE_NOTE: Object Pc.FirstDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Pc.FirstDom = Nothing
        NewCat = Pc

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub AccumYE(ByRef P As CSTotal)

        Const PROC_Name As String = "clsCTables::AccumYE"

        On Error GoTo ErrorHandler

        'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        P.YE = P.YE + GetWeight()
        P.N = P.N + 1

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function WhereIns(ByRef dom As Object) As CSDomain

        Const PROC_Name As String = "clsCTables::WhereIns"

        Dim Pd As CSDomain
        Dim found As Boolean

        On Error GoTo ErrorHandler

        found = False

        'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If (dom.ToString() = "TOTAL") Or (dom.ToString() = "Difference") Then
            WhereIns = LastDom
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object FirstDom.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If (FirstDom.Domain > dom) Then
                'UPGRADE_NOTE: Object WhereIns may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
                WhereIns = Nothing
            Else
                Pd = FirstDom
                While Not Pd.NextDom Is Nothing And Not found
                    'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Pd.NextDom.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    If (Pd.NextDom.Domain > dom) Then
                        WhereIns = Pd
                        found = True
                    Else
                        Pd = Pd.NextDom
                    End If
                End While
                If Not found Then
                    WhereIns = LastDom
                End If
            End If
        End If

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function WhereInsC(ByRef cat As Object) As CSCategory

        Const PROC_Name As String = "clsCTables::WhereInsC"

        Dim Pc As CSCategory
        Dim found As Boolean

        On Error GoTo ErrorHandler

        found = False
        'UPGRADE_WARNING: Couldn't resolve default property of object cat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object FirstCat.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If (FirstCat.Category > cat) Then
            'UPGRADE_NOTE: Object WhereInsC may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
            WhereInsC = Nothing
        Else
            Pc = FirstCat
            While Not Pc.NextCat Is Nothing And Not found
                'UPGRADE_WARNING: Couldn't resolve default property of object cat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Pc.NextCat.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If (Pc.NextCat.Category > cat) Then
                    WhereInsC = Pc
                    found = True
                Else
                    Pc = Pc.NextCat
                End If
            End While
            If Not found Then
                WhereInsC = LastCat
            End If
        End If

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function AddDom(ByRef dom As Object) As CSDomain

        Const PROC_Name As String = "clsCTables::AddDom"

        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim R2 As CSCategory
        Dim Pt As CSDomain
        Dim P As CSTotal
        Dim Q As CSTotal
        Dim R As CSTotal

        On Error GoTo ErrorHandler

        Pd = New CSDomain
        'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Pd.Domain = dom
        'UPGRADE_WARNING: Couldn't resolve default property of object GetWeight. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Pd.SumW = GetWeight()
        Pd.N = 1
        'UPGRADE_NOTE: Object Pd.NextDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Pd.NextDom = Nothing

        If (LastDom Is Nothing) Then
            Pc = NewCat((outcome.FieldEntry))
            FirstCat = Pc
            LastCat = Pc
            P = NewTot(dom, (outcome.FieldEntry))
            Pd.FirstCat = P
            Pc.FirstDom = P
            FirstDom = Pd
            LastDom = Pd
        Else
            Pt = WhereIns(dom)
            If (Pt Is Nothing) Then
                Pd.NextDom = FirstDom
                R2 = FirstCat
                P = NewTot(dom, (R2.Category))
                P.NextDom = R2.FirstDom
                Pd.FirstCat = P
                R2.FirstDom = P
                R2 = R2.NextCat
                Q = P
                While Not R2 Is Nothing
                    P = NewTot(dom, (R2.Category))
                    P.NextDom = R2.FirstDom
                    Q.NextCat = P
                    R2.FirstDom = P
                    R2 = R2.NextCat
                    Q = P
                End While
                FirstDom = Pd
            Else
                Pd.NextDom = Pt.NextDom
                Pt.NextDom = Pd
                R = Pt.FirstCat
                P = NewTot(dom, (R.Category))
                P.NextDom = R.NextDom
                Pd.FirstCat = P
                R.NextDom = P
                R = R.NextCat
                Q = P
                While Not R Is Nothing
                    P = NewTot(dom, (R.Category))
                    P.NextDom = R.NextDom
                    Q.NextCat = P
                    R.NextDom = P
                    R = R.NextCat
                    Q = P
                End While
                If (Pt Is LastDom) Then
                    LastDom = Pd
                End If

            End If

        End If

        AddDom = Pd

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub AddCat(ByRef cat As Object, ByRef dom As Object)

        Const PROC_Name As String = "clsCTables::AddCat"

        Dim Pd As CSDomain
        Dim R2 As CSDomain
        Dim Pc As CSCategory
        Dim Pt As CSCategory
        Dim P As CSTotal
        Dim Q As CSTotal
        Dim R As CSTotal

        On Error GoTo ErrorHandler

        Pc = New CSCategory
        'UPGRADE_WARNING: Couldn't resolve default property of object cat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Pc.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Pc.Category = cat
        'UPGRADE_NOTE: Object Pc.NextCat may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Pc.NextCat = Nothing
        'UPGRADE_NOTE: Object Pc.FirstDom may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        Pc.FirstDom = Nothing
        Pt = WhereInsC(cat)

        If Pt Is Nothing Then
            Pc.NextCat = FirstCat
            R2 = FirstDom
            P = NewTot((R2.Domain), cat)
            'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object R2.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If (R2.Domain = dom) Then
                AccumYE(P)
            End If
            P.NextCat = R2.FirstCat
            Pc.FirstDom = P
            R2.FirstCat = P
            R2 = R2.NextDom
            Q = P
            While Not R2 Is Nothing
                P = NewTot((R2.Domain), cat)
                'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object R2.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If (R2.Domain = dom) Then
                    AccumYE(P)
                End If
                P.NextCat = R2.FirstCat
                Q.NextDom = P
                R2.FirstCat = P
                R2 = R2.NextDom
                Q = P
            End While
            FirstCat = Pc
        Else
            Pc.NextCat = Pt.NextCat
            Pt.NextCat = Pc
            R = Pt.FirstDom
            P = NewTot((R.Domain), cat)
            'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object R.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If (R.Domain = dom) Then
                AccumYE(P)
            End If
            P.NextCat = R.NextCat
            Pc.FirstDom = P
            R.NextCat = P
            R = R.NextDom
            Q = P
            While Not R Is Nothing
                P = NewTot((R.Domain), cat)
                'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object R.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If (R.Domain = dom) Then
                    AccumYE(P)
                End If
                P.NextCat = R.NextCat
                Q.NextDom = P
                R.NextCat = P
                R = R.NextDom
                Q = P
            End While
            If (Pt Is LastCat) Then
                LastCat = Pc
            End If

        End If

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function FindDom(ByRef dom As Object) As CSDomain

        Const PROC_Name As String = "clsCTables::FindDom"

        Dim Pd As CSDomain
        Dim found As Boolean

        On Error GoTo ErrorHandler

        found = False
        Pd = FirstDom

        While Not Pd Is Nothing And Not found

            'UPGRADE_WARNING: Couldn't resolve default property of object dom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Pd.Domain. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If (Pd.Domain = dom) Then
                found = True
            Else
                Pd = Pd.NextDom
            End If

        End While

        FindDom = Pd

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Function FindCat(ByRef P As CSTotal, ByRef cat As Object) As CSTotal

        Const PROC_Name As String = "clsCTables::FindCat"

        Dim found As Boolean

        On Error GoTo ErrorHandler

        found = False

        While (Not P Is Nothing) And Not found

            'UPGRADE_WARNING: Couldn't resolve default property of object cat. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object P.Category. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If (P.Category = cat) Then
                found = True
            Else
                P = P.NextCat
            End If

        End While

        FindCat = P

        Exit Function

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub SumqInit()

        Const PROC_Name As String = "clsCTables::SumqInit"

        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal

        On Error GoTo ErrorHandler

        Pd = FirstDom

        While Not Pd Is Nothing

            P = Pd.FirstCat

            While Not P Is Nothing

                'UPGRADE_WARNING: Couldn't resolve default property of object P.Sumqha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.Sumqha = 0
                'UPGRADE_WARNING: Couldn't resolve default property of object P.Sumqha2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.Sumqha2 = 0
                P = P.NextCat

            End While

            Pd = Pd.NextDom

        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub VarTInit()

        Const PROC_Name As String = "clsCTables::VarTInit"
        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal

        On Error GoTo ErrorHandler

        Pd = FirstDom

        While Not Pd Is Nothing

            P = Pd.FirstCat

            While Not P Is Nothing

                'UPGRADE_WARNING: Couldn't resolve default property of object P.VarT. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.VarT = 0
                P = P.NextCat

            End While

            Pd = Pd.NextDom

        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub QhaInit()

        Const PROC_Name As String = "clsCTables::QhaInit"

        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal

        On Error GoTo ErrorHandler

        Pd = FirstDom

        While Not Pd Is Nothing

            P = Pd.FirstCat

            While Not P Is Nothing

                'UPGRADE_WARNING: Couldn't resolve default property of object P.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.qha = 0
                'UPGRADE_WARNING: Couldn't resolve default property of object P.qha2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.qha2 = 0
                P = P.NextCat

            End While

            Pd = Pd.NextDom

        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub AccumSumq()

        Const PROC_Name As String = "clsCTables::AccumSumq"

        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal

        On Error GoTo ErrorHandler

        Pd = FirstDom

        While Not Pd Is Nothing

            P = Pd.FirstCat

            While Not P Is Nothing

                'UPGRADE_WARNING: Couldn't resolve default property of object P.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object P.Sumqha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.Sumqha = P.Sumqha + P.qha
                'UPGRADE_WARNING: Couldn't resolve default property of object P.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object P.Sumqha2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                P.Sumqha2 = P.Sumqha2 + (P.qha ^ 2)
                P = P.NextCat

            End While

            Pd = Pd.NextDom

        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub AccumVar(ByRef ah As Integer)

        Const PROC_Name As String = "clsCTables::AccumVar"

        Dim Pd As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal

        On Error GoTo ErrorHandler

        Pd = FirstDom

        While Not Pd Is Nothing

            P = Pd.FirstCat

            While Not P Is Nothing

                If (ah > 1) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.Sumqha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.Sumqha2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object P.VarT. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    P.VarT = P.VarT + ((ah * P.Sumqha2) - (P.Sumqha ^ 2)) / (ah - 1)
                End If

                P = P.NextCat

            End While

            Pd = Pd.NextDom

        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    'UPGRADE_NOTE: Class_Initialize was upgraded to Class_Initialize_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub Class_Initialize_Renamed()

        Const PROC_Name As String = "clsCTables::Class_Initialize"

        On Error GoTo ErrorHandler

        'MyFile.goLogFile = goLogFile

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub
    Public Sub New()
        MyBase.New()
        Class_Initialize_Renamed()
    End Sub

    '    Public Function Init(ByRef vntParameters As Object) As Boolean

    '        Const PROC_Name As String = "clsCTables::Init"

    '        Dim sDatabase As String
    '        Dim sTableName As String
    '        Dim sConnectionString As String
    '        Dim bPercents As Object
    '        Dim nOutputLevel As Object
    '        Dim nIsBoolean As Object
    '        Dim vntLabels As Object
    '        Dim i As Short
    '        Dim j As Short

    '        On Error GoTo ErrorHandler

    '        'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '        sDatabase = GetSettingValue(vntParameters, "Database")
    '        'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '        sTableName = GetSettingValue(vntParameters, "TableName")
    '        'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '        sConnectionString = GetSettingValue(vntParameters, "ConnectionString")
    '        'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '        If MyFile.Init(sDatabase, sTableName, sConnectionString, Trim(GetSettingValue(vntParameters, "StrataVar")), Trim(GetSettingValue(vntParameters, "PSUVar"))) Then

    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            If Len(Trim(GetSettingValue(vntParameters, "StrataVar"))) > 0 Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                strata = MyFile.Find(Trim(GetSettingValue(vntParameters, "StrataVar")))
    '            Else
    '                'UPGRADE_NOTE: Object Strata may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '                strata = Nothing
    '            End If

    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            If Len(Trim(GetSettingValue(vntParameters, "PSUVar"))) > 0 Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                psu = MyFile.Find(Trim(GetSettingValue(vntParameters, "PSUVar")))
    '            Else
    '                'UPGRADE_NOTE: Object PSU may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '                psu = Nothing
    '            End If
    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            If Len(Trim(GetSettingValue(vntParameters, "WeightVar"))) > 0 Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                weight = MyFile.Find(Trim(GetSettingValue(vntParameters, "WeightVar")))
    '            Else
    '                'UPGRADE_NOTE: Object Weight may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '                weight = Nothing
    '            End If
    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            If Len(Trim(GetSettingValue(vntParameters, "CrosstabVar"))) > 0 Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain = MyFile.Find(Trim(GetSettingValue(vntParameters, "CrosstabVar")))
    '            Else
    '                'UPGRADE_NOTE: Object Domain may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '                domain = Nothing
    '            End If
    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            If Len(Trim(GetSettingValue(vntParameters, "MainVar"))) > 0 Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                outcome = MyFile.Find(Trim(GetSettingValue(vntParameters, "MainVar")))
    '            Else
    '                'UPGRADE_NOTE: Object Outcome may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
    '                outcome = Nothing
    '            End If

    '            If Not domain Is Nothing Then

    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain1 = Trim(GetSettingValue(vntParameters, "Domain1"))
    '                'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain2 = Trim(GetSettingValue(vntParameters, "Domain2"))

    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                If (domain1 <> domain2) Then
    '                    com = True
    '                Else
    '                    com = False
    '                End If

    '            Else

    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain1 = ""
    '                'UPGRADE_WARNING: Couldn't resolve default property of object Domain2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                domain2 = ""
    '                com = False

    '            End If

    '            '
    '            ' Get the Percents setting.  If True then set boolean indicating
    '            ' that percents should be displayed in the HTML output.
    '            '
    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            'UPGRADE_WARNING: Couldn't resolve default property of object bPercents. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            bPercents = GetSettingValue(vntParameters, "Percents")
    '            'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
    '            If Not IsNothing(bPercents) Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object bPercents. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                cbIncludePercents = bPercents
    '            Else
    '                cbIncludePercents = False
    '            End If

    '            '
    '            ' Set boolean variables based on the OutputLevel parameter specified.
    '            '
    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            'UPGRADE_WARNING: Couldn't resolve default property of object nOutputLevel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            nOutputLevel = GetSettingValue(vntParameters, "OutputLevel")
    '            'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
    '            If Not IsNothing(nOutputLevel) Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object nOutputLevel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                cnOutputLevel = nOutputLevel
    '            Else
    '                cnOutputLevel = 3
    '            End If

    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            'UPGRADE_WARNING: Couldn't resolve default property of object nIsBoolean. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            nIsBoolean = GetSettingValue(vntParameters, "IsBoolean")
    '            'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
    '            If Not IsNothing(nIsBoolean) Then
    '                'UPGRADE_WARNING: Couldn't resolve default property of object nIsBoolean. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                cnIsBoolean = CShort(nIsBoolean)
    '            Else
    '                cnIsBoolean = 0
    '            End If

    '            '
    '            ' Get the label values passed in.
    '            '
    '            'UPGRADE_WARNING: Couldn't resolve default property of object GetSettingValue(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            'UPGRADE_WARNING: Couldn't resolve default property of object vntLabels. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '            vntLabels = GetSettingValue(vntParameters, "BLabels")
    '            'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
    '            If Not IsNothing(vntLabels) And IsArray(vntLabels) Then
    '                j = UBound(vntLabels) - LBound(vntLabels) + 1
    '                If j > 1 Then
    '                    'UPGRADE_WARNING: Lower bound of array cvntLabels was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    '                    ReDim cvntLabels(j)
    '                    j = 1
    '                    For i = LBound(vntLabels) To UBound(vntLabels)
    '                        'UPGRADE_WARNING: Couldn't resolve default property of object vntLabels(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                        cvntLabels(j) = CStr(vntLabels(i))
    '                        j = j + 1
    '                    Next i
    '                Else ' We were not passed two values for labels.
    '                    'UPGRADE_WARNING: Lower bound of array cvntLabels was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    '                    ReDim cvntLabels(3)
    '                    If j = 1 Then ' We at least go one label value -- use it.
    '                        'UPGRADE_WARNING: Couldn't resolve default property of object vntLabels(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
    '                        cvntLabels(1) = CStr(vntLabels(LBound(vntLabels)))
    '                    Else
    '                        cvntLabels(1) = "No Label"
    '                    End If
    '                    cvntLabels(2) = ""
    '                    cvntLabels(3) = ""
    '                End If
    '            Else ' No label values provided.
    '                'UPGRADE_WARNING: Lower bound of array cvntLabels was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    '                ReDim cvntLabels(3)
    '                cvntLabels(1) = ""
    '                cvntLabels(2) = ""
    '                cvntLabels(3) = ""
    '            End If

    '            Init = True

    '            If (outcome Is Nothing) Then
    '                'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "Error.  Main Variable cannot be empty.")
    '                Init = False
    '            End If

    '            If (Not weight Is Nothing) And (strata Is Nothing) And (psu Is Nothing) Then
    '                'goLogFile.Log(System.Diagnostics.TraceEventType.Error, "Error.  Must specify Stratification variable and PSU variable.")
    '                Init = False
    '            End If


    '        Else
    '            Init = False
    '        End If

    '        Exit Function

    'ErrorHandler:

    '        Init = False
    '        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    '    End Function


    Public Function FirstPass(ByRef errorMessage As String) As Integer

        Const PROC_Name As String = "clsCTables::FirstPass"

        '
        '{$IFDEF USE_TV}
        'function FirstPass: word;
        '{$ELSE}
        'procedure FirstPass;
        '{$ENDIF}
        'Var
        '    Pd : PDom;
        '    P : Ptotal;
        '    n : integer;
        '    cate, doma : string;
        'begin
        '  n := 0;
        '{$IFDEF USE_TV}
        '  firstPass := cmNo;
        '{$ENDIF}
        '{$IFDEF USE_CRT}
        '  ClrScr;
        '  gotoxy(3,5);
        '  write('Record No.');
        '{$ENDIF}
        '  While (MyFile.GetNext>=0) do
        '      begin
        '      inc(n);
        '      If (ClusterApp.Progress(n, myFile.NumRecs) = cmQuit) Then
        '        exit;
        '      If ValidCase Then
        '         begin
        '         If (Domain <> nil) Then
        '             begin
        '             doma := Domain^.FieldEntry;
        '             Pd:=findDom(doma);
        '             If Pd = nil Then
        '                AddDom(doma,Pd)
        '             Else
        '                begin
        '                Pd^.SumW:=Pd^.SumW+GetWeight;
        '                Pd^.n:=Pd^.n+1;
        '                end;
        '             End
        '         Else
        '             begin
        '             doma:='Total';
        '             Pd:=firstDom;
        '             If Pd = nil Then
        '                Adddom(doma,Pd)
        '             Else
        '                begin
        '                Pd^.SumW:=Pd^.SumW+GetWeight;
        '                Pd^.n:=Pd^.n+1;
        '                totalDom:=Pd;
        '                end;
        '             end;
        '         cate := Outcome^.FieldEntry;
        '         P:=findCat(Pd^.firstCat,cate);
        '         If P = nil Then
        '            AddCat(cate,doma)
        '         Else
        '            AccumYE(P);
        '         End
        '      Else
        '         inc(mis);
        '    end;
        '    firstPass := cmOK;
        'end;
        '

        Dim Pd As CSDomain
        Dim P As CSTotal
        Dim cate As Object
        Dim doma As Object

        On Error GoTo ErrorHandler

        FirstPass = MsgBoxResult.No

        While (GetNextRow()) 'While (MyFile.GetNext >= 0)

            If ValidCase() Then
                If (Not domain Is Nothing) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Domain.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object doma. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    doma = domain.FieldEntry
                    Pd = FindDom(doma)
                    If Pd Is Nothing Then
                        Pd = AddDom(doma)
                    Else
                        Pd.SumW = Pd.SumW + GetWeight()
                        Pd.N = Pd.N + 1
                    End If
                Else
                    'UPGRADE_WARNING: Couldn't resolve default property of object doma. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    doma = "TOTAL"
                    Pd = FirstDom
                    If Pd Is Nothing Then
                        Pd = AddDom(doma)
                    Else
                        Pd.SumW = Pd.SumW + GetWeight()
                        Pd.N = Pd.N + 1
                        TotalDom = Pd
                    End If
                End If
                'UPGRADE_WARNING: Couldn't resolve default property of object Outcome.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object cate. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                cate = outcome.FieldEntry
                P = FindCat((Pd.FirstCat), cate)
                If P Is Nothing Then
                    AddCat(cate, doma)
                Else
                    AccumYE(P)
                End If

            Else

                Mis = Mis + 1

            End If

        End While

        FirstPass = MsgBoxResult.Ok

        Exit Function

ErrorHandler:

        FirstPass = -1
        errorMessage = Err.Description
        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Public Sub ComputeTot(ByRef errorMessage As String)

        Const PROC_Name As String = "clsCTables::ComputeTot"

        Dim Pd As CSDomain
        Dim Pt As CSDomain
        Dim Pc As CSCategory
        Dim P As CSTotal
        Dim Ptr As CSTotal

        On Error GoTo ErrorHandler

        Pd = AddDom("TOTAL")
        Pd.SumW = 0
        Pd.N = 0
        TotalDom = Pd

        Pt = FirstDom
        While Not (Pt Is Pd)
            Pd.SumW = Pd.SumW + Pt.SumW
            Pd.N = Pd.N + Pt.N
            Pt = Pt.NextDom
        End While
        Ptr = Pd.FirstCat
        Pc = FirstCat
        While (Not Pc Is Nothing)
            P = Pc.FirstDom
            While (Not P Is Ptr)
                'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Ptr.YE = Ptr.YE + P.YE
                Ptr.N = Ptr.N + P.N
                P = P.NextDom
            End While
            Pc = Pc.NextCat
            Ptr = Ptr.NextCat
        End While

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Sub Accumqha(ByRef P As CSTotal, ByRef Pd As CSDomain)

        Const PROC_Name As String = "clsCTables::Accumqha"

        Dim Qhab As Object
        Dim Ptr As CSTotal

        On Error GoTo ErrorHandler

        'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Qhab = 0.0#
        If (Pd.SumW > 0) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object P.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Qhab = (1 - (P.YE / Pd.SumW)) * (GetWeight() / Pd.SumW)
            'UPGRADE_WARNING: Couldn't resolve default property of object Qhab. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object P.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            P.qha = P.qha + Qhab
            Ptr = Pd.FirstCat
            While Not Ptr Is Nothing
                If (Not Ptr Is P) Then
                    'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.YE. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object Ptr.qha. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    Ptr.qha = Ptr.qha - (Ptr.YE / Pd.SumW) * (GetWeight() / Pd.SumW)
                End If
                Ptr = Ptr.NextCat
            End While
        End If

        Exit Sub

ErrorHandler:

        'goLogFile.Log(System.Diagnostics.TraceEventType.Error, PROC_Name & " -- Error #" & Err.Number & " -- " & Err.Description)

    End Sub

    Public Function ExpOf(ByRef X As Object) As Object

        Dim Ex As Object
        Dim LastEx As Object
        Dim Term As Object
        Dim N As Object

        'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Ex. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Ex = CDec(X) + 1.0#
        'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Term. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Term = CDec(X)
        'UPGRADE_WARNING: Couldn't resolve default property of object N. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        N = CDec(1.0#)

        Do
            'UPGRADE_WARNING: Couldn't resolve default property of object N. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            N = N + 1.0#

            'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Term. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Term = Term * X

            'UPGRADE_WARNING: Couldn't resolve default property of object N. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Term. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Term = Term / N

            'UPGRADE_WARNING: Couldn't resolve default property of object Ex. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object LastEx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            LastEx = Ex
            'UPGRADE_WARNING: Couldn't resolve default property of object Term. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Ex. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Ex = Ex + Term
            'UPGRADE_WARNING: Couldn't resolve default property of object LastEx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Ex. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Loop While (Ex <> LastEx)

        'UPGRADE_WARNING: Couldn't resolve default property of object Ex. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object ExpOf. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ExpOf = Ex

    End Function


    Public Function LnOf(ByRef X As Object) As Object

        Dim Y As Object
        Dim Y2 As Object
        Dim num As Object
        Dim Den As Object
        Dim LastLn As Object

        'UPGRADE_WARNING: Couldn't resolve default property of object X. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Y = (CDec(X) - CDec(1.0#)) / (CDec(X) + CDec(1.0#))
        'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object Y2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Y2 = CDec(Y * Y)
        LnOf = CDec(0.0#)
        'UPGRADE_WARNING: Couldn't resolve default property of object Y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object num. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        num = CDec(Y)
        'UPGRADE_WARNING: Couldn't resolve default property of object Den. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Den = CDec(1.0#)

        Do
            'UPGRADE_WARNING: Couldn't resolve default property of object LastLn. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            LastLn = CDec(LnOf)
            'UPGRADE_WARNING: Couldn't resolve default property of object Den. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object num. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object LnOf. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            LnOf = LnOf + num / Den

            'UPGRADE_WARNING: Couldn't resolve default property of object Y2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object num. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            num = num * Y2
            'UPGRADE_WARNING: Couldn't resolve default property of object Den. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Den = Den + 2
            'UPGRADE_WARNING: Couldn't resolve default property of object LastLn. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object LnOf. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Loop While (LnOf <> LastLn)

        'UPGRADE_WARNING: Couldn't resolve default property of object LnOf. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        LnOf = LnOf * 2

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

                'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                i = Int(Val(CStr(s)))
                FieldColl = p1.FieldInt - i

            Else

                'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                R = Val(CStr(s))
                'UPGRADE_WARNING: Couldn't resolve default property of object p1.FieldReal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
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

            'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object p1.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If p1.FieldEntry > s Then
                FieldColl = 1
                'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object p1.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ElseIf p1.FieldEntry < s Then
                FieldColl = -1
                'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object p1.FieldEntry. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ElseIf p1.FieldEntry = s Then
                FieldColl = 0
            End If

        End If

        Exit Function

ErrorHandler:

        FieldColl = 0
        MsgBox("Error #" & Err.Number & " -- " & Err.Description)

    End Function

    Private Sub ResetReader()

        Row = 0

    End Sub

End Class