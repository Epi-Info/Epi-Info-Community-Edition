Option Strict Off
Option Explicit On
Option Compare Text
<System.Runtime.InteropServices.ProgId("EIMatch_NET.EIMatch")> Public Class EIMatch
	Implements EpiInfo.Plugin.IAnalysisStatistic

	Const conColorFreqTotal As String = "ORANGE.GIF"
	Const conColorFreq As String = "YELLOW.GIF"
	Const conTablesMult As Double = 100.0#
	Const ABS_MAX_HTML_COLS As Short = 100
	Const PROVIDER_STRING As String = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="

	Public sHTML As String
	Public sStartup As String
	Public sContents As String 'temporary strings to hold formatting
	Private nVariable As Short
	'Private taVariable() As typeVariable
	Private nRow As Short
	'Private taRow() As typeVariable
	Private nCol As Short
	'Private taCol() As typeVariable
	Private nStrata As Short
	'Private taStrata() As typeVariable
	Private mbolStrataErr As Boolean
	Private nGroup As Short
	'Private taGroup() As typeGroupVariable
	Private saTable() As String
	Private saSection() As String
	Private nTables As Short
	Private nSection As Short
	'UPGRADE_WARNING: Arrays in structure rsOutTbl may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
	'Private rsOutTbl As dao.Recordset '   used for table output
	Private nTotal As Integer
	Private sSelectStmt As String
	Private sFromStmt As String


	Private saSqlBase() As String
	Private sSqlFinal As String
	Private saCaptionBase() As String
	Private sCaptionFinal As String
	Public flagGraphics As Boolean
	Public flagStat As Boolean
	Public flagPrompt As Boolean
	Public flagIgnoreMissing As Boolean
	Public flagPercents As Boolean
	Public flagIncludeTables As Boolean
	Public sSettingTrue As String
	Public sSettingFalse As String
	Public sSettingYes As String
	Public sSettingNo As String
	Public sSettingMissing As String
	Public ms0 As String
	Public ms100 As String

	Private sStrata As String
	Private sStrataPrompt As String
	'Private objStatFreq As New Freq.cFreq
	Private vaFreqDataAll() As Object
	'UPGRADE_WARNING: Lower bound of array naFreqColType was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
	Private naFreqColType(1) As Short
	Private saFreqColName() As String
	'Private objStatAnova As New ANOVA.cANOVA
	'Private objStatTables As New EITable.cTable
	Private vaStatTables() As Object
	Private n2x2Table As Short
	Private nStatType As Short ' 0-Minimal 1-Intermediate 2-Advanced

	Dim results As Object
	Dim apppath As String
	'UPGRADE_WARNING: Lower bound of array errmartin was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
	Dim errmartin(2, 9) As String
	Dim sComma, sDot As String '   substitution characters
	Dim mvaResult(,) As Object
	Dim bError As Boolean


	Private Declare Function Strat2x2 Lib "martinbb.dll" (ByVal TableArray As Object, ByVal ConfLevel As Double, ByRef ResultArray As Object) As Object

	Public Sub Construct(ByVal AnalysisStatisticContext As EpiInfo.Plugin.IAnalysisStatisticContext) Implements EpiInfo.Plugin.IAnalysisStatistic.Construct
		context = AnalysisStatisticContext
	End Sub

	Public ReadOnly Property Name As String Implements EpiInfo.Plugin.IAnalysisStatistic.Name
		Get
			Return "Match"
		End Get
	End Property

	Public WriteOnly Property IEpi_Settings() As Object	'Implements 'IEpiFace.IEpi.Settings
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

	Public Function IEpi_DoFunction(ByRef FunctionName As String) As String	'Implements IEpiFace.IEpi.DoFunction
		Select Case FunctionName
			Case "Table"
				'IEpi_DoFunction = Table()
		End Select
	End Function


	Public Sub Execute() Implements EpiInfo.Plugin.IAnalysisStatistic.Execute

		'Public Sub Match(ByRef sRow As String, ByRef sCol As String)
		'   This sub performs the matched analysis, parameters like TABLES
		'   It assumes that the stratavar structures have been set up

		Dim i As Short
		Dim j As Short
		Dim k As Short
		Dim m As Integer
		Dim n As Integer
		Dim s As String
		Dim ss As String
		'Dim td As dao.TableDef
		'Dim qd As dao.QueryDef
		'UPGRADE_WARNING: Arrays in structure rs may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
		'Dim rs As dao.Recordset
		Dim bChecking As Boolean
		Dim lsSQL As String
		Dim lnaCombo() As Integer
		Dim liRow As Short
		Dim liCol As Short
		Dim bDone As Boolean
		Dim lsFromWhere As String
		Dim lsGroup As String
		Dim lsOutcome As String
		Dim lsExposure As String
		'Dim ltypaCols() As mParserTemp.EpiVarType

		'On Error GoTo probMatch
		'UPGRADE_WARNING: Couldn't resolve default property of object Problem.PushStack. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'Problem.PushStack("Match, cAnaDo2")

		'If Len(msFromClause) = 0 Then 'No View is read
		'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'MsgBox(TT.t("No data source has been read"), conMsgBox, TT.t(conMatch))
		'GoTo Cleanup
		'End If
		'If Len(msWeight) <> 0 Then
		'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'MsgBox(TT.t("TABLES must be used for weighted analysis"), conMsgBox, TT.t(conMatch))
		'GoTo Cleanup
		'End If
		'If mnStrata = 0 Then
		'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'MsgBox(TT.t("No match variable has been specified"), conMsgBox, TT.t(conMatch))
		'GoTo Cleanup
		'End If

		'   Get the variables and their information
		'		Startup()
		'		GetStrataInfo()
		'		If mbolStrataErr Then GoTo Cleanup

		'		If sRow = conStar Or sCol = conStar Then
		'			'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'			MsgBox(TT.t("Matched analysis cannot use * variable specification."), conMsgBox, TT.t(conMatch))
		'			GoTo Cleanup
		'		Else
		'			RowCol(sCol, sRow, True, True)
		'		End If

		'		If (nRow = 0) Or (nCol = 0) Then GoTo Cleanup

		'		'   Open the output table if desired
		'		If msOutTbl <> "" Then '   If no table specified, nothing to do
		'			If rsOutTbl Is Nothing Then	'   If the recordset is not open, create it
		'				bChecking = True '   Drop the old table if it exists
		'				td = objDb.db.TableDefs(msOutTbl)
		'				If bChecking Then
		'					bChecking = False
		'					objDb.db.TableDefs.Delete(msOutTbl)
		'					objDb.db.TableDefs.Refresh()
		'				End If
		'				td = objDb.db.CreateTableDef(msOutTbl) '   Create the new table

		'				With td

		'					For i = 0 To nStrata - 1 '   One variable per strata variable
		'						s = taStrata(i).sName

		'						For j = .fields.Count - 1 To 0 Step -1 '   Make sure we have no duplicate field names
		'							If InStr(1, .fields(j).Name, s, CompareMethod.Text) = 1 Then '   if the left part matches
		'								If (Mid(.fields(j).Name, Len(s) + 1, 1) = "_") And IsNumeric(Mid(.fields(j).Name, Len(s) + 2)) Then	'   if the previous is already suffixed
		'									s = s & "_" & VB6.Format(Val(Mid(.fields(j).Name, Len(s) + 2)) + 1)	'   just bump the suffix
		'									Exit For
		'								ElseIf Len(.fields(j).Name) = Len(s) Then  '   if it's a true dupe
		'									s = s & "_1" '   the suffix is _1
		'									Exit For
		'								End If
		'							End If
		'						Next j

		'						If taStrata(i).nType = dao.DataTypeEnum.dbText Then
		'							.fields.Append(.CreateField(s, taStrata(i).nType, 255))
		'						Else
		'							.fields.Append(.CreateField(s, taStrata(i).nType))
		'						End If
		'					Next i

		'					For i = 0 To nCol - 1 '   One variable per column variable
		'						s = taCol(i).sName

		'						For j = .fields.Count - 1 To 0 Step -1 '   Make sure we have no duplicate field names
		'							If InStr(1, .fields(j).Name, s, CompareMethod.Text) = 1 Then '   if the left part matches
		'								If (Mid(.fields(j).Name, Len(s) + 1, 1) = "_") And IsNumeric(Mid(.fields(j).Name, Len(s) + 2)) Then	'   if the previous is already suffixed
		'									s = s & "_" & VB6.Format(Val(Mid(.fields(j).Name, Len(s) + 2)) + 1)	'   just bump the suffix
		'									Exit For
		'								ElseIf Len(.fields(j).Name) = Len(s) Then  '   if it's a true dupe
		'									s = s & "_1" '   the suffix is _1
		'									Exit For
		'								End If
		'							End If
		'						Next j

		'						If taCol(i).nType = dao.DataTypeEnum.dbText Then
		'							.fields.Append(.CreateField(s, taCol(i).nType, 255))
		'						Else
		'							.fields.Append(.CreateField(s, taCol(i).nType))
		'						End If
		'					Next i

		'					For i = 0 To nRow - 1 '   One variable per row variable
		'						s = taRow(i).sName

		'						For j = .fields.Count - 1 To 0 Step -1 '   Make sure we have no duplicate field names
		'							If InStr(1, .fields(j).Name, s, CompareMethod.Text) = 1 Then '   if the left part matches
		'								If (Mid(.fields(j).Name, Len(s) + 1, 1) = "_") And IsNumeric(Mid(.fields(j).Name, Len(s) + 2)) Then	'   if the previous is already suffixed
		'									s = s & "_" & VB6.Format(Val(Mid(.fields(j).Name, Len(s) + 2)) + 1)	'   just bump the suffix
		'									Exit For
		'								ElseIf Len(.fields(j).Name) = Len(s) Then  '   if it's a true dupe
		'									s = s & "_1" '   the suffix is _1
		'									Exit For
		'								End If
		'							End If
		'						Next j

		'						If taRow(i).nType = dao.DataTypeEnum.dbText Then
		'							.fields.Append(.CreateField(s, taRow(i).nType, 255))
		'						Else
		'							.fields.Append(.CreateField(s, taRow(i).nType))
		'						End If
		'					Next i

		'					s = "Count"

		'					For j = .fields.Count - 1 To 0 Step -1 '   Make sure we have no duplicate field names
		'						If InStr(1, .fields(j).Name, s, CompareMethod.Text) = 1 Then '   if the left part matches
		'							If (Mid(.fields(j).Name, Len(s) + 1, 1) = "_") And IsNumeric(Mid(.fields(j).Name, Len(s) + 2)) Then	'   if the previous is already suffixed
		'								s = s & "_" & VB6.Format(Val(Mid(.fields(j).Name, Len(s) + 2)) + 1)	'   just bump the suffix
		'								Exit For
		'							ElseIf Len(.fields(j).Name) = Len(s) Then  '   if it's a true dupe
		'								s = s & "_1" '   the suffix is _1
		'								Exit For
		'							End If
		'						End If
		'					Next j

		'					.fields.Append(.CreateField(s, dao.DataTypeEnum.dbDouble)) '   One for the count
		'					objDb.db.TableDefs.Append(td) '   Add the table to the DB
		'					objDb.db.TableDefs.Refresh()
		'					'UPGRADE_NOTE: Object td may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		'					td = Nothing
		'					rsOutTbl = objDb.db.OpenRecordset(msOutTbl, dao.RecordsetTypeEnum.dbOpenDynaset) '   Open the recordset
		'				End With

		'			End If
		'		End If

		'		'   Build the constant part of the 4x4 query
		'		'UPGRADE_WARNING: Couldn't resolve default property of object objAnaDo1.Get_WhereClause. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'		lsFromWhere = objAnaDo1.Get_WhereClause
		'		If Len(lsFromWhere) = 0 Then lsFromWhere = " WHERE true"
		'		'UPGRADE_WARNING: Couldn't resolve default property of object objAnaDo1.Get_FromClause. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'		lsFromWhere = objAnaDo1.Get_FromClause & lsFromWhere

		'		ReDim ltypaCols(mnStrata + 5)
		'		For i = 0 To mnStrata - 1
		'			lsGroup = lsGroup & "," & taStrata(i).sExpression
		'			ltypaCols(i) = taStrata(i).VarType_Renamed
		'		Next i

		'		lsGroup = Mid(lsGroup, 2)

		'		'   Step through the variable pairs
		'		If flagStat Then NewContents(strCurrentCmd)
		'		nSection = nRow * nCol
		'		ReDim saSection(nSection - 1)
		'		k = 0

		'		For liCol = 0 To nCol - 1

		'			For liRow = 0 To nRow - 1
		'				If taRow(liRow).sName <> taCol(liCol).sName Then
		'					k = k + 1
		'					saSection(k - 1) = UCase(taCol(liCol).sName) & " : " & UCase(taRow(liRow).sName) & sStrata
		'					If mflagHyper And (nSection > 1) Then '   If more than one pair of variables, put out command contents
		'						sHTML = "<BR><A HREF=#Section" & miaLinkCtr(3) & "_" & miaLinkCtr(4) & "_" & k & ">" & saSection(k - 1) & "</A>"
		'						PrintLine(nHTML, sHTML)
		'					End If
		'				End If
		'			Next liRow

		'		Next liCol

		'		'   Build and execute the 4x4 query, loading the data array and the output table if desired
		'		If flagStat Then NewSection() '   dummy section

		'		ltypaCols(nStrata) = mParserTemp.EpiVarType.vtNumber
		'		ltypaCols(nStrata + 1) = mParserTemp.EpiVarType.vtNumber
		'		ltypaCols(nStrata + 2) = mParserTemp.EpiVarType.vtNumber
		'		ltypaCols(nStrata + 3) = mParserTemp.EpiVarType.vtNumber
		'		ltypaCols(nStrata + 4) = mParserTemp.EpiVarType.vtNumber
		'		ltypaCols(nStrata + 5) = mParserTemp.EpiVarType.vtNumber

		'		'UPGRADE_WARNING: Lower bound of array lvaSettings was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
		'		Dim lvaSettings(2, 3) As Object
		'		For liCol = 0 To nCol - 1
		'			For liRow = 0 To nRow - 1
		'				If taRow(liRow).sName = taCol(liCol).sName Then GoTo NextVar
		'				If flagStat Then NewHTMLTable(saSection(nRow * liCol + liRow))
		'				If taRow(liRow).VarType_Renamed = mParserTemp.EpiVarType.vtYesNo Then '   Set the outcome variable expression based on its type
		'					If taRow(liRow).nType = dao.DataTypeEnum.dbByte Then
		'						lsOutcome = taRow(liRow).sExpression
		'					Else
		'						lsOutcome = "iif(" & taRow(liRow).sExpression & ",1,0)"
		'					End If
		'				Else
		'					lsOutcome = "iif(" & taRow(liRow).sExpression & "=0,0,1)"
		'				End If
		'				If taCol(liCol).VarType_Renamed = mParserTemp.EpiVarType.vtYesNo Then '   Set the exposure variable expression based on its type
		'					If taCol(liCol).nType = dao.DataTypeEnum.dbByte Then
		'						lsExposure = taCol(liCol).sExpression
		'					Else
		'						lsExposure = "iif(" & taCol(liCol).sExpression & ",1,0)"
		'					End If
		'				Else
		'					lsExposure = "iif(" & taCol(liCol).sExpression & "=0,0,1)"
		'				End If
		'				lsSQL = "SELECT " & lsGroup & ", sum(" & lsOutcome & ") as Cases, sum(1-" & lsOutcome & ") as Controls, sum(" & lsOutcome & "*" & lsExposure & ") as a, "
		'				lsSQL = lsSQL & "sum(" & lsOutcome & "*(1-" & lsExposure & ")) as b, sum((1-" & lsOutcome & ")*" & lsExposure & ") as c, sum((1-" & lsOutcome & ")*(1-" & lsExposure & ")) as d "
		'				lsSQL = lsSQL & lsFromWhere & " AND " & taRow(liRow).sExpression & " IS NOT NULL AND " & taCol(liCol).sExpression & " IS NOT NULL GROUP BY " & lsGroup
		'				bChecking = True
		'				qd = objDb.db.QueryDefs.Item("EpiMatch")
		'				If bChecking Then
		'					bChecking = False
		'				Else
		'					qd = objDb.db.CreateQueryDef("EpiMatch")
		'					objDb.db.QueryDefs.Refresh()
		'					qd = objDb.db.QueryDefs.Item("EpiMatch")
		'				End If
		'				'UPGRADE_WARNING: Couldn't resolve default property of object objCommonGen.msXSQL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				objCommonGen.msXSQL = lsSQL
		'				qd.Sql = lsSQL
		'				rs = qd.OpenRecordset(dao.RecordsetTypeEnum.dbOpenDynaset)
		'				If rs.EOF Then
		'					'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'					sHTML = TT.t("No data for variables")
		'					PrintLine(nHTML, sHTML)
		'					sHTML = ""

		'					GoTo NextVar
		'				End If

		'				n = 0
		'				'UPGRADE_WARNING: Lower bound of array vaStatTables was changed from 1,1,1 to 0,0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
		'				ReDim vaStatTables(2, 2, 1)
		'				rs.MoveFirst()
		'				m = 0

		'				Do While Not rs.EOF
		'					If (rs.Fields("a").Value + rs.Fields("b").Value <> 0) And (rs.Fields("c").Value + rs.Fields("d").Value <> 0) And (rs.Fields("a").Value + rs.Fields("c").Value <> 0) And (rs.Fields("b").Value + rs.Fields("d").Value <> 0) Then
		'						m = m + 1
		'						'UPGRADE_WARNING: Lower bound of array vaStatTables was changed from 1,1,1 to 0,0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
		'						ReDim Preserve vaStatTables(2, 2, m)
		'						'UPGRADE_WARNING: Couldn't resolve default property of object vaStatTables(1, 1, m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'						vaStatTables(1, 1, m) = rs.Fields("a").Value
		'						'                    vaStatTables(1, 2, m) = rs!b
		'						'                    vaStatTables(2, 1, m) = rs!c
		'						'UPGRADE_WARNING: Couldn't resolve default property of object vaStatTables(2, 1, m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'						vaStatTables(2, 1, m) = rs.Fields("b").Value 'Reversed
		'						'UPGRADE_WARNING: Couldn't resolve default property of object vaStatTables(1, 2, m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'						vaStatTables(1, 2, m) = rs.Fields("c").Value
		'						'UPGRADE_WARNING: Couldn't resolve default property of object vaStatTables(2, 2, m). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'						vaStatTables(2, 2, m) = rs.Fields("d").Value
		'						n = n + rs.Fields("a").Value + rs.Fields("b").Value + rs.Fields("c").Value + rs.Fields("d").Value
		'					End If
		'					If msOutTbl <> "" Then '   If no table specified, nothing to do

		'						With rsOutTbl

		'							For k = 1 To 4 '   Add 4 records for 4 entries
		'								.AddNew()

		'								For j = 0 To mnStrata - 1
		'									.fields(j).Value = rs.Fields(j).Value
		'								Next j

		'								.fields(taRow(liRow).sName).Value = IIf(k <= 2, 1, 0)
		'								.fields(taCol(liCol).sName).Value = (4 - k) And 1
		'								.Fields("Count").Value = rs.fields(Mid("ABCD", k, 1)).Value
		'							Next k

		'						End With

		'					End If
		'					rs.MoveNext()
		'				Loop

		'				rs.Close()
		'				'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				sHTML = sHTML & "<CENTER><BOLD>" & TT.t("Matched Analysis of Tables with Non-Zero Marginals") & "</BOLD><BR>" & vbCrLf
		'				'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				sHTML = sHTML & TT.t("Matched Sets:") & " " & m & "  " & TT.t("Observations:") & " " & n & "</CENTER>"
		'				PrintLine(nHTML, sHTML)
		'				sHTML = ""

		'				'   Execute the summary query and build the combinatorial tables
		'				lsSQL = "SELECT Cases, Controls, A, C, Count(*) AS Count From EPIMatch GROUP BY Cases, Controls, A, C ORDER BY Cases, Controls, A, C"
		'				'UPGRADE_WARNING: Couldn't resolve default property of object objCommonGen.msXSQL. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				objCommonGen.msXSQL = lsSQL
		'				rs = objDb.db.OpenRecordset(lsSQL, dao.RecordsetTypeEnum.dbOpenDynaset)
		'				rs.MoveFirst()
		'				m = 0
		'				n = 0 '   m=No. cases, n=No. controls

		'				Do While True
		'					If rs.EOF Then
		'						bDone = True
		'					ElseIf (rs.Fields("CASES").Value = m) And (rs.Fields("Controls").Value = n) Then
		'						bDone = False
		'					Else
		'						bDone = True
		'					End If
		'					If bDone Then
		'						If (m <> 0) Or (n <> 0) Then '   If it's not the first time, output the combo table
		'							'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'							sHTML = sHTML & "<TABLE BORDER><CAPTION ALIGN=""top"">" & TT.t("Cases:") & " " & m & "  " & TT.t("Controls:") & " " & n & "</CAPTION>"
		'							'UPGRADE_WARNING: Couldn't resolve default property of object TT.t. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'							sHTML = sHTML & "<TR><TH></TH><TH NOWRAP COLSPAN=" & VB6.Format(n + 1) & ">" & TT.t("Exposed Controls") & " </TH></TR><TR><TH>" & TT.t("Exposed Cases") & " </TH>"

		'							For j = n To 0 Step -1
		'								sHTML = sHTML & "<TH>" & VB6.Format(j) & "</TH>"
		'							Next j

		'							sHTML = sHTML & "</TR>"
		'							PrintLine(nHTML, sHTML)
		'							sHTML = ""

		'							For i = m To 0 Step -1 '   Rows
		'								sHTML = sHTML & "<TR><TH>" & VB6.Format(i) & "</TH>"

		'								For j = n To 0 Step -1 '   Columns
		'									sHTML = sHTML & "<TD ALIGN=""Center"">" & lnaCombo(i, j) & "</TD>"
		'								Next j

		'								sHTML = sHTML & "</TR>"
		'							Next i

		'							sHTML = sHTML & "</TABLE>"
		'							If flagIncludeTables Then PrintLine(nHTML, sHTML)
		'							sHTML = ""

		'						End If
		'						If rs.EOF Then Exit Do '   If it's the last combo table, done
		'						m = rs.Fields("CASES").Value '   Prepare to tally
		'						n = rs.Fields("Controls").Value
		'						ReDim lnaCombo(m, n)
		'					End If
		'					lnaCombo(rs.Fields("a").Value, rs.Fields("c").Value) = rs.Fields("Count").Value	'   Store the current result
		'					rs.MoveNext()
		'				Loop

		'				rs.Close()

		'				'   Produce the Mantel-Haenzel results
		'				If UBound(vaStatTables, 3) <= 1 Then GoTo Closedown
		'				'UPGRADE_WARNING: IsEmpty was upgraded to IsNothing and has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
		'				'UPGRADE_WARNING: Lower bound of array vaStatTables was changed from 1,1,1 to 0,0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
		'				If IsNothing(vaStatTables(1, 1, UBound(vaStatTables, 3))) Then ReDim Preserve vaStatTables(2, 2, UBound(vaStatTables, 3) - 1)
		'				objStatTables.IEpi_DataArray = VB6.CopyArray(vaStatTables)
		'				' for debug
		'				'Dim fh%
		'				'fh = FreeFile
		'				'Open "c:\matchout2.txt" For Output As #fh
		'				'For i = 1 To UBound(vaStatTables, 3)
		'				'Print #fh, i, vaStatTables(1, 1, i), vaStatTables(1, 2, i), vaStatTables(2, 1, i), vaStatTables(2, 2, i)
		'				'Next i

		'				objStatTables.IEpi_NumColumns = 2
		'				objStatTables.IEpi_NumRows = 2
		'				objStatTables.IEpi_NumStrata = UBound(vaStatTables, 3)

		'				'UPGRADE_WARNING: Couldn't resolve default property of object lvaSettings(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				lvaSettings(1, 1) = "P"
		'				'UPGRADE_WARNING: Couldn't resolve default property of object lvaSettings(2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				lvaSettings(2, 1) = 0.95
		'				'UPGRADE_WARNING: Couldn't resolve default property of object lvaSettings(1, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				lvaSettings(1, 2) = "StatType"
		'				'UPGRADE_WARNING: Couldn't resolve default property of object lvaSettings(2, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				lvaSettings(2, 2) = nStatType
		'				'UPGRADE_WARNING: Couldn't resolve default property of object lvaSettings(1, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				lvaSettings(1, 3) = "WARNINGS"
		'				'UPGRADE_WARNING: Couldn't resolve default property of object lvaSettings(2, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				lvaSettings(2, 3) = False
		'				objStatTables.IEpi_Settings = VB6.CopyArray(lvaSettings)
		'				bChecking = True
		'				s = objStatTables.IEpi_DoFunction("Table")
		'				'Print #fh, s
		'				'Close #fh
		'				'UPGRADE_WARNING: Couldn't resolve default property of object TT.TranslateHTML. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'				s = TT.TranslateHTML(s)
		'				'   for debug
		'				'    Dim vv As Variant, vvv As Variant
		'				'    For i = LBound(objStatTables.IEpi_ResultArray(0)) To UBound(objStatTables.IEpi_ResultArray(0))
		'				'        Debug.Print i, objStatTables.IEpi_ResultArray(0)(i), objStatTables.IEpi_ResultArray(1)(i)
		'				'    Next i

		'				bChecking = False
		'				i = InStr(s, "SUMMARY")
		'				If i > 1 Then s = Mid(s, i)
		'				sHTML = sHTML & "<BR><HR><BR>" & s & "<BR><HR><BR>"
		'				PrintLine(nHTML, sHTML)
		'				sHTML = ""

		'				ShowResult()

		'NextVar:
		'			Next liRow
		'		Next liCol
		'Closedown:
		'		ShowResult()
		'		RestoreSettings()
		'		ResetStrata()
		'		mflagStats = False
		'Cleanup:
		'		'UPGRADE_NOTE: Object rs may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		'		rs = Nothing
		'		'UPGRADE_NOTE: Object qd may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		'		qd = Nothing
		'		'UPGRADE_NOTE: Object td may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		'		td = Nothing
		'		'UPGRADE_NOTE: Object rsOutTbl may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
		'		rsOutTbl = Nothing
		'		'UPGRADE_WARNING: Couldn't resolve default property of object Problem.PopStack. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'		Problem.PopStack()
		'		Exit Sub

		'probMatch:
		'		If bChecking Then
		'			bChecking = False
		'			Resume Next
		'		End If

		'		'UPGRADE_WARNING: Couldn't resolve default property of object Problem.Action. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'		Select Case Problem.Action(Err.Number, Err.Description, "D")

		'			Case "A"
		'				Resume

		'			Case "B"
		'				Resume Next

		'			Case "C"
		'				Closedown()

		'			Case "D" 'Save Data.

		'			Case Else
		'				Resume

		'		End Select

	End Sub
End Class