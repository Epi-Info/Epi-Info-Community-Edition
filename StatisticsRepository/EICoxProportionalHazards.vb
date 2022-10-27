Option Strict Off
Option Explicit On
Option Compare Text

<System.Runtime.InteropServices.ProgId("EICox_NET.EICox")> Public Class EICoxProportionalHazards
    Implements EpiInfo.Plugin.IAnalysisStatistic
    'VERSION 1.0 CLASS
    'BEGIN
    '  MultiUse = -1  'True
    '  Persistable = 0  'NotPersistable
    '  DataBindingBehavior = 0  'vbNone
    '  DataSourceBehavior = 0   'vbNone
    '  MTSTransactionMode = 0   'NotAnMTSObject
    'End

    Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)

    Private Const ErrStart As Integer = &H3000
    Private Const conConnStr As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
	Private context As EpiInfo.Plugin.IAnalysisStatisticContext
	Public contextInputVariableList As Dictionary(Of String, String)
	Public contextSetProperties As Dictionary(Of String, String)
	Public contextColumns As System.Data.DataColumnCollection
	Public EpiViewVariableList As Dictionary(Of String, EpiInfo.Plugin.IVariable)
	Public contextDataTable As System.Data.DataTable

	Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
        context = AnalysisStatisticContext
    End Sub

    Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute
		'Formerly EICoxRegress() --the main function running Cox Proportional Hazards statistic

		Dim config As Dictionary(Of String, String) = New Dictionary(Of String, String)
		config = contextSetProperties
		If context IsNot Nothing Then
			config = context.SetProperties
		End If

		Dim DT As System.Data.DataTable = New System.Data.DataTable()

		If context IsNot Nothing Then
			For Each column As DataColumn In context.Columns
				Dim newColumn As DataColumn = New DataColumn(column.ColumnName)
				newColumn.DataType = column.DataType
				DT.Columns.Add(newColumn)
			Next
		Else
			For Each column As DataColumn In contextColumns
				Dim newColumn As DataColumn = New DataColumn(column.ColumnName)
				newColumn.DataType = column.DataType
				DT.Columns.Add(newColumn)
			Next
		End If

		Dim VariableList As List(Of String) = New List(Of String)


		'List<string> VariableList = new List<string>();
		'      VariableList.Add(this.Numeric_Variable);
		'      foreach (DataRow row in this.Context.GetDataRows(VariableList))
		'      {
		'          DT.ImportRow(row);
		'      }

		


		CreateSettingsFromContext()

        mMatrixlikelihood = New EIMatrix

        Dim strCoxPHResults As String

        Dim i As Integer
        Dim ldblB() As Double
        Dim lstrError As String
        Dim args As Dictionary(Of String, String)
        args = New Dictionary(Of String, String)

        On Error GoTo errorLikeLihood
        'Check for a weight variable
        If Len(mstrWeightVar) > 0 Then
            mintWeight = 1
        Else
            mintWeight = 0
        End If

        lstrError = ""

        'The offset of the covariate data in the data array
        mintOffset = 3 + mintWeight
        GetRawData()

        If Not (mstraTimeDependentVar(0) = Nothing) Then
            'If mlstTimeDependentVar.Count > 0 Then
            'den4: Changed from mStrataA(1).dblaData TO mStrataA(0).dblaData
            'ToDo:Den4 Optimize: consider using only the first statement rather than this IF/Then condition.
            '      If no TimeDepVars, then TimeDepCount=0 so (mintVirtualFields - mintOffset + 1 - mintTimeDepCount) equals (mintVirtualFields - mintOffset + 1)--no need for this conditional logic
            mMatrixlikelihood.MaximizeLikelihood(10, 10, mStrataA(0).dblaData, mintOffset, mintVirtualFields - mintOffset + 1 - mintTimeDepCount, mlngIter, mdblToler, mdblConv, False)
            'mMatrixlikelihood.MaximizeLikelihood(10, 10, mStrataA(0).dblaData, mintOffset, mintVirtualFields - mintOffset - mintTimeDepCount, mlngIter, mdblToler, mdblConv, False)  'den4: subtracted 1 from 5th parameter (lintMatrixSize)
        Else
            mMatrixlikelihood.MaximizeLikelihood(10, 10, mStrataA(0).dblaData, mintOffset, mintVirtualFields - mintOffset + 1, mlngIter, mdblToler, mdblConv, False)
            'mMatrixlikelihood.MaximizeLikelihood(10, 10, mStrataA(0).dblaData, mintOffset, mintVirtualFields - mintOffset, mlngIter, mdblToler, mdblConv, False)  'den4: subtracted 1 from 5th parameter (lintMatrixSize)
        End If

        'Get the Beta's from the Maximized likelihood function
        ldblB = VB6.CopyArray(mMatrixlikelihood.GetCoefficients())

        Dim strGraphResults As String
        strGraphResults = String.Empty

        'Get the Error Report
        If mMatrixlikelihood.getError(lstrError) = False And mMatrixlikelihood.getIters > 0 Then
            strCoxPHResults = EICoxOutput.CreateCoxOutputString(mMatrixlikelihood.getFirstLikelihood, mMatrixlikelihood.getLastLikelihood, mMatrixlikelihood.getIters, mMatrixlikelihood.GetCoefficients, mMatrixlikelihood.GetInverseMatrix, mMatrixlikelihood.GetConvergence, mMatrixlikelihood.getScore)

			If UBound(mstraPlotVar) >= 0 Then
                'If mlstPlotVar.Count > 0 Then
                strGraphResults = PlotGraphs(ldblB)
			End If

			args.Add("COMMANDNAME", "COXPH")
			If context IsNot Nothing Then
				args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
			End If
			args.Add("HTMLRESULTS", strGraphResults + strCoxPHResults)

			If context IsNot Nothing Then
				context.Display(args)
			End If

			'If UBound(mstraPlotVar) > 0 Then
			'If mlstPlotVar.Count > 0 Then
			'    'ToDo: den4: Get Plots working when we have Graph set up.
			'    PlotGraphs(ldblB)
			'End If
		Else
            Err.Raise(vbObjectError + 120, , lstrError)
            strCoxPHResults = "<B><TLT>ERROR: Colinear Data </TLT></B>"
            ReDim Results(2, 1)
            Results(0, 0) = "ERROR"
            Results(1, 0) = "Colinear Data"
            strCoxPHResults = "<br clear=""all"" /><p align=""left""><b><tlt>Colinear Data</tlt></b></p>"

			args.Add("COMMANDNAME", "COXPH")
			If context IsNot Nothing Then
				args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
			End If
			args.Add("HTMLRESULTS", strCoxPHResults)
			If context IsNot Nothing Then
				context.Display(args)
			End If
		End If
        Exit Sub
