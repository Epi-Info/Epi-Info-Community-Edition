Option Strict Off
Option Explicit On
<System.Runtime.InteropServices.ProgId("EIMatrix_NET.EIMatrix")> Public Class EIMatrix

    Private mdblaJacobian(,) As Double
    Private mdblaInv(,) As Double
    Private mdblaB() As Double
    Private mdblaF() As Double
    Private mboolConverge As Boolean
    Private mboolErrorStatus As Boolean
    Private mstrerror As String

    Private mdblllfst As Double
    Private mdbllllast As Double
    Private mdblScore As Double
    Private mintIterations As Integer

    Public Event CalcLikelihood(ByRef lintOffset As Integer, ByRef ldblA As System.Array, ByRef ldblaB As System.Array, ByRef ldblaJacobian As System.Array, ByRef ldblaF As System.Array, ByRef nRows As Integer, ByRef likelihood As Double, ByRef strError As String, ByRef booStartAtZero As Boolean)

    'Get the inverse of matrix A. The code is converted from "numerical recipes in C".
    Public Function inv(ByRef a(,) As Double, ByRef invA(,) As Double) As Object
        Dim indx() As Integer
        Dim n As Integer
        Dim col() As Double
        Dim d As Double
        Dim i, j As Integer
        Dim k As Integer

        n = UBound(a, 1)
        ReDim indx(n + 2)
        ReDim col(n + 2)
        ludcmp(a, n, indx, d)

        For j = 0 To n
            For i = 0 To n
                col(i) = 0
            Next
            col(j) = 1

            Dim indxShifted(indx.Length) As Integer
            Dim colShifted(col.Length) As Double

            For k = 0 To indx.Length - 1
                indxShifted(k + 1) = indx(k)
            Next

            For k = 0 To col.Length - 1
                colShifted(k + 1) = col(k)
            Next

            lubksb(a, n, indxShifted, colShifted)
            For i = 0 To n
                invA(i, j) = colShifted(i + 1)
            Next
        Next

    End Function


    ' LU decomposition
    Public Function ludcmp(ByRef a(,) As Double, ByVal n As Integer, ByRef indx() As Integer, ByRef d As Double) As Integer

        Const TINY As Double = 1.0E-20

        Dim j, i, imax, k As Integer
        Dim dum, big, sum As Double
        Dim vv() As Double

        'UPGRADE_WARNING: Lower bound of array vv was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim vv(n + 1)

        d = 1.0#
        For i = 0 To n
            big = 0.0#
            For j = 0 To n
                If System.Math.Abs(a(i, j)) > big Then
                    big = System.Math.Abs(a(i, j))
                End If
            Next
            If (big = 0.0#) Then
                ludcmp = -1
                Exit Function
            End If

            vv(i) = 1.0# / big
        Next
        For j = 0 To n
            For i = 0 To j - 1
                sum = a(i, j)
                For k = 0 To i - 1
                    sum = sum - a(i, k) * a(k, j)
                Next
                a(i, j) = sum
            Next
            big = 0.0#
            For i = j To n
                sum = a(i, j)
                For k = 0 To j - 1
                    sum = sum - a(i, k) * a(k, j)
                Next
                a(i, j) = sum
                dum = vv(i) * System.Math.Abs(sum)
                If dum >= big Then
                    big = dum
                    imax = i
                End If
            Next
            If j <> imax Then
                For k = 0 To n
                    dum = a(imax, k)
                    a(imax, k) = a(j, k)
                    a(j, k) = dum
                Next
                d = -d
                vv(imax) = vv(j)
            End If
            indx(j) = imax + 1
            If (a(j, j) = 0.0#) Then a(j, j) = TINY
            If (j <> n) Then
                dum = 1.0# / a(j, j)
                For i = j + 1 To n
                    a(i, j) = a(i, j) * dum
                Next
            End If
        Next

        'Original VB6 Code ==================
        'ReDim vv(n)

        'd = 1.0#
        'For i = 1 To n
        '    big = 0.0#
        '    For j = 1 To n
        '        If System.Math.Abs(a(i, j)) > big Then
        '            big = System.Math.Abs(a(i, j))
        '        End If
        '    Next
        '    If (big = 0.0#) Then
        '        ludcmp = -1
        '        Exit Function
        '    End If

        '    vv(i) = 1.0# / big
        'Next
        'For j = 1 To n
        '    For i = 1 To j - 1
        '        sum = a(i, j)
        '        For k = 1 To i - 1
        '            sum = sum - a(i, k) * a(k, j)
        '        Next
        '        a(i, j) = sum
        '    Next
        '    big = 0.0#
        '    For i = j To n
        '        sum = a(i, j)
        '        For k = 1 To j - 1
        '            sum = sum - a(i, k) * a(k, j)
        '        Next
        '        a(i, j) = sum
        '        dum = vv(i) * System.Math.Abs(sum)
        '        If dum >= big Then
        '            big = dum
        '            imax = i
        '        End If
        '    Next
        '    If j <> imax Then
        '        For k = 1 To n
        '            dum = a(imax, k)
        '            a(imax, k) = a(j, k)
        '            a(j, k) = dum
        '        Next
        '        d = -d
        '        vv(imax) = vv(j)
        '    End If
        '    indx(j) = imax
        '    If (a(j, j) = 0.0#) Then a(j, j) = TINY
        '    If (j <> n) Then
        '        dum = 1.0# / a(j, j)
        '        For i = j + 1 To n
        '            a(i, j) = a(i, j) * dum
        '        Next
        '    End If
        'Next



    End Function
    'Inv() calls lubksb()
    'Solve a set of linear equations
    Public Sub lubksb(ByRef a(,) As Double, ByVal n As Integer, ByRef indx() As Integer, ByRef B() As Double)
        Dim ip, i, ii, j As Integer
        Dim sum As Double

        ii = 0
        '1 to 0
        For i = 0 To n
            ip = indx(i + 1)
            sum = B(ip)
            B(ip) = B(i + 1)
            If ii > 0 Then
                For j = ii To i
                    sum = sum - a(i, j - 1) * B(j)
                Next
            ElseIf sum <> 0 Then
                ii = i + 1
            End If
            B(i + 1) = sum
        Next
        '1 to 0
        For i = n To 0 Step -1
            sum = B(i + 1)
            For j = i + 1 To n
                sum = sum - a(i, j) * B(j + 1)
            Next
            B(i + 1) = sum / a(i, i)
        Next

        'Original  VB6 Code ===========
        'ii = 0
        'For i = 1 To n
        '    ip = indx(i)
        '    sum = B(ip)
        '    B(ip) = B(i)
        '    If ii > 0 Then
        '        For j = ii To i - 1
        '            sum = sum - a(i, j) * B(j)
        '        Next
        '    ElseIf sum <> 0 Then
        '        ii = i
        '    End If
        '    B(i) = sum
        'Next
        'For i = n To 1 Step -1
        '    sum = B(i)
        '    For j = i + 1 To n
        '        sum = sum - a(i, j) * B(j)
        '    Next
        '    B(i) = sum / a(i, i)
        'Next




    End Sub

    ''C=A * B
    Public Function mul(ByRef A(,) As Double, ByRef B(,) As Double, ByRef C(,) As Double) As Integer

        Dim j, i, k As Integer
        Dim rowB, rowA, colA, colB As Integer

        rowA = UBound(A, 1)
        colA = UBound(A, 2)
        rowB = UBound(B, 1)
        colB = UBound(B, 2)
        If colA <> rowB Then
            mul = -1
            Exit Function
        Else
            For i = 0 To rowA
                For k = 0 To colB
                    C(i, k) = 0
                    For j = 0 To colA
                        C(i, k) = C(i, k) + A(i, j) * B(j, k)
                    Next
                Next
            Next
        End If

        'Original VB6 code below

        'rowA = UBound(a, 1)
        'colA = UBound(a, 2)
        'rowB = UBound(B, 1)
        'colB = UBound(B, 2)
        'If colA <> rowB Then
        '    mul = -1
        '    Exit Function
        'Else
        '    For i = 1 To rowA
        '        For k = 1 To colB
        '            C(i, k) = 0
        '            For j = 1 To colA
        '                C(i, k) = C(i, k) + a(i, j) * B(j, k)
        '            Next
        '        Next
        '    Next
        'End If

    End Function
    'B=A'
    Public Sub trans(ByRef a(,) As Double, ByRef B(,) As Double)
        Dim i, j As Integer
        Dim rowA As Integer
        Dim colA As Integer

        rowA = UBound(a, 1)
        colA = UBound(a, 2)
        For i = 0 To rowA
            For j = 0 To colA
                B(j, i) = a(i, j)
            Next
        Next
    End Sub

    Public Sub MaximizeLikelihood(ByRef nRows As Integer, ByRef nCols As Integer, ByRef dataArray(,) As Double, ByRef lintOffset As Integer, ByRef lintMatrixSize As Integer, ByRef llngIters As Integer, ByRef ldblToler As Double, ByRef ldblConv As Object, ByRef booStartAtZero As Boolean)
        Dim ldbll As Double
        Dim ldbllfst As Double
        Dim ldbloldll As Double
        Dim ldblDet As Double
        Dim i As Integer
        Dim k As Integer
        Dim iterations As Integer
        Dim lbconditional As Boolean
        Dim ldblaInv() As Double
        Dim lintWeight As Integer

        ' added by Eric Fontaine 07/28/03: Used to retrieve error message from CalcLikelihood
        Dim strCalcLikelihoodError As String

        Dim ldblaScore() As Double
        'UPGRADE_WARNING: Lower bound of array ldblaScore was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim ldblaScore(lintMatrixSize - 1)
        mdblScore = 0
        Dim oldmdblScore As Double
        oldmdblScore = CDbl(0)
        mboolConverge = True

        On Error GoTo ERROR_Renamed
        mboolErrorStatus = False
        'Set Array sizes
        'UPGRADE_WARNING: Lower bound of array mdblaJacobian was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mdblaJacobian(lintMatrixSize - 1, lintMatrixSize - 1)
        Dim oldmdblaJacobian(,) As Double
        ReDim oldmdblaJacobian(lintMatrixSize - 1, lintMatrixSize - 1)
        'UPGRADE_WARNING: Lower bound of array mdblaInv was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mdblaInv(lintMatrixSize - 1, lintMatrixSize - 1)
        'UPGRADE_WARNING: Lower bound of array mdblaF was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mdblaF(lintMatrixSize - 1)
        Dim oldmdblaF() As Double
        ReDim oldmdblaF(lintMatrixSize - 1)
        'UPGRADE_WARNING: Lower bound of array mdblaB was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mdblaB(lintMatrixSize - 1)
        'UPGRADE_WARNING: Lower bound of array ldblaScore was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim ldblaScore(lintMatrixSize - 1)

        'Calculate the likelihood
        RaiseEvent CalcLikelihood(lintOffset, dataArray, mdblaB, mdblaJacobian, mdblaF, nRows, ldbllfst, strCalcLikelihoodError, booStartAtZero)

        For i = 0 To UBound(mdblaB)
            For j = 0 To UBound(mdblaB)
                oldmdblaJacobian(i, j) = mdblaJacobian(i, j)
            Next j
            oldmdblaF(i) = mdblaF(i)
        Next i

        ' added by Eric Fontaine 07/28/03: raises an error if CalcLikelihood had an error
        If strCalcLikelihoodError <> "" Then
            Err.Raise(vbObjectError, , strCalcLikelihoodError)
        End If

        'WE have the first likelihood estiamate
        mintIterations = 1
        ldbloldll = ldbllfst
        ldbll = ldbllfst
        If ldbllfst > 0 Then Err.Raise(vbObjectError, , "<tlt>Positive Log-Likelihood, regression is diverging</tlt>")
        inv(mdblaJacobian, mdblaInv)
        Dim oldmdblaInv(,) As Double
        Dim oldmdblaB() As Double
        ReDim oldmdblaInv(lintMatrixSize - 1, lintMatrixSize - 1)
        ReDim oldmdblaB(lintMatrixSize - 1)
        For i = 0 To UBound(mdblaB)
            For j = 0 To UBound(mdblaB)
                oldmdblaInv(i, j) = mdblaInv(i, j)
            Next j
            oldmdblaB(i) = mdblaB(i)
        Next i
        'find the determinant
        'The matrix has already been lu decomposed.
        ldblDet = 1
        For i = 0 To UBound(mdblaB)
            ldblDet = ldblDet * mdblaJacobian(i, i)
        Next i

        If System.Math.Abs(ldblDet) < ldblToler Then
            mboolConverge = False
            Err.Raise(vbObjectError, , "<tlt>Matrix Tolerance Exceeded</tlt>")
            GoTo EndProc
        End If

        'Now find the delta coefficients for this iteration
        'And clear the arrays at the same time
        For i = 0 To UBound(mdblaB)
            For k = 0 To UBound(mdblaB)
                ldblaScore(i) = ldblaScore(i) + mdblaF(k) * mdblaInv(i, k)
                mdblaJacobian(i, k) = 0
            Next
            mdblaB(i) = mdblaB(i) + ldblaScore(i)
            mdblScore = mdblScore + ldblaScore(i) * mdblaF(i)
        Next

        Dim Ridge As Double
        Ridge = CDbl(0)

        For mintIterations = 2 To llngIters
            'clear f
            For i = 0 To UBound(mdblaF)
                mdblaF(i) = 0
            Next i
            'do conditional or unconditional
            RaiseEvent CalcLikelihood(lintOffset, dataArray, mdblaB, mdblaJacobian, mdblaF, nRows, ldbll, strCalcLikelihoodError, booStartAtZero)

            'test for exit time
            If ldbloldll - ldbll > ldblConv Then
                If Ridge > 0.0 And Ridge < 1000 Then
                    mintIterations = mintIterations - 1
                    Ridge = Ridge * 4.0
                    For i = 0 To UBound(mdblaB)
                        For j = 0 To UBound(mdblaB)
                            mdblaJacobian(i, j) = oldmdblaJacobian(i, j) * (1 + Int(i = j) * Ridge)
                        Next j
                        mdblaB(i) = oldmdblaB(i)
                        mdblaF(i) = oldmdblaF(i)
                    Next i
                    GoTo ContinuePoint
                End If
                If Ridge = 0.0 Then
                    mintIterations = mintIterations - 1
                    Ridge = 0.0001
                    For i = 0 To UBound(mdblaB)
                        For j = 0 To UBound(mdblaB)
                            mdblaJacobian(i, j) = oldmdblaJacobian(i, j) * (1 + Int(i = j) * Ridge)
                        Next j
                        mdblaB(i) = oldmdblaB(i)
                        mdblaF(i) = oldmdblaF(i)
                    Next i
                    GoTo ContinuePoint
                End If

                mboolConverge = False
                mdblllfst = ldbllfst

                Err.Raise(vbObjectError, , "<tlt>Regression not converging</tlt>")
                'UPGRADE_WARNING: Couldn't resolve default property of object ldblConv. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ElseIf ldbll - ldbloldll < ldblConv Then
                mdblaB = oldmdblaB
                '                mdblaInv = oldmdblaInv
                mintIterations = mintIterations - 1
                GoTo EndProc
            End If

            For i = 0 To UBound(mdblaB)
                For j = 0 To UBound(mdblaB)
                    oldmdblaInv(i, j) = mdblaInv(i, j)
                    oldmdblaJacobian(i, j) = mdblaJacobian(i, j)
                Next j
                oldmdblaB(i) = mdblaB(i)
                oldmdblaF(i) = mdblaF(i)
            Next i
            Ridge = 0.0
            ldbloldll = ldbll

ContinuePoint:
            inv(mdblaJacobian, mdblaInv)
            'find the determinant
            'The matrix has already been ludecomposed.
            ldblDet = 1
            For i = 0 To UBound(mdblaB)
                ldblDet = ldblDet * mdblaJacobian(i, i)
            Next i

            If System.Math.Abs(ldblDet) < ldblToler Then
                mboolConverge = False
                Err.Raise(vbObjectError, , "<tlt>Matrix Tolerance Exceeded</tlt>")
                GoTo EndProc
            End If

            'Now find the delta coefficients for this iteration
            'And clear the arrays at the same time
            For i = 0 To UBound(mdblaB)
                For k = 0 To UBound(mdblaB)
                    mdblaB(i) = mdblaB(i) + mdblaF(k) * mdblaInv(i, k)
                    mdblaJacobian(i, k) = 0
                Next

            Next


        Next
EndProc:

        mdblllfst = ldbllfst
        mdbllllast = ldbll
        Exit Sub

ERROR_Renamed:
        mboolErrorStatus = True
        mstrerror = Err.Description
        '    Err.Raise vbObjectError, , Err.Description        

    End Sub

    Public Function GetCoefficients() As Double()
        GetCoefficients = VB6.CopyArray(mdblaB)
    End Function
    Public Function GetInverseMatrix() As Double(,)
        GetInverseMatrix = VB6.CopyArray(mdblaInv)
    End Function
    Public Function GetConvergence() As Boolean
        GetConvergence = mboolConverge
    End Function

    Public Function getError(ByRef lstrError As String) As Boolean
        If mboolErrorStatus Then
            lstrError = mstrerror
        End If
        getError = mboolErrorStatus

    End Function

    Public Function getFirstLikelihood() As Double
        getFirstLikelihood = mdblllfst
    End Function
    Public Function getLastLikelihood() As Double
        getLastLikelihood = mdbllllast
    End Function
    Public Function getIters() As Integer
        getIters = mintIterations
    End Function
    Public Function getScore() As Double
        getScore = mdblScore
    End Function
End Class