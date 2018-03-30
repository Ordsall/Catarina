Friend Class Comunicator
    Public WithEvents SP As New IO.Ports.SerialPort
    Dim IsPortBusy As Boolean = False
    Dim IsFlowEnable As Boolean = False
    Public Event DetPackRecieved(ByVal ByteArr As Byte())
    Public Event SonoPackRecieved(ByVal ByteArr As Byte())
    Public Event PackWrited(ByVal Pack As pack)
    Public Event PackReaded(ByVal Pack As pack)
    Dim _SP_rbts As New Queue(Of Byte)






    Public ReadOnly Property IsOpen As Boolean
        Get
            If SP Is Nothing Then Return False
            Return SP.IsOpen
        End Get
    End Property

    Public Function Open(ByVal PortName As String) As Boolean
        If SP Is Nothing Then SP = New IO.Ports.SerialPort
        If SP.IsOpen Then SP.Close()
        p_IsHighBaudRate = False
        On Error Resume Next 
        '  SP.ReadTimeout
        SP.PortName = PortName
        SP.BaudRate = 115200
        SP.Open()
        SP.DiscardInBuffer()
        SP.DiscardOutBuffer()
        Return SP.IsOpen
    End Function

    Public Sub ChangeBaudRate(ByVal IsHigh As Boolean)
        If p_IsHighBaudRate = IsHigh Then Exit Sub
        If SP.IsOpen Then SP.Close()
        If IsHigh Then
            SP.BaudRate = 921600
        Else
            SP.BaudRate = 115200
        End If
        SP.Open()
    End Sub

    Private p_IsHighBaudRate As Boolean
    Public ReadOnly Property IsHighBaudRate() As Boolean
        Get
            Return p_IsHighBaudRate
        End Get
    End Property


    Public Sub Close()
        On Error Resume Next
        If SP IsNot Nothing Then

            SP.Close()
        End If
        SP = Nothing
    End Sub

    Public Function CMD_PING() As Boolean
        Dim _CMD As Byte = &H1
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return False
        Return _rPack.CMD = _CMD
    End Function

    Public Function CMD_REBOOT() As Boolean
        Dim _CMD As Byte = &H2
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return False
        Return _rPack.CMD = _CMD
    End Function


    Public Function CMD_GET_SETTINGS() As Byte()
        Dim _CMD As Byte = &H3
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return Nothing
        If _rPack.CMD <> _CMD Then Return Nothing
        Return _rPack.DATA
    End Function

    Public Function CMD_WRITE_SETTINGS(ByVal ByteArray As Byte()) As Boolean
        Dim _CMD As Byte = &H4
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, ByteArray))
        If _rPack Is Nothing Then Return Nothing
        Return _rPack.CMD = _CMD
    End Function

    Public Function CMD_FLOW_ENABLE() As Boolean
        Dim _CMD As Byte = &H7
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return False
        Return _rPack.CMD = _CMD
    End Function

    Public Function CMD_PROGRAM_ROM() As Boolean
        Dim _CMD As Byte = &H9
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return False
        Return _rPack.CMD = _CMD
    End Function

    Public Sub CMD_BAUD_RATE_LOW()
        Dim _CMD As Byte = &HB
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing)) 
    End Sub

    Public Sub CMD_BAUD_RATE_HIGH()
        Dim _CMD As Byte = &HC
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing)) 
    End Sub

    Public Function CMD_SONO_START() As Boolean
        Dim _CMD As Byte = &HD
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return False
        Return _rPack.CMD = _CMD
    End Function

    Public Function CMD_SONO_STOP() As Boolean
        Dim _CMD As Byte = &HE
        Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, Nothing))
        If _rPack Is Nothing Then Return False
        Return _rPack.CMD = _CMD
    End Function


    'Public Function CMD_GET_SPECTRUM(ByVal ChannelPair As Integer) As Byte()
    '    Dim _CMD As Byte = &HA
    '    Dim _DATA As Byte() = {&H3, &H0}
    '    If ChannelPair = 1 Then
    '        _DATA = {&H6, &H0}
    '    End If
    '    Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, _DATA))
    '    If _rPack Is Nothing Then Return Nothing
    '    Return _rPack.DATA
    'End Function


    'Public Function CMD_MEAS_SPECTR() As Boolean
    '    Dim _CMD As Byte = &HA
    '    Dim _rPack As pack = Write(New pack(pack.eDir.FromHost, _CMD, {&H1, &H0}))
    '    If _rPack Is Nothing Then Return False
    '    Return _rPack.CMD = _CMD
    'End Function


    Public Function GetSpecrtum() As Double(,)
        IsPortBusy = True
        System.Threading.Thread.Sleep(100)
        Do Until SP.BytesToRead = 0
            SP.DiscardInBuffer()
        Loop
        System.Threading.Thread.Sleep(100)

        SP.Write({&H21, &H0, &HA, &H1, &H1, &H0, &H2D}, 0, 7)
        Dim tm As Date = Now
        Do Until SP.BytesToRead >= 5 Or (Now - tm) > TimeSpan.FromMilliseconds(200)

        Loop
        If SP.BytesToRead < 5 Then Return Nothing
        Dim b(4) As Byte
        SP.Read(b, 0, 5)
        For i = 0 To 4
            If b(i) <> {&H22, &H0, &HA, &H0, &H2C}(i) Then
                Return Nothing
            End If
        Next
        System.Threading.Thread.Sleep(100)
        SP.Write({&H21, &H0, &HA, &H1, &H3, &H0, &H2F}, 0, 7)
        tm = Now
        Do Until SP.BytesToRead >= 2053 Or (Now - tm) > TimeSpan.FromMilliseconds(200)
            RaiseEvent PackReaded(Nothing)
        Loop
        Dim Cntr1 As Integer = SP.BytesToRead
        If Cntr1 < 2053 Then
            Dim bf1 As Boolean = True
            Do While bf1
                System.Threading.Thread.Sleep(100)
                bf1 = (SP.BytesToRead > Cntr1) And Not (SP.BytesToRead >= 2053)
                Cntr1 = SP.BytesToRead
            Loop
        End If
        If SP.BytesToRead < 2053 Then
            Return Nothing
        End If

        Dim b1(2052) As Byte
        SP.Read(b1, 0, 2053)
        System.Threading.Thread.Sleep(100)
        SP.Write({&H21, &H0, &HA, &H1, &H6, &H0, &H32}, 0, 7)
        tm = Now
        Do Until SP.BytesToRead >= 2053 Or (Now - tm) > TimeSpan.FromMilliseconds(200)
            RaiseEvent PackReaded(Nothing)
        Loop
        Dim Cntr2 As Integer = SP.BytesToRead
        If Cntr2 < 2053 Then
            Dim bf1 As Boolean = True
            Do While bf1
                System.Threading.Thread.Sleep(100)
                bf1 = (SP.BytesToRead > Cntr2) And Not (SP.BytesToRead >= 2053)
                Cntr2 = SP.BytesToRead
                RaiseEvent PackReaded(Nothing)
            Loop
        End If
        If SP.BytesToRead < 2053 Then
            Return Nothing
        End If
        Dim b2(2052) As Byte
        SP.Read(b2, 0, 2053)
        Dim ret(3, 511) As Double

        For i = 0 To 511
            'ret(0, i) = GetSpectrAmp({b1(5 + i * 2), b1(6 + i * 2)})
            'ret(1, i) = GetSpectrAmp({b1(1029 + i * 2), b1(1030 + i * 2)})
            'ret(2, i) = GetSpectrAmp({b2(5 + i * 2), b2(6 + i * 2)})
            'ret(3, i) = GetSpectrAmp({b2(1029 + i * 2), b2(1030 + i * 2)})

            ret(0, i) = GetSpectrAmp({b1(5 + i * 2), b1(4 + i * 2)})
            ret(1, i) = GetSpectrAmp({b1(1029 + i * 2), b1(1028 + i * 2)})
            ret(2, i) = GetSpectrAmp({b2(5 + i * 2), b2(4 + i * 2)})
            ret(3, i) = GetSpectrAmp({b2(1029 + i * 2), b2(1028 + i * 2)})
        Next
        IsPortBusy = False
        Return ret
    End Function

    Private Function GetSpectrAmp(ByVal Bts As Byte()) As Double
        Dim d As Double = (Bts(0) * 256 + Bts(1))
        d /= 32768
        If d <> 0 Then
            d = 20 * Math.Log10(d)
        Else
            d = -100
        End If
        Return d
    End Function



    Dim ansPack(255) As pack
    Private Function Write(ByVal Pack As pack) As pack 
        IsPortBusy = False
        On Error Resume Next
        Dim bts As Byte() = Pack.ToByte
        ansPack(Pack.CMD) = Nothing
        SP.DiscardInBuffer()
        SP.Write(bts, 0, bts.Length)
        'RaiseEvent PackWrited(Pack)
         
        Dim t1 As Date = Now
        Dim ansDelay As TimeSpan = TimeSpan.FromMilliseconds(150)
        Select Case Pack.CMD
            Case &H3
                ansDelay = TimeSpan.FromMilliseconds(300)
            Case 9
                ansDelay = TimeSpan.FromMilliseconds(2000)
            Case &HA
                ansDelay = TimeSpan.FromMilliseconds(2000)
        End Select
         
        Do Until (ansPack(Pack.CMD) IsNot Nothing Or (Now - t1) > ansDelay)
            System.Threading.Thread.Sleep(5)
        Loop
        Dim tms As TimeSpan = (Now - t1)
         
        Return ansPack(Pack.CMD)
    End Function

    Public datt(93) As Byte
    Dim SP_NowParsing As Boolean = False
    Dim SP_DataReceivedFlag As Boolean = False
    Private Sub SP_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SP.DataReceived
        If IsPortBusy Then
            Exit Sub
        End If
        If SP_DataReceivedFlag Then
            Exit Sub
        End If
        SP_DataReceivedFlag = True
        On Error Resume Next


        '  System.Threading.Thread.CurrentThread.Priority = Threading.ThreadPriority.AboveNormal
 
        Dim StartTime As Date = Now 
        Dim _step As Integer = 0 
        Dim _rd As New List(Of Byte) 
        Do Until _step = 6 Or (Now - StartTime) > TimeSpan.FromMilliseconds(200)
            If IsPortBusy Then
                SP_DataReceivedFlag = False
                Exit Sub
            End If
            If SP.BytesToRead > 500 Then
                Beep()
            End If



            If SP.BytesToRead > 0 Then
                Dim b(SP.BytesToRead - 1) As Byte
                SP.Read(b, 0, b.Length)
                _rd.AddRange(b.Clone)
            End If

            If _rd.Count > 0 Then
                Dim p As Integer = -1
                Do While p <> _step
                    If IsPortBusy Then
                        SP_DataReceivedFlag = False
                        Exit Sub
                    End If
                    p = _step
                    Select Case _step
                        Case 0 ' PREFIX
                            For i = 0 To _rd.Count - 1
                                If _rd(i) = &H22 Then
                                    If i > 0 Then
                                        _rd.RemoveRange(0, i)
                                    End If
                                    _step = 1
                                    Exit For
                                End If
                            Next
                        Case 1 ' ADDR
                            If _rd.Count > 1 Then
                                If _rd(1) = 0 Then
                                    _step = 2
                                Else
                                    _rd.RemoveRange(0, 2)
                                    If _rd.Count > 2 Then
                                        _step = 0
                                    Else
                                        _step = -1
                                    End If
                                End If
                            End If
                        Case 2 ' CMD
                            If _rd.Count > 2 Then
                                _step = 3
                                Select Case _rd(2)
                                    Case &H1

                                    Case &H2

                                    Case &H3

                                    Case &H4

                                    Case &H5

                                    Case &H6

                                    Case &H7

                                    Case &H8
                                    Case &H9

                                    Case &H78

                                    Case Else
                                        _rd.RemoveRange(0, 3)
                                        If _rd.Count > 3 Then
                                            _step = 0
                                        Else
                                            _step = -1
                                        End If
                                End Select
                            End If
                        Case 3 ' LEN
                            If _rd.Count > 3 Then
                                _step = 4
                                Select Case _rd(2)
                                    Case &H1

                                    Case &H2

                                    Case &H3

                                    Case &H4

                                    Case &H5

                                    Case &H6

                                    Case &H7

                                    Case &H8
                                    Case &H9
                                    Case &H78

                                    Case Else
                                        _rd.RemoveRange(0, 4)
                                        If _rd.Count > 3 Then
                                            _step = 0
                                        Else
                                            _step = -1
                                        End If
                                End Select
                            End If
                        Case 4 ' DATA
                            If _rd.Count >= 4 + _rd(3) * 2 + 1 Then
                                _step = 5
                            End If
                        Case 5 ' CRC
                            Dim CRC As Integer = 0
                            For i = 0 To 3 + _rd(3) * 2
                                CRC += _rd(i)
                                If CRC > 255 Then CRC -= 256
                            Next
                            If CRC = _rd(4 + _rd(3) * 2) Then


                                Dim data() As Byte = Nothing
                                If _rd(3) > 0 Then
                                    ReDim data(_rd(3) * 2 - 1)
                                    _rd.CopyTo(4, data, 0, data.Length)
                                End If
                                Dim pck As New pack(pack.eDir.FromModule, _rd(2), data)
                                ansPack(_rd(2)) = pck

                                RaiseEvent PackReaded(pck)
                                'Console.WriteLine(pck.CMD)
                                Select Case _rd(2)
                                    Case &H78
                                        RaiseEvent DetPackRecieved(data)
                                    Case &HD
                                        RaiseEvent SonoPackRecieved(data)
                                End Select

                                _rd.RemoveRange(0, 4 + _rd(3) * 2 + 1)
                                Dim rt As TimeSpan = (Now - StartTime)
                                If _rd.Count > 0 Or SP.BytesToRead > 0 Then
                                    _step = 0
                                    StartTime = Now
                                End If
                                _step = 6
                            Else


                            End If
                    End Select
                Loop
                 
            End If
        Loop
         SP_DataReceivedFlag = False 
    End Sub


   
End Class