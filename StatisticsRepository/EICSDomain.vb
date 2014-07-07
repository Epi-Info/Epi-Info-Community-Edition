Option Strict Off
Option Explicit On
Public Class CSDomain
    '************************************************************************************
    '*
    '* clsDomain.cls Source File
    '*
    '* DESCRIPTION:
    '* This class contains an abstraction of Domains with the EICSamp.DLL.
    '* It holds a value from the Domain as well as the weighted sum, count,
    '* and references to the next Domain and the Category associated with
    '* the Domain values.
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


    '    PDom=^totDom;
    '    totDom = Record
    '            Domain:string[80];
    '            SumW:Float;
    '            n:integer;
    '            NextDom:PDom;
    '            FirstCat:Ptotal;
    '           end;

    Public Domain As Object
    Public SumW As Double
    Public N As Integer
    Public NextDom As CSDomain
    Public FirstCat As CSTotal
End Class