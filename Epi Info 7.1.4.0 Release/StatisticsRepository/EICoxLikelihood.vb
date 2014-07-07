Option Strict Off
Option Explicit On
Option Compare Text

Module EICoxLikelihood

    'BRESLOW METHOOD FOR HANDLING TIES
    Public Function BreslowLikelihood(ByRef lintOffset As Integer, ByRef ldblaDataArray(,) As Double, ByRef ldblaJacobian(,) As Double, ByRef ldblaB() As Double, ByRef ldblaF() As Double, ByRef nRows As Integer, ByRef nCols As Integer) As Double
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim lintMSize As Integer
        'Exponent Sum Variables
        Dim ldblaExp() As Double
        Dim ldblaExpBp() As Double
        Dim ldblaExpBpBm(,) As Double
        Dim ldblExp As Double
        Dim ldblTieSum As Double
        Dim ldblExpT As Double
        Dim ldblaExpBpT() As Double
        Dim ldblaExpBpBmT(,) As Double
        Dim lintWeight As Integer
        'Event time counters
        Dim ldblOldSTime As Double
        Dim ldblCurSTime As Double
        Dim ldblTemp As Double

        'The... ... ... Score Vector, I think
        Dim ldblZ As Double
        Dim ldblaZp() As Double
        Dim lintTies As Integer

        On Error GoTo erroRHandler

        lintMSize = UBound(ldblaB)
        Dim ldblaPreJacobian(lintMSize, lintMSize) As Object
        ReDim ldblaExp(nRows - 1)
        ReDim ldblaExpBp(lintMSize)
        ReDim ldblaExpBpBm(lintMSize, lintMSize)
        ReDim ldblaZp(lintMSize)

        Dim ldblaExpT(nRows - 1) As Double
        ReDim ldblaExpBpT(lintMSize)
        ReDim ldblaExpBpBmT(lintMSize, lintMSize)

        For i = 0 To UBound(ldblaDataArray, 1)
            Debug.Print((i + 1).ToString & " ;  " & ldblaDataArray(i, 0).ToString & " ;  " & ldblaDataArray(i, 1).ToString & " ;  " & ldblaDataArray(i, 2).ToString)
        Next

        'Formula's for partial likelihood's
        'L(B) = x*B - log(sum_Ri(exp(B*x)))
        'L'(B) = xp - 1/sum_Ri(exp(B*x))*(sum_Ri(exp(B*x))*xp)
        'L''(B) =

        'Get the exp(B*X) for every row, xp*exp(B*X) , xp*xm*exp(B*X)
        'Debug.Print("j  i   ..Exp(j)   ..DataArray(j, i)  ..B(i..) XX i  k   ..ExpBp(i..)  ..ExpBpBm(i..)")
        For j = 0 To nRows - 1
            lintWeight = 1
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            End If
            'ldblaExp stores exp(B*x) for everrow
            For i = lintOffset - 1 To nCols - 1
                ldblaExp(j) = ldblaExp(j) + ldblaDataArray(j, i) * ldblaB(i - lintOffset + 1)
                Debug.Print("<< " & j.ToString & "  " & i.ToString & " >> " & ldblaExp(j).ToString & "  " & ldblaDataArray(j, i).ToString & "  " & ldblaB(i - lintOffset + 1).ToString & "   XX ")
            Next i

            ldblTemp = System.Math.Exp(ldblaExp(j)) * lintWeight
            Debug.Print("ldblTemp = " & ldblTemp.ToString)
            For i = lintOffset - 1 To nCols - 1
                ldblaExpBp(i - lintOffset + 1) = ldblaExpBp(i - lintOffset + 1) + ldblTemp * ldblaDataArray(j, i)
                For k = lintOffset - 1 To nCols - 1
                    ldblaExpBpBm(i - lintOffset + 1, k - lintOffset + 1) = ldblaExpBpBm(i - lintOffset + 1, k - lintOffset + 1) + ldblTemp * ldblaDataArray(j, i) * ldblaDataArray(j, k)
                    Debug.Print("                XX <<" & i.ToString & "  " & k.ToString & " >>" & ldblaExpBp(i - lintOffset + 1).ToString & " " & ldblaExpBpBm(i - lintOffset + 1, k - lintOffset + 1))
                Next k
            Next i
            'ldblexp is the Current Risk-Set
            ldblExp = ldblExp + ldblTemp
        Next j

        'Enter the likelihood loop
        ldblOldSTime = ldblaDataArray(0, 0)
        lintTies = 0
        Debug.Print("j  i  k  ..ExpBpBmT(i,k)  ..Temp  ..DataArray(j, i..)  ..DataArray(j, k-..)")
        For j = 0 To nRows - 1
            ldblCurSTime = ldblaDataArray(j, 0)
            lintWeight = 1
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            End If
            If ldblOldSTime = ldblCurSTime Then
                'Guys who die at this time interval are OUT of the risk sum
                'Prepare data for guys we are removing.
                'This data goes in ldblTemp and ldblExpT
                'ldblaexpbpT, ldblaExpBpBmT

                ldblTemp = System.Math.Exp(ldblaExp(j)) * lintWeight
                ldblExpT = ldblExpT + ldblTemp
                For i = 0 To lintMSize
                    ldblaExpBpT(i) = ldblaExpBpT(i) + ldblTemp * ldblaDataArray(j, i - 1 + lintOffset)
                    For k = 0 To lintMSize
                        ldblaExpBpBmT(i, k) = ldblaExpBpBmT(i, k) + ldblTemp * ldblaDataArray(j, i - 1 + lintOffset) * ldblaDataArray(j, k - 1 + lintOffset)  'den4: dropped the (-1) from ldblaDataArray()
                        Debug.Print(j.ToString & "  " & i.ToString & "  " & k.ToString & " || " & ldblaExpBpBmT(i, k).ToString & "  " & ldblTemp.ToString & "  " & ldblaDataArray(j, i - 1 + lintOffset).ToString & "  " & ldblaDataArray(j, k - 1 + lintOffset).ToString)
                    Next k
                Next i

                'Now, check and see if a Death occured..
                If ldblaDataArray(j, 1) = 1 Then
                    'Failed
                    ldblZ = ldblZ + ldblaExp(j) * lintWeight
                    Debug.Print("i; ldblaZp(i);  ldblaDataArray(j, i - 1 + lintOffset); lintWeight")
                    For i = 0 To lintMSize
                        ldblaZp(i) = ldblaZp(i) + ldblaDataArray(j, i - 1 + lintOffset) * lintWeight
                        Debug.Print((i + 1).ToString & "   " & ldblaZp(i).ToString & "   " & ldblaDataArray(j, i - 1 + lintOffset).ToString & "   " & lintWeight.ToString)
                    Next i
                    'the ties should increment by the weight value of this row
                    lintTies = lintTies + lintWeight

                Else
                    'Censored
                End If

            Else

                'The new S Time is different than the old one..
                'Set the new old S Time
                ldblOldSTime = ldblCurSTime
                'Fix the Matrices
                If lintTies = 0 Then
                    'No ties means censored
                    'FIX THE RISK SETS
                    Debug.Print("i; k;  ldblaExpBpBm(i, k); ldblaExpBpBmT(i, k) ")
                    For i = 0 To lintMSize
                        ldblaExpBp(i) = ldblaExpBp(i) - ldblaExpBpT(i)
                        For k = 0 To lintMSize
                            ldblaExpBpBm(i, k) = ldblaExpBpBm(i, k) - ldblaExpBpBmT(i, k)
                            Debug.Print(i.ToString & "  " & k.ToString & "  " & ldblaExpBpBm(i, k).ToString & "  " & ldblaExpBpBmT(i, k).ToString)
                        Next k
                    Next i
                    ldblExp = ldblExp - ldblExpT
                    Debug.Print(ldblExp.ToString)
                    ldblExpT = 0
                    For i = 0 To lintMSize
                        ldblaZp(i) = 0
                        ldblaExpBpT(i) = 0
                        For k = 0 To lintMSize
                            ldblaExpBpBmT(i, k) = 0
                        Next k
                    Next i
                Else
                    'There is A partial likelihood to be calculated.. Calculate it
                    Debug.Print("ldblaF(i) = ldblaF(i) + ldblaZp(i) - lintTies * ldblaExpBp(i) / ldblExp")
                    For i = 0 To lintMSize
                        'Score, or First Partial Derivative
                        Debug.Print(i.ToString & " ; " & ldblaF(i) & " ; " & ldblaZp(i) & " ; " & lintTies & " ; " & ldblaExpBp(i) & " ; " & ldblExp)
                        ldblaF(i) = ldblaF(i) + ldblaZp(i) - lintTies * ldblaExpBp(i) / ldblExp
                        Debug.Print("j = " & j & " ; " & "ldblaF(i) = " & ldblaF(i))
                        For k = 0 To lintMSize
                            'Covariance Matrix, or the Jacobian
                            ldblaJacobian(i, k) = ldblaJacobian(i, k) + lintTies * ((ldblaExpBpBm(i, k) * ldblExp - ldblaExpBp(k) * ldblaExpBp(i)) / (ldblExp * ldblExp))
                            Debug.Print("(i,k) = (" & i.ToString & ", " & k.ToString & ") ; Jacobian(i,k) = " & ldblaJacobian(i, k).ToString)
                        Next k
                    Next i
                    BreslowLikelihood = BreslowLikelihood + ldblZ - lintTies * System.Math.Log(ldblExp)
                    Debug.Print("Breslow = " & BreslowLikelihood)

                    lintTies = 0
                    ldblZ = 0

                    'FIX THE RISK SETS
                    For i = 0 To lintMSize
                        ldblaExpBp(i) = ldblaExpBp(i) - ldblaExpBpT(i)
                        Debug.Print("..ExpBp(i) = " & ldblaExpBp(i).ToString)
                        Debug.Print("i;   k;  ..ExpBpBm(i, k)")
                        For k = 0 To lintMSize
                            ldblaExpBpBm(i, k) = ldblaExpBpBm(i, k) - ldblaExpBpBmT(i, k)
                            Debug.Print(i.ToString & " ; " & k.ToString & " ; " & ldblaExpBpBm(i, k).ToString)
                        Next k
                    Next i
                    ldblExp = ldblExp - ldblExpT
                    Debug.Print("ldblExp = " & ldblExp.ToString)
                    ldblExpT = 0
                    For i = 0 To lintMSize
                        ldblaZp(i) = 0
                        ldblaExpBpT(i) = 0
                        For k = 0 To lintMSize
                            ldblaExpBpBmT(i, k) = 0
                        Next k
                    Next i

                End If

                'If the this new data column, or if it will go in the partial likelihood equation
                If ldblaDataArray(j, 1) = 1 Then
                    ldblZ = ldblZ + ldblaExp(j) * lintWeight
                    For i = 0 To lintMSize
                        ldblaZp(i) = ldblaZp(i) + ldblaDataArray(j, i - 1 + lintOffset) * lintWeight
                    Next i
                    If (mintWeight <> 0) Then
                        lintTies = ldblaDataArray(j, 2)
                    Else
                        lintTies = 1
                    End If
                End If

                'Now, we can compute the new matrix values
                ldblTemp = System.Math.Exp(ldblaExp(j)) * lintWeight
                ldblExpT = ldblExpT + ldblTemp
                Debug.Print("..Temp;   ..ExpT")
                Debug.Print(ldblTemp.ToString & " ;  " & ldblExpT.ToString)
                Debug.Print("j; i; k; ..ExpBpT(i);  ..ExpBpBmT(i,k)")
                For i = 0 To lintMSize
                    ldblaExpBpT(i) = ldblaExpBpT(i) + ldblTemp * ldblaDataArray(j, i - 1 + lintOffset)
                    For k = 0 To lintMSize
                        ldblaExpBpBmT(i, k) = ldblaExpBpBmT(i, k) + ldblTemp * ldblaDataArray(j, i - 1 + lintOffset) * ldblaDataArray(j, k - 1 + lintOffset)
                        Debug.Print(j.ToString & " ;  " & i.ToString & " ;  " & k.ToString & " ;  " & ldblaExpBpT(i).ToString & " ;  " & ldblaExpBpBmT(i, k).ToString)
                    Next k
                Next i

            End If
        Next j

        If lintTies = 0 Then
            'No leftover likelihood to calculate
        Else
            'left over likelihood to calculate
            For i = 0 To lintMSize
                'Score, or First Partial Derivative
                Debug.Print("ldblaF(i) = ldblaF(i) + ldblaZp(i) - lintTies * ldblaExpBp(i) / ldblExp")
                Debug.Print("ldblaF(" & i & ") =     " & ldblaF(i) & "           " & ldblaZp(i) & "           " & lintTies & "           " & ldblaExpBp(i) & "           " & ldblExp)
                ldblaF(i) = ldblaF(i) + ldblaZp(i) - lintTies * ldblaExpBp(i) / ldblExp
                Debug.Print("j = " & j & "  " & "ldblaF(" & i & ") = " & ldblaF(i))
                For k = 0 To lintMSize
                    ' changed " - lintTies " to " + lintTies " by Eric Fontaine 08/05/03
                    ldblaJacobian(i, k) = ldblaJacobian(i, k) + lintTies * ((ldblaExpBpBm(i, k) * ldblExp - ldblaExpBp(k) * ldblaExpBp(i)) / (ldblExp * ldblExp))
                    Debug.Print("i ;  k ;  ..Jacobian(i,k)")
                    Debug.Print(i.ToString & " ;  " & k.ToString & " ;  ..Jacobian(i,k) = " & ldblaJacobian(i, k))
                Next k
            Next i
            BreslowLikelihood = BreslowLikelihood + ldblZ - lintTies * System.Math.Log(ldblExp)
            Debug.Print("Breslow = " & BreslowLikelihood)

        End If
        Exit Function
