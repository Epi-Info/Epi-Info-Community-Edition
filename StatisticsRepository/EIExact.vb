Option Strict Off
Option Explicit On
Option Compare Text
<System.Runtime.InteropServices.ProgId("cExact_NET.cExact")> Public Class EIExact

    '   This program was converted from Pascal and the old program is included as comments.
    '   In general, the DLL exports 3 functions:
    '       Strat2x2(DataArray As Variant, ConfLevel As Double, ResultArray As Variant) As Variant
    '       MatchedCC(DataArray As Variant, ConfLevel As Double, ResultArray As Variant) As Variant
    '       PersonTime(DataArray As Variant, ConfLevel As Double, ResultArray As Variant) As Variant
    '   For all 3:
    '       DataArray is a standard IEpiFace data array
    '       ResultArray is a standard IEpiface result array
    '       The return is a string containing standard text output (not HTML)
    '   Each of the 3 calls Process with its data and a data type indicator
    '   Process moves the data from the data array into Tables, an array of 2x2 tables
    '   It then calls CheckData to determine if the data is well conditioned
    '   If not, the result is an error message and Process returns
    '   If so, it then calls the routines for calculating the statistics
    '   Then it prepares the output and returns
    '
    '   Some changes from the original program are as follows:
    '   (1) All extended type have been changed to variant using doubles
    '   (2) NAN has been changed to False and INFINITY to True
    '   (3) The Format function has been renamed Reformat to avoid a name conflict
    '   (4) Since VB does not allow nested procedures, they have been de-nested and given parameters
    '   (5) Byval has been used extensively as it is the Pascal default; makes a big difference in EvalPoly, Comb, etc.

    '{----------------------------------------------------------------------------}
    '{                                                                            }
    '{  This is a bare-bones program which calculates the conditional maximum     }
    '{  likelihood estimate, exact confidence limits, and exact P-values for      }
    '{  either an an odds ratio (given a series of 2x2 tables with person-count   }
    '{  denominators) or a rate ratio (given a series of 2x2 tables with person-  }
    '{  time denominators). It utilizes an efficient algorithm for calculating    }
    '{  the coefficients of the conditional distribution as described in the      }
    '{  references. To increase execution speed, the arithmetic is performed on   }
    '{  the natural scale (not the log scale). To avoid floating point overflow,  }
    '{  extended precision (10 byte) reals are used at critical points.           }
    '{  Otherwise, double precision (8 byte) reals are used. This program is      }
    '{  released to the public domain.                                            }
    '{                                                                            }
    '{  References:                                                               }
    '{     1. Martin,D; Austin,H (1991): An efficient program for computing       }
    '{        conditional maximum likelihood estimates and exact confidence       }
    '{        limits for a common odds ratio. Epidemiology 2, 359-362.            }
    '{     2. Martin,DO; Austin,H (1996): Exact estimates for a rate ratio.       }
    '{        Epidemiology 7, 29-33.                                              }
    '{                                                                            }
    '{  Author: David O. Martin, MD, MPH                                          }
    '{                                                                            }
    '{  Send questions or comments via e-mail to domartin@aol.com                 }
    '{                                                                            }
    '{----------------------------------------------------------------------------}
    '
    '{ . $m 65520, 0, 0 : memory sizes }
    '{$i+ : i/o error checking on }
    '{$n+ : use math coprocessor }
    '{$e+ : floating point emulation on }
    '{ . $r- : range checking off }
    '{ . $s- : stack checking off }
    '
    'const
    '   MAXNTABLES = 1000;      {was 100}             { Max # "unique" 2x2 tables }
    '   MAXDEGREE  = 3000;                           { Max degree of a polynomial }
    '   MAXITER    = 100;     { Max # of iterations to bracket/converge to a root }
    '   TOLERANCE  = 1e-7;                        { Relative tolerance in results }
    '   INFINITY   = -16777211;                      { Used to represent infinity }
    '   NAN        = -16777212;                { Used to represent "Not A Number" }

    Const MAXNTABLES As Short = 1000 '      {was 100}             { Max # "unique" 2x2 tables }
    Const MAXDEGREE As Integer = 100000 '                           { Max degree of a polynomial }
    Const MAXITER As Short = 10000 '     { Max # of iterations to bracket/converge to a root }
    Const TOLERANCE As Double = 0.0000001 '                        { Relative tolerance in results }
    Const INFINITY As Boolean = True '                      { Used to represent infinity }
    Const NAN As Boolean = False '                { Used to represent "Not A Number" }

    '

    'type
    '   {-------------------------------------------------------------------------}
    '   {  Stratified case-control data, matched case-control data, and           }
    '   {  stratified person-time data are all held in a record (Rec2x2). With    }
    '   {  stratified case-control data, the record is defined so that:           }
    '   {                                                                         }
    '   {                           Exposed        Non-Exposed       Total        }
    '   {        Diseased              a                 b             m1         }
    '   {        Non-Diseased          c                 d             m0         }
    '   {        ---------------------------------------------------------        }
    '   {        Total                 n1                n0             t         }
    '   {                                                                         }
    '   {  For stratified case-control data, FREQ is set to 1. For matched case-  }
    '   {  control data, the record is defined in the same way except that FREQ   }
    '   {  corresponds to the frequency of like 2x2 tables. Note that for         }
    '   {  matched data, M1 must ALWAYS equal 1.                                  }
    '   {                                                                         }
    '   {  For stratified person-time data, the record is defined so that:        }
    '   {                                                                         }
    '   {                           Exposed        Non-Exposed       Total        }
    '   {        Diseased              a                 b             m1         }
    '   {        Person-Time           n1                n0             t         }
    '   {                                                                         }
    '   {  For stratified person-time data, FREQ is set to 1. For all types of    }
    '   {  data, the variable INFORMATIVE is TRUE if no margins of the given 2x2  }
    '   {  table are zero, otherwise INFORMATIVE is FALSE.                        }
    '   {-------------------------------------------------------------------------}
    '
    '   clsRec2x2 = class                         { Data for one "unique" 2x2 table }
    '      a, m1, n1, n0 : double;
    '      freq          : integer;
    '      informative   : boolean;
    '   end; { clsRec2x2 }

    Structure clsRec2x2
        Dim a As Double
        Dim b As Double
        Dim c As Double
        Dim d As Double
        Dim m1 As Double
        Dim n1 As Double
        Dim n0 As Double
        Dim freq As Short
        Dim informative As Boolean
    End Structure

    '
    ' //  Ar2x2 = array[1..MAXNTABLES] of Rec2x2;         { Array of 2x2 table data }
    '
    '   {-----------------------------------------------------------}
    '   { The polynomials are manipulated as arrays of coefficients }
    '   {-----------------------------------------------------------}
    '
    '   Vector = array[0..MAXDEGREE] of extended;         { Array of coefficients }
    '
    'var
    '   NumColumns, NumRows, NumStrata: Integer;  {AD}
    '   Tables    : Tlist;     {List to be filled with Table objects}
    '
    '   sumA      : longint;   { Sum of the observed "a" cells }
    '   minSumA   : longint;   { Lowest value of "a" cell sum w/ given margins }
    '   maxSumA   : longint;   { Highest value of "a" cell sum w/ given margins }
    '
    '   polyD     : Vector;    { The polynomial of conditional coefficients }
    '   degD      : integer;   { The degree of polyD }
    '
    '   value     : double;    { Used in defining Func }
    '
    '   polyN     : Vector;    { The "numerator" polynomial in Func }
    '   degN      : integer;   { The degree of polyN }

    Dim NumRows, NumColumns, NumStrata As Short ': Integer;  {AD}
    Dim Tables() As clsRec2x2 '    : Tlist;     {List to be filled with Table objects}

    Dim sumA As Integer '      : longint;   { Sum of the observed "a" cells }
    Dim minSumA As Integer '   : longint;   { Lowest value of "a" cell sum w/ given margins }
    Dim maxSumA As Integer '  : longint;   { Highest value of "a" cell sum w/ given margins }

    Dim polyD() As Object '     : Vector;    { The polynomial of conditional coefficients }
    Dim degD As Short '      : integer;   { The degree of polyD }

    Dim value As Double '    : double;    { Used in defining Func }

    Dim polyN() As Object '     : Vector;    { The "numerator" polynomial in Func }
    Dim degN As Short '      : integer;   { The degree of polyN }


    '
    '{------------------------}
    '{ A few utility routines }
    '{------------------------}
    '
    'function YesNo(question: string): boolean;
    ' { Returns true if user presses 'y' or 'Y' }
    'var
    '   ch: char;
    'begin
    '   Writeln;
    '   repeat
    '      Write(question+' [Y/N] ');
    '      Readln(ch);
    '   until UpCase(ch) in ['Y','N'];
    '   YesNo:=UpCase(ch) = 'Y';
    'end; { Yes }
    '
    'function Min(x, y: double): double;
    ' { Returns min(x, y). }
    'begin
    '   if x < y then Min:=x else Min:=y;
    'end; { Min }

    Private Function Min(ByVal x As Object, ByVal y As Object) As Double

        ' { Returns min(x, y). }
        If Not IsNumeric(x) Then
            If IsNumeric(y) Then
                'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Min = y
            Else
                Min = 0
            End If
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ElseIf x < y Then
            'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Min = x
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Min = y
        End If
    End Function

    '
    'function Max(x, y: double): double;
    ' { Returns max(x, y). }
    'begin
    '   if x > y then Max:=x else Max:=y;
    'end; { Max }
    Private Function Max(ByVal x As Object, ByVal y As Object) As Double

        ' { Returns max(x, y). }
        If Not IsNumeric(x) Then
            If IsNumeric(y) Then
                'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                Max = y
            Else
                Max = 0
            End If
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ElseIf x > y Then
            'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Max = x
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Max = y
        End If
    End Function
    '
    'function Comb(y, x: double): double;
    ' { Returns the combination y choose x. }
    'var
    '   i: integer;
    '   f: double;
    'begin
    '   f:=1.0;
    '   for i:=1 to Round(Min(x, y - x)) do begin
    '      f:=f * y / i;
    '      y:=y - 1.0;
    '   end; { for }
    '   Comb:=f;
    'end; { Comb }

    Private Function Comb(ByVal y As Double, ByVal x As Double) As Double

        ' { Returns the combination y choose x. }
        Dim i As Short
        Dim f As Double

        f = 1.0#
        For i = 1 To System.Math.Round(Min(x, y - x))
            f = f * y / i
            y = y - 1.0#
        Next i ' { for }
        Comb = f
    End Function

    '
    'function Power(x, y: double): extended;
    ' { Returns x^y. }
    'begin
    '   Power:=Exp(y*Ln(x));
    'end; { Power }
    '
    '{----------------------------------------------------------------------------}
    '{ The routine that follows is used to make sure that exact estimates         }
    '{ can be calculated. In addition, the global variables SUMA, MINSUMA,        }
    '{ and MAXSUMA are calculated.                                                }
    '{----------------------------------------------------------------------------}
    '
    'procedure CheckData(
    '       dataType  : integer;  { i: Type of data }
    '       numTables : integer;  { i: Number of "unique" 2x2 tables }
    '   var tables    : TList;    { i: Array of 2x2 table data }
    '   var error     : integer); { o: Flags error in data }
    '{
    '   This routine determines if the data allow exact estimates to be calculated.
    '   It MUST be called once prior to calling CalcPoly() given below. DATATYPE
    '   indicates the type of data to be analyzed (1 = stratified case-control,
    '   2 = matched case-control, 3 = stratified person-time). Exact estimates
    '   can only be calculated if ERROR = 0.
    '
    '   Errors : 0 = can calc exact estimates, i.e., no error,
    '            1 = too much data (MAXDEGREE too small),
    '            2 = no informative strata.
    '            3 = More than one case in a Matched Table (added July 21, 1998)
    '}
    'var
    '   i: integer;
    '   CurTable : clsRec2x2;
    'begin
    '   error:=0;
    '
    '   { Compute the global vars SUMA, MINSUMA, MAXSUMA }
    '   sumA:=0;
    '   minSumA:=0;
    '   maxSumA:=0;
    '
    '   for i:=0 to Tables.count-1 do
    '      begin {for}
    '      CurTable := Tables.items[i];
    '      with CurTable do
    '      If informative Then begin
    '         sumA:=Round(a*freq) + sumA;
    '         if dataType in [1, 2] then begin                { Case-control data }
    '            minSumA:=Round(Max(0,m1-n0) * freq) + minSumA;
    '            maxSumA:=Round(Min(m1,n1) * freq) + maxSumA;
    '         end { if }
    '         else begin                                       { Person-time data }
    '            minSumA:=0;
    '            maxSumA:=Round(m1*freq) + maxSumA;
    '         end; { else }
    '      end; { if }
    '
    '   { Check for errors }
    '   if (maxSumA - minSumA > MAXDEGREE) then                  { Poly too small }
    '      error:=1
    '   else if (minSumA = maxSumA) then                  { No informative strata }
    '      error:=2;
    '   If (dataType = 2) And (CurTable.a > 1) Then
    '      error := 3;
    '   end; {for}
    'end; { CheckData }

    'UPGRADE_NOTE: error was upgraded to error_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub CheckData(ByVal DataType As Short, ByVal numTables As Short, ByRef Tables() As clsRec2x2, ByRef error_Renamed As Short)

        '       dataType  : integer;  { i: Type of data }
        '       numTables : integer;  { i: Number of "unique" 2x2 tables }
        '   var tables    : TList;    { i: Array of 2x2 table data }
        '   var error     : integer); { o: Flags error in data }
        '{
        '   This routine determines if the data allow exact estimates to be calculated.
        '   It MUST be called once prior to calling CalcPoly() given below. DATATYPE
        '   indicates the type of data to be analyzed (1 = stratified case-control,
        '   2 = matched case-control, 3 = stratified person-time). Exact estimates
        '   can only be calculated if ERROR = 0.
        '
        '   Errors : 0 = can calc exact estimates, i.e., no error,
        '            1 = too much data (MAXDEGREE too small),
        '            2 = no informative strata.
        '            3 = More than one case in a Matched Table (added July 21, 1998)
        '}
        'var
        Dim i As Short
        Dim CurTable As clsRec2x2
        'begin
        error_Renamed = 0
        '
        '   { Compute the global vars SUMA, MINSUMA, MAXSUMA }
        sumA = 0
        minSumA = 0
        maxSumA = 0
        '
        For i = 0 To UBound(Tables)
            '      begin {for}
            'UPGRADE_WARNING: Couldn't resolve default property of object CurTable. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            CurTable = Tables(i)
            With CurTable
                If .informative Then
                    sumA = System.Math.Round(.a * .freq) + sumA
                    If DataType = 1 Or DataType = 2 Then '                { Case-control data }
                        minSumA = System.Math.Round(Max(0, .m1 - .n0) * .freq) + minSumA '
                        maxSumA = System.Math.Round(Min(.m1, .n1) * .freq) + maxSumA '
                    Else '                                       { Person-time data }
                        minSumA = 0
                        maxSumA = System.Math.Round(.m1 * .freq) + maxSumA
                    End If
                End If
                '
                '   { Check for errors }
                If (maxSumA - minSumA > MAXDEGREE) Then '              { Poly too small }
                    error_Renamed = 1
                ElseIf (minSumA = maxSumA) Then  '             { No informative strata }
                    error_Renamed = 2
                ElseIf (DataType = 2) And (.a > 1) Then
                    error_Renamed = 3
                End If '; {for}
            End With
        Next i
        'end; { CheckData }
    End Sub

    '
    '{-----------------------------------------------------------------}
    '{ Two polynomial multiplication routines are given. The first is  }
    '{ a generic routine. The second uses the binomial expansion.      }
    '{-----------------------------------------------------------------}
    '
    'procedure MultPoly(
    '   var p1, p2     : Vector;    { i: Two polynomials }
    '       deg1, deg2 : integer;   { i: The degrees of the above polynomials }
    '   var p3         : Vector;    { o: The product polynomial of p1 * p2 }
    '   var deg3       : integer);  { o: The degree of the product polynomial }
    '{
    '   This routine multiplies together two polynomials P1 and P2 to obtain
    '   the product polynomial P3. Reference: 'Algorithms 2nd ed.', by R.
    '   Sedgewick (Addison-Wesley, 1988), p. 522.
    '}
    'var
    '   i, j: integer;
    'begin
    '   deg3:=deg1 + deg2;
    '   for i:=0 to deg3 do p3[i]:=0.0;
    '   for i:=0 to deg1 do
    '      for j:=0 to deg2 do
    '         p3[i+j]:=p1[i] * p2[j] + p3[i+j];
    'end; { MultPoly }
    '

    Private Sub MultPoly(ByRef p1() As Object, ByRef p2() As Object, ByRef deg1 As Short, ByRef deg2 As Short, ByRef p3() As Object, ByRef deg3 As Short)

        Dim i, j As Short
        deg3 = deg1 + deg2
        ReDim p3(deg3)
        For i = 0 To deg3
            'UPGRADE_WARNING: Couldn't resolve default property of object p3(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            p3(i) = CDbl(0)
        Next i
        For i = 0 To deg1
            For j = 0 To deg2
                'UPGRADE_WARNING: Couldn't resolve default property of object p3(i + j). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object p2(j). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object p1(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                p3(i + j) = p1(i) * p2(j) + p3(i + j)
            Next j
        Next i
    End Sub '; { MultPoly }


    'procedure BinomialExpansion(
    '       c0, c1 : extended; { i: Coefficients of the 1st degree poly c0 + c1*R }
    '       f      : integer;  { i: Power to which (c0 + c1*R) is raised }
    '   var p      : Vector;   { o: Polynomial that results from (c0 + c1*R)^f }
    '   var degP   : integer); { o: Degree of polynomial p }
    '{
    '   Outputs to P the coefficients of the binomial expansion of (C0 + C1*R)^F.
    '   An alternative to this procedure would be to multiply the polynomial
    '   (C0 + C1*R) by itself (F-1) times but using the binomial expansion is much
    '   faster.
    '}
    'var
    '   i: integer;
    'begin
    '   degP:=f;
    '   p[degP]:=Power(c1, degP);
    '   for i:=(degP-1) downto 0 do
    '      p[i]:=p[i+1] * c0 * (i+1) / (c1 * (degP-i));
    'end; { BinomialExpansion }

    Private Sub BinomialExpansion(ByRef c0 As Object, ByRef c1 As Object, ByRef f As Short, ByRef p() As Object, ByRef degp As Short)

        Dim i As Short

        degp = f
        ReDim p(degp)
        'UPGRADE_WARNING: Couldn't resolve default property of object c1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object p(degp). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        p(degp) = CDbl(c1 ^ degp)
        For i = degp - 1 To 0 Step -1
            'UPGRADE_WARNING: Couldn't resolve default property of object c1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object c0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object p(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object p(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            p(i) = p(i + 1) * c0 * CDbl(i + 1) / (c1 * CDbl(degp - i))
        Next i
    End Sub '; { BinomialExpansion }

    '
    '{--------------------------------------------------------------}
    '{ Routines are given to calculate stratum-specific polynomials }
    '{--------------------------------------------------------------}
    '
    'procedure PolyStratCC(
    '   var table  : clsRec2x2;     { i: Data from one person-count 2x2 table }
    '   var polyDi : Vector;     { o: Poly of conditional coefs due to this table }
    '   var degDi  : integer);   { o: The degree of Di }
    '{
    '   This routine outputs the stratum-specific polynomial of conditional
    '   distribution coefficients due to a SINGLE 2x2 table with person-count
    '   denominators. If the 2x2 table is uninformative, then degDi is set to 0
    '   and polyDi[0] to 1.0. Note that the coefficients are scaled so that
    '   the first coefficient is equal to 1.0.
    '}
    'var
    '   i: integer;
    '   minA, maxA, aa, bb, cc, dd: double;
    'begin
    '   degDi:=0;
    '   polyDi[0]:=1.0;
    '   if table.informative then with table do begin
    '      minA:=Max(0, m1-n0);        { Min val of the "a" cell w/ these margins }
    '      maxA:=Min(m1, n1);          { Max val of the "a" cell w/ these margins }
    '      degDi:=Round(maxA-minA);       { The degree of this table's polynomial }
    '      { The polynomial coefficients are scaled so that the first     }
    '      { coef is 1.0. Note the recursive relation between coefficients. }
    '      aa:=minA;                                { Corresponds to the "a" cell }
    '      bb:=m1-minA+1.0;                         { Corresponds to the "b" cell }
    '      cc:=n1-minA+1.0;                         { Corresponds to the "c" cell }
    '      dd:=n0-m1+minA;                          { Corresponds to the "d" cell }
    '      for i:=1 to degDi do
    '         polyDi[i]:=polyDi[i-1] * ((bb-i) / (aa+i)) * ((cc-i) / (dd+i));
    '   end; { if }
    'end; { PolyStratCC }

    Private Sub PolyStratCC(ByRef Table As clsRec2x2, ByRef polyDi() As Object, ByRef degDi As Short)

        Dim i As Short
        Dim cc, aa, minA, maxA, bb, dd As Double

        degDi = 0
        ReDim polyDi(0)
        'UPGRADE_WARNING: Couldn't resolve default property of object polyDi(0). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        polyDi(0) = CDbl(1)
        polyDi(0) = Math.Log10(CDbl(1))
        With Table
            If .informative Then
                minA = Max(0, .m1 - .n0) '        { Min val of the "a" cell w/ these margins }
                maxA = Min(.m1, .n1) '          { Max val of the "a" cell w/ these margins }
                degDi = System.Math.Round(maxA - minA) '       { The degree of this table's polynomial }
                ReDim Preserve polyDi(degDi)
                aa = minA '                                { Corresponds to the "a" cell }
                bb = .m1 - minA + 1.0# '                         { Corresponds to the "b" cell }
                cc = .n1 - minA + 1.0# '                         { Corresponds to the "c" cell }
                dd = .n0 - .m1 + minA '                          { Corresponds to the "d" cell }
                For i = 1 To degDi
                    'UPGRADE_WARNING: Couldn't resolve default property of object polyDi(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object polyDi(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    polyDi(i) = CDbl(polyDi(i - 1) * ((bb - i) / (aa + i)) * ((cc - i) / (dd + i)))
                    polyDi(i) = polyDi(i - 1) + Math.Log10(((bb - i) / (aa + i)) * ((cc - i) / (dd + i)))
                Next i
            End If
        End With
    End Sub ' { PolyStratCC }

    '
    'procedure PolyMatchCC(
    '   var table  : clsRec2x2;     { i: Data from a matched table }
    '   var polyEi : Vector;     { o: Poly due to this table }
    '   var degEi  : integer);   { o: Degree of polyEi }
    '{
    '   This routine outputs the polynomial of conditional distribution
    '   coefficients due to a single matched table. (A single matched table is
    '   generally equivalent to a number of sparse 2x2 tables.) If the table is
    '   uninformative (e.g., cases and controls all exposed), then degEi is set
    '   to 0 and polyEi[0] to 1.0.
    '}
    'var
    '   c0, c1: extended;
    'begin
    '   degEi:=0;
    '   polyEi[0]:=1.0;
    '   if table.informative then with table do begin
    '      c0:=Comb(n1, 0) * Comb(n0, m1);         { Corresponds to 0 in "a" cell }
    '      c1:=Comb(n1, 1) * Comb(n0, m1-1);       { Corresponds to 1 in "a" cell }
    '      BinomialExpansion(c0, c1, freq, polyEi, degEi);
    '   end; { if }
    'end; { PolyMatchCC }

    Private Sub PolyMatchCC(ByRef Table As clsRec2x2, ByRef polyEi() As Object, ByRef degEi As Short)

        Dim c0, c1 As Object
        degEi = 0
        ReDim polyEi(0)
        'UPGRADE_WARNING: Couldn't resolve default property of object polyEi(0). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        polyEi(0) = CDbl(1)
        With Table
            If .informative Then
                'UPGRADE_WARNING: Couldn't resolve default property of object c0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                c0 = CDbl(Comb(.n1, 0) * Comb(.n0, .m1)) '         { Corresponds to 0 in "a" cell }
                'UPGRADE_WARNING: Couldn't resolve default property of object c1. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                c1 = CDbl(Comb(.n1, 1) * Comb(.n0, .m1 - 1)) '       { Corresponds to 1 in "a" cell }
                BinomialExpansion(c0, c1, .freq, polyEi, degEi)
            End If
        End With
    End Sub

    '
    'procedure PolyStratPT(
    '   var table  : clsRec2x2;  { i: Data from one person-time 2x2 table }
    '   var polyDi : Vector;     { o: Poly of conditional coefs due to this table }
    '   var degDi  : integer);   { o: The degree of Di }
    '{
    '   This routine outputs the stratum-specific polynomial of conditional
    '   distribution coefficients due to a SINGLE 2x2 table with person-time
    '   denominators. If the 2x2 table is uninformative, then degDi is set to 0
    '   and polyDi[0] to 1.0. This routine is based on the algorithm discussed in
    '   reference #2 (see above).
    '}
    'begin
    '   degDi:=0;
    '   polyDi[0]:=1.0;
    '   if table.informative then with table do
    '      BinomialExpansion(n0/n1, 1.0, Round(m1), polyDi, degDi);
    'end; { PolyStratPT }

    Private Sub PolyStratPT(ByRef Table As clsRec2x2, ByRef polyDi() As Object, ByRef degDi As Short)

        degDi = 0
        ReDim polyDi(0)
        'UPGRADE_WARNING: Couldn't resolve default property of object polyDi(0). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        polyDi(0) = CDbl(1)
        With Table
            If .informative Then
                BinomialExpansion(CDbl(.n0 / .n1), CDbl(1), System.Math.Round(.m1), polyDi, degDi)
            End If
        End With
    End Sub

    '
    '{--------------------------------------------------------------------------}
    '{ The following routine is an alternative to the above. It is based on     }
    '{ PolyStratCC() and the idea that by letting the c and d cells of a 2x2    }
    '{ table approach infinity, the noncentral hypergeometric becomes binomial. }
    '{ Unlike the above routine, this routine scales the coefficients so that   }
    '{ the first coefficient is always 1.0.                                     }
    '{--------------------------------------------------------------------------}
    '
    '(*
    'procedure PolyStratPT(
    '   var table  : Rec2x2;     { i: Data from one person-time 2x2 table }
    '   var polyDi : Vector;     { o: Poly of conditional coefs due to this table }
    '   var degDi  : integer);   { o: The degree of Di }
    '{
    '   This routine outputs the stratum-specific polynomial of conditional
    '   distribution coefficients due to a SINGLE 2x2 table with person-time
    '   denominators. If the 2x2 table is uninformative, then degDi is set to 0
    '   and polyDi[0] to 1.0.
    '}
    'var
    '   i: integer;
    '   aa, bb: double;
    'begin
    '   degDi:=0;
    '   polyDi[0]:=1.0;
    '   if table.informative then with table do begin
    '      degDi:=Round(m1);              { The degree of this table's polynomial }
    '      { The polynomial coefficients are scaled so that the first     }
    '      { coef is 1.0. Note the recursive relation between coefficients. }
    '      aa:=0;                                   { Corresponds to the "a" cell }
    '      bb:=m1+1.0;                              { Corresponds to the "b" cell }
    '      for i:=1 to degDi do
    '         polyDi[i]:=polyDi[i-1] * ((bb-i) / (aa+i)) * (n1 / n0);
    '   end; { if }
    'end; { PolyStratPT }
    '*)
    '
    '{--------------------------------------------------------------------}
    '{ Now follows the key routine which outputs the "main" polynomial of }
    '{ conditional distribution coefficients to be used to compute exact  }
    '{ estimates.                                                         }
    '{--------------------------------------------------------------------}
    '
    'procedure CalcPoly(
    '       dataType  : integer;  { i: Type of data }
    '       numTables : integer;  { i: Number of "unique" 2x2 tables }
    '   var tables    : TList);   { i: Array of 2x2 table data }
    '{
    '   This routine outputs the "main" polynomial of conditional distribution
    '   coefficients which will subsequently be used to calculate the conditional
    '   maximum likelihood estimate, exact confidence limits, and exact P-values.
    '   For a given data set, this routine MUST be called once before calling
    '   CalcExactPVals(), CalcCmle(), and CalcExactLim(). Note that DATATYPE
    '   indicates the type of data to be analyzed (1 = stratified case-control,
    '   2 = matched case-control, 3 = stratified person-time).
    '}
    'var
    '   poly1, poly2: Vector;    { Intermediate polynomials }
    '   i, deg1, deg2: integer;  { Index; degree of polynomials p1 & p2 }
    '   CurTable : clsRec2x2;
    'begin
    '   CurTable := Tables.Items[0];
    '   case dataType of
    '      1 : PolyStratCC(CurTable, polyD, degD);     { Stratified case-control }
    '      2 : PolyMatchCC(CurTable, polyD, degD);        { Matched case-control }
    '      3 : PolyStratPT(CurTable, polyD, degD);      { Stratified person-time }
    '   end; { case }
    '
    '   { Multiply polynomials }
    '   for i:=1 to Tables.Count-1 do
    '   begin
    '      CurTable := Tables.Items[i];
    '      If CurTable.informative Then
    '      begin
    '      deg1:=degD;
    '      poly1:=polyD;
    '
    '      case dataType of
    '         1 : PolyStratCC(CurTable, poly2, deg2);  { Stratified case-control }
    '         2 : PolyMatchCC(CurTable, poly2, deg2);     { Matched case-control }
    '         3 : PolyStratPT(CurTable, poly2, deg2);   { Stratified person-time }
    '      end; { case }
    '      MultPoly(poly1, poly2, deg1, deg2, polyD, degD);
    '   end; { for  Should be IF?}
    '   end; {for AD}
    'end; { CalcPoly }

    Private Sub CalcPoly(ByVal DataType As Short, ByVal numTables As Short, ByRef Tables() As clsRec2x2)

        Dim poly1() As Object
        Dim poly2() As Object '    { Intermediate polynomials }
        Dim deg1, i, j, deg2 As Short '  { Index; degree of polynomials p1 & p2 }
        Dim CurTable As clsRec2x2

        'UPGRADE_WARNING: Couldn't resolve default property of object CurTable. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        CurTable = Tables(0)
        Select Case DataType
            Case 1 : PolyStratCC(CurTable, polyD, degD) '     { Stratified case-control }
            Case 2 : PolyMatchCC(CurTable, polyD, degD) '        { Matched case-control }
            Case 3 : PolyStratPT(CurTable, polyD, degD) '      { Stratified person-time }
        End Select

        For i = 1 To UBound(Tables)
            'UPGRADE_WARNING: Couldn't resolve default property of object CurTable. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            CurTable = Tables(i)
            If CurTable.informative Then
                deg1 = degD
                ReDim poly1(deg1)
                For j = 0 To deg1
                    'UPGRADE_WARNING: Couldn't resolve default property of object polyD(j). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object poly1(j). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    poly1(j) = polyD(j)
                Next j

                Select Case DataType
                    Case 1 : PolyStratCC(CurTable, poly2, deg2) '  { Stratified case-control }
                    Case 2 : PolyMatchCC(CurTable, poly2, deg2) '     { Matched case-control }
                    Case 3 : PolyStratPT(CurTable, poly2, deg2) '   { Stratified person-time }
                End Select
                MultPoly(poly1, poly2, deg1, deg2, polyD, degD)
            End If
        Next i
    End Sub

    '
    '{----------------------------------------------------------------------------}
    '{ Below is a routine for evaluating a polynomial. If the value at which the  }
    '{ polynomial is being evaluated is > 1.0 then the polynomial is divided      }
    '{ through by R^(degree of the poly). This helps to prevent floating point    }
    '{ overflows but must be taken into account when evaluating Func below.       }
    '{----------------------------------------------------------------------------}
    '
    'function EvalPoly(var c: Vector; degC: integer; R: double): extended;
    '{
    '   This routine returns the value of the polynomial C, a polynomial of
    '   conditional coefficients of degree DEGC, evaluated at an odds ratio or
    '   rate ratio R. If R > 1.0 then the poly evaluated is C / R^(DEGC). Horner's
    '   method is used to evaluate the polynomial.
    '}
    'var
    '   i: integer;
    '   y: extended;
    'begin
    '   If r = 0# Then
    '      y:=c[0]
    '   Else: If r <= 1# Then begin
    '      y:=c[degC];
    '      if R < 1.0
    '         then for i:=(degC-1) downto 0 do y:=y * R + c[i]
    '         else for i:=(degC-1) downto 0 do y:=y + c[i];
    '   end { else }
    '   Else: If r > 1# Then begin
    '      y:=c[0];
    '      R:=1.0 / R;
    '      for i:=1 to degC do y:=y * R + c[i];
    '   end; { else }
    '   EvalPoly:=y;
    'end; { EvalPoly }

    Private Function EvalPoly(ByRef c() As Object, ByVal degC As Short, ByVal r As Double) As Object
        Dim i As Short
        Dim y As Object
        If r = 0.0# Then
            'UPGRADE_WARNING: Couldn't resolve default property of object c(0). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            y = c(0)
        ElseIf r <= 1.0# Then
            'UPGRADE_WARNING: Couldn't resolve default property of object c(degC). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            y = c(degC)
            If r < 1.0# Then
                For i = (degC - 1) To 0 Step -1
                    'UPGRADE_WARNING: Couldn't resolve default property of object c(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    '                    y = y * CDbl(r) + c(i)
                    y = Epi.Statistics.SharedResources.addLogToLog(y + Math.Log10(CDbl(r)), c(i))
                Next i
            Else
                For i = (degC - 1) To 0 Step -1
                    'UPGRADE_WARNING: Couldn't resolve default property of object c(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    '                    y = y + c(i)
                    y = Epi.Statistics.SharedResources.addLogToLog(y, c(i))
                Next i
            End If
        ElseIf r > 1.0# Then
            'UPGRADE_WARNING: Couldn't resolve default property of object c(0). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            y = c(0)
            'Debug.Print degC, c(0), r
            r = 1.0# / r
            For i = 1 To degC
                'UPGRADE_WARNING: Couldn't resolve default property of object c(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                '                y = y * CDbl(r) + c(i)
                y = Epi.Statistics.SharedResources.addLogToLog(y + Math.Log10(CDbl(r)), c(i))
                'Debug.Print i, c(i), y
            Next i
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object EvalPoly. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        EvalPoly = y
    End Function

    '
    '{---------------------------------------------------------------------------}
    '{ Func is defined so that its zero (value at which Func = 0) is the CMLE or }
    '{ an exact confidence limit.                                                }
    '{---------------------------------------------------------------------------}
    '
    'function Func(R: double): double;
    '{
    '   The root of this function is the conditional MLE of the common odds ratio
    '   or common rate ratio, or an exact confidence limit depending on how the
    '   global variables VALUE, POLYN, and POLYD are defined.
    '}
    'var
    '   numer, denom: extended;
    'begin
    '   numer:=EvalPoly(polyN, degN, R);
    '   denom:=EvalPoly(polyD, degD, R);
    '   if R <= 1.0
    '      then Func:=numer / denom - value
    '      else Func:=(numer / Power(R, degD-degN)) / denom - value;
    'end; { Func }

    Private Function Func(ByVal r As Double) As Double
        Dim numer, denom As Object
        'UPGRADE_WARNING: Couldn't resolve default property of object EvalPoly(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object numer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        numer = EvalPoly(polyN, degN, r)
        'UPGRADE_WARNING: Couldn't resolve default property of object EvalPoly(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        denom = EvalPoly(polyD, degD, r)
        If r <= 1.0# Then
            'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object numer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Func = Math.Pow(10.0, numer - denom) - CDbl(value)
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object numer. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            Dim NumOverDenom As Double
            NumOverDenom = (numer - Math.Log10(Math.Pow(r, degD - degN))) - denom
            Func = Math.Pow(10.0, NumOverDenom) - CDbl(value)
            '            Func = (numer / CDbl(r ^ (degD - degN))) / denom - CDbl(value)
        End If
    End Function

    '
    '{----------------------------------------------------------------------------}
    '{ The routines that follow (BracketRoot, Zero, and Converge) locate a zero   }
    '{ to the function Func above.                                                }
    '{----------------------------------------------------------------------------}
    '
    'procedure BracketRoot(
    '       approx : double;    { i: Approximation to the root (> 0) }
    '   var x0, x1 : double;    { o: Bounds to root }
    '   var f0, f1 : double);   { o: Values of Func(x0) and Func(x1) }
    '{
    '   Given a positive non-zero starting value APPROX, this routine searches for
    '   a bracket to the root of the function Func on the interval [0, infinity)
    '   so that on output F0 * F1 <= 0 which guarantees that a root lies in the
    '   interval [X0, X1].
    '}
    'var
    '   iter: integer;
    'begin
    '   iter:=0;
    '   x1:=Max(0.5, approx);                             { X1 is the upper bound }
    '   x0:=0.0;                                          { X0 is the lower bound }
    '   f0:=Func(x0);                                                { Func at X0 }
    '   f1:=Func(x1);                                                { Func at X1 }
    '   { If necessary, increase X1 until F1 and F0 have different signs }
    '   while (f1 * f0 > 0.0) and (iter < MAXITER) do begin
    '      iter:=iter + 1;
    '      x0:=x1;
    '      f0:=f1;
    '      x1:=x1 * 1.5 * iter;
    '      f1:=Func(x1);
    '   end; { while }
    'end; { BracketRoot }

    Private Sub BracketRoot(ByVal approx As Object, ByRef x0 As Double, ByRef x1 As Double, ByRef f0 As Double, ByRef f1 As Double)
        Dim iter As Short

        iter = 0
        x1 = Max(0.5, approx) '                             { X1 is the upper bound }
        x0 = 0.0# '                                          { X0 is the lower bound }
        f0 = Func(x0) '                                                { Func at X0 }
        f1 = Func(x1) '                                                { Func at X1 }
        Do While (f1 * f0 > 0.0#) And (iter < MAXITER)
            iter = iter + 1
            x0 = x1
            f0 = f1
            x1 = x1 * 1.5 * iter
            f1 = Func(x1)
            'Debug.Print iter, Format$(x0, "0.000"), Format$(x1, "0.000"), Format$(f0, "0.000"), Format$(f1, "0.000")
        Loop
    End Sub

    '
    'procedure Zero(
    '       x0, x1 : double;    { i: The root must lie b/w these }
    '       f0, f1 : double;    { i: The function values at X0 and X1 }
    '   var root   : double;    { o: A zero to the function Func }
    '   var error  : integer);  { o: Error flag }
    '{
    '   This procedure returns a single real root of the function Func on the
    '   interval [X0, X1] to within a relative tolerance TOLERANCE. The procedure
    '   implements an elegant modified regula falsi algorithm (the Pegasus
    '   modification). Brent's method for root solving is slightly faster but more
    '   complex.
    '   Errors: 0 = no error,
    '           1 = X0 and X1 don't bracket the root (i.e. F0 * F1 > 0),
    '           2 = root not found in MAXITER iterations.
    'Reference:
    '      Jarrat, P., A review of methods for solving non-linear algebraic
    '      equations in one variable, in Rabinowitz, P. (ed.), Numerical Methods
    '      for Nonlinear Algebraic Equations, 1973, Gordon & Breach, Science
    '      Publ., New York.
    '}
    'var
    '   found: boolean;              { Flags that a root has been found }
    '   x2, f2, swap: double;        { Newest point, Func(X2), storage variable }
    '   iter: integer;               { Current # of iterations }
    'begin
    '   error:=0;                                                    { Initialize }
    '   iter:=0;
    '   if Abs(f0) < Abs(f1) then begin             { Make X1 best approx to root }
    '      swap:=x0; x0:=x1; x1:=swap;
    '      swap:=f0; f0:=f1; f1:=swap;
    '   end; { if }
    '   found:=(f1 = 0.0);                                        { Test for root }
    '   If Not (found) And (f0 * f1 > 0#) Then
    '      error:=1;                                         { Root not bracketed }
    '   { Converge to root }
    '   while not(found) and (iter < MAXITER) and (error = 0) do begin
    '      iter:=iter + 1;
    '      x2:=x1 - f1 * (x1 - x0) / (f1 - f0);
    '      f2:=Func(x2);
    '      if f1 * f2 < 0.0 then begin                          { X0 not retained }
    '         x0:=x1;
    '         f0:=f1;
    '      end { if }
    '      else                                        { X0 retained => modify F0 }
    '         f0:=f0 * f1 / (f1 + f2);                 { The Pegasus modification }
    '      x1:=x2;
    '      f1:=f2;
    '      found:=(Abs(x1 - x0) < Abs(x1) * TOLERANCE) or (f1 = 0.0);
    '   end; { while }
    '   root:=x1;                                                { Estimated root }
    '   If Not (found) And (iter >= MAXITER) And (error = 0) Then
    '      error:=2;                                        { Too many iterations }
    'end; { Zero }

    'UPGRADE_NOTE: error was upgraded to error_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub Zero(ByVal x0 As Double, ByVal x1 As Double, ByVal f0 As Double, ByVal f1 As Double, ByRef root As Object, ByRef error_Renamed As Short)

        Dim found As Boolean '              { Flags that a root has been found }
        Dim f2, x2, swap As Double '        { Newest point, Func(X2), storage variable }
        Dim iter As Short '               { Current # of iterations }

        error_Renamed = 0 '                                                    { Initialize }
        iter = 0
        If System.Math.Abs(f0) < System.Math.Abs(f1) Then '             { Make X1 best approx to root }
            swap = x0 : x0 = x1 : x1 = swap
            swap = f0 : f0 = f1 : f1 = swap
        End If
        found = (f1 = 0.0#) '                                        { Test for root }
        If Not (found) And (f0 * f1 > 0.0#) Then
            error_Renamed = 1 '                                         { Root not bracketed }
        End If
        Do While Not (found) And (iter < MAXITER) And (error_Renamed = 0)
            iter = iter + 1
            x2 = x1 - f1 * (x1 - x0) / (f1 - f0)
            f2 = Func(x2)
            If f1 * f2 < 0.0# Then '                          { X0 not retained }
                x0 = x1
                f0 = f1
            Else '                { X0 retained => modify F0 }
                f0 = f0 * f1 / (f1 + f2) '                 { The Pegasus modification }
            End If
            x1 = x2
            f1 = f2
            found = (System.Math.Abs(x1 - x0) < System.Math.Abs(x1) * TOLERANCE) Or (f1 = 0.0#)
        Loop
        'UPGRADE_WARNING: Couldn't resolve default property of object root. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        root = x1 '                                                { Estimated root }
        If Not (found) And (iter >= MAXITER) And (error_Renamed = 0) Then error_Renamed = 2 '                                        { Too many iterations }
    End Sub

    '
    'procedure Converge(
    '       approx : double;    { i: An approximation to the root }
    '   var root   : double;    { o: The estimated root }
    '   var error  : integer);  { o: Error code as defined in proc Zero above }
    '{
    '   This routine returns the root of Func above on the interval [0, infinity).
    '}
    'var
    '   x0, x1, f0, f1: double;
    'begin
    '   BracketRoot(approx, x0, x1, f0, f1);
    '   Zero(x0, x1, f0, f1, root, error);
    'end; { Converge }
    '

    'UPGRADE_NOTE: error was upgraded to error_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
    Private Sub Converge(ByVal approx As Object, ByRef root As Object, ByRef error_Renamed As Short)
        Dim f0, x0, x1, f1 As Double
        BracketRoot(approx, x0, x1, f0, f1)
        Zero(x0, x1, f0, f1, root, error_Renamed)
    End Sub

    '{---------------------------------------------------------------------------}
    '{ The routines that follow return the exact P-values, the CMLE, or an exact }
    '{ confidence limit.                                                         }
    '{---------------------------------------------------------------------------}
    '
    'procedure CalcExactPVals(
    '   var upFishPVal : double;  { o: The upper exact Fisher P-value }
    '   var loFishPVal : double;  { o: The lower exact Fisher P-value }
    '   var upMidPPVal : double;  { o: The upper exact mid-P P-value }
    '   var loMidPPVal : double); { o: The lower exact mid-P P-value }
    '{
    '   This routine returns the exact P-values as defined in 'Modern
    '   Epidemiology ' by K. J. Rothman (Little, Brown, and Co., 1986).
    '}
    'var
    '   i, diff: Longint;        { Index; sumA - minSumA }
    '   upTail, denom: extended; { Upper tail; the whole distribution }
    'begin
    '   diff:=sumA - minSumA;
    '   upTail:=polyD[degD];
    '   for i:=degD-1 downto diff do upTail:=upTail + polyD[i];
    '   denom:=upTail;
    '   for i:=diff-1 downto 0 do denom:=denom + polyD[i];
    '   upFishPVal:=upTail / denom;
    '   loFishPVal:=1.0 - (upTail - polyD[diff]) / denom ;
    '   upMidPPVal:=(upTail - 0.5 * polyD[diff]) / denom;
    '   loMidPPVal:=1.0 - upMidPPVal;
    'end; { CalcExactPVals }

    Private Sub CalcExactPVals(ByRef upFishPVal As Object, ByRef loFishPVal As Object, ByRef upMidPPVal As Object, ByRef loMidPPval As Object, ByRef twoFishPVal As Object)
        Dim i, diff As Integer '        { Index; sumA - minSumA }
        Dim upTail, pointP, twoTail, denom As Object ' { Upper tail; the whole distribution }

        diff = sumA - minSumA
        'UPGRADE_WARNING: Couldn't resolve default property of object polyD(degD). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object upTail. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        upTail = polyD(degD)
        twoTail = Math.Log10(0)
        If upTail <= Math.Log10(1.000001) + polyD(diff) Then
            twoTail = Epi.Statistics.SharedResources.addLogToLog(twoTail, upTail)
            '            twoTail = twoTail + upTail
        End If
        For i = degD - 1 To diff Step -1
            'UPGRADE_WARNING: Couldn't resolve default property of object polyD(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object upTail. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            '            upTail = upTail + polyD(i)
            upTail = Epi.Statistics.SharedResources.addLogToLog(upTail, polyD(i))
            If polyD(i) <= Math.Log10(1.000001) + polyD(diff) Then
                '                twoTail = twoTail + polyD(i)
                twoTail = Epi.Statistics.SharedResources.addLogToLog(twoTail, polyD(i))
            End If
        Next i
        'UPGRADE_WARNING: Couldn't resolve default property of object upTail. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        denom = upTail
        For i = diff - 1 To 0 Step -1
            'UPGRADE_WARNING: Couldn't resolve default property of object polyD(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            '            denom = denom + polyD(i)
            denom = Epi.Statistics.SharedResources.addLogToLog(denom, polyD(i))
            If polyD(i) <= Math.Log10(1.000001) + polyD(diff) Then
                '                twoTail = twoTail + polyD(i)
                twoTail = Epi.Statistics.SharedResources.addLogToLog(twoTail, polyD(i))
            End If
        Next i
        'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object upTail. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object upFishPVal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        upFishPVal = Math.Pow(10.0, upTail - denom)
        twoFishPVal = Math.Pow(10.0, twoTail - denom)
        'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object polyD(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object upTail. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object loFishPVal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '        loFishPVal = 1.0# - (upTail - polyD(diff)) / denom
        loFishPVal = 1.0# - (Math.Pow(10.0, upTail - denom) - Math.Pow(10.0, polyD(diff) - denom))
        'UPGRADE_WARNING: Couldn't resolve default property of object denom. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object polyD(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object upTail. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object upMidPPVal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        '        upMidPPVal = (upTail - 0.5 * polyD(diff)) / denom
        upMidPPVal = Math.Pow(10.0, upTail - denom) - Math.Pow(10.0, (Math.Log10(0.5) + polyD(diff)) - denom)
        'UPGRADE_WARNING: Couldn't resolve default property of object upMidPPVal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object loMidPPval. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        loMidPPval = 1.0# - upMidPPVal
    End Sub

    '
    'procedure CalcCmle(
    '       approx : double;   { i: An approximation to the cmle }
    '   var cmle   : double);  { o: Estimated conditional MLE }
    '{
    '   This routine returns the conditional maximum likelihood estimate of the
    '   common odds ratio or common rate ratio. APPROX may be set to 1.0 if no
    '   estimate is available, but the solution is obtained faster with a good
    '   approximation. CMLE returns as NAN if convergence to a solution did not
    '   occur in MAXITER iterations.
    '}
    '   procedure GetCmle;
    '   var
    '      i, error: integer;
    '   begin
    '      value:=sumA;                       { The sum of the observed "a" cells }
    '      degN:=degD;                       { Degree of the numerator polynomial }
    '      for i:=0 to degN do                 { Defines the numerator polynomial }
    '         polyN[i]:=(minSumA+i) * polyD[i];
    '      Converge(approx, cmle, error);         { Solves so that Func(cmle) = 0 }
    '      if error <> 0 then cmle:=NAN;                     { Failed convergence }
    '   end; { GetCmle }
    '
    'begin { CalcCmle }
    '   if (minSumA < sumA) and (sumA < maxSumA) then   { Can calc point estimate }
    '      GetCmle
    '   else if (sumA = minSumA) then                        { Point estimate = 0 }
    '      cmle:=0.0
    '   else if (sumA = maxSumA) then                      { Point estimate = inf }
    '      cmle:=INFINITY;
    'end; { CalcCmle }

    Private Sub CalcCmle(ByVal approx As Double, ByRef cmle As Object)

        If (minSumA < sumA) And (sumA < maxSumA) Then '  { Can calc point estimate }
            GetCmle(approx, cmle)
        ElseIf (sumA = minSumA) Then  '                        { Point estimate = 0 }
            'UPGRADE_WARNING: Couldn't resolve default property of object cmle. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cmle = 0.0#
        ElseIf (sumA = maxSumA) Then  '                      { Point estimate = inf }
            'UPGRADE_WARNING: Couldn't resolve default property of object cmle. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            cmle = INFINITY
        End If
    End Sub

    Private Sub GetCmle(ByRef approx As Double, ByRef cmle As Object)
        'UPGRADE_NOTE: error was upgraded to error_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim i, error_Renamed As Short
        value = sumA '                       { The sum of the observed "a" cells }
        degN = degD '                       { Degree of the numerator polynomial }
        ReDim polyN(degN)
        For i = 0 To degN '                 { Defines the numerator polynomial }
            'UPGRADE_WARNING: Couldn't resolve default property of object polyD(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object polyN(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            polyN(i) = Math.Log10(CDbl(minSumA + i)) + polyD(i)
        Next i
        Converge(approx, cmle, error_Renamed) '         { Solves so that Func(cmle) = 0 }
        'UPGRADE_WARNING: Couldn't resolve default property of object cmle. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If error_Renamed <> 0 Then cmle = NAN '                     { Failed convergence }
    End Sub


    '
    'procedure CalcExactLim(
    '       pbLower     : boolean;  { i: TRUE => Lower Limit; FALSE => upper Limit }
    '       pbFisher    : boolean;  { i: TRUE => Fisher Limit; FALSE => mid-P Limit }
    '       pvApprox    : double;   { i: An approximation to the Limit }
    '       pnConfLevel : double;   { i: Confidence level, e.g., 0.95 for a 95% CL }
    '   var pnLimit     : double);  { o: Estimated confidence Limit }
    '{
    '   This routine returns an exact confidence pnLimit for the common odds ratio
    '   or common rate ratio with the level of confidence determined by pnConfLevel
    '   which *must* satisfy 0 <= pnConfLevel < 1. pvApprox may be set to 1.0 if no
    '   estimate is available, but the solution is obtained faster with a good
    '   approximation. pnLimit returns as NAN if convergence to a solution did not
    '   occur in MAXITER iterations.
    '}
    '   procedure GetExactLim;
    '   var
    '      error: integer;
    '   begin
    '      if pbLower
    '         then value:=0.5*(1.0+pnconfLevel)                   { = 1 - alpha / 2 }
    '         else value:=0.5*(1.0-pnConfLevel);                      { = alpha / 2 }
    '      if pbLower and pbFisher                         { Degree of numerator poly }
    '         then degN:=sumA - minSumA - 1
    '         else degN:=sumA - minSumA;
    '      polyN:=polyD;                             { degN<>degD => polyN<>polyD }
    '      If Not (pbFisher) Then
    '         polyN[degN]:=0.5 * polyD[degN];                  { Mid-P adjustment }
    '      Converge(pvApprox, pnLimit, error);       { Solves so that Func(pnLimit) = 0 }
    '      if error <> 0 then pnLimit:=NAN;                    { Failed convergence }
    '   end; { GetExactLim }
    '
    'begin { CalcExactLim }
    '   If (minSumA < sumA) And (sumA < maxSumA) Then
    '      GetExactLim
    '   else if (sumA = minSumA) then     { Point estimate = 0 => pbLower pnLimit = 0 }
    '      if pbLower then pnLimit:=0.0 else GetExactLim
    '   else if (sumA = maxSumA) then  { Point estimate = inf => upper pnLimit = inf}
    '      if not(pbLower) then pnLimit:=INFINITY else GetExactLim;
    'end; { CalcExactLim }

    Private Sub CalcExactLim(ByRef pbLower As Boolean, ByRef pbFisher As Boolean, ByVal pvApprox As Object, ByVal pnConfLevel As Double, ByRef pnLimit As Object)

        If (minSumA < sumA) And (sumA < maxSumA) Then
            GetExactLim(pbLower, pbFisher, pvApprox, pnConfLevel, pnLimit)
        ElseIf (sumA = minSumA) Then  '   { Point estimate = 0 => pbLower pnLimit = 0 }
            If pbLower Then
                'UPGRADE_WARNING: Couldn't resolve default property of object pnLimit. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                pnLimit = 0.0#
            Else
                GetExactLim(pbLower, pbFisher, pvApprox, pnConfLevel, pnLimit)
            End If
        ElseIf (sumA = maxSumA) Then  '{ Point estimate = inf => upper pnLimit = inf}
            If Not (pbLower) Then
                'UPGRADE_WARNING: Couldn't resolve default property of object pnLimit. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                pnLimit = INFINITY
            Else
                GetExactLim(pbLower, pbFisher, pvApprox, pnConfLevel, pnLimit)
            End If
        End If
    End Sub

    Private Sub GetExactLim(ByRef pbLower As Boolean, ByRef pbFisher As Boolean, ByRef pvApprox As Object, ByRef pnConfLevel As Double, ByRef pvLimit As Object) '

        'UPGRADE_NOTE: error was upgraded to error_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim i, error_Renamed As Short '

        If pbLower Then
            value = 0.5 * (1.0# + pnConfLevel) '{ = 1 - alpha / 2 }
        Else
            value = 0.5 * (1.0# - pnConfLevel) '                      { = alpha / 2 }
        End If
        If pbLower And pbFisher Then '                        { Degree of numerator poly }
            degN = sumA - minSumA - 1
        Else
            degN = sumA - minSumA '
        End If
        ReDim polyN(degN)
        For i = 0 To degN
            'UPGRADE_WARNING: Couldn't resolve default property of object polyD(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object polyN(i). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            polyN(i) = polyD(i) '                             { degN<>degD => polyN<>polyD }
        Next i
        If Not (pbFisher) Then
            'UPGRADE_WARNING: Couldn't resolve default property of object polyD(degN). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'UPGRADE_WARNING: Couldn't resolve default property of object polyN(degN). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            polyN(degN) = Math.Log10(CDbl(0.5)) + polyD(degN) '                  { Mid-P adjustment }
        End If
        Converge(pvApprox, pvLimit, error_Renamed) '       { Solves so that Func(pvLimit) = 0 }
        'UPGRADE_WARNING: Couldn't resolve default property of object pvLimit. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If error_Renamed <> 0 Then pvLimit = NAN '                    { Failed convergence }
    End Sub

    '
    'var
    '  // i         : integer;   { Index }
    '   b, c, d   : double;    { b, c, d cells of 2x2 table }
    '//   dataType  : integer;   { Indicates type of data }
    '   numTables : integer;   { Number of "unique" 2x2 tables }
    '   //tables    : TList;     { Array of 2x2 table data AD}
    '   confLevel : double;    { Example: 0.95 for a 95% confidence interval }
    '   cmle      : double;    { Odds Ratio (cond. max. likelihood estimate) }
    '   loFishLim : double;    { lower exact Fisher confidence limit }
    '   upFishLim : double;    { Upper exact Fisher confidence limit }
    '   loMidPLim : double;    { lower mid-P confidence limit }
    '   upMidPLim : double;    { Upper mid-P confidence limit }
    '   loFishP   : double;    { lower exact Fisher P-value }
    '   upFishP   : double;    { Upper exact Fisher P-value }
    '   loMidPP   : double;    { lower exact mid-P P-value }
    '   upMidPP   : double;    { Upper exact mid-P P-value }
    '   approx    : double;    { An approximation to the exact estimate }
    '   error     : integer;   { Error in procedure CheckData }
    '


    'function DPlaces(r: double): integer;
    ' { Determines the # of decimal places for various estimates. In general,
    '   5 significant figures will be shown. }
    'begin
    '   If Abs(r) >= 10000 Then
    '      DPlaces:=0
    '   else if Abs(r) >= 1000 then
    '      DPlaces:=1
    '   else if Abs(r) >= 100 then
    '      DPlaces:=2
    '   else if Abs(r) >= 10 then
    '      DPlaces:=3
    '   else if (Abs(r) >= 0.01) or (r = 0) then
    '      DPlaces:=4
    '   Else
    '      DPlaces:=-1;
    'end; { DPlaces }

    Private Function DPlaces(ByRef r As Object) As Short
        'UPGRADE_WARNING: Couldn't resolve default property of object r. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If System.Math.Abs(r) >= 10000 Then
            DPlaces = 0
            'UPGRADE_WARNING: Couldn't resolve default property of object r. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ElseIf System.Math.Abs(r) >= 1000 Then
            DPlaces = 1
            'UPGRADE_WARNING: Couldn't resolve default property of object r. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ElseIf System.Math.Abs(r) >= 100 Then
            DPlaces = 2
            'UPGRADE_WARNING: Couldn't resolve default property of object r. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ElseIf System.Math.Abs(r) >= 10 Then
            DPlaces = 3
            'UPGRADE_WARNING: Couldn't resolve default property of object r. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        ElseIf (System.Math.Abs(r) >= 0.01) Or (r = 0) Then
            DPlaces = 4
        Else
            DPlaces = -1
        End If
    End Function

    '
    'function Format(x: double): string;
    ' { Formats various estimates }
    'var
    '   s: string;
    '   posn: byte;
    'begin
    '   If x = INFINITY Then
    '      s:='undefined'
    '   else if x = NAN then
    '      s:='n/a'
    '   Else: begin
    '      Str(x:0:DPlaces(x), s);
    '      if DPlaces(x) = -1 then begin         { Remove two zeros from exponent }
    '         posn:=Pos('E', s);
    '         if s[posn+2] = '0' then Delete(s, posn+2, 1);
    '         if s[posn+2] = '0' then Delete(s, posn+2, 1);
    '      end; { if }
    '   end; { else }
    '   while s[1] = ' ' do Delete(s, 1, 1);                  { Remove whitespace }
    '   while s[Length(s)] = ' ' do Delete(s, Length(s), 1);  {   "         "     }
    '   Format:=' '+s+' ';
    'end; { Format }

    Private Function Reformat(ByRef x As Object) As String '

        Dim s As String '
        Dim posn As Short '
        'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
        If VarType(x) = VariantType.Boolean Then
            'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            s = IIf(x, "Undefined", "N/A")
        Else
            posn = DPlaces(x)
            If posn = -1 Then
                'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                s = VB6.Format(x, "0.00E+00")
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                s = VB6.Format(x, Right("######0." & Left("0000", posn), 9))
            End If
        End If ' { else }
        Reformat = " " & s & " " '
    End Function

    '
    'function FormatP(pVal: double): string;
    ' { Formats P-values }
    'var
    '   s: string;
    'begin
    '   If pVal > 0.0001 Then
    '      Str(pVal:0:5, s)
    '   else if pVal >= 1e-7 then
    '      s:=Format(pVal)
    '   Else
    '      s:='< 1E-07';
    '   while s[1] = ' ' do Delete(s, 1, 1);                  { Remove whitespace }
    '   while s[Length(s)] = ' ' do Delete(s, Length(s), 1);  {   "         "     }
    '   FormatP:=' '+s+' ';
    'end; { FormatP }
    '
    Private Function FormatP(ByRef pVal As Object) As String
        Dim s As String
        If pVal > 0.0001 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object pVal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            s = VB6.Format(pVal, "0.0000")
        ElseIf pVal >= 0.0000001 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object pVal. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            s = VB6.Format(pVal, "0.0E+00")
        Else
            s = "<1.0E-07"
        End If
        FormatP = " " & s & " " '
    End Function


    'Function Process(pvaDataArray:OleVariant; Var pvaResultArray:OleVariant;DataType:Integer;
    '                 pnConfLevel:Double):String;
    ' {Uses pvaDataArray as the source of data}
    'Var  i:Integer;
    '     ResultString: String;
    '     Table:clsRec2x2;
    '     ResultArr: Variant;
    '     errornum : Integer;
    ' { Perform calculations, and display the results }
    'begin
    '   {-------------------------------------------------------------------------}
    '   {  Determine the type of data to be inputted and then input the           }
    '   {  confidence level for interval estimates, the number of "unique" 2x2    }
    '   {  tables to be analyzed, and the 2x2 table data.                         }
    '   {-------------------------------------------------------------------------}
    '   (*  Remove data entry portion  AD
    '   AddNewline (pvaResultArray);
    '   Writeln('TYPE OF DATA:');
    '   Writeln('ÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ');
    '   Writeln('  1. Stratified case-control.');
    '   Writeln('  2. Matched case-control.');
    '   Writeln('  3. Stratified person-time.');
    '   Writeln;
    '   repeat
    '      Write('Selection (1-3)? ');
    '      Readln(dataType);
    '   until dataType in [1..3];;
    '
    '   Writeln;
    '   repeat
    '      Write('Confidence level (e.g. 0.95 for a 95% CI)? ');
    '      Readln(pnConfLevel);
    '   until (pnConfLevel >= 0.0) and (pnConfLevel < 1.0);
    '
    '   Writeln;
    '   repeat
    '      Write('Number of tables (max # allowed = ', MAXNTABLES, ')? ');
    '      Readln(numTables);
    '   until (numTables > 0) and (numTables <= MAXNTABLES);
    '
    '   Writeln;
    '   Writeln('ÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ');
    '   Writeln('  Data should be entered as in the following 2x2 table:');
    '   Writeln;
    '   if dataType in [1, 2] then begin                      { Case-control data }
    '      Writeln('                      Exposed   Unexposed');
    '      Writeln('             Cases       A          B');
    '      Writeln('             Controls    C          D');
    '      If dataType = 2 Then begin
    '         { Prompt for frequency only with matched case-control data }
    '         AddNewline (pvaResultArray);
    '         Writeln('  NOTE: A+B must equal 1. FREQUENCY is');
    '         Writeln('  NOTE: the number of like 2x2 tables.');
    '      end; { if }
    '   end { if }
    '   else if dataType = 3 then begin                        { Person-time data }
    '      Writeln('                        Exposed   Unexposed');
    '      Writeln('            Cases          A          B');
    '      Writeln('            Person-Time    Na         Nb');
    '   end; { else }
    '   Writeln('ÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ');
    '
    '   Writeln;
    '   Writeln('Type in 2x2 table data, separated by a space (not a comma):');
    '   for i:=1 to numTables do begin
    '      if dataType = 1 then begin                   { Stratified case-control }
    '         Write('  Table ', i, ' (A B C D): ');
    '         Readln(tables[i].a, b, c, d);
    '         tables[i].freq:=1;
    '         tables[i].m1:=tables[i].a + b; { # cases }
    '         tables[i].n1:=tables[i].a + c; { # exposed }
    '         tables[i].n0:=b + d;           { # unexposed }
    '         tables[i].informative:=(tables[i].a * d <> 0) or (b * c <> 0);
    '      end { if }
    '      else if dataType = 2 then begin                 { Matched case-control }
    '         repeat
    '            Write('  Table ', i, ' (A C D FREQUENCY): ');
    '            Readln(tables[i].a, c, d, tables[i].freq);
    '            if tables[i].a <= 1 then
    '               b:=1 - tables[i].a
    '            Else: begin
    '               AddNewline (pvaResultArray);
    '               Writeln('ERROR: The "A" cell cannot be more than 1.');
    '               AddNewline (pvaResultArray);
    '            end; { else }
    '         until (tables[i].a <= 1);
    '         tables[i].m1:=tables[i].a + b; { # cases }
    '         tables[i].n1:=tables[i].a + c; { # exposed }
    '         tables[i].n0:=b + d;           { # unexposed }
    '         tables[i].informative:=(tables[i].a * d <> 0) or (b * c <> 0);
    '      end { else }
    '      else if dataType = 3 then begin               { Stratified person-time }
    '         Write('  Table ', i, ' (A B Na Nb): ');
    '         Readln(tables[i].a, b, tables[i].n1, tables[i].n0);
    '         tables[i].freq:=1;
    '         tables[i].m1:=tables[i].a + b; { # cases }
    '         tables[i].informative:=(tables[i].a * tables[i].n0 <> 0) or
    '                                (b * tables[i].n1 <> 0);
    '      end; { else }
    '   end; { for }
    '    {End of data entry portion} *)
    '
    '    {----------------------------------------------------}
    '    {Transfer data from Epi Info 3-dimensional data array into
    '     the array of table records used by the routines in this
    '     module.  The dimensions of the array are determined from
    '     the array itself.  AD 01/07/98}
    '    {-----------------------------------------------------}
    '
    '   { Initialize output string and output array.}
    '    ResultString := '';
    '    ResultArr:= VarArrayCreate([1, 2, 1, 1], varVariant);
    '    Tables := TList.Create;
    '
    '   {Get number of Columns, Rows, and strata from DataArray}
    '      If VarArrayDimCount(pvaDataArray) <> 3 Then
    '         begin
    '           AddToString(ResultString, 'pvaDataArray must have 3 dimensions');
    '           exit;
    '         end;
    '      NumColumns := VarArrayHighBound(pvaDataArray,1);
    '      NumRows:= VarArrayHighBound(pvaDataArray,2);
    '      NumStrata := VarArrayHighBound(pvaDataArray, 3);
    '
    '     {For each table, transfer the data and set .Informative and
    '       .Freq}
    '
    '      for i:=1 to NumStrata do begin
    '       Table := clsRec2x2.create;
    '       if dataType = 1 then begin                    { Stratified case-control }
    '         Table.a :=DataArray [1, 1, i];
    '         b := pvaDataArray [2, 1, i];
    '         c := pvaDataArray [1, 2, i];
    '         d := pvaDataArray [2, 2, i];
    '
    '         table.freq:=1;
    '         table.m1:=table.a + b; { # cases }
    '         table.n1:=table.a + c; { # exposed }
    '         table.n0:=b + d;           { # unexposed }
    '         table.informative:=(table.a * d <> 0) or (b * c <> 0);
    '
    '      end { if }
    '      else if dataType = 2 then begin                 { Matched case-control }
    '
    '         // Repeat
    '           // Write('  Table ', i, ' (A C D FREQUENCY): ');
    '           // Readln(tables[i].a, c, d, tables[i].freq);
    '
    '           {Note: Saving this for later.  Must get data from
    '           data array}
    '             Table.a :=DataArray [1, 1, i];
    '             c := pvaDataArray [2, 1, i];
    '             d := pvaDataArray [1, 2, i];
    '             table.freq := pvaDataArray [2, 2, i];
    '
    '            If Table.a <= 1 Then
    '               b:=1 - table.a
    '            Else: begin
    '
    '               //Error will be caught during CheckData and flagged as error 3.
    '             //  Writeln;
    '              // Writeln('ERROR: The "A" cell cannot be more than 1.');
    '             //  Writeln;
    '            end; { else }
    '        // until (table.a <= 1);
    '
    '         table.m1:=table.a + b; { # cases }
    '         table.n1:=table.a + c; { # exposed }
    '         table.n0:=b + d;           { # unexposed }
    '         table.informative:=(table.a * d <> 0) or (b * c <> 0);
    '      end { else }
    '      else if dataType = 3 then begin               { Stratified person-time }
    '         {Write('  Table ', i, ' (A B Na Nb): ');
    '         Readln(      }
    '         table.a :=   pvaDataArray [1, 1, i];
    '         b :=   pvaDataArray [2, 1, i];
    '         table.n1 :=  pvaDataArray [1, 2, i];
    '         table.n0 :=  pvaDataArray [2, 2, i];
    '         table.freq := 1;
    '         table.m1 := table.a + b; { # cases }
    '         table.informative := (table.a * table.n0 <> 0) or
    '                                (b * table.n1 <> 0);
    '      end; { else }
    '      Tables.add(Table);  {Add table to the Tables list}
    '   end; { for }
    '
    '
    '
    '   {----------------------------------------------------}
    '   { Make sure that exact calculations can be performed }
    '   {----------------------------------------------------}
    '
    '   CheckData(dataType, numStrata, tables, errornum);
    '
    '   {----------------------------------------------------------------------}
    '   { If data not OK then write an error message and terminate program. }
    '   {----------------------------------------------------------------------}
    '
    '   If errornum <> 0 Then
    '   begin                     { Can't calc exact estimates }
    '      AddNewline (ResultString);
    '      case errornum of
    '1:           begin
    '             AddToString (ResultString, 'PROBLEM: Too much data. Cannot perform exact calculations.');
    '             AddNewline (ResultString);
    '             AddToArray(ResultArr,'ERROR',1);
    '             end;
    '2:           begin
    '             AddToString (ResultString, 'PROBLEM: All tables have zero marginals. Cannot perform exact calculations.');
    '             AddNewline (ResultString);
    '             AddToArray(ResultArr,'ERROR',2);
    '             end;
    '         3 : begin     {added July 21, 1998}
    '             AddToString (ResultString, 'PROBLEM: Must have only one case in each table for exact calculations.');
    '             AddNewline (ResultString);
    '             AddToArray(ResultArr,'ERROR',3);
    '             End
    '      end; { case }
    '      //exit;
    '   end {if error <> 0}
    '   else { if }
    '    Begin {When error = 0}
    '   {-----------------------------------------}
    '   { Data is OK. Perform exact calculations. }
    '   {-----------------------------------------}
    '
    '   AddNewline (ResultString);
    '  // Write('Calculating exact estimates');
    '   {Write('.'); }CalcPoly(dataType, numTables, tables);
    '   {Write('.');} CalcExactPVals(upFishP, loFishP, upMidPP, loMidPP);
    '   {Write('.');} CalcCmle(1.0, cmle);
    '   {Write('.');} CalcExactLim(FALSE, TRUE,  cmle, pnConfLevel, upFishLim);
    '   {Write('.');} CalcExactLim(FALSE, FALSE, cmle, pnConfLevel, upMidPLim);
    '   {Write('.');} CalcExactLim(TRUE,  TRUE,  cmle, pnConfLevel, loFishLim);
    '   {Write('.');} CalcExactLim(TRUE,  FALSE, cmle, pnConfLevel, loMidPLim);
    '   {Write(#13, ' ':79, #13); }
    '
    '   {---------------------}
    '   { Display the results }
    '   {---------------------}
    '
    '   if dataType in [1, 2]
    '      then AddToString (ResultString, '**** EXACT ODDS RATIO ESTIMATES ****')
    '      else AddToString (ResultString, '**** EXACT RATE RATIO ESTIMATES ****');
    '
    '   AddNewline (ResultString);
    '   If dataType in [1,2] then
    '   AddToString (ResultString,
    '   'Conditional maximum likelihood estimate of Odds Ratio:    ' + Format(cmle))
    '   else AddToString (ResultString,
    '   'Conditional maximum likelihood estimate of Rate Ratio:    ' + Format(cmle));
    '   AddToArray(ResultArr,'CMLE',cmle);
    '   AddNewline (ResultString);
    '   AddToString (ResultString, 'Lower & Upper '
    '      +  FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '      '% Exact Fisher Limits:  '+ Format(loFishLim)+ Format(upFishLim));
    '
    '   AddToArray(ResultArr,'Lower '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Fisher Limit',loFishLim);
    '   AddToArray(ResultArr,'Upper '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Fisher Limit',upFishLim);
    '
    '   AddNewline (ResultString);
    '
    '   AddToString (ResultString, 'Lower & Upper '+
    '       FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '      '% Exact Mid-P Limits:   '+ Format(loMidPLim)+ Format(upMidPLim));
    '
    '   AddToArray(ResultArr,'Lower '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Mid-P Limit',loMidPLim);
    '   AddToArray(ResultArr,'Upper '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Mid-P Limit',upMidPLim);
    '
    '   AddNewline (ResultString);
    '   AddToString (ResultString, 'Lower & Upper Exact Fisher P-values:   '+
    '      FormatP(loFishP)+ FormatP(upFishP));
    '
    '   AddToArray(ResultArr,'Lower '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Fisher p-Value',loFishP);
    '   AddToArray(ResultArr,'Upper '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Fisher p-Value',upFishP);
    '
    '   AddNewline (ResultString);
    '   AddToString (ResultString, 'Lower & Upper Exact Mid-P P-values:    '+
    '      FormatP(loMidpP)+ FormatP(upMidpP));
    '
    '   AddToArray(ResultArr,'Lower '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Mid-P p-Value',loMidpP);
    '   AddToArray(ResultArr,'Upper '+ FloatToStrF (pnConfLevel*100,ffGeneral,2,3)+
    '        '%Exact Mid-P p-Value',upMidpP);
    '
    '   AddNewline (ResultString);
    'End;  {If error <>0}
    '   Process := ResultString;
    '   pvaResultArray := ResultArr;
    'end; { Run }
    '

    Function Process(ByRef pvaDataArray As Object, ByRef pvaResultArray As Object, ByRef DataType As Short, ByRef pnConfLevel As Double) As String
        Dim c, b, d As Double '    { b, c, d cells of 2x2 table }
        Dim numTables As Short '   { Number of "unique" 2x2 tables }
        Dim cmle As Object '    { Odds Ratio (cond. max. likelihood estimate) }
        Dim loFishLim As Object '    { Lower exact Fisher confidence limit }
        Dim upFishLim As Object '    { Upper exact Fisher confidence limit }
        Dim loMidPLim As Object '    { Lower mid-P confidence limit }
        Dim upMidPLim As Object '    { Upper mid-P confidence limit }
        Dim loFishP As Object '    { Lower exact Fisher P-value }
        Dim upFishP As Object '    { Upper exact Fisher P-value }
        Dim loMidPP As Object '    { Lower exact mid-P P-value }
        Dim upMidPP As Object '    { Upper exact mid-P P-value }
        Dim twoFishPVal As Object ' { Two-tailed Fisher P-value }
        Dim approx As Double '    { An approximation to the exact estimate }
        'UPGRADE_NOTE: error was upgraded to error_Renamed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
        Dim error_Renamed As Short '   { Error in procedure CheckData }

        Dim i As Short '

        Dim ResultString As String '
        '    Dim Table As clsRec2x2   '
        Dim errornum As Short '
        ResultString = "" '
        'UPGRADE_WARNING: Lower bound of array ResultArr was changed from 1,1 to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        Dim ResultArr(1, 0) As Object '

        NumColumns = UBound(pvaDataArray, 1) '
        NumRows = UBound(pvaDataArray, 2) '
        NumStrata = UBound(pvaDataArray, 3) '

        ReDim Tables(NumStrata - 1)
        For i = 1 To NumStrata
            With Tables(i - 1)
                If DataType = 1 Then '                    { Stratified case-control }
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .a = pvaDataArray(1, 1, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .b = pvaDataArray(2, 1, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .c = pvaDataArray(1, 2, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .d = pvaDataArray(2, 2, i) '

                    .freq = 1 '
                    .m1 = .a + .b ' { # cases }
                    .n1 = .a + .c ' { # exposed }
                    .n0 = .b + .d '           { # unexposed }
                    .informative = (.a * .d <> 0) Or (.b * .c <> 0) '

                ElseIf DataType = 2 Then  '                 { Matched case-control }

                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .a = pvaDataArray(1, 1, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .c = pvaDataArray(2, 1, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .d = pvaDataArray(1, 2, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .freq = pvaDataArray(1, 2, i) '

                    If .a <= 1 Then
                        b = 1 - .a
                    Else
                        b = -1
                    End If
                    .m1 = .a + .b ' { # cases }
                    .n1 = .a + .c ' { # exposed }
                    .n0 = .b + .d '           { # unexposed }
                    .informative = (.a * .d <> 0) Or (.b * .c <> 0) '
                ElseIf DataType = 3 Then  '               { Stratified person-time }
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .a = pvaDataArray(1, 1, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .b = pvaDataArray(2, 1, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .n1 = pvaDataArray(1, 2, i) '
                    'UPGRADE_WARNING: Couldn't resolve default property of object pvaDataArray(). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                    .n0 = pvaDataArray(2, 1, i) '
                    .freq = 1 '
                    .m1 = .a + .b ' { # cases }
                    .informative = (.a * .n0 <> 0) Or (.b * .n1 <> 0) '
                End If
            End With
        Next i

        CheckData(DataType, NumStrata, Tables, errornum) '


        If errornum <> 0 Then
            AddNewLine((ResultString)) '
            Select Case errornum
                Case 1
                    AddToString(ResultString, "PROBLEM: Too much data. Cannot perform exact calculations.") '
                    AddNewLine(ResultString) '
                    AddToArray(ResultArr, "ERROR", 1) '
                Case 2
                    AddToString(ResultString, "PROBLEM: All tables have zero marginals. Cannot perform exact calculations.") '
                    AddNewLine((ResultString)) '
                    AddToArray(ResultArr, "ERROR", 2) '
                Case 3
                    AddToString(ResultString, "PROBLEM: Must have only one case in each table for exact calculations.") '
                    AddNewLine(ResultString) '
                    AddToArray(ResultArr, "ERROR", 3) '
            End Select
        Else
            On Error GoTo Process_Error
            CalcPoly(DataType, numTables, Tables)
            CalcExactPVals(upFishP, loFishP, upMidPP, loMidPP, twoFishPVal)
            CalcCmle(1.0#, cmle)
            'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
            If VarType(cmle) = VariantType.Boolean Then
                approx = maxSumA
            Else
                'UPGRADE_WARNING: Couldn't resolve default property of object cmle. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                approx = cmle
            End If
            CalcExactLim(False, True, approx, pnConfLevel, upFishLim)
            CalcExactLim(False, False, approx, pnConfLevel, upMidPLim)
            CalcExactLim(True, True, approx, pnConfLevel, loFishLim)
            CalcExactLim(True, False, approx, pnConfLevel, loMidPLim)
            On Error Resume Next
            AddNewLine(ResultString)
            Select Case DataType
                Case 1, 2
                    AddToString(ResultString, "**** EXACT ODDS RATIO ESTIMATES ****")
                    AddNewLine(ResultString)
                    AddToString(ResultString, "Conditional maximum likelihood estimate of Odds Ratio:" & Reformat(cmle))
                Case Else
                    AddToString(ResultString, "**** EXACT RATE RATIO ESTIMATES ****")
                    AddNewLine(ResultString)
                    AddToString(ResultString, "Conditional maximum likelihood estimate of Rate Ratio:" & Reformat(cmle)) '
            End Select
            AddToArray(ResultArr, "CMLE", cmle) '
            AddNewLine(ResultString) '
            AddToString(ResultString, "Lower & Upper " & VB6.Format(pnConfLevel, "00%") & " Exact Fisher Limits:" & Reformat(loFishLim) & Reformat(upFishLim))

            AddToArray(ResultArr, "Lower " & VB6.Format(pnConfLevel, "00%") & " Exact Fisher Limit", loFishLim) '
            AddToArray(ResultArr, "Upper " & VB6.Format(pnConfLevel, "00%") & " Exact Fisher Limit", upFishLim) '

            AddNewLine(ResultString) '

            AddToString(ResultString, "Lower & Upper " & VB6.Format(pnConfLevel, "00%") & " Exact Mid-P Limits:" & Reformat(loMidPLim) & Reformat(upMidPLim)) '

            AddToArray(ResultArr, "Lower " & VB6.Format(pnConfLevel, "00%") & " Exact Mid-P Limit", loMidPLim) '
            AddToArray(ResultArr, "Upper " & VB6.Format(pnConfLevel, "00%") & " Exact Mid-P Limit", upMidPLim) '

            AddNewLine(ResultString) '
            AddToString(ResultString, "Lower & Upper Exact Fisher P-values:" & FormatP(loFishP) & FormatP(upFishP)) '

            AddToArray(ResultArr, "Lower " & VB6.Format(pnConfLevel, "00%") & " Exact Fisher p-Value", loFishP) '
            AddToArray(ResultArr, "Upper " & VB6.Format(pnConfLevel, "00%") & " Exact Fisher p-Value", upFishP) '

            AddNewLine(ResultString) '
            AddToString(ResultString, "Lower & Upper Exact Mid-P P-values:" & FormatP(loMidPP) & FormatP(upMidPP)) '

            AddToArray(ResultArr, "Lower " & VB6.Format(pnConfLevel, "00%") & " Exact Mid-P p-Value", loMidPP) '
            AddToArray(ResultArr, "Upper " & VB6.Format(pnConfLevel, "00%") & " Exact Mid-P p-Value", upMidPP) '

            AddToArray(ResultArr, "Fisher Exact 2-Tailed p-Value", twoFishPVal) '

            AddNewLine(ResultString) '
        End If '  {If error <>0}
Process_Exit:
        Process = ResultString '
        'UPGRADE_WARNING: Couldn't resolve default property of object pvaResultArray. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pvaResultArray = VB6.CopyArray(ResultArr) '
        Exit Function
Process_Error:
        AddToString(ResultString, "PROBLEM: Overflow calculating Fisher and Exact.") '
        AddNewLine(ResultString) '
        AddToArray(ResultArr, "ERROR", 4) '
        Resume Process_Exit

    End Function

    '
    ' Function Strat2x2 (DataArray : OleVariant; ConfLevel : Double;
    '                    Var ResultArray : OleVariant):OleVariant;StdCall;
    '   begin
    '   Strat2x2 := Process (DataArray, ResultArray, 1, ConfLevel);
    '
    '   end;

    Public Function Strat2x2(ByRef DataArray As Object, ByRef ConfLevel As Double, ByRef ResultArray As Object) As Object 'StdCall  '

        Strat2x2 = Process(DataArray, ResultArray, 1, ConfLevel) '

    End Function '

    '
    ' Function MatchedCC(DataArray : OleVariant; ConfLevel : Double;
    '                    Var ResultArray : OleVariant):OleVariant;StdCall;
    '   begin
    '          MatchedCC := Process (DataArray, ResultArray, 2, ConfLevel);
    '   end;

    Public Function MatchedCC(ByRef DataArray As Object, ByRef ConfLevel As Double, ByRef ResultArray As Object) As Object 'StdCall  '

        MatchedCC = Process(DataArray, ResultArray, 2, ConfLevel) '

    End Function '

    '
    ' Function PersonTime (DataArray : OleVariant; ConfLevel : Double;
    '                    Var ResultArray : OleVariant):OleVariant;StdCall;
    '   begin
    '        PersonTime := Process (DataArray, ResultArray, 3, ConfLevel);
    '   end;

    Public Function PersonTime(ByRef DataArray As Object, ByRef ConfLevel As Double, ByRef ResultArray As Object) As Object 'StdCall  '

        PersonTime = Process(DataArray, ResultArray, 3, ConfLevel) '

    End Function '

    '(*begin { main program }
    '   Writeln;
    '   Writeln('³ßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßßß³');
    '   Writeln('³                          EXACTBB                             ³');
    '   Writeln('³             Written by David O. Martin, MD, MPH              ³');
    '   Writeln('³                   Harvard Medical School                     ³');
    '   Writeln('³                Beth Israel Hospital, Boston                  ³');
    '   Writeln('ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ');
    '
    '   Writeln;
    '   Writeln('This is a bare-bones program which calculates the conditional');
    '   Writeln('maximum likelihood estimate, exact confidence limits, and exact ');
    '   Writeln('P-values for either an an odds ratio (given a series of 2x2');
    '   Writeln('tables with person-count denominators) or a rate ratio (given a');
    '   Writeln('series of 2x2 tables with person-time denominators). It utilizes');
    '   Writeln('an efficient algorithm for calculating the coefficients of the');
    '   Writeln('conditional distribution. References: 1) Martin D, & Austin H,');
    '   Writeln('Epidemiology, 2, 359-362, and 2) Martin DO, & Austin H,');
    '   Writeln('Epidemiology, 7, 29-33.');
    '
    '   repeat
    '      Run;
    '   until YesNo('Quit?');
    '
    '       *)
    'begin
    'end. { of unit}

    Private Sub AddNewLine(ByRef ps1 As String)
        ps1 = ps1 & vbCrLf
    End Sub

    Private Sub AddToString(ByRef psExisting As String, ByRef psAdd As String)
        psExisting = psExisting & psAdd
    End Sub

    Private Sub AddToArray(ByRef pvArray(,) As Object, ByRef psTitle As String, ByRef pvResult As Object)
        'UPGRADE_WARNING: Couldn't resolve default property of object pvArray(LBound(), UBound()). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pvArray(LBound(pvArray, 1), UBound(pvArray, 2)) = psTitle
        'UPGRADE_WARNING: Couldn't resolve default property of object pvResult. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object pvArray(UBound(), UBound()). Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        pvArray(UBound(pvArray, 1), UBound(pvArray, 2)) = pvResult
        'UPGRADE_WARNING: Lower bound of array pvArray was changed from LBound(pvArray, 1),LBound(pvArray, 2) to 0,0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve pvArray(UBound(pvArray, 1), UBound(pvArray, 2) + 1)
    End Sub
End Class