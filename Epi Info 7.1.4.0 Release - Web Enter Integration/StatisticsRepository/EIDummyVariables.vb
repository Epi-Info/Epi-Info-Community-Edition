Option Strict Off
Option Explicit On
Module EIDummyVariables

    'A Type to organize dummy vars
    'For non dummy variables, the size is 1
    Structure Variable
        Dim iSize As Integer
        Dim iColumn As Integer
        Dim strName As String
        Dim strBase As String
        Dim strNames() As String
        Dim iType As Integer
        Dim dblTime() As Double
    End Structure

    Structure InteractionVariable
        Dim iTerms As Integer
        Dim Variables() As Integer
    End Structure

    'For dummy variables
    Public mVarArray() As Variable
    Public mIVarArray() As InteractionVariable


    Public Sub RecursiveFactorialize(ByRef ldblA() As Double, ByRef r As Integer)
        Dim i As Integer
        Dim lpos As Integer
        lpos = 1
        If Len(mstrMatchVar) > 0 Then
            lpos = lpos + 1
        End If
        If Len(mstrWeightVar) > 0 Then
            lpos = lpos + 1
        End If

        For i = 0 To UBound(mstraTerms)
            Recurse(ldblA, lpos, 0, i, 1, r)
        Next
    End Sub

    Public Sub Recurse(ByRef ldblA() As Double, ByRef lpos As Integer, ByRef lilevel As Integer, ByRef lIntTerm As Integer, ByVal val_Renamed As Double, ByRef lintdepth As Integer)

        Dim dbltemp As Double
        Dim j As Integer
        Dim linttemppos As Integer

        'if the level is the top most, do the array setting..
        If lilevel = UBound(mIVarArray(lIntTerm).Variables) Then
            For j = 1 To mVarArray(mIVarArray(lIntTerm).Variables(lilevel) - 1).iSize
                linttemppos = mVarArray(mIVarArray(lIntTerm).Variables(lilevel) - 1).iColumn + j - 2
                DataArray(lintdepth, lpos) = val_Renamed * ldblA(linttemppos)
                lpos = lpos + 1
            Next
            Exit Sub
        End If

        For j = 1 To mVarArray(mIVarArray(lIntTerm).Variables(lilevel) - 1).iSize
            'Get the value of the interaction, and pass it onwards
            linttemppos = mVarArray(mIVarArray(lIntTerm).Variables(lilevel) - 1).iColumn + j - 2
            dbltemp = val_Renamed * ldblA(linttemppos)
            Recurse(ldblA, lpos, lilevel + 1, lIntTerm, dbltemp, lintdepth)
        Next
    End Sub
    'Linear Dummify function
    Public Function DummyfyLinear(ByRef lStrName As String, ByRef context As EpiInfo.Plugin.IAnalysisStatisticContext, ByRef currentTable As DataTable) As Variable

        Dim tempTable As DataTable
        Dim nullFreeTable As DataTable
        Dim lVarTemp As Variable
        Dim i As Integer
        Dim j As Integer
        Dim lintnull As Integer
        Dim lVarNullTest As Object
        Dim lintTwoNulls As Integer
        'Dim lstrTemp As String
        Dim lStrAVarNames() As String
        Dim selectQuery As String

        Dim isDiscrete As Integer
        Dim canBeDiscrete As Integer
        '    On Error GoTo DummyError

        'Use 0's and 1's to denote true or false. that way they can be multiplied
        isDiscrete = 0
        canBeDiscrete = 1
        lintTwoNulls = 0

        lStrAVarNames = VB6.CopyArray(GetStrNames)

        selectQuery = ""

        For i = 0 To UBound(lStrAVarNames)
            selectQuery = selectQuery + " [" + lStrAVarNames(i) + "] IS NOT NULL AND"
        Next

        selectQuery = selectQuery.Remove(selectQuery.Length - 4, 4)

        Dim rows() As DataRow
        'rows = context.CurrentDataTable.Select(selectQuery)
        rows = currentTable.Select(selectQuery)

        nullFreeTable = New DataTable(currentTable.TableName)
        nullFreeTable = currentTable.Copy()
        nullFreeTable.Rows.Clear()
        'nullFreeTable.Columns..Add(lStrName, context.CurrentDataTable.Columns(lStrName).DataType)
        rows.CopyToDataTable(nullFreeTable, System.Data.LoadOption.OverwriteChanges)

        'context.CurrentDataTable.
        tempTable = New DataTable(currentTable.TableName)
        tempTable.Columns.Add(lStrName, currentTable.Columns(lStrName).DataType)



        Dim lastValue As Object
        lastValue = VariantType.Null

        ' We must be able to do a SELECT DISTINCT on the data table; this code will replicate
        ' that functionality using .NET code, since we can't do that operating directly against
        ' a DataTable object using SQL.
        For Each row As DataRow In nullFreeTable.Select("", "[" + lStrName + "]") 'context.CurrentDataTable.Select("", lStrName)

            Dim columnEqual As Boolean
            columnEqual = False

            If lastValue Is Nothing And row(lStrName) Is Nothing Then '= VariantType.Null And row(lStrName) = VariantType.Null Then
                columnEqual = True
            ElseIf lastValue Is Nothing Or row(lStrName) Is Nothing Then '= VariantType.Null Or row(lStrName) = VariantType.Null Then
                columnEqual = False
            Else
                columnEqual = lastValue.Equals(row(lStrName))
            End If

            If lastValue Is Nothing Or columnEqual = False Then '= VariantType.Null Or columnEqual = False Then
                lastValue = row(lStrName)
                tempTable.Rows.Add(lastValue)
            End If

        Next

        'CHECK FOR VARIABLE IN THE DISCRETE ARRAY
        If UBound(mStrADiscrete) >= 0 Then
            For i = 0 To UBound(mStrADiscrete)
                If (StrComp(mStrADiscrete(i), lStrName, CompareMethod.Text) = 0) Then
                    isDiscrete = 1
                End If
            Next
        End If

        'Open the record as a distinct set and count the non missing values
        'lconRS.Open("SELECT DISTINCT [" & lStrName & "] from [" & mstrTableName & "]", gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly)
        i = 0
        'Null Counter
        lintnull = 0

        For j = 0 To tempTable.Rows.Count - 1
            'While reader.Read()
            'While Not lconRS.EOF
            i = i + 1

            'UPGRADE_WARNING: Couldn't resolve default property of object lVarNullTest. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            lVarNullTest = tempTable.Rows(j)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            'Found a Null Value...
            'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
            If VarType(lVarNullTest) = VariantType.Null Then
                lintnull = 1
                lintTwoNulls = lintTwoNulls + 1
                'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
            ElseIf VarType(lVarNullTest) = VariantType.String Then
                'Test for the null string
                If Len(lVarNullTest) = 0 Then
                    lintnull = 1
                    lintTwoNulls = lintTwoNulls + 1
                End If
            End If

            'lconRS.MoveNext()
            'End While
        Next

        If lintTwoNulls = 2 Then
            i = i - 1
        End If
        If i = 1 Then Err.Raise(vbObjectError, , "<tlt>Dummy variable must contain more than one value</tlt>")
        lVarTemp.iSize = i
        lVarTemp.strName = lStrName

        Dim rowCounter As Int32
        rowCounter = 0

        'Dim rows() As DataRow
        ReDim rows(tempTable.Rows.Count)
        rows = tempTable.Select(String.Empty, "[" + lStrName & "] ASC")

        'reader = context.DataSource.GetDataTableReader("SELECT DISTINCT [" & lStrName & "] from [" & mstrTableName & "] ORDER BY [" & lStrName & "] ASC")
        'reader.Read()
        'lconRS.Open("SELECT DISTINCT [" & lStrName & "] from [" & mstrTableName & "] ORDER BY [" & lStrName & "] ASC", gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly)
        'Make it a boolean 0 or 1 term
        If lintTwoNulls = 2 Then
            'lconRS.MoveNext()
            'reader.Read()
            rowCounter = rowCounter + 1
        End If

        If i = 2 Then
            If lintnull = 1 Then Err.Raise(vbObjectError, "", "<tlt>Dummy variable must contain more than one level</tlt>")
            'UPGRADE_WARNING: Lower bound of array lVarTemp.strNames was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim lVarTemp.strNames(1)
            lVarTemp.strNames(0) = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            rowCounter = rowCounter + 1 'reader.Read()
            'lconRS.MoveNext()
            lVarTemp.strNames(1) = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            lVarTemp.strBase = lVarTemp.strNames(0)
            lVarTemp.iSize = 1
            lVarTemp.iType = 1

            canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(0))
            canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(1))

            'Make the smaller value the base, just incase it is needed
            'lVarTemp.strBase = lconRS.Fields(0).Value
            'check if the variable is a boolean number variable
            If Val(lVarTemp.strNames(0)) = 0 And Val(lVarTemp.strNames(1)) = 1 Then
                'BOOLEAN YES NO
                lVarTemp.iType = 0
            End If
        Else
            i = i - 1
            'UPGRADE_WARNING: Lower bound of array lVarTemp.strNames was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim lVarTemp.strNames(i - 1)
            'If there is a Null In the Set, set the base = to the empty string
            If lintnull = 1 Then
                lVarTemp.strBase = ""
            Else
                lVarTemp.strBase = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            End If
            rowCounter = rowCounter + 1 'reader.Read() 'lconRS.MoveNext()
            For j = 0 To i - 1
                lVarTemp.strNames(j) = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
                canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(j))
                rowCounter = rowCounter + 1 'reader.Read() 'lconRS.MoveNext()
            Next
            lVarTemp.iType = 2

            If Val(lVarTemp.strNames(0)) = 0 And Val(lVarTemp.strNames(1)) = 1 Then
                lVarTemp.iType = 4
            End If

            lVarTemp.iSize = lVarTemp.iSize - 1
        End If

        'If it was not marked to be discrete, then attempt to make it continous
        If isDiscrete = 0 Then
            'If the canbediscrete flag is not set, then it can be a discrete var
            If canBeDiscrete = 1 Then
                lVarTemp.iSize = 1
                'UPGRADE_WARNING: Lower bound of array lVarTemp.strNames was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
                ReDim lVarTemp.strNames(0)

                'Make it a type 3 variable, continous, so that Null or Missing values reject
                'The data row
                lVarTemp.iType = 3
                lVarTemp.strBase = "NONE"
            End If
        End If

