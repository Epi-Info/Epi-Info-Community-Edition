Public Class Strat2x2
    Function ComputeOddsRatio(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double)
        Dim ComputedOR As Double
        Dim numerator As Double
        Dim denominator As Double

        For i = 0 To a.Length - 1
            numerator = numerator + (a(i) * d(i)) / (a(i) + b(i) + c(i) + d(i))
        Next
        For i = 0 To a.Length - 1
            denominator = denominator + (b(i) * c(i)) / (a(i) + b(i) + c(i) + d(i))
        Next
        ComputedOR = numerator / denominator
        Return ComputedOR
    End Function

    Function ZSElnOR(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double)
        Dim SElnOR As Double
        Dim p1 As Double
        Dim p2 As Double
        Dim p3 As Double
        Dim p4 As Double
        Dim p5 As Double
        Dim ZValue As Double
        ZValue = 1.96

        For i = 0 To a.Length - 1
            p1 = p1 + ((a(i) + d(i)) / (a(i) + b(i) + c(i) + d(i))) * (a(i) * d(i) / (a(i) + b(i) + c(i) + d(i)))
            p2 = p2 + ((a(i) + d(i)) / (a(i) + b(i) + c(i) + d(i))) * (b(i) * c(i) / (a(i) + b(i) + c(i) + d(i))) + ((b(i) + c(i)) / (a(i) + b(i) + c(i) + d(i))) * (a(i) * d(i) / (a(i) + b(i) + c(i) + d(i)))
            p3 = p3 + ((b(i) + c(i)) / (a(i) + b(i) + c(i) + d(i))) * (b(i) * c(i) / (a(i) + b(i) + c(i) + d(i)))
            p4 = p4 + (a(i) * d(i) / (a(i) + b(i) + c(i) + d(i)))
            p5 = p5 + (b(i) * c(i) / (a(i) + b(i) + c(i) + d(i)))
        Next
        SElnOR = Math.Sqrt(p1 / (2 * p4 * p4) + p2 / (2 * p4 * p5) + p3 / (2 * p5 * p5))
        ZSElnOR = ZValue * SElnOR
        Return ZSElnOR
    End Function

    Function ComputedRR(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double)
        Dim RR As Double
        Dim numerator As Double
        Dim denominator As Double

        For i = 0 To a.Length - 1
            numerator = numerator + (a(i) * (c(i) + d(i))) / (a(i) + b(i) + c(i) + d(i))
            denominator = denominator + (c(i) * (a(i) + b(i))) / (a(i) + b(i) + c(i) + d(i))
        Next
        RR = numerator / denominator
        Return RR
    End Function

    Function ZSElnRR(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double)
        Dim SElnRR As Double
        Dim numerator As Double
        Dim denom1 As Double
        Dim denom2 As Double
        Dim ZValue As Double
        ZValue = 1.96

        For i = 0 To a.Length - 1
            numerator = numerator + ((a(i) + c(i)) * (a(i) + b(i)) * (c(i) + d(i)) - a(i) * c(i) * (a(i) + b(i) + c(i) + d(i))) / ((a(i) + b(i) + c(i) + d(i)) * (a(i) + b(i) + c(i) + d(i)))
            denom1 = denom1 + (a(i) * (c(i) + d(i))) / (a(i) + b(i) + c(i) + d(i))
            denom2 = denom2 + (c(i) * (a(i) + b(i))) / (a(i) + b(i) + c(i) + d(i))
        Next
        SElnRR = Math.Sqrt(numerator / (denom1 * denom2))
        ZSElnRR = ZValue * SElnRR
        Return ZSElnRR
    End Function

    Function ucestimaten(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double)
        Dim Tables As Integer
        Tables = UBound(a)
        Dim M1(Tables) As Integer
        Dim M0(Tables) As Integer
        Dim N1(Tables) As Integer
        Dim N0(Tables) As Integer
        Dim ls(Tables) As Integer
        Dim us(Tables) As Integer
        Dim xs() As Double
        Dim x As Integer
        Dim l As Integer
        Dim u As Integer
        Dim maxuldiff As Double

        For i = 0 To a.Length - 1
            If a(i) > 999999 Or b(i) > 999999 Or c(i) > 999999 Or d(i) > 999999 Then
                Return Double.NaN
            End If
        Next


        xs = a
        x = 0
        l = 0
        u = 0
        '    ReDim M1(0 To Tables) As Integer
        '    ReDim M0(0 To Tables) As Integer
        '    ReDim N1(0 To Tables) As Integer
        '    ReDim N0(0 To Tables) As Integer
        '    ReDim ls(0 To Tables) As Integer
        '    ReDim us(0 To Tables) As Integer

        For i = 0 To Tables
            M1(i) = a(i) + c(i)
            M0(i) = b(i) + d(i)
            N1(i) = a(i) + b(i)
            N0(i) = c(i) + d(i)
            ls(i) = Math.Max(0, N1(i) - M0(i))
            us(i) = Math.Min(M1(i), N1(i))
            x = x + xs(i)
            l = l + ls(i)
            u = u + us(i)
        Next i

        Dim Cs(,) As Double
        Dim dimC2 As Integer
        For i = 0 To a.Length - 1
            dimC2 = dimC2 + (us(i) - ls(i))
        Next
        Dim C2(dimC2) As Double
        maxuldiff = 0

        For i = 0 To Tables
            If us(i) - ls(i) > maxuldiff Then maxuldiff = us(i) - ls(i)
            'ReDim Preserve Cs(0 To Tables, 0 To maxuldiff) As Double
            ReDim Preserve Cs(Tables, maxuldiff)
            For s = ls(i) To us(i)
                Cs(i, s - ls(i)) = choosey(M1(i), s) * choosey(M0(i), (N1(i) - s))
            Next s
        Next i

        'ReDim C2(0 To u - l) As Double

        Dim Y(u - l) As Double
        ' ReDim Y(0 To u - l) As Double
        'ReDim Z(0 To u - l) As Double

        For j = 0 To us(0) - ls(0)
            For k = 0 To us(1) - ls(1)
                C2(j + k) = C2(j + k) + Cs(0, j) * Cs(1, k)
            Next k
        Next j
        Dim bound As Integer
        For i = 2 To Tables
            For j = 0 To u - l
                Y(j) = C2(j)
                C2(j) = CDbl(0)
            Next j
            bound = 0
            For j = 0 To i - 1
                bound = bound + (us(i) - ls(i))
            Next j
            For j = 0 To u - l
                For k = 0 To us(i) - ls(i)
                    If j + k <= u - l Then C2(j + k) = C2(j + k) + Y(j) * Cs(i, k)
                Next k
            Next j
        Next i

        Dim R As Double
        R = CDbl(0)
        Dim Ds(Tables) As Double
        'ReDim Ds(0 To Tables) As Double
        For i = 0 To Tables
            Ds(i) = CDbl(0)
        Next i
        Dim d2 As Double
        Dim FR As Double
        d2 = CDbl(1)
        FR = CDbl(0)

        '        While Math.Abs(FR - x) > 0.002
        Dim adder As Double
        While FR < x
            For i = 0 To Tables
                Ds(i) = CDbl(0)
            Next i
            d2 = CDbl(1)
            FR = CDbl(0)
            R = R + 1
            For j = 0 To Tables
                For i = 0 To us(j) - ls(j)
                    Ds(j) = Ds(j) + Cs(j, i) * R ^ (ls(j) + i)
                Next i
                d2 = d2 * Ds(j)
            Next j
            For i = 0 To (u - l)
                adder = (i + l) * C2(i)
                For j = 0 To Ds.Length - 1
                    adder /= Ds(j)
                Next
                adder *= R ^ (i + l)
                FR += adder
                '                FR = FR + ((i + l) * C2(i) * R ^ (i + l)) / d2
            Next i
        End While
        '        R = R - 0.0002
        Dim aa As Double
        Dim bb As Double
        Dim precision As Double
        precision = 0.00001
        aa = R - 1.0
        bb = R + 0.5
        '        While Math.Abs(FR - x) > 0.0003
        While bb - aa > precision
            For i = 0 To Tables
                Ds(i) = CDbl(0)
            Next i
            d2 = CDbl(1)
            FR = CDbl(0)
            R = (bb + aa) / 2.0
            For j = 0 To Tables
                For i = 0 To us(j) - ls(j)
                    Ds(j) = Ds(j) + Cs(j, i) * R ^ (ls(j) + i)
                Next i
                d2 = d2 * Ds(j)
            Next j
            For i = 0 To (u - l)
                adder = (i + l) * C2(i)
                For j = 0 To Ds.Length - 1
                    adder /= Ds(j)
                Next
                adder *= R ^ (i + l)
                FR += adder
                '                FR = FR + ((i + l) * C2(i) * R ^ (i + l)) / d2
            Next i
            If FR < x Then
                aa = R
            Else
                bb = R
            End If
        End While

        ucestimaten = R
        Return ucestimaten
    End Function

    Function choosey(ByVal chooa As Double, ByVal choob As Double)
        Dim ccccc As Integer
        ccccc = chooa - choob
        If choob < chooa / 2 Then choob = ccccc
        choosey = CDbl(1)
        For i = choob + 1 To chooa
            choosey = (choosey * i) / (chooa - (i - 1))
        Next i
        Return choosey
    End Function

    Function chooseyforlep(ByVal chooa As Double, ByVal choob As Double, ByVal ppp As Double)
        Dim ccccc As Integer
        chooseyforlep = CDbl(1.0)
        ccccc = chooa - choob
        Dim oldchoob As Integer
        oldchoob = choob
        If choob < chooa / 2 Then choob = ccccc
        For i = choob + 1 To chooa
            chooseyforlep = (chooseyforlep * i) / (chooa - (i - 1)) * ppp
        Next i
        chooseyforlep = chooseyforlep * (1 - ppp) ^ (chooa - oldchoob)
        If oldchoob = choob Then
            chooseyforlep = chooseyforlep * ppp ^ (choob - (chooa - choob))
        End If
        Return chooseyforlep
    End Function

    Function chooseyforgep(ByVal chooa As Double, ByVal choob As Double, ByVal ppp As Double)
        Dim ccccc As Integer
        chooseyforgep = ppp ^ choob * (1 - ppp) ^ (chooa - choob)
        ccccc = chooa - choob
        If choob < chooa / 2 Then choob = ccccc
        For i = choob + 1 To chooa
            chooseyforgep = (chooseyforgep * i) / (chooa - (i - 1))
        Next i
        Return chooseyforgep
    End Function

    Function chooseyforbeta(ByVal chooa As Double, ByVal choob As Double, ByVal p As Double, ByVal j As Integer)
        Dim q As Double
        q = 1 - p
        Dim pPow As Integer
        pPow = j
        Dim qPow As Integer
        qPow = chooa - j
        Dim ccccc As Double
        ccccc = chooa - choob
        If choob < chooa / 2 Then
            choob = ccccc
        End If
        Dim choosey As Double
        choosey = 1.0
        Dim oldchoosey As Double
        oldchoosey = choosey
        Dim positive200s As Integer
        Dim negative200s As Integer
        positive200s = 0
        negative200s = 0

        For i = choob + 1 To chooa
            oldchoosey = choosey
            If choosey < Math.Pow(10, -200) Then
                choosey = choosey * Math.Pow(10, 200)
                positive200s = positive200s + 1
            End If
            If choosey > Math.Pow(10, 200) Then
                choosey = choosey * Math.Pow(10, -200)
                negative200s = negative200s + 1
            End If
            If pPow > 0 Then
                choosey = choosey * p
                pPow = pPow - 1
            End If
            If qPow > 0 Then
                choosey = choosey * q
                qPow = qPow - 1
            End If
            choosey = (choosey * i) / (chooa - (i - 1))
        Next

        For i = 0 To pPow - 1
            oldchoosey = choosey
            choosey = choosey * p
        Next

        For i = 0 To qPow - 1
            oldchoosey = choosey
            choosey = choosey * q
        Next
        If positive200s > negative200s Then
            positive200s = positive200s - negative200s
            For i = 0 To positive200s - 1
                oldchoosey = choosey
                choosey = choosey * Math.Pow(10, -200)
            Next
        ElseIf positive200s < negative200s Then
            negative200s = negative200s - positive200s
            For i = 0 To negative200s - 1
                oldchoosey = choosey
                choosey = choosey * Math.Pow(10, 200)
            Next
        End If

        Return choosey
    End Function

    Function exactorln(ByVal aas() As Double, ByVal bbs() As Double, ByVal ccs() As Double, ByVal dds() As Double)
        Dim Tables As Double
        Tables = UBound(aas)
        Dim M1(Tables) As Double
        Dim M0(Tables) As Double
        Dim N1(Tables) As Double
        Dim N0(Tables) As Double
        Dim ls(Tables) As Double
        Dim us(Tables) As Double
        Dim xs() As Double
        Dim x As Double
        Dim l As Double
        Dim u As Double
        Dim maxuldiff As Double
        xs = aas
        x = 0
        l = 0
        u = 0
        '    ReDim M1(0 To Tables) As Integer
        '   ReDim M0(0 To Tables) As Integer
        '  ReDim N1(0 To Tables) As Integer
        ' ReDim N0(0 To Tables) As Integer
        'ReDim ls(0 To Tables) As Integer
        'ReDim us(0 To Tables) As Integer

        For i = 0 To aas.Length - 1
            If aas(i) > 999999 Or bbs(i) > 999999 Or ccs(i) > 999999 Or dds(i) > 999999 Then
                Return Double.NaN
            End If
        Next

        For i = 0 To Tables
            M1(i) = aas(i) + ccs(i)
            M0(i) = bbs(i) + dds(i)
            N1(i) = aas(i) + bbs(i)
            N0(i) = ccs(i) + dds(i)
            ls(i) = Math.Max(0, N1(i) - M0(i))
            us(i) = Math.Min(M1(i), N1(i))
            x = x + xs(i)
            l = l + ls(i)
            u = u + us(i)
        Next i

        Dim Cs(,) As Double
        Dim dimC2 As Integer
        For i = 0 To aas.Length - 1
            dimC2 = dimC2 + (us(i) - ls(i))
        Next
        Dim C2(dimC2) As Double
        maxuldiff = 0

        For i = 0 To Tables
            If us(i) - ls(i) > maxuldiff Then maxuldiff = us(i) - ls(i)
            'ReDim Preserve Cs(0 To Tables, 0 To maxuldiff) As Double
            ReDim Preserve Cs(Tables, maxuldiff)
            For s = ls(i) To us(i)
                Cs(i, s - ls(i)) = choosey(M1(i), s) * choosey(M0(i), (N1(i) - s))
            Next s
        Next i

        'ReDim C(0 To u - l) As Double

        Dim Y(u - l) As Double
        'ReDim Y(0 To u - l) As Double
        'ReDim Z(0 To u - l) As Double

        For j = 0 To us(0) - ls(0)
            For k = 0 To us(1) - ls(1)
                C2(j + k) = C2(j + k) + Cs(0, j) * Cs(1, k)
            Next k
        Next j
        Dim bound As Integer
        For i = 2 To Tables
            For j = 0 To u - l
                Y(j) = C2(j)
                C2(j) = CDbl(0)
            Next j
            bound = 0
            For j = 0 To i - 1
                bound = bound + (us(i) - ls(i))
            Next j
            For j = 0 To u - l
                For k = 0 To us(i) - ls(i)
                    If j + k <= u - l Then C2(j + k) = C2(j + k) + Y(j) * Cs(i, k)
                Next k
            Next j
        Next i

        Dim R As Double
        R = CDbl(0)
        Dim Ds(Tables) As Double
        '    ReDim Ds(0 To Tables) As Double
        For i = 0 To Tables
            Ds(i) = CDbl(0)
        Next i
        Dim d2 As Double
        Dim FR As Double
        d2 = CDbl(1)
        FR = CDbl(1)

        '        While Math.Abs(FR - 0.975) > 0.0002
        Dim adder As Double
        While FR > 0.975
            For i = 0 To Tables
                Ds(i) = CDbl(0)
            Next i
            d2 = CDbl(1)
            FR = CDbl(0)
            R = R + 1
            For j = 0 To Tables
                For i = 0 To us(j) - ls(j)
                    Ds(j) = Ds(j) + Cs(j, i) * R ^ (ls(j) + i)
                Next i
                d2 = d2 * Ds(j)
            Next j
            For i = 0 To ((x - 1) - l)
                adder = C2(i)
                For j = 0 To Ds.Length - 1
                    adder /= Ds(j)
                Next
                adder *= R ^ (i + l)
                FR += adder
                '                FR = FR + (C2(i) * R ^ (i + l)) / d2
            Next i
        End While
        '        R = R - 0.0002
        Dim aa As Double
        Dim bb As Double
        Dim precision As Double
        precision = 0.00001
        aa = R - 1.0
        bb = R + 0.5
        '        While Math.Abs(FR - 0.975) > 0.00002
        While bb - aa > precision
            For i = 0 To Tables
                Ds(i) = CDbl(0)
            Next i
            d2 = CDbl(1)
            FR = CDbl(0)
            R = (bb + aa) / 2.0
            For j = 0 To Tables
                For i = 0 To us(j) - ls(j)
                    Ds(j) = Ds(j) + Cs(j, i) * R ^ (ls(j) + i)
                Next i
                d2 = d2 * Ds(j)
            Next j
            For i = 0 To ((x - 1) - l)
                adder = C2(i)
                For j = 0 To Ds.Length - 1
                    adder /= Ds(j)
                Next
                adder *= R ^ (i + l)
                FR += adder
                '                FR = FR + (C2(i) * R ^ (i + l)) / d2
            Next i
            If FR > 0.975 Then
                aa = R
            Else
                bb = R
            End If
        End While

        exactorln = R
        Return exactorln
    End Function

    Function exactorun(ByVal aas() As Double, ByVal bbs() As Double, ByVal ccs() As Double, ByVal dds() As Double, ByRef minimum As Double)
        Dim Tables As Double
        Tables = UBound(aas)
        Dim M1(Tables) As Double
        Dim M0(Tables) As Double
        Dim N1(Tables) As Double
        Dim N0(Tables) As Double
        Dim ls(Tables) As Double
        Dim us(Tables) As Double
        Dim xs() As Double
        Dim x As Double
        Dim l As Double
        Dim u As Double
        Dim maxuldiff As Double
        xs = aas
        x = 0
        l = 0
        u = 0
        '    ReDim M1(0 To Tables) As Integer
        '    ReDim M0(0 To Tables) As Integer
        '    ReDim N1(0 To Tables) As Integer
        '    ReDim N0(0 To Tables) As Integer
        '    ReDim ls(0 To Tables) As Integer
        '    ReDim us(0 To Tables) As Integer

        For i = 0 To aas.Length - 1
            If aas(i) > 999999 Or bbs(i) > 999999 Or ccs(i) > 999999 Or dds(i) > 999999 Then
                Return Double.NaN
            End If
        Next

        For i = 0 To Tables
            M1(i) = aas(i) + ccs(i)
            M0(i) = bbs(i) + dds(i)
            N1(i) = aas(i) + bbs(i)
            N0(i) = ccs(i) + dds(i)
            ls(i) = Math.Max(0, N1(i) - M0(i))
            us(i) = Math.Min(M1(i), N1(i))
            x = x + xs(i)
            l = l + ls(i)
            u = u + us(i)
        Next i

        Dim Cs(,) As Double
        Dim dimC2 As Integer
        For i = 0 To aas.Length - 1
            dimC2 = dimC2 + (us(i) - ls(i))
        Next
        Dim C2(dimC2) As Double
        maxuldiff = 0

        For i = 0 To Tables
            If us(i) - ls(i) > maxuldiff Then maxuldiff = us(i) - ls(i)
            '      ReDim Preserve Cs(0 To Tables, 0 To maxuldiff) As Double
            ReDim Preserve Cs(Tables, maxuldiff)
            For s = ls(i) To us(i)
                Cs(i, s - ls(i)) = choosey(M1(i), s) * choosey(M0(i), (N1(i) - s))
            Next s
        Next i

        '    ReDim C(0 To u - l) As Double

        Dim Y(u - l) As Double
        '    ReDim Y(0 To u - l) As Double
        '    ReDim Z(0 To u - l) As Double

        For j = 0 To us(0) - ls(0)
            For k = 0 To us(1) - ls(1)
                C2(j + k) = C2(j + k) + Cs(0, j) * Cs(1, k)
                If Double.IsInfinity(C2(j + k)) Then
                    Return Double.NaN
                End If
            Next k
        Next j
        Dim bound As Integer
        For i = 2 To Tables
            For j = 0 To u - l
                Y(j) = C2(j)
                C2(j) = CDbl(0)
            Next j
            bound = 0
            For j = 0 To i - 1
                bound = bound + (us(i) - ls(i))
            Next j
            For j = 0 To u - l
                For k = 0 To us(i) - ls(i)
                    If j + k <= u - l Then C2(j + k) = C2(j + k) + Y(j) * Cs(i, k)
                    If j + k <= u - l Then
                        If Double.IsInfinity(C2(j + k)) Then
                            Return Double.NaN
                        End If
                    End If
                Next k
            Next j
        Next i

        Dim R As Double
        R = minimum - 0.1
        Dim Ds(Tables) As Double
        '    ReDim Ds(0 To Tables) As Double
        For i = 0 To Tables
            Ds(i) = CDbl(0)
        Next i
        Dim d2 As Double
        Dim FR As Double
        Dim MiddlePart As Double
        d2 = CDbl(1)
        FR = CDbl(0)
        MiddlePart = CDbl(1)

        Do
            For i = 0 To Tables
                Ds(i) = CDbl(0)
            Next i
            d2 = CDbl(1)
            FR = CDbl(0)
            R = R + 0.1
            For j = 0 To Tables
                For i = 0 To us(j) - ls(j)
                    Ds(j) = Ds(j) + Cs(j, i) * R ^ (ls(j) + i)
                Next i
                d2 = d2 * Ds(j)

                If Double.IsInfinity(d2) = True Then
                    'Return Double.NaN
                End If
            Next j
            MiddlePart = 1 / Ds(0)
            For i = 0 To (x - l)
                If (i + l > 0) Then
                    MiddlePart = R / Ds(0)
                End If
                For j = 2 To (i + l)
                    MiddlePart = MiddlePart * R
                Next
                For j = 1 To Tables
                    MiddlePart = MiddlePart / Ds(j)
                Next j
                FR = FR + (C2(i) * MiddlePart)
            Next i
        Loop Until (FR <= 0.025)
        '        R = R - 0.1
        Dim aa As Double
        Dim bb As Double
        Dim precision As Double
        precision = 0.00001
        aa = R - 1.0
        bb = R + 0.5
        '        While Math.Abs(FR - 0.025) > 0.00002
        While bb - aa > precision
            For i = 0 To Tables
                Ds(i) = CDbl(0)
            Next i
            d2 = CDbl(1)
            FR = CDbl(0)
            R = (bb + aa) / 2.0
            For j = 0 To Tables
                For i = 0 To us(j) - ls(j)
                    Ds(j) = Ds(j) + Cs(j, i) * R ^ (ls(j) + i)
                Next i
                d2 = d2 * Ds(j)
            Next j
            MiddlePart = 1 / Ds(0)
            For i = 0 To (x - l)
                If (i + l > 0) Then
                    MiddlePart = R / Ds(0)
                End If
                For j = 2 To (i + l)
                    MiddlePart = MiddlePart * R
                Next
                For j = 1 To Tables
                    MiddlePart = MiddlePart / Ds(j)
                Next j
                FR = FR + (C2(i) * MiddlePart)
            Next i
            If FR > 0.025 Then
                aa = R
            Else
                bb = R
            End If
        End While

        exactorun = R
        Return exactorun
    End Function

    Function ComputeUnChisq(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double) As Double
        Dim unChisq As Double
        Dim numerator As Double
        Dim denominator As Double

        For i = 0 To a.Length - 1
            numerator = numerator + (a(i) * d(i) - b(i) * c(i)) / (a(i) + b(i) + c(i) + d(i))
            denominator = denominator + ((a(i) + b(i)) * (c(i) + d(i)) * (a(i) + c(i)) * (b(i) + d(i))) / (((a(i) + b(i) + c(i) + d(i)) - 1) * (a(i) + b(i) + c(i) + d(i)) * (a(i) + b(i) + c(i) + d(i)))
        Next
        unChisq = (numerator * numerator) / denominator
        Return unChisq
    End Function

    Function ComputeCorrChisq(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double) As Double
        Dim CorrChisq As Double
        Dim numerator As Double
        Dim denominator As Double

        For i = 0 To a.Length - 1
            numerator = numerator + (a(i) * d(i) - b(i) * c(i)) / (a(i) + b(i) + c(i) + d(i))
            denominator = denominator + ((a(i) + b(i)) * (c(i) + d(i)) * (a(i) + c(i)) * (b(i) + d(i))) / (((a(i) + b(i) + c(i) + d(i)) - 1) * (a(i) + b(i) + c(i) + d(i)) * (a(i) + b(i) + c(i) + d(i)))
        Next
        CorrChisq = ((Math.Abs(numerator) - 0.5) * (Math.Abs(numerator) - 0.5)) / denominator
        Return CorrChisq
    End Function

    Function ComputeFisherExactRightTail(ByVal a() As Double, ByVal b() As Double, ByVal c() As Double, ByVal d() As Double) As Double
        Dim section1(a.Length - 1) As Double

        For k = 0 To a.Length - 1
            For i = 0 To Math.Min(b(k), d(k))
                section1(k) = section1(k) + ((choosey(a(k) + b(k), a(k) + i)) * (choosey(c(k) + d(k), c(k) - i))) / choosey(a(k) + b(k) + c(k) + d(k), a(k) + c(k))
            Next
        Next
        Return section1(1)
    End Function

    Function pForChisq(ByVal X2Val As Double)
        Dim pval As Double
        pval = dist1.PfromX2(X2Val, 1)
        Return pval
    End Function

    Function pForChisq(ByVal X2Val As Double, ByVal df As Double)
        Dim pval As Double
        pval = dist1.PfromX2(X2Val, df)
        Return pval
    End Function

    Function binpdf(ByVal n As Integer, ByVal x As Integer, ByVal p As Double, ByRef ltp As Double, ByRef lep As Double, ByRef eqp As Double, ByRef gep As Double, ByRef gtp As Double, ByRef ptt As Double, ByRef lcl As Integer, ByRef ucl As Integer)
        ltp = 0.0
        lep = 0.0
        'eqp = choosey(n, x) * p ^ x * (1 - p) ^ (n - x)
        eqp = chooseyforlep(n, x, p)
        gep = 0.0
        gtp = 0.0
        ptt = 0.0
        lcl = 0
        ucl = n

        For i = 1 To x
            'ltp = ltp + choosey(n, x - i) * p ^ (x - i) * (1 - p) ^ (n - (x - i))
            ltp = ltp + chooseyforlep(n, x - i, p)
        Next
        For i = 0 To x
            'lep = lep + choosey(n, x - i) * p ^ (x - i) * (1 - p) ^ (n - (x - i))
            lep = lep + chooseyforlep(n, x - i, p)
        Next
        For i = 0 To n - x
            'gep = gep + choosey(n, x + i) * p ^ (x + i) * (1 - p) ^ (n - (x + i))
            gep = gep + chooseyforlep(n, x + i, p)
        Next
        For i = 1 To n - x
            'gtp = gtp + choosey(n, x + i) * p ^ (x + i) * (1 - p) ^ (n - (x + i))
            gtp = gtp + chooseyforlep(n, x + i, p)
        Next
        ptt = Math.Min(2 * Math.Min(lep, gep), 1)

        Dim pp As Double
        pp = 0.0
        Dim pvalue As Double
        Dim LowerDesiredPValue As Double
        LowerDesiredPValue = 0.025
        Dim UpperDesiredPValue As Double
        UpperDesiredPValue = 0.975
        Dim a As Double
        Dim b As Double
        a = 0.0
        b = 1.0
        If x > 0 Then
            While b - a > 0.00001
                pp = (a + b) / 2.0
                pvalue = ribetafunction(pp, x, n - x + 1, True)
                If pvalue > LowerDesiredPValue Then
                    b = pp
                Else
                    a = pp
                End If
            End While
            lcl = Math.Round(pp * n)
            If Double.IsNaN(pvalue) Then
                lcl = -1
            End If
        End If
        pp = 1.0
        pvalue = 0.0
        a = 0.0
        b = 1.0
        If x < n Then
            While b - a > 0.00001
                pp = (a + b) / 2.0
                pvalue = ribetafunction(pp, x + 1, n - x, True)
                If pvalue > UpperDesiredPValue Then
                    b = pp
                Else
                    a = pp
                End If
            End While
            ucl = Math.Round(pp * n)
            If Double.IsNaN(pvalue) Then
                ucl = -1
            End If
        End If
    End Function

    Function poipdf(ByVal x As Integer, ByVal lambda As Double, ByRef ltp As Double, ByRef lep As Double, ByRef eqp As Double, ByRef gep As Double, ByRef gtp As Double)
        Dim denominator As Double
        ltp = 0.0
        lep = 0.0
        eqp = 0.0
        gep = 0.0
        gtp = 0.0

        If x > 0 Then
            ltp = Math.Exp(-lambda)
            For j = 1 To x - 1
                denominator = 1
                For i = 0 To j - 1
                    denominator = denominator * (j - i)
                Next
                ltp = ltp + (lambda ^ j * Math.Exp(-lambda)) / denominator
            Next
        End If
        lep = Math.Exp(-lambda)
        For j = 1 To x
            denominator = 1
            For i = 0 To j - 1
                denominator = denominator * (j - i)
            Next
            lep = lep + (lambda ^ j * Math.Exp(-lambda)) / denominator
        Next
        denominator = 1
        For i = 0 To x - 1
            denominator = denominator * (x - i)
        Next
        eqp = (lambda ^ x * Math.Exp(-lambda)) / denominator
        gep = 1 - ltp
        gtp = 1 - lep
    End Function

    Function newribetafunction(ByVal p As Double, ByVal alpha As Integer, ByVal beta As Integer)
        Dim functionvalue As Double
        Dim aa As Double
        Dim bb As Double
        Dim cc As Double
        aa = 1.0
        bb = 1.0
        cc = 1.0

        For i = 0 To alpha + beta - 2
            aa = aa * ((alpha + beta - 1) - i)
        Next
        For j = alpha To alpha + beta - 1
            bb = 1.0
            cc = 1.0
            For i = 0 To j - 1
                bb = bb * (p / (j - i))
            Next
            For i = 0 To (alpha + beta - 1 - j) - 1
                cc = cc * ((1 - p) / ((alpha + beta - 1 - j) - i))
            Next
            functionvalue = functionvalue + (aa * bb * cc)
        Next

        Return functionvalue
    End Function

    Function ribetafunction(ByVal p As Double, ByVal alpha As Integer, ByVal beta As Integer)
        Dim functionvalue As Double
        Dim aa As Double
        Dim bb As Double
        Dim cc As Double
        aa = 1.0
        bb = 1.0
        cc = 1.0

        For i = 0 To alpha + beta - 2
            aa = aa * ((alpha + beta - 1) - i)
        Next
        For j = alpha To alpha + beta - 1
            bb = 1.0
            cc = 1.0
            For i = 0 To j - 1
                bb = bb * (j - i)
            Next
            For i = 0 To (alpha + beta - 1 - j) - 1
                cc = cc * ((alpha + beta - 1 - j) - i)
            Next
            functionvalue = functionvalue + (aa / (bb * cc)) * p ^ j * (1 - p) ^ (alpha + beta - 1 - j)
        Next

        Return functionvalue
    End Function

    Function ribetafunction(ByVal p As Double, ByVal alpha As Integer, ByVal beta As Integer, UseChoosey As Boolean)
        Dim functionvalue As Double
        Dim functionadd As Double
        functionvalue = 0.0
        functionadd = 0.0

        For j = alpha To alpha + beta - 1
            functionadd = chooseyforbeta(alpha + beta - 1, j, p, j)
            functionvalue = functionvalue + functionadd
        Next

        Return functionvalue
    End Function
End Class
