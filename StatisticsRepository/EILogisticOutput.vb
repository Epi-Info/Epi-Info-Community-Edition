Option Strict Off
Option Explicit On
Module EILogisticOutput
    Public Function CreateOutputString(ByRef ldblll1 As Double, ByRef ldbllll As Double, ByRef lIntIters As Integer, ByRef ldblaB() As Double, ByRef ldblaCo(,) As Double, ByRef lboolConv As Boolean, ByRef ldblScore As Double) As String
        Dim i As Integer
        Dim lstrResult As String
        Dim lstrError As String
        Dim p As Integer
        Dim llngDegrees As Integer
        Dim ldblConf As Double
        Dim lvarCoeff(,) As Object
        Dim lvarTest(,) As Object
        Dim lstrTitle As String
        'Dim lstrError As String
        Dim lstrConv As String
        Dim lvara(2, 4) As Object
        On Error GoTo errorOutput
        'UPGRADE_WARNING: Lower bound of array Results was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Results(1, 2)

        Dim dist As New statlib
        dist = New statlib()

        If Len(mstrMatchVar) > 0 Then
            llngDegrees = (UBound(ldblaB) + 1) + CShort(mboolIntercept)
            lstrTitle = "<tlt>Conditional Logistic Regression</tlt>"
        Else
            lstrTitle = "<tlt>Unconditional Logistic Regression</tlt>"
            llngDegrees = (UBound(ldblaB) + 1) + CShort(mboolIntercept)
        End If

        lstrResult = "<br clear=""all"" /><p align=""left""><strong>" & lstrTitle & "</strong></p>"

        lstrResult = lstrResult & "<br clear=""all"" /><table align=""left"" cellspacing=""8"">" & "<tr><td class=""stats"" align=""left""><strong><tlt>Term</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>Odds Ratio</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong>" & mstrC & "%</strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>C.I.</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>Coefficient</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>S. E.</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>Z-Statistic</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>P-Value</tlt></strong></td></tr>"
        ReDim lvarCoeff(8, UBound(ldblaB))

        For i = 0 To UBound(ldblaB)
            lvarCoeff(0, i) = mStrAMatrixLabels(i)
            lstrResult = lstrResult & "<tr><td class=""stats"" align=""left""><strong>" & mStrAMatrixLabels(i) & "</strong></td>"
            If Not StrComp(mStrAMatrixLabels(i), "CONSTANT") = 0 Then
                ldblConf = System.Math.Sqrt(System.Math.Abs(ldblaCo(i, i))) * mdblP
                lvarCoeff(1, i) = System.Math.Exp(ldblaB(i))
                lvarCoeff(2, i) = System.Math.Exp(ldblaB(i) - ldblConf)

                If (ldblaB(i) + ldblConf) > 30 Then
                    lvarCoeff(3, i) = 1.0E+20
                Else
                    lvarCoeff(3, i) = System.Math.Exp(ldblaB(i) + ldblConf)
                End If
                'If it includes 1 underline it
                If Not (lvarCoeff(2, i) <= 1 And lvarCoeff(3, i) >= 1) Then
                    lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(1, i)) & "</u></td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(2, i)) & "</u></td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(3, i)) & "</u></td>"
                Else
                    lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(1, i)) & "</td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(2, i)) & "</td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(3, i)) & "</td>"
                End If


            Else
                lvarCoeff(1, i) = "*"
                lvarCoeff(2, i) = "*"
                lvarCoeff(3, i) = "*"
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & "*" & "</td>"
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & "*" & "</td>"
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & "*" & "</td>"
            End If
            lvarCoeff(4, i) = ldblaB(i)
            lvarCoeff(5, i) = System.Math.Sqrt(System.Math.Abs(ldblaCo(i, i)))
            lvarCoeff(6, i) = ldblaB(i) / lvarCoeff(5, i)

            lvarCoeff(7, i) = 2 * dist.PfromZ(System.Math.Abs(lvarCoeff(6, i))) '0 'dist1.PfromZ(System.Math.Abs(lvarCoeff(7, i)))

            lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(ldblaB(i)) & "</td>"
            lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(5, i)) & "</td>"
            lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(6, i)) & "</td>"
            If lvarCoeff(7, i) < mdblC Then
                lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(7, i)) & "</u></td></tr>"
            Else
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(7, i)) & "</td></tr>"
            End If

        Next
        ReDim lvarTest(3, 1)

        lvarTest(0, 0) = "Score"
        lvarTest(1, 0) = ldblScore
        lvarTest(2, 0) = llngDegrees
        lvarTest(3, 0) = dist.PfromX2(ldblScore, llngDegrees)

        lvarTest(0, 1) = "Likelihood Ratio"
        lvarTest(1, 1) = 2.0# * (ldbllll - ldblll1)
        lvarTest(2, 1) = llngDegrees
        lvarTest(3, 1) = dist.PfromX2(2 * (ldbllll - ldblll1), llngDegrees)

        If lboolConv = True And ldbllll >= ldblll1 Then
            lstrConv = "<tlt>Converged</tlt>"
            lvara(1, 0) = 1 ''
        ElseIf ldbllll < ldblll1 Then
            lstrConv = "<tlt>Diverged</tlt>"
            lvara(1, 0) = -1 ''
        Else
            lstrConv = "<tlt>Partial Convergence</tlt>"
            lvara(1, 0) = 0
        End If
        lstrResult = lstrResult & "</table><br clear=""all"" />"
        lstrResult = lstrResult & "<br clear=""all"" /><table><tr><td class=""stats""><strong><tlt>Convergence:</tlt></strong></td><td class=""stats"" align=""right"">" & lstrConv & "</td></tr>" & "<tr><td class=""stats""><strong><tlt>Iterations:</tlt> </strong></td><td class=""stats"" align=""right"">" & lIntIters & "</td></tr>" & "<tr><td class=""stats""><strong><tlt>Final -2*Log-Likelihood:</tlt> </strong></td><td class=""stats"" align=""right"">" & VB6.Format(-2 * ldbllll, "0.0000") & "<tr><td class=""stats""><strong><tlt>Cases included:</tlt> </strong></td><td class=""stats"" align=""right"">" & VB6.Format(NumRows) & "</td></tr></table><br clear=""all"" />"


        lstrResult = lstrResult & "<table align=""left"" cellspacing=""8"">" & "<tr><td class=""stats"" align=""left""><strong><tlt>Test</tlt></strong></td>" & "<td class=""stats"" align=""left""><strong><tlt>Statistic</tlt></strong></td>" & "<td class=""stats"" align=""left""><strong><tlt>D.F.</tlt></strong></td>" & "<td class=""stats"" align=""left""><strong><tlt>P-Value</tlt></strong></td></tr>" & "<tr><td class=""stats"" align=""left""><strong><tlt>Score</tlt></strong></td>" & "<td class=""stats"" align=""right"">" & OutClip(ldblScore) & "</td>" & "<td class=""stats"" align=""right"">" & llngDegrees & "</td>" & "<td class=""stats"" align=""right"">" & OutClip(lvarTest(3, 0)) & "</td></tr>" & "<tr><td class=""stats"" align=""left""><strong><tlt>Likelihood Ratio</tlt></strong></td>" & "<td class=""stats"" align=""right"">" & OutClip(lvarTest(1, 1)) & "</td>" & "<td class=""stats"" align=""right"">" & llngDegrees & "</td>" & "<td class=""stats"" align=""right"">" & OutClip(lvarTest(3, 1)) & "</td></tr>"


        Results(0, 0) = Mid(lstrTitle, 6, Len(lstrTitle) - 11)
        Results(1, 0) = VB6.CopyArray(lvarCoeff)
        Results(0, 1) = "Convergence"

        lvara(0, 0) = "Converged"
        '    lvara(1, 0) = 1=converged, 0=partial convergence, -1=diverged
        lvara(0, 1) = "Iterations"
        lvara(1, 1) = lIntIters
        lvara(0, 2) = "Final -2*Log(Likelihood)"
        lvara(1, 2) = -2 * ldbllll
        lvara(0, 3) = "Cases"
        lvara(1, 3) = NumRows

        Results(1, 1) = VB6.CopyArray(lvara)
        Results(0, 2) = "Tests"
        Results(1, 2) = VB6.CopyArray(lvarTest)
        lstrResult = lstrResult & "</table><br clear=""all"" />"
        lstrResult = lstrResult & Epi.Statistics.LogisticInteraction.IOR(ldblaCo, mStrAMatrixLabels, ldblaB, DataArray)
        CreateOutputString = lstrResult & lstrError
        Exit Function

