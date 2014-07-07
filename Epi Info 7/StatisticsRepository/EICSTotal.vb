Option Strict Off
Option Explicit On
Public Class CSTotal
    '************************************************************************************
    '*
    '* clsTotal.cls Source File
    '*
    '* DESCRIPTION:
    '* This class contains an abstraction of the totals that are calculated
    '* by the CTables function.
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
    '*
    '*
    '* Centers for Disease Control and Prevention
    '*
    '* This source is public domain.  It should be made freely available
    '* to users of any software whose creation is wholly or partly dependent
    '* on this file.
    '*
    '************************************************************************************


    'type
    '    Ptotal=^total;
    '    total = Record
    '            Domain:string[80];
    '            Category:string[80];
    '            YE:Float;
    '            n:integer;
    '            qha,qha2,Sumqha,Sumqha2:Float;
    '            VarT:Float;
    '            NextDom,NextCat:Ptotal;
    '          end;

    Public Domain As Object
    Public Category As Object
    Public YE As Object
    Public N As Integer
    Public qha As Object
    Public qha2 As Object
    Public Sumqha As Object
    Public Sumqha2 As Object
    Public VarT As Object
    Public NextDom As CSTotal
    Public NextCat As CSTotal
End Class