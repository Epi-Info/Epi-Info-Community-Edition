Option Strict Off
Option Explicit On
Option Compare Text

Module EICoxGraph
    'Create the BaseLine Hazard Curve
    Public Function BaseLineHazard(ByRef ldblaDataArray(,) As Double, ByRef ldblaB() As Double, ByRef lintcols As Integer, ByRef lintrows As Integer, ByRef ldblaT() As Double, ByRef lintOffset As Integer) As Double()
        Dim i, j, k As Integer
        Dim ldblOldSTime, ldblCurSTime As Double
        Dim ldblaExp, ldblRiskSum As Double
        Dim ldblaRS() As Double
        Dim lintTies As Integer
        Dim ldblaResult() As Double
        Dim lintTimeSteps As Integer
        Dim lintWeight As Integer
        'There is one risk sum per row.
        ReDim ldblaRS(lintrows - 1)

        On Error GoTo errBaseLineHazard

        Debug.Print("")
        Debug.Print("Entering BaseLineHazard")
        'Get the exp(B*X) for every row
        ldblRiskSum = 0

        Dim baselineDictionary As Dictionary(Of Integer, Decimal)
        baselineDictionary = New Dictionary(Of Integer, Decimal)

        For Each variable As StatisticsRepository.EIDummyVariables.Variable In mVarArray

            If variable.strBase = "NONE" Then
                Dim n As Int32
                Dim total As Decimal
                total = 0

                For j = lintrows - 1 To 0 Step -1
                    If mintWeight <> 0 Then
                        lintWeight = ldblaDataArray(j, 2)
                    Else
                        lintWeight = 1
                    End If
                    total = total + ldblaDataArray(j, variable.iColumn + 1)
                Next j

                Dim mean As Decimal
                mean = total / lintrows
                baselineDictionary.Add(variable.iColumn, mean)
            Else
                For j = 0 To variable.iSize - 1
                    baselineDictionary.Add(variable.iColumn + j, 0)
                Next
            End If

        Next

        'Go through all the rows
        For j = lintrows - 1 To 0 Step -1
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            Else
                lintWeight = 1
            End If
            ldblaExp = 0
            For i = lintOffset - 1 To lintcols - 1

                Dim baseline As Decimal
                baseline = baselineDictionary(i - 1)

                ldblaExp = ldblaExp + (ldblaDataArray(j, i) - baseline) * ldblaB(i - lintOffset + 1)
            Next i
            ldblaExp = System.Math.Exp(ldblaExp) * lintWeight
            ldblRiskSum = ldblRiskSum + ldblaExp
            ldblaRS(j) = ldblRiskSum
        Next j
        'Loop Forwards...
        'However, this means we must calculate how many unique STimes there are
        ldblOldSTime = 0
        ReDim ldblaT(lintrows + 1)
        ldblaT(0) = ldblOldSTime
        k = 0
        ldblaT(0) = 0
        For j = 0 To lintrows - 1
            If ldblaDataArray(j, 1) = 1 Then
                ldblCurSTime = ldblaDataArray(j, 0)
                If ldblCurSTime <> ldblOldSTime Then
                    ldblOldSTime = ldblCurSTime
                    k = k + 1
                    ldblaT(k) = ldblCurSTime
                End If
            End If
        Next
        If ldblaDataArray(j - 2, 1) = 0 Then
            k = k + 1
            ldblaT(k) = ldblaDataArray(j - 1, 0)
        End If

        'ToDo:den4: Need to determine how to handle k.
        '           With 1-based arrayse, maybe needed to start this when k <> 0, 
        '           but now first time through, k = 0.
        'If k <> 0 Then
        If k >= 0 Then
            ReDim ldblaResult(k)
            ReDim Preserve ldblaT(k)
            ldblOldSTime = ldblaDataArray(0, 0)
            lintTies = 0
            ldblRiskSum = 0

            'Skip artificial first row
            k = 1
            ldblaResult(0) = 0
            'ZFJ4 Added the below line
            ldblOldSTime = 0
            For j = 0 To lintrows - 1
                'ZFJ4 defined GreatestRS
                Dim GreatestRS
                ldblCurSTime = ldblaDataArray(j, 0)
                lintWeight = 1
                If mintWeight <> 0 Then
                    lintWeight = ldblaDataArray(j, 2)
                End If

                If ldblCurSTime = ldblOldSTime Then
                    If ldblaDataArray(j, 1) = 1 Then
                        lintTies = lintTies + lintWeight
                    End If

                Else
                    'If the survival time has changed, then we need to add more people to the risk set
                    'This sum was already calculated a long time ago though
                    ldblOldSTime = ldblCurSTime
                    'ZFJ4 Added Here...
                    If lintTies <> 0 Then
                        ldblRiskSum = ldblRiskSum + lintTies / GreatestRS
                        ldblaResult(k) = ldblRiskSum
                    End If
                    If ldblaDataArray(j, 1) = 1 Then
                        GreatestRS = ldblaRS(j)
                    End If
                    '... to here

                    If lintTies <> 0 Then
                        'This is where Curve Calculations are carried out
                        'Because last iteration
                        'when there is a new dead guy with a unique s time
                        '    Debug.Print "RiskSum in denominator, Guys who died " & ldblaRS(j) & " , " & lintTies
                        'ZFJ4 Comment out:  ldblRiskSum = ldblRiskSum + lintTies / ldblaRS(j)
                        'ZFJ4 Comment out:  ldblaResult(k) = ldblRiskSum
                        lintTies = 0
                        k = k + 1
                    End If
                    'check for censored/uncensored
                    If ldblaDataArray(j, 1) = 1 Then
                        lintTies = lintWeight
                    Else
                        lintTies = 0
                    End If
                End If
            Next j

            If lintTies = 0 Then
                'Fix End point
                'k = k + 1
                ldblRiskSum = ldblRiskSum + lintTies / ldblaRS(lintrows - 1)
                ReDim Preserve ldblaResult(k)
                ReDim Preserve ldblaT(k)
                ldblaResult(k) = ldblRiskSum
                ldblaT(k) = ldblCurSTime
            Else
                ldblRiskSum = ldblRiskSum + lintTies / ldblaRS(lintrows - 1)
                ReDim Preserve ldblaResult(k)
                ReDim Preserve ldblaT(k)
                ldblaResult(k) = ldblRiskSum
                ldblaT(k) = ldblCurSTime
            End If
        Else
            ReDim ldblaResult(1)
            ldblaResult(0) = 0
        End If

cleanup:
        BaseLineHazard = VB6.CopyArray(ldblaResult)
        Exit Function
