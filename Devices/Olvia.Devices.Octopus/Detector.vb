Namespace Olvia.Devices.Octopus
    Public Class Detector

        Dim DistanceCoeff As Double = 1
        Dim PhaseCoeff As Double = 1

        Private p_State As Byte
        Public Property State() As Byte
            Get
                Return p_State
            End Get
            Set(ByVal value As Byte)
                p_State = value
            End Set
        End Property

        Private p_Speed As Double
        Public Property Speed() As Double
            Get
                Return p_Speed
            End Get
            Set(ByVal value As Double)
                p_Speed = value
            End Set
        End Property

        Private p_Distance As Double
        Public Property Distance() As Double
            Get
                Return p_Distance
            End Get
            Set(ByVal value As Double)
                p_Distance = value
                p_PhaseA = value / DistanceCoeff
            End Set
        End Property

        Private p_Angle As Double
        Public Property Angle() As Double
            Get
                Return p_Angle
            End Get
            Set(ByVal value As Double)
                p_Angle = value
                p_PhaseB = 10 * value / PhaseCoeff
                If p_Distance > 0 Then p_PhaseB *= -1
            End Set
        End Property

        Private p_PhaseA As Double
        Public Property PhaseA() As Double
            Get
                Return p_PhaseA
            End Get
            Set(ByVal value As Double)
                p_PhaseA = value
                p_Distance = value * DistanceCoeff
            End Set
        End Property

        Private p_PhaseB As Double
        Public Property PhaseB() As Double
            Get
                Return p_PhaseB
            End Get
            Set(ByVal value As Double)
                p_PhaseB = value
                p_Angle = value * PhaseCoeff / 10
            End Set
        End Property

        Dim p_Amp As Double
        Public Property Amp As Double
            Get
                Return p_Amp
            End Get
            Set(ByVal value As Double)
                p_Amp = value
            End Set
        End Property

        Public Sub New()

        End Sub
        Public Sub New(ByVal State As Byte, ByVal Speed As Double, ByVal Distance As Double, ByVal Angle As Double, ByVal DistanceCoeff As Double, ByVal PhaseCoeff As Double)
            Me.DistanceCoeff = DistanceCoeff
            Me.PhaseCoeff = PhaseCoeff
            Me.State = State
            Me.Speed = Speed
            Me.Distance = Distance
            Me.Angle = Angle
        End Sub

        Public Sub New(ByVal State As Byte, ByVal Speed As Double, ByVal PhaseA As Double, ByVal PhaseB As Double, ByVal DistanceCoeff As Double, ByVal PhaseCoeff As Double, ByVal o As Double)
            Me.DistanceCoeff = DistanceCoeff
            Me.PhaseCoeff = PhaseCoeff
            Me.State = State
            Me.Speed = Speed
            Me.PhaseA = PhaseA
            Me.PhaseB = PhaseB
        End Sub

        Public Shared Function FromPhase(ByVal State As Byte, ByVal Speed As Double, ByVal PhaseA As Double, ByVal PhaseB As Double, ByVal DistanceCoeff As Double, ByVal PhaseCoeff As Double) As Detector
            Return New Detector(State, Speed, PhaseA, PhaseB, DistanceCoeff, PhaseCoeff, Nothing)
        End Function

        Public Shared Function FromDistanceAngle(ByVal State As Byte, ByVal Speed As Double, ByVal Distance As Double, ByVal Angle As Double, ByVal DistanceCoeff As Double, ByVal PhaseCoeff As Double) As Detector
            Return New Detector(State, Speed, Distance, Angle, DistanceCoeff, PhaseCoeff)
        End Function

        Public Shared Function DetectorsFromByteArray(ByVal ByteArray As Byte()) As Detector()
            If ByteArray Is Nothing Then Return Nothing
            If ByteArray.Length = 6 * 8 + 2 Then
                Dim state(7) As Double
                Dim speed(7) As Double
                Dim distance(7) As Double
                Dim angle(7) As Double
                Dim amp(7) As Double
                Dim res(7) As Detector
                For i = 0 To 7
                    state(i) = ((ByteArray(2 + 0 + i * 6)) And &HF)
                    speed(i) = ByteArray(2 + 1 + i * 6) + ByteArray(2 + 2 + i * 6) * 256
                    speed(i) /= 32


                    distance(i) = ByteArray(2 + 3 + i * 6)
                    angle(i) = ByteArray(2 + 4 + i * 6) / 10
                    If ((ByteArray(2 + 0 + i * 6)) And &H10) = &H10 Then
                        distance(i) *= -1
                    End If

                    If ((ByteArray(2 + 0 + i * 6)) And &H20) = &H20 Then
                        angle(i) *= -1
                    End If

                    amp(i) = GetAmp(ByteArray(2 + 5 + i * 6))

                    res(i) = Detector.FromDistanceAngle(state(i), speed(i), distance(i), angle(i), 1, 1)
                    res(i).Amp = amp(i)
                Next


                Return res

            End If
            If ByteArray.Length = 5 * 8 + 2 Then
                Dim state(7) As Double
                Dim speed(7) As Double
                Dim distance(7) As Double
                Dim angle(7) As Double
                Dim amp(7) As Double
                Dim res(7) As Detector
                For i = 0 To 7
                    state(i) = ((ByteArray(2 + 0 + i * 5)) And &HF)
                    speed(i) = ByteArray(2 + 1 + i * 5)
                    distance(i) = ByteArray(2 + 2 + i * 5)
                    angle(i) = ByteArray(2 + 3 + i * 5) / 10
                    If ((ByteArray(2 + 0 + i * 5)) And &H10) = &H10 Then
                        distance(i) *= -1
                    End If

                    If ((ByteArray(2 + 0 + i * 5)) And &H20) = &H20 Then
                        angle(i) *= -1
                    End If
                    amp(i) = GetAmp(ByteArray(2 + 4 + i * 5))
                    res(i) = Detector.FromDistanceAngle(state(i), speed(i), distance(i), angle(i), 1, 1)
                Next


                Return res
            End If
            Return Nothing
        End Function
        Shared GetAmp() As Double = { _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -100, _
                         -18.06179974, _
                         -100, _
                         -100, _
                         -100, _
                         -90.3089987, _
                         -84.28839879, _
                         -78.26779887, _
                         -72.24719896, _
                         -66.22659905, _
                         -60.20599913, _
                         -54.18539922, _
                         -48.16479931, _
                         -42.14419939, _
                         -36.12359948, _
                         -30.10299957, _
                         -24.08239965, _
                         -12.04119983, _
                         -100, _
                         -100, _
                         -90.3089987, _
                         -84.28839879, _
                         -78.26779887, _
                         -72.24719896, _
                         -66.22659905, _
                         -60.20599913, _
                         -54.18539922, _
                         -48.16479931, _
                         -42.14419939, _
                         -36.12359948, _
                         -30.10299957, _
                         -24.08239965, _
                         -18.06179974, _
                         -8.519374645, _
                         -100, _
                         -100, _
                         -90.3089987, _
                         -80.7665736, _
                         -74.74597369, _
                         -68.72537378, _
                         -62.70477386, _
                         -56.68417395, _
                         -50.66357404, _
                         -44.64297413, _
                         -38.62237421, _
                         -32.6017743, _
                         -26.58117439, _
                         -20.56057447, _
                         -14.53997456, _
                         -6.020599913, _
                         -100, _
                         -90.3089987, _
                         -84.28839879, _
                         -78.26779887, _
                         -72.24719896, _
                         -66.22659905, _
                         -60.20599913, _
                         -54.18539922, _
                         -48.16479931, _
                         -42.14419939, _
                         -36.12359948, _
                         -30.10299957, _
                         -24.08239965, _
                         -18.06179974, _
                         -12.04119983, _
                         -4.082399653, _
                         -100, _
                         -90.3089987, _
                         -84.28839879, _
                         -76.32959861, _
                         -70.3089987, _
                         -64.28839879, _
                         -58.26779887, _
                         -52.24719896, _
                         -46.22659905, _
                         -40.20599913, _
                         -34.18539922, _
                         -28.16479931, _
                         -22.14419939, _
                         -16.12359948, _
                         -10.10299957, _
                         -2.498774732, _
                         -100, _
                         -90.3089987, _
                         -80.7665736, _
                         -74.74597369, _
                         -68.72537378, _
                         -62.70477386, _
                         -56.68417395, _
                         -50.66357404, _
                         -44.64297413, _
                         -38.62237421, _
                         -32.6017743, _
                         -26.58117439, _
                         -20.56057447, _
                         -14.53997456, _
                         -8.519374645, _
                         -1.15983894, _
                         -100, _
                         -90.3089987, _
                         -80.7665736, _
                         -73.4070379, _
                         -67.38643799, _
                         -61.36583807, _
                         -55.34523816, _
                         -49.32463825, _
                         -43.30403833, _
                         -37.28343842, _
                         -31.26283851, _
                         -25.24223859, _
                         -19.22163868, _
                         -13.20103877, _
                         -7.180438853}

    End Class
End Namespace