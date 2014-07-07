Option Strict Off
Option Explicit On
Option Compare Text

Module EICoxOutput
    Public Function CreateCoxOutputString(ByRef ldblll1 As Double, ByRef ldbllll As Double, ByRef lIntIters As Integer, ByRef ldblaB() As Double, ByRef ldblaCo(,) As Double, ByRef lboolConv As Boolean, ByRef ldblScore As Double) As String
        Dim i As Integer
        Dim lstrResult As String
        'Dim p As Integer
        Dim llngDegrees As Integer
        Dim ldblConf As Double
        Dim lvarCoeff(,) As Object
        Dim lvarTest(,) As Object
        Dim lstrTitle As String
        Dim lstrConv As String
        'Dim lvara(3) As Object
        Dim lvara(2) As Object
        'ReDim Results(2, 3)
        ReDim Results(1, 2)

        Dim dist As New statlib
        dist = New statlib()

        lstrTitle = "<TLT>Cox Proportional Hazards</TLT>"
        'den4: added 1 to below since Ubound() is now 0 based.  Degrees Freedom based on UBound must be raised by 1.
        llngDegrees = UBound(ldblaB) + 1


        lstrResult = "<BR CLEAR=ALL><P ALIGN=LEFT><B>" & lstrTitle & "</B></P>"

        lstrResult = lstrResult & "<BR CLEAR=ALL><TABLE ALIGN=LEFT CELLSPACING=8>" & "<TR><TD class=""stats"" ALIGN=LEFT><B><TLT>Term</TLT></B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B><TLT>Hazard Ratio</TLT></B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B>" & mstrC & "%</B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B><TLT>C.I.</TLT></B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B><TLT>Coefficient</TLT></B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B><TLT>S. E.</TLT></B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B><TLT>Z-Statistic</TLT></B></TD>" & "<TD class=""stats"" ALIGN=CENTER><B><TLT>P-Value</TLT></B></TD></TR>"
        'ReDim lvarCoeff(8, UBound(ldblaB))
        ReDim lvarCoeff(7, UBound(ldblaB))

        For i = 0 To UBound(ldblaB)
            lvarCoeff(0, i) = mStrAMatrixLabels(i)
            lstrResult = lstrResult & "<TR><TD class=""stats"" ALIGN=LEFT><B>" & mStrAMatrixLabels(i) & "</B></TD>"

            ldblConf = System.Math.Sqrt(System.Math.Abs(ldblaCo(i, i))) * mdblP
            'lvarCoeff(2, i) = System.Math.Exp(ldblaB(i))
            lvarCoeff(1, i) = System.Math.Exp(ldblaB(i))
            'lvarCoeff(3, i) = System.Math.Exp(ldblaB(i) - ldblConf)
            lvarCoeff(2, i) = System.Math.Exp(ldblaB(i) - ldblConf)

            If (ldblaB(i) + ldblConf) > 30 Then
                'lvarCoeff(4, i) = 1.0E+20
                lvarCoeff(3, i) = 1.0E+20
            Else
                'lvarCoeff(4, i) = System.Math.Exp(ldblaB(i) + ldblConf)
                lvarCoeff(3, i) = System.Math.Exp(ldblaB(i) + ldblConf)
            End If
            'If it includes 1 underline it
            'If Not (lvarCoeff(3, i) <= 1 And lvarCoeff(4, i) >= 1) Then
            If Not (lvarCoeff(2, i) <= 1 And lvarCoeff(3, i) >= 1) Then
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT><U>" & OutClip(lvarCoeff(1, i)) & "</U></TD>"
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT><U>" & OutClip(lvarCoeff(2, i)) & "</U></TD>"
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT><U>" & OutClip(lvarCoeff(3, i)) & "</U></TD>"
            Else
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarCoeff(1, i)) & "</TD>"
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarCoeff(2, i)) & "</TD>"
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarCoeff(3, i)) & "</TD>"
            End If

            lvarCoeff(4, i) = ldblaB(i)
            lvarCoeff(5, i) = System.Math.Sqrt(System.Math.Abs(ldblaCo(i, i)))
            'lvarCoeff(7, i) = ldblaB(i) / lvarCoeff(6, i)
            lvarCoeff(6, i) = ldblaB(i) / lvarCoeff(5, i)
            'PValue... underline it if P < 1-P
            'lvarCoeff(8, i) = 2 * (diststatlib_definst.PfromZ(System.Math.Abs(lvarCoeff(7, i))))
            lvarCoeff(7, i) = 2 * (dist.PfromZ(System.Math.Abs(lvarCoeff(6, i))))


            'lstrResult = lstrResult & "<TD ALIGN=RIGHT>" & OutClip(ldblaB(i)) & "</TD>"
            lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(ldblaB(i)) & "</TD>"
            lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarCoeff(5, i)) & "</TD>"
            lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarCoeff(6, i)) & "</TD>"
            If lvarCoeff(7, i) < mdblC Then
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT><U>" & OutClip(lvarCoeff(7, i)) & "</U></TD></TR>"
            Else
                lstrResult = lstrResult & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarCoeff(7, i)) & "</TD></TR>"
            End If

        Next
        'ReDim lvarTest(4, 2)
        ReDim lvarTest(3, 1)

        lvarTest(0, 0) = "Score"
        lvarTest(1, 0) = ldblScore
        lvarTest(2, 0) = llngDegrees
        lvarTest(3, 0) = dist1.PfromX2(ldblScore, llngDegrees)

        lvarTest(0, 1) = "Likelihood Ratio"
        lvarTest(1, 1) = 2.0# * (ldbllll - ldblll1)
        lvarTest(2, 1) = llngDegrees
        lvarTest(3, 1) = dist1.PfromX2(2 * (ldbllll - ldblll1), llngDegrees)
        If lboolConv = True And ldbllll >= ldblll1 Then
            lstrConv = "<TLT>Converged</TLT>"
        ElseIf ldbllll < ldblll1 Then
            lstrConv = "<TLT>Diverged</TLT>"
        Else
            lstrConv = "<TLT>Partial Convergence</TLT>"
        End If
        lstrResult = lstrResult & "</TABLE><BR CLEAR=ALL>"

        lstrResult = lstrResult & "<BR CLEAR=ALL><TABLE><TR><TD class=""stats""><B><TLT>Convergence:</TLT></B></TD><TD class=""stats"" ALIGN=RIGHT>" & lstrConv & "</TLT></TD></TR>" & "<TR><TD class=""stats"" ><B><TLT>Iterations:</TLT></B></TD><TD class=""stats"" ALIGN=RIGHT>" & lIntIters & "</TD></TR>" & "<TR><TD class=""stats""><B><TLT>-2 * Log-Likelihood:</TLT> </B></TD><TD class=""stats"" ALIGN=RIGHT>" & VB6.Format(-2 * ldbllll, "0.0000") & "</TD></TR></TABLE><BR CLEAR=ALL>"

        'lstrResult = lstrResult &"<TABLE ALIGN=LEFT CELLSPACING=8>" & "<TR><TD ALIGN=LEFT><B><TLT>Test</TLT></B></TD>" & "<TD ALIGN=LEFT><B><TLT>Statistic</TLT></B></TD>" & "<TD ALIGN=LEFT><B><TLT>D.F.</TLT></B></TD>" & "<TD ALIGN=LEFT><B><TLT>P-Value</TLT></B></TD></TR>" & "<TR><TD ALIGN=LEFT><B><TLT>Score</TLT></B></TD>" & "<TD ALIGN=RIGHT>" & OutClip(ldblScore) & "</TD>" & "<TD ALIGN=RIGHT>" & llngDegrees & "</TD>" & "<TD ALIGN=RIGHT>" & OutClip(lvarTest(4, 1)) & "</TD></TR>" & "<TR><TD ALIGN=LEFT><B><TLT>Likelihood Ratio</TLT></B></TD>" & "<TD ALIGN=RIGHT>" & OutClip(lvarTest(2, 2)) & "</TD>" & "<TD ALIGN=RIGHT>" & llngDegrees & "</TD>" & "<TD ALIGN=RIGHT>" & OutClip(lvarTest(3, 1)) & "</TD></TR>"    
        lstrResult = lstrResult & "<TABLE ALIGN=LEFT CELLSPACING=8>" & "<TR><TD class=""stats"" ALIGN=LEFT><B><TLT>Test</TLT></B></TD>" & "<TD class=""stats"" ALIGN=LEFT><B><TLT>Statistic</TLT></B></TD>" & "<TD class=""stats"" ALIGN=LEFT><B><TLT>D.F.</TLT></B></TD>" & "<TD class=""stats"" ALIGN=LEFT><B><TLT>P-Value</TLT></B></TD></TR>" & "<TR><TD class=""stats"" ALIGN=LEFT><B><TLT>Score</TLT></B></TD>" & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(ldblScore) & "</TD>" & "<TD class=""stats"" ALIGN=RIGHT>" & llngDegrees & "</TD>" & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarTest(3, 0)) & "</TD></TR>" & "<TR><TD class=""stats"" ALIGN=LEFT><B><TLT>Likelihood Ratio</TLT></B></TD>" & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarTest(1, 1)) & "</TD>" & "<TD class=""stats"" ALIGN=RIGHT>" & llngDegrees & "</TD>" & "<TD class=""stats"" ALIGN=RIGHT>" & OutClip(lvarTest(3, 1)) & "</TD></TR>"

        Results(0, 0) = lstrTitle
        Results(1, 0) = VB6.CopyArray(lvarCoeff)

        Results(0, 1) = "Convergence"
        lvara(0) = lstrConv
        lvara(1) = lIntIters
        lvara(2) = ldbllll
        Results(1, 1) = VB6.CopyArray(lvara)

        Results(0, 2) = "Tests"
        Results(1, 2) = VB6.CopyArray(lvarTest)
        lstrResult = lstrResult & "</Table><BR CLEAR=ALL>"
        CreateCoxOutputString = lstrResult
    End Function

    'Den4: Using OutClip defined in EILogisticOutput.vb
    'Public Function OutClip(ByVal ldblData As Double) As String
    '    If ldblData > 1000000000000.0# Then
    '        OutClip = ">10E12"
    '    Else
    '        OutClip = VB6.Format(ldblData, "0.0###")

    '    End If
    'End Function

End Module