errBaseLineHazard:
        MsgBox(Err.Description, MsgBoxStyle.MsgBoxSetForeground, "BaseLineHazard")
        Resume cleanup
        Resume
    End Function

    Public Function BaseLineSurvivalCurve(ByRef ldblaHazard() As Double) As Double()
        Dim ldblaSurvival() As Double
        ReDim ldblaSurvival(UBound(ldblaHazard))
        Dim i As Integer
        For i = 0 To UBound(ldblaHazard)
            ldblaSurvival(i) = System.Math.Exp(-ldblaHazard(i))
        Next i
        BaseLineSurvivalCurve = VB6.CopyArray(ldblaSurvival)
    End Function

    Public Function SurvivalCurve(ByRef ldblaSurvival() As Double, ByVal ldblScale As Double) As Double()
        Dim ldblaResult() As Double
        ReDim ldblaResult(UBound(ldblaSurvival))
        Dim i As Integer
        'ldblScale = System.Math.Exp(ldblScale)
        ldblaResult(0) = ldblaSurvival(0)
        For i = 1 To UBound(ldblaSurvival)
            '            ldblaResult(i) = ldblaSurvival(i) ^ ldblScale
            ldblaResult(i) = System.Math.Exp(System.Math.Log(ldblaSurvival(i)) * System.Math.Exp(ldblScale))
        Next i
        SurvivalCurve = VB6.CopyArray(ldblaResult)
    End Function

    Public Function HazardCurve(ByRef ldblaHazard() As Double, ByVal ldblScale As Double) As Double()
        Dim ldblaResult() As Double
        ReDim ldblaResult(UBound(ldblaHazard))
        Dim i As Integer
        ldblScale = System.Math.Exp(ldblScale)
        For i = 0 To UBound(ldblaHazard)
            ldblaResult(i) = ldblaHazard(i) * ldblScale
        Next i
        HazardCurve = VB6.CopyArray(ldblaResult)
    End Function
    'Assume ordered by survival time and censor status
    Public Function Expected(ByRef ldblaDataArray(,) As Double, ByRef lintrows As Integer, ByRef lintcols As Integer, ByRef lintOffset As Integer, ByRef ldblaSTimes() As Double, ByRef lintaR() As Integer, ByRef lintaF() As Integer, ByRef lintaC() As Integer) As Double()
        Dim j, k As Integer
        Dim ldblOldSTime, ldblCurSTime As Double
        Dim ldblaResult() As Double
        Dim ldblSurvivalAccumulator As Double
        Dim lintRiskSet As Integer
        Dim ldblCensor As Double
        Dim lintWeight As Integer
        Dim ldblaRS(lintrows) As Object

        'Count the Unique Survival times
        ldblOldSTime = -1231231231230.0# 'ldblaDataArray(1, 1)
        ReDim ldblaSTimes(lintrows + 1)
        ReDim lintaF(lintrows + 1)
        ReDim lintaC(lintrows + 1)
        ReDim lintaR(lintrows + 1)
        ReDim ldblaResult(lintrows + 1)
        'Add initial survival times
        ldblaSTimes(0) = 0
        lintaF(0) = 0
        lintaC(0) = 0
        lintaR(0) = 0

        k = 0
        ldblCensor = 0
        If mintWeight <> 0 Then
            For j = 0 To lintrows - 1
                lintRiskSet = lintRiskSet + ldblaDataArray(j, 2)
                'Debug.Print ldblaDataArray(j, 1), ldblaDataArray(j, 2), ldblaDataArray(j, 3)
            Next j
        Else
            lintRiskSet = lintrows
        End If
        ldblSurvivalAccumulator = 1
        lintaR(0) = lintRiskSet
        ldblaResult(0) = 1
        k = 0
        'On Error GoTo errorpoo
        For j = 0 To lintrows - 1
            'Get the weight
            ldblCurSTime = ldblaDataArray(j, 0)
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            Else
                lintWeight = 1
            End If

            If ldblaDataArray(j, 1) = 1 Then
                'if there is a death
                If ldblCurSTime <> ldblOldSTime Then
                    ldblOldSTime = ldblCurSTime
                    'Event counter k...

                    'The only time k will = lintweight is when k used to be 0
                    'Change the risk set, but remove the guys censored before this time

                    ldblSurvivalAccumulator = (lintaR(k) - lintaF(k)) / lintaR(k) * ldblSurvivalAccumulator
                    ldblaResult(k) = ldblSurvivalAccumulator

                    k = k + 1
                    lintaF(k) = lintaF(k) + lintWeight
                    ldblaSTimes(k) = ldblCurSTime
                    lintaR(k) = lintRiskSet
                Else
                    'A tie at this time...
                    lintaF(k) = lintaF(k) + lintWeight
                End If
            Else
                lintaC(k) = lintaC(k) + lintWeight
            End If
            lintRiskSet = lintRiskSet - lintWeight
        Next
        'Do last row
        ldblSurvivalAccumulator = (lintaR(k) - lintaF(k)) / lintaR(k) * ldblSurvivalAccumulator
        ldblaResult(k) = ldblSurvivalAccumulator
        'Fix the end point
        If ldblaDataArray(j - 1, 1) = 0 And ldblCurSTime <> ldblOldSTime Then
            k = k + 1
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j - 1, 2)
            Else
                lintWeight = 1
            End If
            lintaC(k) = lintWeight
            lintaF(k) = 0
            lintaR(k) = lintaC(k)
            ldblaResult(k) = ldblaResult(k - 1)
            'Fix censors
            lintaC(k - 1) = lintaC(k - 1) - lintaC(k)
            ldblaSTimes(k) = ldblCurSTime
        End If
        'Ah, the data we want from the function
        'The event times
        ReDim Preserve ldblaSTimes(k)
        'THe failures at each time.. always greater than 0
        ReDim Preserve lintaF(k)
        'The censored between the previous and at this interval
        ReDim Preserve lintaC(k)
        'This risk set
        ReDim Preserve lintaR(k)
        ReDim Preserve ldblaResult(k)
        Debug.Print(VB6.TabLayout("ldblastimes", "lintac", "lintaf", "lintar", "ldbls"))

        '  All occurances of Dim and ReDim ldblaSTimes used (1 to ...) as the lower bound in the VB6 code.
        '  .NET Upgrade wizard changed all lower bounds to 0
        For j = 0 To UBound(ldblaSTimes)
            Debug.Print(VB6.TabLayout(ldblaSTimes(j), lintaC(j), lintaF(j), lintaR(j), ldblaResult(j)))
        Next

        Expected = VB6.CopyArray(ldblaResult)
        Exit Function
        'errorpoo:
        'Resume
    End Function

    'Creates Expected Curve For Covariates that are not separated in different strata
    'lintcolumn() stores the information for which values to check for the expected curve

    Public Function ExpectedForCovariates(ByRef ldblaDataArray(,) As Double, ByRef lintrows As Integer, ByRef lintcols As Integer, ByRef lintColumn() As Integer, ByRef ldblValue() As Double, ByRef ldblaSTimes() As Double) As Double()
        Dim i, j, k As Integer
        Dim ldblOldSTime, ldblCurSTime As Double
        Dim ldblaResult() As Double
        Dim ldblSurvivalAccumulator As Double
        Dim lintRiskSet As Integer
        Dim lintWeight As Integer
        Dim lboolTrue As Boolean
        Dim lintC As Short
        Dim lintF As Short
        'Count the Unique Survival times
        'For the indiividulas with values in ldblValue
        ldblOldSTime = 0
        ReDim ldblaSTimes(lintrows + 1)
        ReDim ldblaResult(lintrows + 1)
        'Get the risk set
        For j = 0 To lintrows - 1
            lboolTrue = True

            'Only count the survivors
            For i = 0 To UBound(lintColumn)
                If lintColumn(i) > 0 Then
                    If Not ldblaDataArray(j, lintColumn(i)) = ldblValue(i) Then
                        lboolTrue = False
                    End If
                End If
            Next i
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
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

        ldblaResult(0) = 1
        ldblaSTimes(0) = 0
        k = 0
        ldblSurvivalAccumulator = 1
        For j = 0 To lintrows - 1
            lboolTrue = True

            'Only count the survivors
            For i = 0 To UBound(lintColumn) 
                If lintColumn(i) > 0 Then
                    If Not ldblaDataArray(j, lintColumn(i)) = ldblValue(i) Then
                        lboolTrue = False
                    End If
                End If
            Next i

            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            Else
                lintWeight = 1
            End If

            'If an individual meets the criteria
            'Check to see if it is unique

            If lboolTrue Then
                'If it was censored, it is not of interest
                If ldblaDataArray(j, 1) = 1 Then
                    lintF = lintF + lintWeight
                    'Because there was a death, set the cur survival time
                    ldblCurSTime = ldblaDataArray(j, 0)
                    'If the survival time is different than the old one,
                    'Then this is a unique death
                    If ldblCurSTime <> ldblOldSTime Then
                        ldblOldSTime = ldblCurSTime
                        k = k + 1
                        'Add the time to the array, The Array of Deaths!!!
                        ldblaSTimes(k) = ldblCurSTime
                        ldblSurvivalAccumulator = (lintRiskSet - lintF) / lintRiskSet * ldblSurvivalAccumulator
                        ldblaResult(k) = ldblSurvivalAccumulator
                        'Calculate the Failure Rate. . ... .. .. .. .. . . .
                        lintRiskSet = lintRiskSet - lintC - lintF
                        lintC = 0
                        lintF = 0
                    End If

                Else
                    lintC = lintC + lintWeight
                End If
            End If
        Next
        'Redim the survival times to the correct size
        ReDim Preserve ldblaSTimes(k)
        ReDim Preserve ldblaResult(k)

        ExpectedForCovariates = VB6.CopyArray(ldblaResult)
    End Function

    Public Function ExpectedForCovariates2(ByRef ldblaDataArray(,) As Double, ByRef lintrows As Integer, ByRef lintcols As Integer, ByRef lintColumn() As Integer, ByRef ldblValue() As Double, ByRef ldblaSTimes() As Double, ByRef lintaR() As Integer, ByRef lintaF() As Integer, ByRef lintaC() As Integer) As Double()
        Dim i, j, k As Integer
        Dim ldblOldSTime, ldblCurSTime As Double

        Dim lintTies As Integer
        Dim ldblaResult() As Double
        Dim ldblSurvivalAccumulator As Double
        Dim lintRiskSet As Integer
        Dim lintCurrentSurvivors As Integer
        Dim lintWeight As Integer
        Dim lboolTrue As Boolean

        Dim llnglastRow As Integer
        Dim llnglastCensor As Integer

        Dim lintC As Short
        Dim lintF As Short
        'Count the Unique Survival times
        'For the indiividulas with values in ldblValue
        ldblOldSTime = 0
        '       The following ReDim statments all specified "lintrows + 1" prior to migration to .NET
        '       Since the LBound changed from 1 to 0, the UBound will be changed from "lintrows + 1" to "lintrows" 
        ReDim ldblaSTimes(lintrows)
        ReDim ldblaResult(lintrows)
        ReDim lintaF(lintrows)
        ReDim lintaR(lintrows)
        ReDim lintaC(lintrows)
        On Error GoTo errorTest
        'Get the risk set

        For j = 0 To lintrows - 1
            lboolTrue = True

            'Only count the survivors
            For i = 0 To UBound(lintColumn)
                If lintColumn(i) > 0 Then
                    If Not ldblaDataArray(j, lintColumn(i) - 1) = ldblValue(i) Then
                        lboolTrue = False
                    End If
                End If
            Next i
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
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
        ldblaResult(0) = 1
        ldblaSTimes(0) = 0
        lintaC(0) = 0
        lintaF(0) = 0
        lintaR(0) = lintRiskSet
        k = 0
        ldblSurvivalAccumulator = 1
        'If the risk set is not ZERO continue.
        'If lintRiskSet <> 0 Then

        For j = 0 To lintrows - 1
            lboolTrue = j

            'Only count the survivors
            For i = 0 To UBound(lintColumn)
                If lintColumn(i) > 0 Then
                    If Not ldblaDataArray(j, lintColumn(i) - 1) = ldblValue(i) Then
                        lboolTrue = False
                    End If
                End If
            Next i

            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            Else
                lintWeight = 1
            End If

            'If an individual meets the criteria
            'Check to see if it is unique
            ldblCurSTime = ldblaDataArray(j, 0)
            If lboolTrue Then
                'If it was censored, it is not of interest
                'Because there was a death, set the cur survival time
                ldblCurSTime = ldblaDataArray(j, 0)
                If ldblaDataArray(j, 1) = 1 Then
                    'lintF = lintF + lintWeight

                    'If the survival time is different than the old one,
                    'Then this is a unique death
                    If ldblCurSTime <> ldblOldSTime Then
                        ldblOldSTime = ldblCurSTime

                        ldblSurvivalAccumulator = (lintaR(k) - lintaF(k)) / lintaR(k) * ldblSurvivalAccumulator
                        ldblaResult(k) = ldblSurvivalAccumulator

                        k = k + 1
                        lintaF(k) = lintaF(k) + lintWeight
                        'Add the time to the array, The Array of Deaths!!!
                        ldblaSTimes(k) = ldblCurSTime
                        lintaR(k) = lintRiskSet
                    Else
                        'A tie at this time...
                        lintaF(k) = lintaF(k) + lintWeight
                    End If
                Else
                    lintaC(k) = lintaC(k) + lintWeight
                End If
                lintRiskSet = lintRiskSet - lintWeight
                llnglastRow = j
            ElseIf ldblaDataArray(j, 1) = 1 Then
                If ldblCurSTime <> ldblOldSTime And lintaR(k) <> 0 Then
                    ldblOldSTime = ldblCurSTime
                    ldblSurvivalAccumulator = (lintaR(k) - lintaF(k)) / lintaR(k) * ldblSurvivalAccumulator
                    ldblaResult(k) = ldblSurvivalAccumulator
                    k = k + 1
                    lintaR(k) = lintRiskSet
                    ldblaSTimes(k) = ldblCurSTime
                    llnglastCensor = j
                ElseIf lintaR(k) = 0 And ldblCurSTime <> ldblOldSTime Then
                    ldblOldSTime = ldblCurSTime
                    ldblSurvivalAccumulator = ldblSurvivalAccumulator
                    ldblaResult(k) = ldblSurvivalAccumulator
                    k = k + 1
                    lintaR(k) = lintRiskSet
                    ldblaSTimes(k) = ldblCurSTime
                    llnglastCensor = j
                End If
            End If
        Next
        If lintaR(k) <> 0 Then
            ldblSurvivalAccumulator = (lintaR(k) - lintaF(k)) / lintaR(k) * ldblSurvivalAccumulator
        End If

        ldblaResult(k) = ldblSurvivalAccumulator
        'Fix End Point
        If llnglastRow + 1 > 0 Then
            If ldblaDataArray(llnglastRow, 1) = 0 And llnglastRow > llnglastCensor Then
                k = k + 1
                If mintWeight <> 0 Then
                    lintWeight = ldblaDataArray(llnglastRow, 2)
                Else
                    lintWeight = 1
                End If
                lintaC(k) = lintWeight
                lintaF(k) = 0
                lintaR(k) = lintaC(k)
                ldblaResult(k) = ldblaResult(k - 1)
                'Fix censors
                'Make sure we are at correct spot in data array.
                i = k - 1
                While i > 0
                    'If lintaC(i) > 0 Then
                    '    i = 0

                    If lintaF(i) > 0 Or lintaC(i) > 0 Then
                        lintaC(i) = lintaC(i) - lintaC(k)
                        i = 0
                    Else
                        lintaR(i) = lintaR(k)
                    End If
                    i = i - 1

                End While
                'ldblaSTimes(k) = ldblaDataArray(llnglastRow, 1) 
                ldblaSTimes(k) = ldblaDataArray(llnglastRow, 0)
            End If
        Else

        End If
        'Else
        '    k = 1
        '    'There is no group...
        '    ldblaSTimes(k) = 0
        '    ldblaResult(k) = 1
        '    lintaC (k)
        'End If

        'Redim the survival times to the correct size
        ReDim Preserve ldblaSTimes(k)
        ReDim Preserve ldblaResult(k)
        ReDim Preserve lintaC(k)
        ReDim Preserve lintaF(k)
        ReDim Preserve lintaR(k)
        ExpectedForCovariates2 = VB6.CopyArray(ldblaResult)
        Exit Function
