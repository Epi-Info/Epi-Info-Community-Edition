Option Strict Off
Option Explicit On
Option Compare Text

Public Module EICoxGlobals

    ''String Data Loading Variables
    Public mstrTimeVar As String
    'Public mstrWeightVar As String
    Public mstrGroupVar As String
    Public mstraStrataVar() As String
    Public mstraTimeDependentVar() As String
    Public mstraCovariates() As String

    Public mstraBLabels(2) As String
    Public mstrCensoredVar As String
    Public mstrUncensoredVal As String

    ''Plot Variables
    Public mstraPlotVar() As String
    Public mlstPlotVar As New List(Of String)

    ''Strat Specific
    Structure Strata
        Dim strName As String
        Dim strNames() As String
        Dim dblaData(,) As Double
        Dim lintcols As Integer
        Dim lintrows As Integer
        'Time Dependent Stuff..!!!..!!!>...!!!..!!!..!!!!..!!!....!!
        Dim mdblaTime() As Double
        Dim intaDataColumns() As Integer
        Dim intaTimeSelectors() As Integer
    End Structure

    Structure StrataVariable
        Dim iTerms As Integer
        Dim straTerms() As Object
        Dim strName As String
    End Structure

    Public mSVarArray() As StrataVariable
    Public mintTimeDepCount As Integer
    ''Log Rank Statistic stuff
    'Public mdblaNj(1, 0) As Double
    'Public mdblaMj(1, 0) As Double

    ''The strata Array
    ''If there is no strata, it holds
    ''The data of all
    Public mStrataA() As Strata

    'Global data for loading stuff into the data tables
    Public mintRealFields As Integer
    Public mintVirtualFields As Integer

    ''Global offset value to tell us if there is a wieght
    Public mintOffset As Integer
    Public mintWeight As Integer

    ''Mean tables
    Public mdblaMeans() As Double
    Public dist1 As New statlib

End Module
