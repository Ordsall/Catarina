Namespace Olvia.Devices.Octopus
    Public Class Device

        Dim WithEvents COM As New Communicator

        Public WithEvents phoct As New Olvia.Devices.Octopus.Phasantlike.Device

#Region "Propertys"

        Public Property p_IsConnected As Boolean
            Get
                Return pp_IsConnected
            End Get
            Set(value As Boolean)
                RaiseEvent ConnectEvent(value)
                pp_IsConnected = value
            End Set
        End Property

        Event ConnectEvent As Action(Of Boolean)

        Dim pp_IsConnected As Boolean = False
        Public ReadOnly Property IsConnected As Boolean
            Get
                Return p_IsConnected
            End Get
        End Property

        Dim p_IDString As String = ""

        Public ReadOnly Property PortName As String
            Get
                If Not Me.IsConnected Then Return Nothing
                Return COM.PortName
            End Get
        End Property

        Dim p_Address As UInteger = 0
        Public ReadOnly Property Address As UInteger
            Get
                Return p_Address
            End Get
        End Property

#Region "UserParametrs"

        Enum eUserParam
            SpeedLimit
            Borders
            Height
            Slope
            Direction
            Mode
        End Enum


        Dim p_SpeedLimit As Byte = 0
        Public Property SpeedLimit As Byte
            Get
                Return p_SpeedLimit
            End Get
            Set(ByVal value As Byte)
                p_SpeedLimit = value
            End Set
        End Property

        Public ReadOnly Property ZoneCount As Integer
            Get
                Try
                    Return p_Borders.Length / 2
                Catch ex As Exception
                    Return 0
                End Try
            End Get
        End Property

        Dim p_Borders As Double() = Nothing
        Public Property Borders As Double()
            Get
                Return p_Borders
            End Get
            Set(ByVal value As Double())
                p_Borders = value
            End Set
        End Property

        Dim p_Height As Double = 0
        Public Property Height As Double
            Get
                Return p_Height
            End Get
            Set(ByVal value As Double)
                p_Height = value
            End Set
        End Property
        Dim p_Slope As Double = 0
        Public Property Slope As Double
            Get
                Return p_Slope
            End Get
            Set(ByVal value As Double)
                p_Slope = value
            End Set
        End Property

        Enum eDir
            none
            Both
            Incoming
            Outcoming
        End Enum
        Dim p_Direction As eDir = eDir.none
        Public Property Direction As eDir
            Get
                Return p_Direction
            End Get
            Set(ByVal value As eDir)
                p_Direction = value
            End Set
        End Property

        Enum eMode
            none
            Settings
            MeasPassive
            MeasActiv
            Service
            Flow
            UnFlow
        End Enum
        Dim p_Mode As eMode = eMode.none
        Public Property Mode As eMode
            Get
                Return p_Mode
            End Get
            Set(ByVal value As eMode)
                p_Mode = value
            End Set
        End Property

#End Region


#Region "ServiceParametrs"
        Enum eServiceParam
            Address
        End Enum

        Structure sServiceParam
            Public Address As UInteger


        End Structure


        Private p_ServiceParametrs As sServiceParam = New sServiceParam With {.Address = 0}
        Public Property ServiceParametrs() As sServiceParam
            Get
                Return p_ServiceParametrs
            End Get
            Set(ByVal value As sServiceParam)
                p_ServiceParametrs = value
            End Set
        End Property
#End Region



#End Region

#Region "Events"
        Public Event NewTarget(ByVal Target As TargetData)
        Public Event NewKVpack(ByVal Detectors As Olvia.Devices.Octopus.Detector())



#End Region



#Region "AMS_Func"

        Public Function ConnectLikePhasant(ByVal Port As String) As Boolean
            If Connect(Port) Then
                SwitchFromPheasant()
                ReBoot()
                SwitchToPheasant()
                Disconnect()
                Return phoct.Connect(Port)
            Else
                Disconnect()
                Return phoct.Connect(Port)
            End If
            Return False
        End Function

        Public Function ConnectLikePhasant() As Boolean
            Dim portc As String = String.Empty
            If Connect() Then
                portc = PortName
                SwitchToPheasant()
                Disconnect()
                Return phoct.Connect(portc)
            End If
            Return False
        End Function



