Option Strict Off
Option Explicit On
Option Compare Text
<System.Runtime.InteropServices.ProgId("cTable_NET.cTable")> Public Class cTable
    Implements EpiInfo.Plugin.IAnalysisStatistic
    'Implements IEpiFace.IEpi
    'Option Explicit

    '   The following constant controls conditional compilation
    '   If True, it uses the VB version of MartinBB, developed as part of version 1.0.3
    '   If False, it uses the Pascal version of MartinBB, which was giving problems in non-English versions of the OS
    '   When using the VB version, an object must be created/destroyed on init/terminate
    '   When using the Pascal version, it is accessed via Declare and output must substitute for periods and commas returned by it
#Const VBMARTIN = False

    Dim results As Object
    Dim apppath As String
    'UPGRADE_WARNING: Lower bound of array errmartin was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    Dim errmartin(2, 9) As String
    Dim sComma, sDot As String '   substitution characters
    Dim mvaResult(,) As Object
    Dim bError As Boolean

#If VBMARTIN Then
	'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression VBMARTIN did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
	Dim moMartin As Object
	Const VBEXACT = "EIExact.cExact"
#Else
    Private Declare Function Strat2x2 Lib "martinbb.dll" (ByVal TableArray As Object, ByVal ConfLevel As Double, ByRef ResultArray As Object) As Object
#End If

    Private Const errmsg As String = "<tt><tlt>Note: Unable to do Mid-P and Fisher Exact.</tlt></tt>"

    Private DataArray As Object
    Private ColumnNames As Object
    Private NumColumns As Integer
    Private NumRows As Integer
    Private settings As Object
    Private Numstrata As Short
    Dim dist1 As New statlib
    'UPGRADE_WARNING: Lower bound of array lable was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    Private lable(9) As String
    'UPGRADE_WARNING: Lower bound of array value was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
    Private value(9) As Double

    Dim sInter, sMinimal, sAdvanced As String

    'UPGRADE_NOTE: Class_Initialize was upgraded to Class_Initialize_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub Class_Initialize_Renamed()
#If VBMARTIN Then
		'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression VBMARTIN did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
		Set moMartin = CreateObject(VBEXACT)
#Else
        NumRows = NumRows '   Avoid a no code in initialize situation
#End If
    End Sub
    Public Sub New()
        MyBase.New()
        Class_Initialize_Renamed()
    End Sub

    'UPGRADE_NOTE: Class_Terminate was upgraded to Class_Terminate_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub Class_Terminate_Renamed()
#If VBMARTIN Then
		'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression VBMARTIN did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
		Set moMartin = Nothing
#Else
        NumRows = NumRows '   Avoid a no code in terminate situation
#End If
        'Beep() ' This is causing incessent beeping in the Dashboard. I have no idea why BEEP is called here. -- E. Knudsen 6/10/2012
    End Sub
    Protected Overrides Sub Finalize()
        Class_Terminate_Renamed()
        MyBase.Finalize()
    End Sub

    Public WriteOnly Property IEpi_Settings() As Object 'Implements 'IEpiFace.IEpi.Settings
        Set(ByVal Value As Object)
            'UPGRADE_WARNING: Couldn't resolve default property of object RHS. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object settings. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            settings = Value
        End Set
    End Property

    Public WriteOnly Property IEpi_DataArray() As Object 'Implements 'IEpiFace.IEpi.DataArray
        Set(ByVal Value As Object)
            'UPGRADE_WARNING: Couldn't resolve default property of object RHS. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            DataArray = Value
        End Set
    End Property

    Public ReadOnly Property IEpi_ResultArray() As Object 'Implements 'IEpiFace.IEpi.ResultArray
        Get
            Dim s As String
            On Error GoTo probIEpi_ResultArray
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            s = mvaResult(1, 1)
            'UPGRADE_WARNING: Couldn't resolve default property of object IEpi_ResultArray. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            IEpi_ResultArray = VB6.CopyArray(mvaResult)
            Exit Property
