Option Strict Off
Option Explicit On
Option Compare Text

Module EICoxLoadData
    Public dataTable As DataTable
    'Private lconDB As ADODB.Connection
    Public context As EpiInfo.Plugin.IAnalysisStatisticContext


    Public Sub RecursiveFactorializeCox(ByRef ldblA() As Double, ByRef r As Integer, ByRef ldblaData(,) As Double, ByRef lintOffset As Integer)
        Dim i As Integer
        Dim lpos As Integer
        'Set some Basic stuff
        'Star the covariates at the offset
        lpos = lintOffset
        For i = 0 To UBound(mIVarArray)
            Debug.Print("RecurseCox(ldblA, lpos, 0, i, 1, r, ldblaData)")
            Debug.Print("RecurseCox(ldblA,    " & lpos & ", 0, " & i & ", 1, " & r & ", ldblaData)")
            RecurseCox(ldblA, lpos, 0, i, 1, r, ldblaData)
        Next
    End Sub

    Public Sub RecurseCox(ByRef ldblA() As Double, ByRef lpos As Integer, ByRef lilevel As Integer, ByRef lIntTerm As Integer, ByVal val_Renamed As Double, ByRef lintdepth As Integer, ByRef ldblaData(,) As Double)
        Dim dbltemp As Double
        Dim j As Integer
        Dim linttemppos As Integer
        Dim strDB As String
        strDB = "ldblaData(lintdepth, lpos - 1) = val_renamed * ldblA(linttemppos)"
        Debug.Print(strDB)
        'if the level is the top most, do the array setting..
        If lilevel = UBound(mIVarArray(lIntTerm).Variables) Then
            Debug.Print("Level is topmost")
            For j = 0 To mVarArray(mIVarArray(lIntTerm).Variables(lilevel)).iSize - 1
                linttemppos = mVarArray(mIVarArray(lIntTerm).Variables(lilevel)).iColumn + j - 1
                ldblaData(lintdepth, lpos - 1) = val_Renamed * ldblA(linttemppos)
                strDB = "ldblaData(lintdepth, " & lpos & " - 1 ) = val_Renamed * ldblA(" & linttemppos & ")"
                Debug.Print(strDB)
                strDB = "ldblaData(" & lintdepth & ", " & lpos - 1 & ") = " & val_Renamed & " * " & ldblA(linttemppos)
                Debug.Print(strDB)
                strDB = "ldblaData(" & lintdepth & ", " & lpos - 1 & ") = " & val_Renamed * ldblA(linttemppos)
                Debug.Print(strDB)
                Debug.Print("==============")

                lpos = lpos + 1
            Next
            Exit Sub
        End If

        For j = 1 To mVarArray(mIVarArray(lIntTerm).Variables(lilevel)).iSize
            'Get the value of the interaction, and pass it onwards
            linttemppos = mVarArray(mIVarArray(lIntTerm).Variables(lilevel)).iColumn + j - 1
            dbltemp = val_Renamed * ldblA(linttemppos)
            strDB = "dbltemp = val_Renamed * ldblA(linttemppos)"
            Debug.Print(strDB)
            strDB = "dbltemp = " & val_Renamed & " * ldblA(" & linttemppos & ")"
            Debug.Print(strDB)
            strDB = "dbltemp = " & val_Renamed * ldblA(linttemppos)
            Debug.Print(strDB)
            Debug.Print("==================")
            Debug.Print("RecurseCox(ldblA, lpos, lilevel + 1, lIntTerm, dbltemp, lintdepth, ldblaData)")
            Debug.Print("RecurseCox(ldblA, " & lpos & ", " & lilevel + 1 & ", " & lIntTerm & ", " & dbltemp & ", " & lintdepth & ", ldblaData)")
            RecurseCox(ldblA, lpos, lilevel + 1, lIntTerm, dbltemp, lintdepth, ldblaData)
        Next
    End Sub

    Private Function ResetTempValues(ByRef ldblaTempArray() As Double) As Boolean
        Dim j As Integer
        'If a missing value was found in the data row, or the data row is done reading
        'Reset all the temp values
        For j = 0 To UBound(ldblaTempArray)
            ldblaTempArray(j) = 0
        Next j
        ResetTempValues = True
    End Function


    'Dummify function
    Public Function Dummyfy(ByRef lstrName As String) As Variable
        Dim tempTable As DataTable
        Dim lVarTemp As Variable
        Dim i As Integer
        Dim j As Integer
        Dim lintnull As Integer
        Dim lvarNullTest As Object
        Dim lintTwoNulls As Integer
        Dim lstrTemp As String
        Dim isDiscrete As Integer
        Dim canBeDiscrete As Integer
        Dim isTimeDependent As Integer

        Dim ldblPrev As Double
        Dim ldblCur As Double
        On Error GoTo DummyError

        'Use 0's and 1's to denote true or false. that way they can be multiplied 
        isDiscrete = 0
        isTimeDependent = 0
        canBeDiscrete = 1
        lintTwoNulls = 0

        tempTable = New DataTable("output")
        tempTable.Columns.Add(lstrName, context.Columns(lstrName).DataType)

        Dim lastValue As Object
        lastValue = VariantType.Null

        'CHECK FOR VARIABLE IN THE DISCRETE ARRAY
        If (UBound(mStrADiscrete) > 0) Or (Not (mStrADiscrete(0) = Nothing)) Then
            For i = 0 To UBound(mStrADiscrete)
                If (StrComp(mStrADiscrete(i), lstrName) = 0) Then
                    isDiscrete = 1
                End If
            Next
        End If

        'CHECK FOR VARIABLE IN THE time dependent ARRAY

        If (UBound(mstraTimeDependentVar) > 0) Or (Not (mstraTimeDependentVar(0) = Nothing)) Then
            For i = 0 To UBound(mstraTimeDependentVar)
                If (StrComp(mstraTimeDependentVar(i) & "$", lstrName) = 0) Then
                    isTimeDependent = 1
                End If
            Next
        End If

        Debug.Print("")
        Debug.Print("Analyzing " & lstrName)
        'Open the record as a distinct set and count the non missing values
        i = 0
        'Null Counter
        lintnull = 0
        'The following code creates a distinct table of rows;
        For Each row As DataRow In dataTable.Select("", lstrName)
            Dim columnEqual As Boolean
            columnEqual = False
            If lastValue Is Nothing And row(lstrName) Is Nothing Then
                columnEqual = True
            ElseIf lastValue Is Nothing Or row(lstrName) Is Nothing Then
                columnEqual = False
            Else
                columnEqual = lastValue.Equals(row(lstrName))
            End If
            If lastValue Is Nothing Or columnEqual = False Then
                lastValue = row(lstrName)
                tempTable.Rows.Add(lastValue)
            End If
        Next

        For j = 0 To tempTable.Rows.Count - 1
            'Below is using i to count the size of the table not counting nulls. 
            i = i + 1
            lvarNullTest = tempTable.Rows(j)(lstrName) 'lconRS.Fields(0).Value
            'Found a Null Value...
            If VarType(lvarNullTest) = VariantType.Null Then
                lintnull = 1
                lintTwoNulls = lintTwoNulls + 1
            ElseIf VarType(lvarNullTest) = VariantType.String Then
                'Test for the null string
                If Len(lvarNullTest) = 0 Then
                    lintnull = 1
                    lintTwoNulls = lintTwoNulls + 1
                End If
            End If
        Next

        If lintTwoNulls = 2 Then
            i = i - 1
        End If

        lVarTemp = New Variable
        lVarTemp.iSize = i   '.iSize is the number of distinct values the variable has. 
        ' Includes null as one. Multiple nulls, still only one.
        lVarTemp.strName = lstrName
        '.strName (singular) holds the variable name
        '.strNames (plural) holds the distinct values of the variable 
        '.iSize should be the same as Ubound(.strNames)+1  

        Dim rowCounter As Int32
        rowCounter = 0

        Dim rows() As DataRow
        ReDim rows(tempTable.Rows.Count)
        rows = tempTable.Select(String.Empty, lstrName & " ASC")

        'Make it a boolean 0 or 1 term
        If lintTwoNulls = 2 Then
            rowCounter = rowCounter + 1
        End If

        If i = 2 Then  'when i=2, Variable.iSize = 2 meaning it is essentially a boolean-type value "0" or "1".
            If lintnull = 1 Then Err.Raise(vbObjectError + 123456, , "<tlt>Dummy variable is missing or has only 1 value</tlt>")
            ReDim lVarTemp.strNames(1)
            lVarTemp.strNames(0) = rows(rowCounter)(lstrName)   'lconRS.Fields(0).Value
            rowCounter = rowCounter + 1    'lconRS.MoveNext()
            lVarTemp.strNames(1) = rows(rowCounter)(lstrName)   'lconRS.Fields(0).Value
            lVarTemp.iSize = 1
            lVarTemp.iType = 1 'iType = 1 means the value is boolean, but maybe not a number, i.e. True/False, Yes/No, "1" & "0" as strings, etc.

            canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(0))
            canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(1))

            'Make the smaller value the base, just incase it is needed
            lVarTemp.strBase = lVarTemp.strNames(0)
            'check if the variable is a boolean number variable
            If Val(lVarTemp.strNames(0)) = 0 And Val(lVarTemp.strNames(1)) = 1 Then
                If isDiscrete = 1 Then
                    lVarTemp.iType = 0  'iType = 0 means the variable is a boolean number; i.e. 0 & 1
                Else
                    lVarTemp.iType = 1
                End If
            End If
        ElseIf i = 1 Then
            ReDim lVarTemp.strNames(0)
            lVarTemp.strNames(0) = rows(rowCounter)(lstrName)  'lconRS.Fields(0).Value
            lVarTemp.iType = 2
        Else
            i = i - 1
            ReDim lVarTemp.strNames(i - 1)
            'If there is a Null In the Set, set the base = to the empty string
            If lintnull = 1 Then
                lVarTemp.strBase = mstraBLabels(1)
            Else
                lVarTemp.strBase = rows(rowCounter)(lstrName)  'lconRS.Fields(0).Value
            End If
            rowCounter = rowCounter + 1   'lconRS.MoveNext()
            For j = 0 To i - 1
                lVarTemp.strNames(j) = rows(rowCounter)(lstrName)  'lconRS.Fields(0).Value
                canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(j))
                rowCounter = rowCounter + 1   'lconRS.MoveNext()
            Next
            lVarTemp.iType = 2
            If Val(lVarTemp.strNames(0)) = 0 And Val(lVarTemp.strNames(1)) = 1 Then
                ' .iType = 4 means the choices for the variable are only 0 or 1.
                lVarTemp.iType = 4
            End If

            lVarTemp.iSize = lVarTemp.iSize - 1
        End If
        'ToDo: den4: Uncomment section below when CoxPH TimeDependent variables are working
        '...also need to make this section work with the tempTable
        If isTimeDependent Then
            mintTimeDepCount = mintTimeDepCount + 1
            lVarTemp.iType = 5

            ReDim rows(tempTable.Rows.Count)
            rows = tempTable.Select(lstrName & ", " & mstrTimeVar, mstrTimeVar & " ASC, " & mstrCensoredVar & " DESC")

            'Var type 2 signifies a dummy
            lVarTemp.strBase = "NO BASE"
            rowCounter = 0
            ldblPrev = rows(rowCounter)(lstrName)  'lconRS.Fields(0).Value
            lVarTemp.iSize = 1
            ReDim Preserve lVarTemp.strNames(0) '
            ReDim Preserve lVarTemp.dblTime(0) '
            lVarTemp.dblTime(0) = rows(rowCounter)(mstrTimeVar)  'lconRS.Fields(1).Value  
            lVarTemp.strNames(0) = rows(rowCounter)(lstrName)   'lconRS.Fields(0).Value 

            For rowCounter = 0 To rows.Count
                ldblCur = rows(rowCounter)(lstrName)   'lconRS.Fields(0).Value
                If ldblCur <> ldblPrev Then
                    ldblPrev = ldblCur
                    ReDim Preserve lVarTemp.dblTime(lVarTemp.iSize - 1) '
                    lVarTemp.dblTime(lVarTemp.iSize - 1) = rows(rowCounter)(mstrTimeVar) 'lconRS.Fields(1).Value
                End If
            Next rowCounter
        Else
            If lVarTemp.iType <> 0 Then
                'If it was not marked to be discrete, then attempt to make it continous
                If isDiscrete = 0 Then
                    'If the canbediscrete flag is not set, then it can be a discrete var
                    If canBeDiscrete <> 0 Then
                        lVarTemp.iSize = 1
                        'Make it a type 3 variable, continous, so that Null or Missing values reject
                        'The data row
                        lVarTemp.iType = 3
                        lVarTemp.strBase = "NONE"
                    End If
                End If
            End If
        End If

        Debug.Print(VB6.TabLayout("Name= " & lVarTemp.strName, "Size= " & lVarTemp.iSize, "Column= " & lVarTemp.iColumn))
        Debug.Print("-----------")