errorTest:
        Resume
    End Function
    Public Function PlotGraphs(ByRef ldblaB() As Double)
        Dim m, k, i, j, l, p As Integer

        'Dim llstStrataPlots As New List(Of String)
        'Dim llstCovariatePlots As New List(Of String)
        'Dim llstCategoryPlots As New List(Of String)

        Dim lintNumPlots As Integer
        Dim lstraStrataPlots() As String
        Dim lstraCovariatePlots() As String
        Dim lstraCategoryPlots() As String
        Dim lstrWhereQuery As String
        Dim lstrBaseQuery As String

        Dim lstrdebug As String

        Dim lsvar() As StrataVariable
        Dim lStrataA() As Strata

        Dim ldblaExpected() As Double
        Dim ldblaHazard() As Double
        Dim ldblaActualHazard() As Double
        Dim ldblaBaselineSurvival() As Double
        Dim ldblaESurvival() As Double
        Dim ldblaExpected2() As Double
        Dim ldblaSTE() As Double

        Dim lintaRisk() As Integer
        Dim lintaCensored() As Integer
        Dim lintaFailed() As Integer

        Dim ldblaSTimes() As Double

        Dim lvaraOutTable(,) As Object
        Dim lvaraStringTable() As Object

        Dim lintcols As Integer
        Dim lintrows As Integer

        Dim lintStartPos As Integer
        Dim lintEndPos As Integer

        Dim ldblMean As Double
        Dim ldblISum As Double
        Dim lintArrayToChange As Integer
        Dim lintaVariableIndexArray() As Integer
        Dim lintaVariableSize() As Integer
        Dim lintaVariablePos() As Integer
        Dim lintaPlotVarIndex() As Integer

        Dim ldblaTempValues() As Double
        Dim ldblaExpandedValues(,) As Double

        Dim lintaVariableInfo() As Integer
        Dim lintIndex As Integer
        Dim lboolLoop As Boolean

        Dim lboolInList As Boolean
        Dim ldblaValues() As Double
        Dim lintaColumn() As Integer
        Dim lintEstimated As Integer

        Dim dummyDouble As Double
        Dim lintCount As Integer

        Dim ldblFinalTime As Double

        Dim ldblaMeans() As Double
        Dim lintO As Short
        On Error GoTo errPlotGraphs
        Debug.Print("Plot Var's")
        For i = 0 To UBound(mstraPlotVar)
            'For Each plotvar As String In mlstPlotVar
            System.Diagnostics.Debug.Write(VB6.TabLayout(mstraPlotVar(i), TAB))
        Next i
        Debug.Print("")
        'Out Table will have 1 column for time, n columns for the plot vars, 3 for the risk set
        'and 3 for the expected, and actual s curves
        lintcols = 1 + mlstPlotVar.Count + 3 + 3
        ReDim lvaraOutTable(lintcols - 1, 0)
        ReDim lvaraStringTable(mlstPlotVar.Count - 1)
        ReDim ldblaTempValues(mintRealFields - mintOffset)
        ReDim ldblaExpandedValues(0, mintVirtualFields - mintOffset)
        ReDim ldblaValues(mintVirtualFields - mintOffset)
        ReDim lintaColumn(mintVirtualFields - mintOffset)
        ReDim lstraStrataPlots(0)
        'llstStrataPlots.Add(String.Empty)
        ReDim lstraCategoryPlots(0)
        ReDim lstraCategoryPlots(0)
        'llstCovariatePlots.Add(String.Empty)

        Debug.Print("Strata Plot Var's")

        For i = 0 To UBound(mstraPlotVar)
            If IsStrataVar(mstraPlotVar(i)) >= 0 Then
                ReDim Preserve lstraStrataPlots(lintNumPlots - 1)
                lstraStrataPlots(lintNumPlots - 1) = mstraPlotVar(i)
                Debug.Print(lstraStrataPlots(lintNumPlots))
                lintNumPlots = lintNumPlots + 1
            End If
        Next i

        lintNumPlots = 1

        Debug.Print("Covariate Plot Var's")

        For i = 0 To UBound(mstraPlotVar)
            If IsCovariate(mstraPlotVar(i)) >= 0 Then
                ReDim Preserve lstraCovariatePlots(lintNumPlots - 1)
                lstraCovariatePlots(lintNumPlots - 1) = mstraPlotVar(i)
                Debug.Print(lstraCovariatePlots(lintNumPlots - 1))
                lintNumPlots = lintNumPlots + 1
            End If
        Next
        lintNumPlots = 1
        Debug.Print("Category Plot Var's")

        For i = 0 To UBound(mstraPlotVar)
            If IsCovariate(mstraPlotVar(i)) = -1 And IsStrataVar(mstraPlotVar(i)) = -1 Then
                ReDim Preserve lstraCategoryPlots(lintNumPlots)
                lstraCategoryPlots(lintNumPlots) = mstraPlotVar(i)
                Debug.Print(lstraCategoryPlots(lintNumPlots))
                lintNumPlots = lintNumPlots + 1
            End If
        Next i
        GetMeans()
        lintrows = 0

        'If there are Group Variables..
        If Len(lstraCategoryPlots(0)) > 0 Then
            Debug.Print("Plotting the Group Variables")
            SetStratified(lstraCategoryPlots, lsvar)
            'For Group Vars, load the data for the expected curves
            LoadStrata(lsvar, lStrataA)
            'find the final survival time in the group

            ldblFinalTime = lStrataA(0).dblaData(UBound(lStrataA(0).dblaData, 0), 0)
            For k = 1 To UBound(lStrataA)
                If ldblFinalTime < lStrataA(k).dblaData(UBound(lStrataA(k).dblaData, 0), 0) Then
                    ldblFinalTime = lStrataA(k).dblaData(UBound(lStrataA(k).dblaData, 0), 0)
                End If
            Next k

            For k = 0 To UBound(lStrataA)
                ReDim ldblaMeans(UBound(lStrataA(k).mdblaTime) - 1)

                If lStrataA(k).lintrows > 0 Then

                    ldblaExpected = Expected(lStrataA(k).dblaData, lStrataA(k).lintrows, lStrataA(k).lintcols, 2, ldblaSTE, lintaRisk, lintaFailed, lintaCensored)

                    If mstraTimeDependentVar.Count > 0 Then
                        Debug.Print("Creating time dependent Mean Tables")
                        ldblMean = 0
                        For i = 0 To UBound(ldblaB)
                            ldblMean = ldblMean + ldblaB(i) * mdblaMeans(lStrataA(k).intaDataColumns(i))
                            'ldblaExpandedValues(1, lStrataA(k).intaDataColumns(i)) = ldblaB(i) * mdblaMeans(lStrataA(k).intaDataColumns(i))
                            ldblaExpandedValues(0, lStrataA(k).intaDataColumns(i)) = ldblaB(i) * mdblaMeans(lStrataA(k).intaDataColumns(i))
                        Next i

                        ldblaHazard = BaseLineHazardT(lStrataA(k).dblaData, ldblaB, lStrataA(k).lintcols, lStrataA(k).lintrows, ldblaSTimes, mintOffset, lStrataA(k).mdblaTime, lStrataA(k).intaTimeSelectors, lStrataA(k).intaDataColumns)
                        ldblaBaselineSurvival = BaseLineSurvivalCurve(ldblaHazard) 'den4: fixed BaseLineSurvivalCurve() note: no function for BaseLineSurvivialCurve*T*
                        ldblaESurvival = SurvivalCurveT(lStrataA(k).dblaData, ldblaBaselineSurvival, lStrataA(k).mdblaTime, lStrataA(k).intaTimeSelectors, lStrataA(k).intaDataColumns, ldblaSTimes, ldblaExpandedValues) 'den4: fixed SurvivalCurveT()
                        ldblaActualHazard = HazardCurveT(lStrataA(k).dblaData, ldblaHazard, lStrataA(k).mdblaTime, lStrataA(k).intaTimeSelectors, lStrataA(k).intaDataColumns, ldblaSTimes, ldblaExpandedValues)  'den4: fixed HazardCurveT()
                    Else
                        ldblMean = 0
                        For i = 0 To UBound(mdblaMeans)
                            ldblMean = ldblMean + ldblaB(i) * mdblaMeans(i)
                        Next i
                        ldblaHazard = BaseLineHazard(lStrataA(k).dblaData, ldblaB, lStrataA(k).lintcols, lStrataA(k).lintrows, ldblaSTimes, mintOffset)   'den4: Fixed BaseLineHazard()
                        ldblaBaselineSurvival = BaseLineSurvivalCurve(ldblaHazard) 'den4: fixed above
                        ldblaESurvival = SurvivalCurve(ldblaBaselineSurvival, ldblMean)
                        ldblaActualHazard = HazardCurve(ldblaHazard, ldblMean)
                    End If

                    ReDim Preserve lvaraOutTable(lintcols - 1, lintrows + UBound(ldblaExpected))

                    'For i = 1 To UBound(mstraPlotVar)
                    '    'If we find the plotvar in the strata name index string
                    '    lintStartPos = InStr(1, lStrataA(k).strName, mstraPlotVar(i))
                    '    If lintStartPos > 0 Then
                    '        lintStartPos = InStr(lintStartPos + 1, lStrataA(k).strName, " ")
                    '        lintEndPos = InStr(lintStartPos + 1, lStrataA(k).strName, " ")
                    '        If lintEndPos = 0 Then
                    '            lvaraStringTable(i) = lStrataA(k).strName
                    '        Else
                    '            lvaraStringTable(i) = Mid(lStrataA(k).strName, lintStartPos + 1, lintEndPos - lintStartPos - 1)
                    '        End If
                    '        Debug.Print("Plot Var " & i & " = " & lvaraStringTable(i))
                    '        lintStartPos = lintEndPos
                    '    Else
                    '        'Otherwise, it should be held constant, at the name of the plot var, or the
                    '        'Debug.Print "Plot Var " & i & " = NULL "
                    '        lvaraStringTable(i) = System.DBNull.Value
                    '    End If
                    'Next i

                    i = 0
                    For Each plotvar As String In mlstPlotVar
                        'If we find the plotvar in the strata name index string
                        lintStartPos = InStr(1, lStrataA(k).strName, plotvar)
                        If lintStartPos > 0 Then
                            lintStartPos = InStr(lintStartPos + 1, lStrataA(k).strName, " ")
                            lintEndPos = InStr(lintStartPos + 1, lStrataA(k).strName, " ")
                            If lintEndPos = 0 Then
                                lvaraStringTable(i) = lStrataA(k).strName
                            Else
                                lvaraStringTable(i) = Mid(lStrataA(k).strName, lintStartPos + 1, lintEndPos - lintStartPos - 1)
                            End If
                            Debug.Print("Plot Var " & plotvar & " = " & lvaraStringTable(i))
                            lintStartPos = lintEndPos
                        Else
                            'Otherwise, it should be held constant, at the name of the plot var, or the
                            'Debug.Print "Plot Var " & i & " = NULL "
                            lvaraStringTable(i) = System.DBNull.Value
                        End If
                        i = i + 1
                    Next

                    Debug.Print("Plot Vars")
                    For i = 0 To UBound(lvaraStringTable)
                        Debug.Print(lvaraStringTable(i))
                    Next i
                    ''''''''''''''''''''''''''Modification -1
                    'j = 1  'den4

                    'Set up column names for this strata thingy

                    'For i = 1 To UBound(mstraPlotVar)
                    '    lvaraOutTable(i + 1, lintrows + j) = lvaraStringTable(i)
                    'Next i
                    'i = i + 1
                    'lvaraOutTable(1, lintrows + 1) = 0
                    'lvaraOutTable(i, lintrows + j) = lintaFailed(j)
                    'lvaraOutTable(i + 1, lintrows + j) = lintaCensored(j)
                    'lvaraOutTable(i + 2, lintrows + j) = lintaRisk(j)
                    'lvaraOutTable(i + 3, lintrows + j) = 1
                    'lvaraOutTable(i + 4, lintrows + j) = 1
                    'lvaraOutTable(i + 5, lintrows + j) = 0
                    'If ldblaSTE(1) = 0 Then lintEstimated = 2
                    'There is extra data sitting around in
                    'For j = 1 To UBound(ldblaSTimes)
                    For j = 0 To UBound(ldblaSTimes)
                        'lvaraOutTable(1, lintrows + j) = ldblaSTimes(j)
                        lvaraOutTable(0, lintrows + j - 1) = ldblaSTimes(j)
                        'Set up column names for this strata thingy
                        'For i = 1 To UBound(mstraPlotVar)
                        For i = 0 To mlstPlotVar.Count - 1
                            'lvaraOutTable(i + 1, lintrows + j) = lvaraStringTable(i)
                            lvaraOutTable(i, lintrows + j - 1) = lvaraStringTable(i)
                        Next i
                        i = i + 1
                        'Risk set Data
                        lvaraOutTable(i - 1, lintrows + j - 1) = lintaFailed(j)
                        lvaraOutTable(i, lintrows + j - 1) = lintaCensored(j)
                        lvaraOutTable(i + 1, lintrows + j - 1) = lintaRisk(j)
                        'Survival curve
                        lvaraOutTable(i + 2, lintrows + j - 1) = ldblaExpected(j)
                        'Regressed survival curve
                        lvaraOutTable(i + 3, lintrows + j - 1) = ldblaESurvival(j)
                        lvaraOutTable(i + 4, lintrows + j - 1) = ldblaActualHazard(j)
                    Next j
                    'Fix El Endo
                    If UBound(lintaFailed) - UBound(ldblaSTimes) = 1 Then
                        For i = 0 To mlstPlotVar.Count
                            lvaraOutTable(i + 1, lintrows + j) = lvaraStringTable(i)
                        Next i
                        i = i + 1
                        lvaraOutTable(1, lintrows + j) = ldblaSTE(UBound(ldblaSTE))
                        lvaraOutTable(i, lintrows + j) = lintaFailed(UBound(lintaFailed))
                        lvaraOutTable(i + 1, lintrows + j) = lintaCensored(UBound(lintaCensored))
                        lvaraOutTable(i + 2, lintrows + j) = lintaRisk(UBound(lintaRisk))
                        lvaraOutTable(i + 3, lintrows + j) = ldblaExpected(UBound(ldblaExpected))
                        lvaraOutTable(i + 4, lintrows + j) = ldblaESurvival(UBound(ldblaESurvival))
                        lvaraOutTable(i + 5, lintrows + j) = ldblaActualHazard(UBound(ldblaActualHazard))
                        lintrows = lintrows + 1
                    End If
                    lintrows = lintrows + UBound(ldblaSTimes)
                End If
            Next k
        End If
        'If Len(llstCovariatePlots(0)) > 0 Or Len(llstStrataPlots(0)) > 0 Then
        If Len(lstraCovariatePlots(0)) > 0 Or Len(lstraStrataPlots(0)) > 0 Then
            Debug.Print("")
            Debug.Print("Plotting Strata or Covariate Plots")
            'For Strata and Plot Vars
            'SetStratified(lstraStrataPlots, lsvar)
            SetStratified(lstraStrataPlots, lsvar)
            LoadStrata(lsvar, lStrataA)
            'ReDim lintaVariableIndexArray(UBound(lstraCovariatePlots))
            'ReDim lintaVariablePos(UBound(lstraCovariatePlots))
            'ReDim lintaVariableSize(UBound(lstraCovariatePlots))
            'ReDim lintaPlotVarIndex(UBound(lstraCovariatePlots))
            'ReDim lintaVariableInfo(UBound(mVarArray))
            'ReDim lintaVariableInfo(1 To UBound(ldblaB))

            ReDim lintaVariableIndexArray(UBound(lstraCovariatePlots))
            ReDim lintaVariablePos(UBound(lstraCovariatePlots))
            ReDim lintaVariableSize(UBound(lstraCovariatePlots))
            ReDim lintaPlotVarIndex(UBound(lstraCovariatePlots))
            ReDim lintaVariableInfo(UBound(mVarArray))
            'ReDim lintaVariableInfo(1 To UBound(ldblaB))  'previously commented prior to import to .Net

            ldblFinalTime = lStrataA(0).dblaData(UBound(lStrataA(0).dblaData, 1), 0)
            For k = 1 To UBound(lStrataA)
                If ldblFinalTime < lStrataA(k).dblaData(UBound(lStrataA(k).dblaData, 1), 1) Then
                    ldblFinalTime = lStrataA(k).dblaData(UBound(lStrataA(k).dblaData, 1), 1)
                End If
            Next k

            For k = 0 To UBound(lStrataA)
                ReDim ldblaMeans(UBound(lStrataA(k).mdblaTime) - 1)
                'Get the baseLine Curves for each strata
                'If UBound(mstraTimeDependentVar) > 0 Then
                If mstraTimeDependentVar.Count > 0 And Not Len(mstraTimeDependentVar(0)) = 0 Then
                    ldblaHazard = BaseLineHazardT(lStrataA(k).dblaData, ldblaB, lStrataA(k).lintcols, lStrataA(k).lintrows, ldblaSTimes, mintOffset, lStrataA(k).mdblaTime, lStrataA(k).intaTimeSelectors, lStrataA(k).intaDataColumns)
                Else
                    ldblaHazard = BaseLineHazard(lStrataA(k).dblaData, ldblaB, lStrataA(k).lintcols, lStrataA(k).lintrows, ldblaSTimes, mintOffset)
                End If

                ldblaBaselineSurvival = BaseLineSurvivalCurve(ldblaHazard)
                ldblaExpected = Expected(lStrataA(k).dblaData, lStrataA(k).lintrows, lStrataA(k).lintcols, 2, ldblaSTimes, lintaRisk, lintaFailed, lintaCensored)
                lintArrayToChange = 0
                'Variable Info Initialized
                For i = 0 To UBound(lintaVariableInfo)
                    lintaVariableInfo(i) = 0
                Next i

                'Figure out the size of the plot variables..
                'a type 5 (time dependent expand term) variable willl never be a plot var
                For i = 0 To mlstPlotVar.Count - 1
                    'Get the Name of the Variable
                    lintStartPos = InStr(1, lStrataA(k).strName, mlstPlotVar(i))
                    If lintStartPos > 0 Then
                        lintStartPos = InStr(lintStartPos + 1, lStrataA(k).strName, " ")
                        lintEndPos = InStr(lintStartPos + 1, lStrataA(k).strName, " ")

                        If lintEndPos = 0 Then
                            lvaraStringTable(i) = Left(lStrataA(k).strName, lintStartPos - 1) & " " & Mid(lStrataA(k).strName, lintStartPos + 1)
                        Else
                            lvaraStringTable(i) = Mid(lStrataA(k).strName, lintStartPos + 1, lintEndPos - 1 - lintStartPos)
                        End If
                        lintStartPos = lintEndPos
                    ElseIf IsCovariate(mstraPlotVar(i)) >= 0 Then
                        lvaraStringTable(i) = System.DBNull.Value
                        'However, fix the array that needs this data
                        'Search the mvararray for this variable number
                        'i.e. it's column in the data array
                        For j = 0 To UBound(mVarArray)
                            If StrComp(mVarArray(j).strName, mlstPlotVar(i)) = 0 Then
                                GoTo NEXTO
                            End If
                        Next j