probIEpi_ResultArray:
            'UPGRADE_WARNING: Lower bound of array mvaResult was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim mvaResult(2, 1)
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(1, 1) = "<tlt>ERROR</tlt>"
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(2, 1) = "<tlt>No results</tlt>"
            Resume Next
        End Get
    End Property

    Public Function IEpi_DoFunction(ByRef FunctionName As String) As String 'Implements IEpiFace.IEpi.DoFunction
        Select Case FunctionName
            Case "Table"
                IEpi_DoFunction = Table()
        End Select
    End Function

    Private Function Table() As String
        'UPGRADE_WARNING: Lower bound of array Chi was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Chi(3) As Double
        'UPGRADE_WARNING: Lower bound of array ChiP was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim ChiP(3) As Double
        Dim d, b, a, c, P As Double
        'UPGRADE_WARNING: Lower bound of array Lower was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Lower(10) As Double
        'UPGRADE_WARNING: Lower bound of array Upper was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Upper(10) As Double
        'UPGRADE_WARNING: Lower bound of array PEST was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim PEST(10) As Double
        Dim i, j As Short
        Dim liStatType As Short
        Dim m1, n1, n0, m0 As Double
        Dim n, x As Double
        Dim lbWarnings As Boolean
        Dim lstrTable As String

        liStatType = -1
        sMinimal = ""
        sInter = ""
        sAdvanced = ""

        lbWarnings = True
        If IsArray(settings) Then
            On Error Resume Next
            For i = LBound(settings, 2) To UBound(settings, 2)
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(1, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If settings(1, i) = "P" Then P = settings(2, i)
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(1, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If settings(1, i) = "StatType" Then liStatType = settings(2, i)
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(1, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If settings(1, i) = "WARNINGS" Then lbWarnings = settings(2, i)
            Next i
            On Error GoTo 0
        End If
        '  P = CDbl(settings(1))
        If P <= 0 Or P >= 1 Then
            P = 0.95
        End If
        If NumRows = 2 And NumColumns = 2 Then
            For i = 1 To Numstrata
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                a = CDbl(DataArray(1, 1, i))
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                b = CDbl(DataArray(1, 2, i))
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                c = CDbl(DataArray(2, 1, i))
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                d = CDbl(DataArray(2, 2, i))
                n1 = a + b : n0 = c + d
                m1 = a + c : m0 = b + d
                n = m1 + m0
                If n1 < 0.00001 Or n0 < 0.000001 Or m1 < 0.000001 Or m0 < 0.00001 Then
                    lstrTable = lstrTable & "<P align=center><tlt>Statistics cannot be computed because at least one row or column total is zero.</tlt></p>"
                    Exit Function
                End If
                x = m1 / n * n1
                If x < 5 Or m1 - x < 5 Or n1 - x < 5 Or n - m1 - n1 + x < 5 Then
                    If lbWarnings Then
                        lstrTable = lstrTable & "<P align=center><tlt>Warning: The expected value of a cell is &lt;5.  Fisher Exact Test should be used.</tlt></P>"
                    End If
                End If
                sAdvanced = Space(0) : sInter = Space(0) : sMinimal = Space(0)
                SigTable(a, b, c, d, P)
            Next

            If Numstrata > 1 Then
                lstrTable = lstrTable & "<h4 align=center> <tlt>SUMMARY INFORMATION</tlt> </h4>" & TableMHstrat(P, PEST, Lower, Upper, Chi, ChiP)
            End If
        ElseIf NumRows > 1 And NumColumns > 1 Then
            ChiSqMN()
        End If
        Select Case liStatType
            Case 0
                If Len(sMinimal) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sMinimal & vbCrLf & lstrTable
            Case 1
                If Len(sInter) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sInter & vbCrLf & lstrTable
            Case 2
                If Len(sAdvanced) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sAdvanced & vbCrLf & lstrTable
            Case Else
                If Len(sAdvanced) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sAdvanced & vbCrLf & lstrTable
        End Select
        Table = lstrTable
    End Function

    Private Function GetRawData(ByRef errorMessage As String) As Boolean

    End Function

    Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
    End Sub

    Private Sub CreateSettingsFromContext()

        'On Error GoTo Errorhandler

        'com = False
        'outputLevel = 3
        'booleanLabels = "Yes;No;Missing"
        'percents = True
        'booleanValues = False
        'domain1 = String.Empty
        'domain2 = String.Empty

        'If context.SetProperties.ContainsKey("TableName") Then
        '    tableName = context.SetProperties("TableName")
        'End If

        'If context.SetProperties.ContainsKey("BLabels") Then
        '    booleanLabels = context.SetProperties("BLabels")
        'End If

        'If context.SetProperties.ContainsKey("IsBoolean") Then
        '    booleanValues = context.SetProperties("IsBoolean")
        'End If

        'For Each kvp As KeyValuePair(Of String, String) In context.InputVariableList
        '    If kvp.Key.ToLower().Equals("percents") Then
        '        percents = kvp.Value
        '    End If

        '    If kvp.Key.ToLower().Equals("stratavar") Then
        '        strataVar = kvp.Value
        '    End If

        '    If kvp.Key.ToLower().Equals("mainvar") Or kvp.Key.ToLower().Equals("identifier") Or kvp.Key.ToLower().Equals("identifier1") Or kvp.Key.ToLower().Equals("exposurevar") Then
        '        domainVar = kvp.Value
        '    End If

        '    If kvp.Key.ToLower().Equals("crosstabvar") Or kvp.Key.ToLower().Equals("identifier2") Or kvp.Key.ToLower().Equals("outcomevar") Then
        '        mainVar = kvp.Value
        '    End If

        '    If kvp.Key.ToLower().Equals("psuvar") Then
        '        psuVar = kvp.Value
        '    End If

        '    If kvp.Key.ToLower().Equals("weightvar") Then
        '        weightVar = kvp.Value
        '    End If

        '    If kvp.Key.ToLower().Equals("tablename") Then
        '        tableName = kvp.Value
        '    End If
        'Next

        'vntLabels = New List(Of String)
        'vntLabels.Add(String.Empty)
        'vntLabels.Add(String.Empty)
        'vntLabels.Add(String.Empty)

        'vntLabels(0) = context.SetProperties("RepresentationOfYes")
        'vntLabels(1) = context.SetProperties("RepresentationOfNo")
        'vntLabels(2) = context.SetProperties("RepresentationOfMissing")

        'cnOutputLevel = outputLevel
        'cbIncludePercents = percents

cleanup:
        Exit Sub

Errorhandler:
        'errorMessage = Err.Description
        'numErrors = numErrors + 1

    End Sub

    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Tables"
        End Get
    End Property

    Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute

        CreateSettingsFromContext()

        Dim errorMessage As String
        errorMessage = String.Empty

        Dim output As String
        Dim output1 As String
        Dim output2 As String

        Dim args As Dictionary(Of String, String)
        args = New Dictionary(Of String, String)

        If GetRawData(errorMessage) = False Then
            ReDim results(1, 0)
            results(0, 0) = "ERROR"
            results(1, 0) = errorMessage
            output = "<br clear=""all"" /><p align=""left""><b><tlt>" + errorMessage + "</tlt></b></p>"

            args.Add("COMMANDNAME", "TABLES")
            args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
            args.Add("HTMLRESULTS", output)
            context.Display(args)
            Exit Sub
        End If


        'UPGRADE_WARNING: Lower bound of array Chi was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Chi(3) As Double
        'UPGRADE_WARNING: Lower bound of array ChiP was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim ChiP(3) As Double
        Dim d, b, a, c, P As Double
        'UPGRADE_WARNING: Lower bound of array Lower was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Lower(10) As Double
        'UPGRADE_WARNING: Lower bound of array Upper was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Upper(10) As Double
        'UPGRADE_WARNING: Lower bound of array PEST was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim PEST(10) As Double
        Dim i, j As Short
        Dim liStatType As Short
        Dim m1, n1, n0, m0 As Double
        Dim n, x As Double
        Dim lbWarnings As Boolean
        Dim lstrTable As String

        liStatType = -1
        sMinimal = ""
        sInter = ""
        sAdvanced = ""

        lbWarnings = True
        If IsArray(settings) Then
            On Error Resume Next
            For i = LBound(settings, 2) To UBound(settings, 2)
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(1, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If settings(1, i) = "P" Then P = settings(2, i)
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(1, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If settings(1, i) = "StatType" Then liStatType = settings(2, i)
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(1, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object settings(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                If settings(1, i) = "WARNINGS" Then lbWarnings = settings(2, i)
            Next i
            On Error GoTo 0
        End If
        '  P = CDbl(settings(1))
        If P <= 0 Or P >= 1 Then
            P = 0.95
        End If
        If NumRows = 2 And NumColumns = 2 Then
            For i = 1 To Numstrata
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                a = CDbl(DataArray(1, 1, i))
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                b = CDbl(DataArray(1, 2, i))
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                c = CDbl(DataArray(2, 1, i))
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                d = CDbl(DataArray(2, 2, i))
                n1 = a + b : n0 = c + d
                m1 = a + c : m0 = b + d
                n = m1 + m0
                If n1 < 0.00001 Or n0 < 0.000001 Or m1 < 0.000001 Or m0 < 0.00001 Then
                    lstrTable = lstrTable & "<P align=center><tlt>Statistics cannot be computed because at least one row or column total is zero.</tlt></p>"
                    Exit Sub
                End If
                x = m1 / n * n1
                If x < 5 Or m1 - x < 5 Or n1 - x < 5 Or n - m1 - n1 + x < 5 Then
                    If lbWarnings Then
                        lstrTable = lstrTable & "<P align=center><tlt>Warning: The expected value of a cell is &lt;5.  Fisher Exact Test should be used.</tlt></P>"
                    End If
                End If
                sAdvanced = Space(0) : sInter = Space(0) : sMinimal = Space(0)
                SigTable(a, b, c, d, P)
            Next

            If Numstrata > 1 Then
                lstrTable = lstrTable & "<h4 align=center> <tlt>SUMMARY INFORMATION</tlt> </h4>" & TableMHstrat(P, PEST, Lower, Upper, Chi, ChiP)
            End If
        ElseIf NumRows > 1 And NumColumns > 1 Then
            ChiSqMN()
        End If
        Select Case liStatType
            Case 0
                If Len(sMinimal) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sMinimal & vbCrLf & lstrTable
            Case 1
                If Len(sInter) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sInter & vbCrLf & lstrTable
            Case 2
                If Len(sAdvanced) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sAdvanced & vbCrLf & lstrTable
            Case Else
                If Len(sAdvanced) <> 0 Then lstrTable = "<BR><H4 ALIGN=CENTER> <tlt>Single Table Analysis</tlt> </H4>" & vbCrLf & sAdvanced & vbCrLf & lstrTable
        End Select
        'Table = lstrTable
    End Sub

    Public Function SigTable(ByVal a As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double, ByVal P As Double) As SingleTableResults
        Dim i As Object
        Dim test As Object
        Dim strat As String
        Dim rd2 As Object
        Dim rd1 As Object
        Dim rd As Object
        Dim rr2 As Object
        Dim rr1 As Object
        Dim rr As Object
        Dim odr2 As Object
        Dim odr1 As Object
        Dim odr As Object
        Dim pe As Object
        Dim r As Object
        Dim ru As Object
        Dim re As Object
        Dim n As Object
        Dim m0 As Object
        Dim m1 As Object
        Dim n0 As Object
        Dim n1 As Object
        'UPGRADE_WARNING: Lower bound of array Chi was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim Chi(6) As Double
        'UPGRADE_WARNING: Lower bound of array ChiP was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim ChiP(6) As Double
        'UPGRADE_WARNING: Lower bound of array data was changed from 1,1,1 to 0,0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim data(2, 2, 1) As Object
        Dim z As Double
        Dim x As Double
        Dim sBChi As String

        If P < 0.95 + 0.00001 And P > 0.95 - 0.00001 Then
            z = 1.96
        ElseIf P < 0.99 + 0.00001 And P > 0.99 - 0.00001 Then
            z = 2.58
        ElseIf P < 0.9 + 0.00001 And P > 0.9 - 0.00001 Then
            z = 1.64
        Else
            z = dist1.ZFROMP(P)
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object data(1, 1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        data(1, 1, 1) = a
        'UPGRADE_WARNING: Couldn't resolve default property of object data(1, 2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        data(1, 2, 1) = b
        'UPGRADE_WARNING: Couldn't resolve default property of object data(2, 1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        data(2, 1, 1) = c
        'UPGRADE_WARNING: Couldn't resolve default property of object data(2, 2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        data(2, 2, 1) = d
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        n1 = a + b
        'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        n0 = c + d
        'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        m1 = a + c
        'UPGRADE_WARNING: Couldn't resolve default property of object m0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        m0 = b + d
        'UPGRADE_WARNING: Couldn't resolve default property of object m0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        n = m1 + m0
        ' Look for cells with low expected values
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object re. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        re = a / n1
        'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object ru. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ru = c / n0
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object r. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        r = m1 / n
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object pe. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pe = n1 / n
        If b * c < 0.00000001 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object odr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            odr = -1
            'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            odr1 = -1
            'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            odr2 = -1
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object odr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            odr = (a * d) / (b * c)
            'afpor = 100 * (b / m0) * (ODR - 1) / ((b / m0) * (ODR - 1) + 1)
            'afeor = 100 * (ODR - 1) / ODR
            'pfpor = 100 * (b / m0) * (1 - ODR)
            'pfeor = 100 * (1 - ODR)
            ' 95% confidence limits for odds ratio
            If d * a < 0.000001 Then
                'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                odr1 = -1
                'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                odr2 = -1
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                odr1 = System.Math.Exp(System.Math.Log((a * d) / (b * c)) - z * (1 / a + 1 / b + 1 / c + 1 / d) ^ 0.5)
                'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                odr2 = System.Math.Exp(System.Math.Log((a * d) / (b * c)) + z * (1 / a + 1 / b + 1 / c + 1 / d) ^ 0.5)
                ' 95% confidence limits for risk ratio
            End If
        End If
        ''  If a * c < 0.00001 Then
        If ru < 0.00001 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            rr = -1
            'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            rr1 = -1
            'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            rr2 = -1
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object ru. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object re. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            rr = re / ru
            'afb = ((r - ru) / r) * 100
            'afe = ((rr - 1) / rr) * 100
            'pfp = ((ru - m1 / n) / ru) * 100
            'pfe = (1 - rr) * 100
            If re < 0.00001 Then
                'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                rr1 = -1
                'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                rr2 = -1
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                rr1 = System.Math.Exp(System.Math.Log((a / n1) / (c / n0)) - z * (d / (c * n0) + b / (n1 * a)) ^ 0.5)
                'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                rr2 = System.Math.Exp(System.Math.Log((a / n1) / (c / n0)) + z * (d / (c * n0) + b / (n1 * a)) ^ 0.5)
            End If
        End If
        ' 95% confidence limits for risk difference
        'UPGRADE_WARNING: Couldn't resolve default property of object ru. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object re. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rd = (re - ru) * 100
        'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object ru. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object re. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rd1 = (re - ru - 1.96 * (re * (1 - re) / n1 + ru * (1 - ru) / n0) ^ 0.5) * 100
        'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object ru. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object re. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rd2 = (re - ru + 1.96 * (re * (1 - re) / n1 + ru * (1 - ru) / n0) ^ 0.5) * 100

        Dim errorMessage As String

        On Error GoTo Main_error
        bError = False

#If VBMARTIN Then
		'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression VBMARTIN did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
		strat$ = moMartin.Strat2x2(data, P, results)
#Else
        'UPGRADE_WARNING: Couldn't resolve default property of object Strat2x2(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Dim exact As EIExact
        exact = New EIExact()

        strat = exact.Strat2x2(data, P, results) 'Strat2x2(data, P, results)
        If strat.ToLower().StartsWith("error") Or strat.ToLower().StartsWith("problem") Then
            errorMessage = strat
        End If

        If Not bError Then
            Reformat(P, strat, results)
        End If

#End If
        Call Dump("SigTable1", data, P, results, strat)
        'UPGRADE_WARNING: Couldn't resolve default property of object results(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object test. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        test = results(1, 0)

        '  result = result & "<table cellspacing=20 align=center><TR><TD><TD align=center>Point <TD colspan=2 align=center>" & Int(p * 100 + 1) & "% Confidence Interval"
        '  result = result & "<TR><TD>PARAMETERS: Odds-based<TD align=center>Estimate<TD ALIGN=RIGHT>Lower<TD ALIGN=RIGHT>Upper"
        '  result = result & "<TR><TD>Odds Ratio (cross product)<TD ALIGN=RIGHT>" & IIf(ODR >= 0, Format(ODR, "##0.0000"), "Undefined")
        '  result = result & "<TD ALIGN=RIGHT>" & IIf(ODR1 >= 0, Format(ODR1, "##0.0000"), "Undefined") & ",<TD ALIGN=RIGHT>" & IIf(ODR2 >= 0, Format(ODR2, "##0.0000"), "Undefined") & "<TT> (T)</TT>"
        '  result = result & "<TR><TD>Odds Ratio (MLE)<TD ALIGN=RIGHT>" & Format(Results(2, 1), "#0.0000") & "<TD ALIGN=RIGHT>" & Format(Results(2, 4), "#0.0000") & ",<TD ALIGN=RIGHT>" & Format(Results(2, 5), "#0.0000") & "<TT> (M)</TT>"
        '  result = result & "<TR><TD><TD><TD ALIGN=RIGHT>" & Format(Results(2, 2), "#0.0000") & ",<TD ALIGN=RIGHT>" & Format(Results(2, 3), "#0.0000") & "<TT> (F)</TT>"
        '  result = result & "<TR><TD>PARAMETERS: Risk-based<TD><TD><TD>"
        '  result = result & "<TR><TD>Risk Ratio (RR)<TD ALIGN=RIGHT>" & IIf(rr >= 0, Format(rr, "##0.0000"), "Undefined") & "<TD ALIGN=RIGHT>" & IIf(rr1 >= 0, Format(rr1, "##0.0000"), "Undefined") & ",<TD ALIGN=RIGHT>" & IIf(rr2 >= 0, Format(rr2, "##0.0000"), "Undefined") & "<TT> (T)</TT>"
        '  result = result & "<TR><TD>Risk Difference (RD%)<TD ALIGN=RIGHT>" & Format(rd, "##0.0000") & "<TD ALIGN=RIGHT>" & Format(rd1, "##0.0000") & ",<TD ALIGN=RIGHT>" & Format(rd2, "##0.0000") & "<TT> (T)</TT>"
        '  result = result & "</TABLE>"
        '  result = result & "<p align=center><tt> (T=Taylor series; C=Cornfield; M=Mid-P; F=Fisher Exact)</tt></P>"
        '  result = result & TableBchi(a, b, c, d, chi, chip)
        '
        ' SigTable = result
        '  SigTable = result & "<p align=center><tt>" & errmsg & "</tt></P>"

        'Advanced Statistics
        'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sAdvanced = "<TABLE ALIGN=CENTER>" & vbCrLf & "<TR><TD><TD ALIGN=CENTER>Point " & vbCrLf & "<TD COLSPAN=2 ALIGN=CENTER>" & Int(P * 100 + 1) & "% <tlt>Confidence Interval</tlt>" & vbCrLf & "<TR><TD><TD ALIGN=CENTER><tlt>Estimate</tlt><TD ALIGN=RIGHT><tlt>Lower</tlt><TD ALIGN=RIGHT><tlt>Upper</tlt>" & vbCrLf & "<TR><TD><tlt>PARAMETERS: Odds-based </tlt><TD><TD><TD>" & vbCrLf & "<TR><TD><tlt>Odds Ratio (cross product)</tlt>" & "<TD ALIGN=RIGHT>" & IIf(odr >= 0, VB6.Format(odr, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(odr1 >= 0, VB6.Format(odr1, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(odr2 >= 0, VB6.Format(odr2, "##0.0000"), "<tlt>Undefined</tlt>") & "<TT> <tlt>(T)</tlt></TT>" & vbCrLf
        If bError Then
            sAdvanced = sAdvanced & "<TR><TD COLSPAN=4 ALIGN=CENTER>" & errmsg & "</TD></TR>"
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object results(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sAdvanced = sAdvanced & "<TR><TD><tlt>Odds Ratio (MLE)</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(results(1, 0), "#0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(results(1, 3), "#0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(results(1, 4), "#0.0000") & "<TT> <tlt>(M)</tlt></TT>" & vbCrLf & "<TR><TD><TD><TD ALIGN=RIGHT>" & VB6.Format(results(1, 1), "#0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(results(1, 2), "#0.0000") & "<TT> <tlt>(F)</tlt></TT>" & vbCrLf
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object rd2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sAdvanced = sAdvanced & "<TR><TD><tlt>PARAMETERS: Risk-based</tlt><TD><TD><TD>" & vbCrLf & "<TR><TD><tlt>Risk Ratio (RR)</tlt>" & "<TD ALIGN=RIGHT>" & IIf(rr >= 0, VB6.Format(rr, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(rr1 >= 0, VB6.Format(rr1, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(rr2 >= 0, VB6.Format(rr2, "##0.0000"), "<tlt>Undefined</tlt>") & "<TT> <tlt>(T)</tlt></TT>" & vbCrLf & "<TR><TD><tlt>Risk Difference (RD%)</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(rd, "##0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(rd1, "##0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(rd2, "##0.0000") & "<TT> <tlt>(T)</tlt></TT>" & vbCrLf
        sAdvanced = sAdvanced & "<TR><TR><TR><TR><TR>" & vbCrLf & "<TR> <TD COLSPAN=4><P ALIGN=CENTER><TT> <tlt>(T=Taylor series; C=Cornfield; M=Mid-P; F=Fisher Exact)</tlt></TT></P>" & vbCrLf
        'Intermediate Statistics
        'UPGRADE_WARNING: Couldn't resolve default property of object rd2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rd. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sInter = "<TABLE ALIGN=CENTER>" & vbCrLf & "<TR><TD><TD ALIGN=CENTER><tlt>Point</tlt> " & "<TD COLSPAN=2 ALIGN=CENTER>" & Int(P * 100 + 1) & "<tlt>% Confidence Interval</tlt>" & vbCrLf & "<TR><TD><TD ALIGN=CENTER><tlt>Estimate</tlt><TD ALIGN=RIGHT><tlt>Lower</tlt><TD ALIGN=RIGHT><tlt>Upper</tlt>" & vbCrLf & "<TR><TD>Odds Ratio (cross product)" & "<TD ALIGN=RIGHT>" & IIf(odr >= 0, VB6.Format(odr, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(odr1 >= 0, VB6.Format(odr1, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(odr2 >= 0, VB6.Format(odr2, "##0.0000"), "<tlt>Undefined</tlt>") & "<TT> <tlt>(T)</tlt></TT>" & vbCrLf & "<TR><TD><tlt>Risk Ratio (RR)</tlt>" & "<TD ALIGN=RIGHT>" & IIf(rr >= 0, VB6.Format(rr, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(rr1 >= 0, VB6.Format(rr1, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(rr2 >= 0, VB6.Format(rr2, "##0.0000"), "<tlt>Undefined</tlt>") & "<TT> (T)</TT>" & vbCrLf & "<TR><TD><tlt>Risk Difference (RD%)</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(rd, "##0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(rd1, "##0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(rd2, "##0.0000") & "<TT> <tlt>(T)</tlt></TT>" & vbCrLf
        'Minimal Statistics
        'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sMinimal = "<TABLE ALIGN=CENTER>" & vbCrLf & "<TR><TD><TD ALIGN=CENTER><tlt>Point</tlt> " & vbCrLf & "<TD COLSPAN=2 ALIGN=CENTER>" & Int(P * 100 + 1) & "<tlt>% Confidence Interval</tlt>" & vbCrLf & "<TR><TD><TD ALIGN=CENTER><tlt>Estimate</tlt><TD ALIGN=RIGHT><tlt>Lower</tlt><TD ALIGN=RIGHT><tlt>Upper</tlt>" & vbCrLf & "<TR><TD><tlt>Odds Ratio (cross product)</tlt>" & "<TD ALIGN=RIGHT>" & IIf(odr >= 0, VB6.Format(odr, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(odr1 >= 0, VB6.Format(odr1, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(odr2 >= 0, VB6.Format(odr2, "##0.0000"), "<tlt>Undefined</tlt>") & "<TT> (T)</TT>" & vbCrLf & "<TR><TD>Risk Ratio (RR)" & "<TD ALIGN=RIGHT>" & IIf(rr >= 0, VB6.Format(rr, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(rr1 >= 0, VB6.Format(rr1, "##0.0000"), "<tlt>Undefined</tlt>") & "<TD ALIGN=RIGHT>" & IIf(rr2 >= 0, VB6.Format(rr2, "##0.0000"), "<tlt>Undefined</tlt>") & "<TT> (T)</TT>" & vbCrLf

        'for ChiSquare
        'UPGRADE_WARNING: Lower bound of array mvaResult was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mvaResult(2, 17)
        Dim extraResults As SingleTableResults
        extraResults = TableBchi(a, b, c, d, Chi, ChiP)
        'Call TableBchi(a, b, c, d, Chi, ChiP)
        'for error message
        If bError Then
            sAdvanced = sAdvanced & "</table><P ALIGN=CENTER><TT> " & errmsg & "</TT></P>" & vbCrLf
            sInter = sInter & "</table><P ALIGN=CENTER><TT> " & errmsg & "</TT></P>" & vbCrLf
            sMinimal = sMinimal & "</table><P ALIGN=CENTER><TT> " & errmsg & "</TT></P>" & vbCrLf
        Else
            sAdvanced = sAdvanced & sBChi & vbCrLf
            sInter = sInter & sBChi & vbCrLf
            sMinimal = sMinimal & sBChi & vbCrLf
        End If
        For i = 1 To 8
            'UPGRADE_WARNING: Couldn't resolve default property of object i. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Debug.Print(VB6.TabLayout(mvaResult(1, i), mvaResult(2, i)))
        Next i
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 9). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 9) = "<tlt>Odds Ratio</tlt>"
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 9). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 9) = IIf(odr = -1, System.DBNull.Value, odr)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 10). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 10) = "<tlt>Lower 95% Confidence Limit for Odds Ratio</tlt>"
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 10). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 10) = IIf(odr1 = -1, System.DBNull.Value, odr1)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 11). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 11) = "<tlt>Upper 95% Confidence Limit for Odds Ratio</tlt>"
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
        'UPGRADE_WARNING: Couldn't resolve default property of object odr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 11). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 11) = IIf(odr2 = -1, System.DBNull.Value, odr2)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 12). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 12) = "<tlt>Risk Ratio</tlt>"
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 12). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 12) = IIf(rr = -1, System.DBNull.Value, rr)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 13). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 13) = "<tlt>Lower 95% Confidence Limit for Risk Ratio</tlt>"
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 13). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 13) = IIf(rr1 = -1, System.DBNull.Value, rr1)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 14). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 14) = "<tlt>Upper 95% Confidence Limit for Risk Ratio</tlt>"
        'UPGRADE_WARNING: Use of Null/IsNull() detected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="2EED02CB-5C0E-4DC1-AE94-4FAA3A30F51A"'
        'UPGRADE_WARNING: Couldn't resolve default property of object rr2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 14). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 14) = IIf(rr2 = -1, System.DBNull.Value, rr2)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 15). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 15) = "<tlt>Risk Difference (%)</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object rd. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 15). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 15) = rd
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 16). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 16) = "<tlt>Lower 95% Confidence Limit for Risk Difference (%)</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object rd1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 16). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 16) = rd1
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 17). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 17) = "<tlt>Upper 95% Confidence Limit for Risk Difference (%)</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object rd2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 17). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 17) = rd2

        Dim expectedYY As Double
        Dim expectedYN As Double
        Dim expectedNY As Double
        Dim expectedNN As Double

        Dim n1p1q1 As Double
        Dim n2p2q2 As Double

        expectedYY = ((a + c) * (a + b)) / (a + b + c + d)
        expectedYN = ((a + c) * (c + d)) / (a + b + c + d)
        expectedNY = ((b + d) * (a + b)) / (a + b + c + d)
        expectedNN = ((b + d) * (c + d)) / (a + b + c + d)

        n1p1q1 = n1 * (a / n1) * (1 - (a / n1))
        n2p2q2 = n0 * (c / n0) * (1 - (c / n0))

        Dim tableResults As New SingleTableResults
        tableResults.LowestExpectedCellCount = CType(expectedYY, Double)
        If expectedYN < expectedYY Then
            tableResults.LowestExpectedCellCount = CType(expectedYN, Double)
        End If
        If expectedNY < expectedYN Then
            tableResults.LowestExpectedCellCount = CType(expectedNY, Double)
        End If
        If expectedNN < expectedNY Then
            tableResults.LowestExpectedCellCount = CType(expectedNN, Double)
        End If
        tableResults.LowNPQ = CType(n1p1q1, Double)
        If n2p2q2 < n1p1q1 Then
            tableResults.LowNPQ = CType(n2p2q2, Double)
        End If

        tableResults.ChiSquareMantel2P = CType(mvaResult(2, 4), Double)
        tableResults.ChiSquareMantelVal = CType(mvaResult(2, 3), Double)
        tableResults.ChiSquareUncorrected2P = CType(mvaResult(2, 2), Double)
        tableResults.ChiSquareUncorrectedVal = CType(mvaResult(2, 1), Double)
        tableResults.ChiSquareYates2P = CType(mvaResult(2, 6), Double)
        tableResults.ChiSquareYatesVal = CType(mvaResult(2, 5), Double)

        If mvaResult(2, 9) Is DBNull.Value Then
            tableResults.OddsRatioEstimate = Nothing
        Else
            tableResults.OddsRatioEstimate = CType(mvaResult(2, 9), Double)
        End If

        If mvaResult(2, 10) Is DBNull.Value Then
            tableResults.OddsRatioLower = Nothing
        Else
            tableResults.OddsRatioLower = CType(mvaResult(2, 10), Double)
        End If

        If mvaResult(2, 11) Is DBNull.Value Then
            tableResults.OddsRatioUpper = Nothing
        Else
            tableResults.OddsRatioUpper = CType(mvaResult(2, 11), Double)
        End If

        tableResults.OddsRatioMLEEstimate = CType(results(1, 0), Double)
        tableResults.OddsRatioMLEMidPLower = CType(results(1, 3), Double)
        tableResults.OddsRatioMLEMidPUpper = CType(results(1, 4), Double)
        tableResults.OddsRatioMLEFisherLower = CType(results(1, 1), Double)
        tableResults.OddsRatioMLEFisherUpper = CType(results(1, 2), Double)

        tableResults.RiskDifferenceEstimate = CType(mvaResult(2, 15), Double)
        tableResults.RiskDifferenceLower = CType(mvaResult(2, 16), Double)
        tableResults.RiskDifferenceUpper = CType(mvaResult(2, 17), Double)

        If Not mvaResult(2, 12) Is DBNull.Value Then
            tableResults.RiskRatioEstimate = CType(mvaResult(2, 12), Double)
        End If

        If Not mvaResult(2, 13) Is DBNull.Value Then
            tableResults.RiskRatioLower = CType(mvaResult(2, 13), Double)
        End If

        If Not mvaResult(2, 14) Is DBNull.Value Then
            tableResults.RiskRatioUpper = CType(mvaResult(2, 14), Double)
        End If

        tableResults.MidP = extraResults.MidP
        tableResults.FisherExactP = extraResults.FisherExactP
        tableResults.FisherExact2P = extraResults.FisherExact2P
        tableResults.ErrorMessage = errorMessage
        Return tableResults

