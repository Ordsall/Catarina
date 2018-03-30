Public Class Device

    Dim WithEvents BW As ComponentModel.BackgroundWorker
    Dim WithEvents COM As Comunicator
    Dim WithEvents tmr_ConnectionWatchDog As New Timers.Timer With {.Interval = 200, .Enabled = True}
    Public Property Settings As ServiceSettings
    Dim p_Detectors(7) As Detector
    Public Event DetectorsChanged(ByVal Detectors As Detector())
    Public Event ConnectionStateChanged(ByVal IsConnected As Boolean)

    Dim p_SonoAmp As Double()
    Public ReadOnly Property SonoAmp As Double()
        Get
            Return p_SonoAmp
        End Get
    End Property

    Dim p_SonoPh1 As Double()
    Public ReadOnly Property SonoPh1 As Double()
        Get
            Return p_SonoPh1
        End Get
    End Property

    Dim p_SonoPh2 As Double()
    Public ReadOnly Property SonoPh2 As Double()
        Get
            Return p_SonoPh2
        End Get
    End Property

    Public Event SonoChanged(ByVal Amp() As Double, ByVal Ph1() As Double, ByVal Ph2() As Double)


    Dim p_Signal() As Double = {-100, -100, -100, -100}
    Public ReadOnly Property Signal As Double()
        Get
            Return p_Signal
        End Get
    End Property

    Dim p_Noise() As Double = {-100, -100, -100, -100}
    Public ReadOnly Property Noise As Double()
        Get
            Return p_Noise
        End Get
    End Property

    Public ReadOnly Property Detectors As Detector()
        Get
            Return p_Detectors
        End Get
    End Property

    Dim p_IsConnected As Boolean = False
    Public ReadOnly Property IsConnected As Boolean
        Get
            Return p_IsConnected
        End Get
    End Property


 

    Private p_IsConnectionWatchDogEnable As Boolean = False
    Public Property IsConnectionWatchDogEnable() As Boolean
        Get
            Return p_IsConnectionWatchDogEnable
        End Get
        Set(ByVal value As Boolean)
            p_IsConnectionWatchDogEnable = value

        End Set
    End Property


    Private p_ConnectionWatchDogEnableInterval As TimeSpan = TimeSpan.FromSeconds(2)
    Public Property ConnectionWatchDogEnableInterval() As TimeSpan
        Get
            Return p_ConnectionWatchDogEnableInterval
        End Get
        Set(ByVal value As TimeSpan)
            p_ConnectionWatchDogEnableInterval = value
        End Set
    End Property
     

    Private p_DetectorsPackCounter As ULong = 0
    Public Property DetectorsPackCounter() As ULong
        Get
            Return p_DetectorsPackCounter
        End Get
        Set(ByVal value As ULong)
            p_DetectorsPackCounter = value
        End Set
    End Property




    Public ReadOnly Property PortName As String
        Get
            If p_IsConnected Then
                Return COM.SP.PortName
            Else
                Return ""
            End If
        End Get
    End Property

    Public Function CheckDevice(ByVal PortName As String) As Boolean
        Dim tc As New Comunicator
        Dim res As Boolean = False
        If tc.Open(PortName) Then
            res = tc.CMD_PING()
        End If
        tc.Close()
        Return res
    End Function

    Public Overloads Function Connect() As Boolean
        For Each spn As String In My.Computer.Ports.SerialPortNames
            If Me.Connect(spn) Then Return True
        Next
        Return False
    End Function

    Public Overloads Function Connect(ByVal PortName As String) As Boolean
        If COM Is Nothing Then COM = New Comunicator
        If COM.IsOpen Then COM.Close()
        If COM.Open(PortName) Then
            If COM.CMD_PING Then 
                If Me.ReadServiceSettings Then
                    p_IsConnected = True
                    RaiseEvent ConnectionStateChanged(p_IsConnected)
                    Return True
                End If
            End If
        End If
        Me.Disconnect()
        Return False
    End Function

    Public Sub Disconnect()
        If COM IsNot Nothing Then
            COM.Close()
        End If
        COM = Nothing
        If p_IsConnected Then
            RaiseEvent ConnectionStateChanged(p_IsConnected)
        End If
        p_IsConnected = False
    End Sub

    Public Function ReadServiceSettings() As Boolean

        Dim tb() As Byte = COM.CMD_GET_SETTINGS
        Settings = ServiceSettings.FromByte(tb)
        System.Threading.Thread.Sleep(200) 

        Return (Settings IsNot Nothing)
    End Function

    Public Function WriteServiceSettings() As Boolean
        If Me.Settings Is Nothing Then Return False
        Return COM.CMD_WRITE_SETTINGS(Settings.ToByte)
    End Function

    Public Function ProgramServiceSettings() As Boolean
        Return COM.CMD_PROGRAM_ROM
    End Function

    Public Function EnableFlow() As Boolean
        Return COM.CMD_FLOW_ENABLE
    End Function

    Public Function DisableFlow() As Boolean
        Return COM.CMD_PING
    End Function

    Public Function GetSpectrum() As Double(,)
        Dim d(,) As Double = COM.GetSpecrtum
        If d IsNot Nothing Then
            GetSignalNoise(d, p_Signal, p_Noise)
        End If 
        Return d
    End Function



    Private Sub COM_DetPackRecieved(ByVal ByteArr() As Byte) Handles Com.DetPackRecieved
        Me.p_Detectors = Detector.DetectorsFromByteArray(ByteArr)
        If p_DetectorsPackCounter = ULong.MaxValue Then
            p_DetectorsPackCounter = 0
        Else
            p_DetectorsPackCounter += 1
        End If
        RaiseEvent DetectorsChanged(p_Detectors)
    End Sub

    Public Sub SonoStart()
        If Not p_IsConnected Then Exit Sub
        COM.CMD_BAUD_RATE_HIGH()
        System.Threading.Thread.Sleep(200)

        COM.ChangeBaudRate(True)
        System.Threading.Thread.Sleep(200)
        COM.CMD_SONO_START()
        System.Threading.Thread.Sleep(200)
    End Sub

    Public Sub SonoStop()
        If Not p_IsConnected Then Exit Sub
        COM.CMD_SONO_STOP()
        System.Threading.Thread.Sleep(200)
        COM.CMD_BAUD_RATE_LOW()
        System.Threading.Thread.Sleep(200)
        COM.ChangeBaudRate(False)
    End Sub

    Private Function GetSignalNoise(ByVal Spectrums As Double(,), ByRef Signal As Double(), ByRef Noise As Double()) As Boolean
        Dim _signal(3) As Double
        Dim _noise(3) As Double
        Dim max() As Double = {Double.MinValue, Double.MinValue, Double.MinValue, Double.MinValue}
        Dim maxI(3) As Integer
        Const lb As Integer = 80
        Const ub As Integer = 500
        Dim nlb As Integer = 0
        Dim nub As Integer = 0
        Dim avernoise(3) As Double
        Dim avernoisecntr(3) As Integer
        Dim peaknoise(3) As Double
        For i = 0 To 3
            ' находим максимум сигнал
            For j = lb To ub
                If max(i) < Spectrums(i, j) Then
                    max(i) = Spectrums(i, j)
                    maxI(i) = j
                End If
            Next
            _signal(i) = max(i)
            ' находим область сигнала
            Dim lbl As Double = max(i)
            Dim lblI As Integer = 0
            Dim lst As Integer = 0
            Dim lbdlt As Double = 0
            For j = maxI(i) To lb Step -1
                Select Case lst
                    Case 0
                        If Spectrums(i, j) > lbl Then
                            lst = 1
                            lbdlt = Spectrums(i, j) - lbl
                        Else
                            lbl = Spectrums(i, j)
                            lblI = j
                        End If
                    Case 1
                        If Spectrums(i, j) > lbl Then
                            lbdlt = Spectrums(i, j) - lbl
                        Else
                            If lbdlt > 2 Then
                                nlb = lblI
                                Exit For
                            Else
                                lbl = Spectrums(i, j)
                                lblI = j
                                lst = 0
                            End If
                        End If
                End Select
            Next

            lbl = max(i)
            lst = 0
            lbdlt = 0
            For j = maxI(i) To ub
                Select Case lst
                    Case 0
                        If Spectrums(i, j) > lbl Then
                            lst = 1
                            lbdlt = Spectrums(i, j) - lbl
                        Else
                            lbl = Spectrums(i, j)
                            lblI = j
                        End If
                    Case 1
                        If Spectrums(i, j) > lbl Then
                            lbdlt = Spectrums(i, j) - lbl
                        Else
                            If lbdlt > 2 Then
                                nub = lblI
                                Exit For
                            Else
                                lbl = Spectrums(i, j)
                                lblI = j
                                lst = 0
                            End If
                        End If
                End Select
            Next
            ' средний уровень шумов
            avernoise(i) = 0
            avernoisecntr(i) = 0
            peaknoise(i) = Double.MinValue

            For j = lb To nlb
                If Spectrums(i, j) > -90 Then
                    avernoise(i) += Spectrums(i, j)
                    avernoisecntr(i) += 1
                    If peaknoise(i) < Spectrums(i, j) Then peaknoise(i) = Spectrums(i, j)
                End If
            Next
            'For j = nub To ub
            '    If Spectrums(i, j) > -90 Then
            '        avernoise(i) += Spectrums(i, j)
            '        avernoisecntr(i) += 1
            '        If peaknoise(i) < Spectrums(i, j) Then peaknoise(i) = Spectrums(i, j)
            '    End If
            'Next

            avernoise(i) /= avernoisecntr(i)
            Dim tAverNoise = avernoise(i)
            avernoise(i) = 0
            avernoisecntr(i) = 0


            For j = lb To nlb
                If Spectrums(i, j) > tAverNoise Then
                    avernoise(i) += Spectrums(i, j)
                    avernoisecntr(i) += 1
                    If peaknoise(i) < Spectrums(i, j) Then peaknoise(i) = Spectrums(i, j)
                End If
            Next
            'For j = nub To ub
            '    If Spectrums(i, j) > -90 Then
            '        avernoise(i) += Spectrums(i, j)
            '        avernoisecntr(i) += 1
            '        If peaknoise(i) < Spectrums(i, j) Then peaknoise(i) = Spectrums(i, j)
            '    End If
            'Next




            avernoise(i) /= avernoisecntr(i)

            _signal(i) = max(i)
            _noise(i) = avernoise(i)
        Next


        Signal = _signal
        Noise = _noise
        Return True
    End Function

    Dim LastWDping As Date = Now
    Private Sub tmr_ConnectionWatchDog_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles tmr_ConnectionWatchDog.Elapsed
        If p_IsConnectionWatchDogEnable Then
            If Me.IsConnected Then
                If (Now - LastWDping) >= p_ConnectionWatchDogEnableInterval Then
                    LastWDping = Now


                    If Not COM.CMD_PING Then
                        p_IsConnected = False
                        RaiseEvent ConnectionStateChanged(p_IsConnected)
                    End If
                End If
            End If
        End If 
    End Sub

    Private Sub COM_PackReaded(ByVal Pack As pack) Handles COM.PackReaded
        LastWDping = Now
    End Sub

    Private Sub COM_PackWrited(ByVal Pack As pack) Handles COM.PackWrited
        '     LastWDping = Now
    End Sub

    Private Sub COM_SonoPackRecieved(ByVal ByteArr() As Byte) Handles COM.SonoPackRecieved
        On Error Resume Next
        ReDim Preserve p_SonoAmp(511)
        ReDim Preserve p_SonoPh1(511)
        ReDim Preserve p_SonoPh2(511)
        For i = 0 To 511
            p_SonoAmp(i) = Detector.GetAmp(ByteArr(i * 2))
            p_SonoPh1(i) = ByteArr(1024 + i)
            p_SonoPh2(i) = ByteArr(1536 + i)
        Next
        RaiseEvent SonoChanged(p_SonoAmp, p_SonoPh1, p_SonoPh2)
    End Sub

    
End Class