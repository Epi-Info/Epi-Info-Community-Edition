Option Strict Off
Option Explicit On
Option Compare Text
<System.Runtime.InteropServices.ProgId("EILSFit_NET.EILSFit")> Public Class LinearRegression
    Implements EpiInfo.Plugin.IAnalysisStatistic
    'EIREGRES
    'Does unconditional and conditional logistic regresion
    'sources-> Breslow and Day volumne 1
    'APplied logistic regression, hosmer lemeshow
    'kleinbaum-> logistic regression


    Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)

    Private Const ErrStart As Integer = &H3000
    Private Const conConnStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
    Private context As EpiInfo.Plugin.IAnalysisStatisticContext
    Private currentTable As DataTable

    'Private WithEvents mMatrixlikelihood As EIMatrixLib.EIMatrix
    
    Public Structure VariableRow
        Public variableName As String
        Public coefficient As Double
        Public stdError As Double
        Public Ftest As Double
        Public P As Double
    End Structure

    Public Structure LinearRegressionResults
        Public variables As List(Of VariableRow)
        Public correlationCoefficient As Double
        Public pearsonCoefficient As Double
        Public pearsonCoefficientT As Double
        Public pearsonCoefficientTP As Double
        Public spearmanCoefficient As Double
        Public spearmanCoefficientT As Double
        Public spearmanCoefficientTP As Double
        Public regressionDf As Integer
        Public regressionSumOfSquares As Double
        Public regressionMeanSquare As Double
        Public regressionF As Double
        Public residualsDf As Integer
        Public residualsSumOfSquares As Double
        Public residualsMeanSquare As Double
        Public totalDf As Integer
        Public totalSumOfSquares As Double
        Public errorMessage As String
    End Structure

    Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
    End Sub

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
            ReDim Results(1, 0)
            Results(0, 0) = "ERROR"
            Results(1, 0) = errorMessage
            output = "<br clear=""all"" /><p align=""left""><b><tlt>" + errorMessage + "</tlt></b></p>"

            args.Add("COMMANDNAME", "REGRESS")
            args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
            args.Add("HTMLRESULTS", output)
            context.Display(args)
            Exit Sub
        End If

        Dim y(,) As Double
        Dim x(,) As Double
        Dim xx(,) As Double
        Dim invxx(,) As Double
        Dim xy(,) As Double
        Dim tx(,) As Double
        Dim B(,) As Double
        Dim yhat(,) As Double
        Dim resid() As Double
        Dim j, i, df As Integer
        Dim sse, mse As Double
        Dim rmse As Double
        Dim indx() As Integer
        Dim d As Double
        Dim fvalue() As Double
        Dim covb(,) As Double
        Dim probf() As Double
        Dim stdb() As Double
        Dim coeff(,) As Object
        Dim meanY As Double
        Dim ra2 As Double
        Dim r2 As Double
        Dim ssy As Double
        Dim ftest As Double
        Dim lintWRows As Integer
        Dim p As Integer
        Dim k As Integer

        On Error GoTo ERROR_PROC
        Dim lintweight As Integer
        Dim ldblweight As Boolean
        Dim lintrowCount As Integer
        Dim ldblMagic As Double
        If Len(mstrWeightVar) > 0 Then
            lintweight = 1
        Else
            lintweight = 0
        End If

        For i = 0 To NumRows - 1
            If lintweight = 1 Then
                lintWRows = lintWRows + 1
                lintrowCount = lintrowCount + DataArray(i, 1)
            Else
                lintWRows = lintWRows + 1
                lintrowCount = lintWRows
            End If
        Next i

        ReDim fvalue(NumColumns - 2 - lintweight)
        ReDim covb(NumColumns - 2 - lintweight, NumColumns - 2 - lintweight)
        ReDim probf(NumColumns - 2 - lintweight)
        ReDim stdb(NumColumns - 2 - lintweight)
        ReDim y(lintWRows - 1, 0)
        ReDim x(lintWRows - 1, NumColumns - 2 - lintweight)
        ReDim xx(NumColumns - 2 - lintweight, NumColumns - 2 - lintweight)
        ReDim invxx(NumColumns - 2 - lintweight, NumColumns - 2 - lintweight)
        ReDim xy(NumColumns - 2 - lintweight, 0)
        ReDim tx(NumColumns - 2 - lintweight, lintWRows - 1)
        ReDim B(NumColumns - 2 - lintweight, 0)
        ReDim yhat(lintWRows - 1, 0)
        ReDim resid(lintWRows - 1)

        k = 0
        For i = 0 To NumRows - 1

            If lintweight = 1 Then
                y(k, 0) = DataArray(i, 0) * (DataArray(i, 1)) ^ 0.5
                For j = 1 + lintweight To NumColumns - 1
                    x(k, j - 1 - lintweight) = DataArray(i, j) * (DataArray(i, 1)) ^ 0.5
                Next j
                k = k + 1
            Else
                y(k, 0) = DataArray(i, 0)
                For j = 0 + lintweight To NumColumns - 2
                    x(k, j - lintweight) = DataArray(i, j + 1)

                Next j
                k = k + 1
            End If
        Next

        Matrix1.trans(x, tx)
        Matrix1.mul(tx, x, xx)
        Matrix1.mul(tx, y, xy)

        invxx = VB6.CopyArray(xx)
        ReDim indx(UBound(invxx, 1))
        Matrix1.ludcmp(invxx, UBound(invxx, 1), indx, d)
        d = 1
        For i = 0 To UBound(invxx, 1)

            d = invxx(i, i) * d
            If d = 0 Then
                ReDim Results(1, 0)
                Results(0, 0) = "ERROR"
                Results(1, 0) = "Colinear Data"
                output = "<br clear=""all"" /><p align=""left""><b><tlt>Colinear Data</tlt></b></p>"

                args.Add("COMMANDNAME", "REGRESS")
                args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
                args.Add("HTMLRESULTS", output)
                context.Display(args)

                Exit Sub
            End If
        Next i
        If System.Math.Abs(d) < mdblToler Then
            ReDim Results(1, 0)
            Results(0, 0) = "ERROR"
            Results(1, 0) = "Matrix Tolerance Exceeded"
            output = "<br clear=""all"" /><p align=""left""><b><tlt>Matrix Tolerance Exceeded</tlt></b></p>"

            args.Add("COMMANDNAME", "REGRESS")
            args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
            args.Add("HTMLRESULTS", output)
            context.Display(args)

            Exit Sub
        End If
        Matrix1.inv(xx, invxx)
        Matrix1.mul(invxx, xy, B)
        Matrix1.mul(x, B, yhat)
        sse = 0
        meanY = 0
        For i = 0 To lintWRows - 1
            If lintweight > 0 Then
                ldblMagic = DataArray(i, 1) ^ 0.5
                resid(i) = (y(i, 0) - yhat(i, 0))
                meanY = meanY + y(i, 0) * ldblMagic
                sse = sse + resid(i) ^ 2
            Else
                resid(i) = y(i, 0) - yhat(i, 0)
                meanY = meanY + y(i, 0)
                sse = sse + resid(i) ^ 2
            End If

        Next
        meanY = meanY / lintrowCount ' mean of Y
        ssy = 0
        For i = 0 To lintWRows - 1
            If lintweight > 0 Then
                If DataArray(i, 1) <> 0 Then
                    ssy = ssy + ((y(i, 0) * DataArray(i, 1) ^ -0.5 - meanY)) ^ 2 * DataArray(i, 1)
                End If
            Else

                ssy = ssy + (y(i, 0) - meanY * System.Math.Abs(CInt(mboolIntercept))) ^ 2
            End If

        Next
        r2 = (ssy - sse) / ssy

        df = lintrowCount - (NumColumns - lintweight - CShort(mboolIntercept) * 0 - 1) ' degrees of squared errors
        ra2 = 1 - CInt(lintrowCount + CInt(mboolIntercept)) * sse / (df * ssy)
        mse = sse / df ' mean squared error

        Dim dist As New statlib
        dist = New statlib()

        ftest = (ssy - sse) / CShort(NumColumns - lintweight - 1 + CShort(mboolIntercept))
        ftest = ftest / mse 'F value
        rmse = System.Math.Sqrt(mse) ' root mean squared error
        For i = 0 To NumColumns - lintweight - 2
            For j = 0 To NumColumns - lintweight - 2
                covb(i, j) = invxx(i, j) * mse
            Next
            stdb(i) = System.Math.Sqrt(System.Math.Abs(covb(i, i))) ' standard errors
            fvalue(i) = (B(i, 0) / stdb(i)) ^ 2
            'probf(i) = dist.PfromT(Math.Sqrt(fvalue(i)), df) / 2
            probf(i) = 0.0#
            On Error Resume Next
            probf(i) = dist.PfromF(fvalue(i), 1, df)
            On Error GoTo ERROR_PROC
        Next
        output = "<br clear=""all""><p align=""left""><b><tlt>Linear Regression</tlt></b></p>"
        output1 = String.Empty
        output2 = "<br clear=""all""><table align=""left"" cellspacing=""8""><tr><td class=""stats"" align=""left""><b><tlt>Correlation Coefficient: r^2=</tlt></b></td><td class=""stats"" align=""right"">" & VB6.Format(r2, "0.00") & "</td></table>"

        output2 = output2 & vbCrLf & "<br clear=""all""><br clear=""all"" /><table align=""left"" cellspacing=""8"">" & "<tr>" & "<td class=""stats""><b><tlt>Source</tlt></b> </td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>df</tlt></b> </td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>Sum of Squares</tlt></b> </td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>Mean Square</tlt></b> </td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>F-statistic</tlt></b> </td> </tr>" & "<tr>" & "<td class=""stats"" align=""left""><b><tlt>Regression</tlt></b> </td>" & "<td class=""stats"" ALIGN=RIGHT>" & CInt(lintrowCount + CInt(mboolIntercept)) - df & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(ssy - sse, "0.000") & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format((ssy - sse) / CShort(NumColumns - lintweight - 1 + CShort(mboolIntercept)), "0.000") & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(ftest, "0.000") & "</td></tr>" & "<tr>" & "<td class=""stats"" align=""left""><b><tlt>Residuals</tlt></b> </td>" & "<td class=""stats"" ALIGN=RIGHT>" & df & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(sse, "0.000") & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(mse, "0.000") & "</td>" & "<TD class=""stats"">&nbsp;</td></tr>" & "<tr>" & "<td class=""stats"" align=""left""><b><tlt>Total</tlt></b></td>" & "<td class=""stats"" ALIGN=RIGHT>" & lintrowCount + CShort(mboolIntercept) & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(ssy, "0.000") & "</td>" & "<TD class=""stats"">&nbsp;</td>" & "<TD class=""stats"">&nbsp;</td></tr></TABLE><BR CLEAR=ALL>"
        output1 = output1 & vbCrLf & "<br clear=""all""><table align=""left"" cellspacing=""8"">" & "<tr><td class=""stats""><b><tlt>Variable</tlt></b></td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>Coefficient</tlt></b></td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>Std Error</tlt></b></td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>F-test</tlt></b></td>" & "<td class=""stats"" ALIGN=RIGHT><b><tlt>P-Value</tlt></b></td></tr>"



        ReDim Results(1, 10)
        Results(0, 0) = "Correlation Coefficient: r^2"
        Results(1, 0) = r2

        Results(0, 1) = "Regression df"
        Results(1, 1) = CInt(lintrowCount + CShort(mboolIntercept)) - df

        Results(0, 2) = "Sum of Squares"
        Results(1, 2) = ssy - sse

        Results(0, 3) = "Mean Square"
        Results(1, 3) = (ssy - sse) / CShort(NumColumns - lintweight - 1 + CShort(mboolIntercept))

        Results(0, 4) = "F-statistic"
        Results(1, 4) = ftest

        Results(0, 5) = "Residuals df"
        Results(1, 5) = df

        Results(0, 6) = "Residuals Sum of Squares"
        Results(1, 6) = sse

        Results(0, 7) = "Residuals Mean Square"
        Results(1, 7) = mse

        Results(0, 8) = "Total df"
        Results(1, 8) = lintrowCount + CShort(mboolIntercept)

        Results(0, 9) = "Total Sum of Squares"
        Results(1, 9) = ssy


        ReDim coeff(4, NumColumns - 2 - lintweight)

        For i = 0 To NumColumns - 1 - lintweight - 1
            output1 = output1 & "<tr><td class=""stats"" align=""left""><b>" & mStrAMatrixLabels(i) & "</b></td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(B(i, 0), "0.000") & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(stdb(i), "0.000") & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(fvalue(i), "0.0000") & "</td>" & "<td class=""stats"" ALIGN=RIGHT>" & VB6.Format(probf(i), "0.000000") & "</td></tr>"
            coeff(0, i) = mStrAMatrixLabels(i)
            coeff(1, i) = B(i, 0)
            coeff(2, i) = stdb(i)
            coeff(3, i) = fvalue(i)
            coeff(4, i) = probf(i)
        Next

        Results(0, 10) = "Variable Coefficients"

        Results(1, 10) = VB6.CopyArray(coeff)

        output1 = output1 & "</TABLE><BR CLEAR=ALL>"
        output = output & output1 & output2

        args.Add("COMMANDNAME", "REGRESS")

        args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
        args.Add("HTMLRESULTS", output)

        context.Display(args)

        'Residuals2(resid) ' TODO: Re-enable later

        Exit Sub
