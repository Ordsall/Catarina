 
<Serializable()>
Public Class DeviceSettings
    Public Sub ExportToXml(ByVal Path As String)
        Dim fn As String = Path
        If IO.File.Exists(Path) Then
            For i = 1 To Integer.MaxValue
                fn = Path & "_" & i.ToString
                If Not IO.File.Exists(fn) Then Exit For
            Next
        End If
        ' Insert code to set properties and fields of the object.
        Dim mySerializer As Xml.Serialization.XmlSerializer = New Xml.Serialization.XmlSerializer(GetType(DeviceSettings))
        ' To write to a file, create a StreamWriter object. 
        Dim myWriter As IO.StreamWriter = New IO.StreamWriter(fn)
        mySerializer.Serialize(myWriter, Me)
        myWriter.Close()
    End Sub


    Public Sub ExportToTxt(ByVal Path As String)
        Dim fn As String = Path
        If IO.File.Exists(Path) Then
            For i = 1 To Integer.MaxValue
                fn = Path & "_" & i.ToString
                If Not IO.File.Exists(fn) Then Exit For
            Next
        End If
        IO.File.AppendAllText(fn, Me.p0_SerialNumber)
    End Sub


    Public Const SetCount As Integer = 26
    Public trueKey() As Byte = {1, 8, 0, 2, 1, 9, 8, 9}
    ' настройки хранятся в четвертом секторе флэхи. для их замены сущ сл алгоритм:
    ' 1) копируем настройки из 4го в 3й
    ' 2) выставляем в 3м флаг (последние 8 байт сектора = 1,8,0,2,1,9,8,9) - тем самым указываем что был начат процесс изменения настроек
    ' 3) трем 4й сектор
    ' 4) пишем НОВЫЕ настройки в 4й сектор
    ' 5) проверяем что записались. если нет, то к пункту 3
    ' 6) трем 3й сектор




    ''' <summary>
    ''' #0 Серийный номер устройства
    ''' </summary>
    ''' <remarks></remarks>
    Public p0_SerialNumber As String
    ''' <summary>
    ''' #1 количество параметров (каждый параметр 8ми байтный)
    ''' </summary>
    ''' <remarks></remarks>
    Public p1_SetCount As Long
    ''' <summary>
    ''' #2 Резерв
    ''' </summary>
    ''' <remarks></remarks>
    Public p2_AN_RefSinAmpIn1 As Double
    ''' <summary>
    ''' #3 Резерв
    ''' </summary>
    ''' <remarks></remarks>
    Public p3_AN_RefSinAmpOut1 As Double
    ''' <summary>
    ''' #4 Резерв
    ''' </summary>
    ''' <remarks></remarks>
    Public p4_AN_CH1_Out_SCAmp As Double
    ''' <summary>
    ''' #5 Резерв
    ''' </summary>
    ''' <remarks></remarks>
    Public p5_AN_CH1_Out_SCPhase As Double
    ''' <summary>
    ''' #6 Резерв
    ''' </summary>
    ''' <remarks></remarks>
    Public p6_AN_CH2_Out_SCAmp As Double
    ''' <summary>
    ''' #7 Резерв
    ''' </summary>
    ''' <remarks></remarks>
    Public p7_AN_CH2_Out_SCPhase As Double
    ''' <summary>
    ''' #8 
    ''' </summary>
    ''' <remarks></remarks>
    Public p8_AN_CH1_In_SCAmp As Double
    ''' <summary>
    ''' #9
    ''' </summary>
    ''' <remarks></remarks>
    Public p9_AN_CH1_In_SCPhase As Double
    ''' <summary>
    ''' #10
    ''' </summary>
    ''' <remarks></remarks>
    Public p10_AN_CH2_In_SCAmp As Double
    ''' <summary>
    ''' #11 
    ''' </summary>
    ''' <remarks></remarks>
    Public p11_AN_CH2_In_SCPhase As Double
    ''' <summary>
    ''' #12
    ''' </summary>
    ''' <remarks></remarks>
    Public p12_AN_LastCalibrationDate As Date
    ''' <summary>
    ''' #13
    ''' </summary>
    ''' <remarks></remarks>
    Public p13_MW_A1 As Double
    ''' <summary>
    ''' #14
    ''' </summary>
    ''' <remarks></remarks>
    Public p14_MW_A2 As Double
    ''' <summary>
    ''' #15
    ''' </summary>
    ''' <remarks></remarks>
    Public p15_MW_A3 As Double
    ''' <summary>
    ''' #16
    ''' </summary>
    ''' <remarks></remarks>
    Public p16_UserCalibration_Date As Date

    Public p17_AN_RefSinAmpIn2 As Double
    Public p18_AN_RefSinAmpOut2 As Double
    Public p19_RefFreq As Double


    Private Overloads Sub _toArrayPart1_ARappend(ByRef AR() As Byte, ByVal Param As Long)
        Dim tr() As Byte = BitConverter.GetBytes(Param)
        If tr.Length <> 8 Then Throw New Exception("упс")
        If BitConverter.IsLittleEndian = False Then tr = ReverseBytes(tr)
        If AR Is Nothing Then
            ReDim AR(7)
        Else
            ReDim Preserve AR(UBound(AR) + 8)
        End If
        For i = 0 To 7
            AR(UBound(AR) - 7 + i) = tr(i)
        Next
    End Sub

    Private Overloads Sub _toArrayPart1_ARappend(ByRef AR() As Byte, ByVal Param As Double)
        Dim tr() As Byte = BitConverter.GetBytes(Param)
        If tr.Length <> 8 Then Throw New Exception("упс")
        If BitConverter.IsLittleEndian = False Then tr = ReverseBytes(tr)
        If AR Is Nothing Then
            ReDim AR(7)
        Else
            ReDim Preserve AR(UBound(AR) + 8)
        End If
        For i = 0 To 7
            AR(UBound(AR) - 7 + i) = tr(i)
        Next
    End Sub

    Private Overloads Sub _toArrayPart1_ARappend(ByRef AR() As Byte, ByVal Param As String)
        If Param Is Nothing Then
            Param = ""
        End If
        If Param.Length < 8 Then
            Do Until Param.Length = 8
                Param = " " & Param
            Loop
        End If
        Dim tr(7) As Byte
        For i = 0 To 7
            tr(i) = Asc(Param(i))
        Next

        If BitConverter.IsLittleEndian = False Then tr = ReverseBytes(tr)
        If AR Is Nothing Then
            ReDim AR(7)
        Else
            ReDim Preserve AR(UBound(AR) + 8)
        End If
        For i = 0 To 7
            AR(UBound(AR) - 7 + i) = tr(i)
        Next
    End Sub


    Private Overloads Sub _toArrayPart1_ARappend(ByRef AR() As Byte, ByVal Param As Date)
        Dim tm As Double = Param.ToOADate
        _toArrayPart1_ARappend(AR, tm)
    End Sub




    Public Function ToArray() As Byte()
        Dim AR() As Byte = Nothing
        _toArrayPart1_ARappend(AR, Me.p0_SerialNumber)
        _toArrayPart1_ARappend(AR, Me.p1_SetCount)
        _toArrayPart1_ARappend(AR, Me.p2_AN_RefSinAmpIn1)
        _toArrayPart1_ARappend(AR, Me.p3_AN_RefSinAmpOut1)
        _toArrayPart1_ARappend(AR, Me.p4_AN_CH1_Out_SCAmp)
        _toArrayPart1_ARappend(AR, Me.p5_AN_CH1_Out_SCPhase)
        _toArrayPart1_ARappend(AR, Me.p6_AN_CH2_Out_SCAmp)
        _toArrayPart1_ARappend(AR, Me.p7_AN_CH2_Out_SCPhase)
        _toArrayPart1_ARappend(AR, Me.p8_AN_CH1_In_SCAmp)
        _toArrayPart1_ARappend(AR, Me.p9_AN_CH1_In_SCPhase)
        _toArrayPart1_ARappend(AR, Me.p10_AN_CH2_In_SCAmp)
        _toArrayPart1_ARappend(AR, Me.p11_AN_CH2_In_SCPhase)
        _toArrayPart1_ARappend(AR, Me.p12_AN_LastCalibrationDate)
        _toArrayPart1_ARappend(AR, Me.p13_MW_A1)
        _toArrayPart1_ARappend(AR, Me.p14_MW_A2)
        _toArrayPart1_ARappend(AR, Me.p15_MW_A3)
        _toArrayPart1_ARappend(AR, Me.p16_UserCalibration_Date)
        _toArrayPart1_ARappend(AR, Me.p17_AN_RefSinAmpIn2)
        _toArrayPart1_ARappend(AR, Me.p18_AN_RefSinAmpOut2)
        _toArrayPart1_ARappend(AR, Me.p19_RefFreq)
        Return AR
    End Function

    Private Overloads Sub _FromArrayPart2_AR(ByVal AR() As Byte, ByRef Param As Long, ByVal Index As Integer)
        If AR Is Nothing Then
            Exit Sub
        End If

        Dim tar(7) As Byte
        For i = 0 To 7
            tar(i) = AR(Index * 8 + i)
        Next
        If BitConverter.IsLittleEndian = False Then tar = ReverseBytes(tar)
        Param = BitConverter.ToInt64(tar, 0)
    End Sub

    Private Overloads Sub _FromArrayPart2_AR(ByVal AR() As Byte, ByRef Param As Double, ByVal Index As Integer)
        Dim tar(7) As Byte
        For i = 0 To 7
            tar(i) = AR(Index * 8 + i)
        Next
        If BitConverter.IsLittleEndian = False Then tar = ReverseBytes(tar)
        Param = BitConverter.ToDouble(tar, 0)
    End Sub

    Private Overloads Sub _FromArrayPart2_AR(ByVal AR() As Byte, ByRef Param As String, ByVal Index As Integer)
        Dim s As String = String.Empty
        For i = 0 To 7
            s &= Chr(AR(Index * 8 + i))
        Next

        If BitConverter.IsLittleEndian = False Then s = Strings.StrReverse(s)
        Param = s
    End Sub

    Private Overloads Sub _FromArrayPart2_AR(ByVal AR() As Byte, ByRef Param As Date, ByVal Index As Integer)
        Dim tar(7) As Byte
        For i = 0 To 7
            tar(i) = AR(Index * 8 + i)
        Next
        If BitConverter.IsLittleEndian = False Then tar = ReverseBytes(tar)
        Try
            Param = Date.FromOADate(BitConverter.ToDouble(tar, 0))
        Catch ex As Exception
            Param = Nothing
        End Try

    End Sub










    Public Shared Function FromArray(ByVal AR As Byte()) As DeviceSettings
        Dim ret As New DeviceSettings
        Try
            ret._FromArrayPart2_AR(AR, ret.p0_SerialNumber, 0)
            ret._FromArrayPart2_AR(AR, ret.p1_SetCount, 1)
            ret._FromArrayPart2_AR(AR, ret.p2_AN_RefSinAmpIn1, 2)
            ret._FromArrayPart2_AR(AR, ret.p3_AN_RefSinAmpOut1, 3)
            ret._FromArrayPart2_AR(AR, ret.p4_AN_CH1_Out_SCAmp, 4)
            ret._FromArrayPart2_AR(AR, ret.p5_AN_CH1_Out_SCPhase, 5)
            ret._FromArrayPart2_AR(AR, ret.p6_AN_CH2_Out_SCAmp, 6)
            ret._FromArrayPart2_AR(AR, ret.p7_AN_CH2_Out_SCPhase, 7)
            ret._FromArrayPart2_AR(AR, ret.p8_AN_CH1_In_SCAmp, 8)
            ret._FromArrayPart2_AR(AR, ret.p9_AN_CH1_In_SCPhase, 9)
            ret._FromArrayPart2_AR(AR, ret.p10_AN_CH2_In_SCAmp, 10)
            ret._FromArrayPart2_AR(AR, ret.p11_AN_CH2_In_SCPhase, 11)
            ret._FromArrayPart2_AR(AR, ret.p12_AN_LastCalibrationDate, 12)
            ret._FromArrayPart2_AR(AR, ret.p13_MW_A1, 13)
            ret._FromArrayPart2_AR(AR, ret.p14_MW_A2, 14)
            ret._FromArrayPart2_AR(AR, ret.p15_MW_A3, 15)
            ret._FromArrayPart2_AR(AR, ret.p16_UserCalibration_Date, 16)
            ret._FromArrayPart2_AR(AR, ret.p17_AN_RefSinAmpIn2, 17)
            ret._FromArrayPart2_AR(AR, ret.p18_AN_RefSinAmpOut2, 18)
            ret._FromArrayPart2_AR(AR, ret.p19_RefFreq, 19)
            Return ret
        Catch ex As Exception
            Return Nothing
        End Try
    End Function




    Private Shared Function ReverseBytes(ByVal inArray() As Byte) As Byte()
        Dim temp As Byte
        Dim highCtr As Integer = inArray.Length - 1

        For ctr As Integer = 0 To highCtr \ 2
            temp = inArray(ctr)
            inArray(ctr) = inArray(highCtr)
            inArray(highCtr) = temp
            highCtr -= 1
        Next
        Return inArray
    End Function







End Class

 