Main_error:
        bError = True
        tableResults.ErrorMessage = errorMessage
        Resume Next
        Resume
    End Function

    Public Structure SingleTableResults
        Public OddsRatioEstimate As Nullable(Of Double)
        Public OddsRatioLower As Nullable(Of Double)
        Public OddsRatioUpper As Nullable(Of Double)
        Public OddsRatioMLEEstimate As Double
        Public OddsRatioMLEMidPLower As Double
        Public OddsRatioMLEMidPUpper As Double
        Public OddsRatioMLEFisherLower As Double
        Public OddsRatioMLEFisherUpper As Double

        Public LowestExpectedCellCount As Double
        Public LowNPQ As Double

        Public RiskRatioEstimate As Nullable(Of Double)
        Public RiskRatioLower As Nullable(Of Double)
        Public RiskRatioUpper As Nullable(Of Double)
        Public RiskDifferenceEstimate As Double
        Public RiskDifferenceLower As Double
        Public RiskDifferenceUpper As Double
        Public ChiSquareUncorrectedVal As Double
        Public ChiSquareUncorrected2P As Double
        Public ChiSquareMantelVal As Double
        Public ChiSquareMantel2P As Double
        Public ChiSquareYatesVal As Double
        Public ChiSquareYates2P As Double
        Public MidP As Double
        Public FisherExactP As Double
        Public FisherExact2P As Double
        Public ErrorMessage As String
    End Structure


    Public Function TableBchi(ByVal a As Double, ByVal b As Double, ByVal c As Double, ByVal d As Double, ByRef Chi() As Double, ByRef P() As Double) As SingleTableResults
        Dim mantel_haenszel_c As Object
        Dim mantel_haenszel As Object
        Dim yates_c As Object
        Dim McNemar As Object
        Dim McNemar_Yates As Object
        Dim McNemar_Edwards As Object
        Dim pearson As Object
        Dim phi As Object
        Dim likelihood_ratio As Object
        Dim q3 As Object
        Dim h3 As Object
        Dim n As Object
        Dim m0 As Object
        Dim m1 As Object
        Dim n0 As Object
        Dim n1 As Object
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        n1 = a + b
        'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        n0 = c + d
        'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        m1 = a + c
        'UPGRADE_WARNING: Couldn't resolve default property of object m0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        m0 = b + d
        'UPGRADE_WARNING: Couldn't resolve default property of object m0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        n = m1 + m0
        'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object m0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        h3 = m1 * m0 * n1 * n0
        If a > 0 And b > 0 And c > 0 And d > 0 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 + System.Math.Log(n) * n
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 + System.Math.Log(a) * a
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 + System.Math.Log(b) * b
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 + System.Math.Log(c) * c
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 + System.Math.Log(d) * d
            'UPGRADE_WARNING: Couldn't resolve default property of object m1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 - System.Math.Log(m1) * m1
            'UPGRADE_WARNING: Couldn't resolve default property of object m0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 - System.Math.Log(m0) * m0
            'UPGRADE_WARNING: Couldn't resolve default property of object n1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 - System.Math.Log(n1) * n1
            'UPGRADE_WARNING: Couldn't resolve default property of object n0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            q3 = q3 - System.Math.Log(n0) * n0
            'UPGRADE_WARNING: Couldn't resolve default property of object q3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object likelihood_ratio. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            likelihood_ratio = System.Math.Abs(q3 * 2)
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object phi. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'If h3 = 0 Then
        '    Throw New System.Exception("TableBchi: h3 == 0.")
        'End If

        phi = ((a * d) - b * c) / System.Math.Sqrt(h3)
        'UPGRADE_WARNING: Couldn't resolve default property of object phi. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object pearson. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pearson = n * phi ^ 2
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object yates_c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        yates_c = (n / h3) * (System.Math.Abs(a * d - b * c) - n * 0.5) ^ 2
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mantel_haenszel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        McNemar = (b - c) ^ 2 / (b + c)
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mantel_haenszel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        McNemar_Yates = (System.Math.Abs(b - c) - 0.5) ^ 2 / (b + c)
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mantel_haenszel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        McNemar_Edwards = (System.Math.Abs(b - c) - 1) ^ 2 / (b + c)
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mantel_haenszel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mantel_haenszel = (n - 1) / h3 * (a * d - b * c) ^ 2
        'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object h3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object mantel_haenszel_c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mantel_haenszel_c = (n - 1) / h3 * (System.Math.Abs(a * d - b * c) - n * 0.5) ^ 2
        'UPGRADE_WARNING: Couldn't resolve default property of object pearson. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Chi(1) = pearson
        'UPGRADE_WARNING: Couldn't resolve default property of object mantel_haenszel. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Chi(2) = mantel_haenszel
        'On Error GoTo Main_error
        'UPGRADE_WARNING: Couldn't resolve default property of object yates_c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Chi(3) = yates_c
        Chi(4) = McNemar
        Chi(5) = McNemar_Yates
        Chi(6) = McNemar_Edwards
        If Chi(1) >= 100 Then
            P(1) = 0
            P(2) = 0
            P(3) = 0
            P(4) = 0
            P(5) = 0
            P(6) = 0
        Else
            P(1) = dist1.PfromX2(Chi(1), 1)
            P(2) = dist1.PfromX2(Chi(2), 1)
            P(3) = dist1.PfromX2(Chi(3), 1)
            P(4) = dist1.PfromX2(Chi(4), 1)
            P(5) = dist1.PfromX2(Chi(5), 1)
            P(6) = dist1.PfromX2(Chi(6), 1)
        End If

        '  TableBchi = "<table cellspacing=20 align=center><TR><TD>STATISTICAL TESTS<TD>Chi-square<TD>1-tailed p<TD>2-tailed p"
        '  TableBchi = TableBchi & "<TR><TD>Chi square - uncorrected<TD ALIGN=RIGHT>" & Format(chi(1), "##0.0000") & "<TD ALIGN=RIGHT>" & "  " & "<TD ALIGN=RIGHT>" & Format(p(1), "  ##0.0000000000")
        '  TableBchi = TableBchi & "<TR><TD>Chi square - Mantel-Haenszel<TD ALIGN=RIGHT>" & Format(chi(2), "##0.0000") & "<TD><TD ALIGN=RIGHT> " & Format(p(2), "##0.0000000000")
        '  TableBchi = TableBchi & "<TR><TD>Chi square - corrected (Yates)<TD ALIGN=RIGHT>" & Format(chi(3), "##0.0000") & "<TD><TD ALIGN=RIGHT>" & Format(p(3), " ##0.0000000000")
        '  TableBchi = TableBchi & "<TR><TD>Mid-p exact<TD><TD ALIGN=RIGHT>" & Format(Results(2, 9), "#0.0000000000") & "<TD>"
        '  TableBchi = TableBchi & "<TR><TD>Fisher exact<TD><TD ALIGN=RIGHT>" & Format(Results(2, 7), "#0.0000000000") & "<TD>"
        '  TableBchi = TableBchi & "</TABLE>"
        '  TableBchi = TableBchi & "<P>" & errmsg & "</P>"

        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 1) = "<tlt>Chi-square - uncorrected</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 1) = Chi(1)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 2) = "<tlt>2-tailed p</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 2) = P(1)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 3) = "<tlt>Chi-square - Mantel-Haenszel</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 3) = Chi(2)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 4) = "<tlt>2-tailed p</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 4) = P(2)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 5). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 5) = "<tlt>Chi-square - corrected (Yates)</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 5). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 5) = Chi(3)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 6). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 6) = "<tlt>2-tailed p</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 6). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 6) = P(3)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 7). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 7) = "<tlt>Mid-p exact</tlt>"
        'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        If Not IsNothing(results) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object results(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 7). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(2, 7) = IIf(results(1, 7) < results(1, 8), results(1, 7), results(1, 8))
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 8). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(1, 8) = "<tlt>Fisher exact</tlt>"
            'UPGRADE_WARNING: Couldn't resolve default property of object results(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 8). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(2, 8) = IIf(results(1, 5) < results(1, 6), results(1, 5), results(1, 6))

            mvaResult(1, 9) = "<tlt>Fisher exact 2-tailed p</tlt>"
            mvaResult(2, 9) = results(1, 9)
        End If

        'Advanced Statistics
        sAdvanced = sAdvanced & "<TR><TR><TR><TR><TR>" & vbCrLf & "<TR><TD><tlt>STATISTICAL TESTS</tlt><TD><tlt>Chi-square</tlt><TD><tlt>1-tailed p</tlt><TD><tlt>2-tailed p</tlt>" & "<TR><TD><tlt>Chi-square - uncorrected</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(1), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT>" & VB6.Format(P(1), "  ##0.0000000000") & vbCrLf & "<TR><TD><tlt>Chi-square - Mantel-Haenszel</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(2), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT> " & VB6.Format(P(2), "##0.0000000000") & vbCrLf & "<TR><TD><tlt>Chi-square - corrected (Yates)</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(3), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT>" & VB6.Format(P(3), " ##0.0000000000") & vbCrLf
        'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        If Not IsNothing(results) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object results(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sAdvanced = sAdvanced & "<TR><TD><tlt>Mid-p exact</tlt> <TD> " & "<TD ALIGN=RIGHT>" & VB6.Format(IIf(results(1, 7) < results(1, 8), results(1, 7), results(1, 8)), "#0.0000000000") & "<TD>" & vbCrLf & "<TR><TD><tlt>Fisher exact</tlt><TD> " & "<TD ALIGN=RIGHT>" & VB6.Format(IIf(results(1, 5) < results(1, 6), results(1, 5), results(1, 6)), "#0.0000000000") & "<TD>" & vbCrLf
        End If
        sAdvanced = sAdvanced & "</TABLE>" & vbCrLf

        'Intermediate Statistics
        sInter = sInter & "<TR><TR><TR><TR><TR>" & vbCrLf & "<TR><TD><TD><TD><tlt>1-tailed p<TD>2-tailed p</tlt>" & "<TR><TD><tlt>Chi-square - uncorrected</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(1), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT>" & VB6.Format(P(1), "  ##0.0000000000") & vbCrLf & "<TR><TD><tlt>Chi-square - Mantel-Haenszel</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(2), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT> " & VB6.Format(P(2), "##0.0000000000") & vbCrLf & "<TR><TD><tlt>Chi-square - corrected (Yates)</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(3), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT>" & VB6.Format(P(3), " ##0.0000000000") & vbCrLf & "<TR><TD><tlt>Fisher exact</tlt><TD> "
        'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        If Not IsNothing(results) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object results(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            sInter = sInter & "<TD ALIGN=RIGHT>" & VB6.Format(IIf(results(1, 5) < results(1, 6), results(1, 5), results(1, 6)), "#0.0000000000") & "<TD>" & vbCrLf
        End If
        sInter = sInter & "</TABLE>" & vbCrLf

        'Minimal Statistics
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        sMinimal = sMinimal & "<TR><TR><TR><TR><TR>" & vbCrLf & "<TR><TD><TD><TD><tlt>1-tailed p</tlt><TD><tlt>2-tailed p</tlt>" & "<TR><TD><tlt>Chi-square - Mantel-Haenszel</tlt>" & "<TD ALIGN=RIGHT>" & VB6.Format(Chi(2), "##0.0000") & "<TD>" & "<TD ALIGN=RIGHT> " & VB6.Format(P(2), "##0.0000000000") & vbCrLf & "<TR><TD><tlt>Fisher exact</tlt><TD> " & "<TD ALIGN=RIGHT>" & VB6.Format(IIf(mvaResult(2, 8) < mvaResult(2, 9), mvaResult(2, 8), mvaResult(2, 9)), "#0.0000000000") & "<TD>" & vbCrLf & "</TABLE>" & vbCrLf

        'Exit Function
        'Main_error:
        ' errmsg = errmsg & Err.Description & "<br>"
        ' Resume Next

        Dim tableResults As New SingleTableResults
        tableResults.MidP = CType(mvaResult(2, 7), Double)
        tableResults.FisherExactP = CType(mvaResult(2, 8), Double)
        tableResults.FisherExact2P = CType(mvaResult(2, 9), Double)
        'tableResults.OddsRatioMLEEstimate = extraResults.OddsRatioMLEEstimate
        'tableResults.OddsRatioMLEMidPLower = extraResults.OddsRatioMLEMidPLower
        'tableResults.OddsRatioMLEMidPUpper = extraResults.OddsRatioMLEMidPUpper
        'tableResults.OddsRatioMLEFisherLower = extraResults.OddsRatioMLEFisherLower
        'tableResults.OddsRatioMLEFisherUpper = extraResults.OddsRatioMLEFisherUpper
        Return tableResults

    End Function
    Public Function TableMHstrat(ByVal P As Double, ByRef PEstimate() As Double, ByRef Lower() As Double, ByRef Upper() As Double, ByRef Chi() As Double, ByRef ChiP() As Double) As String
        Dim t As Object
        Dim rr As Object

        Dim i As Short
        Dim z As Double '   p equivalent z-score
        Dim a() As Double '   Table cells for each stratum
        Dim b() As Double
        Dim c() As Double
        Dim d() As Double
        Dim a1 As Double '   Table cells for pooled estimate
        Dim b1 As Double
        Dim c1 As Double
        Dim d1 As Double
        Dim m1 As Double '   Row, column and overall totals
        Dim m0 As Double
        Dim n1 As Double
        Dim n0 As Double
        Dim n As Double
        Dim e As Double '   Stratum temporaries and accumulators for OR confidence intervals
        Dim f As Double
        Dim r As Double
        Dim s As Double
        Dim n2 As Double
        Dim q0 As Double
        Dim p0 As Double
        Dim r0 As Double
        Dim s1 As Double
        Dim p1 As Double
        Dim p2 As Double
        Dim p3 As Double
        Dim rr1 As Double '   Accumlators for RR
        Dim rr2 As Double
        Dim crr1 As Double '   Accumulator for confidence interval for RR
        Dim v As Double '   Stratum temporary and accumulators for chi-squared
        Dim v0 As Double
        Dim v00 As Double
        Dim re As Double '   Individual risks, ratio and difference
        Dim ru As Double
        Dim uncRR As Double
        Dim uncRD As Double
        Dim uncRR1 As Double '   Confidence intervals on RR and RD
        Dim uncRR2 As Double
        Dim uncRD1 As Double
        Dim uncRD2 As Double
        Dim uncODR As Double '   Odds ratio and CIs
        Dim uncODR1 As Double
        Dim uncODR2 As Double
        Dim g1 As Double '   M-H temporary, OR, RR and CIs
        Dim o1 As Double
        Dim seor As Double
        Dim se As Double
        Dim ORLower As Double
        Dim ORUpper As Double
        Dim RRLower As Double
        Dim RRUpper As Double
        Dim x3 As Double '   M-H summary chi sq
        Dim lnLnPoolOR As Double '   Rothman version 1 test
        Dim lnLnPoolRR As Double
        Dim lnLnPoolORDenom As Double
        Dim lnLnPoolRRDenom As Double
        Dim lnLnStratOR() As Double
        Dim lnLnStratRR() As Double
        Dim lnVarLnOR() As Double
        Dim lnVarLnRR() As Double
        Dim lnChiSqOR As Double
        Dim lnChiSqRR As Double
        Dim lbWolfError As Boolean
        'UPGRADE_WARNING: Lower bound of array ldaUncData was changed from 1,1,1 to 0,0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim ldaUncData(2, 2, 1) As Double '   For passing table cells to exact statistics
        Dim strat As String '   Exact result string
        Dim lvaTemp() As Object '
        Dim lv1 As Object

        sMinimal = ""
        sInter = ""
        sAdvanced = ""

        '   Get the z value corresponding to P
        If P < 0.95 + 0.00001 And P > 0.95 - 0.00001 Then
            z = 1.96
        ElseIf P < 0.99 + 0.00001 And P > 0.99 - 0.00001 Then
            z = 2.58
        ElseIf P < 0.9 + 0.00001 And P > 0.9 - 0.00001 Then
            z = 1.64
        Else
            z = dist1.ZFROMP(P)
        End If

        '   Fill the cell arrays and the pooled estimate
        ReDim a(Numstrata)
        ReDim b(Numstrata)
        ReDim c(Numstrata)
        ReDim d(Numstrata)
        ReDim lnLnStratOR(Numstrata)
        ReDim lnLnStratRR(Numstrata)
        ReDim lnVarLnOR(Numstrata)
        ReDim lnVarLnRR(Numstrata)
        For i = 0 To Numstrata - 1
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            a(i) = DataArray(0, 0, i)
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            b(i) = DataArray(0, 1, i)
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            c(i) = DataArray(1, 0, i)
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            d(i) = DataArray(1, 1, i)
            a1 = a(i) + a1
            b1 = b(i) + b1
            c1 = c(i) + c1
            d1 = d(i) + d1
        Next

        '   Accumulate over strata
        For i = 0 To Numstrata - 1
            m1 = a(i) + c(i)
            m0 = b(i) + d(i)
            n1 = a(i) + b(i)
            n0 = c(i) + d(i)
            n = m1 + m0
            If m1 < 0.0001 Or m0 < 0.0001 Or n1 < 0.0001 Or n0 < 0.0001 Then GoTo NextLoop
            e = a(i) * d(i)
            f = b(i) * c(i)
            r = e / n
            r0 = r0 + r
            s = f / n
            s1 = s1 + s
            ' compute RR
            rr1 = rr1 + a(i) * n0 / (n0 + n1)
            rr2 = rr2 + c(i) * n1 / (n0 + n1)
            ' done
            'compute confidence interval for RR
            crr1 = crr1 + (m1 * n1 * n0 - a(i) * c(i) * (n0 + n1)) / ((n0 + n1) ^ 2)
            'done
            'compute confidence interval for OR
            n2 = n1 * n0 * m1 * m0
            q0 = (b(i) + c(i)) / n
            p0 = (a(i) + d(i)) / n
            p1 = p1 + r * p0
            p2 = p2 + p0 * s + q0 * r
            p3 = p3 + q0 * s
            'done
            'compute Chi-square
            v = n2 / (n ^ 2 * (n - 1))
            v0 = v0 + v
            v00 = v00 + (a(i) * d(i) - b(i) * c(i)) / n
            'done
            lbWolfError = lbWolfError Or (a(i) = 0) Or (b(i) = 0) Or (c(i) = 0) Or (d(i) = 0)
            If Not lbWolfError Then
                lnLnStratRR(i) = System.Math.Log((a(i) / (a(i) + b(i))) / (c(i) / (c(i) + d(i))))
                lnVarLnRR(i) = 1.0# / a(i) - 1.0# / (a(i) + b(i)) + 1.0# / c(i) - 1.0# / (c(i) + d(i)) '   Rothman 2nd ed p. 243
                lnLnStratOR(i) = System.Math.Log((a(i) * d(i)) / (b(i) * c(i)))
                lnVarLnOR(i) = 1.0# / a(i) + 1.0# / b(i) + 1.0# / c(i) + 1.0# / d(i) '   Rothman 1st ed eqn 12-13
                lnLnPoolOR = lnLnPoolOR + lnLnStratOR(i) / lnVarLnOR(i) '   Rothman 1st ed eqn 12-14
                lnLnPoolORDenom = lnLnPoolORDenom + 1 / lnVarLnOR(i)
                lnLnPoolRR = lnLnPoolRR + lnLnStratRR(i) / lnVarLnRR(i) '   Rothman 1st ed eqn 12-11
                lnLnPoolRRDenom = lnLnPoolRRDenom + 1 / lnVarLnRR(i)
                '       Debug.Print i, lnLnStratRR(i), lnVarLnRR(i), lnLnStratOR(i), lnVarLnOR(i)
            End If
