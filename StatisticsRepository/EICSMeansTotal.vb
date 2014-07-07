Option Strict Off
Option Explicit On

Public Class CSMeansTotal
    Public Domain As Object
    Public YE As Object
    Public NextTotal As CSMeansTotal
    Public SumW As Object
    Public N As Short
    Public Min As Object
    Public Max As Object
    Public qha As Object
    Public qha2 As Object
    Public Sumqha As Object
    Public Sumqha2 As Object
    Public VarT As Object

    Public Sub New()

        Domain = String.Empty
        YE = 0.0#
        SumW = 0.0#
        N = 0
        Min = 0.0# 'Outcome.FieldReal
        Max = Min
        NextTotal = Nothing

    End Sub

    Public Sub New(ByVal dom As String)
        
        Domain = dom
        YE = 0.0#
        SumW = 0.0#
        N = 0
        Min = 0.0# 'Outcome.FieldReal
        Max = Min
        NextTotal = Nothing

    End Sub


End Class