errorOutput:
        lstrError = lstrError & "<tlt>Error In Regression, Data Invalid</tlt>"
        Err.Clear()
        CreateOutputString = lstrError
        Exit Function
        Resume Next


    End Function
    Public Function CreateOutputStringLB(ByRef ldblll1 As Double, ByRef ldbllll As Double, ByRef lIntIters As Integer, ByRef ldblaB() As Double, ByRef ldblaCo(,) As Double, ByRef lboolConv As Boolean, ByRef ldblScore As Double) As String
        Dim i As Integer
        Dim lstrResult As String
        Dim lstrError As String
        Dim p As Integer
        Dim llngDegrees As Integer
        Dim ldblConf As Double
        Dim lvarCoeff(,) As Object
        Dim lvarTest(,) As Object
        Dim lstrTitle As String
        'Dim lstrError As String
        Dim lstrConv As String
        Dim lvara(2, 4) As Object
        On Error GoTo errorOutput
        'UPGRADE_WARNING: Lower bound of array Results was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Results(1, 2)

        Dim dist As New statlib
        dist = New statlib()

        If Len(mstrMatchVar) > 0 Then
            llngDegrees = (UBound(ldblaB) + 1) + CShort(mboolIntercept)
            lstrTitle = "<tlt>Conditional Logistic Regression</tlt>"
        Else
            lstrTitle = "<tlt>Lob-Binomial Regression</tlt>"
            llngDegrees = (UBound(ldblaB) + 1) + CShort(mboolIntercept)
        End If

        lstrResult = "<br clear=""all"" /><p align=""left""><strong>" & lstrTitle & "</strong></p>"

        lstrResult = lstrResult & "<br clear=""all"" /><table align=""left"" cellspacing=""8"">" & "<tr><td class=""stats"" align=""left""><strong><tlt>Term</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>Risk Ratio</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong>" & mstrC & "%</strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>C.I.</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>Coefficient</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>S. E.</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>Z-Statistic</tlt></strong></td>" & "<td class=""stats"" align=""center""><strong><tlt>P-Value</tlt></strong></td></tr>"
        ReDim lvarCoeff(8, UBound(ldblaB))

        For i = 0 To UBound(ldblaB)
            lvarCoeff(0, i) = mStrAMatrixLabels(i)
            lstrResult = lstrResult & "<tr><td class=""stats"" align=""left""><strong>" & mStrAMatrixLabels(i) & "</strong></td>"
            If Not StrComp(mStrAMatrixLabels(i), "CONSTANT") = 0 Then
                ldblConf = System.Math.Sqrt(System.Math.Abs(ldblaCo(i, i))) * mdblP
                lvarCoeff(1, i) = System.Math.Exp(ldblaB(i))
                lvarCoeff(2, i) = System.Math.Exp(ldblaB(i) - ldblConf)

                If (ldblaB(i) + ldblConf) > 30 Then
                    lvarCoeff(3, i) = 1.0E+20
                Else
                    lvarCoeff(3, i) = System.Math.Exp(ldblaB(i) + ldblConf)
                End If
                'If it includes 1 underline it
                If Not (lvarCoeff(2, i) <= 1 And lvarCoeff(3, i) >= 1) Then
                    lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(1, i)) & "</u></td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(2, i)) & "</u></td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(3, i)) & "</u></td>"
                Else
                    lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(1, i)) & "</td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(2, i)) & "</td>"
                    lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(3, i)) & "</td>"
                End If


            Else
                lvarCoeff(1, i) = "*"
                lvarCoeff(2, i) = "*"
                lvarCoeff(3, i) = "*"
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & "*" & "</td>"
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & "*" & "</td>"
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & "*" & "</td>"
            End If
            lvarCoeff(4, i) = ldblaB(i)
            lvarCoeff(5, i) = System.Math.Sqrt(System.Math.Abs(ldblaCo(i, i)))
            lvarCoeff(6, i) = ldblaB(i) / lvarCoeff(5, i)

            lvarCoeff(7, i) = 2 * dist.PfromZ(System.Math.Abs(lvarCoeff(6, i))) '0 'dist1.PfromZ(System.Math.Abs(lvarCoeff(7, i)))

            lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(ldblaB(i)) & "</td>"
            lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(5, i)) & "</td>"
            lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(6, i)) & "</td>"
            If lvarCoeff(7, i) < mdblC Then
                lstrResult = lstrResult & "<td class=""stats"" align=""right""><u>" & OutClip(lvarCoeff(7, i)) & "</u></td></tr>"
            Else
                lstrResult = lstrResult & "<td class=""stats"" align=""right"">" & OutClip(lvarCoeff(7, i)) & "</td></tr>"
            End If

        Next
        ReDim lvarTest(3, 1)

        If lboolConv = True And ldbllll >= ldblll1 Then
            lstrConv = "<tlt>Converged</tlt>"
            lvara(1, 0) = 1 ''
        ElseIf ldbllll < ldblll1 Then
            lstrConv = "<tlt>Diverged</tlt>"
            lvara(1, 0) = -1 ''
        Else
            lstrConv = "<tlt>Partial Convergence</tlt>"
            lvara(1, 0) = 0
        End If
        lstrResult = lstrResult & "</table><br clear=""all"" />"
        lstrResult = lstrResult & "<br clear=""all"" /><table><tr><td class=""stats""><strong><tlt>Convergence:</tlt></strong></td><td class=""stats"" align=""right"">" & lstrConv & "</td></tr>" & "<tr><td class=""stats""><strong><tlt>Iterations:</tlt> </strong></td><td class=""stats"" align=""right"">" & lIntIters & "</td></tr>" & "<tr><td class=""stats""><strong><tlt>Final Log-Likelihood:</tlt> </strong></td><td class=""stats"" align=""right"">" & VB6.Format(ldbllll, "0.0000") & "<tr><td class=""stats""><strong><tlt>Cases included:</tlt> </strong></td><td class=""stats"" align=""right"">" & VB6.Format(NumRows) & "</td></tr></table><br clear=""all"" />"


        'lstrResult = lstrResult & "<table align=""left"" cellspacing=""8"">" & "<tr><td class=""stats"" align=""left""><strong><tlt>Test</tlt></strong></td>" & "<td class=""stats"" align=""left""><strong><tlt>Statistic</tlt></strong></td>" & "<td class=""stats"" align=""left""><strong><tlt>D.F.</tlt></strong></td>" & "<td class=""stats"" align=""left""><strong><tlt>P-Value</tlt></strong></td></tr>" & "<tr><td class=""stats"" align=""left""><strong><tlt>Score</tlt></strong></td>" & "<td class=""stats"" align=""right"">" & OutClip(ldblScore) & "</td>" & "<td class=""stats"" align=""right"">" & llngDegrees & "</td>" & "<td class=""stats"" align=""right"">" & OutClip(lvarTest(3, 0)) & "</td></tr>" & "<tr><td class=""stats"" align=""left""><strong><tlt>Likelihood Ratio</tlt></strong></td>" & "<td class=""stats"" align=""right"">" & OutClip(lvarTest(1, 1)) & "</td>" & "<td class=""stats"" align=""right"">" & llngDegrees & "</td>" & "<td class=""stats"" align=""right"">" & OutClip(lvarTest(3, 1)) & "</td></tr>"


        Results(0, 0) = Mid(lstrTitle, 6, Len(lstrTitle) - 11)
        Results(1, 0) = VB6.CopyArray(lvarCoeff)
        Results(0, 1) = "Convergence"

        lvara(0, 0) = "Converged"
        '    lvara(1, 0) = 1=converged, 0=partial convergence, -1=diverged
        lvara(0, 1) = "Iterations"
        lvara(1, 1) = lIntIters
        lvara(0, 2) = "Final Log-Likelihood"
        lvara(1, 2) = ldbllll
        lvara(0, 3) = "Cases"
        lvara(1, 3) = NumRows

        Results(1, 1) = VB6.CopyArray(lvara)
        lstrResult = lstrResult & "</table><br clear=""all"" />"
        lstrResult = lstrResult & Epi.Statistics.LogBinomialInteraction.IOR(ldblaCo, mStrAMatrixLabels, ldblaB, DataArray)
        CreateOutputStringLB = lstrResult & lstrError
        Exit Function

