Option Strict Off
Option Explicit On
<System.Runtime.InteropServices.ProgId("cFreq_NET.cFreq")> Public Class cFreq
    'Implements IEpiFace.IEpi
	
	Private DataArray As Object
	Private ColumnNames As Object
	Private ColumnTypes As Object
	Private NumColumns As Integer
    Dim outdat(,) As Object
    'Dim dist1 As New dist.statlib
	
	Dim sInter, sMinimal, sAdvanced As String
	Private Settings As Object
    Public WriteOnly Property IEpi_ColumnNames() As Object 'Implements IEpiFace.IEpi.ColumnNames
        Set(ByVal Value As Object)
            'UPGRADE_WARNING: Couldn't resolve default property of object RHS. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object ColumnNames. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ColumnNames = Value

        End Set
    End Property
	
    Public WriteOnly Property IEpi_ColumnTypes() As Object 'Implements IEpiFace.IEpi.ColumnTypes
        Set(ByVal Value As Object)
            'UPGRADE_WARNING: Couldn't resolve default property of object RHS. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object ColumnTypes. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            ColumnTypes = Value
        End Set
    End Property
	
    Public WriteOnly Property IEpi_DataArray() As Object 'Implements IEpiFace.IEpi.DataArray
        Set(ByVal Value As Object)
            'UPGRADE_WARNING: Couldn't resolve default property of object RHS. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            DataArray = Value
        End Set
    End Property
	
    Private ReadOnly Property IEpi_Explanation() As String 'Implements IEpiFace.IEpi.Explanation
        Get

        End Get
    End Property
	
    Private ReadOnly Property IEpi_FunctionNames() As Object 'Implements IEpiFace.IEpi.FunctionNames
        Get

        End Get
    End Property
	
    Private WriteOnly Property IEpi_MoreDataComing() As Boolean 'Implements IEpiFace.IEpi.MoreDataComing
        Set(ByVal Value As Boolean)

        End Set
    End Property
	
    Public WriteOnly Property IEpi_NumColumns() As Integer 'Implements IEpiFace.IEpi.NumColumns
        Set(ByVal Value As Integer)
            NumColumns = Value
        End Set
    End Property
	
    Private WriteOnly Property IEpi_NumRows() As Integer 'Implements IEpiFace.IEpi.NumRows
        Set(ByVal Value As Integer)
        End Set
    End Property
	
    Private WriteOnly Property IEpi_NumStrata() As Integer 'Implements IEpiFace.IEpi.NumStrata
        Set(ByVal Value As Integer)

        End Set
    End Property
	
    Public ReadOnly Property IEpi_ResultArray() As Object 'Implements IEpiFace.IEpi.ResultArray
        Get
            'UPGRADE_WARNING: Couldn't resolve default property of object IEpi_ResultArray. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            IEpi_ResultArray = VB6.CopyArray(outdat)
        End Get
    End Property
	
    Public WriteOnly Property IEpi_Settings() As Object 'Implements IEpiFace.IEpi.Settings
        Set(ByVal Value As Object)
            'UPGRADE_WARNING: Couldn't resolve default property of object RHS. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object Settings. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Settings = Value
        End Set
    End Property
	
    Public Function IEpi_DoFunction(ByRef FunctionName As String) As String 'Implements IEpiFace.IEpi.DoFunction
        Select Case FunctionName
            Case "freq"
                IEpi_DoFunction = Freq()
        End Select
    End Function
	Private Function Freq() As String
		Dim j As Object
		Dim colcum, i, outcol, numrows As Short
        Dim out(,) As Double
		
		colcum = 0
		numrows = 0
		sAdvanced = Space(0) : sInter = Space(0) : sMinimal = Space(0)
		For i = 1 To NumColumns
			'UPGRADE_WARNING: Couldn't resolve default property of object ColumnTypes(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If ColumnTypes(i) = 0 Then
				numrows = numrows + UBound(DataArray(i), 1)
			End If
		Next 
		If numrows >= 1 Then
			'UPGRADE_WARNING: Lower bound of array outdat was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
			ReDim outdat(numrows, 2)
		End If
		For i = 1 To NumColumns
			'UPGRADE_WARNING: Couldn't resolve default property of object ColumnTypes(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If ColumnTypes(i) = 0 Then
				outcol = UBound(DataArray(i), 1)
				'UPGRADE_WARNING: Lower bound of array out was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
				ReDim out(outcol, 2)
				Call freqChar(i, out)
				sAdvanced = "<TABLE> <CAPTION><B><tlt>95% Conf Limits</tlt></B></CAPTION>" & vbCrLf
				For j = 1 To outcol
					'UPGRADE_WARNING: Couldn't resolve default property of object j. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_WARNING: Couldn't resolve default property of object outdat(j + colcum, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					outdat(j + colcum, 1) = out(j, 1)
					'UPGRADE_WARNING: Couldn't resolve default property of object j. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_WARNING: Couldn't resolve default property of object outdat(j + colcum, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					outdat(j + colcum, 2) = out(j, 2)
					'UPGRADE_WARNING: Couldn't resolve default property of object j. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_WARNING: Couldn't resolve default property of object DataArray()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					sAdvanced = sAdvanced & "<TR><TD>" & DataArray(i)(j, 1) & "<TD>" & VB6.Format(out(j, 1), "##0.0%") & "<TD>" & VB6.Format(out(j, 2), "##0.0%") & vbCrLf
				Next 
				sAdvanced = sAdvanced & "</TABLE>"
				sInter = sAdvanced
				sMinimal = sAdvanced
				colcum = colcum + outcol
			Else
				freqNum(i)
			End If
		Next 
		'Freq = result
		Select Case Settings(0) 'Statistics Level
			Case 0 : Freq = sMinimal & vbCrLf
			Case 1 : Freq = sInter & vbCrLf
			Case 2 : Freq = sAdvanced & vbCrLf
			Case Else : Freq = sAdvanced & vbCrLf
		End Select
	End Function
	
    Private Function freqChar(ByRef col As Short, ByRef out(,) As Double) As String
        Dim i As Object
        Dim outcol As Object
        Dim Result As Object
        Dim m As Object
        Dim pct, LOWER, Upper, cum As Double
        'UPGRADE_WARNING: Couldn't resolve default property of object sum(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        m = sum(DataArray(col))
        'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        Result = "<tlt>95% Conf Limit</tlt>" & vbNewLine
        'UPGRADE_WARNING: Couldn't resolve default property of object outcol. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        outcol = UBound(DataArray(col), 1)
        'UPGRADE_WARNING: Couldn't resolve default property of object outcol. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        For i = 1 To outcol
            'pct = CDbl(DataArray(col)(i, 2)) / m
            'cum = cum + pct
            'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object DataArray()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Call getConfInt(CDbl(DataArray(col)(i, 2)), CDbl(m), 95, LOWER, Upper)
            'result = result & DataArray(col)(i, 1) & "   " & DataArray(col)(i, 2) & "   "
            'result = result & Format(pct, "##0.0%") & "   "
            ' result = result & Format(cum, "##0.0%") & "   "
            'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Result = Result & VB6.Format(LOWER, "##0.0%") & " " & VB6.Format(Upper, "##0.0%") & vbNewLine
            'UPGRADE_WARNING: Couldn't resolve default property of object i. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            out(i, 1) = LOWER
            'UPGRADE_WARNING: Couldn't resolve default property of object i. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            out(i, 2) = Upper
        Next
        'result = result & "Total   " & m & vbNewLine
        'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        freqChar = Result
    End Function
	
	Private Sub freqNum(ByRef col As Short)
		Dim Result As Object
		Dim T As Object
		Dim std_err As Object
		Dim DF As Object
		Dim i As Object
		Dim rowhigh As Object
		Dim n As Object
		Dim m As Object
		Dim s, pct, LOWER, Upper, cum, meanV As Double
		'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		m = 0#
		s = 0#
		'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		n = 0#
		'UPGRADE_WARNING: Couldn't resolve default property of object rowhigh. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		rowhigh = UBound(DataArray(col), 1)
		'UPGRADE_WARNING: Couldn't resolve default property of object rowhigh. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		For i = 1 To rowhigh
			'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(col)(i, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object DataArray()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			m = m + DataArray(col)(i, 1) * DataArray(col)(i, 2) 'sum
			'UPGRADE_WARNING: Couldn't resolve default property of object DataArray()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			n = n + DataArray(col)(i, 2)
		Next 
		'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		meanV = m / n
		'UPGRADE_WARNING: Couldn't resolve default property of object rowhigh. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		For i = 1 To rowhigh
			'UPGRADE_WARNING: Couldn't resolve default property of object DataArray(col)(i, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object DataArray()(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			s = s + (DataArray(col)(i, 1) - meanV) ^ 2 * DataArray(col)(i, 2)
		Next 
		'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		s = s / (n - 1)
		'result = "ColumnNames(col) & "   Freq    Percent   Cum. " & vbNewLine
		
		'For i = 1 To rowhigh
		'pct = CDbl(DataArray(col)(i, 2)) / n
		'cum = cum + pct
		'result = result & DataArray(col)(i, 1) & "   " & DataArray(col)(i, 2) & "   "
		'result = result & Format(pct, "##0.0%") & "   "
		'result = result & Format(cum, "##0.0%") & vbNewLine
		'Next
		'result = result & "Total" & n & "   " & Format(cum, "##0.0%") & vbNewLine
		'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object DF. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		DF = n - 1
		'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object std_err. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		std_err = System.Math.Sqrt(s / n)
		'UPGRADE_WARNING: Couldn't resolve default property of object std_err. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object T. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		T = meanV / std_err
		
		'UPGRADE_WARNING: Lower bound of array outdat was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
		ReDim outdat(2, 7)
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 1) = "Mean"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 2) = "Variance"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 3) = "Std Dev"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 4) = "Std Err"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 5). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 5) = "T"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 6). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 6) = "df"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(1, 7). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(1, 7) = "p-value"
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 1). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(2, 1) = meanV
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 2). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(2, 2) = s
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 3). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(2, 3) = System.Math.Sqrt(s)
		'UPGRADE_WARNING: Couldn't resolve default property of object std_err. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 4). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(2, 4) = std_err
		'UPGRADE_WARNING: Couldn't resolve default property of object T. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 5). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(2, 5) = T
		'UPGRADE_WARNING: Couldn't resolve default property of object DF. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 6). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		outdat(2, 6) = DF
		'UPGRADE_WARNING: Couldn't resolve default property of object DF. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object T. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(2, 7). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        outdat(2, 7) = 0 'dist1.PfromT(T, DF)
		
		'UPGRADE_WARNING: Couldn't resolve default property of object std_err. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object n. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		Result = "<TABLE ALIGN=CENTER>" & vbCrLf & "<TR><TD> <tlt>Total</tlt> " & "<TD> <tlt>Sum</tlt> " & "<TD ALIGN=CENTER> <tlt>Mean</tlt> " & "<TD ALIGN=CENTER> <tlt>Variance</tlt> " & "<TD ALIGN=CENTER> <tlt>Std Dev</tlt> " & "<TD ALIGN=CENTER> <tlt>Std Err</tlt> " & vbCrLf & "<TR><TD ALIGN=RIGHT> " & n & "<TD ALIGN=RIGHT>" & m & "<TD ALIGN=RIGHT>" & VB6.Format(meanV, "##0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(s, "##0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(System.Math.Sqrt(s), "0.0000") & "<TD ALIGN=RIGHT>" & VB6.Format(std_err, "0.0000") & vbCrLf & "</TABLE>" & vbCrLf
		'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		sMinimal = Result
		'Inserted by AD, Dec 2, 99
		'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		Result = Result & "<TABLE ALIGN=CENTER>" & vbCrLf & "<TR><TD> <tlt>Student t, testing whether mean differs from zero</tlt>" & "</TABLE>"
		'End insertion
		
		'UPGRADE_WARNING: Couldn't resolve default property of object outdat(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object DF. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object T. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		Result = Result & "<TABLE ALIGN=CENTER>" & vbCrLf & "<TR><TD> <tlt>T statistic=</tlt>" & VB6.Format(T, "#0.0000") & "<TD><tlt>df</tlt>=" & DF & "<TD><tlt>p-value=</tlt>" & VB6.Format(outdat(2, 7), "##0.0000") & "</TABLE>"
		'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		sInter = Result
		'UPGRADE_WARNING: Couldn't resolve default property of object Result. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		sAdvanced = Result
	End Sub
	
	Private Function sum(ByRef x As Object) As Object
		Dim i As Object
		Dim b As Object
		Dim a As Object
		Dim c As Object
		'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		c = 0
		'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		a = LBound(x, 1)
		'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		b = UBound(x, 1)
		'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		For i = a To b
			'UPGRADE_WARNING: Couldn't resolve default property of object x(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			c = c + x(i, 2)
		Next 
		'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object sum. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		sum = c
	End Function
    Sub FLEISS(ByVal a As Double, ByVal m1 As Double, ByVal z As Double, ByRef LOWER As Double, ByRef Upper As Double)

        Dim p, c7, c5, c3, c1, c2, c4, c6, c8, q As Double
        p = a / m1
        q = 1 - p
        c1 = 2 * m1 * p
        c2 = z * z
        c3 = c1 + c2
        c4 = c3 - 1
        c5 = z * System.Math.Sqrt(c2 - (2 + 1 / m1) + 4 * p * (m1 * q + 1))
        c6 = 2 * (m1 + c2)
        c7 = c3 + 1
        c8 = z * System.Math.Sqrt(c2 + (2 - 1 / m1) + 4 * p * (m1 * q - 1))
        LOWER = System.Math.Abs((c4 - c5) / c6)
        Upper = System.Math.Abs((c7 + c8) / c6)

    End Sub '  { FLEISS )
	'(****************************************************************************
	'*      Exact confidence interval for proportion using the F-distribution    *
	'*      algorythm of Rothman and Boice, 1982 Brownlee, 1965.                *
	'*      adapted to EPI Info from code provided by A. Ray Simons.             *
	'*****************************************************************************)
	Private Sub SwapReal(ByRef n As Double, ByRef m As Double)
		Dim x As Double
		x = n
		n = m
		m = x
	End Sub
	
	Sub Sub1(ByVal a As Object, ByVal b As Object, ByVal r8 As Object, ByVal r0 As Object, ByVal pass As Object, ByRef a1 As Object, ByRef d3 As Object)
		Dim d2 As Object
		Dim d1 As Object
		Dim i As Short
		Dim r7 As Integer ': longint
		Dim s5, r4, r2, r1, r3, r5, p0 As Double
		'begin
		'UPGRADE_WARNING: Couldn't resolve default property of object pass. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If pass = 1 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			r2 = (a + 1) * 2
			'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			r1 = b * 2
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			r2 = a * 2
			'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			r1 = (b + 1) * 2
		End If
		'UPGRADE_WARNING: Couldn't resolve default property of object r8. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		r3 = r8
		r7 = 2
		'UPGRADE_WARNING: Couldn't resolve default property of object r8. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		r4 = (r2 * 0.5) * System.Math.Log(System.Math.Abs(r8))
		i = (r1 - 2) * 0.5
		If i = 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object d1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			d1 = System.Math.Exp(r4)
		Else
			r3 = 1 - r3
			'UPGRADE_WARNING: Couldn't resolve default property of object a1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			a1 = r3 * (r2 * 0.5) * 1E-30
			'UPGRADE_WARNING: Couldn't resolve default property of object a1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			r5 = a1 + 1E-30
			i = i - 1
			Do While i > 0
				r2 = r2 + 2
				r7 = r7 + 2
				'UPGRADE_WARNING: Couldn't resolve default property of object a1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				a1 = a1 * ((r2 * r3) / r7)
				'UPGRADE_WARNING: Couldn't resolve default property of object a1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				r5 = r5 + a1
				i = i - 1
			Loop 
			'UPGRADE_WARNING: Couldn't resolve default property of object d1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			d1 = System.Math.Exp(System.Math.Log(System.Math.Abs(r5)) + System.Math.Log(1E+30) + r4)
		End If
		'UPGRADE_WARNING: Couldn't resolve default property of object pass. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If pass = 1 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object d1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object d2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			d2 = 1 - d1
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object d1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object d2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			d2 = d1
		End If
		'UPGRADE_WARNING: Couldn't resolve default property of object d2. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object r0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object d3. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		d3 = r0 + d2
	End Sub
	Sub ExactCI(ByVal a As Double, ByVal m1 As Double, ByVal p3 As Double, ByRef lowerCI As Double, ByRef UpperCI As Double)
		
		Dim flag3, pass, flag2, flag4 As Short ' : integer
		Dim s1, r8, r0, d2, x, b, a1, p, d1, d3, r6, r9, s3 As Double
		'begin
		
		x = 0.01745506493 'x = Tan(1)
		a1 = 0
		b = m1 - a
		pass = 0
		flag2 = 0
		flag3 = 1
		flag4 = 0
		If a = 0 Then
			'begin
			flag3 = 0
			Call SwapReal(a, b)
			'end
		Else
			'begin
			If a / m1 < 0.5 Then
				'begin
				flag4 = 1
				Call SwapReal(a, b)
			End If
			'end
		End If
		'end
		s1 = a / m1
		s3 = s1 * (1 - s1)
		r0 = (p3 / 100 - 1) * 0.5
		If b = 0 Then
			flag2 = 1
		End If
		Do 
			p = System.Math.Sqrt(s3 / m1)
			If p = 0 Then
				p = 1 / a - x
			End If
			If pass = 1 Then
				p = -p
			End If
			r8 = s1 - p
			r9 = r8
			Call Sub1(a, b, r8, r0, pass, a1, d3)
			r6 = d3
			r8 = r8 - x
			Do 
				Call Sub1(a, b, r8, r0, pass, a1, d3)
				d2 = d3
				d1 = r8
				d3 = ((r9 - r8) / (r6 - d3)) * d2
				r8 = r8 - d3
				r9 = d1
				r6 = d2
			Loop Until System.Math.Abs(d3 / r8) < 0.0000000001
			If flag4 = 1 Then
				r8 = 1 - r8
			End If
			If flag3 = 1 Then
				flag3 = 0
			Else
				'begin
				If flag2 = 1 Then
					'begin
					flag2 = 0
					r8 = 1 - r8
					lowerCI = 0
				End If
				'end
				UpperCI = System.Math.Abs(r8)
				If flag4 = 1 Then
					'begin
					flag4 = 0
					Call SwapReal(lowerCI, UpperCI)
					'end
				End If
				Exit Do
			End If
			'end
			lowerCI = System.Math.Abs(r8)
			If flag2 = 1 Then
				flag2 = 0
				UpperCI = 0
				Exit Do
			End If
			pass = 1
		Loop Until (False)
	End Sub '{ ExactCI }
	
	Sub getConfInt(ByVal a As Double, ByVal m1 As Double, ByVal p3 As Double, ByRef lowerCI As Double, ByRef UpperCI As Double)
		
		Dim plim, p, z As Double
		p = a / m1 : plim = m1 * 0.0048 - 1.1
		'B-1577  2/23/06    'zack add see design for B-1577  if freg=tatol then return uper=1, low=1
		If a = m1 Then
			UpperCI = 1
			lowerCI = 1
		Else
			If ((m1 > 300) And (p < plim)) Or ((m1 > 300) And (p > 0.92)) Or (m1 > 1600) Then
				If p3 / 100 < 0.95 + 0.0001 And p3 / 100 > 0.95 - 0.0001 Then z = 1.96
				If p3 / 100 < 0.99 + 0.0001 And p3 / 100 > 0.99 - 0.0001 Then z = 2.58
				If p3 / 100 < 0.9 + 0.0001 And p3 / 100 > 0.9 - 0.0001 Then z = 1.64
				Call FLEISS(a, m1, z, lowerCI, UpperCI)
			Else
				Call ExactCI(a, m1, p3, lowerCI, UpperCI)
			End If
		End If
	End Sub
	Public Function FREQGetConf(ByVal f As Double, ByVal total As Double, ByVal p As Double, ByRef LOWER As Double, ByRef Upper As Double) As String
		
		Call getConfInt(f, total, p * 100, LOWER, Upper)
		FREQGetConf = "<tlt>Lower</tlt> " & LOWER & vbNewLine & "<tlt>Upper</tlt> " & Upper
	End Function
	
	Public Function FREQGetT(ByVal total As Integer, ByVal sum As Double, ByVal variance As Double, ByRef T As Double, ByRef DF As Integer, ByRef p As Double) As String
		DF = total - 1
		T = (sum / System.Math.Sqrt(total)) / (System.Math.Sqrt(variance) + 0.000000001)
        p = 0 'dist1.PfromT(T, DF)
		FREQGetT = "<tlt>T statistic</tlt> " & T & vbNewLine & "<tlt>Degree of freedom</tlt> " & DF & vbNewLine & "P value " & p
	End Function
End Class