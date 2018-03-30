

Public Class Device

#Region "Variables"
    Dim WithEvents SP As New IO.Ports.SerialPort
    Dim WithEvents tmr_Watcher As New Windows.Forms.Timer
    Dim WithEvents tmr_WriteGenerationValues As New Windows.Forms.Timer
    Dim LastCounterWorkTime As Date = Now
    ''' <summary> 
    ''' Показывает, были ли изменены параметры эмуляции целей. Используется компонентом tmr_WriteGenerationValues
    ''' </summary>
    ''' <remarks></remarks>
    Dim IsTargetValuesChanged As Boolean = False
    Dim IsChannelValuesChanged As Boolean = False
    Const SerialPortBaudRate As Integer = 115200
    Const SerialPortMaxTimeOut As Integer = 400

    Dim Flash() As Byte
#End Region

#Region "Attenuation"
    Public AttenuationTable(255, 255) As Double
    Public AttenuationBaseIndex As Byte = 30

    Private Sub WriteInAttenuationTable(ByVal BaseIndex As Byte)
        Dim Att As Integer = 0
        Dim Amp As Integer = 0
        For Att = BaseIndex To 255
            Dim k1 As Double = GetAttK(BaseIndex)
            Dim k2 As Double = GetAttK(Att)
            Dim k3 As Double = k1 / k2
            For Amp = 0 To 255
                AttenuationTable(Att, Amp) = Amp * k3
            Next
        Next
    End Sub

    Private Function GetAttK(ByVal Index As Byte) As Double
        Dim r As Double = 0
        Try
            r = CDbl(0.05) * CDbl(0.375) * CDbl(Index)
            r = Math.Pow(10, r)
        Catch ex As Exception
            Beep()
        End Try

        Return r
    End Function

    Public Function GetAttAmp(ByVal Parrot As Double, ByVal MaxAmp As Byte, ByRef Att As Byte, ByRef Amp As Byte, ByVal IsAttManual As Boolean) As Boolean
        If Parrot > 255 Then Return False
        ' search for max legal att
        Dim in1 As Byte = 0

        If IsAttManual = True Then
            in1 = Att
        Else
            For i = 255 To AttenuationBaseIndex Step -1
                in1 = i
                If AttenuationTable(i, MaxAmp) > Parrot Then
                    Exit For
                End If

                If i = AttenuationBaseIndex Then
                    Exit For
                End If
            Next
        End If


        ' search for amp
        Dim k1 As Double = 0
        Dim k2 As Double = 0
        For i = 0 To 254
            If AttenuationTable(in1, i) <= Parrot Then
                If AttenuationTable(in1, i + 1) > Parrot Then
                    k1 = Math.Abs(AttenuationTable(in1, i) - Parrot)
                    k2 = Math.Abs(AttenuationTable(in1, i) - Parrot)
                    If k1 < k2 Then
                        Att = in1
                        Amp = i
                    Else
                        Att = in1
                        Amp = i + 1
                    End If
                    Return True
                End If
            End If
            If i = 254 Then
                Att = in1
                Amp = 255
                Return False
            End If
        Next
        Return True
    End Function

#End Region