NEXTO:
                        'j is the location in the data array
                        lintaVariableInfo(j) = lintArrayToChange
                        lintaVariableIndexArray(lintArrayToChange) = j

                        'For Continous Variables
                        If mVarArray(j).iType = 3 Then
                            lintaVariableSize(lintArrayToChange) = 1
                        Else
                            lintaVariableSize(lintArrayToChange) = mVarArray(j).iSize + 1
                        End If

                        lintaVariablePos(lintArrayToChange) = 0
                        lintaPlotVarIndex(lintArrayToChange) = i
                        lintArrayToChange = lintArrayToChange + 1
                    Else
                        'Otherwise, it should be held constant, at the name of the plot var, or the
                        lvaraStringTable(i) = System.DBNull.Value
                    End If
                Next i
                'The variables that need to be plotted are put in the
                'lintaplotvarindex
                'lintavariableindexarray holds the indexes in the data array
                'of the variables to plot.

                'This loop Creates the names for the variables
                'And sets up the data to get the means
                'Since the Baseline hazard curve has already been creatd
                Dim PlotVariableLevel As Integer
                PlotVariableLevel = 0
                lboolLoop = True
                While lboolLoop = True
                    If Len(lstraCovariatePlots(0)) <> 0 Then
                        'Loop through the plot variables
                        For m = 0 To UBound(lintaVariableIndexArray)
                            'Set the plot var categories
                            If lintaVariableSize(m) < 3 Then
                                If mVarArray(lintaVariableIndexArray(m)).iType <> 3 Then
                                    lvaraStringTable(lintaPlotVarIndex(m)) = mVarArray(lintaVariableIndexArray(m)).strName & " " & mVarArray(lintaVariableIndexArray(m)).strNames(lintaVariablePos(m))
                                Else
                                    lvaraStringTable(lintaPlotVarIndex(m)) = mVarArray(lintaVariableIndexArray(m)).strName & " " & VB6.Format(mdblaMeans(lintaVariableIndexArray(m)), "0.0000")
                                End If
                            Else
                                'ZFJ4 changed the If condition to keep array in bounds when graph variable has 4 levels
                                'If lintaVariablePos(m) = lintaVariableSize(m) Then
                                If lintaVariablePos(m) = 0 Then
                                    lvaraStringTable(lintaPlotVarIndex(m)) = mVarArray(lintaVariableIndexArray(m)).strName & " " & mVarArray(lintaVariableIndexArray(m)).strBase
                                Else
                                    lvaraStringTable(lintaPlotVarIndex(m)) = mVarArray(lintaVariableIndexArray(m)).strName & " " & mVarArray(lintaVariableIndexArray(m)).strNames(lintaVariablePos(m) - 1)
                                End If
                            End If
                        Next m
                    Else
                        lboolLoop = False
                    End If

                    'Find the Means of the data
                    For j = 0 To UBound(ldblaTempValues)
                        ldblaTempValues(j) = 0
                    Next j

                    'Initialize the ARray
                    For j = 0 To UBound(ldblaExpandedValues, 1) ' turn the 1 back to 2??
                        ldblaExpandedValues(0, j) = 0
                        ldblaValues(j) = 0
                        lintaColumn(j) = -1
                    Next j

                    For j = 0 To UBound(mVarArray)
                        lintIndex = lintaVariableInfo(j)
                        With mVarArray(j)
                            'If the variable is of type 0, it is a boolean with no missing values
                            If .iType = 0 Then
                                If lintIndex >= 0 Then ' May need fix for array index change...
                                    ldblaTempValues(.iColumn - 1) = lintaVariablePos(lintIndex)
                                    lintaColumn(.iColumn - 1) = .iColumn + mintOffset - 1
                                    ldblaValues(.iColumn - 1) = lintaVariablePos(lintIndex)
                                Else
                                    ldblaTempValues(.iColumn - 1) = mdblaMeans(j)
                                End If
                                'Otherwise, the variable is type one, Then it has a base string
                            ElseIf .iType = 1 Then
                                'Need to decide what setting it is at
                                If lintIndex > 0 Then
                                    ldblaTempValues(.iColumn - 1) = lintaVariablePos(lintIndex)
                                    lintaColumn(.iColumn - 1) = .iColumn + mintOffset - 1
                                    ldblaValues(.iColumn - 1) = lintaVariablePos(lintIndex)
                                Else
                                    ldblaTempValues(.iColumn - 1) = mdblaMeans(j)
                                End If
                                'For Variables of type two, it is a dummy null with string values
                            ElseIf .iType = 2 Or .iType = 4 Or .iType = 5 Then
                                If lintIndex > 0 Then
                                    If lintaVariablePos(lintIndex) > 1 Then
                                        ldblaTempValues(.iColumn + lintaVariablePos(lintIndex) - 2) = 1
                                        lintaColumn(.iColumn + lintaVariablePos(lintIndex) - 2) = .iColumn + mintOffset - 1 + lintaVariablePos(lintIndex) - 2
                                        ldblaValues(.iColumn + lintaVariablePos(lintIndex) - 2) = 1
                                    Else
                                        For p = 1 To .iSize
                                            'ldblaTempValues(.iColumn + p - 2) = 1
                                            lintaColumn(.iColumn + p - 1) = .iColumn + mintOffset - 1 + p - 1
                                            ldblaValues(.iColumn + p - 1) = 0
                                        Next p
                                    End If
                                Else
                                    'if we are going to take the means of this dummy
                                    'do it at all levels...
                                    If .iType = 5 Then
                                        For p = 1 To .iSize
                                            ldblaTempValues(.iColumn + p - 1) = 0
                                        Next p
                                    Else
                                        For p = 1 To .iSize
                                            'ZFJ4 changed the If condition to keep array in bounds when graph variable has 4 levels
                                            'ldblaTempValues(.iColumn + p - 1) = mdblaMeans(j + p - 1)
                                            ldblaTempValues(p - 1) = mdblaMeans(j + p - 1)
                                        Next p
                                    End If
                                End If
                                'If the variable is of type 3, Do value at 0 or one
                            ElseIf .iType = 3 Then

                                ldblaTempValues(.iColumn - 1) = mdblaMeans(j)


                            End If
                        End With
                    Next j

                    RecursiveFactorializeCox(ldblaTempValues, 0, ldblaExpandedValues, 1)

                    ldblMean = 0
                    'If UBound(mstraTimeDependentVar) > 0 Then
                    If mstraTimeDependentVar.Count > 1 Then
                        For i = 1 To UBound(ldblaB)
                            ldblaExpandedValues(1, lStrataA(k).intaDataColumns(i)) = ldblaB(i) * ldblaExpandedValues(1, lStrataA(k).intaDataColumns(i))
                        Next
                    Else

                        'For i = 0 To UBound(ldblaExpandedValues, 2)
                        'ldblMean = ldblMean + ldblaB(i) * ldblaExpandedValues(0, i)
                        'Next i
                        'ldblMean = 0
                        If PlotVariableLevel = 0 Then
                            ldblMean = 0
                        Else
                            ldblMean = ldblaB(PlotVariableLevel - 1)
                        End If
                        PlotVariableLevel = PlotVariableLevel + 1
                    End If

                    If mstraTimeDependentVar.Count > 1 Then
                        ldblaESurvival = SurvivalCurveT(lStrataA(k).dblaData, ldblaBaselineSurvival, lStrataA(k).mdblaTime, lStrataA(k).intaTimeSelectors, lStrataA(k).intaDataColumns, ldblaSTimes, ldblaExpandedValues)
                        ldblaActualHazard = HazardCurveT(lStrataA(k).dblaData, ldblaHazard, lStrataA(k).mdblaTime, lStrataA(k).intaTimeSelectors, lStrataA(k).intaDataColumns, ldblaSTimes, ldblaExpandedValues)
                    Else
                        ldblaESurvival = SurvivalCurve(ldblaBaselineSurvival, ldblMean)
                        ldblaActualHazard = HazardCurve(ldblaHazard, ldblMean)
                    End If

                    'ReDim Preserve lvaraOutTable(lintcols, lintrows + UBound(ldblaSTimes))

                    Dim lintrowsTemp As Integer
                    lintrowsTemp = lintrows
                    If lintrows > 0 Then
                        lintrowsTemp = lintrowsTemp + 1
                    End If
                    Dim lvaraOutTableNew(lintcols - 1, lintrowsTemp + UBound(ldblaSTimes)) As Object

                    For i = 0 To UBound(lvaraOutTable, 1)
                        For o = 0 To UBound(lvaraOutTable, 2)
                            lvaraOutTableNew(i, o) = lvaraOutTable(i, o)
                        Next
                    Next

                    lvaraOutTable = lvaraOutTableNew


                    'Must calculate a new expected curve

                    'right here, must check to see if there are covariate plots...
                    If Len(lstraCovariatePlots(0)) = 0 Then
                        ldblaExpected2 = VB6.CopyArray(ldblaExpected)
                        ldblaSTE = VB6.CopyArray(ldblaSTimes)
                    Else
                        'put another thingy here to protect from making duplicated expected curves
                        lintCount = 0
                        For i = 0 To UBound(lintaColumn)
                            If lintaColumn(i) > 0 Then
                                lintCount = lintCount + 1
                            End If
                        Next i
                        'Always make expected curves
                        'If lintCount <> 0 Then
                        'If UBound(mstraTimeDependentVar) > 0 Then
                        If mstraTimeDependentVar.Count - 1 > 0 Then
                            ldblaExpected2 = ExpectedForCovariates2(lStrataA(k).dblaData, lStrataA(k).lintrows, lStrataA(k).lintcols, lintaColumn, ldblaValues, ldblaSTE, lintaRisk, lintaFailed, lintaCensored)
                        Else
                            ldblaExpected2 = ExpectedForCovariates2(lStrataA(k).dblaData, lStrataA(k).lintrows, lStrataA(k).lintcols, lintaColumn, ldblaValues, ldblaSTE, lintaRisk, lintaFailed, lintaCensored)
                        End If
                        'Else
                        '    ldblaExpected2 = ldblaExpected
                        '    ldblaSTE = ldblaSTimes
                        'End If
                    End If
                    lintEstimated = 1

                    'Fix Beginning...
                    'j = 1
                    'For i = 1 To UBound(mstraPlotVar)
                    '        lvaraOutTable(i + 1, lintrows + j) = lvaraStringTable(i)
                    'Next i
                    'i = i + 1
                    '
                    'lvaraOutTable(1, lintrows + j) = lintaFailed(j)
                    'lvaraOutTable(i, lintrows + j) = 0
                    'lvaraOutTable(i + 1, lintrows + j) = lintaCensored(j)
                    'lvaraOutTable(i + 2, lintrows + j) = lintaRisk(j)
                    'lvaraOutTable(i + 3, lintrows + j) = 1
                    'lvaraOutTable(i + 4, lintrows + j) = 1
                    'lvaraOutTable(i + 5, lintrows + j) = 0
                    'If ldblaSTE(1) = 0 Then lintEstimated = 2
                    'Beginning fixed
                    For j = 0 To UBound(ldblaSTimes)

                        Dim q As Integer
                        q = j
                        If lintrows > 0 Then
                            q = j + 1
                        End If

                        'For i = 1 To UBound(mstraPlotVar)
                        For i = 0 To mlstPlotVar.Count - 1
                            lvaraOutTable(i + 1, lintrows + q) = lvaraStringTable(i)
                        Next i
                        i = i + 1

                        ' ''Expected curves generated for part of the risk set...
                        'Output STime to table
                        lvaraOutTable(0, lintrows + q) = ldblaSTimes(j)
                        'Set up column names for this strata thingy

                        'So there is one of these for each failure, plus an end point, and a starting point..
                        If lintEstimated <= UBound(ldblaSTE) + 1 Then
                            While ldblaSTE(lintEstimated - 1) < ldblaSTimes(j)
                                lintEstimated = lintEstimated + 1
                                If lintEstimated > UBound(ldblaSTE) Then
                                    'There is a Value for the outtable, a censored ending...
                                    If ldblaSTE(UBound(ldblaSTE)) < ldblaSTimes(j) Then
                                        lintEstimated = lintEstimated - 1
                                        ReDim Preserve lvaraOutTable(lintcols, lintrows + UBound(ldblaSTimes) + 1)
                                        lvaraOutTable(1, lintrows + j) = ldblaSTE(lintEstimated)
                                        lvaraOutTable(i, lintrows + j) = lintaFailed(lintEstimated)
                                        lvaraOutTable(i + 1, lintrows + j) = lintaCensored(lintEstimated)
                                        lvaraOutTable(i + 2, lintrows + j) = lintaRisk(lintEstimated)
                                        lvaraOutTable(i + 3, lintrows + j) = ldblaExpected2(lintEstimated)
                                        lvaraOutTable(i + 4, lintrows + j) = ldblaESurvival(j)
                                        lvaraOutTable(i + 5, lintrows + j) = ldblaActualHazard(j)
                                        lintEstimated = lintEstimated + 1
                                        lintrows = lintrows + 1
                                        lvaraOutTable(1, lintrows + j) = ldblaSTimes(j)
                                        'For i = 1 To UBound(mstraPlotVar)
                                        For i = 0 To mlstPlotVar.Count
                                            lvaraOutTable(i + 1, lintrows + j) = lvaraStringTable(i)
                                        Next i
                                        i = i + 1
                                        GoTo Boogoo
                                    End If
                                End If
                            End While

                            If ldblaSTE(lintEstimated - 1) = ldblaSTimes(j) Then
                                lvaraOutTable(i, lintrows + q) = lintaFailed(lintEstimated - 1)
                                lvaraOutTable(i + 1, lintrows + q) = lintaCensored(lintEstimated - 1)
                                lvaraOutTable(i + 2, lintrows + q) = lintaRisk(lintEstimated - 1)
                                lvaraOutTable(i + 3, lintrows + q) = ldblaExpected2(lintEstimated - 1)
                                lintEstimated = lintEstimated + 1
                            Else
                                lvaraOutTable(i, lintrows + j) = 0
                                lvaraOutTable(i + 1, lintrows + j) = 0
                                lvaraOutTable(i + 2, lintrows + j) = lintaRisk(lintEstimated - 1)
                                lvaraOutTable(i + 3, lintrows + j) = ldblaExpected2(lintEstimated - 1)
                            End If

                        ElseIf lintEstimated = UBound(ldblaSTE) Then

                            If ldblaSTE(lintEstimated - 1) = ldblaSTimes(j) Then
                                lvaraOutTable(i, lintrows + j) = lintaFailed(lintEstimated - 1)
                                lvaraOutTable(i + 1, lintrows + j) = lintaCensored(lintEstimated - 1)
                                lvaraOutTable(i + 2, lintrows + j) = lintaRisk(lintEstimated - 1)
                                lvaraOutTable(i + 3, lintrows + j) = ldblaExpected2(lintEstimated - 1)
                                lintEstimated = lintEstimated + 1
                            Else
                                lvaraOutTable(i, lintrows + j) = 0
                                lvaraOutTable(i + 1, lintrows + j) = 0
                                lvaraOutTable(i + 2, lintrows + j) = 0
                                lvaraOutTable(i + 3, lintrows + j) = ldblaExpected2(lintEstimated - 1)
                            End If
                        Else