NextLoop:
        Next

        '   Compute risks and confidence intervals
        re = a1 / (a1 + b1)
        ru = c1 / (c1 + d1)
        uncRR = re / ru ' uncorrected RR
        uncRD = (re - ru) * 100 ' uncorrected rd
        ' 95% confidence limits for risk ratio
        uncRR1 = System.Math.Exp(System.Math.Log((a1 / (a1 + b1)) / (c1 / (c1 + d1))) - z * (d1 / (c1 * (c1 + d1)) + b1 / ((a1 + b1) * a1)) ^ 0.5)
        uncRR2 = System.Math.Exp(System.Math.Log((a1 / (a1 + b1)) / (c1 / (c1 + d1))) + z * (d1 / (c1 * (c1 + d1)) + b1 / ((a1 + b1) * a1)) ^ 0.5)
        ' 95% confidence limits for risk difference
        uncRD1 = (re - ru - z * (re * (1 - re) / (a1 + b1) + ru * (1 - ru) / (c1 + d1)) ^ 0.5) * 100
        uncRD2 = (re - ru + z * (re * (1 - re) / (a1 + b1) + ru * (1 - ru) / (c1 + d1)) ^ 0.5) * 100
        ' Odds Ratio, 95% confidence limits for odds ratio
        If b1 * c1 < 0.0000001 Then
            uncODR = -1
            uncODR1 = -1
            uncODR2 = -1
        Else
            uncODR = (a1 * d1) / (b1 * c1) ' uncorreced OR
            If a1 * d1 < 0.000001 Then
                uncODR1 = -1
                uncODR2 = -1
            Else
                uncODR1 = System.Math.Exp(System.Math.Log((a1 * d1) / (b1 * c1)) - z * (1 / a1 + 1 / b1 + 1 / c1 + 1 / d1) ^ 0.5)
                uncODR2 = System.Math.Exp(System.Math.Log((a1 * d1) / (b1 * c1)) + z * (1 / a1 + 1 / b1 + 1 / c1 + 1 / d1) ^ 0.5)
            End If
        End If
        '   Compute M-H OR and RR and CIs
        o1 = r0 / s1
        g1 = System.Math.Log(o1)
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rr = rr1 / rr2
        seor = System.Math.Sqrt(p1 / (2 * r0 ^ 2) + p2 / (2 * r0 * s1) + p3 / (2 * s1 ^ 2))
        se = System.Math.Sqrt(crr1) / System.Math.Sqrt(rr1 * rr2)
        ORLower = System.Math.Exp(System.Math.Log(o1) - z * seor)
        ORUpper = System.Math.Exp(System.Math.Log(o1) + z * seor)
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        RRLower = System.Math.Exp(System.Math.Log(rr) - z * se)
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        RRUpper = System.Math.Exp(System.Math.Log(rr) + z * se)
        PEstimate(0) = o1 'Mantel Haenszel weighted Odds Ratio
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PEstimate(1) = rr
        Lower(0) = ORLower
        Upper(0) = ORUpper
        Lower(1) = RRLower
        Upper(1) = RRUpper

        x3 = v00 ^ 2 / v0 ' Mantel Haenszel summary ChiSquare
        Chi(0) = x3
        ChiP(0) = dist1.PfromX2(Chi(0), 1)
        Chi(1) = System.Math.Abs(v00 - 0.5) ^ 2 / v0
        ChiP(1) = dist1.PfromX2(Chi(1), 1)
        '   Pooled RR, OR, Interaction chi-sq
        If Not lbWolfError Then
            lnLnPoolOR = lnLnPoolOR / lnLnPoolORDenom '   Rothman 1st ed eqn 12-14
            lnLnPoolRR = lnLnPoolRR / lnLnPoolRRDenom '   Rothman 1st ed eqn 12-11
            For i = 1 To Numstrata
                lnChiSqOR = lnChiSqOR + (lnLnStratOR(i) - lnLnPoolOR) ^ 2 / lnVarLnOR(i) '   Rothman 1st ed eqn 12-60
                lnChiSqRR = lnChiSqRR + (lnLnStratRR(i) - lnLnPoolRR) ^ 2 / lnVarLnRR(i) '   Rothman 1st ed eqn 12-60
            Next i
        End If

        ldaUncData(0, 0, 0) = a1
        ldaUncData(0, 1, 0) = b1
        ldaUncData(1, 0, 0) = c1
        ldaUncData(1, 1, 0) = d1
        On Error GoTo Main_error
        bError = False

        'UPGRADE_WARNING: Couldn't resolve default property of object lv1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        lv1 = Nothing
        Erase mvaResult