errorOutput:
        lstrError = lstrError & "<tlt>Error In Regression, Data Invalid</tlt>"
        Err.Clear()
        CreateOutputStringLB = lstrError
        Exit Function
        Resume Next


    End Function
    Public Function Residuals(ByRef lintOffset As Integer, ByRef ldblaB() As Double) As Object
        'Dim lintweight As Integer
        'Dim lintMatch As Integer

        'Dim lstrQuery As String
        'Dim lconRS As ADODB.Recordset
        'Dim ldblGx As Double
        'Dim lintDOff As Integer

        'Dim i As Integer

        'If Len(mstrMatchVar) > 0 Then
        '	lintMatch = 1
        'End If

        'If Len(mstrWeightVar) > 0 Then
        '	lintweight = 1
        'End If
        'On Error GoTo end_proc
        'lstrQuery = "SELECT [" & mstrDependVar & "], [OUTTABLE] FROM [" & mstrTableName & "]"

        'gconDB = New ADODB.Connection
        'lconRS = New ADODB.Recordset
        'gconDB.ConnectionString = mstrConnString
        'gconDB.Open()
        'lconRS.Open(lstrQuery, gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockPessimistic)

        'lintDOff = 1
        'While Not lconRS.EOF
        '	'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        '	If Not VarType(lconRS.Fields(0).Value) = VariantType.Null Then
        '		'Calculate the residual.

        '		For i = 1 To UBound(ldblaB)
        '			ldblGx = ldblGx + DataArray(lintDOff, lintOffset + i) * ldblaB(i)
        '		Next i
        '		ldblGx = System.Math.Exp(ldblGx) / (1 + System.Math.Exp(ldblGx))
        '		If (DataArray(lintDOff, 1) > 0) Then
        '			lconRS.Fields(1).Value = (1 - ldblGx)
        '		Else
        '			lconRS.Fields(1).Value = (0 - ldblGx)
        '		End If
        '		lintDOff = lintDOff + 1
        '	Else
        '		lconRS.Fields(1).Value = VariantType.Null
        '	End If
        '	lconRS.Update()
        '	lconRS.MoveNext()
        'End While
        'lconRS.Close()
        'lstrQuery = "select count(outtable) from [" & mstrTableName & "]"
        'lconRS.Open(lstrQuery, gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly)
        'Debug.Print(lconRS.Fields(0).Value)
        'lconRS.Close()
        ''UPGRADE_NOTE: Object lconRS may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        'lconRS = Nothing
		