cleanup:
        'lconRS.Close()
        'UPGRADE_NOTE: Object lconRS may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        'lconRS = Nothing
        'reader.Close()
        'reader.Dispose()

        'UPGRADE_WARNING: Couldn't resolve default property of object Dummyfy. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        DummyfyLinear = lVarTemp
        Exit Function
        'DummyError:
        '    Debug.Print "ERROR IN LABELING VARIABLE " & lStrName
        '    Debug.Print "Variable has " & lintnull & " Null property " & lVarTemp.iType
        '    Debug.Print " i = " & i
        '    Resume cleanup

    End Function

    'Logistic Dummify function
    Public Function DummyfyLogistic(ByRef lStrName As String, ByRef currentTable As DataTable) As Variable

        Dim tempTable As DataTable
        Dim lVarTemp As Variable
        Dim i As Integer
        Dim j As Integer
        Dim lintnull As Integer
        Dim lVarNullTest As Object
        Dim lintTwoNulls As Integer
        'Dim lstrTemp As String

        Dim isDiscrete As Integer
        Dim canBeDiscrete As Integer
        '    On Error GoTo DummyError

        'Use 0's and 1's to denote true or false. that way they can be multiplied
        isDiscrete = 0
        canBeDiscrete = 1
        lintTwoNulls = 0

        tempTable = New DataTable(currentTable.TableName)
        tempTable.Columns.Add(lStrName, currentTable.Columns(lStrName).DataType)

        Dim lastValue As Object
        lastValue = VariantType.Null

        ' We must be able to do a SELECT DISTINCT on the data table; this code will replicate
        ' that functionality using .NET code, since we can't do that operating directly against
        ' a DataTable object using SQL.
        For Each row As DataRow In currentTable.Select("", "[" + lStrName + "]")

            Dim columnEqual As Boolean
            columnEqual = False

            If lastValue Is Nothing And row(lStrName) Is Nothing Then '= VariantType.Null And row(lStrName) = VariantType.Null Then
                columnEqual = True
            ElseIf lastValue Is Nothing Or row(lStrName) Is Nothing Then '= VariantType.Null Or row(lStrName) = VariantType.Null Then
                columnEqual = False
            Else
                columnEqual = lastValue.Equals(row(lStrName))
            End If

            If lastValue Is Nothing Or columnEqual = False Then '= VariantType.Null Or columnEqual = False Then
                lastValue = row(lStrName)
                tempTable.Rows.Add(lastValue)
            End If

        Next

        'CHECK FOR VARIABLE IN THE DISCRETE ARRAY
        If UBound(mStrADiscrete) >= 0 Then
            For i = 0 To UBound(mStrADiscrete)
                If (StrComp(mStrADiscrete(i), lStrName, CompareMethod.Text) = 0) Then
                    isDiscrete = 1
                End If
            Next
        End If

        'Open the record as a distinct set and count the non missing values
        'lconRS.Open("SELECT DISTINCT [" & lStrName & "] from [" & mstrTableName & "]", gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly)
        i = 0
        'Null Counter
        lintnull = 0

        For j = 0 To tempTable.Rows.Count - 1
            'While reader.Read()
            'While Not lconRS.EOF
            i = i + 1

            'UPGRADE_WARNING: Couldn't resolve default property of object lVarNullTest. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
            lVarNullTest = tempTable.Rows(j)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            'Found a Null Value...
            'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
            If VarType(lVarNullTest) = VariantType.Null Then
                lintnull = 1
                lintTwoNulls = lintTwoNulls + 1
                'UPGRADE_WARNING: VarType has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
            ElseIf VarType(lVarNullTest) = VariantType.String Then
                'Test for the null string
                If Len(lVarNullTest) = 0 Then
                    lintnull = 1
                    lintTwoNulls = lintTwoNulls + 1
                End If
            End If

            'lconRS.MoveNext()
            'End While
        Next

        If lintTwoNulls = 2 Then
            i = i - 1
        End If
        If i = 1 Then Err.Raise(vbObjectError, , "<tlt>Dummy variable must contain more than one value</tlt>")
        lVarTemp.iSize = i
        lVarTemp.strName = lStrName

        Dim rowCounter As Int32
        rowCounter = 0

        Dim rows() As DataRow
        ReDim rows(tempTable.Rows.Count)
        rows = tempTable.Select(String.Empty, "[" + lStrName & "] ASC")

        'reader = context.DataSource.GetDataTableReader("SELECT DISTINCT [" & lStrName & "] from [" & mstrTableName & "] ORDER BY [" & lStrName & "] ASC")
        'reader.Read()
        'lconRS.Open("SELECT DISTINCT [" & lStrName & "] from [" & mstrTableName & "] ORDER BY [" & lStrName & "] ASC", gconDB, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly)
        'Make it a boolean 0 or 1 term
        If lintTwoNulls = 2 Then
            'lconRS.MoveNext()
            'reader.Read()
            rowCounter = rowCounter + 1
        End If

        If i = 2 Then
            If lintnull = 1 Then Err.Raise(vbObjectError, "", "<tlt>Dummy variable must contain more than one level</tlt>")
            'UPGRADE_WARNING: Lower bound of array lVarTemp.strNames was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim lVarTemp.strNames(1)
            lVarTemp.strNames(0) = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            rowCounter = rowCounter + 1 'reader.Read()
            'lconRS.MoveNext()
            lVarTemp.strNames(1) = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            lVarTemp.strBase = lVarTemp.strNames(0)
            lVarTemp.iSize = 1
            lVarTemp.iType = 1

            canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(0))
            canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(1))

            'Make the smaller value the base, just incase it is needed
            'lVarTemp.strBase = lconRS.Fields(0).Value
            'check if the variable is a boolean number variable
            If Val(lVarTemp.strNames(0)) = 0 And Val(lVarTemp.strNames(1)) = 1 Then
                'BOOLEAN YES NO
                lVarTemp.iType = 0
            End If
        Else
            i = i - 1
            'UPGRADE_WARNING: Lower bound of array lVarTemp.strNames was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim lVarTemp.strNames(i - 1)
            'If there is a Null In the Set, set the base = to the empty string
            If lintnull = 1 Then
                lVarTemp.strBase = ""
            Else
                lVarTemp.strBase = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
            End If
            rowCounter = rowCounter + 1 'reader.Read() 'lconRS.MoveNext()
            For j = 0 To i - 1
                lVarTemp.strNames(j) = rows(rowCounter)(lStrName) 'reader(0) 'lconRS.Fields(0).Value
                canBeDiscrete = canBeDiscrete * isNumber(lVarTemp.strNames(j))
                rowCounter = rowCounter + 1 'reader.Read() 'lconRS.MoveNext()
            Next
            lVarTemp.iType = 2

            If Val(lVarTemp.strNames(0)) = 0 And Val(lVarTemp.strNames(1)) = 1 Then
                lVarTemp.iType = 4
            End If

            lVarTemp.iSize = lVarTemp.iSize - 1
        End If

        'If it was not marked to be discrete, then attempt to make it continous
        If isDiscrete = 0 Then
            'If the canbediscrete flag is not set, then it can be a discrete var
            If canBeDiscrete = 1 Then
                lVarTemp.iSize = 1
                'UPGRADE_WARNING: Lower bound of array lVarTemp.strNames was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
                ReDim lVarTemp.strNames(0)

                'Make it a type 3 variable, continous, so that Null or Missing values reject
                'The data row
                lVarTemp.iType = 3
                lVarTemp.strBase = "NONE"
            End If
        End If