errorLikeLihood:
        strCoxPHResults = "<TLT>ERROR: " & Err.Description & "</TLT>"
        ReDim Results(2, 1)
        Results(0, 0) = "ERROR"
        Results(1, 0) = Err.Description
        strCoxPHResults = "<br clear=""all"" /><p align=""left""><b><tlt> " & Err.Description & "</tlt></b></p>"

		args.Add("COMMANDNAME", "COXPH")
		If context IsNot Nothing Then
			args.Add("COMMANDTEXT", context.SetProperties("CommandText"))
		End If
		args.Add("HTMLRESULTS", strCoxPHResults)
		If context IsNot Nothing Then
			context.Display(args)
		End If
		'Resume
		ReDim Results(1, 0) 'den4
        Exit Sub
        Resume
    End Sub
    Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
        Get
            Return "Cox Proportional Hazards"
        End Get
    End Property

    Private WithEvents mMatrixlikelihood As EIMatrix

    Public ReadOnly Property ResultArray() As Object
        Get
            ResultArray = VB6.CopyArray(Results)
        End Get
    End Property

    Private Sub CreateSettingsFromContext()
        Dim i As Integer
        Dim j As Integer

        Dim dist1 As New statlib
        dist1 = New statlib()

        'mlstCovariates.Clear()

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
        mstraBLabels(2) = "Missing"

		'Extract data from context.SetProperties

		If context IsNot Nothing Then
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
				'mstrConnString = context.SetProperties("BLabels")
			End If
		Else
			If contextSetProperties.ContainsKey("Database") Then
				mstrConnString = conConnStr & contextSetProperties("Database") & ";"
			End If

			If contextSetProperties.ContainsKey("ConnectionString") Then
				mstrConnString = contextSetProperties("ConnectionString")
			End If

			If contextSetProperties.ContainsKey("TableName") Then
				mstrTableName = contextSetProperties("TableName")
			End If

			If contextSetProperties.ContainsKey("BLabels") Then
				Dim booleanLabels() As String
				booleanLabels = contextSetProperties("BLabels").ToString().Split(";")

				For i = 0 To UBound(mstraboolean)
					mstraboolean(i) = booleanLabels(i)
				Next
				'mstrConnString = context.SetProperties("BLabels")
			End If
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

		If context IsNot Nothing Then
			mstraCovariates = context.InputVariableList("CovariateList").ToString().Split(";")
			'mstraDiscrete is a subset of mstraCovariates, but not necessarily equal, 
			'but may be equal if only one, GroupVar or if ALL OtherVar's are also specified as DummyVars
			mStrADiscrete = context.InputVariableList("DiscreteList").ToString().Split(";")
			mstraStrataVar = context.InputVariableList("StrataVarList").ToString().Split(";")
			mstrTimeVar = context.InputVariableList("time_variable").ToString()
			'ToDo: den4: eventually, remove next ReDim when ExtendVarList added to context
			ReDim mstraTimeDependentVar(0)
			'mstraTimeDependentVar = context.InputVariableList("ExtendVarList").ToString().Split(";")
			mstrCensoredVar = context.InputVariableList("censor_variable").ToString()
			Select Case (context.InputVariableList("censor_value").ToString())
				Case "(+)"
					mstrUncensoredVal = "1"
				Case "(-)"
					mstrUncensoredVal = "0"
				Case "(.)"
					mstrUncensoredVal = String.Empty
				Case Else
					mstrUncensoredVal = context.InputVariableList("censor_value").ToString()
			End Select
			If Not context.InputVariableList("weightvar") Is Nothing Then
				mstrWeightVar = context.InputVariableList("weightvar").ToString()
			Else
				mstrWeightVar = String.Empty
			End If
			If Not context.InputVariableList("GraphVariableList") Is Nothing Then
				mstraPlotVar = context.InputVariableList("GraphVariableList").ToString().Split(",")
				mlstPlotVar = mstraPlotVar.ToList()
			End If
		Else
			mstraCovariates = contextInputVariableList("CovariateList").ToString().Split(";")
			mStrADiscrete = contextInputVariableList("DiscreteList").ToString().Split(";")
			mstraStrataVar = contextInputVariableList("StrataVarList").ToString().Split(";")
			mstrTimeVar = contextInputVariableList("time_variable").ToString()
			ReDim mstraTimeDependentVar(0)
			mstrCensoredVar = contextInputVariableList("censor_variable").ToString()
			Select Case (contextInputVariableList("censor_value").ToString())
				Case "(+)"
					mstrUncensoredVal = "1"
				Case "(-)"
					mstrUncensoredVal = "0"
				Case "(.)"
					mstrUncensoredVal = String.Empty
				Case Else
					mstrUncensoredVal = contextInputVariableList("censor_value").ToString()
			End Select
			If Not contextInputVariableList("weightvar") Is Nothing Then
				mstrWeightVar = contextInputVariableList("weightvar").ToString()
			Else
				mstrWeightVar = String.Empty
			End If
			If Not contextInputVariableList("GraphVariableList") Is Nothing Then
				mstraPlotVar = contextInputVariableList("GraphVariableList").ToString().Split(",")
				mlstPlotVar = mstraPlotVar.ToList()
			End If
		End If
	End Sub

    Public Function GetRawData() As Boolean
        Dim lstrError As String
        'Dim lconRS As ADODB.Recordset
        Dim lstrQuery As String

        Dim lStrAVarNames() As String
        'Dim lVarNames As List(Of String) = New List(Of String)
        Dim lIntRealFields As Integer
        Dim lIntVirtualFields As Integer
        Dim lvarCurData As Object

        Dim lintTempFields As Integer
        Dim ldblatemp() As Double
        Dim lintOffset As Integer
        Dim lintnull As Integer

        Dim i As Integer
        Dim p As Integer

        Dim d As Date
        Dim lstraTimeDep() As String

		Dim NumRows As Integer
		If context IsNot Nothing Then
			NumRows = context.GetDataRows(Nothing).Count
		Else
			NumRows = contextDataTable.Rows.Count
			EICoxLoadData.contextColumns = contextColumns
			EICoxLoadData.contextDataTable = contextDataTable
			EICoxLoadData.contextInputVariableList = contextInputVariableList
			EICoxLoadData.contextSetProperties = contextSetProperties
		End If
		EICoxLoadData.context = context
        EICoxLoadData.dataTable = New DataTable

		If context IsNot Nothing Then
			For Each column As DataColumn In context.Columns
				dataTable.Columns.Add(column.ColumnName, column.DataType)
			Next

			For Each row As DataRow In context.GetDataRows(Nothing)
				EICoxLoadData.dataTable.ImportRow(row)
			Next
		Else
			For Each column As DataColumn In contextColumns
				dataTable.Columns.Add(column.ColumnName, column.DataType)
			Next

			For Each row As DataRow In contextDataTable.Rows
				EICoxLoadData.dataTable.ImportRow(row)
			Next
		End If


		If NumRows <> 0 Then

            'Check to make sure DataBase has been updated
            p = 0
            d = TimeOfDay

            On Error GoTo erroRHandler

            'Parse for Covariate Names
            'If interaction Terms are added, this will make it easier
            'Epi7: note the GetCoxStrNames function now also splits the mstraCovariates based on the comma separators --den4
            lStrAVarNames = GetCoxStrNames(mstraCovariates)
            mstraDiscrete = GetCoxStrNames(mstraDiscrete)

            'Attempt to dummyfy variables.
            'Dummyfy will classify the variable into different types
            'time function, boolean, multiple levels, discrete numeric, continous

            ReDim mVarArray(UBound(lStrAVarNames))
            For i = 0 To UBound(lStrAVarNames)
                mVarArray(i) = Dummyfy(lStrAVarNames(i))
            Next

            'Set the interaction terms
            'This is a function taken from EIRegress
            'It is modified to do time-dependent Covariates

            setCoxInteractionTerms(mstraCovariates, mIVarArray)

            'ReDim mSVarArray(mlstStrataVar.Count - 1) 
            'Set Strata Variables, and prepare to load data
            'ToDo: den4: CoxPH Get SetStratified() working for when strata variables are working
            SetStratified(mstraStrataVar, mSVarArray)
            SetDataTableSizes()

            LoadStrata(mSVarArray, mStrataA)


            'For i = 0 To UBound(MS, 1)
            '    Debug.Print(i.ToString & " ;  " & ldblaDataArray(i, 0).ToString & " ;  " & ldblaDataArray(i, 1).ToString & " ;  " & ldblaDataArray(i, 2).ToString)
            'Next




            'Create the matrix labels for the Coefficients
            ReDim mstraMatrixLabels(mintVirtualFields - 1)  'den4

            'den4: changed i to start at 0 to be used for 0-based indexes within makeCoxMatrixLabels()
            '       also changed p from p=1 to p=0
            p = 0
            For i = 0 To UBound(mstraCovariates) + mintTimeDepCount
                'den4: changed the third parm from 1 to 0 for 0-based arrays within makeCoxMatrixLabels()
                'makeCoxMatrixLabels("", i, 1, p)
                makeCoxMatrixLabels("", i, 0, p)
            Next
            Debug.Print("MATRIX LABELS")
            For i = 0 To UBound(mStrAMatrixLabels)
                System.Diagnostics.Debug.Write(VB6.TabLayout(mStrAMatrixLabels(i), TAB))
            Next i
            Debug.Print("-------------")
            'gconDB.Close()

            GetRawData = True
        End If

