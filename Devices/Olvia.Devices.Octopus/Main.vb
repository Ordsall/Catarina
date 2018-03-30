Imports System.Reflection

Public Class Plugin
    Implements Advanced_Measurement_Suit.Iface.IPlugin
    Implements ICloneable

    Public ReadOnly Property Author As String Implements Advanced_Measurement_Suit.Iface.IPlugin.Author
        Get
            Return "Ordsall based on RodionovAM"
        End Get
    End Property

    Public ReadOnly Property Info As String Implements Advanced_Measurement_Suit.Iface.IPlugin.Info
        Get
            Return "ДТ Осьминог"
        End Get
    End Property

    Public ReadOnly Property Name As String Implements Advanced_Measurement_Suit.Iface.IPlugin.Name
        Get
            Return "Octopus"
        End Get
    End Property

    Public ReadOnly Property Version As String Implements Advanced_Measurement_Suit.Iface.IPlugin.Version
        Get
            Return Assembly.GetExecutingAssembly().GetName().Version.ToString()
        End Get
    End Property

    Public Function Clone() As Object Implements ICloneable.Clone
        Return Me.MemberwiseClone()
    End Function

End Class


Public Class Main
    Inherits Advanced_Measurement_Suit.Iface.IDevice


    Dim WithEvents oct As Olvia.Devices.Octopus.Device
    Dim WithEvents ds As DevSet
    Dim WithEvents par_set As Interfaces.Iface.Exchange.Parameter
    Dim WithEvents ph As Olvia.Devices.Octopus.Phasantlike.Device
    Dim port As String = String.Empty

    Public Overrides ReadOnly Property ConfigurationControl As System.Windows.Controls.UserControl
        Get
            Return ds
        End Get
    End Property

    Public Overloads Overrides Function Connect() As Boolean
        Return oct.ConnectLikePhasant(port)
    End Function

    Public Overloads Overrides Function Connect(Parameters() As Object) As Boolean
        Return oct.ConnectLikePhasant(Parameters(0))
    End Function

    Public Overrides ReadOnly Property Description As String
        Get
            Return "Детектор транспорта"
        End Get
    End Property

    Public Overrides Function Disconnect() As Boolean
        oct.phoct.Disconnect()
        Return True
    End Function

    Public Overrides ReadOnly Property ID As Integer
        Get
            Return 111250
        End Get
    End Property

    Public Overrides ReadOnly Property IsConnected As Boolean
        Get
            Return oct.phoct.IsConnected
        End Get
    End Property

    Public Overrides Sub Load()
        oct = New Olvia.Devices.Octopus.Device
        ph = oct.phoct
        Base_class = ph
        ds = New DevSet(oct)
        par_set = New Interfaces.Iface.Exchange.Parameter("Имя порта", Advanced_Measurement_Suit.Iface.eSettings.COM_Port, True)
        par_set.Value = String.Empty
        Settings.Add(0, par_set)

        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_1, "Детектор 1")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_2, "Детектор 2")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_3, "Детектор 3")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_4, "Детектор 4")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_5, "Детектор 5")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_6, "Детектор 6")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_7, "Детектор 7")
        AddGroupe(Advanced_Measurement_Suit.Iface.GroupIDs.Detector_8, "Детектор 8")

        AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Spectrum, "Спектр сигнала", "GetSpectrum")
        AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Signal, "Уровень сигнала", "Signal")
        AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Noize, "Уровень шума", "Noise")
        AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Flow, "Триггер покового режима", "Flow_cntr")

        For dnum = Advanced_Measurement_Suit.Iface.GroupIDs.Detector_1 To Advanced_Measurement_Suit.Iface.GroupIDs.Detector_8
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Amplitude, "Амплитуда", "GetDetector", dnum, New Object() {dnum - 1000, 1})
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Angle, "Угловая координата", "GetDetector", dnum, New Object() {dnum - 1000, 2})
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Distance, "Расстояние", "GetDetector", dnum, New Object() {dnum - 1000, 3})
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.PhaseA, "Фаза А", "GetDetector", dnum, New Object() {dnum - 1000, 4})
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.PhaseB, "Фаза В", "GetDetector", dnum, New Object() {dnum - 1000, 5})
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.Speed, "Скорость", "GetDetector", dnum, New Object() {dnum - 1000, 6})
            AddParameter(Advanced_Measurement_Suit.Iface.ParametersIDs.DetectorState, "Статус", "GetDetector", dnum, New Object() {dnum - 1000, 7})
        Next



    End Sub

    Private Sub ph_DetectorsChanged(Detectors() As Olvia.Devices.Octopus.Phasantlike.Detector) Handles ph.DetectorsChanged
        Dim iter As Integer = 0
        Dim founder As Integer = 0
        Dim tm As System.DateTime = System.DateTime.Now
        For Each det In Detectors
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.Amplitude, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.Amp, tm)
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.Angle, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.Angle, tm)
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.Distance, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.Distance, tm)
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.PhaseA, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.PhaseA, tm)
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.PhaseB, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.PhaseB, tm)
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.Speed, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.Speed, tm)
            founder = FindRealId(Advanced_Measurement_Suit.Iface.ParametersIDs.DetectorState, iter + 1001)
            AvaibleParameters(founder).ExternSetValue(det.State, tm)
            iter = iter + 1
        Next
    End Sub

    Public Overrides ReadOnly Property Name As String
        Get
            Return "Детектор транспорта 'ОСЬМИНОГ' БКЮФ.464542.003 (протокол Фазана)"
        End Get
    End Property

    Public Overrides ReadOnly Property Version As Integer
        Get
            Return 1
        End Get
    End Property

    Private Sub par_set_ParameterUpdated(value As Object) Handles par_set.ParameterUpdated
        If (SaveParameters) Then
            port = value
        End If
    End Sub

    Private Sub ds_Unloaded(sender As Object, e As Windows.RoutedEventArgs) Handles ds.Unloaded
        Settings(0).Value = ds.Found_Port
    End Sub

    Private Sub oct_ConnectEvent(obj As Boolean) Handles oct.ConnectEvent

    End Sub

    Private Sub ph_ConnectionStateChanged(IsConnected As Boolean) Handles ph.ConnectionStateChanged
        RaiseConectionStateEvent(IsConnected)
    End Sub

    Public Overrides ReadOnly Property CalibrationControl As Windows.Controls.UserControl
        Get

        End Get
    End Property
End Class