Boogoo:
                            lvaraOutTable(i, lintrows + q) = 0
                            lvaraOutTable(i + 1, lintrows + q) = 0
                            lvaraOutTable(i + 2, lintrows + q) = 0
                            lvaraOutTable(i + 3, lintrows + q) = ldblaExpected2(UBound(ldblaExpected2))
                        End If

                        '                    'Survival curve
                        '                    If (lintEstimated <= UBound(ldblaSTE)) Then
                        '                        If ldblaSTE(lintEstimated) = ldblaSTimes(j) Then
                        '
                        '                            lintEstimated = lintEstimated + 1
                        '                        Else
                        '                            lvaraOutTable(i + 3, lintrows + j) = Null
                        '                        End If
                        '                    Else
                        '                            lvaraOutTable(i + 3, lintrows + j) = Null
                        '
                        '                    End If

                        'Regressed survival curve
                        'Checkie the end poitn
                        'One point per failure..
                        lvaraOutTable(i + 4, lintrows + q) = ldblaESurvival(j)
                        lvaraOutTable(i + 5, lintrows + q) = ldblaActualHazard(j)
                    Next j
                    lintrows = lintrows + UBound(ldblaSTimes)
                    'Reset this variables position and increment the other variables pos
                    'For m = UBound(lstraCovariatePlots) To 1 Step -1
                    For m = UBound(lstraCovariatePlots) To 0 Step -1
                        'Increment the variable pos
                        lintaVariablePos(m) = lintaVariablePos(m) + 1
                        'See if we need to go backwards
                        If lintaVariablePos(m) + 1 > lintaVariableSize(m) Then
                            If m = 0 Then
                                lboolLoop = False
                            End If
                            lintaVariablePos(m) = 1
                        Else
                            GoTo BREAKFOR
                        End If
                    Next m