cleanup:
        Exit Function
erroRHandler:
        Err.Raise(vbObjectError, , Err.Description)
        Exit Function
        Resume
    End Function

    Private Sub mMatrixlikelihood_CalcLikelihood(ByRef lintOffset As Integer, ByRef ldblA As System.Array, ByRef ldblaB As System.Array, ByRef ldblaJacobian As System.Array, ByRef ldblaF As System.Array, ByRef nRows As Integer, ByRef likelihood As Double, ByRef strError As String, ByRef booStartAtZero As Boolean) Handles mMatrixlikelihood.CalcLikelihood
        Dim i As Integer
        likelihood = 0

        ' added by Eric Fontaine 07/28/03: a value of "" means no error
        strError = ""

        If UBound(mstraTimeDependentVar) > 0 Then
            'If mstraTimeDependentVar.Count > 0 Then
            'den4: changed to i = 0 
            For i = 0 To UBound(mStrataA)
                With mStrataA(i)
                    ' Constructor:         TimeDependentLikelihood(lintOffset,  dblaDataArray(,), ldblaJacobian(,), ldblaB(), ldblaF(), nRows, nCols,   ldblaTime(),  lintaData(),     lintaTimeFuncPos()
                    likelihood = likelihood + TimeDependentLikelihood(lintOffset, .dblaData, ldblaJacobian, ldblaB, ldblaF, .lintrows, .lintcols, .mdblaTime, .intaDataColumns, .intaTimeSelectors)
                End With
            Next i
        Else
            'den4: Fixed 1 TO 0 for 0-based Arrays
            For i = 0 To UBound(mStrataA)
                'ToDo: den4: Resolve potential error in Epi 3 code...BreslowLikelihood(...Jacobian, ldblaB(), ...)
                '  does not agree with BreslowLikelihood definition which calls for (...ldblaB(), Jacobian, ...)
                '  Jacobian and B() are switched in order.
                likelihood = likelihood + BreslowLikelihood(lintOffset, mStrataA(i).dblaData, ldblaJacobian, ldblaB, ldblaF, mStrataA(i).lintrows, mStrataA(i).lintcols)
            Next i
        End If


    End Sub
End Class