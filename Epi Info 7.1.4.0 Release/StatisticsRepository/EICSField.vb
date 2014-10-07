Option Strict Off
Option Explicit On
Public Class CSField
    '************************************************************************************
    '*
    '* clsField.cls Source File
    '*
    '* DESCRIPTION:
    '* This class contains the abstraction of fields read from the database.
    '* It stores the name, value, data type for each field.
    '*
    '*
    '* Algorithms by W.D. Kalsbeek
    '* Written by:   Mario Chin (UNC)
    '*               D. Chris Smith (CDC)
    '*
    '* REMARKS/CHANGE HISTORY:
    '* 16-OCT-2000 - Cass Pallansch, AditTech Consultants
    '*      Ported the original Turbo Pascal code into Visual Basic.  Original
    '*      Turbo Pascal included as comments where appropriate.
    '* 08-MAR-2011 - Erik Knudsen
    '*      Ported VB6 code to VB .NET
    '*
    '*
    '* Centers for Disease Control and Prevention
    '*
    '* This source is public domain.  It should be made freely available
    '* to users of any software whose creation is wholly or partly dependent
    '* on this file.
    '*
    '************************************************************************************


    Private cnFieldLen As Byte ' 0 means no entry for this field
    Private csFieldEntry As Object
    Private csFieldLabel As String
    Private csFieldName As String
    Private cbMissing As Boolean
    Private cenumFieldType As Integer

    Public Function FieldInt() As Short
        ' return the integer value of the field

        If (IsNumeric(csFieldEntry) And CDec(Val(csFieldEntry)) Mod 1 = 0) Then
            FieldInt = CLng(Val(csFieldEntry))
        Else
            FieldInt = 0.0#
        End If

    End Function

    Public Function FieldReal() As Object
        ' return the real value of the field

        If (IsNumeric(csFieldEntry)) Then
            FieldReal = CDec(Val(csFieldEntry))
        Else
            FieldReal = 0.0#
        End If

    End Function

    Public Sub CopyFrom(ByRef P As CSField)

        If (Not P Is Nothing) Then
            Missing = P.Missing
            If Missing Then
                FieldEntry = ""
                FieldLen = 0
            Else
                FieldEntry = P.FieldEntry
                FieldLen = P.FieldLen
            End If
        End If

    End Sub

    Public Sub Show()
        MsgBox(csFieldLabel & " " & csFieldEntry)

    End Sub

    Private Sub Class_Initialize_Renamed()

        cnFieldLen = 0
        csFieldEntry = ""
        csFieldLabel = ""
        csFieldName = ""

    End Sub
    Public Sub New()
        MyBase.New()
        Class_Initialize_Renamed()
    End Sub

    Public Property FieldLen() As Byte
        Get
            FieldLen = cnFieldLen
        End Get
        Set(ByVal Value As Byte)
            cnFieldLen = Value
        End Set
    End Property


    Public Property FieldEntry() As Object
        Get
            FieldEntry = csFieldEntry
        End Get
        Set(ByVal Value As Object)
            csFieldEntry = Value
        End Set
    End Property


    Public Property FieldLabel() As String
        Get
            FieldLabel = csFieldLabel
        End Get
        Set(ByVal Value As String)
            csFieldLabel = Value
        End Set
    End Property


    Public Property FieldName() As String
        Get
            FieldName = csFieldName
        End Get
        Set(ByVal Value As String)
            csFieldName = Value
        End Set
    End Property


    Public Property Missing() As Boolean
        Get
            Missing = cbMissing
        End Get
        Set(ByVal Value As Boolean)
            cbMissing = Value
        End Set
    End Property

    Public Property FieldType() As Integer
        Get
            FieldType = cenumFieldType
        End Get
        Set(ByVal Value As Integer)
            cenumFieldType = Value
        End Set
    End Property
End Class