erroRHandler:
        If nRows = 0 Then
            Err.Raise(vbObjectError + 1, "Breslowlikelihood", "<tlt>No rows in data array.</tlt>")
        Else
            Err.Raise(vbObjectError + 1, "BreslowLikeLiHood", "<tlt>Error evaluating Breslow likelihood.</tlt>")
        End If
        Exit Function
    End Function

    'Lets pretend that there is an array that comes into the time dependent likelihood function
    'It will be of one dimensions.. The index is the time step identifies
    'The value is the end of the time step.
    'Active times, has the array of data of each covariate telling me waht time is currently active
    Public Function TimeDependentLikelihood(ByRef lintOffset As Integer, ByRef ldblaDataArray(,) As Double, ByRef ldblaJacobian(,) As Double, ByRef ldblaB() As Double, ByRef ldblaF() As Double, ByRef nRows As Integer, ByRef nCols As Integer, ByRef ldblaTime() As Double, ByRef lintaData() As Integer, ByRef lintaTimeFuncPos() As Integer) As Double

        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim l As Integer

        Dim lintMSize As Integer
        Dim ldblParamDotProduct As Double
        Dim ldblaExp(,) As Double
        Dim ldblaExpBp(,) As Double
        Dim ldblaExpBpBm(,,) As Double
        Dim ldblExp() As Double
        Dim ldblTieSum As Double
        Dim ldblExpT As Double
        Dim ldblaExpBpT() As Double
        Dim ldblaExpBpBmT(,) As Double

        'New variables to accomidate time dependencies
        Dim lintUniqueSteps As Integer
        Dim lintTimePos As Integer
        Dim ldblOldSTime As Double
        Dim ldblCurSTime As Double
        Dim ldblTemp As Double
        Dim ldblTDLikelihood As Double

        Dim ldblZ As Double
        Dim ldblaZp() As Double
        Dim lintTies As Integer
        Dim lintCurPos As Integer
        Dim ldblaTimeFunc() As Double
        Dim lintBase As Integer
        Dim lintWeight As Integer
        On Error GoTo erroRHandler
        'Initialize time dependent stuff
        lintUniqueSteps = UBound(ldblaTime)
        lintTimePos = 0
        lintMSize = UBound(ldblaB)

        Dim ldblaPreJacobian(lintMSize, lintMSize) As Object
        ReDim ldblaExp(nRows - 1, lintUniqueSteps)
        ReDim ldblaExpBp(lintMSize, lintUniqueSteps)
        ReDim ldblaExpBpBm(lintMSize, lintMSize, lintUniqueSteps)
        ReDim ldblaZp(lintMSize)

        Dim ldblaExpT(nRows - 1) As Double
        ReDim ldblaExpBpT(lintMSize)
        ReDim ldblaExpBpBmT(lintMSize, lintMSize)

        ReDim ldblExp(lintUniqueSteps)

        'The time sum has the sum computed at each time interval...
        Dim ldblaTimeSum(nRows - 1) As Double
        Dim ldblaTimeInt(nRows - 1) As Double

        ldblTDLikelihood = 0

        ReDim ldblaTimeFunc(lintMSize)
        'Get the exp(B*X) for every row, xp*exp(B*X(t)) , xp*xm*exp(B*X), and every time step...
        'Time saving solution.. loop through each time step..
        lintBase = 0
        For l = 0 To lintUniqueSteps
            'Get the Time Function Values
            For i = 0 To lintMSize
                If lintaTimeFuncPos(lintaData(i)) > 0 Then
                    ldblaTimeFunc(i) = ldblaDataArray(lintBase, lintaTimeFuncPos(lintaData(i)) + lintOffset - 1)
                Else
                    ldblaTimeFunc(i) = 1
                End If
            Next i

            For j = lintBase To nRows - 1
                lintWeight = 1
                If mintWeight <> 0 Then
                    lintWeight = ldblaDataArray(j, 2)
                End If

                'Only need to compute data for time set, if it is passed its time step..
                If l <= UBound(ldblaTime) - 1 Then
                    If ldblaTime(l) > ldblaDataArray(j, 0) Then
                        lintBase = j
                    End If
                End If
                For i = 0 To lintMSize
                    'If this time variable starts at the end of this time frame do not include it
                    'if this time variable has ended before this time frame, do not include it.
                    ldblaExp(j, 0) = ldblaExp(j, 0) + ldblaDataArray(j, lintaData(i) + lintOffset - 1) * ldblaB(i) * ldblaTimeFunc(i)
                Next i
                ldblTemp = System.Math.Exp((ldblaExp(j, 0)) * lintWeight)
                For i = 0 To lintMSize
                    'If the current variable has data within the time sum...
                    ldblaExpBp(i, 0) = ldblaExpBp(i, 0) + ldblTemp * ldblaDataArray(j, lintaData(i) + lintOffset - 1) * ldblaTimeFunc(i)
                    For k = 0 To lintMSize
                        'Must only include survival sums at the current time...
                        ldblaExpBpBm(i, k, l) = ldblaExpBpBm(i, k, l) + ldblTemp * ldblaDataArray(j, lintaData(i) + lintOffset - 1) * ldblaDataArray(j, lintaData(k) + lintOffset - 1) * ldblaTimeFunc(i) * ldblaTimeFunc(k)  'den4
                    Next k
                Next i
                ldblExp(l) = ldblExp(l) + ldblTemp

            Next j
            lintBase = lintBase + 1
        Next l
        ldblOldSTime = ldblaDataArray(0, 0)
        lintTies = 0
        'Get the Time Function Values
        For i = 0 To lintMSize
            If lintaTimeFuncPos(lintaData(i)) > 0 Then
                ldblaTimeFunc(i) = ldblaDataArray(0, lintaTimeFuncPos(lintaData(i)) + lintOffset - 1)
            Else
                ldblaTimeFunc(i) = 1
            End If
        Next i
        For j = 0 To nRows - 1
            lintWeight = 1
            If mintWeight <> 0 Then
                lintWeight = ldblaDataArray(j, 2)
            End If
            ldblCurSTime = ldblaDataArray(j, 0)
            If ldblOldSTime = ldblCurSTime Then
                'Get the data ready for the people we are removing from the risk sets.
                ldblTemp = System.Math.Exp((ldblaExp(j, lintTimePos)) * lintWeight)
                For i = 0 To lintMSize
                    ldblaExpBpT(i) = ldblaExpBpT(i) + ldblTemp * ldblaDataArray(j, lintaData(i) - 1 + lintOffset) * ldblaTimeFunc(i)
                    For k = 0 To lintMSize
                        ldblaExpBpBmT(i, k) = ldblaExpBpBmT(i, k) + ldblTemp * ldblaDataArray(j, lintaData(i) - 1 + lintOffset) * ldblaDataArray(j, lintaData(k) - 1 + lintOffset) * ldblaTimeFunc(i) * ldblaTimeFunc(k)
                    Next k
                Next i
                ldblExpT = ldblExpT + ldblTemp
                If ldblaDataArray(j, 1) = 1 Then
                    'Failed
                    ldblZ = ldblZ + ldblaExp(j, lintTimePos - 1) * lintWeight
                    For i = 0 To lintMSize
                        ldblaZp(i) = ldblaZp(i) + ldblaDataArray(j, lintaData(i) + lintOffset) * ldblaTimeFunc(i) * lintWeight  'den4: dropped the -1 from DataArray
                    Next i
                    lintTies = lintTies + lintWeight
                End If
            Else
                'The new S Time is different than the old one..
                'Set the new old S Time
                ldblOldSTime = ldblCurSTime
                'Fix the Matrices
                If lintTies = 0 Then
                    'If there are no ties, this means the last time we went through
                    'Thus, the risk set must be flushed of the Temporaries
                    'Remove The Individuals out of the risk set
                    'Maybe it is a new time column.
                    If lintTimePos < lintUniqueSteps Then
                        If (ldblaTime(lintTimePos) <= ldblCurSTime) Then
                            lintTimePos = lintTimePos + 1
                            ldblExpT = 0
                            For i = 0 To lintMSize
                                ldblaExpBpT(i) = 0
                                For k = 0 To lintMSize
                                    ldblaExpBpBmT(i, k) = 0
                                Next k
                            Next i
                            For i = 0 To lintMSize
                                If lintaTimeFuncPos(lintaData(i)) > 0 Then
                                    ldblaTimeFunc(i) = ldblaDataArray(j, lintaTimeFuncPos(lintaData(i)) + lintOffset - 1)
                                Else
                                    ldblaTimeFunc(i) = 1
                                End If
                            Next i
                        End If
                    End If
                    'Remove The Individuals out of the risk set
                    For i = 0 To lintMSize
                        ldblaExpBp(i, lintTimePos) = ldblaExpBp(i, lintTimePos) - ldblaExpBpT(i)
                        For k = 0 To lintMSize
                            ldblaExpBpBm(i, k, lintTimePos) = ldblaExpBpBm(i, k, lintTimePos) - ldblaExpBpBmT(i, k)
                        Next k
                    Next i
                    ldblExp(lintTimePos) = ldblExp(lintTimePos) - ldblExpT
                    ldblExpT = 0
                    ldblZ = 0
                    For i = 0 To lintMSize
                        ldblaZp(i) = 0
                        ldblaExpBpT(i) = 0
                        For k = 0 To lintMSize
                            ldblaExpBpBmT(i, k) = 0
                        Next k
                    Next i
                Else
                    For i = 0 To lintMSize
                        'Score, or First Partial Derivative
                        ldblaF(i) = ldblaF(i) + ldblaZp(i) - lintTies * ldblaExpBp(i, lintTimePos) / ldblExp(lintTimePos)
                        For k = 0 To lintMSize
                            'Covariance Matrix, or the Jacobian
                            ldblaJacobian(i, k) = ldblaJacobian(i, k) + lintTies * ((ldblaExpBpBm(i, k, lintTimePos) * ldblExp(lintTimePos) - ldblaExpBp(k, lintTimePos) * ldblaExpBp(i, lintTimePos)) / (ldblExp(lintTimePos) * ldblExp(lintTimePos)))
                        Next k
                    Next i

                    ldblTDLikelihood = ldblTDLikelihood + ldblZ - lintTies * System.Math.Log(ldblExp(lintTimePos))
                    lintTies = 0

                    'Remove The Individuals out of the risk set
                    For i = 0 To lintMSize
                        ldblaExpBp(i, lintTimePos) = ldblaExpBp(i, lintTimePos) - ldblaExpBpT(i)
                        For k = 1 To lintMSize
                            ldblaExpBpBm(i, k, lintTimePos) = ldblaExpBpBm(i, k, lintTimePos) - ldblaExpBpBmT(i, k)
                        Next k
                    Next i
                    ldblExp(lintTimePos) = ldblExp(lintTimePos) - ldblExpT

                    'Clear Variables
                    ldblExpT = 0
                    ldblZ = 0
                    For i = 0 To lintMSize
                        ldblaZp(i) = 0
                        ldblaExpBpT(i) = 0
                        For k = 0 To lintMSize
                            ldblaExpBpBmT(i, k) = 0
                        Next k
                    Next i

                End If

                'Maybe it is a new time column.
                If lintTimePos < lintUniqueSteps Then
                    If (ldblaTime(lintTimePos + 1) <= ldblCurSTime) Then
                        lintTimePos = lintTimePos + 1
                        ldblExpT = 0
                        For i = 0 To lintMSize
                            ldblaExpBpT(i) = 0
                            For k = 0 To lintMSize
                                ldblaExpBpBmT(i, k) = 0
                            Next k
                        Next i

                        For i = 0 To lintMSize
                            If lintaTimeFuncPos(lintaData(i)) > 0 Then
                                ldblaTimeFunc(i) = ldblaDataArray(j, lintaTimeFuncPos(lintaData(i)) + lintOffset - 1)
                            Else
                                ldblaTimeFunc(i) = 1
                            End If
                        Next i
                    End If
                End If

                'Now, we can compute the new matrix values
                ldblTemp = System.Math.Exp(ldblaExp(j, lintTimePos)) * lintWeight
                For i = 0 To lintMSize
                    ldblaExpBpT(i) = ldblaExpBpT(i) + ldblTemp * ldblaDataArray(j, lintaData(i) - 1 + lintOffset) * ldblaTimeFunc(i)
                    For k = 0 To lintMSize
                        ldblaExpBpBmT(i, k) = ldblaExpBpBmT(i, k) + ldblTemp * ldblaDataArray(j, lintaData(i) - 1 + lintOffset) * ldblaDataArray(j, lintaData(k) - 1 + lintOffset) * ldblaTimeFunc(i) * ldblaTimeFunc(k)
                    Next k
                Next i
                ldblExpT = ldblExpT + ldblTemp
                'If the this new data column, or if it will go in the partial likelihood equation
                If ldblaDataArray(j, 1) = 1 Then
                    ldblZ = ldblZ + ldblaExp(j, lintTimePos) * lintWeight
                    For i = 0 To lintMSize
                        ldblaZp(i) = ldblaZp(i) + ldblaDataArray(j, lintaData(i) + lintOffset) * ldblaTimeFunc(i) * lintWeight 'den4: dropped the -1 from DataArray()
                    Next i
                    lintTies = lintWeight

                End If
            End If
        Next j

        If lintTies = 0 Then
            'No leftover likelihood to calculate
        Else
            'left over likelihood to calculate
            For i = 0 To lintMSize
                'Score, or First Partial Derivative
                ldblaF(i) = ldblaF(i) + ldblaZp(i) - lintTies * ldblaExpBp(i, lintTimePos) / ldblExp(lintTimePos)
                For k = 0 To lintMSize
                    ldblaJacobian(i, k) = ldblaJacobian(i, k) - lintTies * ((ldblaExpBpBm(i, k, lintTimePos) * ldblExp(lintTimePos) - ldblaExpBp(k, lintTimePos) * ldblaExpBp(i, lintTimePos)) / (ldblExp(lintTimePos) * ldblExp(lintTimePos)))
                Next k
            Next i
            ldblTDLikelihood = ldblTDLikelihood + ldblZ - lintTies * System.Math.Log(ldblExp(lintTimePos))
        End If
        TimeDependentLikelihood = ldblTDLikelihood
        Debug.Print("B")
        For i = 0 To lintMSize
            System.Diagnostics.Debug.Write(VB6.TabLayout(ldblaB(i), TAB))
        Next i
        Debug.Print("")
        Debug.Print(VB6.TabLayout("Likelihood", TimeDependentLikelihood))
        Exit Function
erroRHandler:
        Err.Raise(vbObjectError + 2, , Err.Description)
    End Function
End Module