#Region "Propertys"

    Dim WithEvents trg1 As New Target
    Dim WithEvents trg2 As New Target
    Dim WithEvents trg3 As New Target
    Dim WithEvents trg4 As New Target
    Dim WithEvents trg5 As New Target


    Dim p_Targets() As Target = {trg1, trg2, trg3, trg4, trg5}
    Dim p_Channels() As Channel = {New Channel, New Channel, New Channel, New Channel}
    Dim p_IsConnected As Boolean = False
    Dim p_Jumpers(3) As Boolean
    Dim p_IsUserCalibrationAllowed As Boolean = False
    Dim p_ID As Byte()
    Dim p_FrequencyShiftValue As Double = 0.5
    Dim p_CanServiceMenu As Boolean = False
    Dim p_SpeedToFreqCoeff As Double = 44.753
    Dim p_ADCIndex As Double = 3.3
    Enum ModificationEnum
        Литера1
        Литера2
    End Enum


    Public Property ADC_Index As Double
        Get
            Return p_ADCIndex
        End Get
        Set(ByVal value As Double)
            p_ADCIndex = value
        End Set
    End Property

    ''' <summary>
    ''' Возвращает 5 целей, эмулируемых устройством
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Targets As Target()
        Get
            Return p_Targets
        End Get
        Set(ByVal rvl As Target())
            p_Targets = rvl
            IsTargetValuesChanged = True
        End Set
    End Property

    ''' <summary>
    ''' Возвращает 4 канала генерации. {Sin1, Cos1, Sin2, Cos2}
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Channels As Channel()
        Get
            Return p_Channels
        End Get
        Set(ByVal rvl As Channel())
            p_Channels = rvl
            IsTargetValuesChanged = True
        End Set
    End Property






    ''' <summary>
    ''' Возвращает значение, показывающее подключено ли утройство
    ''' </summary>
    Public ReadOnly Property IsConnected As Boolean
        Get
            Return p_IsConnected
        End Get
    End Property


    ''' <summary>
    ''' Возвращает значения джамперов устройства
    ''' </summary>
    Public ReadOnly Property Jumpers As Boolean()
        Get
            Return p_Jumpers
        End Get
    End Property


    ''' <summary>
    ''' Возвращает значение, показывающее, возможна ли калибровка (выкручен ли винт калибровки)
    ''' </summary>
    Public ReadOnly Property IsUserCalibrationAllowed As Boolean
        Get
            Return p_IsUserCalibrationAllowed
        End Get
    End Property

    Dim p_IsOldFlashUsed As Boolean? = Nothing
    ''' <summary>
    ''' Возвращает значение, показывающее, возможна ли калибровка (выкручен ли винт калибровки)
    ''' </summary>
    Public ReadOnly Property IsOldFlashUsed As Boolean?
        Get
            Return p_IsOldFlashUsed
        End Get
    End Property

    ''' <summary>
    ''' Возвращает идентификационный номер прибора (по микросхеме)
    ''' </summary>
    Public ReadOnly Property ID As String
        Get
            Dim str As String = ""
            If p_ID IsNot Nothing Then
                For i = 0 To UBound(p_ID)
                    Dim c As String = Hex(p_ID(i))
                    If c.Length = 1 Then c = "0" & c
                    str &= c
                Next
            End If
            Return str
        End Get
    End Property






    ''' <summary>
    ''' Возвращает имя порта, к которому подключено устройство. При отсутсвии подключения возвращает Nothing
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property PortName As String
        Get
            Return SP.PortName
        End Get
    End Property

    ' TODO : WTF
    ''' <summary>
    ''' Возвращает структуру S3Settings, отображающую параметры устройства.
    ''' </summary>
    ''' <remarks></remarks>
    Dim p_Settings As New DeviceSettings
    Public ReadOnly Property Settings As DeviceSettings
        Get
            Return p_Settings
        End Get
    End Property

    ''' <summary>
    ''' Величина частотной манипуляции. Используется для пересчета рассояния в фазовую разность каналов генерации. Задается в MHz
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FrequencyShiftValue As Double
        Get
            Return p_FrequencyShiftValue
        End Get
        Set(ByVal rvl As Double)
            p_FrequencyShiftValue = rvl
            IsTargetValuesChanged = True
        End Set
    End Property

    ''' <summary>
    ''' Разрешено ли пользоваться меню сервис
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property CanServiceMenu As Boolean
        Get
            Return p_CanServiceMenu
        End Get
    End Property


    Public Property SpeedToFreqCoeff As Double
        Get
            Return p_SpeedToFreqCoeff
        End Get
        Set(ByVal value As Double)
            p_SpeedToFreqCoeff = value
        End Set
    End Property

    Public p_Modification As ModificationEnum = ModificationEnum.Литера1
    Public ReadOnly Property Modification As ModificationEnum
        Get
            Return p_Modification
        End Get
    End Property


    Public ReadOnly Property IsCalibratedService As Boolean
        Get
            Try

                If Me.Settings.p12_AN_LastCalibrationDate.Year < 2012 Then Return False
                If Double.IsNaN(Me.Settings.p10_AN_CH2_In_SCAmp) Then Return False
                If Double.IsNaN(Me.Settings.p11_AN_CH2_In_SCPhase) Then Return False
                If Double.IsNaN(Me.Settings.p11_AN_CH2_In_SCPhase) Then Return False
                If Double.IsNaN(Me.Settings.p17_AN_RefSinAmpIn2) Then Return False
                If Double.IsNaN(Me.Settings.p18_AN_RefSinAmpOut2) Then Return False
                If Double.IsNaN(Me.Settings.p2_AN_RefSinAmpIn1) Then Return False
                If Double.IsNaN(Me.Settings.p3_AN_RefSinAmpOut1) Then Return False
                If Double.IsNaN(Me.Settings.p4_AN_CH1_Out_SCAmp) Then Return False
                If Double.IsNaN(Me.Settings.p5_AN_CH1_Out_SCPhase) Then Return False
                If Double.IsNaN(Me.Settings.p6_AN_CH2_Out_SCAmp) Then Return False
                If Double.IsNaN(Me.Settings.p7_AN_CH2_Out_SCPhase) Then Return False
                If Double.IsNaN(Me.Settings.p8_AN_CH1_In_SCAmp) Then Return False
                If Double.IsNaN(Me.Settings.p9_AN_CH1_In_SCPhase) Then Return False
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property


    Public ReadOnly Property IsCalibratedUser As Boolean
        Get
            Try
                If Me.Settings.p16_UserCalibration_Date.Year < 2012 Then Return False
                If Double.IsNaN(Me.Settings.p13_MW_A1) Then Return False
                If Double.IsNaN(Me.Settings.p14_MW_A2) Then Return False
                If Double.IsNaN(Me.Settings.p15_MW_A3) Then Return False
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property


    Public ReadOnly Property CheckSum As String
        Get
            If Flash Is Nothing Then
                Return Nothing
            Else
                Return getCRC32Hash(Flash)
            End If
        End Get
    End Property


    Private p_CheckSumReadPercentage As Integer = 0
    Public Property CheckSumReadPercentage() As Integer
        Get
            Return p_CheckSumReadPercentage
        End Get
        Set(ByVal value As Integer)
            p_CheckSumReadPercentage = value
            RaiseEvent CheckSumReadPercentageChanged(value)
        End Set
    End Property


#End Region



#Region "Events"
    Public Event PinChanged()
    Public Event SettingsChanged()
    Public Event IsUserCalibrationAllowedChanged(ByVal IsUserCalibrationAllowed As Boolean)
    Public Event CanServiceMenuChanged(ByVal CanServiceMenu As Boolean)
    Public Event ModificationChanged(ByVal Modification As ModificationEnum)
    Public Event ConnectionLost()
    Public Event CheckSumReadPercentageChanged(ByVal Percentage As Integer)




#End Region
     
    Dim WithEvents BW As ComponentModel.BackgroundWorker
    Dim WithEvents BW_CheckSumReader As ComponentModel.BackgroundWorker
     
     

#Region "Serial Port Control Block"
    'Dim buff_PortIN As New Queue
    'Dim buff_PortOUT As New Queue

    Dim _SPCB_rbts As New Queue
    Dim _SPCB_WaitForACommand As Boolean = False
    Dim _SPCB_WaitCommandID As Byte = 0
    Dim _SPCB_Answer() As Byte
    Dim _SPCB_AnswerReady As Boolean = False


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub _SPCB_SP_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SP.DataReceived
        Dim rb(SP.BytesToRead - 1) As Byte
        SP.Read(rb, 0, rb.Length)
        For i = 0 To UBound(rb)
            _SPCB_rbts.Enqueue(rb(i))

        Next

        _SPCB_PacksParsing(_SPCB_rbts)
    End Sub

    Dim _SPCB_IsDB As Boolean = False
    Dim _SPCB_IsESC As Boolean = False
    Dim _SPCB_RB() As Byte = Nothing

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <remarks></remarks>
    Private Sub _SPCB_PacksParsing(ByRef bytes As Queue)
        If bytes.Count = 0 Then Exit Sub
        Dim tB As Byte
        For i = 0 To bytes.Count - 1
            tB = bytes.Dequeue
            If _SPCB_IsDB = False Then
                If tB = &HDB Then
                    ReDim _SPCB_RB(0)
                    _SPCB_RB = Nothing
                    _SPCB_IsDB = True
                End If
            Else
                If _SPCB_IsESC = True Then
                    _AppEnd(_SPCB_RB, {_ByteStaff(tB, True)})
                    _SPCB_IsESC = False
                Else
                    Select Case tB
                        Case &H1B
                            _SPCB_IsESC = True
                        Case &HDB
                            _SPCB_RB(0) = tB
                            _SPCB_IsDB = True
                        Case &HDC

                            _SPCB_IsDB = False
                            _SPCB_RPacksParsing(_SPCB_RB)
                        Case Else
                            _AppEnd(_SPCB_RB, {tB})
                    End Select
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="bytes"></param>
    ''' <remarks></remarks>
    Private Sub _SPCB_RPacksParsing(ByRef bytes As Byte())
        ' CS
        If bytes Is Nothing Then Exit Sub ' должен быть 
        If bytes.Length = 1 Then Exit Sub ' кроме контрольной суммы должно быть еще что-то
        Dim cs As Byte = bytes(0)
        If bytes.Length > 3 Then
            For i = 1 To bytes.Length - 2
                cs = cs Xor bytes(i)
            Next
        End If
        If cs <> bytes(UBound(bytes)) Then Exit Sub
        ReDim Preserve bytes(bytes.Length - 2)

        ' Parsing


        Select Case bytes(0)

            Case &H19
                '  
                RaiseEvent PinChanged()
            Case &H20
                 
                RaiseEvent PinChanged()
            Case _SPCB_WaitCommandID
                If _SPCB_WaitForACommand = True Then
                    If bytes.Length = 1 Then
                        _SPCB_Answer = Nothing
                    Else
                        ReDim _SPCB_Answer(bytes.Length - 2)
                        For i = 1 To UBound(bytes)
                            _SPCB_Answer(i - 1) = bytes(i)
                        Next
                    End If

                    _SPCB_AnswerReady = True
                    _SPCB_WaitForACommand = False
                End If







        End Select



        bytes = Nothing




    End Sub


#End Region