BREAKFOR:
                End While

            Next k
        End If
        'gconDB.Close()
        Debug.Print("RESULT TABLE")

        Debug.Print(VB6.TabLayout("STime", "PVars", "Died ", "Censor", "Riskset", "ActualS", "ExpectS", "Hazard"))
        For i = 1 To UBound(lvaraOutTable, 2)

            For j = 1 To UBound(lvaraOutTable, 1)
                If VarType(lvaraOutTable(j, i)) = VariantType.Null Then
                    System.Diagnostics.Debug.Write(VB6.TabLayout(TAB))
                Else
                    System.Diagnostics.Debug.Write(VB6.TabLayout(VB6.Format(lvaraOutTable(j, i), "0.0000"), TAB))
                End If
            Next j
            Debug.Print("")
        Next i

        'ReDim Preserve Results(2, UBound(Results, 2) + 1)
        Dim NewResults(2, UBound(Results, 2) + 1)

        For i = 0 To UBound(Results, 2)
            For j = 0 To 1
                NewResults(j, i) = Results(j, i)
            Next
        Next

        Results = NewResults

        PlotGraphs = String.Empty

cleanup:
        Results(0, UBound(Results, 2)) = "outtable"
        Results(1, UBound(Results, 2)) = VB6.CopyArray(lvaraOutTable)

        Dim lstrGraphOut As String
        lstrGraphOut = String.Empty

        Dim lstrInputParams As String
        Dim lstrChartTitle As String
        Dim lstrDomainLabel As String
        Dim lstrChartType As String
        Dim lstrIndependentValueType As String
        Dim lstrChartLineSeries As String

        lstrDomainLabel = "domainLabel=" & "Cox PH" & " vs. " & mstrTimeVar
        lstrChartTitle = "chartTitle=" & "Survival Probability"
        lstrChartType = "chartType=" & "Line"
        lstrIndependentValueType = "independentValueType=" & "System.Double"
        lstrChartLineSeries = "chartLineSeries=("
        Dim lstrGroupVar As String
        lstrGroupVar = Trim$(lvaraOutTable(1, 0))
        lstrGroupVar = lstrGroupVar.Replace(" ", "=")
        
        lstrChartLineSeries = lstrChartLineSeries & "LineSeriesTitle" & lstrGroupVar & "LineSeriesTitle" & "LineSeriesDataString"
        ' hard-coded to work with the single test case we have, for now
        lstrChartLineSeries = lstrChartLineSeries & lvaraOutTable(0, 0) & "^sidv^" & lvaraOutTable(6, 0)
        Dim prevValue As Double = lvaraOutTable(6, 0)
        For i = 1 To UBound(lvaraOutTable, 2)

            If (lvaraOutTable(1, i).Equals(lvaraOutTable(1, i - 1))) Then
                lstrChartLineSeries = lstrChartLineSeries & "^&^"
            Else
                lstrChartLineSeries = lstrChartLineSeries & "LineSeriesDataString)^("
                lstrGroupVar = Trim$(lvaraOutTable(1, i))
                lstrGroupVar = lstrGroupVar.Replace(" ", "=")
                lstrChartLineSeries = lstrChartLineSeries & "LineSeriesTitle" & lstrGroupVar & "LineSeriesTitle" & "LineSeriesDataString"
                prevValue = lvaraOutTable(6, i)
            End If
            lstrChartLineSeries = lstrChartLineSeries & (CDbl(lvaraOutTable(0, i)) - 0.0000000001) & "^sidv^" & prevValue & "^&^"
            lstrChartLineSeries = lstrChartLineSeries & lvaraOutTable(0, i) & "^sidv^" & lvaraOutTable(6, i)
            prevValue = lvaraOutTable(6, i)
        Next i

        lstrChartLineSeries = lstrChartLineSeries & "LineSeriesDataString"
        lstrChartLineSeries = lstrChartLineSeries & ")"
        lstrInputParams = lstrDomainLabel & "," & lstrChartType & "," & "independentLabel=Time,dependentLabel=Survival Probability," & lstrIndependentValueType & "," & lstrChartLineSeries

        lstrGraphOut = lstrGraphOut & "<br clear=""all"">"
        lstrGraphOut = lstrGraphOut & "<div id=""silverlightControlHost"">"
        lstrGraphOut = lstrGraphOut & "<object data=""data:application/x-silverlight-2,"""
        lstrGraphOut = lstrGraphOut & "type=""application/x-silverlight-2"" style=""width:820; height: 615px"">"
        lstrGraphOut = lstrGraphOut & "<param name=""initparams"" value=""" & lstrInputParams & """/>"
        lstrGraphOut = lstrGraphOut & "<param name=""source"" value=""SilverlightApplication.xap"" />"
        lstrGraphOut = lstrGraphOut & "<param name=""onError"" value=""onSilverlightError"" />"
        lstrGraphOut = lstrGraphOut & "<param name=""minRuntimeVersion"" value=""4.0.50826.0"" />"
        lstrGraphOut = lstrGraphOut & "<param name=""autoUpgrade"" value=""true"" />"
        lstrGraphOut = lstrGraphOut & "<a href=""http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50826.0"" style=""text-decoration: none"">"
        lstrGraphOut = lstrGraphOut & "<img src=""http://go.microsoft.com/fwlink/?LinkId=161376"" alt=""Get Microsoft Silverlight"""
        lstrGraphOut = lstrGraphOut & "style=""border-style: none"" />"
        lstrGraphOut = lstrGraphOut & "</a>"
        lstrGraphOut = lstrGraphOut & "</object>"
        lstrGraphOut = lstrGraphOut & "</div>"
        lstrGraphOut = lstrGraphOut & "<br />"

        PlotGraphs = lstrGraphOut

        'gconDB = Nothing
        Exit Function
