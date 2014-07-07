Option Strict Off
Option Explicit On
'UPGRADE_WARNING: Class instancing was changed to public. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="ED41034B-3890-49FC-8076-BD6FC2F42A85"'
<System.Runtime.InteropServices.ProgId("statlib_NET.statlib")> Public Class statlib
	Private Const ErrStart As Integer = &H3000
	
	Function mean(ByRef a() As Object, ByRef b() As Object) As Double
		Dim i As Object
		For i = 1 To UBound(a)
			'UPGRADE_WARNING: Couldn't resolve default property of object i. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			mean = mean + a(i) * b(i)
		Next 
		mean = mean / UBound(a)
	End Function
	
	Function var(ByRef a() As Object, ByRef b() As Object) As Double
		Dim i As Object
		For i = 1 To UBound(a)
			'UPGRADE_WARNING: Couldn't resolve default property of object i. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			var = var + b(i) * (a(i) - mean(a, b)) ^ 2
		Next 
		var = var / (UBound(a) - 1)
	End Function
	
	' return right tail from z score
	Function PfromZ(ByVal Z As Double) As Double
		Dim P As Object
		Dim y As Object
		Dim x As Object
		Const LTONE As Short = 7
		Const UTZERO As Short = 12
		Const CON As Double = 1.28
		
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		x = System.Math.Abs(Z)
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If x > UTZERO Then
			If Z < 0 Then
				PfromZ = 1
			Else
				PfromZ = 0
			End If
			Exit Function
		End If
		'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		y = (Z ^ 2) / 2
		If x > CON Then
			'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = x - 0.151679116635 + 5.29330324926 / (x + 4.8385912808 - 15.1508972451 / (x + 0.742380924027 + 30.789933034 / (x + 3.99019417011)))
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = x + 0.000398064794 + 1.986158381364 / P
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = x - 0.000000038052 + 1.00000615302 / P
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = 0.398942280385 * System.Math.Exp(-y) / P
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object y. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = y / (y + 5.75885480458 - 29.8213557808 / (y + 2.624331121679 + 48.6959930692 / (y + 5.92885724438)))
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = 0.398942280444 - 0.399903438504 * P
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			P = 0.5 - x * P
		End If
		If Z < 0 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PfromZ = 1 - P
		Else
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PfromZ = P
		End If
	End Function
	
	' return the right tail from T
	Public Function PfromT(ByVal T As Double, ByVal df As Integer) As Double
		Dim P As Object
		Dim c As Object
		Dim s As Object
		Dim b As Object
		Dim a As Object
		' Z distribution used if DF > MaxInt
		Dim F, i, ddf As Short
		Const G1 As Double = 0.3183098862
		Const MaxInt As Short = 1000
		
		T = System.Math.Abs(T)
		If df < MaxInt Then
			ddf = df
			'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			a = T / System.Math.Sqrt(ddf)
			'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			b = ddf / (ddf + T ^ 2)
			i = ddf Mod 2
			
			'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			s = 1
			'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			c = 1
			F = 2 + i
			Do While F <= ddf - 2
				'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				c = c * b * (F - 1) / F
				'UPGRADE_WARNING: Couldn't resolve default property of object c. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				s = s + c
				F = F + 2
			Loop 
			If i <= 0 Then
				'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				P = 0.5 - a * System.Math.Sqrt(b) * s / 2
			Else
				'UPGRADE_WARNING: Couldn't resolve default property of object a. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object s. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object b. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				P = 0.5 - (a * b * s + System.Math.Atan(a)) * G1
			End If
			
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If P < 0 Then
				'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				P = 0
			End If
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			If P > 1 Then
				'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				P = 1
			End If
			'UPGRADE_WARNING: Couldn't resolve default property of object P. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			PfromT = P
		Else
			PfromT = PfromZ(T)
		End If
	End Function '{ PfromT }
	
	'return right tail p from chi square
    Function PfromX2(ByVal x As Double, ByVal df As Double) As Double
        Dim m As Object
        Dim l As Object
        Dim j As Object
        Dim k As Object
        Const pi As Double = 3.14159265359

        ' Added by Eric Fontaine 07/22/03
        ' Need to dim this in order to protect against overflow
        Dim rr As Double
        Dim ii As Integer

        If x < 0.000000001 Or df < 1 Then
            PfromX2 = 1
            Exit Function
        End If

        rr = 1
        ii = df

        ' Added by Eric Fontaine 07/22/03: begin overflow error checking
        On Error Resume Next

        While ii >= 2
            rr = rr * ii
            ii = ii - 2
        End While

        ' Added by Eric Fontaine 07/22/03: end overflow error checking
        On Error GoTo 0

        'UPGRADE_WARNING: Couldn't resolve default property of object k. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        k = System.Math.Exp(Int((df + 1) * 0.5) * System.Math.Log(System.Math.Abs(x)) - x * 0.5) / rr

        If k < 0.00001 Then '       Added by RF 11/30/99, fixed 9/28/00
            PfromX2 = 0 '       same
            Exit Function '       same
        End If '       same

        If Int(df * 0.5) = df * 0.5 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object j. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            j = 1
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object j. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            j = System.Math.Sqrt(2 / x / pi)
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object l. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        l = 1
        'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        m = 1

        If Not Double.IsNaN(x) And Not Double.IsInfinity(x) Then
            Do Until m < 0.00000001
                df = df + 2
                'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                m = m * x / df
                'UPGRADE_WARNING: Couldn't resolve default property of object m. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                'UPGRADE_WARNING: Couldn't resolve default property of object l. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
                l = l + m
            Loop
        End If

        'UPGRADE_WARNING: Couldn't resolve default property of object l. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object k. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object j. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        PfromX2 = 1 - j * k * l

    End Function
	
	
	Function algama(ByVal s As Double) As Double
		Dim Z As Object
		Dim F As Object
		Dim x As Object
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		x = s
		algama = 0
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If x < 0 Then Exit Function
		'UPGRADE_WARNING: Couldn't resolve default property of object F. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		F = 0
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If x < 7 Then
			'UPGRADE_WARNING: Couldn't resolve default property of object F. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			F = 1
			'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			Z = x - 1
			Do 
				'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				Z = Z + 1
				'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				If Z < 7 Then
					'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					x = Z
					'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					'UPGRADE_WARNING: Couldn't resolve default property of object F. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
					F = F * Z
				End If
				'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			Loop Until Z >= 7
			'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			x = x + 1
			'UPGRADE_WARNING: Couldn't resolve default property of object F. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			F = -System.Math.Log(F)
		End If
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		Z = 1 / (x ^ 2)
		'UPGRADE_WARNING: Couldn't resolve default property of object x. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object Z. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		'UPGRADE_WARNING: Couldn't resolve default property of object F. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		algama = F + (x - 0.5) * System.Math.Log(x) - x + 0.918938533204673 + (((-1 / 1680 * Z + 1 / 1260) * Z - 1 / 360) * Z + 1 / 12) / x
	End Function
	
    Function PfromF(ByVal F As Double, ByVal df1 As Double, ByVal df2 As Double) As Double
        Dim beta As Object
        Dim xx As Object
        Dim P, q As Double
        Dim psq, term1, term, temp As Double
        Dim qq, cx, ai, pp, b As Double
        Dim rx, ns, x As Double
        Dim index As Boolean
        Const ACU As Double = 0.000000001
        If F = 0 Then Exit Function
        If F < 0 Or df1 < 1 Or df2 < 1 Then
            'UPGRADE_WARNING: Couldn't resolve default property of object Error.Raise. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'MessageBox.Show("<tlt>F distribution function error</tlt>")
            Exit Function
        End If
        If df1 = 1 Then
            PfromF = PfromT(System.Math.Sqrt(F), df2) * 2
            Exit Function
        End If
        x = df1 * F / (df2 + df1 * F)
        P = df1 / 2
        q = df2 / 2
        psq = P + q
        cx = 1 - x
        If P >= x * psq Then
            'UPGRADE_WARNING: Couldn't resolve default property of object xx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            xx = x
            pp = P
            qq = q
            index = False
        Else
            'UPGRADE_WARNING: Couldn't resolve default property of object xx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            xx = cx
            cx = x
            pp = q
            qq = P
            index = True
        End If
        term = 1
        ai = 1
        b = 1
        ns = qq + cx * psq
        'UPGRADE_WARNING: Couldn't resolve default property of object xx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        rx = xx / cx
        term1 = 1
