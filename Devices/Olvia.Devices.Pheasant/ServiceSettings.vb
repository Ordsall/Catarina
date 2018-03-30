Public Class ServiceSettings
    Implements ICloneable

    Private bts(197) As Byte

  
    Public Shared Function FromByte(ByVal ByteArray As Byte()) As ServiceSettings
        If ByteArray Is Nothing Then Return Nothing
        If ByteArray.Length = 97 * 2 Then ReDim Preserve ByteArray(99 * 2 - 1)
        If ByteArray.Length <> 99 * 2 Then Return Nothing
        Dim ss As New ServiceSettings
        ss.bts = ByteArray
        Return ss
    End Function
    Public Function ToByte()
        Return Me.bts.Clone
    End Function

    Private Function GetPar(ByVal Index As Integer) As UInt16
        Try
            Dim ret As UInt16 = bts(Index * 2 + 1) * 256
            ret += bts(Index * 2)
            Return ret
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub SetPar(ByVal value As UInt16, ByVal Index As Integer)
        Try
            bts(Index * 2 + 1) = value \ 256
            bts(Index * 2) = value - bts(Index * 2 + 1) * 256
        Catch ex As Exception

        End Try
    End Sub


    Public Property SerialNumber As String
        Get
            Dim r1 As UInteger = GetPar(0)
            Dim r2 As UInteger = GetPar(1)
            r2 *= &H10000
            Dim r3 As UInteger = r1 + r2
            r3 = r3 << 8
            r3 = r3 >> 8
            Return r3
        End Get
        Set(ByVal value As String)
            Dim _r1 As UInteger = GetPar(0)
            Dim _r2 As UInteger = GetPar(1)
            Dim _r3 As UInteger = _r1 + _r2
            _r3 = _r3 >> 24
            _r3 = _r3 << 24
            _r3 += value

            Dim r2 As Integer = _r3 \ &H10000
            Dim r1 As Integer = _r3 - r2 * &H10000

            SetPar(r1, 0)
            SetPar(r2, 1)
        End Set
    End Property

    Enum eDeviceType
        БМ_ФАЗАН
        БМ_ОСЬМИНОГ
        ДТ_ОСЬМИНОГ
        БМ_ОСЬМИНОГ_М
        Reserved4
        Reserved5
        Reserved6
        Reserved7
        Reserved8
        Reserved9
        Reserved10
        Reserved11
        Reserved12
        Reserved13
        Reserved14
        Reserved15
    End Enum

    Public Property DeviceType() As eDeviceType
        Get
            Dim r1 As UInteger = GetPar(0)
            Dim r2 As UInteger = GetPar(1)
            r2 *= &H10000
            Dim r3 As UInteger = r1 + r2
            r3 = r3 << 4
            r3 = r3 >> 28
            Return r3
        End Get
        Set(ByVal value As eDeviceType)
            Dim _r1 As UInteger = GetPar(0)
            Dim _r2 As UInteger = GetPar(1)
            _r2 *= &H10000
            Dim _r3 As UInteger = _r1 + _r2
            _r3 = _r3 And Not &HF000000

            _r3 += value << 24


            Dim r2 As Integer = _r3 \ &H10000
            Dim r1 As Integer = _r3 - r2 * &H10000

            SetPar(r1, 0)
            SetPar(r2, 1)
        End Set
    End Property


    Enum eFrequencyRange
        Reserved0
        Reserved1
        Reserved2
        Reserved3
        Reserved4
        Reserved5
        Reserved6
        Reserved7
        Reserved8
        Reserved9
        Reserved10
        Reserved11
        Reserved12
        Reserved13
        Reserved14
        Reserved15


    End Enum
    Public Property FrequencyRange() As eFrequencyRange
        Get
            Dim r1 As UInteger = GetPar(0)
            Dim r2 As UInteger = GetPar(1)
            r2 *= &H10000
            Dim r3 As UInteger = r1 + r2
            r3 = r3 >> 28
            Return r3
        End Get
        Set(ByVal value As eFrequencyRange)
            Dim vv As ULong = value

            Dim _r1 As UInteger = GetPar(0)
            Dim _r2 As UInteger = GetPar(1)
            _r2 *= &H10000
            Dim _r3 As UInteger = _r1 + _r2
            _r3 = _r3 And Not &HF0000000

            _r3 += vv << 28


            Dim r2 As Integer = _r3 \ &H10000
            Dim tr1 As Long = Math.BigMul(r2, &H10000)

            Dim r1 As Integer = _r3 - tr1

            SetPar(r1, 0)
            SetPar(r2, 1)
        End Set
    End Property




    Public Property RadarMode As Boolean()
        Get
            Return InterpretationFrom(GetPar(2), eInterpretation.BoolAr)
        End Get
        Set(ByVal value As Boolean())
            SetPar(InterpretationTo(value, eInterpretation.BoolAr), 2)
        End Set
    End Property
    Public Property ThrAmp As Double
        Get
            Return InterpretationFrom(GetPar(3), eInterpretation.Amp)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation.Amp), 3)
        End Set
    End Property
    Public Property ThrPhaseDiff As Double
        Get
            Return InterpretationFrom(GetPar(4), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 4)
        End Set
    End Property
    Public Property Voltage0 As Double
        Get
            Return InterpretationFrom(GetPar(5), eInterpretation.Voltage)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation.Voltage), 5)
        End Set
    End Property
    Public Property Voltage1 As Double
        Get
            Return InterpretationFrom(GetPar(6), eInterpretation.Voltage)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation.Voltage), 6)
        End Set
    End Property
    Public Property SawMin As Double
        Get
            Return InterpretationFrom(GetPar(7), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 7)
        End Set
    End Property
    Public Property SawMax As Double
        Get
            Return InterpretationFrom(GetPar(8), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 8)
        End Set
    End Property
    Public Property SawStep As Double
        Get
            Return InterpretationFrom(GetPar(9), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 9)
        End Set
    End Property
    ''' <summary>
    ''' не используется
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FrequencyCntrDifference As Double
        Get
            Return InterpretationFrom(GetPar(10), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 10)
        End Set
    End Property
    Public Property FrequencySetup0 As Double
        Get
            Return InterpretationFrom(GetPar(11), eInterpretation._UShort) * 0.00043065
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value / 0.00043065, eInterpretation._UShort), 11)
        End Set
    End Property
    Public Property FrequencySetup1 As Double
        Get
            Return InterpretationFrom(GetPar(12), eInterpretation._UShort) * 0.00043065
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value / 0.00043065, eInterpretation._UShort), 12)
        End Set
    End Property
    Public Property DeltaF As Double
        Get
            Return FrequencySetup1 * 1.7555908457915084
        End Get
        Set(ByVal value As Double)
            FrequencySetup1 = value / 1.7555908457915084
        End Set
    End Property
    Public Property fc As Double
        Get
            Return FrequencySetup0 * 1.7555908457915084
        End Get
        Set(ByVal value As Double)
            FrequencySetup0 = value / 1.7555908457915084
        End Set
    End Property
    Public Property FrequencyCurrent0 As Double
        Get
            Return InterpretationFrom(GetPar(13), eInterpretation._UShort) * 0.000430561
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value / 0.000430561, eInterpretation._UShort), 13)
        End Set
    End Property
    Public Property FrequencyCurrent1 As Double
        Get
            Return InterpretationFrom(GetPar(14), eInterpretation._UShort) * 0.000430561
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value / 0.000430561, eInterpretation._UShort), 14)
        End Set
    End Property
    Public Property PhAmin As Integer()
        Get
            If bts Is Nothing Then Return Nothing
            Dim res(7) As Integer
            For i = 0 To 7
                res(i) = InterpretationFrom(GetPar(15 + i * 5), eInterpretation._Short)
            Next
            Return res
        End Get
        Set(ByVal value As Integer())
            For i = 0 To 7
                SetPar(InterpretationTo(value(i), eInterpretation._Short), 15 + i * 5)
            Next
        End Set
    End Property
    Public Property PhAmax As Integer()
        Get
            If bts Is Nothing Then Return Nothing
            Dim res(7) As Integer
            For i = 0 To 7
                res(i) = InterpretationFrom(GetPar(16 + i * 5), eInterpretation._Short)
            Next
            Return res
        End Get
        Set(ByVal value As Integer())
            For i = 0 To 7
                SetPar(InterpretationTo(value(i), eInterpretation._Short), 16 + i * 5)
            Next
        End Set
    End Property
    Public Property LaneThr As Double()
        Get
            If bts Is Nothing Then Return Nothing
            Dim res(7) As Double
            For i = 0 To 7
                res(i) = InterpretationFrom(GetPar(17 + i * 5), eInterpretation.Amp)
            Next
            Return res
        End Get
        Set(ByVal value As Double())
            For i = 0 To 7
                SetPar(InterpretationTo(value(i), eInterpretation.Amp), 17 + i * 5)
            Next
        End Set
    End Property
    Public Property PhBmin As Integer()
        Get
            If bts Is Nothing Then Return Nothing
            Dim res(7) As Integer
            For i = 0 To 7
                res(i) = InterpretationFrom(GetPar(18 + i * 5), eInterpretation._Short)
            Next
            Return res
        End Get
        Set(ByVal value As Integer())
            For i = 0 To 7
                SetPar(InterpretationTo(value(i), eInterpretation._Short), 18 + i * 5)
            Next
        End Set
    End Property
    Public Property PhBmax As Integer()
        Get
            If bts Is Nothing Then Return Nothing
            Dim res(7) As Integer
            For i = 0 To 7
                res(i) = InterpretationFrom(GetPar(19 + i * 5), eInterpretation._Short)
            Next
            Return res
        End Get
        Set(ByVal value As Integer())
            For i = 0 To 7
                SetPar(InterpretationTo(value(i), eInterpretation._Short), 19 + i * 5)
            Next
        End Set
    End Property
    Public Property Vmin As Double
        Get
            Return InterpretationFrom(GetPar(55), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 55)
        End Set
    End Property
    Public Property Vmax As Double
        Get
            Return InterpretationFrom(GetPar(56), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 56)
        End Set
    End Property
    Public Property ConvoySpeedDiff As Double
        Get

            Return InterpretationFrom(GetPar(57), eInterpretation._UShort) * 0.51178
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value / 0.51178, eInterpretation._UShort), 57)
        End Set
    End Property
    Public Property ConvoyPhADiff As Double
        Get
            Return InterpretationFrom(GetPar(58), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 58)
        End Set
    End Property
    Public Property ThrTickProlog As Double
        Get
            Return InterpretationFrom(GetPar(59), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 59)
        End Set
    End Property
    Public Property ThrTickLost As Double
        Get
            Return InterpretationFrom(GetPar(60), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 60)
        End Set
    End Property
    Public Property ThrPhaseIn As Double
        Get
            Return InterpretationFrom(GetPar(61), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 61)
        End Set
    End Property
    Public Property ThrPhaseOut As Double
        Get
            Return InterpretationFrom(GetPar(62), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 62)
        End Set
    End Property
    Public Property RadarStatus As Boolean()
        Get
            Return InterpretationFrom(GetPar(63), eInterpretation.BoolAr)
        End Get
        Set(ByVal value As Boolean())
            SetPar(InterpretationTo(value, eInterpretation.BoolAr), 63)
        End Set
    End Property
    Public Property CodecControlRegister As Double
        Get
            Return InterpretationFrom(GetPar(64), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 64)
        End Set
    End Property
    Public Property SmoothPhA_A0 As Double
        Get
            Return InterpretationFrom(GetPar(65), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 65)
        End Set
    End Property
    Public Property SmoothPhA_A1 As Double
        Get
            Return InterpretationFrom(GetPar(66), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 66)
        End Set
    End Property
    Public Property FrequencyAp As Double
        Get
            Return InterpretationFrom(GetPar(67), eInterpretation._UShort) / 16384
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 16384, eInterpretation._UShort), 67)
        End Set
    End Property
    Public Property FrequencyAm As Double
        Get
            Return InterpretationFrom(GetPar(68), eInterpretation._Short) / 16384
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 16384, eInterpretation._Short), 68)
        End Set
    End Property
    Public Property harmStep As Double
        Get
            Return InterpretationFrom(GetPar(69), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 69)
        End Set
    End Property
    Public Property harmA1 As Double
        Get
            Return InterpretationFrom(GetPar(70), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 70)
        End Set
    End Property
    Public Property harmA2 As Double
        Get
            Return InterpretationFrom(GetPar(71), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 71)
        End Set
    End Property
    Public Property harmPhase As Double
        Get
            Return InterpretationFrom(GetPar(72), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 72)
        End Set
    End Property
    Public Property GeometricSpeedCoefficient As Double
        Get
            Return InterpretationFrom(GetPar(73), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 73)
        End Set
    End Property
    Public Property Alfa As Double
        Get
            Return InterpretationFrom(GetPar(74), eInterpretation._Short) / 256
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 256, eInterpretation._Short), 74)
        End Set
    End Property
    Public Property AlfaAver As Double
        Get
            Return InterpretationFrom(GetPar(75), eInterpretation._Short) / 256
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 256, eInterpretation._Short), 75)
        End Set
    End Property
    Public Property Beta As Double
        Get
            Return InterpretationFrom(GetPar(76), eInterpretation._Short) / 256
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 256, eInterpretation._Short), 76)
        End Set
    End Property
    Public Property BetaAver As Double
        Get
            Return InterpretationFrom(GetPar(77), eInterpretation._Short) / 256
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 256, eInterpretation._Short), 77)
        End Set
    End Property
    Public Property PrefirePhAdiff As Double
        Get
            Return InterpretationFrom(GetPar(78), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 78)
        End Set
    End Property
    Public Property SmoothPhB_A0 As Double
        Get
            Return InterpretationFrom(GetPar(79), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 79)
        End Set
    End Property
    Public Property SmoothPhB_A1 As Double
        Get
            Return InterpretationFrom(GetPar(80), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 80)
        End Set
    End Property
    Public Property LengthCoeffA1 As Double
        Get
            Return InterpretationFrom(GetPar(81), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 81)
        End Set
    End Property
    Public Property LengthCoeffA0 As Double
        Get
            Return InterpretationFrom(GetPar(82), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 82)
        End Set
    End Property


    Public Property Phase1fSource As Byte()
        Get
            Try
                Dim res As Byte() = {Ph1_w1, Ph1_w2, Ph1_w3, Ph1_w4}
                Return res
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
        Set(ByVal value As Byte())
            Ph1_w1 = value(0)
            Ph1_w2 = value(1)
            Ph1_w3 = value(2)
            Ph1_w4 = value(3)
        End Set
    End Property

    Public Property Phase2fSource As Byte()
        Get
            Try
                Dim res As Byte() = {Ph2_w1, Ph2_w2, Ph2_w3, Ph2_w4}
                Return res
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
        Set(ByVal value As Byte())
            Ph2_w1 = value(0)
            Ph2_w2 = value(1)
            Ph2_w3 = value(2)
            Ph2_w4 = value(3)
        End Set
    End Property


    Public Property Ph1_w1 As Double
        Get
            Return InterpretationFrom(GetPar(83), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 83)
        End Set
    End Property
    Public Property Ph1_w2 As Double
        Get
            Return InterpretationFrom(GetPar(84), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 84)
        End Set
    End Property
    Public Property Ph1_w3 As Double
        Get
            Return InterpretationFrom(GetPar(85), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 85)
        End Set
    End Property
    Public Property Ph1_w4 As Double
        Get
            Return InterpretationFrom(GetPar(86), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 86)
        End Set
    End Property
    Public Property Ph2_w1 As Double
        Get
            Return InterpretationFrom(GetPar(87), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 87)
        End Set
    End Property
    Public Property Ph2_w2 As Double
        Get
            Return InterpretationFrom(GetPar(88), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 88)
        End Set
    End Property
    Public Property Ph2_w3 As Double
        Get
            Return InterpretationFrom(GetPar(89), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 89)
        End Set
    End Property
    Public Property Ph2_w4 As Double
        Get
            Return InterpretationFrom(GetPar(90), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 90)
        End Set
    End Property
    Public Property PhA_A0 As Double
        Get
            Return InterpretationFrom(GetPar(91), eInterpretation._Short)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._Short), 91)
        End Set
    End Property
    Public Property PhA_A1 As Double
        Get
            Return InterpretationFrom(GetPar(92), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 92)
        End Set
    End Property
    Public Property PhB_A0 As Double
        Get
            Return InterpretationFrom(GetPar(93), eInterpretation._Short)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._Short), 93)
        End Set
    End Property
    Public Property PhB_A1 As Double
        Get
            Return InterpretationFrom(GetPar(94), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 94)
        End Set
    End Property
    Public Property ConvoyPhBdiff As Double
        Get
            Return InterpretationFrom(GetPar(95), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 95)
        End Set
    End Property
    Public Property PrefirePhBdiff As Double
        Get
            Return InterpretationFrom(GetPar(96), eInterpretation._UShort)
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value, eInterpretation._UShort), 96)
        End Set
    End Property
    Public Property SmoothAmpA0 As Double
        Get
            Return InterpretationFrom(GetPar(97), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 97)
        End Set
    End Property
    Public Property SmoothAmpA1 As Double
        Get
            Return InterpretationFrom(GetPar(98), eInterpretation._UShort) / 32768
        End Get
        Set(ByVal value As Double)
            SetPar(InterpretationTo(value * 32768, eInterpretation._UShort), 98)
        End Set
    End Property

















    Public Sub RestoreDefault()
        Me.Alfa = 0
        Me.AlfaAver = 0
        Me.Beta = 0
        Me.BetaAver = 0
        Me.ConvoyPhADiff = 8
        Me.ConvoyPhBdiff = 40
        Me.ConvoySpeedDiff = 3
        Me.GeometricSpeedCoefficient = 1
        Me.harmStep = 0

        Dim tPhAmin = -150
        Dim tPhAmax = 150
        Dim tLaneThr = -40
        Dim tPhBmin = -180
        Dim tPhBmax = 180

        Me.PhAmin = New Integer() {tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin}
        Me.PhAmax = New Integer() {tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax}
        Me.LaneThr = New Double() {tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr}
        Me.PhBmin = New Integer() {tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin}
        Me.PhBmax = New Integer() {tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax}


        Me.Phase2fSource = New Byte() {0, 2, 1, 3}
        Me.Phase1fSource = New Byte() {0, 1, 2, 3}
        Me.PhA_A0 = 0
        Me.PhA_A1 = 0.833
        Me.PhB_A0 = 0
        Me.PhB_A1 = 1
        Me.PrefirePhAdiff = 4
        Me.PrefirePhBdiff = 30
        Me.RadarMode = New Boolean() {False, False, True, False, False, False, False, False, False, False, False, False, False, False, False, False}
        Me.SawMax = 0
        Me.SawMin = 0
        Me.SawStep = 0
        Me.SmoothAmpA0 = 1
        Me.SmoothAmpA1 = 0
        Me.SmoothPhA_A0 = 0.7
        Me.SmoothPhA_A1 = 0.3
        Me.SmoothPhB_A0 = 0.7
        Me.SmoothPhB_A1 = 0.3
        Me.ThrAmp = -50
        Me.ThrPhaseIn = 1
        Me.ThrPhaseOut = 1
        Me.ThrTickLost = 2
        Me.ThrTickProlog = 2
        Me.Vmax = 250
        Me.Vmin = 10
        Me.Voltage0 = -0.2
        Me.Voltage1 = -0.2
        Me.DeltaF = 0.125
        Me.FrequencyAp = 0.999939
        Me.FrequencyAm = -1.037415





    End Sub


    'Public Sub RestoreDefaultOctopopus()
    '    Me.Alfa = 0
    '    Me.AlfaAver = 0
    '    Me.Beta = 0
    '    Me.BetaAver = 0
    '    Me.ConvoyPhADiff = 8
    '    Me.ConvoyPhBdiff = 40
    '    Me.ConvoySpeedDiff = 3
    '    Me.GeometricSpeedCoefficient = 1
    '    Me.harmStep = 0

    '    Dim tPhAmin = -150
    '    Dim tPhAmax = -5
    '    Dim tLaneThr = -40
    '    Dim tPhBmin = -180
    '    Dim tPhBmax = 180

    '    Me.PhAmin = {tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin, tPhAmin}
    '    Me.PhAmax = {tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax, tPhAmax}
    '    Me.LaneThr = {tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr, tLaneThr}
    '    Me.PhBmin = {tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin, tPhBmin}
    '    Me.PhBmax = {tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax, tPhBmax}


    '    Me.Phase2fSource = {0, 2, 1, 3}
    '    Me.Phase1fSource = {1, 0, 3, 2}
    '    Me.PhA_A0 = 0
    '    Me.PhA_A1 = 0.833
    '    Me.PhB_A0 = 0
    '    Me.PhB_A1 = 1
    '    Me.PrefirePhAdiff = 4
    '    Me.PrefirePhBdiff = 30
    '    Me.RadarMode = {False, False, True, False, False, False, False, False, False, False, False, False, False, False, False, False}
    '    Me.SawMax = 0
    '    Me.SawMin = 0
    '    Me.SawStep = 0
    '    Me.SmoothAmpA0 = 1
    '    Me.SmoothAmpA1 = 0
    '    Me.SmoothPhA_A0 = 0.7
    '    Me.SmoothPhA_A1 = 0.3
    '    Me.SmoothPhB_A0 = 0.7
    '    Me.SmoothPhB_A1 = 0.3
    '    Me.ThrAmp = -50
    '    Me.ThrPhaseIn = 1
    '    Me.ThrPhaseOut = 1
    '    Me.ThrTickLost = 2
    '    Me.ThrTickProlog = 2
    '    Me.Vmax = 250
    '    Me.Vmin = 10
    '    Me.Voltage0 = -0.2
    '    Me.Voltage1 = -0.2
    '    Me.DeltaF = 0.125
    '    Me.FrequencyAp = 0.999939
    '    Me.FrequencyAm = -1.037415





    'End Sub









    Enum eInterpretation
        _UShort
        BoolAr
        Amp
        Voltage
        _Short
    End Enum

    Private Function InterpretationFrom(ByVal Val As UShort, ByVal inter As eInterpretation) As Object
        Select Case inter
            Case eInterpretation._UShort
                Return Val
            Case eInterpretation.BoolAr
                Dim ba(15) As Boolean
                For i = 0 To 15
                    ba(i) = (Val And 2 ^ i) = 2 ^ i
                Next
                Return ba
            Case eInterpretation.Amp
                Dim r As Double = 20 * Math.Log10(Val / 32768)
                Return r
            Case eInterpretation.Voltage
                Dim res As Integer = Val
                If res > 2 ^ 15 Then res -= 2 ^ 16
                Return res / 32768
            Case eInterpretation._Short
                Dim res As Integer = Val
                If res > 2 ^ 15 Then res -= 2 ^ 16
                Return res
        End Select
        Return Nothing
    End Function


    Private Function InterpretationTo(ByVal Val As Object, ByVal inter As eInterpretation) As UShort
        Select Case inter
            Case eInterpretation._UShort
                Return Val
            Case eInterpretation.BoolAr
                Dim ba() As Boolean = Val
                Dim us As UShort = 0
                For i = 0 To 15
                    If ba(i) Then us += 2 ^ i
                Next
                Return us
            Case eInterpretation.Amp
                Dim r As Double = 10 ^ (Val / 20) * 32768
                Return r
            Case eInterpretation.Voltage
                Dim res As Double = Val * 32768
                If res < 0 Then res += 2 ^ 16
                Return res
            Case eInterpretation._Short
                Dim res As Integer = Val
                If res < 0 Then res += 2 ^ 16
                Return res
        End Select
        Return Nothing
    End Function

    Public Function Clone() As Object Implements System.ICloneable.Clone
        Return ServiceSettings.FromByte(bts.Clone)
    End Function


End Class