#Region "Comunication protokol"
    ''' <summary>
    ''' CMD 0x00
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_TEST() As Boolean
        Dim isAn As Boolean = False
        _WriteCMD(&H0, &HFF, Nothing, isAn)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x01
    ''' </summary>
    ''' <param name="ChannelMask"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function _CMD_GEN_START(ByVal ChannelMask As Byte) As Boolean
        Dim isAn As Boolean = False
        _WriteCMD(&H1, &HFF, {ChannelMask}, isAn)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x02
    ''' </summary>
    ''' <param name="ChannelMask"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_GEN_STOP(ByVal ChannelMask As Byte) As Boolean
        Dim isAn As Boolean = False
        _WriteCMD(&H2, &HFF, {ChannelMask}, isAn)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x03 Установка параметров канала
    ''' </summary>
    ''' <param name="ChannelIndex">Номер канала, значения которого необходимо установить</param>
    ''' <param name="Sin_amp">Массив(4) амплитуды каждой из пяти целей. Задается в вольтах!!!</param>
    ''' <param name="Sin_freq">Массив(4) частота каждой из пяти целей. Задается в герцах!!!</param>
    ''' <param name="Sin_phase">Массив(4) Начальная фаза каждой из пяти целей. Задается в градусах (0..360)!!!</param>
    Private Function _CMD_SET_CH_PROP(ByVal ChannelIndex As Byte, ByVal Sin_amp() As Byte, ByVal Sin_freq() As Integer, ByVal Sin_phase() As Integer, ByVal Attenuation As Byte) As Boolean
        Dim isAn As Boolean = False
        Dim tAtten As Byte = Attenuation              ' значение ослабления для канала. расчитывается с ориентировкой на максимальную из пяти составляющих синуса
        Dim tAmp As Byte = 0
        Dim tStartPhase As Integer = 0


        Dim Data(26) As Byte

        Data(0) = ChannelIndex ' номер канала настройки которого устанавливаются
        Data(1) = tAtten



        For i = 0 To 4
            tAmp = Sin_amp(i)  ' TODO : вставить обращение к функции расчета амплитуды по заданным значениям
            tStartPhase = MakeStartPhase(Sin_phase(i)) ' TODO : вставить обращение к функции расчета начальной фазы по заданным значениям

            ' Dim qrz As Double = 0.73288
            Dim qrz As Double = 0.73244


            Data(5 * i + 2) = tAmp
            Data(5 * i + 3) = (Sin_freq(i) / qrz) \ 256 ' Старший байт частоты (скорости1)
            Data(5 * i + 4) = (Sin_freq(i) / qrz) - (Data(5 * i + 3) * 256) ' Младший байт частоты (скорости1)
            Data(5 * i + 5) = tStartPhase \ 256
            Data(5 * i + 6) = tStartPhase - (Data(5 * i + 5) * 256)
        Next
        _WriteCMD(&H3, &HFF, Data, isAn)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x05 Установить переключатель каналов (переключать каналы по импульсу синхронизации или нет)
    ''' </summary>
    ''' <param name="PinValue">
    ''' 0 - каналы не переключаются
    ''' 1 - каналы попарно переключаются по импульсу синхронизации
    ''' </param>
    Private Function _CMD_SEL_CH(ByVal PinValue As Byte) As Boolean
        Dim isAn As Boolean = False
        _WriteCMD(&H5, &HFF, {PinValue}, isAn)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x06 Начать измерение частоты
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_FR_START() As Boolean
        Dim isAn As Boolean = False
        _WriteCMD(&H6, &HFF, Nothing, isAn)
        Return isAn
    End Function

    ''' <summary>
    '''  CMD 0x07 Считать измеренную частоту
    ''' </summary>
    ''' <param name="C1"></param>
    ''' <param name="C2"></param>
    ''' <param name="C3"></param>
    ''' <param name="C4"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_FR_GET(ByRef C1 As ULong, ByRef C2 As ULong, ByRef C3 As ULong, ByRef C4 As ULong) As Boolean
        Dim isAn As Boolean = False
        Dim answer() As Byte = _WriteCMD(&H7, &H7, Nothing, isAn)
        If answer Is Nothing Then Return False
        If answer.Length = 19 Then



            C1 = Math.BigMul(&H1, answer(3))
            C1 += Math.BigMul(&H100, answer(4))
            C1 += Math.BigMul(&H10000, answer(5))
            C1 += Math.BigMul(&H1000000, answer(6))

            C2 = Math.BigMul(&H1, answer(7))
            C2 += Math.BigMul(&H100, answer(8))
            C2 += Math.BigMul(&H10000, answer(9))
            C2 += Math.BigMul(&H1000000, answer(10))

            C3 = Math.BigMul(&H1, answer(11))
            C3 += Math.BigMul(&H100, answer(12))
            C3 += Math.BigMul(&H10000, answer(13))
            C3 += Math.BigMul(&H1000000, answer(14))

            C4 = Math.BigMul(&H1, answer(15))
            C4 += Math.BigMul(&H100, answer(16))
            C4 += Math.BigMul(&H10000, answer(17))
            C4 += Math.BigMul(&H1000000, answer(18))
            Return isAn
        End If
        Return False
    End Function

    ''' <summary>
    ''' CMD 0x08 Очистить частоту
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_FR_CLEAR() As Boolean
        Dim isAn As Boolean = False
        _WriteCMD(&H8, &HFF, Nothing, isAn)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x11
    ''' </summary>
    ''' <param name="StartAddress"></param>
    ''' <param name="Length"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function _CMD_FLASH_READ(ByVal StartAddress As UInteger, ByVal Length As UInteger) As Byte()
        If StartAddress + Length > 65535 Then Return Nothing
        Dim Data(Length - 1) As Byte
        Dim isAn As Boolean = False
        If Length > 64 Then

            Dim cnt As Integer = Math.Truncate(Length / 64)
            Dim tD(0) As Byte
            Try
                For i = 0 To cnt - 1
                    tD = _CMD_FLASH_READ(StartAddress + i * 64, 64)
                    If tD Is Nothing Then Return Nothing
                    If tD.Length <> 64 Then Return Nothing

                    For j = 0 To 63
                        Data(i * 64 + j) = tD(j)
                    Next
                Next
            Catch ex As Exception
                Beep()
            End Try
            ' остатки сладки
            If cnt * 64 <> Length Then
                tD = _CMD_FLASH_READ(StartAddress + cnt * 64, Length - cnt * 64)
                If tD Is Nothing Then Return Nothing
                If tD.Length <> Length - cnt * 64 Then Return Nothing
                For j = 0 To UBound(tD)
                    Data(cnt * 64 + j) = tD(j)
                Next
            End If
            Return Data
        Else
            Data(2) = Length
            Data(1) = StartAddress \ 256
            Data(0) = StartAddress - (Data(1) * 256)

            Dim Answer(0) As Byte

            For attempts = 1 To 5
                Answer = _WriteCMD(&H11, &H13, Data, isAn)
                If Answer Is Nothing Then
                    isAn = False
                Else
                    If Answer(0) <> Data(0) Then isAn = False
                    If Answer(1) <> Data(1) Then isAn = False
                    If Answer.Length < 4 Then isAn = False
                    If Answer(2) <> Data(2) Then isAn = False
                End If

                If isAn = True Then Exit For
                If attempts = 5 Then Return Nothing
            Next
            Dim ab(Answer.Length - 4) As Byte
            Array.ConstrainedCopy(Answer, 3, ab, 0, ab.Length)
            Return ab
        End If
    End Function






    ''' <summary>
    ''' Функция записывает блок данных по указанному адресу.
    ''' </summary>
    ''' <param name="StartAddress"></param>
    ''' <param name="Data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_FLASH_WRITE(ByVal StartAddress As UInteger, ByVal Data As Byte()) As Boolean
        If StartAddress < &H8000 Then Return False
        Dim isAn As Boolean = False
        Dim WrData(63) As Byte
        If Data.Length > 64 Then
            ' пишем блоками по 64 байта
            Dim cnt As Integer = Math.Truncate(Data.Length / 64)
            For i = 0 To cnt - 1
                ' первые блоки по 64 байта
                Dim tadr As UInteger = StartAddress + 64 * i
                For j = 0 To 63
                    WrData(j) = Data(j + 64 * i)
                Next
                If _CMD_FLASH_WRITE(tadr, WrData) = False Then Return False
            Next
            ' остатки сладки
            If cnt * 64 <> Data.Length Then
                ReDim WrData(Data.Length - (cnt * 64) - 1)


                For i = 0 To UBound(WrData)
                    WrData(i) = Data(i + (cnt * 64))
                Next
                If _CMD_FLASH_WRITE(StartAddress + 64 * cnt, WrData) = False Then Return False
            End If
        Else

            ReDim WrData(Data.Length + 2)

            WrData(2) = Data.Length
            WrData(1) = StartAddress \ 256
            WrData(0) = StartAddress - (WrData(1) * 256)

            For i = 0 To UBound(Data)
                WrData(i + 3) = Data(i)
            Next

            For attemts = 1 To 5
                _WriteCMD(&H12, &HFF, WrData, isAn)
                If isAn = True Then Exit For
                If attemts = 5 Then Return False
            Next
        End If
        ' все записано. осталось проверить все ли сохранено
        Dim chckbts() As Byte = _CMD_FLASH_READ(StartAddress, Data.Length)
        If chckbts Is Nothing Then Return False
        If chckbts.Length <> Data.Length Then Return False
        For i = 0 To UBound(Data)
            If chckbts(i) <> Data(i) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' CMD 0x14
    ''' </summary>
    ''' <param name="SectorIndex"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_FLASH_ERRASE_SECTOR(ByVal SectorIndex As Byte) As Boolean
        'If Me.IsOldFlashUsed Then
        '    If SectorIndex < 2 Then Return False
        'Else
        '    If SectorIndex < 4 Then Return False
        'End If

        Dim isAn As Boolean = False
        _WriteCMD(&H14, &HFF, {SectorIndex}, isAn)
        _WriteCMD(&H14, &HFF, {SectorIndex}, isAn)
        System.Threading.Thread.Sleep(700)
        Return isAn
    End Function

    ''' <summary>
    ''' CMD 0x15
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_GET_SERIAL() As Byte()
        Dim isAn As Boolean = False
        Return _WriteCMD(&H15, &H15, Nothing, isAn)
    End Function


    ''' <summary>
    ''' CMD 0x16
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_GET_ADDR() As Byte()
        Dim isAn As Boolean = False
        Return _WriteCMD(&H16, &H16, Nothing, isAn)
    End Function


    ''' <summary>
    ''' CMD 0x21
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _CMD_GET_ADC() As Byte()
        Dim isAn As Boolean = False
        Dim fdgd() As Byte = _WriteCMD(&H21, &H21, Nothing, isAn)
        If isAn Then
            Return fdgd
        Else
            Return Nothing
        End If
    End Function


    Private Function _WriteCMD(ByVal CmdID As Byte, ByVal AnswerID As Byte, ByVal Data() As Byte, ByRef IsAnswered As Boolean) As Byte()
        If IsSpPaused Then
            SerialPortPlay()
        End If
        Try
            LastCounterWorkTime = Now
            Dim WrAr() As Byte = _MakePack2Device(CmdID, Data)
            _SPCB_WaitCommandID = AnswerID
            _SPCB_WaitForACommand = True
            _SPCB_AnswerReady = False
            If IsSpPaused Then Return Nothing
            SP.Write(WrAr, 0, WrAr.Length)
            IsAnswered = False
            Dim tm As Date = Now
            Do Until (Now - tm) > TimeSpan.FromMilliseconds(SerialPortMaxTimeOut)
                If _SPCB_AnswerReady = True Then
                    IsAnswered = True
                    Return _SPCB_Answer
                End If
            Loop


        Catch ex As Exception
            If Not IsSpPaused Then
                p_IsConnected = False
                RaiseEvent ConnectionLost()
            End If

        End Try
        Return Nothing
    End Function

#End Region

#Region "Utility Functions"
    ''' <summary>
    ''' Фунция формирует массив байт для передачи устройсту
    ''' </summary>
    ''' <param name="Command">Код команды</param>
    ''' <param name="Data">Массив передаваемыз данных</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function _MakePack2Device(ByVal Command As Byte, ByVal Data As Byte()) As Byte()
        Dim ans() As Byte = {&HDB}      ' выходной массив (пока содержит лишь признак начала пакета - DB
        Dim tar() As Byte               ' временное хранилище реультатов байтстаффинга
        Dim CS As Byte = Command        ' контрольная сумма пакета
        ' код команды
        tar = _ByteStaff(Command)
        _AppEnd(ans, tar)
        ' тело пакета
        If Data IsNot Nothing Then
            For i = 0 To UBound(Data)
                tar = _ByteStaff(Data(i))
                _AppEnd(ans, tar)
                CS = CS Xor Data(i)
            Next
        End If
        tar = _ByteStaff(CS)
        _AppEnd(ans, tar)
        _AppEnd(ans, {&HDC})
        Return ans
    End Function

    ''' <summary>
    ''' Функия возвращает (при необходимости) ESC-последовательность заданного байта. В случае, если байт не нуждается в замене - функция возвращает сам входной параметр
    ''' </summary>
    ''' <param name="ByteToConvert">Байт для байтстаффинга</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Overloads Function _ByteStaff(ByVal ByteToConvert As Byte) As Byte()
        Dim ans() As Byte = {ByteToConvert}
        Select Case ByteToConvert
            Case &HDB
                ReDim ans(1)
                ans(0) = &H1B
                ans(1) = &HFA
            Case &HDC
                ReDim ans(1)
                ans(0) = &H1B
                ans(1) = &HFD
            Case &H1B
                ReDim ans(1)
                ans(0) = &H1B
                ans(1) = &H3A
            Case &H0
                ReDim ans(1)
                ans(0) = &H1B
                ans(1) = &H21
            Case &HFF
                ReDim ans(1)
                ans(0) = &H1B
                ans(1) = &HDE
        End Select
        Return ans
    End Function

    ''' <summary>
    ''' Функция возвращает байт, эквивалентный заданной ESC последовательности
    ''' </summary>
    ''' <param name="EscCode">Код ESC последовательности</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Overloads Function _ByteStaff(ByVal EscCode As Byte, ByVal isRetNothing As Boolean) As Byte
        Select Case EscCode
            Case &HFA
                Return &HDB
            Case &HFD
                Return &HDC
            Case &H3A
                Return &H1B
            Case &H21
                Return &H0
            Case &HDE
                Return &HFF
            Case Else
                If isRetNothing = True Then
                    Return Nothing
                Else
                    Return EscCode
                End If
        End Select
    End Function

    ''' <summary>
    ''' Копирует содержимое массива SourceArray в конец массива destinationArray, предварительно изменив его размер
    ''' </summary>
    ''' <param name="destinationArray">Массив в который производится копирование</param>
    ''' <param name="SourceArray">мМассив для копирования</param>
    ''' <remarks></remarks>
    Private Sub _AppEnd(ByRef destinationArray() As Byte, ByVal SourceArray() As Byte)
        If destinationArray IsNot Nothing Then
            Array.Resize(destinationArray, destinationArray.Length + SourceArray.Length)
            Array.Copy(SourceArray, 0, destinationArray, destinationArray.Length - SourceArray.Length, SourceArray.Length)
        Else
            destinationArray = SourceArray
        End If
    End Sub



    Public Function MakeStartPhase(ByVal StartPhase As Double) As Integer
        'Dim an As Double = StartPhase
        'If an < 0 Then
        '    Do Until an >= 0
        '        an += 360
        '    Loop
        'End If
        'If an > 360 Then
        '    Do Until an <= 360
        '        an -= 360
        '    Loop
        'End If

        'Dim x As Integer
        'x = (an + 90) Mod 360
        'x = Math.Truncate(((x * 4096) / 360) * 16)

        'Return x

        Dim an As Double = StartPhase
        If an < 0 Then
            Do Until an >= 0
                an += 360
            Loop
        End If
        If an > 360 Then
            Do Until an <= 360
                an -= 360
            Loop
        End If



        Dim x As Double
        x = (an + 90) Mod 360
        x = Math.Truncate(((x * 4096) / 360) * 16)

        Return x

    End Function


    Public Function GetAmpFromDistance(ByVal Distance As Double, ByVal A1 As Double, ByVal A2 As Double, ByVal A3 As Double) As Double
        'Me.Settings.p13_MW_A1 = 100
        'Me.Settings.p14_MW_A2 = 155
        'Me.Settings.p15_MW_A3 = 1.4

        If Me.Modification = ModificationEnum.Литера1 Then

            Dim amp1 As Double = 0
            Dim k As Double = (Me.Settings.p13_MW_A1 ^ Me.Settings.p15_MW_A3) * Me.Settings.p14_MW_A2
            amp1 = (k / (Distance ^ Me.Settings.p15_MW_A3))
            Dim amp2 As Double = 0
            Try
                amp2 = (amp1 - 0) '/ 3.6
                If amp2 > 255 Then amp2 = 255
            Catch ex As Exception

            End Try
            Return amp2
        Else
            Dim amp1 As Double = 0
            Dim k As Double = (Me.Settings.p13_MW_A1 ^ Me.Settings.p15_MW_A3) * Me.Settings.p14_MW_A2
            amp1 = (k / (Distance ^ Me.Settings.p15_MW_A3))
            Dim amp2 As Double = 0
            Try
                amp2 = (amp1 - 0) '/ 3.6
                If amp2 > 255 Then amp2 = 255
            Catch ex As Exception

            End Try
            Return amp2
        End If



    End Function

    Public Function GetPhaseByDistance(ByVal Distance As Double, ByVal FreqShiftValue As Double) As Double
        Console.WriteLine(2.4 * FreqShiftValue * Distance & " v")
      
        Return Math.IEEERemainder(2.4 * FreqShiftValue * Distance, 360)
    End Function