cleanup:
        'lconRS.Close()
        'UPGRADE_NOTE: Object lconRS may not be destroyed until it is garbage collected. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6E35BFF6-CD74-4B09-9689-3E1A43DF8969"'
        'lconRS = Nothing
        'reader.Close()
        'reader.Dispose()

        'UPGRADE_WARNING: Couldn't resolve default property of object Dummyfy. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
        DummyfyLogistic = lVarTemp
        Exit Function
        'DummyError:
        '    Debug.Print "ERROR IN LABELING VARIABLE " & lStrName
        '    Debug.Print "Variable has " & lintnull & " Null property " & lVarTemp.iType
        '    Debug.Print " i = " & i
        '    Resume cleanup

    End Function

    Public Function GetStrNames() As String()

        'Parse the term array, searching for *
        'count for the max number of *'s
        'this is for the term matrix
        Dim lszaTemp() As String
        Dim lStrAReturn() As String
        Dim i As Integer
        Dim j As Integer
        Dim strpos As Integer
        Dim nPos As Integer
        Dim newStrPos As Integer
        Dim mIntNNames As Integer
        Dim bOkayToAdd As Boolean
        mIntNNames = UBound(mstraTerms)

        For i = 0 To UBound(mstraTerms)

            strpos = InStr(1, mstraTerms(i), "*")
            While Not strpos = 0
                mIntNNames = mIntNNames + 1
                strpos = InStr(strpos + 1, mstraTerms(i), "*")
            End While
        Next


        'UPGRADE_WARNING: Lower bound of array lszaTemp was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim lszaTemp(mIntNNames)
        nPos = 0

        'Fill the lszaStrings
        For i = 0 To UBound(mstraTerms)

            strpos = InStr(1, mstraTerms(i), "*")

            If strpos = 0 Then
                lszaTemp(nPos) = mstraTerms(i)
                nPos = nPos + 1
            Else
                lszaTemp(nPos) = Left(mstraTerms(i), strpos - 1)
                nPos = nPos + 1

                newStrPos = 1
                While Not newStrPos = 0
                    newStrPos = InStr(strpos + 1, mstraTerms(i), "*")
                    If newStrPos = 0 Then
                        lszaTemp(nPos) = Right(mstraTerms(i), Len(mstraTerms(i)) - strpos)
                        nPos = nPos + 1
                    Else
                        lszaTemp(nPos) = Mid(mstraTerms(i), strpos + 1, newStrPos - strpos - 1)
                        strpos = newStrPos
                        nPos = nPos + 1
                    End If
                End While
            End If
        Next


        'Now must check for multiple terms.... in the term array
        'UPGRADE_WARNING: Lower bound of array lStrAReturn was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim lStrAReturn(mIntNNames)
        nPos = 0
        For i = 0 To UBound(lszaTemp)
            bOkayToAdd = True
            For j = 0 To nPos - 1
                If StrComp(lStrAReturn(j), lszaTemp(i)) = 0 Then
                    mIntNNames = mIntNNames - 1
                    bOkayToAdd = False
                End If

            Next
            If bOkayToAdd = True Then
                lStrAReturn(nPos) = lszaTemp(i)
                nPos = nPos + 1
            End If
        Next

        'UPGRADE_WARNING: Lower bound of array lStrAReturn was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim Preserve lStrAReturn(mIntNNames)
        GetStrNames = VB6.CopyArray(lStrAReturn)
    End Function

    Public Sub setInteractionTerms()
        Dim lszaTemp() As String
        Dim lStrAReturn() As String
        Dim i As Integer
        Dim j As Integer
        Dim strpos As Integer
        Dim nPos As Integer
        Dim newStrPos As Integer
        Dim mIntNNames As Integer
        Dim bOkayToAdd As Boolean
        mIntNNames = UBound(mstraTerms)
        Dim lIntNTerms As Integer

        'Set the number of terms for the interaction array
        'UPGRADE_WARNING: Lower bound of array mIVarArray was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
        ReDim mIVarArray(UBound(mstraTerms))
        'pre parse, setting the arrays in the interactionvariable array
        For i = 0 To UBound(mstraTerms)

            strpos = InStr(1, mstraTerms(i), "*")
            lIntNTerms = 0
            While Not strpos = 0
                lIntNTerms = lIntNTerms + 1
                strpos = InStr(strpos + 1, mstraTerms(i), "*")
            End While
            mIVarArray(i).iTerms = i + 1
            'UPGRADE_WARNING: Lower bound of array mIVarArray(i).Variables was changed from 1 to 0. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="0F1C9BE1-AF9D-476E-83B1-17D43BECFF20"'
            ReDim mIVarArray(i).Variables(lIntNTerms)
        Next


        For i = 0 To UBound(mstraTerms)

            strpos = InStr(1, mstraTerms(i), "*")

            If strpos = 0 Then
                mIVarArray(i).Variables(0) = getVariableIndex(mstraTerms(i))

            Else
                mIVarArray(i).Variables(0) = getVariableIndex(Left(mstraTerms(i), strpos - 1))
                nPos = 2
                newStrPos = 1
                While Not newStrPos = 0
                    newStrPos = InStr(strpos + 1, mstraTerms(i), "*")
                    If newStrPos = 0 Then
                        mIVarArray(i).Variables(nPos - 1) = getVariableIndex(Right(mstraTerms(i), Len(mstraTerms(i)) - strpos))
                        nPos = nPos + 1
                    Else
                        mIVarArray(i).Variables(nPos - 1) = getVariableIndex(Mid(mstraTerms(i), strpos + 1, newStrPos - strpos - 1))
                        strpos = newStrPos
                        nPos = nPos + 1
                    End If
                End While
            End If
        Next

    End Sub

    Public Function getVariableIndex(ByRef lStrName As String) As Integer
        Dim i As Integer

        For i = 0 To UBound(mVarArray)
            If StrComp(mVarArray(i).strName, lStrName) = 0 Then
                getVariableIndex = i + 1
                Exit Function
            End If
        Next
        getVariableIndex = -1
    End Function
    'Recursive function to make the labels for the matrix
    'Requires global variables, mVarArray and MIvararray to be already processed by the dummyfy function
    Public Function makeMatrixLabels(ByRef lstrbase As String, ByRef lintvar As Integer, ByRef lilevel As Integer, ByRef lintpos As Integer) As Object
        Dim dbltemp As Double
        Dim i As Integer
        Dim j As Integer
        Dim linttemppos As Integer
        Dim lstrpasson As String
        Dim lstrOther As String
        Dim lstrBoolean As String
        On Error GoTo MatrixLabelsError
        'If it is the first interaction level, do not put an ampersand on the label
        If lilevel = 0 Then
            lstrOther = ""
        Else
            lstrOther = " * "
        End If

        'if at the last interaction term,  then create the number of labels for each dummy variable
        'within the last term
        If lilevel = UBound(mIVarArray(lintvar).Variables) Then
            'Get the variable index of the dummy
            i = mIVarArray(lintvar).Variables(lilevel) - 1
            'Loop through each dummy within the variable
            For j = 0 To mVarArray(i).iSize - 1
                'Get it's position in the output matrix by it's column representation
                linttemppos = mVarArray(i).iColumn + j - 1
                'Tack on the base label
                mStrAMatrixLabels(lintpos) = lstrbase & lstrOther
                'If the variable has multiple dummy's output the dummy names
                'Otherwise output the variables base name
                If mVarArray(i).iType = 2 Then
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(j) & "/" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strBase & ") "
                    'YES NO MISSING
                ElseIf mVarArray(i).iType = 4 Then
                    If CDbl(mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(j)) = 1 Then
                        lstrBoolean = "Yes"
                    Else
                        lstrBoolean = "No"
                    End If
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & lstrBoolean & "/" & mstraBoolean(2) & ") "
                ElseIf mVarArray(i).iType = 1 Then
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(1) & "/" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(0) & ") "
                ElseIf mVarArray(i).iType = 0 Then
                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & "Yes" & "/" & "No" & ") "
                Else

                    mStrAMatrixLabels(lintpos) = mStrAMatrixLabels(lintpos) & mVarArray(i).strName
                End If
                'increase the position of the matrix labels "pointer"
                lintpos = lintpos + 1
            Next
            Exit Function
        End If

        'We are not at the top level so loop through each dummy within the variable
        'and call this function again.
        For j = 0 To mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).iSize - 1
            'Get the value of the interaction, and pass it onwards
            'depending upon the type
            If mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).iType = 2 Then
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(j) & "/" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strBase & ") "
            ElseIf mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).iType = 4 Then
                If CDbl(mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(j)) = 1 Then
                    lstrBoolean = "Yes"
                Else
                    lstrBoolean = "No"
                End If
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & lstrBoolean & "/" & mstraBoolean(2) & ") "
            ElseIf mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).iType = 1 Then
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(1) & "/" & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strNames(0) & ") "
            ElseIf mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).iType = 0 Then
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName & " (" & "Yes" & "/" & "No" & ") "
            Else
                lstrpasson = lstrbase & lstrOther & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName
            End If
            makeMatrixLabels(lstrpasson, lintvar, lilevel + 1, lintpos)
        Next

        Exit Function
MatrixLabelsError:
        Debug.Print("Trying to Label Variable " & mVarArray(mIVarArray(lintvar).Variables(lilevel) - 1).strName)

    End Function
    'isNumber returns a 1 if lstr is a number, otherwise it returns 0
    Public Function isNumber(ByRef lstr As String) As Integer
        If StrComp(Trim(CStr(Val(lstr))), Trim(lstr)) = 0 Then
            isNumber = 1
        Else
            isNumber = 0
        End If
    End Function
End Module