ERROR_PROC:
        ReDim Results(1, 0)

        Results(0, 0) = "ERROR"

        Results(1, 0) = Err.Description

        Exit Sub
        Resume
    End Sub

    Private Sub CreateSettings(ByVal inputVariableList As Dictionary(Of String, String))

        Dim j As Integer
        '   Set default values

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
        mstraBoolean(1) = "False"
        mstraBoolean(2) = "True"
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
                If type.Equals("System.Byte") Then
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

            If kvp.Key.ToLower().Equals("P") Then
                Dim success As Boolean
                success = Double.TryParse(kvp.Value.ToLower(), mdblP)
                If success = True Then
                    mdblC = 1 - mdblP
                    mstrC = Str(mdblP * 100)
                    mdblP = 0 'dist1.ZFROMP((1 - mdblP) * 0.5)
                End If
            End If
        Next
    End Sub

    Public Function LinearRegression(ByVal inputVariableList As Dictionary(Of String, String), ByVal dataTable As DataTable) As LinearRegressionResults

        currentTable = dataTable

        CreateSettings(inputVariableList)

        Dim errorMessage As String
        errorMessage = String.Empty

        Dim regressionResults As New LinearRegressionResults
        regressionResults.errorMessage = String.Empty

        Dim args As Dictionary(Of String, String)
        args = New Dictionary(Of String, String)

        If GetRawData(errorMessage) = False Then
            regressionResults.errorMessage = errorMessage
            Return regressionResults
        End If

        Dim y(,) As Double
        Dim x(,) As Double
        Dim xx(,) As Double
        Dim invxx(,) As Double
        Dim xy(,) As Double
        Dim tx(,) As Double
        Dim B(,) As Double
        Dim yhat(,) As Double
        Dim resid() As Double
        Dim j, i, df As Integer
        Dim sse, mse As Double
        Dim rmse As Double
        Dim indx() As Integer
        Dim d As Double
        Dim fvalue() As Double
        Dim covb(,) As Double
        Dim probf() As Double
        Dim stdb() As Double
        Dim coeff(,) As Object
        Dim meanY As Double
        Dim ra2 As Double
        Dim r2 As Double
        Dim ssy As Double
        Dim ftest As Double
        Dim lintWRows As Integer
        Dim p As Integer
        Dim k As Integer

        On Error GoTo ERROR_PROC
        Dim lintweight As Integer
        Dim ldblweight As Boolean
        Dim lintrowCount As Integer
        Dim ldblMagic As Double
        If Len(mstrWeightVar) > 0 Then
            lintweight = 1
        Else
            lintweight = 0
        End If

        For i = 0 To NumRows - 1
            If lintweight = 1 Then
                lintWRows = lintWRows + 1
                lintrowCount = lintrowCount + DataArray(i, 1)
            Else
                lintWRows = lintWRows + 1
                lintrowCount = lintWRows
            End If
        Next i

        ReDim fvalue(NumColumns - 2 - lintweight)
        ReDim covb(NumColumns - 2 - lintweight, NumColumns - 2 - lintweight)
        ReDim probf(NumColumns - 2 - lintweight)
        ReDim stdb(NumColumns - 2 - lintweight)
        ReDim y(lintWRows - 1, 0)
        ReDim x(lintWRows - 1, NumColumns - 2 - lintweight)
        ReDim xx(NumColumns - 2 - lintweight, NumColumns - 2 - lintweight)
        ReDim invxx(NumColumns - 2 - lintweight, NumColumns - 2 - lintweight)
        ReDim xy(NumColumns - 2 - lintweight, 0)
        ReDim tx(NumColumns - 2 - lintweight, lintWRows - 1)
        ReDim B(NumColumns - 2 - lintweight, 0)
        ReDim yhat(lintWRows - 1, 0)
        ReDim resid(lintWRows - 1)

        k = 0
        For i = 0 To NumRows - 1

            If lintweight = 1 Then
                y(k, 0) = DataArray(i, 0) * (DataArray(i, 1)) ^ 0.5
                For j = 1 + lintweight To NumColumns - 1
                    x(k, j - 1 - lintweight) = DataArray(i, j) * (DataArray(i, 1)) ^ 0.5
                Next j
                k = k + 1
            Else
                y(k, 0) = DataArray(i, 0)
                For j = 0 + lintweight To NumColumns - 2
                    x(k, j - lintweight) = DataArray(i, j + 1)

                Next j
                k = k + 1
            End If
        Next

        Matrix1.trans(x, tx)
        Matrix1.mul(tx, x, xx)
        Matrix1.mul(tx, y, xy)

        invxx = VB6.CopyArray(xx)
        ReDim indx(UBound(invxx, 1))
        Matrix1.ludcmp(invxx, UBound(invxx, 1), indx, d)
        d = 1
        For i = 0 To UBound(invxx, 1)

            d = invxx(i, i) * d
            If d = 0 Then
                regressionResults.errorMessage = "Error: Colinear Data"
                Return regressionResults
            End If
        Next i
        If System.Math.Abs(d) < mdblToler Then
            regressionResults.errorMessage = "Error: Matrix Tolerance Exceeded"
            Return regressionResults
        End If
        Matrix1.inv(xx, invxx)
        Matrix1.mul(invxx, xy, B)
        Matrix1.mul(x, B, yhat)
        sse = 0
        meanY = 0
        For i = 0 To lintWRows - 1
            If lintweight > 0 Then
                ldblMagic = DataArray(i, 1) ^ 0.5
                resid(i) = (y(i, 0) - yhat(i, 0))
                meanY = meanY + y(i, 0) * ldblMagic
                sse = sse + resid(i) ^ 2
            Else
                resid(i) = y(i, 0) - yhat(i, 0)
                meanY = meanY + y(i, 0)
                sse = sse + resid(i) ^ 2
            End If

        Next
        meanY = meanY / lintrowCount ' mean of Y
        ssy = 0
        For i = 0 To lintWRows - 1
            If lintweight > 0 Then
                If DataArray(i, 1) <> 0 Then
                    ssy = ssy + ((y(i, 0) * DataArray(i, 1) ^ -0.5 - meanY * System.Math.Abs(CInt(mboolIntercept)))) ^ 2 * DataArray(i, 1)
                End If
            Else

                ssy = ssy + (y(i, 0) - meanY * System.Math.Abs(CInt(mboolIntercept))) ^ 2
            End If

        Next
        r2 = (ssy - sse) / ssy

        df = lintWRows - (NumColumns - lintweight - CShort(mboolIntercept) * 0 - 1) ' degrees of squared errors
        ra2 = 1 - CInt(lintrowCount + CInt(mboolIntercept)) * sse / (df * ssy)
        mse = sse / df ' mean squared error

        Dim dist As New statlib
        dist = New statlib()

        ftest = (ssy - sse) / CShort(NumColumns - lintweight - 1 + CShort(mboolIntercept))
        ftest = ftest / mse 'F value
        rmse = System.Math.Sqrt(mse) ' root mean squared error
        For i = 0 To NumColumns - lintweight - 2
            For j = 0 To NumColumns - lintweight - 2
                covb(i, j) = invxx(i, j) * mse
            Next
            stdb(i) = System.Math.Sqrt(System.Math.Abs(covb(i, i))) ' standard errors
            fvalue(i) = (B(i, 0) / stdb(i)) ^ 2
            'probf(i) = dist.PfromT(Math.Sqrt(fvalue(i)), df) / 2
            probf(i) = 0.0#
            On Error Resume Next
            probf(i) = dist.PfromF(fvalue(i), 1, df)
            On Error GoTo ERROR_PROC
        Next

        regressionResults.correlationCoefficient = r2
        regressionResults.regressionDf = CInt(lintWRows + CShort(mboolIntercept)) - df
        regressionResults.regressionSumOfSquares = ssy - sse
        regressionResults.regressionMeanSquare = (ssy - sse) / CShort(NumColumns - lintweight - 1 + CShort(mboolIntercept))
        regressionResults.regressionF = ftest
        regressionResults.residualsDf = df
        regressionResults.residualsSumOfSquares = sse
        regressionResults.residualsMeanSquare = mse
        regressionResults.totalDf = lintWRows + CShort(mboolIntercept)
        regressionResults.totalSumOfSquares = ssy

        Dim myComparer As New spearmanComparer


        If NumColumns + CShort(mboolIntercept) - lintweight = 2 And r2 >= 0.0 Then
            Dim rankArray As Array()
            ReDim rankArray(y.Length)
            For i = 0 To y.Length - 1
                If lintweight = 0 Then
                    rankArray(i) = {DataArray(i, 1), 0, DataArray(i, 0), CDbl(i + 1), CDbl(1)}
                Else
                    rankArray(i) = {DataArray(i, 2), 0, DataArray(i, 0), CDbl(i + 1), DataArray(i, 1)}
                End If
            Next i

            Dim ties As Integer
            Dim rankAvg As Double
            Dim i0 As Integer
            Dim i1 As Integer
            For i = 0 To y.Length - 1
                i0 = i + 1
                ties = 1
                rankAvg = rankArray(i)(3)
                If i0 < y.Length Then
                    While rankArray(i)(2) = rankArray(i0)(2)
                        ties += 1
                        rankAvg += rankArray(i0)(3)
                        i0 += 1
                        If i0 >= y.Length Then
                            Exit While
                        End If
                    End While
                End If
                rankAvg /= CDbl(ties)
                If ties > 1 Then
                    For i1 = i To i + ties - 1
                        rankArray(i1)(3) = rankAvg
                    Next i1
                End If
                i += ties - 1
            Next i

            Array.Sort(rankArray, myComparer)
            For i = 0 To y.Length - 1
                rankArray(i)(1) = CDbl(i + 1)
            Next i
            For i = 0 To y.Length - 1
                i0 = i + 1
                ties = 1
                rankAvg = rankArray(i)(1)
                If i0 < y.Length Then
                    While rankArray(i)(0) = rankArray(i0)(0)
                        ties += 1
                        rankAvg += rankArray(i0)(1)
                        i0 += 1
                        If i0 >= y.Length Then
                            Exit While
                        End If
                    End While
                End If
                rankAvg /= CDbl(ties)
                If ties > 1 Then
                    For i1 = i To i + ties - 1
                        rankArray(i1)(1) = rankAvg
                    Next i1
                End If
                i += ties - 1
            Next i

            If lintweight = 0 Then
                regressionResults.pearsonCoefficient = Math.Sqrt(r2)
                If B(0, 0) < 0 Then
                    regressionResults.pearsonCoefficient *= -1
                End If

                Dim yRankMean As Double
                Dim xRankMean As Double
                Dim sumWeights As Double
                For i = 0 To y.Length - 1
                    yRankMean += rankArray(i)(3) * rankArray(i)(4)
                    xRankMean += rankArray(i)(1) * rankArray(i)(4)
                    sumWeights += rankArray(i)(4)
                Next i
                yRankMean /= sumWeights
                xRankMean /= sumWeights
                Dim numerator As Double
                Dim denominatorA As Double
                Dim denominatorB As Double
                For i = 0 To y.Length - 1
                    numerator += rankArray(i)(4) * (rankArray(i)(1) - xRankMean) * (rankArray(i)(3) - yRankMean)
                    denominatorA += rankArray(i)(4) * (rankArray(i)(1) - xRankMean) ^ 2.0
                    denominatorB += rankArray(i)(4) * (rankArray(i)(3) - yRankMean) ^ 2.0
                Next i
                regressionResults.spearmanCoefficient = numerator / Math.Sqrt(denominatorA * denominatorB)
                regressionResults.spearmanCoefficientT = Math.Sqrt(y.Length - 2) * Math.Sqrt(regressionResults.spearmanCoefficient ^ 2.0 / (1.0 - regressionResults.spearmanCoefficient ^ 2.0))
                regressionResults.spearmanCoefficientTP = dist.PfromT(regressionResults.spearmanCoefficientT, y.Length - 2) * 2
                Dim yMean As Double
                Dim xMean As Double
                sumWeights = CDbl(0)
                For i = 0 To y.Length - 1
                    yMean += DataArray(i, 0)
                    xMean += DataArray(i, 1)
                    sumWeights += 1.0
                Next i
                yMean /= sumWeights
                xMean /= sumWeights
                numerator = CDbl(0)
                denominatorA = CDbl(0)
                denominatorB = CDbl(0)
                For i = 0 To y.Length - 1
                    numerator += (DataArray(i, 1) - xMean) * (DataArray(i, 0) - yMean)
                    denominatorA += (DataArray(i, 1) - xMean) ^ 2.0
                    denominatorB += (DataArray(i, 0) - yMean) ^ 2.0
                Next i
                regressionResults.pearsonCoefficient = numerator / Math.Sqrt(denominatorA * denominatorB)
            Else
                Dim yMean As Double
                Dim xMean As Double
                Dim sumWeights As Double
                For i = 0 To y.Length - 1
                    yMean += DataArray(i, 0) * DataArray(i, 1)
                    xMean += DataArray(i, 1) * DataArray(i, 2)
                    sumWeights += DataArray(i, 1)
                Next i
                yMean /= sumWeights
                xMean /= sumWeights
                Dim numerator As Double
                Dim denominatorA As Double
                Dim denominatorB As Double
                For i = 0 To y.Length - 1
                    numerator += DataArray(i, 1) * (DataArray(i, 2) - xMean) * (DataArray(i, 0) - yMean)
                    denominatorA += DataArray(i, 1) * (DataArray(i, 2) - xMean) ^ 2.0
                    denominatorB += DataArray(i, 1) * (DataArray(i, 0) - yMean) ^ 2.0
                Next i
                regressionResults.pearsonCoefficient = numerator / Math.Sqrt(denominatorA * denominatorB)
                regressionResults.spearmanCoefficient = 10.0
            End If
            regressionResults.pearsonCoefficientT = Math.Sqrt(y.Length - 2) * Math.Sqrt(regressionResults.pearsonCoefficient ^ 2.0 / (1.0 - regressionResults.pearsonCoefficient ^ 2.0))
            regressionResults.pearsonCoefficientTP = dist.PfromT(regressionResults.pearsonCoefficientT, y.Length - 2) * 2
        Else
            regressionResults.pearsonCoefficient = 10.0
            regressionResults.spearmanCoefficient = 10.0
        End If

        ReDim coeff(4, NumColumns - 2 - lintweight)

        regressionResults.variables = New List(Of VariableRow)

        For i = 0 To NumColumns - 1 - lintweight - 1
            coeff(0, i) = mStrAMatrixLabels(i)
            coeff(1, i) = B(i, 0)
            coeff(2, i) = stdb(i)
            coeff(3, i) = fvalue(i)
            coeff(4, i) = probf(i)

            Dim variableRow As VariableRow
            variableRow.variableName = coeff(0, i).ToString()
            variableRow.coefficient = B(i, 0)
            variableRow.stdError = stdb(i)
            variableRow.Ftest = fvalue(i)
            variableRow.P = probf(i)

            regressionResults.variables.Add(variableRow)
        Next

        Return regressionResults 'Exit Function
