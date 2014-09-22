Option Strict Off
Option Explicit On
Option Compare Text

Module EICoxTimeDep

    Public Function BaseLineHazardT(ByRef ldblaDataArray(,) As Double, ByRef ldblaB() As Double, ByRef lintcols As Integer, ByRef lintrows As Integer, ByRef ldblaT() As Double, ByRef lintOffset As Integer, ByRef ldblaTimeT() As Double, ByRef lintaTimeFuncPos() As Integer, ByRef lintaDataColumns() As Integer) As Double()
        Dim i, j, k As Integer
        Dim ldblOldSTime, ldblCurSTime As Double
        Dim ldblaExp, ldblRiskSum As Double
        Dim ldblaRS(,) As Double
        Dim lintTies As Integer
        Dim ldblaResult() As Double
        Dim lintTimeSteps As Integer
        Dim ldblaTime() As Double
        Dim ldblaTimeFunc() As Double
        Dim lintBase As Integer
        Dim lintWeight As Integer
        'Time Dependent Time Step Variable
        Dim l As Integer
        l = 1
        ldblaTime = VB6.CopyArray(ldblaTimeT)
        lintTimeSteps = UBound(ldblaTime)
        'UPGRADE_WARNING: Lower bound of array ldblaRS was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim ldblaRS(lintrows - 1, lintTimeSteps) 'den4: lintTimeSteps is already adjusted for 0
        'UPGRADE_WARNING: Lower bound of array ldblaTimeFunc was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim ldblaTimeFunc(UBound(ldblaB))
        'UPGRADE_WARNING: Lower bound of array ldblaTime was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve ldblaTime(lintTimeSteps + 1)

        'Make the first time really large
        ldblaTime(lintTimeSteps + 1) = 1.0E+15
        lintBase = 1
        'Get the exp(B*X) for every row
        For l = 0 To lintTimeSteps  'den4
            ldblRiskSum = 0
            For i = 0 To UBound(ldblaB)
                If lintaTimeFuncPos(lintaDataColumns(i)) > 0 Then
                    ldblaTimeFunc(i) = ldblaDataArray(lintBase, lintaTimeFuncPos(lintaDataColumns(i)) + lintOffset - 1)
                Else
                    ldblaTimeFunc(i) = 1
                End If
            Next i
            'For j = lintrows To 1 Step -1
            For j = lintrows - 1 To 0 Step -1 'den4
                'This sum only starts after the first time, don't sum when it becomes  less than the time.
                lintWeight = 1
                If mintWeight <> 0 Then
                    lintWeight = ldblaDataArray(j, 2) 'den4
                End If

                If ldblaTime(l) <= ldblaDataArray(j, 0) Then  'den4
                    ldblaExp = 0
                    'The ubound of the lintamask is the amount of variables available.
                    For i = 0 To UBound(lintaDataColumns, 0) 'den4
                        'Sum into ldblaExp
                        ldblaExp = ldblaExp + ldblaDataArray(j, lintaDataColumns(i) + lintOffset - 1) * ldblaB(i) * ldblaTimeFunc(i)
                    Next i
                    'Exponentize the ldblaExp sum
                    ldblaExp = System.Math.Exp(ldblaExp) * lintWeight
                    'Add to the risk sum
                    ldblRiskSum = ldblRiskSum + ldblaExp
                    'Place it in the array
                    ldblaRS(j, l) = ldblRiskSum
                Else
                    lintBase = j + 1
                End If
            Next j
        Next l

        'Loop Forwards...
        'However, this means we must calculate how many unique STimes there are
        ldblOldSTime = 0 'ldblaDataArray(1, 1)
        'UPGRADE_WARNING: Lower bound of array ldblaT was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        'ReDim ldblaT(lintrows + 1)
        ReDim ldblaT(lintrows) 'den4
        'ldblaT(1) = ldblOldSTime
        ldblaT(0) = 0  'den4
        'If ldblaDataArray(1, 2) = 1 Then
        k = 1

        'Else
        '   k = 0
        'End If
        For j = 0 To lintrows - 1  'den4
            If ldblaDataArray(j, 1) = 1 Then  'den4
                ldblCurSTime = ldblaDataArray(j, 0)  'den4
                If ldblCurSTime <> ldblOldSTime Then
                    ldblOldSTime = ldblCurSTime
                    k = k + 1
                    ldblaT(k) = ldblCurSTime
                End If
            End If
        Next


        'if k = 0 there were no deaths
        If k <> 0 Then
            'UPGRADE_WARNING: Lower bound of array ldblaResult was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim ldblaResult(k - 1) 'den4
            'UPGRADE_WARNING: Lower bound of array ldblaT was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim Preserve ldblaT(k - 1) 'den4
            ldblOldSTime = ldblaDataArray(0, 0) 'den4
            lintTies = 0

            ldblRiskSum = 0
            ldblaResult(0) = 0  'den4
            k = 2
            l = 1
            'gO trhough all the rows and calculate the
            'baseline hazard at each point
            For j = 0 To lintrows - 1  'den4
                ldblCurSTime = ldblaDataArray(j, 0)  'den4
                lintWeight = 1
                If mintWeight <> 0 Then
                    lintWeight = ldblaDataArray(j, 2)  'den4
                End If

                If ldblaTime(l + 1) <= ldblaDataArray(j, 0) Then   'den4
                    l = l + 1
                End If

                'Check for ties in the data
                If ldblCurSTime = ldblOldSTime Then
                    If ldblaDataArray(j, 1) = 1 Then   'den4
                        lintTies = lintTies + lintWeight
                    End If
                Else
                    'If the survival time has changed, then we need to add more people to the risk set
                    'This sum was already calculated a long time ago though
                    ldblOldSTime = ldblCurSTime

                    If lintTies <> 0 Then
                        ldblRiskSum = ldblRiskSum + lintTies / ldblaRS(j, l)
                        ldblaResult(k) = ldblRiskSum
                        lintTies = 0
                        k = k + 1
                    End If
                    'check for censored/uncensored
                    If ldblaDataArray(j, 1) = 1 Then   'den4
                        lintTies = lintWeight
                    Else
                        lintTies = 0
                    End If
                End If
            Next j


            If lintTies = 0 Then
                'Fix End point
                'k = k + 1
                ldblRiskSum = ldblRiskSum + lintTies / ldblaRS(lintrows, l)
                'UPGRADE_WARNING: Lower bound of array ldblaResult was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
                ReDim Preserve ldblaResult(k - 1)   'den4
                'UPGRADE_WARNING: Lower bound of array ldblaT was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
                ReDim Preserve ldblaT(k - 1)  'den4
                ldblaResult(k) = ldblRiskSum
                ldblaT(k) = ldblCurSTime
            Else
                ldblRiskSum = ldblRiskSum + lintTies / ldblaRS(lintrows - 1, 0)   'den4
                'UPGRADE_WARNING: Lower bound of array ldblaResult was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
                ReDim Preserve ldblaResult(k - 1)  'den4
                'UPGRADE_WARNING: Lower bound of array ldblaT was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
                ReDim Preserve ldblaT(k - 1)  'den4
                ldblaResult(k) = ldblRiskSum
                ldblaT(k) = ldblCurSTime
            End If
        Else
            'UPGRADE_WARNING: Lower bound of array ldblaResult was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim ldblaResult(0)
            ldblaResult(0) = 1
        End If

        BaseLineHazardT = VB6.CopyArray(ldblaResult)
    End Function

    ''FIX THIS ONE

    'Expected For Covariates.. Takes
    Public Function ExpectedForCovariatesT(ByRef ldblaDataArray(,) As Double, ByRef lintrows As Integer, ByRef lintcols As Integer, ByRef lintColumn() As Integer, ByRef ldblValue() As Double, ByRef ldblaSTimes() As Double, ByRef lintaR() As Integer, ByRef lintaF() As Integer, ByRef lintaC() As Integer) As Double()
        'Public Function ExpectedForCovariatesT(ldblaDataArray() As Double, lintrows As Long, lintcols As Long, lintColumn() As Long, ldblValue() As Double, ldblaSTimes() As Double, ldblaTimeT() As Double, lintaTimeFuncPos() As Long, lintaDataColumns() As Long) As Double()
        Dim i, j, k As Integer
        Dim ldblOldSTime, ldblCurSTime As Double

        'Dim lintTies As Integer
        Dim ldblaResult() As Double
        Dim ldblSurvivalAccumulator As Double
        Dim lintRiskSet As Integer
        'Dim lintCurrentSurvivors As Integer
        Dim lintWeight As Integer
        Dim lboolTrue As Boolean

        Dim llnglastRow As Integer


        'Dim lintC As Short
        'Dim lintF As Short
        'Count the Unique Survival times
        'For the indiividulas with values in ldblValue
        ldblOldSTime = 0
        'ReDim ldblaSTimes(1 To lintrows + 1) As Double

        'UPGRADE_WARNING: Lower bound of array ldblaSTimes was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim ldblaSTimes(lintrows + 1)
        'UPGRADE_WARNING: Lower bound of array ldblaResult was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim ldblaResult(lintrows + 1)
        'UPGRADE_WARNING: Lower bound of array lintaF was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim lintaF(lintrows + 1)
        'UPGRADE_WARNING: Lower bound of array lintaR was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim lintaR(lintrows + 1)
        'UPGRADE_WARNING: Lower bound of array lintaC was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim lintaC(lintrows + 1)


        'Get the risk set
        For j = 1 To lintrows
            lboolTrue = True

            'Only count the survivors
            For i = 1 To UBound(lintColumn)
                If lintColumn(i) > 0 Then
                    If Not ldblaDataArray(j, lintColumn(i)) = ldblValue(i) Then
                        lboolTrue = False
                    End If
                End If
            Next i
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 3)
            Else
                lintWeight = 1
            End If

            'If an individual meets the criteria
            'Check to see if it is unique
            If lboolTrue Then
                lintRiskSet = lintRiskSet + lintWeight
            End If
        Next

        'Initialize dummy first row
        ldblaResult(1) = 1
        ldblaSTimes(1) = 0
        lintaC(1) = 0
        lintaF(1) = 0
        lintaR(1) = lintRiskSet
        k = 1
        ldblSurvivalAccumulator = 1

        For j = 1 To lintrows
            lboolTrue = j

            'Only count the survivors
            For i = 1 To UBound(lintColumn)
                If lintColumn(i) > 0 Then
                    If Not ldblaDataArray(j, lintColumn(i)) = ldblValue(i) Then
                        lboolTrue = False
                    End If
                End If
            Next i

            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 3)
            Else
                lintWeight = 1
            End If

            'If an individual meets the criteria
            'Check to see if it is unique
            ldblCurSTime = ldblaDataArray(j, 1)
            If lboolTrue Then
                'If it was censored, it is not of interest
                'Because there was a death, set the cur survival time
                ldblCurSTime = ldblaDataArray(j, 1)
                If ldblaDataArray(j, 2) = 1 Then
                    'lintF = lintF + lintWeight

                    'If the survival time is different than the old one,
                    'Then this is a unique death
                    If ldblCurSTime <> ldblOldSTime Then
                        ldblOldSTime = ldblCurSTime

                        k = k + 1
                        lintaF(k) = lintaF(k) + lintWeight
                        'Add the time to the array, The Array of Deaths!!!
                        ldblaSTimes(k) = ldblCurSTime
                        ldblSurvivalAccumulator = (lintRiskSet - lintaF(k)) / lintRiskSet * ldblSurvivalAccumulator
                        ldblaResult(k) = ldblSurvivalAccumulator
                        'Calculate the Failure Rate. . ... .. .. .. .. . . .

                        lintaR(k) = lintRiskSet


                        'lintC = 0
                        'lintF = 0
                    Else
                        'A tie at this time...
                        lintaF(k) = lintaF(k) + lintWeight
                    End If

                Else
                    lintaC(k) = lintaC(k) + lintWeight
                End If
                lintRiskSet = lintRiskSet - lintWeight
                llnglastRow = j
            ElseIf ldblaDataArray(j, 2) = 1 Then
                If ldblCurSTime <> ldblOldSTime Then
                    ldblOldSTime = ldblCurSTime
                    k = k + 1
                    lintaR(k) = lintRiskSet
                    ldblaSTimes(k) = ldblCurSTime
                    ldblaResult(k) = ldblSurvivalAccumulator

                End If
            End If
        Next
        'Fix End Point
        If ldblaDataArray(llnglastRow, 2) = 0 And ldblCurSTime <> ldblOldSTime Then
            k = k + 1
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(llnglastRow, 3)
            Else
                lintWeight = 1
            End If
            lintaC(k) = lintWeight
            lintaF(k) = 0
            lintaR(k) = lintaC(k)
            ldblaResult(k) = ldblaResult(k - 1)
            'Fix censors
            i = k - 1
            While i > 0
                If lintaF(i) > 0 Then
                    lintaC(i) = lintaC(i) - lintaC(k)
                    i = 0
                End If

            End While
            'lintaC(k - 1) = lintaC(k - 1) - lintaC(k)
            ldblaSTimes(k) = ldblaDataArray(llnglastRow, 1)
        End If
        'Redim the survival times to the correct size
        'UPGRADE_WARNING: Lower bound of array ldblaSTimes was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve ldblaSTimes(k)
        'UPGRADE_WARNING: Lower bound of array ldblaResult was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve ldblaResult(k)
        'UPGRADE_WARNING: Lower bound of array lintaC was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve lintaC(k)
        'UPGRADE_WARNING: Lower bound of array lintaF was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve lintaF(k)
        'UPGRADE_WARNING: Lower bound of array lintaR was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve lintaR(k)

        ExpectedForCovariatesT = VB6.CopyArray(ldblaResult)
    End Function
    'Time Dependent Versions of the hazard curves and stuff like that

    Public Function SurvivalCurveT(ByRef ldblaDataArray(,) As Double, ByRef ldblaSurvival() As Double, ByRef ldblaTime() As Double, ByRef lintaTimeFuncPos() As Integer, ByRef lintaDataColumns() As Integer, ByRef ldblaSTimes() As Double, ByRef ldblaValues(,) As Double) As Double()
        Dim ldblaResult() As Double
        Dim ldblScale As Double
        ReDim ldblaResult(UBound(ldblaSurvival))
        Dim i As Integer
        Dim l As Integer
        Dim j As Integer
        Dim k As Integer
        Dim ldblaTimeFunc() As Double
        ReDim ldblaTimeFunc(UBound(lintaDataColumns))

        'ToDo: den4: Check counters if they should be initialized to -1 
        '      l is used with ldblaTime(l + 1). To get this to be (0) in the first iteration, l would need to be -1
        '      k is added to i so, unlike l, if k = -1 then ldblaDataArray(i + k, 1) would solve to ldblaDataArray(-1, 1)=error.
        'l = 0
        l = -1
        k = 0

        For i = 0 To UBound(ldblaSurvival) 'den4
            If (l < UBound(ldblaTime)) Then
                If ldblaSTimes(i) >= ldblaTime(l + 1) Then
                    l = l + 1
                    'While ldblaDataArray(i + k, 1) < ldblaSTimes(i)
                    While ldblaDataArray(i + k, 0) < ldblaSTimes(i) 'den4
                        k = k + 1
                    End While
                    For j = 0 To UBound(lintaDataColumns) 'den4
                        If lintaTimeFuncPos(lintaDataColumns(j) - 1) > 0 Then  'den4
                            'ldblaTimeFunc(j) = ldblaDataArray(i + k, lintaTimeFuncPos(lintaDataColumns(j)) + mintOffset - 1)
                            ldblaTimeFunc(j) = ldblaDataArray(i + k, lintaTimeFuncPos(lintaDataColumns(j) - 1) + mintOffset - 2)  'den4
                        Else
                            ldblaTimeFunc(j) = 1
                        End If
                    Next j
                    ldblScale = 0
                    For j = 0 To UBound(lintaDataColumns)
                        'ldblScale = ldblScale + ldblaValues(1, lintaDataColumns(j)) * ldblaTimeFunc(j)
                        ldblScale = ldblScale + ldblaValues(0, lintaDataColumns(j) - 1) * ldblaTimeFunc(j) 'den4
                    Next j
                    ldblScale = System.Math.Exp(ldblScale)
                End If
            End If
            ldblaResult(i) = ldblaSurvival(i) ^ ldblScale
        Next i
        SurvivalCurveT = VB6.CopyArray(ldblaResult)

    End Function
    Public Function HazardCurveT(ByRef ldblaDataArray(,) As Double, ByRef ldblaHazard() As Double, ByRef ldblaTime() As Double, ByRef lintaTimeFuncPos() As Integer, ByRef lintaDataColumns() As Integer, ByRef ldblaSTimes() As Double, ByRef ldblaValues(,) As Double) As Double()
        Dim ldblaResult() As Double
        ReDim ldblaResult(UBound(ldblaHazard))
        Dim ldblScale As Double
        Dim i As Integer
        Dim l As Integer
        Dim j As Integer
        Dim k As Integer
        Dim ldblaTimeFunc() As Double
        ReDim ldblaTimeFunc(UBound(lintaDataColumns))
        'l = 0
        l = -1 'den4
        k = 0 'den4
        For i = 0 To UBound(ldblaHazard) 'den4
            If (l < UBound(ldblaTime)) Then
                If ldblaSTimes(i) >= ldblaTime(l + 1) Then
                    l = l + 1
                    'While ldblaDataArray(i + k, 1) < ldblaSTimes(i)
                    While ldblaDataArray(i + k, 0) < ldblaSTimes(i) 'den4
                        k = k + 1
                    End While
                    For j = 0 To UBound(lintaDataColumns) 'den4
                        If lintaTimeFuncPos(lintaDataColumns(j) - 1) > 0 Then 'den4
                            'ldblaTimeFunc(j) = ldblaDataArray(i + k, lintaTimeFuncPos(lintaDataColumns(j)) + mintOffset - 1)
                            ldblaTimeFunc(j) = ldblaDataArray(i + k, lintaTimeFuncPos(lintaDataColumns(j) - 1) + mintOffset - 2)
                        Else
                            ldblaTimeFunc(j) = 1
                        End If
                    Next j
                    ldblScale = 0
                    For j = 0 To UBound(lintaDataColumns) 'den4
                        'ldblScale = ldblScale + ldblaValues(1, lintaDataColumns(j)) * ldblaTimeFunc(j)
                        ldblScale = ldblScale + ldblaValues(0, lintaDataColumns(j) - 1) * ldblaTimeFunc(j)
                    Next j
                    ldblScale = System.Math.Exp(ldblScale)
                End If
            End If
            ldblaResult(i) = ldblaHazard(i) * ldblScale
        Next i
        HazardCurveT = VB6.CopyArray(ldblaResult)
    End Function
End Module