3:      temp = qq - ai
        'UPGRADE_WARNING: Couldn't resolve default property of object xx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        If ns = 0 Then rx = xx
4:      term = term / (pp + ai) * temp * rx
        If System.Math.Abs(term) <= term1 Then
            b = b + term
            temp = System.Math.Abs(term)
            term1 = temp
            If temp > ACU Or temp > (ACU * b) Then
                ai = ai + 1
                ns = ns - 1
                If ns >= 0 Then GoTo 3
                temp = psq
                psq = psq + 1
                GoTo 4
            End If
        End If
        'UPGRADE_WARNING: Couldn't resolve default property of object beta. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        beta = algama(P) + algama(q) - algama(P + q)
        'UPGRADE_WARNING: Couldn't resolve default property of object beta. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        'UPGRADE_WARNING: Couldn't resolve default property of object xx. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        temp = (pp * System.Math.Log(xx) + (qq - 1) * System.Math.Log(cx) - beta) - System.Math.Log(pp)
        If temp > -70 Then
            b = b * System.Math.Exp(temp)
        Else
            b = b * 0
        End If
        If index Then b = 1 - b
        PfromF = 1 - b
    End Function
	
	Public Function ZFROMP(ByRef P As Double) As Double
		Dim LIMIT As Object
		
		'Provides Z value from right-tail P
		'pfromz(.025)=1.96
		
		Const P0 As Double = -0.322232431088
		Const P2 As Double = -0.342242088547
		Const P3 As Double = -0.0204231210245
		Const P4 As Double = -4.53642210148E-05
		Const Q0 As Double = 0.099348462606
		Const Q1 As Double = 0.588581570495
		Const Q2 As Double = 0.531103462366
		Const Q3 As Double = 0.10353775285
		Const Q4 As Double = 0.0038560700634
		
		Dim F, T As Double
		
		F = P
		ZFROMP = 0
		'UPGRADE_WARNING: Couldn't resolve default property of object LIMIT. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
		If (F >= 1) Or (F <= LIMIT) Then
			'UPGRADE_WARNING: Couldn't resolve default property of object Error.Raise. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            'MessageBox.Show("<tlt>P value is beyond allowable limit</tlt>")
			Exit Function
		End If
		If F > 0.5 Then
			F = 1 - F
		End If
		If F = 0.5 Then
			Exit Function
		End If
		T = System.Math.Sqrt(System.Math.Log(1 / F ^ 2))
		T = T + ((((T * P4 + P3) * T + P2) * T - 1) * T + P0) / ((((T * Q4 + Q3) * T + Q2) * T + Q1) * T + Q0)
		If P > 0.5 Then
			ZFROMP = -T
		Else
			ZFROMP = T
		End If
		
	End Function '{ ZFROMP }
	
	
	'-------------------------------------------------------------------------------
	'   The following functions from Numerical Recipes work, but not enough better than PFromX2
	'
	'Public Function GammaP(a As Double, x As Double) As Double
	''   This returns the incomplete gamma function.
	''   Modified from Numerical Recipes in Pascal sec 6.2
	'
	'    If (x < 0#) Or (a <= 0#) Then
	'        GammaP = -1#        '   Invalid arguments
	'    ElseIf x < a + 1# Then
	'        GammaP = gaer(a, x)
	'    Else
	'        GammaP = 1# - gcf(a, x)
	'    End If
	'End Function
	'
	'Public Function GammaQ(a As Double, x As Double) As Double
	''   This returns the complement of the incomplete gamma function
	''   Modified from Numerical Recipes in Pascal sec 6.2
	'
	'    If (x < 0#) Or (a <= 0#) Then
	'        GammaQ = -1#        '   Invalid arguments
	'    ElseIf x < a + 1# Then
	'        GammaQ = 1# - gaer(a, x)
	'    Else
	'        GammaQ = gcf(a, x)
	'    End If
	'
	'End Function
	'
	'Private Function gaer(a As Double, x As Double) As Double
	''   This returns the series representation of the incomplete gamma function.
	''   Modified from Numerical Recipes in Pascal sec 6.2
	'Const itmax = 300
	'Const eps = 0.000000003
	'
	'Dim gamser As Double
	'Dim gln As Double
	'Dim n As Integer
	'Dim summ As Double
	'Dim del As Double
	'Dim ap As Double
	'
	'    gamser = -1#
	'    gln = GammaLn(a)
	'    If x = 0# Then
	'        gamser = 0#
	'    ElseIf x > 0# Then
	'        ap = a
	'        summ = 1# / a
	'        del = summ
	'        For n = 1 To itmax
	'            ap = ap + 1
	'            del = del * x / ap
	'            summ = summ + del
	'            If Abs(del) < Abs(summ) * eps Then
	'                gamser = summ * Exp(-x + a * Log(x) - gln)
	'                Exit For
	'            End If
	'        Next n
	'    End If
	'    gaer = gamser
	'
	'End Function
	'
	'Private Function gcf(a As Double, x As Double) As Double
	''   This returns the continued fraction representation of the incomplete gamma function
	''   Modified from Numerical Recipes in Pascal sec 6.1
	'Const itmax = 300
	'Const eps = 0.000000003
	'
	'Dim gammcf As Double
	'Dim gln As Double
	'Dim n As Integer
	'Dim gold As Double
	'Dim g As Double
	'Dim fac As Double
	'Dim b1 As Double
	'Dim b0 As Double
	'Dim anf As Double
	'Dim ana As Double
	'Dim an As Double
	'Dim a1 As Double
	'Dim a0 As Double
	'
	'    gammcf = -1#
	'    gln = GammaLn(a)
	'    gold = 0#
	'    a0 = 1#
	'    a1 = x
	'    b0 = 0#
	'    b1 = 1#
	'    fac = 1#
	'    For i = 1 To itmax
	'        an = n
	'        ana = an - a
	'        a0 = (a1 + a0 * ana) * fac
	'        b0 = (b1 + b0 * ana) * fac
	'        anf = an * fac
	'        a1 = x * a0 + anf * a1
	'        b1 = x * b0 + anf * b1
	'        If a1 <> 0# Then
	'            fac = 1# / a1
	'            g = b1 * fac
	'            If Abs((g - gold) / g) < eps Then
	'                gammcf = Exp(-x + a * Log(x) - gln) * g
	'                Exit For
	'            End If
	'            gold = g
	'        End If
	'    Next i
	'    gcf = gammcf
	'End Function
	'
	'Public Function GammaLn(xx As Double) As Double
	''   Returns the ln of gamma(xx)
	''   Modified from Numerical Recipes in Pascal sec. 6.1
	'Const stp = 2.50662827465
	'
	'Dim x As Double
	'Dim tmp As Double
	'Dim ser As Double
	'
	'    x = xx - 1#
	'    tmp = x + 5.5
	'    tmp = (x + 0.5) * Log(tmp) - tmp
	'    ser = 1# + 76.18009173 / (x + 1#) - 86.50532033 / (x + 2#) + 24.01409822 / (x + 3#) - 1.231739516 / (x + 4#) + 0.00120858003 / (x + 5#) - 0.00000536382 / (x + 6#)
	'    GammaLn = tmp + Log(stp * ser)
	'End Function
	'
	'Public Function QFromX2(x As Double, df As Long) As Double
	''   This computes the chi-squared function
	''   Modified from Numerical Recipes in Pascal sec. 6.2
	'    QFromX2 = GammaQ(CDbl(df) / 2#, x / 2#)
	'End Function
	
	Public Function TfromP(ByRef pdblProportion As Double, ByRef pintDF As Short) As Double
        '   Computes t statistic associated with DF and proportion
        If pintDF <= 0 Then
            Return Double.NaN
        End If
		Const TOLERANCE As Double = 0.00000001
		Dim ldblProp As Double
		Dim ldblLeftX As Double
		Dim ldblLeftT As Double
		Dim ldblRightX As Double
		Dim ldblRightT As Double
		Dim ldblX As Double
		Dim ldblT As Double
		Dim i As Short
		
		If pdblProportion < 0 Or pdblProportion > 1 Then
			TfromP = 0
			Exit Function
		ElseIf pdblProportion < 0.5 Then 
			ldblProp = pdblProportion
		Else
			ldblProp = 1# - pdblProportion
		End If
		ldblProp = ldblProp / 2
		
		ldblLeftX = 0#
		ldblLeftT = 0.5
		ldblRightX = 1#
		Do 
			ldblRightX = 10# * ldblRightX
			ldblRightT = PfromT(ldblRightX, pintDF)
		Loop Until ldblRightT < ldblProp
		
		Do 
			ldblX = (ldblRightX + ldblLeftX) / 2
			ldblT = PfromT(ldblX, pintDF)
			If ldblT < ldblProp Then
				ldblRightT = ldblT
				ldblRightX = ldblX
			Else
				ldblLeftT = ldblT
				ldblLeftX = ldblX
			End If
			'i = i + 1
			'Debug.Print i, ldblLeftX, ldblRightX, ldblLeftT, ldblRightT, ldblProp, Abs(ldblT - ldblProp)
		Loop Until System.Math.Abs(ldblT - ldblProp) < TOLERANCE
		TfromP = ldblX
	End Function
End Class