errPlotGraphs:
        MsgBox(Err.Description, MsgBoxStyle.MsgBoxSetForeground, "PlotGraphs")
        Exit Function
        Resume cleanup
        Resume
    End Function

    'This function commented as unneeded since mstraStrataVar is now a List rather than array.
    '  Determine if the string exists in the List using the .Contains method.
    Public Function IsStrataVar(ByRef lstr As String) As Integer
        Dim i As Integer
        For i = 0 To UBound(mstraStrataVar)
            If StrComp(mstraStrataVar(i), lstr) = 0 Then
                IsStrataVar = i
                Exit Function
            End If
        Next i
        IsStrataVar = -1
        Exit Function
    End Function

    'This function commented as unneeded since mstraCovariates is now a List rather than array.
    '  Determine if the string exists in the List using the .Contains method.
    Public Function IsCovariate(ByRef lstr As String) As Integer
        Dim i As Integer
        For i = 0 To UBound(mVarArray)
            If StrComp(mVarArray(i).strName, lstr) = 0 Then
                IsCovariate = i
                Exit Function
            End If
        Next i
        IsCovariate = -1
        Exit Function
    End Function

    Public Sub GetMeans()
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim lintTotal As Integer
        ReDim mdblaMeans(mintVirtualFields - mintOffset)

        For k = 0 To UBound(mStrataA)
			With mStrataA(k)
				If mintWeight <> 0 Then
					For i = 0 To .lintrows - 1
						For j = mintOffset - 1 To .lintcols - 1
							mdblaMeans(j - mintOffset) = mdblaMeans(j - mintOffset) + .dblaData(i, j) * .dblaData(i, 2)
						Next j
						lintTotal = lintTotal + .dblaData(i, 2)
					Next i
				Else
					For i = 0 To .lintrows - 1
						For j = mintOffset - 1 To mStrataA(k).lintcols - 1
							mdblaMeans(j - mintOffset + 1) = mdblaMeans(j - mintOffset + 1) + mStrataA(k).dblaData(i, j)
						Next j
						lintTotal = lintTotal + 1
					Next i
				End If
			End With
        Next k

        For j = 0 To UBound(mdblaMeans)
            mdblaMeans(j) = mdblaMeans(j) / lintTotal
        Next j

    End Sub
    Public Sub LoadPlotTable(ByRef lstraGroups() As String, ByRef lSVarArray() As StrataVariable, ByRef lStrataA() As Strata)
        'called from EIKaplanMeierSurvival.vb  LoadPlotTable(mstraStrataVar, lSVarArray, lStrataA)
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
        ReDim lStrataA(lintStrata)
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
                            'ToDo: den4: shouldn't the next row filter strName for null OR "", rather than null AND ""?
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
        Debug.Print(lstrQuery)

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

        If k = 0 Then
            ReDim lStrataA(lintIndex).dblaData(0, lStrataA(lintIndex).lintcols - 1)
            lintOffset = 1
        Else
            ReDim lStrataA(lintIndex).dblaData(k, lStrataA(lintIndex).lintcols)
        End If
        Dim currentRow As Integer
        For currentRow = 0 To rows.Count
            'den4: Original query was 
            '   SELECT TimeVar, CensoredVar, WeightVar (if applicable), So...
            '   RS.Fields(0) = mstrTimeVar
            '   RS.Fields(1) = mstrCensoredVar 
            '   RS.Fields(2) = mstrWeightVar

            'lvarCurData = lconRS.Fields(0).Value
            lvarCurData = rows(currentRow)(mstrTimeVar)
            'This is a missing value.. then..
            If VarType(lvarCurData) = VariantType.Null Then
                lintOffset = lintOffset + 1
                GoTo MISSING
                'lStrataA(lintIndex).dblaData(i - lintOffset, 1) = 0 / 0
                'ElseIf lconRS.Fields(0).Value <= 0 Then
            ElseIf lvarCurData <= 0 Then
                Err.Raise(vbObjectError + 2333, , "<tlt>Cannot accept survival times less than or equal to zero.</tlt>")
            Else
                'lStrataA(lintIndex).dblaData(i - lintOffset, 1) = lconRS.Fields(0).Value
                lStrataA(lintIndex).dblaData(currentRow - lintOffset, 0) = lvarCurData
            End If

            'lvarCurData = lconRS.Fields(1).Value
            lvarCurData = rows(currentRow)(mstrCensoredVar)
            If VarType(lvarCurData) = VariantType.Null Then
                lintOffset = lintOffset + 1
                GoTo MISSING
            End If
            'lStrataA(lintIndex).dblaData(i - lintOffset, 2) = lconRS.Fields(1).Value
            lStrataA(lintIndex).dblaData(currentRow - lintOffset, 1) = rows(currentRow)(mstrCensoredVar)
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

                'If lconRS.Fields(2).Value <= 0 Then Err.Raise(vbObjectError, "", "<tlt>Weight must be greater than 0. Weight value found as: </tlt>" & lconRS.Fields(2).Value)
                'lStrataA(lintIndex).dblaData(i - lintOffset, 3) = lconRS.Fields(2).Value
                lStrataA(lintIndex).dblaData(currentRow - lintOffset, 2) = rows(currentRow)(mstrWeightVar)
            End If