#If VBMARTIN Then
		'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression VBMARTIN did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
		strat = moMartin.Strat2x2(ldaUncData, P, lv1)
#Else
        'UPGRADE_WARNING: Couldn't resolve default property of object Strat2x2(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Dim exact As EIExact
        exact = New EIExact()

        strat = exact.Strat2x2(ldaUncData, P, lv1)
        If Not bError Then Reformat(P, strat, lv1)
#End If
        Call Dump("TableMHStrat1", ldaUncData, P, lv1, strat)
        '    AddResult mvaResult, lv1
        '    t = mvaResult(2, 1)

        AddOneResult(mvaResult, "<tlt>Probability value</tlt>", P)
        TableMHstrat = TableMHstrat & "<table cellspacing=10 align=center><TR><TD><TD>Point<TD colspan=2>" & VB6.Format(P, "##%") & "<tlt>Confidence Interval</tlt></tr>"
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Parameters</tlt><TD ALIGN=RIGHT><tlt>Estimate</tlt><TD ALIGN=RIGHT><tlt>Lower</tlt><TD ALIGN=RIGHT><tlt>Upper</tlt>"
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Odds Ratio Estimates</tlt><TD><TD><TD>"
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Crude OR (cross product)</tlt><TD ALIGN=RIGHT>" & VB6.Format(uncODR, "##0.0000") & " <TD ALIGN=RIGHT> " & VB6.Format(uncODR1, "##0.0000") & ",<TD ALIGN=RIGHT> " & VB6.Format(uncODR2, "##0.0000") & "<TT> <tlt>(T)</tlt></TT>"
        AddOneResult(mvaResult, "<tlt>Crude OR (cross product)</tlt>", uncODR)
        AddOneResult(mvaResult, "<tlt>Lower Confidence Limit for Crude OR (cross product)</tlt>", uncODR1)
        AddOneResult(mvaResult, "<tlt>Upper Confidence Limit for Crude OR (cross product)</tlt>", uncODR2)
        If bError Then
            TableMHstrat = TableMHstrat & "<TR><TD ALIGN=CENTER COLSPAN=4>" & errmsg & "</TD></tr>"
            '        bError = False
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            TableMHstrat = TableMHstrat & "<TR><TD><tlt>Crude (MLE)</tlt><TD ALIGN=RIGHT>" & VB6.Format(lv1(1, 0), "#0.0000") & " <TD ALIGN=RIGHT> " & VB6.Format(lv1(1, 3), "#0.0000") & ",<TD ALIGN=RIGHT>" & VB6.Format(lv1(1, 4), "#.0000") & "<TT> <tlt>(M)</tlt></TT>"
            AddOneResult(mvaResult, "<tlt>Crude OR (MLE)</tlt>", lv1(1, 0))
            AddOneResult(mvaResult, "<tlt>Mid-P Lower Confidence Limit for Crude OR (MLE)</tlt>", lv1(1, 3))
            AddOneResult(mvaResult, "<tlt>Mid-P Upper Confidence Limit for Crude OR (MLE)</tlt>", lv1(1, 4))
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            TableMHstrat = TableMHstrat & "<TR><TD><TD ALIGN=RIGHT> <TD ALIGN=RIGHT> " & VB6.Format(lv1(1, 1), "#0.0000") & ",<TD ALIGN=RIGHT>" & VB6.Format(lv1(1, 2), "#0.0000") & "<TT> (F)</TT>"
            AddOneResult(mvaResult, "<tlt>Fisher Lower Confidence Limit for Crude OR (MLE)</tlt>", lv1(1, 1))
            AddOneResult(mvaResult, "<tlt>Fisher Upper Confidence Limit for Crude OR (MLE)</tlt>", lv1(1, 2))
        End If
        bError = False
        'UPGRADE_WARNING: Couldn't resolve default property of object lv1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        lv1 = Nothing
#If VBMARTIN Then
		'UPGRADE_NOTE: #If #EndIf block was not upgraded because the expression VBMARTIN did not evaluate to True or was not evaluated. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="27EE2C3C-05AF-4C04-B2AF-657B4FB6B5FC"'
		strat = moMartin.Strat2x2(DataArray, P, lv1)
#Else
        'UPGRADE_WARNING: Couldn't resolve default property of object Strat2x2(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        strat = exact.Strat2x2(DataArray, P, lv1)
        If Not bError Then Reformat(P, strat, lv1)
#End If
        Call Dump("TableMHStrat2", DataArray, P, lv1, strat)
        '    AddResult mvaResult, lv1

        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        t = mvaResult(1, 0)
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Adjusted OR (MH)</tlt><TD ALIGN=RIGHT>" & VB6.Format(o1, "##0.0000") & " <TD ALIGN=RIGHT> " & VB6.Format(Lower(0), "##0.0000") & ",<TD ALIGN=RIGHT>" & VB6.Format(Upper(0), "##0.0000") & "<TT> <tlt>(R)</tlt></TT>"
        AddOneResult(mvaResult, "<tlt>Adjusted OR (cross product)</tlt>", o1)
        AddOneResult(mvaResult, "<tlt>Lower Confidence Limit for Adjusted OR (cross product)</tlt>", Lower(0))
        AddOneResult(mvaResult, "<tlt>Upper Confidence Limit for Adjusted OR (cross product)</tlt>", Upper(0))
        If bError Then '   Error will be printed below
            '        TableMHstrat = TableMHstrat & "<TR><TD ALIGN=CENTER COLSPAN=4>" & errmsg & "</TD></tr>"
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            TableMHstrat = TableMHstrat & "<TR><TD><tlt>Adjusted OR (MLE)</tlt><TD ALIGN=RIGHT>" & VB6.Format(lv1(1, 0), "#0.0000") & " <TD ALIGN=RIGHT> " & VB6.Format(lv1(1, 3), "#0.0000") & ",<TD ALIGN=RIGHT>" & VB6.Format(lv1(1, 4), "#.0000") & "<TT> <tlt>(M)</tlt></TT>"
            AddOneResult(mvaResult, "<tlt>Adjusted OR (MLE)</tlt>", lv1(2, 1))
            AddOneResult(mvaResult, "<tlt>Mid-P Lower Confidence Limit for Adjusted OR (MLE)</tlt>", lv1(1, 3))
            AddOneResult(mvaResult, "<tlt>Mid-P Upper Confidence Limit for Adjusted OR (MLE)</tlt>", lv1(1, 4))
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            TableMHstrat = TableMHstrat & "<TR><TD><TD ALIGN=RIGHT>  <TD ALIGN=RIGHT> " & VB6.Format(lv1(1, 1), "#0.0000") & ",<TD ALIGN=RIGHT>" & VB6.Format(lv1(1, 2), "#0.0000") & "<TT> <tlt>(F)</tlt></TT>"
            AddOneResult(mvaResult, "<tlt>Fisher Lower Confidence Limit for Adjusted OR (MLE)</tlt>", lv1(1, 1))
            AddOneResult(mvaResult, "<tlt>Fisher Upper Confidence Limit for Adjusted OR (MLE)</tlt>", lv1(1, 4))
        End If
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Risk Ratios (RR)</tlt><TD><TD><TD>"
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Crude Risk Ratio</tlt> (RR)<TD ALIGN=RIGHT>" & VB6.Format(uncRR, "##0.0000") & " <TD ALIGN=RIGHT> " & VB6.Format(uncRR1, "##0.0000") & ",<TD ALIGN=RIGHT> " & VB6.Format(uncRR2, "##0.0000")
        AddOneResult(mvaResult, "<tlt>Crude Risk Ratio</tlt>", uncRR)
        AddOneResult(mvaResult, "<tlt>Lower Confidence Limit for Crude Risk Ratio</tlt>", Lower(0))
        AddOneResult(mvaResult, "<tlt>Upper Confidence Limit for Crude Risk Ratio</tlt>", Upper(0))
        'UPGRADE_WARNING: Couldn't resolve default property of object rr. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>Adjusted RR (MH)</tlt><TD ALIGN=RIGHT>" & VB6.Format(rr, "##0.0000") & " <TD ALIGN=RIGHT> " & VB6.Format(Lower(1), "##0.0000") & ",<TD ALIGN=RIGHT>" & VB6.Format(Upper(1), "##0.0000")
        TableMHstrat = TableMHstrat & "</TABLE>"
        AddOneResult(mvaResult, "<tlt>Crude Risk Ratio</tlt>", rr)
        AddOneResult(mvaResult, "<tlt>Lower Confidence Limit for Crude Risk Ratio</tlt>", Lower(1))
        AddOneResult(mvaResult, "<tlt>Upper Confidence Limit for Crude Risk Ratio</tlt>", Upper(1))
        TableMHstrat = TableMHstrat & "<p align=center><tt> <tlt>(T=Taylor series; R=RGB; M=Exact mid-P; F=Fisher exact)</tlt></tt></P>"

        TableMHstrat = TableMHstrat & "<table cellspacing=10 align=center><TR><TD> <tlt>STATISTICAL TESTS (overall association)</tlt><TD><tlt>Chi-square</tlt><TD><tlt>1-tailed p</tlt><TD><tlt>2-tailed p</tlt>"
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>MH Chi-square - uncorrected</tlt><TD ALIGN=RIGHT>" & VB6.Format(Chi(0), "#0.0000") & "<TD><TD ALIGN=RIGHT>" & VB6.Format(ChiP(0), "#0.0000")
        AddOneResult(mvaResult, "<tlt>MH Chi-square - uncorrected</tlt>", Chi(0))
        AddOneResult(mvaResult, "<tlt>MH Chi-square - uncorrected 2-tailed probability</tlt>", ChiP(0))
        TableMHstrat = TableMHstrat & "<TR><TD><tlt>MH Chi-square - corrected</tlt><TD ALIGN=RIGHT>" & VB6.Format(Chi(1), "#0.0000") & "<TD><TD ALIGN=RIGHT>" & VB6.Format(ChiP(1), "#0.0000")
        AddOneResult(mvaResult, "<tlt>MH Chi-square - corrected</tlt>", Chi(1))
        AddOneResult(mvaResult, "<tlt>MH Chi-square - corrected 2-tailed probability</tlt>", ChiP(1))
        If bError Then
            TableMHstrat = TableMHstrat & "<TR><TD ALIGN=CENTER COLSPAN=4>" & errmsg & "</TD></tr>"
            bError = False
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            TableMHstrat = TableMHstrat & "<TR><TD><tlt>Mid-p exact</tlt><TD><TD ALIGN=RIGHT>" & VB6.Format(IIf(lv1(1, 7) < lv1(1, 8), lv1(1, 7), lv1(1, 8)), "#0.0000") & "<TD>"
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            TableMHstrat = TableMHstrat & "<TR><TD><tlt>Fisher exact</tlt><TD><TD ALIGN=RIGHT>" & VB6.Format(IIf(lv1(1, 5) < lv1(1, 6), lv1(1, 5), lv1(1, 6)), "#0.0000") & "<TD>"
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            AddOneResult(mvaResult, "<tlt>Mid-P Exact 1-tailed probability</tlt>", IIf(lv1(1, 7) < lv1(1, 8), lv1(1, 7), lv1(1, 8)))
            'UPGRADE_WARNING: Couldn't resolve default property of object lv1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            AddOneResult(mvaResult, "<tlt>Fisher Exact 1-tailed probability</tlt>", IIf(lv1(1, 5) < lv1(1, 6), lv1(1, 5), lv1(1, 6)))
        End If

        If Not lbWolfError Then
            TableMHstrat = TableMHstrat & "<TR><TD COLSPAN=4 ALIGN=CENTER><tlt>In the following two tests, low p values suggest that ratios differ by stratum</tlt></TR>"
            TableMHstrat = TableMHstrat & "<TR><TD><tlt>Chi-square for differing Odds Ratios by stratum (interaction)</tlt><TD ALIGN=RIGHT>" & VB6.Format(lnChiSqOR, "#0.0000") & "<TD><TD ALIGN=RIGHT>" & VB6.Format(dist1.PfromX2(lnChiSqOR, Numstrata - 1), "#0.0000")
            TableMHstrat = TableMHstrat & "<TR><TD><tlt>Chi-square for differing Risk Ratios by stratum</tlt><TD ALIGN=RIGHT>" & VB6.Format(lnChiSqRR, "#0.0000") & "<TD><TD ALIGN=RIGHT>" & VB6.Format(dist1.PfromX2(lnChiSqRR, Numstrata - 1), "#0.0000")

            AddOneResult(mvaResult, "<tlt>Chi-square for differing Risk Ratios by stratum</tlt>", CObj(lnChiSqRR))
            AddOneResult(mvaResult, "<tlt>df for Risk Ratios</tlt>", CObj(Numstrata - 1))
            AddOneResult(mvaResult, "<tlt>p value for Risk Ratios</tlt>", CObj(dist1.PfromX2(lnChiSqRR, Numstrata - 1)))
            AddOneResult(mvaResult, "<tlt>Chi-square for differing Odds Ratios by stratum (interaction)</tlt>", CObj(lnChiSqOR))
            AddOneResult(mvaResult, "<tlt>df for Odds Ratios</tlt>", CObj(Numstrata - 1))
            AddOneResult(mvaResult, "<tlt>p value for Odds Ratios</tlt>", CObj(dist1.PfromX2(lnChiSqOR, Numstrata - 1)))
        End If
        TableMHstrat = TableMHstrat & "</TABLE>"
        If bError Then TableMHstrat = TableMHstrat & "<p align=center> <tt>" & errmsg & "</tt></P>"
        'For i = 1 To UBound(mvaResult, 2)
        'Debug.Print i, mvaResult(1, i), mvaResult(2, i)
        'Next i
        Exit Function

Main_error:
        bError = True
        Resume Next
    End Function



    Private Sub Dump(ByRef Source As String, ByRef InArray As Object, ByRef P As Object, ByRef OutResult As Object, ByRef OutText As String)
        Const bDebug As Boolean = False
        Const logfile As String = "c:\tablelog.txt"
        If bDebug Or Not bDebug Then Exit Sub
        Dim k1, j1, i1, j, fh, i, k, i2, j2, k2 As Short
        fh = FreeFile()
        FileOpen(fh, logfile, OpenMode.Append)
        PrintLine(fh, Now, Source, P)
        i1 = LBound(InArray, 1) : i2 = UBound(InArray, 1)
        j1 = LBound(InArray, 2) : j2 = UBound(InArray, 2)
        k1 = LBound(InArray, 3) : k2 = UBound(InArray, 3)
        'UPGRADE_WARNING: TypeName has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        PrintLine(fh, TypeName(InArray), i1 & " to " & i2, j1 & " to " & j2, k1 & " to " & k2)
        For i = i1 To i2 : For j = j1 To j2 : For k = k1 To k2
                    'UPGRADE_WARNING: TypeName has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
                    PrintLine(fh, i, j, k, InArray(i, j, k), TypeName(InArray(i, j, k)))
                Next k : Next j : Next i
        i1 = LBound(OutResult, 1) : i2 = UBound(OutResult, 1)
        j1 = LBound(OutResult, 2) : j2 = UBound(OutResult, 2)
        'UPGRADE_WARNING: TypeName has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        PrintLine(fh, TypeName(OutResult), i1 & " to " & i2, j1 & " to " & j2)
        For i = i1 To i2 : For j = j1 To j2
                'UPGRADE_WARNING: TypeName has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
                PrintLine(fh, i, j, OutResult(i, j), TypeName(OutResult(i, j)))
            Next j : Next i
        PrintLine(fh, "****************")
        PrintLine(fh, OutText)
        PrintLine(fh, "****************")
        FileClose(fh)
    End Sub

    Private Sub Reformat(ByRef pnProb As Double, ByRef psText As String, ByRef pvaResult As Object)
        Dim i As Object
        Const INFINITY As Integer = -16777211 '                      { Used to represent infinity }
        Const NAN As Integer = -16777212 '                { Used to represent "Not A Number" }
        psText = Replace(psText, "{0}", VB6.Format(100 * pnProb, "00"))
        If Not IsArray(pvaResult) Then Exit Sub
        For i = 1 To 9
            'UPGRADE_WARNING: Couldn't resolve default property of object pvaResult(2, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            If pvaResult(1, i) = INFINITY Then
                'UPGRADE_WARNING: Couldn't resolve default property of object pvaResult(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                pvaResult(1, i) = "<tlt>Undefined</tlt>"
                'UPGRADE_WARNING: Couldn't resolve default property of object pvaResult(2, i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ElseIf pvaResult(1, i) = NAN Then
                'UPGRADE_WARNING: Couldn't resolve default property of object pvaResult(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                pvaResult(1, i) = "<tlt>N/A</tlt>"
            End If
            'UPGRADE_WARNING: Couldn't resolve default property of object pvaResult(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object i. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            psText = Replace(psText, "{" & Chr(48 + i) & "}", VB6.Format(pvaResult(1, i)))
            'UPGRADE_WARNING: Couldn't resolve default property of object pvaResult(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            pvaResult(0, i) = Replace(pvaResult(0, i), "{0}", VB6.Format(100 * pnProb, "00"))
        Next i
    End Sub

    Private Sub ChiSqMN()
        Dim i As Integer
        Dim j As Integer
        Dim lnaRow() As Double
        Dim lnaCol() As Double
        Dim lnTotal As Double
        Dim lnTemp As Double
        Dim lnChiSq As Double
        Dim llDF As Integer
        Dim llRZero As Integer
        Dim llCZero As Integer
        Dim lnProb As Double
        Dim lbolUnder5 As Boolean

        'for debug
        On Error GoTo chisqerr
        GoTo normal
chisqerr:
        Debug.Print(VB6.TabLayout(Err.Number, Err.Description))
        System.Diagnostics.Debug.Assert(False, "")
        Resume Next
normal:

        '   Compute the row, column and grand totals
        ReDim lnaRow(NumRows)
        ReDim lnaCol(NumColumns)
        For i = 0 To NumRows - 1
            For j = 0 To NumColumns - 1
                'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                lnTemp = DataArray(i + LBound(DataArray, 1), j + LBound(DataArray, 2), LBound(DataArray, 3))
                lnaRow(i) = lnaRow(i) + lnTemp
                lnaCol(j) = lnaCol(j) + lnTemp
            Next j
            If lnaRow(i) = 0 Then '   Check for empty row
                llRZero = llRZero + 1
            End If
            lnTotal = lnTotal + lnaRow(i)
        Next i
        For j = 0 To NumColumns - 1 '   Check for empty column
            If lnaCol(j) = 0 Then
                llCZero = llCZero + 1
            End If
        Next j
        If lnTotal = 0 Then Exit Sub '   Empty row or column
        '    j = NumRows
        '    If NumColumns > NumRows Then j = NumColumns
        '    Debug.Print "TOT", lnTotal
        '    For i = 0 To j - 1
        '        If i < NumRows Then
        '            Debug.Print i, lnaRow(i),
        '        Else
        '            Debug.Print i, " ",
        '        End If
        '        If i < NumColumns Then
        '            Debug.Print lnaCol(i)
        '        Else
        '            Debug.Print " "
        '        End If
        '    Next i



        '   Compute the expected value and accumulate the chi-square; get the p-value
        For i = 0 To NumRows - 1
            If lnaRow(i) <> 0 Then
                For j = 0 To NumColumns - 1
                    If lnaCol(j) <> 0 Then
                        lnTemp = lnaRow(i) / lnTotal * lnaCol(j)
                        If lnTemp < 5 Then lbolUnder5 = True
                        'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                        lnChiSq = lnChiSq + (DataArray(i + LBound(DataArray, 1), j + LBound(DataArray, 2), LBound(DataArray, 3)) - lnTemp) ^ 2 / lnTemp
                    End If
                Next j
            End If
        Next i
        llDF = (NumRows - llRZero - 1) * (NumColumns - llCZero - 1)
        If llDF <> 0 Then
            On Error Resume Next
            lnProb = dist1.PfromX2(lnChiSq, llDF)
            If Err.Number <> 0 Then lnProb = 1.0#
            Err.Clear()
            On Error GoTo 0
        End If
        sAdvanced = "<table align=center border=0 cellspacing=4><tr><th><tlt>Chi-square</tlt></th><th><tlt>df</tlt></th><th><tlt>Probability</tlt></th></tr><tr><td align=center>" & VB6.Format(lnChiSq, "#.0000") & "</td><td align=center>" & VB6.Format(llDF) & "</td><td align=center>" & VB6.Format(lnProb, "0.0000") & "</td></tr>"
        If lbolUnder5 Then sAdvanced = sAdvanced & "<tr><td colspan=3 align=center><b><tlt>An expected value is < 5. Chi-square not valid.</tlt></b></td></tr>"
        sAdvanced = sAdvanced & "</table>"
        sInter = sAdvanced
        sMinimal = sAdvanced
        'UPGRADE_WARNING: Lower bound of array mvaResult was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mvaResult(2, 3)
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 1) = "<tlt>Chi-square</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 1) = lnChiSq
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 2) = "<tlt>df</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 2) = llDF
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(1, 3) = "<tlt>p</tlt>"
        'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        mvaResult(2, 3) = lnProb
        If llRZero + llCZero <> 0 Then
            'UPGRADE_WARNING: Lower bound of array mvaResult was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim Preserve mvaResult(2, 4)
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(1, 4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(1, 4) = "<tlt>Warning:</tlt>"
            'UPGRADE_WARNING: Couldn't resolve default property of object mvaResult(2, 4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            mvaResult(2, 4) = "<tlt>Rows and columns with zero totals excluded</tlt>"
        End If
    End Sub

    'UPGRADE_NOTE: pv was upgraded to pv_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub AddResult(ByRef pva(,) As Object, ByRef pv_Renamed As Object)
        '   Adds a variant containing a (1:2,1:n) array to a (1:2,1:m) array of variants
        Dim i As Short
        Dim m As Short
        Dim bChecking As Boolean

        If Not IsArray(pv_Renamed) Then Exit Sub
        On Error GoTo AddResultError
        bChecking = True
        m = UBound(pva, 2)
        If bChecking Then
            'UPGRADE_WARNING: Lower bound of array pva was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim Preserve pva(2, m + UBound(pv_Renamed, 2))
        Else
            m = 0
            'UPGRADE_WARNING: Lower bound of array pva was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim pva(2, UBound(pv_Renamed, 2))
        End If
        For i = 1 To UBound(pv_Renamed, 2)
            'UPGRADE_WARNING: Couldn't resolve default property of object pv_Renamed(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object pva(1, i + m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            pva(1, i + m) = pv_Renamed(1, i)
            'UPGRADE_WARNING: Couldn't resolve default property of object pv_Renamed(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object pva(2, i + m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            pva(2, i + m) = pv_Renamed(2, i)
        Next i
        m = UBound(pva, 2)
        'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        bChecking = IsNothing(pva(1, m))
        If Not bChecking Then bChecking = (Len(pva(1, m)) = 0)
        'UPGRADE_WARNING: Lower bound of array pva was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        If bChecking Then ReDim Preserve pva(2, m - 1)
        Exit Sub
AddResultError:
        bChecking = False
        Resume Next
    End Sub

    'UPGRADE_NOTE: pv was upgraded to pv_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub AddOneResult(ByRef pva(,) As Object, ByRef ps As String, ByRef pv_Renamed As Object)
        '   Adds one result to a (1:2,1:n) array
        Dim bChecking As Boolean
        Dim m As Short
        If Len(ps) = 0 Then Exit Sub
        On Error GoTo AddOneResultError
        bChecking = True
        m = UBound(pva, 2) + 1
        If bChecking Then
            'UPGRADE_WARNING: Lower bound of array pva was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim Preserve pva(2, m)
        Else
            m = 1
            'UPGRADE_WARNING: Lower bound of array pva was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim pva(2, 1)
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object pva(1, m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pva(1, m) = ps
        'UPGRADE_WARNING: Couldn't resolve default property of object pv_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object pva(2, m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pva(2, m) = pv_Renamed
        Exit Sub
AddOneResultError:
        bChecking = False
        Resume Next
    End Sub
End Class