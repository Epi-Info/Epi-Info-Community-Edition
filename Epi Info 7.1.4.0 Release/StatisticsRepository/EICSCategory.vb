Option Strict Off
Option Explicit On
Public Class CSCategory
    '************************************************************************************
    '*
    '* clsCategory.cls Source File
    '*
    '* DESCRIPTION:
    '* This class contains an abstraction of Categories within the EICSamp.DLL.
    '* It holds a value for the category and a reference to the next category in
    '* the list of categories as well as a reference to the first Domain object.
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


    '    PCat=^Cat;
    '    cat = Record
    '            Category:string[80];
    '            NextCat:PCat;
    '            FirstDom:Ptotal;
    '        end;

    Public Category As Object
    Public NextCat As CSCategory
    Public FirstDom As CSTotal
End Class