MISSING:
        Next currentRow

        lStrataA(lintIndex).lintrows = lStrataA(lintIndex).lintrows - lintOffset
        For i = 0 To lStrataA(lintIndex).lintrows - 1
            debugstring = "Loaded "
            For j = 0 To lStrataA(lintIndex).lintcols - 1
                debugstring = debugstring & lStrataA(lintIndex).dblaData(i, j) & " "
            Next
            Debug.Print(debugstring)
        Next
    End Sub

    Public Function logrank2(ByRef S() As Strata, ByRef Ref() As Strata, ByRef peto As Double, ByRef logrankscore As Double, ByRef lvaraout(,) As Object, ByRef ldblaSI() As Double, ByRef lvaraTab(,) As Object) As String
        Dim G As Double
        Dim n_j As Double
        Dim m_j As Double
        Dim n_ij() As Double
        Dim m_ij As Double
        Dim mm_j As Double
        Dim e_ij As Double
        Dim ldblTime As Double
        Dim ldblaM(,) As Double
        Dim ldblaMP(,) As Double
        Dim ldblaD() As Double
        Dim ldblaT() As Double
        Dim ldblaP() As Double
        Dim ldblaV(,) As Double
        Dim ldblaSI2() As Double
        Dim ldblTemp As Double
        Dim lintWeight As Integer
        Dim j, i As Integer
        Dim mMatrixlikelihood As EIMatrix

        'Local Variables for the results table output
        Dim lstrResultsTable As String
        Dim lstrSurvTime As String
        Dim lstrGroupVar As String
        Dim lstrFailure As String
        Dim lstrCensored As String
        Dim lstrRisk As String
        Dim lstrSurvProb As String
        Dim lintCumFail As Integer

        Dim lstrGraphOut As String = String.Empty
        Dim strTestResults As String = String.Empty
        Dim lstrLogRank As String = String.Empty

        '1 = survival time
        '2 = censor
        '3 = failure
        '4 = riskset
        G = UBound(S)
        If G = 0 Then GoTo OneGroup
        'for big nasty matrix
        ReDim ldblaM(G - 1, G - 1)
        ReDim ldblaMP(G - 1, G - 1)
        ReDim ldblaV(G - 1, G - 1)
        ReDim ldblaD(G - 1)
        ReDim ldblaP(G - 1)
        ReDim ldblaT(G - 1)
        ReDim n_ij(G - 1)

        Dim ldblaIndexer(G) As Object
        ReDim ldblaSI(G)
        ReDim ldblaSI2(G)

        For i = 0 To G
            'ldblaSI(i) = 1
            ldblaSI(i) = 0
            'ldblaSI2(i) = 1
            ldblaSI2(i) = 0
        Next i

        Debug.Print(VB6.TabLayout("time", "n_j", "m_j", "m_ij", "m_ij - e_ij"))
        ldblTime = 1.2312318239081E+15
        For i = 0 To G
            If ldblTime > S(i).dblaData(ldblaSI(i), 0) Then
                ldblTime = S(i).dblaData(ldblaSI(i), 0)
            End If
            n_j = n_j + S(i).dblaData(ldblaSI(i), 3)
            If i <> G Then
                n_ij(i) = S(i).dblaData(ldblaSI(i), 3)
            End If
        Next i
        While n_j > 0
            m_j = 0
            For i = 0 To G
                If S(i).dblaData(ldblaSI(i), 0) = ldblTime Then
                    m_j = m_j + S(i).dblaData(ldblaSI(i), 2)
                End If
            Next i
            For i = 0 To G - 1
                m_ij = 0
                If S(i).dblaData(ldblaSI(i), 0) = ldblTime Then
                    m_ij = S(i).dblaData(ldblaSI(i), 2)
                End If

                'Check for divide by zero
                e_ij = n_ij(i) * m_j / n_j

                Debug.Print(VB6.TabLayout(ldblTime, n_j, m_j, m_ij, m_ij - e_ij))

                ldblaD(i) = ldblaD(i) + (m_ij - e_ij)
                ldblaP(i) = ldblaP(i) + (m_ij - e_ij) * n_j

                If (n_j > 1) Then
                    mm_j = (n_j - m_j) * m_j / (n_j * n_j * (n_j - 1.0#))
                End If

                For j = 0 To G - 1
                    If j = i Then
                        'Calculate Variance...
                        'add protection
                        If (n_j > 1) Then
                            ldblaM(j, i) = ldblaM(i, j) + n_ij(i) * (n_j - n_ij(i)) * mm_j
                            ldblaMP(j, i) = ldblaMP(j, i) + n_ij(i) * (n_j - n_ij(i)) * mm_j * n_j * n_j
                        End If
                    Else
                        'Calculate Covariance
                        If (n_j > 1) Then
                            ldblaM(i, j) = ldblaM(i, j) - n_ij(i) * n_ij(j) * mm_j
                            ldblaMP(i, j) = ldblaMP(i, j) - n_ij(i) * n_ij(j) * mm_j * n_j * n_j
                        End If
                    End If
                Next j
            Next i

            ldblTemp = 100000000000.0#
            'Find the NExt Min time
            For i = 0 To G
                If ldblaSI(i) < UBound(S(i).dblaData, 1) Then
                    If S(i).dblaData(ldblaSI(i) + 1, 0) < ldblTemp Then
                        ldblTemp = S(i).dblaData(ldblaSI(i) + 1, 0)
                    End If
                End If
            Next i

            For i = 0 To G
                If ldblaSI(i) <= UBound(S(i).dblaData, 1) Then
                    If S(i).dblaData(ldblaSI(i), 0) < ldblTemp And S(i).dblaData(ldblaSI(i), 0) > ldblTime Then
                        ldblTemp = S(i).dblaData(ldblaSI(i), 0)
                    End If
                End If
            Next i
            ldblTime = ldblTemp

            For i = 0 To G
                If ldblaSI(i) < UBound(S(i).dblaData, 1) Then
                    If S(i).dblaData(ldblaSI(i), 0) < ldblTime Then
                        ldblaSI(i) = ldblaSI(i) + 1
                    End If
                End If
            Next i

            'Count Data Points before this time interval, they need to be removed.
            For i = 0 To G
                Do While ldblaSI2(i) <= UBound(Ref(i).dblaData, 1)
                    If Ref(i).dblaData(ldblaSI2(i), 0) >= ldblTime Then
                        Exit Do
                    End If

                    'Gotta subtract the weight also...
                    If mintWeight <> 0 Then
                        lintWeight = Ref(i).dblaData(ldblaSI2(i), 2)
                    Else
                        lintWeight = 1
                    End If
                    n_j = n_j - lintWeight

                    If i <> G Then
                        n_ij(i) = n_ij(i) - lintWeight
                    End If

                    ldblaSI2(i) = ldblaSI2(i) + 1
                Loop
            Next i

        End While
        mMatrixlikelihood = New EIMatrix
        i = mMatrixlikelihood.inv(ldblaM, ldblaV)

        For i = 0 To G - 1
            For j = 0 To G - 1
                ldblaT(i) = ldblaT(i) + ldblaD(j) * ldblaV(j, i)
            Next j
        Next i

        For i = 0 To UBound(ldblaT)
            logrankscore = logrankscore + ldblaT(i) * ldblaD(i)
            'clear ldblat
            ldblaT(i) = 0
        Next

        'do peto test
        i = mMatrixlikelihood.inv(ldblaMP, ldblaV)
        For i = 0 To G - 1
            For j = 0 To G - 1
                ldblaT(i) = ldblaT(i) + ldblaP(j) * ldblaV(j, i)
            Next j
        Next i

        For i = 0 To UBound(ldblaT)
            peto = peto + ldblaT(i) * ldblaP(i)
        Next
        ReDim lvaraTab(1, 3)  'den4
        lvaraTab(0, 0) = "Log-Rank Statistic"
        lvaraTab(0, 1) = logrankscore
        lvaraTab(0, 2) = G
        lvaraTab(0, 3) = dist1.PfromX2(logrankscore, G)
        lvaraTab(1, 0) = "Wilcoxon"
        lvaraTab(1, 1) = peto
        lvaraTab(1, 2) = G
        lvaraTab(1, 3) = dist1.PfromX2(peto, G)

        Debug.Print(VB6.TabLayout("Wilcoxon " & peto, "Logrank " & logrankscore))

        lstrResultsTable = "<br clear = all><table><tr><th>" & mstrTimeVar & "</th><th>" & mstrGroupVar & "</th><th>" & Epi.SharedStrings.KMSURV_FAILURES & "</th><th>" & Epi.SharedStrings.KMSURV_CENSORED & "</th><th>" & Epi.SharedStrings.KMSURV_RISK & "</th><th>" & Epi.SharedStrings.KMSURV_SURVIVALPROB & "</th><th>" & Epi.SharedStrings.KMSURV_CUMFAILURES & "</th></tr>"

        'loop through each row
        For i = 0 To UBound(lvaraout, 2)
            'If the Survival Time {lvaraout(0,i)} is 0 then reset the cumulative Failure to 0 for the next test group var
            If lvaraout(0, i).ToString.Equals("0") Then lintCumFail = 0
            lstrSurvTime = lvaraout(0, i).ToString
            'format the Group Var = lvaraout(1, i)
            lstrGroupVar = Trim$(lvaraout(1, i))
            lstrGroupVar = Mid$(lstrGroupVar, InStr(lstrGroupVar, " ") + 1)

            'For the Failure column: lvaraout(2, i), add the failure rate to the cumulative failure
            lstrFailure = lvaraout(2, i).ToString
            lintCumFail = lintCumFail + lvaraout(2, i)

            lstrCensored = IIf(lvaraout(3, i).ToString = Nothing, "0", lvaraout(3, i).ToString)

            lstrRisk = lvaraout(4, i).ToString
            lstrSurvProb = lvaraout(5, i).ToString

            lstrResultsTable = lstrResultsTable & "<tr><td>" & lstrSurvTime & "</td><td>" & lstrGroupVar.ToString() & "</td><td>" & lstrFailure & "</td><td>" & lstrCensored & "</td><td>" & lstrRisk & "</td><td>" & lstrSurvProb & "</td><td>" & lintCumFail & "</td></tr>"

        Next i
        lstrResultsTable = lstrResultsTable & "</table>"
        
        Dim lstrInputParams As String
        Dim lstrChartTitle As String
        Dim lstrDomainLabel As String
        Dim lstrChartType As String
        Dim lstrIndependentValueType As String
        Dim lstrChartLineSeries As String

        lstrDomainLabel = "domainLabel=" & Epi.SharedStrings.KMSURV_SURVIVALPROB & " vs. " & mstrTimeVar
        lstrChartTitle = "chartTitle=" & "Survival Probability"
        lstrChartType = "chartType=" & "Line"
        lstrIndependentValueType = "independentValueType=" & "System.Double"
        lstrChartLineSeries = "chartLineSeries=("

        lstrGroupVar = Trim$(lvaraout(1, 0))
        lstrGroupVar = lstrGroupVar.Replace(" ", "=")
        lstrChartLineSeries = lstrChartLineSeries & "LineSeriesTitle" & lstrGroupVar & "LineSeriesTitle" & "LineSeriesDataString"
        lstrChartLineSeries = lstrChartLineSeries & lvaraout(0, 0) & "^sidv^" & lvaraout(5, 0)
        Dim prevValue As Double = lvaraout(5, 0)
        For i = 1 To UBound(lvaraout, 2)

            If (lvaraout(1, i).Equals(lvaraout(1, i - 1))) Then
                lstrChartLineSeries = lstrChartLineSeries & "^&^"
            Else
                lstrChartLineSeries = lstrChartLineSeries & "LineSeriesDataString)^("
                lstrGroupVar = Trim$(lvaraout(1, i))
                lstrGroupVar = lstrGroupVar.Replace(" ", "=")
                lstrChartLineSeries = lstrChartLineSeries & "LineSeriesTitle" & lstrGroupVar & "LineSeriesTitle" & "LineSeriesDataString"
                prevValue = lvaraout(5, i)
            End If

            lstrChartLineSeries = lstrChartLineSeries & (CDbl(lvaraout(0, i)) - 0.0000000001) & "^sidv^" & prevValue & "^&^"
            lstrChartLineSeries = lstrChartLineSeries & lvaraout(0, i) & "^sidv^" & lvaraout(5, i)

            prevValue = lvaraout(5, i)
        Next i

        lstrChartLineSeries = lstrChartLineSeries & "LineSeriesDataString"
        lstrChartLineSeries = lstrChartLineSeries & ")"
        'lstrInputParams = lstrChartTitle & "," & lstrDomainLabel & "," & lstrChartType & "," & lstrChartLineSeries
        lstrInputParams = lstrDomainLabel & "," & lstrChartType & "," & "independentLabel=Time,dependentLabel=Survival Probability," & lstrIndependentValueType & "," & lstrChartLineSeries

        lstrGraphOut = lstrGraphOut & "<br clear=""all"">"
        lstrGraphOut = lstrGraphOut & "<div id=""silverlightControlHost"">"
        lstrGraphOut = lstrGraphOut & "<object data=""data:application/x-silverlight-2,"""
        lstrGraphOut = lstrGraphOut & "type=""application/x-silverlight-2"" style=""width:820; height: 615px"">"
        lstrGraphOut = lstrGraphOut & "<param name=""initparams"" value=""" & lstrInputParams & """/>"
        lstrGraphOut = lstrGraphOut & "<param name=""source"" value=""SilverlightApplication.xap"" />"
        lstrGraphOut = lstrGraphOut & "<param name=""onError"" value=""onSilverlightError"" />"
        'lstrGraphOut = lstrGraphOut & "<param name=""background"" value=""lightgray"" />"
        lstrGraphOut = lstrGraphOut & "<param name=""minRuntimeVersion"" value=""4.0.50826.0"" />"
        lstrGraphOut = lstrGraphOut & "<param name=""autoUpgrade"" value=""true"" />"
        lstrGraphOut = lstrGraphOut & "<a href=""http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50826.0"" style=""text-decoration: none"">"
        lstrGraphOut = lstrGraphOut & "<img src=""http://go.microsoft.com/fwlink/?LinkId=161376"" alt=""Get Microsoft Silverlight"""
        lstrGraphOut = lstrGraphOut & "style=""border-style: none"" />"
        lstrGraphOut = lstrGraphOut & "</a>"
        lstrGraphOut = lstrGraphOut & "</object>"
        lstrGraphOut = lstrGraphOut & "</div>"
        lstrGraphOut = lstrGraphOut & "<br />"

        strTestResults = "<br clear = all><table><tr><th><b><tlt>Test</tlt></b></th>"
        strTestResults = strTestResults & "<th><B><tlt>Statistic</tlt></B></th>"
        strTestResults = strTestResults & "<th><B><TLT>D.F.</tlt></B></th>"
        strTestResults = strTestResults & "<th><B><tlt>P-Value</tlt></B></th></TR>"
        strTestResults = strTestResults & "<TR><TD><B><tlt>Log-Rank</tlt></B></TD>"
        strTestResults = strTestResults & "<TD>"
        strTestResults = strTestResults & OutClip(lvaraTab(0, 1))
        strTestResults = strTestResults & "</TD>"
        strTestResults = strTestResults & "<TD>"
        strTestResults = strTestResults & G
        strTestResults = strTestResults & "</TD>"
        strTestResults = strTestResults & "<TD>"
        strTestResults = strTestResults & OutClip(lvaraTab(0, 3))
        strTestResults = strTestResults & "</TD></TR>"
        strTestResults = strTestResults & "<TR><TD><B><TLT>Wilcoxon</TLT></B></TD>"
        strTestResults = strTestResults & "<TD>"
        strTestResults = strTestResults & OutClip(lvaraTab(1, 1))
        strTestResults = strTestResults & "</TD>"
        strTestResults = strTestResults & "<TD>"
        strTestResults = strTestResults & G
        strTestResults = strTestResults & "</TD>"
        strTestResults = strTestResults & "<TD>"
        strTestResults = strTestResults & OutClip(lvaraTab(1, 3))
        strTestResults = strTestResults & "</TD></TR></TABLE>"

        Select Case mstrGraphType

            Case "Data Table Only"
                lstrLogRank = strTestResults & lstrResultsTable

            Case "None"
                lstrLogRank = strTestResults

            Case "Survival Probability"
                lstrLogRank = lstrGraphOut & strTestResults

            Case Else
                lstrLogRank = lstrGraphOut & strTestResults

        End Select

        logrank2 = lstrLogRank

            Exit Function

OneGroup:
        logrank2 = lstrGraphOut & "<TLT>Unable to calculate Peto and Logrank Statistics: Only One Group</TLT>"
            Exit Function
breako:
        logrank2 = lstrGraphOut & "<TLT>Unable to calculate Peto and Logrank Statistics: Bad Input Data</TLT>"
    End Function
End Module