cleanup:

        Dummyfy = lVarTemp
        Exit Function
DummyError:
        Debug.Print("ERROR IN LABELING VARIABLE " & lstrName)
        Debug.Print("Variable has " & lintnull & " Null property " & lVarTemp.iType)
        Debug.Print(" i = " & i)
        Err.Raise(vbObjectError + 1, "Dummyfy", Err.Description)
        Exit Function
        Resume cleanup
        Resume
    End Function

    Public Function GetStrNamesFromList(ByRef lstTermList As List(Of String)) As List(Of String)
        'Parse the term array, searching for *
        Dim strpos As Integer
        Dim newStrPos As Integer
        Dim strTempTerm As String

        Dim lstOutList As List(Of String) = New List(Of String)

        'Loop through list of terms looking for double terms within a string.
        ' i.e. "Clinic*Clinic$" contains 2 terms within a single string.
        ' Add only unique terms.
        '      Not counting *, the term Clinic is not same as Clinic$, so both get added.

        'Add the first term
        For Each strTerm As String In lstTermList
            strpos = InStr(1, strTerm, "*")
            'If the term does not have an *, then add the term to the OutList,
            '  otherwise strip the * (or *'s), check for duplicates, then add unique terms
            '  to the OutList
            If strpos = 0 Then
                If Not lstOutList.Contains(strTerm) Then lstOutList.Add(strTerm)
            Else
                newStrPos = 1
                While Not newStrPos = 0
                    newStrPos = InStr(strpos + 1, strTerm, "*")
                    strTempTerm = Right(strTerm, Len(strTerm) - strpos)
                    If newStrPos = 0 Then
                        If Not lstOutList.Contains(strTempTerm) Then
                            lstOutList.Add(strTempTerm)
                        End If
                    Else
                        strTempTerm = Mid(strTerm, strpos + 1, newStrPos - strpos - 1)
                        If Not lstOutList.Contains(strTempTerm) Then
                            lstOutList.Add(strTempTerm)
                        End If
                        newStrPos = InStr(strpos + 1, strTerm, "*")
                        strpos = newStrPos
                    End If
                End While
            End If
        Next
        GetStrNamesFromList = lstOutList
    End Function


    Public Function GetCoxStrNames(ByRef lstraTerms() As String) As String()
        'Parse the term array, searching for *, AND for Epi 7 "," commas
        'count for the max number of *'s
        'this is for the term matrix
        Dim lszaTemp() As String
        Dim lStrAReturn() As String
        Dim i As Integer
        Dim j As Integer
        Dim strpos As Integer
        Dim nPos As Integer
        Dim newStrPos As Integer
        Dim lIntNNames As Integer
        Dim bOkayToAdd As Boolean
        Dim lstraTermsFromString() As String

        strpos = 0

        'For Epi7: We first need to split out the terms by their commas
        ' Then reset lstraTerms to be the new array of terms rather than one long string
        If UBound(lstraTerms) = 0 Then
            lstraTermsFromString = Split(lstraTerms(0), ",")
            ReDim lstraTerms(UBound(lstraTermsFromString))
            'If interaction terms are added (i.e. "Rx*wbc" as the product of Rx and wbc variables), 
            '  then some of the terms in the new lstraTerms will contain the 
            '  interaction delineator "*" to be handled below.
            lstraTerms = lstraTermsFromString
        End If

        lIntNNames = UBound(lstraTerms)

        For i = 0 To UBound(lstraTerms)
            strpos = InStr(1, lstraTerms(i), "*")
            While Not strpos = 0
                lIntNNames = lIntNNames + 1
                strpos = InStr(strpos + 1, lstraTerms(i), "*")
            End While
        Next

        ReDim lszaTemp(lIntNNames)
        nPos = 0

        'Fill the lszaStrings
        For i = 0 To UBound(lstraTerms) '
            strpos = InStr(1, lstraTerms(i), "*")
            If strpos = 0 Then

                lszaTemp(nPos) = lstraTerms(i)
                nPos = nPos + 1
            Else
                lszaTemp(nPos) = Left(lstraTerms(i), strpos - 1)
                nPos = nPos + 1

                newStrPos = 1
                While Not newStrPos = 0
                    newStrPos = InStr(strpos + 1, lstraTerms(i), "*")
                    If newStrPos = 0 Then
                        lszaTemp(nPos) = Right(lstraTerms(i), Len(lstraTerms(i)) - strpos)
                        nPos = nPos + 1
                    Else
                        lszaTemp(nPos) = Mid(lstraTerms(i), strpos + 1, newStrPos - strpos - 1)
                        strpos = newStrPos
                        nPos = nPos + 1
                    End If
                End While
            End If
        Next

        'Now must check for multiple terms.... in the term array
        ReDim lStrAReturn(lIntNNames)
        nPos = 0
        For i = 0 To UBound(lszaTemp)
            bOkayToAdd = True
            'For loop is "To -1" to skip the first time through
            For j = 0 To nPos - 1
                If StrComp(lStrAReturn(j), lszaTemp(i)) = 0 Then
                    lIntNNames = lIntNNames - 1
                    bOkayToAdd = False
                End If

            Next
            If bOkayToAdd = True Then
                lStrAReturn(nPos) = lszaTemp(i)
                nPos = nPos + 1
            End If
        Next

        ReDim Preserve lStrAReturn(lIntNNames)
        GetCoxStrNames = VB6.CopyArray(lStrAReturn)
    End Function

    Public Sub setCoxInteractionTerms(ByRef lstraTerms() As String, ByRef lIVarArray() As InteractionVariable)
        Dim i As Integer
        Dim k As Integer
        Dim strpos As Integer
        Dim nPos As Integer
        Dim newStrPos As Integer
        Dim mIntNNames As Long
        mIntNNames = UBound(lstraTerms)
        Dim lIntNTerms As Integer

        'Set the number of terms for the interaction array
        ReDim lIVarArray(UBound(lstraTerms) + mintTimeDepCount)
        'pre parse, setting the arrays in the interactionvariable array
        k = 0
        For i = 0 To UBound(lstraTerms)

            strpos = InStr(1, lstraTerms(i), "*")
            lIntNTerms = 0
            While Not strpos = 0
                lIntNTerms = lIntNTerms + 1
                'Check for Time Dependency
                nPos = InStr(strpos + 1, lstraTerms(i), "$")
                If nPos <> 0 Then
                    'Time Dependency Found, interact in a special way.
                    lIVarArray(i + k).iTerms = i + k
                    ReDim lIVarArray(i + k).Variables(0)
                    lIntNTerms = 1
                    k = k + 1
                    strpos = 0
                Else
                    strpos = InStr(strpos + 1, lstraTerms(i), "*")
                End If
            End While
            lIVarArray(i + k).iTerms = i + k
            ReDim lIVarArray(i + k).Variables(lIntNTerms)
        Next

        k = 0
        For i = 0 To UBound(lstraTerms)

            strpos = InStr(1, lstraTerms(i), "*")

            If strpos = 0 Then
                lIVarArray(i + k).Variables(0) = getVariableIndex(lstraTerms(i))

            Else
                'Do a time dependent check
                'Check for Time Dependency

                If InStr(strpos + 1, lstraTerms(i), "$") <> 0 Then
                    'Time Dependency Found, interact in a special way.
                    lIVarArray(i + k).Variables(0) = getVariableIndex(Left(lstraTerms(i), strpos - 1))
                    k = k + 1
                    lIVarArray(i + k).Variables(0) = getVariableIndex(Right(lstraTerms(i), Len(lstraTerms(i)) - strpos)) '

                Else

                    lIVarArray(i + k).Variables(0) = getVariableIndex(Left(lstraTerms(i), strpos - 1)) '
                    nPos = 2
                    newStrPos = 1
                    While Not newStrPos = 0
                        newStrPos = InStr(strpos + 1, lstraTerms(i), "*")
                        If newStrPos = 0 Then
                            lIVarArray(i + k).Variables(nPos - 1) = getVariableIndex(Right(lstraTerms(i), Len(lstraTerms(i)) - strpos)) '
                            nPos = nPos + 1
                        Else
                            lIVarArray(i + k).Variables(nPos - 1) = getVariableIndex(Mid(lstraTerms(i), strpos + 1, newStrPos - strpos - 1)) '
                            strpos = newStrPos
                            nPos = nPos + 1
                        End If
                    End While
                End If
            End If
        Next

    End Sub

    Public Function getVariableIndex(ByRef lstrName As String) As Integer
        Dim i As Integer

        For i = 0 To UBound(mVarArray)
            If StrComp(mVarArray(i).strName, lstrName) = 0 Then
                getVariableIndex = i
                Exit Function
            End If
        Next
        getVariableIndex = -1
    End Function

    'isNumber returns a True if lobj is a number, otherwise it returns False
    Public Function isNumber(ByRef lobj As Object) As Boolean
        If StrComp(Trim(CStr(Val(lobj))), Trim(lobj)) = 0 Then
            isNumber = vbTrue
        Else
            isNumber = vbFalse
        End If
    End Function

    'NOTE: SetStratified() for KMSurvival has been moved to EIKaplanMeierSurvival.vb and may be more
    ' up to date than the method below.
    Public Sub SetStratified(ByRef lstraStrata() As String, ByRef lSVarArray() As StrataVariable)
        Dim i As Integer
        Dim k As Integer
        Dim lvarNullTest As Object
        Dim lboolNull As Boolean


        ReDim lSVarArray(UBound(lstraStrata))

        If Len(lstraStrata(0)) = 0 Then
            lSVarArray(0).iTerms = 1
            lSVarArray(0).strName = "ALL"
            ReDim lSVarArray(0).straTerms(0)
            lSVarArray(0).straTerms(0) = "ALL"

        Else
            'Set up Strata Variable Array
            For k = 0 To UBound(lstraStrata)
                With lSVarArray(k)
                    .iTerms = 0
                    .strName = lstraStrata(k)
                    ReDim .straTerms(0)
                    'Open the record as a distinct set and count the non missing values
                    ' We must be able to do a SELECT DISTINCT on the data table; this code will replicate
                    ' that functionality using .NET code, since we can't do that operating directly against
                    ' a DataTable object using SQL.
                    Dim tempTable As DataTable
                    Dim rows() As DataRow
                    tempTable = New DataTable(dataTable.TableName)
                    tempTable.Columns.Add(lstraStrata(k), context.Columns(lstraStrata(k)).DataType)
                    Dim lastValue As Object
                    lastValue = VariantType.Null

                    rows = dataTable.Select("", lstraStrata(k))
                    For Each row As DataRow In rows

                        Dim columnEqual As Boolean
                        columnEqual = False

                        If lastValue Is Nothing And row(lstraStrata(k)) Is Nothing Then
                            columnEqual = True
                        ElseIf lastValue Is Nothing Or row(lstraStrata(k)) Is Nothing Then
                            columnEqual = False
                        Else
                            columnEqual = lastValue.Equals(row(lstraStrata(k)))
                        End If

                        If lastValue Is Nothing Or columnEqual = False Then
                            lastValue = row(lstraStrata(k))
                            tempTable.Rows.Add(lastValue)
                        End If

                    Next

                    'Set the null value counter to 0
                    lboolNull = False

                    For i = 0 To rows.Count - 1
                        lvarNullTest = rows(i)(mstrTimeVar)  'lconRS.Fields(0).Value
                        If VarType(lvarNullTest) = VariantType.Null Then
                            If lboolNull = False Then
                                ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                                .straTerms(UBound(.straTerms) - 1) = System.DBNull.Value
                                .iTerms = .iTerms + 1
                            End If
                            lboolNull = True
                        ElseIf VarType(lvarNullTest) = VariantType.String Then
                            If Len(lvarNullTest) = 0 Then
                                If lboolNull = False Then
                                    ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                                    .straTerms(UBound(.straTerms) - 1) = String.Empty
                                    .iTerms = .iTerms + 1
                                End If
                                lboolNull = True

                            Else
                                'If it is a normal number, add it to the array
                                ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                                .straTerms(UBound(.straTerms) - 1) = lvarNullTest
                                .iTerms = .iTerms + 1
                            End If
                        Else
                            'If it is a normal number, add it to the array
                            ReDim Preserve .straTerms(UBound(.straTerms) + 1)
                            .straTerms(UBound(.straTerms) - 1) = lvarNullTest
                            .iTerms = .iTerms + 1
                        End If
                    Next i
                    ReDim Preserve .straTerms(.iTerms)
                End With
            Next k
        End If
    End Sub

    Public Sub LoadStrata(ByRef lSVarArray() As StrataVariable, ByRef lStrataA() As Strata)
        Dim lintStrata As Integer
        Dim i As Integer
        Dim lstrQuery As String
        Dim lstrSortOrder As String
        lintStrata = 1 ' This needs to be at least 1 for the multiplication below
        'If there are multiple strata's find the combined levels in the strata
        For i = 0 To UBound(lSVarArray)
            lintStrata = lintStrata * lSVarArray(i).iTerms
        Next

        'Set the number of strata to it's correct value
        ReDim lStrataA(lintStrata - 1)
        lstrQuery = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "]"

        'Since if there is a weight, the third variable is gonna be the weight
        'Add the weight to the base query string
        If Len(mstrWeightVar) > 0 Then
            lstrQuery = lstrQuery & ", [" & mstrWeightVar & "]"
        End If

        'Check the covariate flag
        If UBound(mVarArray) >= 0 Then
            'if there are covariates add them to the base string
            lstrQuery = lstrQuery & ", [" & mVarArray(0).strName & "]"
            '
            'FOR loop must start at 1 to begin from the second item in the array
            For i = 1 To UBound(mVarArray)
                lstrQuery = lstrQuery & ", [" & mVarArray(i).strName & "]"
            Next
        End If

        'finally, append the table name to the base string
        If lintStrata = 1 Then

            'if there is only one level to load
            lstrSortOrder = "[" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC"

            For i = 0 To UBound(mVarArray)
                lstrSortOrder = lstrSortOrder & ", [" & mVarArray(i).strName & "] DESC"
            Next
            Debug.Print("lstrSortOrder = " & lstrSortOrder)

            'Use the load data function
            LoadData(lstrQuery, lstrSortOrder, 0, lStrataA)
        Else
            'Otherwise recurse through all Strata
            'And create individual data tables for all the strata
            StrataRecurse(lstrQuery, 0, 0, lStrataA, lSVarArray)
        End If

    End Sub

    Public Sub SetDataTableSizes()
        Dim k, i, lintTempFields, j As Integer

        For i = 0 To UBound(mVarArray)
            mVarArray(i).iColumn = lintTempFields + 1
            lintTempFields = lintTempFields + mVarArray(i).iSize
        Next
        mintRealFields = lintTempFields + mintOffset - 1
        'For Cox, there is the censored, surv time that must be there.  weight is optional
        mintVirtualFields = mintOffset - 1
        For i = 0 To UBound(mIVarArray)
            k = 1
            For j = 0 To UBound(mIVarArray(i).Variables)

                k = k * mVarArray(mIVarArray(i).Variables(j)).iSize
            Next
            mintVirtualFields = mintVirtualFields + k
        Next
        Debug.Print("Dummy fields " & mintRealFields)
        Debug.Print("Interaction fields " & mintVirtualFields)
        Debug.Print("Time Functions " & mintTimeDepCount)
    End Sub
    Private Sub StrataRecurse(ByVal lstrbase As String, ByVal lintLevel As Object, ByRef lintSIndex As Integer, ByRef lStrataA() As Strata, ByRef lSVarArray() As StrataVariable)
        Dim j As Integer
        Dim k As Integer

        Dim lstrOther As String
        Dim lstrQuery As String
        Dim lstrSortOrder As String

        lstrOther = String.Empty
        lstrQuery = String.Empty
        lstrSortOrder = String.Empty

        'If the level is the last strata var
        If lintLevel = UBound(lSVarArray) Then

            With lSVarArray(UBound(lSVarArray))
                'loop through each value of the strata
                For j = 0 To .iTerms - 1 '
                    If lintLevel = 0 Then
                        If IsDBNull(.straTerms(j)) Then
                            lstrQuery = lstrbase & " WHERE [" & .strName & "] IS NULL ORDER BY [" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC "
                        Else
                            lstrQuery = lstrbase & " WHERE [" & .strName & "] = " & Wrap(.straTerms(j)) & " ORDER BY [" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC "
                        End If
                    Else
                        If IsDBNull(.straTerms(j)) Then
                            lstrQuery = lstrbase & " AND [" & .strName & "] IS NULL ORDER BY [" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC "
                        Else
                            lstrQuery = lstrbase & " AND [" & .strName & "] = " & Wrap(.straTerms(j)) & " ORDER BY [" & mstrTimeVar & "], [" & mstrCensoredVar & "] DESC "
                        End If
                    End If
                    If Len(lStrataA(lintSIndex).strName) <> 0 Then
                        lStrataA(lintSIndex).strName = lStrataA(lintSIndex).strName & " " & .strName & " " & .straTerms(j)
                    Else
                        lStrataA(lintSIndex).strName = .strName & " " & .straTerms(j - 1)
                    End If

                    'Time to load the data
                    LoadData(lstrQuery, lstrSortOrder, lintSIndex, lStrataA)
                    lintSIndex = lintSIndex + 1
                Next
            End With
            Exit Sub
        End If

        With lSVarArray(lintLevel)
            For j = 0 To .iTerms - 1 '
                If lintLevel = 0 Then
                    If .straTerms(j) = "" Then
                        lstrQuery = lstrbase & " WHERE [" & .strName & "] = " & VariantType.Null & " AND [" & .strName & "] = " & Chr(34) & Chr(34)
                    Else
                        lstrOther = "WHERE [" & .strName & "] = " & Wrap(.straTerms(j)) & " "
                    End If
                Else
                    If .straTerms(j) = "" Then
                        lstrQuery = lstrbase & "AND [" & .strName & "] = " & VariantType.Null & " AND [" & .strName & "] = " & Chr(34) & Chr(34)
                    Else
                        lstrOther = " AND [" & .strName & "] = " & Wrap(.straTerms(j)) & " "
                    End If
                End If
                For k = 0 To lSVarArray(lintLevel + 1).iTerms - 1 '
                    lStrataA(lintSIndex + k - 1).strName = lStrataA(lintSIndex + k - 1).strName & " " & .strName & " " & .straTerms(j)
                Next k
                StrataRecurse(lstrbase & lstrOther, lintLevel + 1, lintSIndex, lStrataA, lSVarArray)
            Next
        End With
    End Sub

    Private Sub LoadData(ByRef lstrQuery As String, ByRef lstrSortOrder As String, ByRef lintIndex As Integer, ByRef lStrataA() As Strata)        'Dim debugstring As String
        Dim lvarCurData As Object
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim lpos As Integer
        Dim lintWeight As Integer
        Dim lintOffset As Integer
        Dim ldblatemp() As Double
        Dim ldblTime As Double
        Dim bTempValuesReset As Boolean
        Dim strCurData As String
        Dim dbugString As String
        'If there are dummy terms, the ldblatemp stores the temporary data.
        ReDim ldblatemp(mintRealFields - mintOffset)  '
        Debug.Print(lstrQuery)
        'Open the Data Set

        Dim table As New DataTable
        table = dataTable

        Dim rows() As DataRow
        ReDim rows(table.Rows.Count)
        rows = table.Select(String.Empty, lstrSortOrder)
        k = 0
        lintWeight = mintWeight
        'Count the number of entries.
        k = table.Rows.Count

        If k = 0 Then Err.Raise(vbObjectError, "", "<tlt>No Data in Data Array</tlt>")

        'Set the Columns of the strata, and the rows
        lStrataA(lintIndex).lintcols = mintVirtualFields
        lStrataA(lintIndex).lintrows = k

        Debug.Print("Continuing to Load Strata " & lStrataA(lintIndex).strName)
        Debug.Print("Columns = " & lStrataA(lintIndex).lintcols)
        Debug.Print("Rows = " & lStrataA(lintIndex).lintrows)

        If k = 0 Then
            ReDim lStrataA(lintIndex).dblaData(0, mintVirtualFields - 1)    '
            lintOffset = 1
        Else
            ReDim lStrataA(lintIndex).dblaData(k - 1, mintVirtualFields - 1)    '
        End If
        dbugString = "Contents of lStratA(" & lintIndex & ").dblaData(x, y)"
        Debug.Print(dbugString)
        dbugString = "Row(x)    Offset    TimeVar   CensorVar WeightVar CoVariates"
        Debug.Print(dbugString)

        Dim rowCounter As Int32
        rowCounter = 0
        For rowCounter = 0 To rows.Count - 1
            dbugString = rowCounter.ToString
            lvarCurData = rows(rowCounter)(mstrTimeVar) 'lconRS.Fields(0).Value
            If VarType(lvarCurData) = VariantType.Null Then
                'Missing Values get special attention
                'Increment the offset counter which skips this data row
                lintOffset = lintOffset + 1
                dbugString = dbugString & "            " & lintOffset & "   ||"
                GoTo MISSING
            ElseIf rows(rowCounter)(mstrTimeVar) <= 0 Then   'ElseIf lconRS.Fields(0).Value <= 0 Then
                Err.Raise(vbObjectError + 123123123, "Load Data", "<tlt>Survival times must be greater than zero.</tlt>")
            Else
                'Sets DataArray(i, 0)
                lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 0) = lvarCurData  'lconRS.Fields(0).Value  '
                dbugString = dbugString & "        " & lintOffset & "         " & lvarCurData

            End If

            ldblTime = rows(rowCounter)(mstrTimeVar) 'ldblTime = lconRS.Fields(0).Value

            'Read the next column, the Censored Variable
            'Note: In Epi3, the temp table is constructed with the SQL Select where
            ' SELECT iif(" & taVariable(0).sExpression & "=" & s & ",1,0)
            '   .sExpression is the Censored var and s is the value for uncensored.
            '   so, when Censored_var = Uncensored_value then it returns a 1, otherwise, including this
            '   condition when Censored_var is VariantType.Null, then it returns 0.
            'As long as the value for uncensored is not null, then when it is null, return a zero.
            lvarCurData = rows(rowCounter)(mstrCensoredVar)  'lconRS.Fields(1).Value
            If VarType(lvarCurData) = VariantType.Null Then
                'Same story as above, if missing, IGNORE Data Row
                'Upgrade_Note: The VB6 code pre-processed records to convert any null CensorVars to be either 0 or 1
                '  based on the value of the UncensoredVar.

                'lintOffset = lintOffset + 1
                'dbugString = dbugString & "   ||"
                'GoTo MISSING


                'If lvarCurData *IS* null, then you can't compare it to a string, so check
                '  to see if mstrUncensoredVal is also null (or missing). If so, set a 1 for dblaData, 
                '  otherwise set it to 0.

                lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 1) = IIf(mstrUncensoredVal = "", 1, 0)   'lconRS.Fields(1).Value 
                dbugString = dbugString & "        " & lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 1)


            Else
                'If isNumber(lvarCurData) Then
                'If lvarCurData <> 1 And lvarCurData <> 0 Then Err.Raise(vbObjectError + 123123123, "Load Data", "<tlt>Censor variables must be 0 or 1.</tlt>")
                strCurData = CType(lvarCurData, String)
                lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 1) = IIf(strCurData = mstrUncensoredVal, 1, 0)   'lconRS.Fields(1).Value 
                dbugString = dbugString & "        " & lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 1)

            End If

            'If a Weight Variable Exists...
            If lintWeight = 1 And Not bTempValuesReset Then
                lvarCurData = rows(rowCounter)(mstrWeightVar) 'lconRS.Fields(2).Value

                If VarType(lvarCurData) = VariantType.Null Then
                    lintOffset = lintOffset + 1
                    'lconRS.Fields(0).Value = VariantType.Null
                    'lconRS.Update()
                    dbugString = dbugString & "   ||"
                    GoTo MISSING
                    bTempValuesReset = ResetTempValues(ldblatemp)
                Else
                    If lvarCurData <= 0 Then Err.Raise(vbObjectError, "", "<tlt>Weight Var must be greater than 0. Current weight: " & lvarCurData & "</tlt>")
                    lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 2) = rows(rowCounter)(mstrWeightVar)   'lconRS.Fields(2).Value  '
                    dbugString = dbugString & "        " & lStrataA(lintIndex).dblaData(rowCounter - lintOffset, 2)
                End If
            End If

            'Now the 2 or 3 non covariate values have been read,
            'Time, Censor, (and Weight, if applicable)
            'Read from the mintOffset to the last value of the fields
            'Mintoffset = 3 if no weight, 4 if weight 
            For j = 0 To UBound(mstraCovariates)
                dbugString = dbugString & "          " & mVarArray(j).strName & ":"

                lvarCurData = rows(rowCounter)(mstraCovariates(j))
                If VarType(lvarCurData) = VariantType.Null Then
                    'If a null value, recode to empty string
                    dbugString = dbugString & "null "
                    lvarCurData = ""
                End If

                'Now, look at the mvararray, to see what type of variable this is
                With mVarArray(j)
                    'If the variable is of type 0, it is a boolean with no missing values
                    dbugString = dbugString & "     mVarArray(" & j & ").iType=" & .iType

                    Select Case .iType
                        Case 0
                            'Boolean with no missing gets dumped right into the temp array
                            ldblatemp(.iColumn - 1) = lvarCurData
                        Case 1
                            'Otherwise, the variable is type one, Then it has a base string, and another 
                            If Not (StrComp(.strBase, lvarCurData.ToString) = 0) Then
                                'if not the base string, this event gets recoded with a 1
                                'Otherwise it is 0  
                                ldblatemp(.iColumn - 1) = 1
                            End If
                        Case 2, 4
                            'For Variables of type two, it is a dummy null with string values
                            'If the variable has a base value, it will not be in the string array
                            'If a variable is found, it will not be a base variable.
                            'if lvarcurdata contains a Null, the loop will end, without finding
                            'The Null Variable, causing every element in this dummy to be 0
                            For k = 0 To .iSize - 1 '
                                Debug.Print(k & ", " & .strNames(k) & ", " & lvarCurData)
                                If StrComp(.strNames(k), lvarCurData) = 0 Then
                                    'Find the column to put the 1 in
                                    ' for example 0,0,1 or 1,0,0
                                    Debug.Print("Strings are equal, ldblatemp(" & .iColumn + k - 1 & ") = 1")
                                    ldblatemp(.iColumn + k - 1) = 1
                                End If
                            Next
                        Case 5
                            'if the variable is a time dependent one, it is coded as its many different values
                            'A recent change had the time dependent code changed so that the .isize of a time dep
                            'variable is always of size one.
                            'The for loop could be removed
                            For k = 0 To .iSize - 1
                                ldblatemp(.iColumn + k - 1) = lvarCurData
                            Next
                            'If the variable is of type 3, Null values must be rejected
                            'Type three is Continous
                        Case 3
                            If Len(lvarCurData) = 0 Then
                                lintOffset = lintOffset + 1
                                'lconRS.Fields(0).Value = VariantType.Null
                                'lconRS.Update()
                                GoTo MISSING
                            Else
                                'otherwise we can put the value in the array
                                ldblatemp(.iColumn - 1) = lvarCurData
                            End If
                    End Select
                    dbugString = dbugString & "       ldblatemp(mVarArray(" & j & ").iColumn - 1)=" & ldblatemp(.iColumn - 1)
                    Debug.Print(dbugString)

                End With
            Next j

            'If we made it here, fix the data so it fits in the data array
            'Use the RecursiveFactorializeCox function to combine any interaction terms
            'Internally, Interaction Variables are supported, but there is no interface to get to them
            'Sets DataArray(i,2) inside the RecurseCox function
            RecursiveFactorializeCox(ldblatemp, rowCounter - lintOffset, lStrataA(lintIndex).dblaData, mintOffset)
