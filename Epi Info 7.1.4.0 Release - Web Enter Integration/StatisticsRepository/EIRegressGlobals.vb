Option Strict Off
Option Explicit On
Module EIRegressGlobals
	
    Public DataArray(,) As Double 'Used to load from the DataBase
	
	Public ColumnNames As Object
	Public NumColumns As Integer
	Public NumRows As Integer
    Public Results(,) As Object
	Public Settings As Object
	
	Public mstrConnString As String
    Public mstrTableName As String
    Public mstrGraphType As String
	Public mstrTitle As String
	Public mstraTerms() As String
	Public mstrMatchVar As String
	Public mstrWeightVar As String
	Public mstrDependVar As String
	Public mdblP As Double
	Public mlngIter As Integer
	Public mdblConv As Double
	Public mdblToler As Double
	Public mStrADiscrete() As String
	Public mstraBoolean() As String
	Public mboolIntercept As Boolean
	Public mstrStratum As String
	Public mboolFirst As Boolean
	'for the stratum
	Public mLngStrata As Integer
	Public gconDB As ADODB.Connection
	
	
	'Matrix labels
	Public mStrAMatrixLabels() As String
	
	Public mdblConf As Double
	
    'Public dist1 As New dist.statlib
    Public Matrix1 As New EIMatrix
	
	Public mstrC As String
	Public mdblC As Double
End Module