#End Region


        Dim Targets() As ComponentModel.BindingList(Of TargetData) = {New ComponentModel.BindingList(Of TargetData), _
                                                                      New ComponentModel.BindingList(Of TargetData), _
                                                                      New ComponentModel.BindingList(Of TargetData), _
                                                                      New ComponentModel.BindingList(Of TargetData)}
        'Dim Targets() As List(Of TargetData) = {New List(Of TargetData), New List(Of TargetData), New List(Of TargetData), New List(Of TargetData)}

        '#Region "Phasant_Flow"


        '#End Region

        Public Overloads Function Connect() As Boolean
            For Each sp As String In My.Computer.Ports.SerialPortNames
                If Me.Connect(sp) Then Return True
            Next
            Return False
        End Function


        Public Sub SwitchToPheasant()
            Me.Mode = eMode.Flow
            For i = 0 To 10
                If Me.WriteUserParametr(eUserParam.Mode) Then Exit For
                System.Threading.Thread.Sleep(1000)
            Next
            COM.Close()
        End Sub

        Public Sub SwitchFromPheasant()
            COM.Open(COM.PortName, COM.SP.BaudRate)
            Me.Mode = eMode.UnFlow
            For i = 0 To 5
                If Me.WriteUserParametr(eUserParam.Mode) Then Exit For
                System.Threading.Thread.Sleep(1000)
            Next
            Me.Mode = eMode.Settings
            For i = 0 To 5
                If Me.WriteUserParametr(eUserParam.Mode) Then Exit For
                System.Threading.Thread.Sleep(1000)
            Next
        End Sub


        Public Overloads Function Connect(ByVal PortName As String) As Boolean
            Return Me.Connect(PortName, 0)
        End Function

        Public Overloads Function Connect(ByVal PortName As String, ByVal Address As UInteger) As Boolean
            If Me.IsConnected Then
                If PortName = Me.PortName And Address = Me.Address Then
                    Return True
                Else
                    Me.Disconnect()
                    Return Me.Connect(PortName, Address)
                End If
            Else
                COM.Close()
                If Not COM.Open(PortName, 115200) Then Return False
                p_Address = Address
                Me.Mode = eMode.UnFlow
                Me.WriteUserParametr(eUserParam.Mode)
                System.Threading.Thread.Sleep(1000)
                Me.Mode = eMode.Settings
                If Me.WriteUserParametr(eUserParam.Mode) Then
                    System.Threading.Thread.Sleep(1000)
                    If COM.CMD_PING(Address, p_IDString) Then
                        p_IsConnected = True
                        Return True
                    End If
                End If
                COM.Close()
            End If
            Return Me.IsConnected
        End Function

        Public Sub Disconnect()
            p_IDString = ""
            p_IsConnected = False
            COM.Close()
        End Sub


        Public Function ReBoot()
            Return COM.CMD_RESET(p_Address)
        End Function


        Public Function ReadUserParametr(ByVal Param As eUserParam) As Boolean
            Dim pv() As Byte = Nothing
            Select Case Param
                Case eUserParam.SpeedLimit
                    If COM.CMD_GET_PAR(p_Address, 1, pv) Then
                        p_SpeedLimit = pv(0)
                        Return True
                    End If
                Case eUserParam.Borders
                    If COM.CMD_GET_PAR(p_Address, 2, pv) Then
                        ReDim p_Borders(UBound(pv))
                        For i = 0 To UBound(pv)
                            p_Borders(i) = pv(i)
                            If p_Borders(i) > 127 Then
                                p_Borders(i) -= 256
                            End If
                            p_Borders(i) /= 10
                        Next
                        Return True
                    End If
                Case eUserParam.Height
                    If COM.CMD_GET_PAR(p_Address, 3, pv) Then
                        p_Height = pv(0) / 10
                        Return True
                    End If
                Case eUserParam.Slope
                    If COM.CMD_GET_PAR(p_Address, 4, pv) Then
                        p_Slope = pv(0) / 10
                        Return True
                    End If
                Case eUserParam.Direction
                    If COM.CMD_GET_PAR(p_Address, 5, pv) Then
                        Select Case pv(0)
                            Case 0
                                p_Direction = eDir.Both
                            Case 1
                                p_Direction = eDir.Outcoming
                            Case 2
                                p_Direction = eDir.Incoming
                            Case Else
                                p_Direction = eDir.none
                        End Select
                        Return True
                    End If
                Case eUserParam.Mode
                    If COM.CMD_GET_PAR(p_Address, 6, pv) Then
                        Select Case pv(0)
                            Case 0
                                p_Mode = eMode.Settings
                            Case 1
                                p_Mode = eMode.MeasPassive
                            Case 2
                                p_Mode = eMode.MeasActiv
                            Case 3
                                p_Mode = eMode.Service
                            Case Else
                                p_Mode = eMode.none
                        End Select
                        Return True
                    End If
            End Select
            Return False
        End Function

        Public Function WriteUserParametr(ByVal Param As eUserParam) As Boolean
            Dim ind As Byte = 0
            Dim val As Byte()
            Select Case Param
                Case eUserParam.SpeedLimit
                    ind = 1
                    val = {p_SpeedLimit}
                Case eUserParam.Borders
                    ind = 2
                    ReDim val(UBound(p_Borders))
                    For i = 0 To UBound(val)
                        Dim brd As Integer = p_Borders(i) * 10
                        If brd < 0 Then brd += 256
                        val(i) = brd
                    Next
                Case eUserParam.Height
                    ind = 3
                    val = {p_Height * 10}
                Case eUserParam.Slope
                    ind = 4
                    val = {p_Slope * 10}
                Case eUserParam.Direction
                    ind = 5
                    Select Case p_Direction
                        Case eDir.Both
                            val = {0}
                        Case eDir.Outcoming
                            val = {1}
                        Case eDir.Incoming
                            val = {2}
                        Case Else
                            val = {0}
                    End Select
                Case eUserParam.Mode
                    ind = 6
                    Select Case p_Mode
                        Case eMode.Settings
                            val = {0}
                        Case eMode.MeasPassive
                            val = {1}
                        Case eMode.MeasActiv
                            val = {2}
                        Case eMode.Service
                            val = {3}
                        Case eMode.UnFlow
                            val = {&HFE}
                        Case eMode.Flow
                            val = {&HFF}
                        Case Else
                            val = {0}
                    End Select
                Case Else
                    Return False
            End Select
            Return COM.CMD_SET_PAR(p_Address, ind, val)
        End Function

        Public Function ReadServiceParametr(ByVal Param As eServiceParam) As Boolean
            Dim pv() As Byte = Nothing
            Select Case Param
                Case eServiceParam.Address
                    If COM.CMD_GET_SERV_PAR(p_Address, 1, pv) Then
                        If pv.Length = 4 Then
                            If BitConverter.IsLittleEndian Then Array.Reverse(pv)
                            p_ServiceParametrs.Address = BitConverter.ToUInt32(pv, 0)
                            Return True
                        End If
                    End If
            End Select
            Return False
        End Function

        Public Function WriteServiceParametr(ByVal Param As eServiceParam) As Boolean
            Dim ind As Byte = 0
            Dim val As Byte()
            Select Case Param
                Case eServiceParam.Address
                    Dim addr() As Byte = BitConverter.GetBytes(p_ServiceParametrs.Address)
                    If BitConverter.IsLittleEndian Then Array.Reverse(addr)
                    ind = 1
                    val = addr
                Case Else
                    Return False
            End Select
            Return COM.CMD_SET_SERV_PAR(p_Address, ind, val)
        End Function

        Private Sub COM_PackReceived(ByVal Pack As Pack) Handles COM.PackReceived
            p_Address = Pack.Address

            If Pack.CMD = 5 Then
                If Pack.Data.Length = 21 Then

                    Dim zone As Byte = Pack.Data(0)
                    Dim zI As Byte = zone - 1
                    For i = 0 To 3
                        Dim tsp As Double = Pack.Data(2 + i * 5)
                        Dim tdr As TargetData.TargetDirection
                        tsp += Pack.Data(3 + i * 5) * 256
                        If tsp > 2 ^ 15 Then tsp -= 2 ^ 16
                        tsp /= 10
                        If tsp = 0 Then
                            tdr = TargetData.TargetDirection.none
                        ElseIf tsp > 0 Then
                            tdr = TargetData.TargetDirection.Outcoming
                        Else
                            tdr = TargetData.TargetDirection.Incoming
                        End If
                        tsp = Math.Abs(tsp)

                        Dim tt As New TargetData(Pack.Data(0), _
                                            Pack.Data(1 + i * 5), _
                                            tsp, _
                                            tdr, _
                                            Pack.Data(4 + i * 5), _
                                            Pack.Data(5 + i * 5))

                        'Dim tt As New TargetData(Pack.Data(0), _
                        '                    Pack.Data(1 + i * 5), _
                        '                    tsp, _
                        '                    tdr  ,_  
                        '                    Pack.Data(4 + i * 5), _
                        '                    Pack.Data(5 + i * 5))

                        If tt.Speed <> 0 Then
                            If Me.Targets(zI).Count = 0 Then
                                Me.Targets(zI).Add(tt)
                                RaiseEvent NewTarget(tt)
                            Else
                                Dim tI As Integer = Me.Targets(zI).Count - 1
                                Dim tb As Boolean = True

                                For j = 0 To tI

                                    If Me.Targets(zI)(tI - j).Counter = tt.Counter Then
                                        tb = False
                                        Exit For
                                    End If
                                Next

                                If tb Then
                                    Me.Targets(zI).Add(tt)
                                    If Me.Targets(zI).Count > 8 Then
                                        Do Until Me.Targets(zI).Count = 8
                                            Me.Targets(zI).RemoveAt(0)
                                        Loop
                                    End If


                                    RaiseEvent NewTarget(tt)
                                End If
                            End If
                        End If
                    Next
                End If
            End If

            If Pack.CMD = 4 Then
                If Pack.Data.Length = 50 Then

                    RaiseEvent NewKVpack(Olvia.Devices.Octopus.Detector.DetectorsFromByteArray(Pack.Data))

                End If
            End If



        End Sub

        Public Sub simulate_NewKVpack(ByVal Detectors As Olvia.Devices.Octopus.Detector())
            RaiseEvent NewKVpack(Detectors)
        End Sub

        Public Sub simulate_NewTarget(ByVal Target As TargetData)
            RaiseEvent NewTarget(Target)
        End Sub


        Dim GetAmp() As Double = { _
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