end_proc: 
cleanup: 
		gconDB.Close()
		'UPGRADE_NOTE: Object gconDB may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		gconDB = Nothing
		Exit Function
		
		
	End Function
	Public Function Residuals2(ByRef resid() As Double) As Object
		Dim lintweight As Integer
		Dim lintMatch As Integer
		
		Dim lstrQuery As String
		Dim lconRS As ADODB.Recordset
		Dim lintDOff As Short
		On Error GoTo cleanup
		lstrQuery = "SELECT [" & mstrDependVar & "], [OUTTABLE] FROM [" & mstrTableName & "]"
		gconDB = New ADODB.Connection
		lconRS = New ADODB.Recordset
		gconDB.ConnectionString = mstrConnString
		gconDB.Open()
		lconRS.Open(lstrQuery, gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockPessimistic)
		
		lintDOff = 1
		While Not lconRS.EOF
			'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
			If Not VarType(lconRS.Fields(0).Value) = VariantType.Null Then
				'Calculate the residual.
				
				
				
				lconRS.Fields(1).Value = resid(lintDOff)
				
				
				
				lintDOff = lintDOff + 1
			Else
				lconRS.Fields(1).Value = VariantType.Null
			End If
			lconRS.Update()
			lconRS.MoveNext()
		End While
		lconRS.Close()
		lstrQuery = "select count(outtable) from [" & mstrTableName & "]"
		lconRS.Open(lstrQuery, gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly)
		Debug.Print(lconRS.Fields(0).Value)
		lconRS.Close()
		'UPGRADE_NOTE: Object lconRS may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		lconRS = Nothing
		
end_proc: 
cleanup: 
		gconDB.Close()
		'UPGRADE_NOTE: Object gconDB may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		gconDB = Nothing
		Exit Function
		
		
	End Function
	Public Function OutClip(ByVal ldblData As Double) As String
		If ldblData > 1000000000000# Then
			OutClip = "&gt;1.0E12"
		Else
			OutClip = VB6.Format(ldblData, "0.0000")
		End If
	End Function
End Module