ERROR_PROC:
        regressionResults.errorMessage = "Error: " + Err.Description
        Return regressionResults 'Exit Function
        Resume
    End Function

    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Linear Regression"
        End Get
    End Property

    Public ReadOnly Property ResultArray() As Object
        Get
            ResultArray = VB6.CopyArray(EIRegressGlobals.Results)
        End Get
    End Property

    Private Sub CreateSettingsFromContext()

        Dim j As Integer
        '   Set default values

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
        mstraBoolean(1) = "False"
        mstraBoolean(2) = "True"
        mstrMatchVar = ""
        mstrWeightVar = ""

        ' TODO: Many of these will be moved to InputVariables, must modify Rule to set these properly

        If context.SetProperties.ContainsKey("Intercept") Then
            Dim success As Boolean
            success = Boolean.TryParse(context.SetProperties("Intercept"), mboolIntercept)
        End If

        If context.SetProperties.ContainsKey("P") Then
            Dim success As Boolean
            success = Double.TryParse(context.SetProperties("P"), mdblP)
            If success = True Then
                mdblC = 1 - mdblP
                mdblP = dist.ZFROMP((1 - mdblP) * 0.5)
                mstrC = Str(context.SetProperties("P") * 100)
            End If
        End If
        ReDim mstraTerms(0)
        Dim terms As Integer
        Dim discrete As Integer

        terms = 0
        discrete = 0

        Dim columnNames As List(Of String)
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
                mstrMatchVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("weightvar") Then
                columnNames.Add(kvp.Key.ToLower())
                mstrWeightVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("dependvar") Then
                columnNames.Add(kvp.Key.ToLower())
                mstrDependVar = kvp.Key
            End If

            If kvp.Value.ToLower().Equals("unsorted") Then
                Dim type As String
                type = context.Columns(kvp.Key).DataType.ToString()
                If type.Equals("System.Byte") Then
                    ReDim Preserve mStrADiscrete(discrete)
                    mStrADiscrete(discrete) = kvp.Key
                    discrete = discrete + 1
                End If
                columnNames.Add(kvp.Key.ToLower())
                ReDim Preserve mstraTerms(terms)
                mstraTerms(terms) = kvp.Key
                terms = terms + 1

            End If

            If kvp.Key.ToLower().Equals("intercept") Then
                Dim success As Boolean
                success = Boolean.TryParse(kvp.Value, mboolIntercept) ' TODO: Test
            End If

            If kvp.Key.ToLower().Equals("P") Then
                Dim success As Boolean
                success = Double.TryParse(kvp.Value.ToLower(), mdblP)
                If success = True Then
                    mdblC = 1 - mdblP
                    mstrC = Str(mdblP * 100)
                    mdblP = 0 'dist1.ZFROMP((1 - mdblP) * 0.5)
                End If
            End If
        Next

        Dim columnNamesArray() As String
        columnNamesArray = columnNames.ToArray()

        currentTable = context.GetDataRows(Nothing).CopyToDataTable().DefaultView.ToTable("REGRESS", False, columnNamesArray)

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

        lStrAVarNames = VB6.CopyArray(GetStrNames)

        On Error Resume Next

        'Dim tableName As String
        'tableName = context.SetProperties("TableName")

        Dim selectQuery As String
        Dim sortOrder As String
        selectQuery = ""
        sortOrder = ""
        sortOrder = sortOrder + mstrDependVar + ", "
        For i = 0 To UBound(lStrAVarNames)
            selectQuery = selectQuery + " [" + lStrAVarNames(i) + "] IS NOT NULL AND"
            sortOrder = sortOrder + "[" + lStrAVarNames(i) + "], "
        Next

        selectQuery = selectQuery.Remove(selectQuery.Length - 4, 4)
        sortOrder = sortOrder.Remove(sortOrder.Length - 2, 2)
        sortOrder = sortOrder + " ASC"

        Dim rows() As DataRow
        'rows = context.CurrentDataTable.Select(selectQuery, sortOrder)
        rows = currentTable.Select(selectQuery, sortOrder)

        NumRows = rows.Count 'context.CurrentDataTable.Rows.Count

        On Error GoTo ErrorhandlEr
        If NumRows = 0 Then Err.Raise(vbObjectError, , "<tlt>No Data available to load</tlt>")

        lIntIsMatch = 0

        ReDim mVarArray(UBound(lStrAVarNames))
        For i = 0 To UBound(lStrAVarNames)
            mVarArray(i) = DummyfyLinear(lStrAVarNames(i), context, currentTable)
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

            lVarCurData = rows(i)(mstrDependVar) 'table.Rows(i)(mstrDependVar) 'lVarCurData = table.Rows(i)(0) 'lVarCurData = reader(0) 'lconRS.Fields(0).Value

            If VarType(lVarCurData) = VariantType.Null Then
                lintOffset = lintOffset + 1
                GoTo MISSING

            ElseIf VarType(lVarCurData) = VariantType.String Then
                Err.Raise(3 + vbObjectError, "GET RAW DATA", "<tlt>Please Recode Independent Variable to be numeric</tlt>")

            End If
            DataArray(i - lintOffset, 0) = rows(i)(mstrDependVar) 'table.Rows(i)(mstrDependVar) 'DataArray(i - lintOffset, 0) = table.Rows(i)(0) 'reader(0) 'lconRS.Fields(0).Value

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

        NumRows = NumRows - lintOffset
        NumColumns = lIntVirtualFields
        GetRawData = True
        'End If

cleanup:
        Exit Function

ErrorhandlEr:
        errorMessage = Err.Description
        'Err.Raise(vbObjectError + 234, , Err.Description)
        GetRawData = False

    End Function

    Private Class SpearmanComparer
        Implements IComparer
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
            Dim a1() As Double = DirectCast(x, Double())
            Dim a2() As Double = DirectCast(y, Double())
            Dim rv As Integer
            If (Not (a1 Is Nothing)) And (Not (a2 Is Nothing)) Then
                rv = a1(0).CompareTo(a2(0))
            ElseIf Not (a1 Is Nothing) Then
                rv = -1
            ElseIf Not (a2 Is Nothing) Then
                rv = 1
            End If
            Return rv
        End Function
    End Class


End Class