#End Region

#Region "Control block"


    ''' <summary>
    ''' Производит попытку подключения к устройству и возвращает ее результат.
    ''' </summary>
    Public Overloads Function Connect() As Boolean
        For Each spn As String In My.Computer.Ports.SerialPortNames
            If Me.Connect(spn) Then Return True
        Next
        Return False
    End Function

    ''' <summary>
    ''' Производит попытку подключения к устройству на указанном порте и возвращает ее результат.
    ''' </summary>
    ''' <param name="PortName">Имя порта истемы для подключение к устройству</param>
    Public Overloads Function Connect(ByVal PortName As String) As Boolean

        WriteInAttenuationTable(AttenuationBaseIndex)
        ' PING
        If SP Is Nothing Then SP = New IO.Ports.SerialPort

        If SP.IsOpen = True Then SP.Close()
        SP.PortName = PortName
        SP.BaudRate = SerialPortBaudRate

        Try
            SP.Open()
        Catch ex As Exception
            Return False
        End Try
        If Not SP.IsOpen Then Return False



        If _CMD_TEST() = True Then
            p_IsConnected = True
        Else
            SP.Close()
        End If


        If Me.IsConnected = False Then Return False

        ' JUMPERS
        If UpdateJumpers(5) = False Then Return False

        ' ID
        If UpdateID(5) = False Then Return False


        'Do
        '    UpdateJumpers(5)
        'Loop


        ' ChannelSwithcing
        If ChangeSyncReaction(True, 5) = False Then Return False




        ' Settings
        If Me.ReadSettings(False, Nothing) = False Then Return False

        If Me.IsConnected = False Then Return False
        For i = 0 To 4
            p_Targets(i) = New Target
        Next
        ' Flash
        Flash = Nothing


        BW = New ComponentModel.BackgroundWorker
        BW.WorkerSupportsCancellation = True
        BW.RunWorkerAsync()




        GenerationStart()
        p_CheckSumReadPercentage = 0
        Return True
    End Function


    Public Function CheckDevice(ByVal PortName As String) As Boolean

        WriteInAttenuationTable(AttenuationBaseIndex)
        ' PING
        If SP Is Nothing Then SP = New IO.Ports.SerialPort
        If SP.IsOpen = True Then SP.Close()
        SP.PortName = PortName
        SP.BaudRate = SerialPortBaudRate
        Try
            SP.Open()
        Catch ex As Exception
            Return False
        End Try
        If Not SP.IsOpen Then Return False
        Dim res As Boolean = _CMD_TEST()
        p_IsConnected = False
        SP.Close()
        Return res
    End Function


    Public Sub Disconnect()
        _CMD_GEN_STOP(&HFF)
        Try
            BW.CancelAsync()
        Catch ex As Exception

        End Try

        System.Threading.Thread.Sleep(500)
        SP.Close()
        p_IsConnected = False
    End Sub



    ''' <summary>
    ''' Производит попытку запустить генерацию и возвращает ее результат.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GenerationStart() As Boolean
        For i = 0 To 3
            If _CMD_GEN_START(&HFF) Then Return True
        Next
        Return False
    End Function

    Public Function GenerationStop() As Boolean
        For i = 0 To 3
            If _CMD_GEN_STOP(&HFF) Then Return True
        Next
        Return False
    End Function



    ''' <summary>
    ''' Обнавляет состояния джамперов устройства.
    ''' Возвращает False при отсутствии подключения к устройству.
    ''' </summary>
    ''' <param name="Attempts">Количество попыток выполнить команду перед возникновенем события ConnectionLost</param>
    Public Function UpdateJumpers(ByVal Attempts As UInteger) As Boolean
        For i As UInteger = 0 To Attempts
            Dim tb() As Byte = _CMD_GET_ADDR()

            If tb IsNot Nothing Then
                If tb.Length > 3 Then
                    For g As Integer = 0 To UBound(tb) 
                    Next
                     
                    'p_Jumpers = tb
                    ReDim p_Jumpers(3)
                    Dim d As New BitArray(New Byte() {tb(3)})
                    For j = 0 To 3
                        p_Jumpers(j) = Not d.Get(j)
                    Next

                    If p_IsUserCalibrationAllowed <> p_Jumpers(0) Then
                        p_IsUserCalibrationAllowed = p_Jumpers(0)
                        RaiseEvent IsUserCalibrationAllowedChanged(p_IsUserCalibrationAllowed)
                    End If

                    Dim jM As ModificationEnum = ModificationEnum.Литера1
                    If p_Jumpers(1) Then jM = ModificationEnum.Литера2
                    If p_Modification <> jM Then
                        p_Modification = jM
                        RaiseEvent ModificationChanged(p_Modification)
                    End If

                    If p_CanServiceMenu <> p_Jumpers(3) Then
                        p_CanServiceMenu = p_Jumpers(3)
                        RaiseEvent CanServiceMenuChanged(p_CanServiceMenu)
                    End If
                    Return True
                End If
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Обнавляет уникальный идентификационный номер устройства
    ''' Возвращает False при отсутствии подключения к устройству.
    ''' </summary>
    ''' <param name="Attempts">Количество попыток выполнить команду перед возникновенем события ConnectionLost</param>
    Public Function UpdateID(ByVal Attempts As UInteger) As Boolean
        For i As UInteger = 0 To Attempts
            Dim ser As Byte() = _CMD_GET_SERIAL()
            If ser IsNot Nothing Then
                p_ID = ser
                Return True
            End If
        Next
        Return False
    End Function

    Dim IsSpPaused As Boolean = False
    Public Sub SerialPortPause()
        IsSpPaused = True
        SP.Close()
        System.Threading.Thread.Sleep(1000)
    End Sub

    Public Sub SerialPortPlay()
        SP.Open()
        System.Threading.Thread.Sleep(100)
        IsSpPaused = False
    End Sub

    Public Function ChangeSyncReaction(ByVal SyncReactionEnable As Boolean, ByVal Attempts As UInteger) As Boolean
        For i As UInteger = 0 To Attempts
            If _CMD_SEL_CH(SyncReactionEnable) = True Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function FrequencyStart(ByVal Attempts As UInteger) As Boolean
        LastCounterWorkTime = Now
        For i As UInteger = 0 To Attempts
            If _CMD_FR_START() = True Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function FrequencyGet(ByRef C1 As ULong, ByRef C2 As ULong, ByRef C3 As ULong, ByRef C4 As ULong) As Boolean
        LastCounterWorkTime = Now
        For i = 1 To 5
            If _CMD_FR_GET(C1, C2, C3, C4) = True Then Return True
        Next
        Return False
    End Function


    Public Function SET_Channel_PROP(ByVal ChannelIndex As Byte, ByVal Sin_amp() As Byte, ByVal Sin_freq() As Integer, ByVal Sin_phase() As Integer, ByVal Attenuation As Byte) As Boolean
        For i = 1 To 5
            If _CMD_SET_CH_PROP(ChannelIndex, Sin_amp, Sin_freq, Sin_phase, Attenuation) = True Then Return True
        Next
        Return False
    End Function




    Private Function _ReadKey(ByVal SectorIndex As Byte) As Byte()
        Dim key3() As Byte = Nothing
        Dim add As UInteger

        If Me.IsOldFlashUsed Is Nothing Then
            GetFlash()
        End If


        If Me.IsOldFlashUsed Then
            If SectorIndex = 2 Then
                add = &HBFF8
            ElseIf SectorIndex = 3 Then
                add = &HCFF8
            End If
        Else
            If SectorIndex = 2 Then
                add = &H9FF8
            ElseIf SectorIndex = 3 Then
                add = &HCFF8
            End If
        End If



        For i = 1 To 5
            key3 = _CMD_FLASH_READ(add, 8)
            If key3 IsNot Nothing Then If key3.Length = 8 Then Exit For
            If i = 5 Then Return Nothing
        Next
        Return key3
    End Function

    Private Sub GetFlash()
        'E000
        _CMD_FLASH_ERRASE_SECTOR(7)
        System.Threading.Thread.Sleep(700)
        Dim b As Boolean = False
        Dim bt() As Byte = Nothing
        Do Until bt IsNot Nothing
            bt = _CMD_FLASH_READ(&HE000, 8)
            System.Threading.Thread.Sleep(100)
        Loop
        b = True
        For i = 0 To 7
            If bt(i) <> i Then
                b = False
            End If
        Next
        If b Then
            p_IsOldFlashUsed = True
            Exit Sub
        End If
        b = False
        Do Until b
            b = _CMD_FLASH_WRITE(&HE000, {0, 1, 2, 3, 4, 5, 6, 7})
            System.Threading.Thread.Sleep(100)
        Loop
        System.Threading.Thread.Sleep(500)
        _CMD_FLASH_ERRASE_SECTOR(7)
        System.Threading.Thread.Sleep(700)
        bt = Nothing
        Do Until bt IsNot Nothing
            bt = _CMD_FLASH_READ(&HE000, 8)
            System.Threading.Thread.Sleep(100)
        Loop
        b = True
        For i = 0 To 7
            If bt(i) <> i Then
                b = False
            End If
        Next
        p_IsOldFlashUsed = b
    End Sub


    Private Function _WriteKey(ByVal SectorIndex As Byte, ByVal Key() As Byte) As Boolean
        Dim add As UInteger
        If Me.IsOldFlashUsed Is Nothing Then
            GetFlash()
        End If
        If Me.IsOldFlashUsed Then
            If SectorIndex = 2 Then
                add = &HBFF8
            ElseIf SectorIndex = 3 Then
                add = &HCFF8
            Else
                Return False
            End If
        Else
            If SectorIndex = 2 Then
                add = &H9FF8
            ElseIf SectorIndex = 3 Then
                add = &HCFF8
            Else
                Return False
            End If
        End If

        If Key Is Nothing Then Return False
        If Key.Length <> 8 Then Return False
        For i = 1 To 5
            If _CMD_FLASH_WRITE(add, Key) = True Then Exit For
            If i = 5 Then Return False
        Next
        Return True
    End Function

    Private Function _CheckKey(ByVal Key() As Byte, ByVal TrueKey() As Byte) As Boolean
        If ((Key Is Nothing) Or (TrueKey Is Nothing)) Then Return False
        If Key.Length <> TrueKey.Length Then Return False
        For i = 0 To UBound(Key)
            If Key(i) <> TrueKey(i) Then Return Nothing
        Next
        Return True
    End Function

    Public Function ReadSettings(ByVal manualAddr As Boolean, ByVal startAddress As ULong) As Boolean
        LastCounterWorkTime = Now

        ' TODO : fix about new flash 
        Dim rSett As New DeviceSettings
        Dim key3() As Byte = _ReadKey(2)


        Dim MainSettSectorIndex As Byte = 6
        Dim ReservSettSectorIndex As Byte = 4
        If Me.IsOldFlashUsed Is Nothing Then
            GetFlash()
        End If
        If IsOldFlashUsed Then
            MainSettSectorIndex = 3
            ReservSettSectorIndex = 2
        End If

        If manualAddr = False Then
            If _CheckKey(key3, rSett.trueKey) = True Then
                ' не стерт 3 сектор и настройки лежат там!!! достать и переписать в 4й
                Dim tS As New DeviceSettings
                tS = DeviceSettings.FromArray(_CMD_FLASH_READ(&H8000, DeviceSettings.SetCount * 8))
                If tS IsNot Nothing Then
                    For i = 1 To 5
                        If _CMD_FLASH_ERRASE_SECTOR(MainSettSectorIndex) = True Then Exit For
                        If i = 5 Then Return Nothing
                    Next
                    Threading.Thread.Sleep(500)

                    For i = 1 To 5
                        If _CMD_FLASH_WRITE(&HC000, tS.ToArray) = True Then Exit For
                        If i = 5 Then Return Nothing
                    Next

                    For i = 1 To 5
                        If _CMD_FLASH_ERRASE_SECTOR(ReservSettSectorIndex) = True Then Exit For
                        If i = 5 Then Return Nothing
                    Next
                    Threading.Thread.Sleep(500)
                    p_Settings = tS
                    Return True
                Else
                    p_Settings = Nothing
                    Return False
                End If

                For i = 1 To 5
                    If _CMD_FLASH_ERRASE_SECTOR(ReservSettSectorIndex) = True Then Exit For
                    If i = 5 Then Return Nothing
                Next
                Threading.Thread.Sleep(500)
            Else
                rSett = DeviceSettings.FromArray(_CMD_FLASH_READ(&HC000, DeviceSettings.SetCount * 8))
                p_Settings = rSett
                Return True
            End If
        Else
            rSett = DeviceSettings.FromArray(_CMD_FLASH_READ(startAddress, DeviceSettings.SetCount * 8))
            p_Settings = rSett
            Return True
        End If
        'p_Settings = rSett
        'Return True
    End Function

    Public Function WriteSettings(ByVal Settings As DeviceSettings) As Boolean
        LastCounterWorkTime = Now
        ' для старой флэхи настойки лежат в сеторе 4м(секторе) i=3, резерв в 3м
        ' для новой флэхи настойки лежат в сеторе 6м(секторе) i=5, резерв в 5м

        ' настройки хранятся в четвертом секторе флэхи. для их замены сущ сл алгоритм:
        ' 1) проверяем, есть ли настройки в 3м секторе (есть ли флаг 1,8,0,2,1,9,8,9 в конце 3 сектора)



        ' 1) копируем настройки из 4го в 3й
        ' 2) выставляем в 3м флаг (последние 8 байт сектора = 1,8,0,2,1,9,8,9) - тем самым указываем что был начат процесс изменения настроек
        ' 3) трем 4й сектор
        ' 4) пишем НОВЫЕ настройки в 4й сектор
        ' 5) проверяем что записались. если нет, то к пункту 3
        ' 6) трем 3й сектор



        If p_IsConnected = False Then Return False

        Dim nsta() As Byte = Settings.ToArray

        If nsta Is Nothing Then Return False

        Dim key3() As Byte = Nothing
        Dim key4() As Byte = Nothing

        Dim MainSettSectorIndex As Byte = 6
        Dim ReservSettSectorIndex As Byte = 4
        If Me.IsOldFlashUsed Is Nothing Then
            GetFlash()
        End If
        If IsOldFlashUsed Then
            MainSettSectorIndex = 3
            ReservSettSectorIndex = 2
        End If


        ' 1)

        Dim rSet As New DeviceSettings
        ReadSettings(False, Nothing)
        rSet = p_Settings
        If Not Me.IsCalibratedService Then rSet = Settings

        For i = 1 To 5
            If _CMD_FLASH_ERRASE_SECTOR(ReservSettSectorIndex) = True Then Exit For
            If i = 5 Then Return False
        Next
        '    Threading.Thread.Sleep(500)
        If rSet IsNot Nothing Then
            For i = 1 To 5
                If _CMD_FLASH_WRITE(&H8000, rSet.ToArray) = True Then Exit For
                If i = 5 Then Return False
            Next
        End If


        If _WriteKey(2, (New DeviceSettings).trueKey) = False Then Return False

        For i = 1 To 5
            If _CMD_FLASH_ERRASE_SECTOR(MainSettSectorIndex) = True Then Exit For
            If i = 5 Then Return False
        Next

        '    Threading.Thread.Sleep(500)

        For i = 1 To 5
            If _CMD_FLASH_WRITE(&HC000, Settings.ToArray) = True Then Exit For
            If i = 5 Then Return False
        Next

        For i = 1 To 5
            If _CMD_FLASH_ERRASE_SECTOR(ReservSettSectorIndex) = True Then Exit For
            If i = 5 Then Return False
        Next
        '       Threading.Thread.Sleep(500)
        p_Settings = Settings
        RaiseEvent SettingsChanged()
        Return True
    End Function

#End Region


#Region "Generation Control Block"

    Public Overloads Sub SetTarget(ByVal TargetIndex As Byte, ByVal Target As Target)
        Try
            p_Targets(TargetIndex) = Target
            IsTargetValuesChanged = True

        Catch ex As Exception

        End Try

    End Sub

    Public Overloads Sub SetTarget(ByVal Targets As Target())
        If Targets IsNot Nothing Then
            If Targets.Length > 0 And Targets.Length < 6 Then
           
                For i = 0 To UBound(Targets)
                    p_Targets(i) = Targets(i)
                Next
                If UBound(Targets) < 4 Then
                    For i = 0 To UBound(Targets)
                        Dim tr As New Target()
                        tr.Enable = False
                        p_Targets(i) = tr
                    Next
                End If
                IsTargetValuesChanged = True
            End If
        End If
    End Sub

    Public Overloads Sub SetChannel(ByVal ChannelIndex As Byte, ByVal Channel As Channel)
        p_Channels(ChannelIndex) = Channel
        IsChannelValuesChanged = True
    End Sub

    Public Overloads Sub SetChannel(ByVal Channels As Channel())
        Dim li As Integer = 3
        If Channels.Length - 1 < li Then li = Channels.Length - 1
        For i = 0 To li
            p_Channels(i) = Channels(i)
        Next
        IsChannelValuesChanged = True
    End Sub

    Public Function GetADC1() As Double
        Dim ADC() As Byte
        Dim Data As Double
        ADC = _CMD_GET_ADC()

        If ADC Is Nothing Then
            Return 0
        Else
            Data = ADC(3) + ADC(4) * &H100
            Data = (Data * p_ADCIndex) / 65535
            Return Data
        End If


    End Function

    Public Function GetADC2() As Double
        Dim ADC() As Byte
        Dim Data As Double
        ADC = _CMD_GET_ADC()
        If ADC Is Nothing Then
            Return 0
        Else
            Data = ADC(5) + ADC(6) * &H100
            Data = (Data * p_ADCIndex) / 65535
            Return Data
        End If

    End Function

    Public Function GetADC1Data() As Byte()
        Dim ADC() As Byte
        Dim Data(2) As Byte
        ADC = _CMD_GET_ADC()
        If ADC Is Nothing Then
            Data(0) = 0
            Data(1) = 0
        Else
            Data(0) = ADC(3)
            Data(1) = ADC(4)
        End If

        Return Data
    End Function

    Public Function GetADC2Data() As Byte()
        Dim ADC() As Byte
        Dim Data(2) As Byte
        ADC = _CMD_GET_ADC()
        If ADC Is Nothing Then
            Data(0) = 0
            Data(1) = 0
        Else
            Data(0) = ADC(5)
            Data(1) = ADC(6)
        End If
        Return Data
    End Function

    Public Function GetADCData() As Byte()
        Dim ADC() As Byte
        ADC = _CMD_GET_ADC()
        Return ADC
    End Function

    Public Function WriteDefaultCalibration() As Boolean
        Dim sett As New DeviceSettings
        sett.p0_SerialNumber = 0
        sett.p1_SetCount = 19
        sett.p2_AN_RefSinAmpIn1 = 100
        sett.p3_AN_RefSinAmpOut1 = 100
        sett.p4_AN_CH1_Out_SCAmp = 1
        sett.p5_AN_CH1_Out_SCPhase = 90
        sett.p6_AN_CH2_Out_SCAmp = 1
        sett.p7_AN_CH2_Out_SCPhase = 90
        sett.p8_AN_CH1_In_SCAmp = 1
        sett.p9_AN_CH1_In_SCPhase = 90
        sett.p10_AN_CH2_In_SCAmp = 1
        sett.p11_AN_CH2_In_SCPhase = 90
        sett.p12_AN_LastCalibrationDate = New Date(1989, 2, 18)
        sett.p13_MW_A1 = 50
        sett.p14_MW_A2 = 150
        sett.p15_MW_A3 = 1.4
        sett.p16_UserCalibration_Date = New Date(1989, 2, 18)
        sett.p17_AN_RefSinAmpIn2 = 100
        sett.p18_AN_RefSinAmpOut2 = 100

        Return WriteSettings(sett)

    End Function



    Public Sub ReadCheckSum()
        Flash = _CMD_FLASH_READ(0, 32768)
    End Sub

    Public Sub StartReadCheckSum()
        p_CheckSumReadPercentage = 0
        BW_CheckSumReader = New ComponentModel.BackgroundWorker With {.WorkerReportsProgress = True}
        BW_CheckSumReader.RunWorkerAsync()
    End Sub

    Function getMd5Hash(ByVal Buffer As Byte()) As String
        ' Create a new instance of the MD5 object.
        Dim md5Hasher As System.Security.Cryptography.MD5 = Security.Cryptography.MD5.Create()

        ' Convert the input string to a byte array and compute the hash.
        Dim data As Byte() = md5Hasher.ComputeHash(Buffer)

        ' Create a new Stringbuilder to collect the bytes
        ' and create a string.
        Dim sBuilder As New Text.StringBuilder()

        ' Loop through each byte of the hashed data 
        ' and format each one as a hexadecimal string.
        Dim i As Integer
        For i = 0 To data.Length - 1
            sBuilder.Append(data(i).ToString("x2"))
        Next i

        ' Return the hexadecimal string.
        Return sBuilder.ToString()
    End Function


    Function getCRC32Hash(ByVal Buffer As Byte()) As String
        ' Create a new instance of the Crc32 object.
        Dim md5Hasher As New Crc32

        ' Convert the input string to a byte array and compute the hash.
        Dim data As Byte() = md5Hasher.ComputeHash(Buffer)

        ' Create a new Stringbuilder to collect the bytes
        ' and create a string.
        Dim sBuilder As New Text.StringBuilder()

        ' Loop through each byte of the hashed data 
        ' and format each one as a hexadecimal string.
        Dim i As Integer
        For i = 0 To data.Length - 1
            sBuilder.Append(data(i).ToString("x2"))
        Next i

        ' Return the hexadecimal string.
        Return sBuilder.ToString()
    End Function













    Public Sub GetSinCosAmp(ByVal C As Double, ByVal K As Double, ByRef SinAmp As Double, ByRef CosAmp As Double)
        SinAmp = C
        CosAmp = SinAmp / K
    End Sub


#End Region



    Private Sub trg1_ValueChanged() Handles _
        trg1.ValueChanged, _
        trg2.ValueChanged, _
        trg3.ValueChanged, _
        trg4.ValueChanged, _
        trg5.ValueChanged

        IsTargetValuesChanged = True
    End Sub



    Private Sub tmr_WriteGenerationValues_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmr_WriteGenerationValues.Tick
        'If IsTargetValuesChanged = True Then
        '    Dim tCH(3) As Channel

        '    ' сколько активных целей?
        '    Dim ActiveTargetCount As Byte = 0
        '    For t = 0 To 4
        '        If p_Targets(t).Enable = True Then ActiveTargetCount += 1
        '    Next

        '    ' пропорционально раздаем 255 значений амплитуды. ослабление пока не знаем, но на всех каналах оно будет одно
        '    Dim Amps(3, 4) As Double
        '    Dim maxamp As Double = 0
        '    For t = 0 To 4
        '        ' sincos1
        '        If p_Targets(t).Direction = Target.Directn.Встречное Then
        '            GetSinCosAmp(GetAmpFromDistance(p_Targets(t).Distance, Me.Settings.p13_MW_A1, Me.Settings.p14_MW_A2, Me.Settings.p15_MW_A3), p_Settings.p8_AN_CH1_In_SCAmp, Amps(0, t), Amps(1, t))
        '            GetSinCosAmp(GetAmpFromDistance(p_Targets(t).Distance, Me.Settings.p13_MW_A1, Me.Settings.p14_MW_A2, Me.Settings.p15_MW_A3), p_Settings.p10_AN_CH2_In_SCAmp, Amps(2, t), Amps(3, t))
        '        Else
        '            GetSinCosAmp(GetAmpFromDistance(p_Targets(t).Distance, Me.Settings.p13_MW_A1, Me.Settings.p14_MW_A2, Me.Settings.p15_MW_A3), p_Settings.p4_AN_CH1_Out_SCAmp, Amps(0, t), Amps(1, t))
        '            GetSinCosAmp(GetAmpFromDistance(p_Targets(t).Distance, Me.Settings.p13_MW_A1, Me.Settings.p14_MW_A2, Me.Settings.p15_MW_A3), p_Settings.p6_AN_CH2_Out_SCAmp, Amps(2, t), Amps(3, t))
        '        End If
        '        If p_Targets(t).Enable = True Then

        '            For i = 0 To 3
        '                If maxamp < Amps(i, t) Then maxamp = Amps(i, t)
        '            Next
        '        End If
        '    Next

        '    Dim tAmp As Byte = 0
        '    Dim tAtt As Byte = 0

        '    'If ActiveTargetCount <> 0 Then
        '    If ActiveTargetCount = 0 Then ActiveTargetCount = 1
        '    If tAtt < 30 Then tAtt = 30
        '    GetAttAmp(maxamp, 255 \ ActiveTargetCount, tAtt, tAmp, False)
        '    For c = 0 To 3
        '        tCH(c) = New Channel
        '        For t = 0 To 4
        '            GetAttAmp(Amps(c, t), 255 \ ActiveTargetCount, tAtt, tAmp, True)
        '            If p_Targets(t).Enable = True Then
        '                tCH(c).Amplitude(t) = tAmp
        '            Else
        '                tCH(c).Amplitude(t) = 0
        '            End If

        '            tCH(c).Attenuation = tAtt
        '            tCH(c).Frequency(t) = p_Targets(t).Speed * p_SpeedToFreqCoeff
        '        Next
        '    Next

        '    '
        '    For t = 0 To 4
        '        Dim kkk As Integer = 1
        '        If Me.IsCalibratedService Then
        '            If p_Targets(t).Direction = Target.Directn.Встречное Then
        '                kkk = -1
        '                tCH(0).Phase(t) = 0
        '                tCH(1).Phase(t) = kkk * Me.Settings.p9_AN_CH1_In_SCPhase
        '                tCH(2).Phase(t) = 0
        '                tCH(3).Phase(t) = kkk * Me.Settings.p11_AN_CH2_In_SCPhase
        '            Else
        '                tCH(0).Phase(t) = 0
        '                tCH(1).Phase(t) = kkk * Me.Settings.p5_AN_CH1_Out_SCPhase
        '                tCH(2).Phase(t) = 0
        '                tCH(3).Phase(t) = kkk * Me.Settings.p7_AN_CH2_Out_SCPhase
        '            End If
        '        Else
        '            tCH(0).Phase(t) = 0
        '            tCH(1).Phase(t) = 0
        '            tCH(2).Phase(t) = 0
        '            tCH(3).Phase(t) = 0
        '        End If


        '        If p_Targets(t).IsPhaseManual = True Then
        '            tCH(2).Phase(t) += p_Targets(t).Phase
        '            tCH(3).Phase(t) += p_Targets(t).Phase
        '        Else
        '            Dim dph As Double = GetPhaseByDistance(p_Targets(t).Distance, p_FrequencyShiftValue)
        '            tCH(2).Phase(t) += kkk * dph
        '            tCH(3).Phase(t) += kkk * dph
        '        End If
        '    Next

        '    For c = 0 To 3
        '        SET_Channel_PROP(c, tCH(c).Amplitude, tCH(c).Frequency, tCH(c).Phase, tCH(c).Attenuation)
        '    Next
        '    'Else
        '    '    For c = 0 To 3
        '    '        SET_Channel_PROP(c, {0, 0, 0, 0, 0}, {0, 0, 0, 0, 0}, {0, 0, 0, 0, 0}, 0)
        '    '    Next
        '    'End If
        '    _CMD_GEN_START(&HFF)
        '    IsTargetValuesChanged = False
        'End If

        'If IsChannelValuesChanged = True Then


        '    For c = 0 To 3
        '        SET_Channel_PROP(c, p_Channels(c).Amplitude, p_Channels(c).Frequency, p_Channels(c).Phase, p_Channels(c).Attenuation)
        '    Next

        '    _CMD_GEN_START(&HFF)
        '    IsChannelValuesChanged = False
        'End If
    End Sub



    Private Sub UpdateTargets()
        Dim tCH(3) As Channel
        ' сколько активных целей?
        Dim ActiveTargetCount As Byte = 0
        Console.WriteLine(p_Targets(0).Phase)
        Console.WriteLine(p_Targets(0).IsPhaseManual)
        For t = 0 To 4
            If p_Targets(t).Enable = True Then ActiveTargetCount += 1
        Next
        ' пропорционально раздаем 255 значений амплитуды. ослабление пока не знаем, но на всех каналах оно будет одно
        Dim Amps(3, 4) As Double
        Dim maxamp As Double = 0
        For t = 0 To 4
            Dim tamp1 As Double = GetAmpFromDistance(p_Targets(t).Distance, Me.Settings.p13_MW_A1, Me.Settings.p14_MW_A2, Me.Settings.p15_MW_A3)

            ' sincos1
            If p_Targets(t).Direction = Target.Directn.Встречное Then
                GetSinCosAmp(tamp1, p_Settings.p8_AN_CH1_In_SCAmp, Amps(0, t), Amps(1, t))
                GetSinCosAmp(tamp1, p_Settings.p10_AN_CH2_In_SCAmp, Amps(2, t), Amps(3, t))
            Else
                GetSinCosAmp(tamp1, p_Settings.p4_AN_CH1_Out_SCAmp, Amps(0, t), Amps(1, t))
                GetSinCosAmp(tamp1, p_Settings.p6_AN_CH2_Out_SCAmp, Amps(2, t), Amps(3, t))
            End If
            If p_Targets(t).Enable = True Then

                For i = 0 To 3
                    If maxamp < Amps(i, t) Then maxamp = Amps(i, t)
                Next
            End If
        Next

        If maxamp > 255 Then
            Dim del As Double = maxamp / 255
            For t = 0 To 4
                For i = 0 To 3
                    Amps(i, t) /= del
                Next
            Next
            maxamp = 255
        End If


        Dim tAmp As Byte = 0
        Dim tAtt As Byte = 0

        'If ActiveTargetCount <> 0 Then
        If ActiveTargetCount = 0 Then ActiveTargetCount = 1
        If tAtt < 30 Then tAtt = 30

        GetAttAmp(maxamp, 255 \ ActiveTargetCount, tAtt, tAmp, False)
        For c = 0 To 3
            tCH(c) = New Channel
            For t = 0 To 4
                GetAttAmp(Amps(c, t), 255 \ ActiveTargetCount, tAtt, tAmp, True)
                If p_Targets(t).Enable = True Then
                    tCH(c).Amplitude(t) = tAmp
                Else
                    tCH(c).Amplitude(t) = 0
                End If

                tCH(c).Attenuation = tAtt
                tCH(c).Frequency(t) = p_Targets(t).Speed * p_SpeedToFreqCoeff
            Next
        Next

        '
        For t = 0 To 4
            Dim kkk As Integer = 1
            If Me.IsCalibratedService Then
                If p_Targets(t).Direction = Target.Directn.Встречное Then
                    kkk = -1
                    tCH(0).Phase(t) = 0
                    tCH(1).Phase(t) = kkk * Me.Settings.p9_AN_CH1_In_SCPhase
                    tCH(2).Phase(t) = 0
                    tCH(3).Phase(t) = kkk * Me.Settings.p11_AN_CH2_In_SCPhase
                Else
                    tCH(0).Phase(t) = 0
                    tCH(1).Phase(t) = kkk * Me.Settings.p5_AN_CH1_Out_SCPhase
                    tCH(2).Phase(t) = 0
                    tCH(3).Phase(t) = kkk * Me.Settings.p7_AN_CH2_Out_SCPhase
                End If
            Else
                tCH(0).Phase(t) = 0
                tCH(1).Phase(t) = 0
                tCH(2).Phase(t) = 0
                tCH(3).Phase(t) = 0
            End If


            If p_Targets(t).IsPhaseManual = True Then
                tCH(2).Phase(t) += p_Targets(t).Phase
                tCH(3).Phase(t) += p_Targets(t).Phase
            Else
                Dim dph As Double = GetPhaseByDistance(p_Targets(t).Distance, p_FrequencyShiftValue)
                tCH(2).Phase(t) += kkk * dph
                tCH(3).Phase(t) += kkk * dph
            End If
        Next

        For c = 0 To 3
            SET_Channel_PROP(c, tCH(c).Amplitude, tCH(c).Frequency, tCH(c).Phase, tCH(c).Attenuation)

        Next

        _CMD_GEN_START(&HFF)
    End Sub
    Dim LastChannels() As Channel = {Nothing, Nothing, Nothing, Nothing}
    Private Sub UpdateChannels()
        For c = 0 To 3
            If LastChannels(c) Is Nothing Then
                LastChannels(c) = p_Channels(c)
                SET_Channel_PROP(c, p_Channels(c).Amplitude, p_Channels(c).Frequency, p_Channels(c).Phase, p_Channels(c).Attenuation)
            Else
                If LastChannels(c) IsNot p_Channels(c) Then
                    SET_Channel_PROP(c, p_Channels(c).Amplitude, p_Channels(c).Frequency, p_Channels(c).Phase, p_Channels(c).Attenuation)
                End If
            End If
        Next
        _CMD_GEN_START(&HFF)
    End Sub


    Private Sub BW_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BW.DoWork
        Dim LastPinRequestTime As Date = Now
        Dim PinRequestPeriod As TimeSpan = TimeSpan.FromMilliseconds(3000)
        Do
            System.Threading.Thread.Sleep(20)
            If IsTargetValuesChanged = True Then
                If Not IsSpPaused Then IsTargetValuesChanged = False
                If Not IsSpPaused Then UpdateTargets()
            End If

            If IsChannelValuesChanged = True Then
                If Not IsSpPaused Then IsChannelValuesChanged = False
                If Not IsSpPaused Then UpdateChannels()
            End If

            If (Now - LastPinRequestTime) > PinRequestPeriod Then
                If (Now - LastCounterWorkTime) > TimeSpan.FromSeconds(5) Then
                    If Not IsSpPaused Then LastPinRequestTime = Now
                    If Not IsSpPaused Then
                        If Not UpdateJumpers(1) Then
                            RaiseEvent ConnectionLost()
                        End If
                    End If 
                End If
            End If

            If BW.CancellationPending Then
                Exit Sub
            End If
        Loop
    End Sub

    Private Sub Device_ConnectionLost() Handles Me.ConnectionLost
        p_IsConnected = False
    End Sub

     


    Private Sub BW_CheckSumReader_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BW_CheckSumReader.DoWork
        Dim t_Flash(32768 - 1) As Byte
        For i = 0 To 511
            If Not Me.IsConnected Then
                t_Flash = Nothing
                Exit For
            End If

            Dim perc As Double = 100 * i / 513
            BW_CheckSumReader.ReportProgress(CInt(perc))
            Dim t() As Byte = _CMD_FLASH_READ(i * 64, 64)
            If t IsNot Nothing Then
                Array.Copy(t, 0, t_Flash, i * 64, 64)
            Else
                i -= 1
            End If
        Next
        Flash = t_Flash
        BW_CheckSumReader.ReportProgress(100)
    End Sub

    Private Sub BW_CheckSumReader_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BW_CheckSumReader.ProgressChanged
        Me.CheckSumReadPercentage = e.ProgressPercentage
       
    End Sub
End Class






