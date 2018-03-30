
Public Class DevSet

    Dim oct As Olvia.Devices.Octopus.Device

    Sub New(device As Olvia.Devices.Octopus.Device)
        InitializeComponent()
        oct = device
    End Sub

    Dim WithEvents bw_find As New System.ComponentModel.BackgroundWorker With {.WorkerReportsProgress = True, .WorkerSupportsCancellation = True}

    Public Found_Port As String = String.Empty

    Private Sub bt_Afound_Click(sender As Object, e As Windows.RoutedEventArgs) Handles bt_Afound.Click
        cb_Port.ItemsSource = Interfaces.Service.GetPortNamesAuto()
        Me.IsEnabled = False
        bt_Afound.Content = "Поиск..."
        bw_find.RunWorkerAsync()
    End Sub

    Private Sub cb_Port_SelectionChanged(sender As Object, e As Windows.Controls.SelectionChangedEventArgs) Handles cb_Port.SelectionChanged
        Found_Port = cb_Port.SelectedItem
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As Windows.RoutedEventArgs)
        Found_Port = cb_Port.SelectedItem
    End Sub

    Private Sub bw_find_DoWork(sender As Object, e As ComponentModel.DoWorkEventArgs) Handles bw_find.DoWork
        If (oct.Connect()) Then

            bw_find.ReportProgress(10)
        End If
    End Sub

    Private Sub bw_find_ProgressChanged(sender As Object, e As ComponentModel.ProgressChangedEventArgs) Handles bw_find.ProgressChanged
        If e.ProgressPercentage = 10 Then
            cb_Port.SelectedItem = oct.PortName
        End If

    End Sub

    Private Sub bw_find_RunWorkerCompleted(sender As Object, e As ComponentModel.RunWorkerCompletedEventArgs) Handles bw_find.RunWorkerCompleted
        oct.Disconnect()
        bt_Afound.Content = "Автопоиск"
        Me.IsEnabled = True
    End Sub

    Private Sub UserControl_Loaded(sender As Object, e As Windows.RoutedEventArgs)
        cb_Port.ItemsSource = Interfaces.Service.GetPortNamesAuto()
        cb_Port.SelectedIndex = 0
    End Sub
End Class
