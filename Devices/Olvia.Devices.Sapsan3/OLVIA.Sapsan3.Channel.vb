Public Class Channel
    Dim p_Attenuation As Byte
    Dim p_Frequency(4) As Integer
    Dim p_Amplitude(4) As Byte
    Dim p_Phase(4) As Integer

    ''' <summary>
    ''' Общее ослабление канала
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Attenuation As Byte
        Get
            Return p_Attenuation
        End Get
        Set(ByVal rvl As Byte)
            p_Attenuation = rvl
        End Set
    End Property


    ''' <summary>
    ''' Частоты пяти составляющих генерируемого в канале сигнала
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Frequency As Integer()
        Get
            Return p_Frequency
        End Get
        Set(ByVal rvl As Integer())
            p_Frequency = rvl
        End Set
    End Property


    ''' <summary>
    ''' Амплитуды пяти составляющих генерируемого в канале сигнала
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Amplitude As Byte()
        Get
            Return p_Amplitude
        End Get
        Set(ByVal rvl As Byte())

        End Set
    End Property


    ''' <summary>
    ''' Начальные фазы пяти составляющих генерируемого в канале сигнала
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Phase As Integer()
        Get
            Return p_Phase
        End Get
        Set(ByVal rvl As Integer())
            p_Phase = rvl
        End Set
    End Property


    Sub New()

    End Sub

    'Dim p_Attenuation As Byte
    'Dim p_Frequency(4) As Integer
    'Dim p_Amplitude(4) As Byte
    'Dim p_Phase(4) As Integer
    Sub New(ByVal Attenuation As Byte, ByVal Frequency() As Integer, ByVal Amplitude() As Byte, ByVal Phase() As Integer)
        Me.Attenuation = Attenuation
        For i = 0 To 4
            Me.Frequency(i) = Frequency(i)
            Me.Amplitude(i) = Amplitude(i)
            Me.Phase(i) = Phase(i)
        Next
    End Sub

End Class