MISSING:
            Debug.Print(dbugString)

            'If a missing value was found in the data row, or the data row is done reading
            'Reset all the temp values
            'bTempValuesReset should always be False after next line; only goes into ResetTempValues if not TempValsReset already
            'bTempValuesReset = IIf(bTempValuesReset, False, Not ResetTempValues(ldblatemp))
            For j = 0 To UBound(ldblatemp)
                ldblatemp(j) = 0
            Next j
        Next rowCounter
        'lintOffset is incremented for each row that has a null value for any of the 2 or 3 main variables (Time, Censor, or Weight)
        'These rows are overwritten in the array to exclude them from the calculations therefore
        ' causing the count of rows (.lintrows) to be .lintrows - lintOffset
        lStrataA(lintIndex).lintrows = lStrataA(lintIndex).lintrows - lintOffset


        ReDim ldblatemp(UBound(mVarArray))
        ReDim lStrataA(lintIndex).mdblaTime(lStrataA(lintIndex).lintrows)
        ldblTime = lStrataA(lintIndex).dblaData(0, 0)
        lStrataA(lintIndex).mdblaTime(0) = ldblTime
        'Search Data to Find The Amount of discrete time function positions
        lpos = 1
        '==================================================================
        'Time Dependent Data Fixers
        'Representation of TimeDep Covariates Changed MANY MANY MANY times
        'In order to increase speed, and decrease amount of space taken up in memory
        If Not (mstraTimeDependentVar(0) = Nothing) Then
            For i = 0 To UBound(mVarArray)    '
                ldblatemp(i) = lStrataA(lintIndex).dblaData(0, mVarArray(0).iColumn + mintOffset - 2)
            Next i
            For j = 0 To lStrataA(lintIndex).lintrows
                For i = 0 To UBound(mVarArray)
                    If mVarArray(i).iType = 5 Then
                        If ldblatemp(i) <> lStrataA(lintIndex).dblaData(j, mVarArray(i).iColumn + mintOffset - 2) Then

                            If ldblTime <> lStrataA(lintIndex).dblaData(j, 1) Then
                                ldblTime = lStrataA(lintIndex).dblaData(j, 1)
                                lStrataA(lintIndex).mdblaTime(lpos) = ldblTime
                                lpos = lpos + 1
                            End If
                            For k = 0 To UBound(mVarArray)
                                ldblatemp(k) = lStrataA(lintIndex).dblaData(j, mVarArray(k).iColumn + mintOffset - 2)
                            Next k
                            i = k
                        End If
                    End If
                Next i
            Next j
            '==================================================================
            ReDim Preserve lStrataA(lintIndex).mdblaTime(lpos - 1)
            ReDim lStrataA(lintIndex).intaTimeSelectors(lStrataA(lintIndex).lintcols - 2 - lintWeight)
            ReDim lStrataA(lintIndex).intaDataColumns(lStrataA(lintIndex).lintcols - 2 - lintWeight - mintTimeDepCount)
            lpos = 0
            For i = 0 To UBound(mVarArray)
                If mVarArray(i).iType = 5 Then
                    For j = 0 To mVarArray(i - 2).iSize
                        lStrataA(lintIndex).intaTimeSelectors(mVarArray(i - 1).iColumn + j - 1) = mVarArray(i).iColumn

                    Next j
                Else
                    For j = 0 To mVarArray(i).iSize - 1
                        lStrataA(lintIndex).intaTimeSelectors(mVarArray(i).iColumn + j - 1) = -1
                        lStrataA(lintIndex).intaDataColumns(lpos) = mVarArray(i).iColumn + j - 1
                        lpos = lpos + 1
                    Next j
                End If
            Next i
        End If
        '=====================================================
        Debug.Print("")
        Debug.Print("Variable Names")
        For i = 0 To UBound(mVarArray)
            System.Diagnostics.Debug.Write(VB6.TabLayout(mVarArray(i).strName, TAB))
            For j = 0 To UBound(mVarArray(i).strNames)
                System.Diagnostics.Debug.Write(VB6.TabLayout(mVarArray(i).strNames(j), TAB))
            Next j
            Debug.Print("+++")
        Next i
        Debug.Print("--------------")
        Debug.Print("other time array")
        For i = 0 To UBound(lStrataA(lintIndex).mdblaTime)
            System.Diagnostics.Debug.Write(VB6.TabLayout(lStrataA(lintIndex).mdblaTime(i), TAB))
        Next
        Debug.Print("===")
    End Sub

    Public Function Wrap(ByRef value As Object) As Object
        If VarType(value) = VariantType.String Then
            Wrap = Chr(39) & value.Replace("'", "''") & Chr(39)
        Else
            Wrap = value
        End If

    End Function

    Public Sub makeCoxMatrixLabels(ByRef lstrbase As String, ByRef lintvar As Integer, ByRef lilevel As Integer, ByRef lintpos As Integer)
        Dim i As Integer
        Dim j As Integer
        Dim k As Integer
        Dim linttemppos As Integer
        Dim lstrpasson As String
        Dim lstrOther As String
        Dim lstrBoolean As String

        lstrpasson = ""

        'If it is the first interaction level, do not put an ampersand on the label
        If lilevel = 0 Then
            lstrOther = ""
        Else
            lstrOther = "*"
        End If

        'if at the last interaction term,  then create the number of labels for each dummy variable
        'within the last term
        If lilevel = UBound(mIVarArray(lintvar).Variables) Then
            'Get the variable index of the dummy
            i = mIVarArray(lintvar).Variables(lilevel)
            'Loop through each dummy within the variable
            For j = 0 To mVarArray(i).iSize - 1
                'Get it's position in the output matrix by it's column representation
                linttemppos = mVarArray(i).iColumn + j - 1
                'Tack on the base label
                mStrAMatrixLabels(lintpos) = lstrbase & lstrOther
                'If the variable has multiple dummy's output the dummy names
                'Otherwise output the variables base name
                If mVarArray(i).iType = 2 Then
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strName & "(" & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strNames(j) & "/" & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strBase & ") "
                ElseIf mVarArray(i).iType = 4 Then
                    If CDbl(mVarArray(mIVarArray(lintvar).Variables(lilevel)).strNames(j)) = 1 Then
                        lstrBoolean = "Yes"
                    Else
                        lstrBoolean = "No"
                    End If
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strName & " (" & lstrBoolean & "/" & mstraBLabels(2) & ") "
                ElseIf mVarArray(i).iType = 1 Then
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strName & " (" & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strNames(1) & "/" & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strNames(0) & ") "
                ElseIf mVarArray(i).iType = 5 Then
                    For k = 0 To mVarArray(i).iSize  '
                        mStrAMatrixLabels(lintpos - k) = mStrAMatrixLabels(lintpos - k) & "(t)"
                    Next k
                    Exit Sub
                ElseIf mVarArray(i).iType = 0 Then
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel)).strName & "(Yes/No)"
                Else
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(i).strName
                End If
                'increase the position of the matrix labels "pointer"
                lintpos = lintpos + 1
            Next
            Exit Sub
        End If

        'We are not at the top level so loop through each dummy within the variable
        'and call this function again.
        For j = 0 To mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).iSize - 1
            'Get the value of the interaction, and pass it onwards
            'depending upon the type
            If mVarArray(mIVarArray(lintvar).Variables(lilevel)).iType = 2 Then
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strName & "(" & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strNames(j) & "/" & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strBase & ") "
            ElseIf mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).iType = 4 Then
                If CDbl(mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strNames(j)) = 1 Then
                    lstrBoolean = "Yes"
                Else
                    lstrBoolean = "No"
                End If
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strName & " (" & lstrBoolean & "/" & mstraBLabels(1) & ") "
            ElseIf mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).iType = 1 Then
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strName & " (" & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strNames(1) & "/" & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strNames(0) & ") "
            ElseIf mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).iType = 5 Then
                makeCoxMatrixLabels(lstrpasson, lintvar, lilevel + 1, lintpos)
                Exit Sub
            Else
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar - 1).Variables(lilevel - 1) - 1).strName  '
            End If
            makeCoxMatrixLabels(lstrpasson, lintvar, lilevel + 1, lintpos)
        Next
        Exit Sub
    End Sub
End Module