Public Class Target



    Public Event ValueChanged()


    ''' <summary>
    ''' Перечисление. Направление движения цели
    ''' </summary>
    ''' <remarks></remarks>
    Enum Directn
        Встречное
        Попутное
    End Enum


    Dim p_Direction As Directn = Directn.Встречное
    Dim p_Speed As Double = 60
    Dim p_Speed2Frequency As Double = 44.753
    Dim p_Frequency As Double = 60 * p_Speed2Frequency
    Dim p_IsSpeedInFreq As Boolean = False
    Dim p_Distance As Double = 1000
    Dim p_Phase As Double = 0
    Dim p_IsPhaseManual As Boolean = False
    Dim p_Enable As Boolean = False

    ''' <summary>
    ''' Направление движения цели
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Direction As Directn
        Get
            Return p_Direction
        End Get
        Set(ByVal rvl As Directn)
            p_Direction = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property

    ''' <summary>
    ''' Скорость движения цели. Задается в км/ч
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Speed As Double
        Get
            Return p_Speed
        End Get
        Set(ByVal rvl As Double)
            If Not p_IsSpeedInFreq Then
                p_Frequency = rvl * p_Speed2Frequency
            End If
            p_Speed = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property

    Public Property Speed2Frequency As Double
        Get
            Return p_Speed2Frequency
        End Get
        Set(ByVal value As Double)
            p_Speed2Frequency = value
            RaiseEvent ValueChanged()
        End Set
    End Property

    ''' <summary>
    ''' Частота отраженного от цели сигнала
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Frequency As Double
        Get
            Return p_Frequency
        End Get
        Set(ByVal rvl As Double)
            If p_IsSpeedInFreq Then
                p_Speed = rvl / p_Speed2Frequency
            End If
            p_Frequency = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property

    ''' <summary>
    ''' Указывает на способ задания скорости движения цели
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsSpeedInFreq As Boolean
        Get
            Return p_IsSpeedInFreq
        End Get
        Set(ByVal rvl As Boolean)
            p_IsSpeedInFreq = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property

    ''' <summary>
    ''' Расстояние до цели. Задается в м
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Distance As Double
        Get
            Return p_Distance
        End Get
        Set(ByVal rvl As Double)
            p_Distance = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property


    ''' <summary>
    ''' Фазовая разность манипулируемых сигналов. Используется только при p_IsPhaseManual = True
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Phase As Double
        Get
            Return p_Phase
        End Get
        Set(ByVal rvl As Double)
            p_Phase = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property

    ''' <summary>
    ''' Определяет способ задания фазовой разности
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsPhaseManual As Boolean
        Get
            Return p_IsPhaseManual
        End Get
        Set(ByVal rvl As Boolean)
            p_IsPhaseManual = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property

    ''' <summary>
    ''' Получает ил задает значение, показывающее активность цели
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Enable As Boolean
        Get
            Return p_Enable
        End Get
        Set(ByVal rvl As Boolean)
            p_Enable = rvl
            RaiseEvent ValueChanged()
        End Set
    End Property


End Class


