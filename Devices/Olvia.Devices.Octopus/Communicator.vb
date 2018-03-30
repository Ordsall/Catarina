Namespace Olvia.Devices.Octopus
    Public Class Communicator
        Public Event PackReceived(ByVal Pack As Pack)
        Public WithEvents SP As New IO.Ports.SerialPort
        Dim cmd_recflag(255) As Boolean
        Dim cmd_recdata(255) As Object

        Public ReadOnly Property PortName As String
            Get
                Return SP.PortName
            End Get
        End Property

        Dim p_LastReceivedPack As Pack = Nothing
        Public ReadOnly Property LastReceivedPack As Pack
            Get
                Return p_LastReceivedPack
            End Get
        End Property
        Public Sub ClearLastReceivedPack()
            p_LastReceivedPack = Nothing
        End Sub


        Public Function WriteCMD(ByVal Pck As Pack) As Boolean
            Dim tosend As Byte() = Pck.ToBytes
            Try
                SP.DiscardInBuffer()
                SP.DiscardOutBuffer()
                SP.Write(tosend, 0, tosend.Length)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function


#Region "SP_CNTR"
        Dim SP_RData As New Queue(Of Byte)
        Dim SP_RPack As New Pack()
        Dim SP_Rarr As Byte()
        Dim SP_NowParsing As Boolean = False





        Private Sub SP_DataReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs) Handles SP.DataReceived

            Dim rb(SP.BytesToRead - 1) As Byte
            SP.Read(rb, 0, rb.Length)
            Dim tmpBT As Byte = 0
            For i = 0 To UBound(rb)
                tmpBT = rb(i)
                If tmpBT = &HDB Then
                    ReDim SP_Rarr(0)
                Else
                    If SP_Rarr Is Nothing Then
                        ReDim SP_Rarr(0)
                    Else
                        ReDim Preserve SP_Rarr(SP_Rarr.Length)
                    End If

                End If
                SP_Rarr(UBound(SP_Rarr)) = tmpBT
                If tmpBT = &HDC Then
                    SP_RPack = New Pack(SP_Rarr)
                    If SP_RPack IsNot Nothing Then RaiseEvent PackReceived(SP_RPack)
                End If
            Next







            'Try
            '    Dim rb(SP.BytesToRead - 1) As Byte
            '    SP.Read(rb, 0, rb.Length)
            '    ' fill queue
            '    If rb IsNot Nothing Then
            '        If rb.Length > 0 Then
            '            For i = 0 To UBound(rb)
            '                SP_RData.Enqueue(rb(i))
            '            Next
            '        End If
            '    End If
            'Catch ex As Exception

            'End Try
            'If Not SP_NowParsing Then SP_Parsing() 
        End Sub





        Private Sub SP_Parsing()
            ' Parsing
            SP_NowParsing = True
            Dim tmpBT As Byte = 0
            If SP_RData.Count > 0 Then
                Do While SP_RData.Count > 0
                    tmpBT = SP_RData.Dequeue
                    If tmpBT = &HDB Then
                        ReDim SP_Rarr(0)
                    Else
                        If SP_Rarr Is Nothing Then
                            ReDim SP_Rarr(0)
                        Else
                            ReDim Preserve SP_Rarr(SP_Rarr.Length)
                        End If

                    End If
                    SP_Rarr(UBound(SP_Rarr)) = tmpBT
                    If tmpBT = &HDC Then
                        SP_RPack = New Pack(SP_Rarr)
                        If SP_RPack IsNot Nothing Then RaiseEvent PackReceived(SP_RPack)
                    End If
                Loop
            End If
            SP_NowParsing = False
        End Sub



#End Region


        Public Function Open(ByVal PortName As String, ByVal BaudRate As Integer) As Boolean
            Try
                If SP.IsOpen Then SP.Close()
                SP.PortName = PortName
                SP.BaudRate = BaudRate
                SP.Open()
                SP.DiscardInBuffer()
                SP.DiscardOutBuffer()
            Catch ex As Exception

            End Try
            Return SP.IsOpen
        End Function


        Public Function Close()
            Try
                SP.Close()
            Catch ex As Exception

            End Try
            Return True
        End Function

#Region "CMD"
        Public Function CMD_PING(ByVal Address As UInteger, ByRef IDinfo As String) As Boolean
            Dim CMD_ID As Byte = 1
            Dim CMD_DATA As Byte() = Nothing
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(500)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            If Not WriteCMD(New Pack(CMD_ID, False, Address, CMD_DATA)) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop
            If cmd_recflag(CMD_ID) Then
                If cmd_recdata(CMD_ID) = Nothing Then Return False
                IDinfo = CStr(cmd_recdata(CMD_ID))
                Return True
            Else
                Return False
            End If
        End Function

        Public Function CMD_RESET(ByVal Address As UInteger) As Boolean
            Dim CMD_ID As Byte = 2
            Dim CMD_DATA As Byte() = Nothing
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(500)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            If Not WriteCMD(New Pack(CMD_ID, False, Address, CMD_DATA)) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop
            Return cmd_recflag(CMD_ID)
        End Function

        Public Function CMD_GET_PAR(ByVal Address As UInteger, ByVal ParInd As Byte, ByRef ParValue As Byte()) As Boolean
            Dim CMD_ID As Byte = 3
            Dim CMD_DATA As Byte() = {ParInd}
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(500)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            If Not WriteCMD(New Pack(CMD_ID, False, Address, CMD_DATA)) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop

            If cmd_recflag(CMD_ID) Then
                ParValue = cmd_recdata(CMD_ID)
                Return True
            Else
                Return False
            End If
        End Function

        Public Function CMD_SET_PAR(ByVal Address As UInteger, ByVal ParInd As Byte, ByVal ParValue As Byte()) As Boolean
            Dim CMD_ID As Byte = 4
            Dim CMD_DATA(ParValue.Length) As Byte
            CMD_DATA(0) = ParInd
            For i = 0 To UBound(ParValue)
                CMD_DATA(i + 1) = ParValue(i)
            Next
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(2000)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            If Not WriteCMD(New Pack(CMD_ID, False, Address, CMD_DATA)) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop
            If cmd_recflag(CMD_ID) Then
                If cmd_recdata(CMD_ID) IsNot Nothing Then
                    Dim tar() As Byte = CType(cmd_recdata(CMD_ID), Byte())
                    If tar.Length = cmd_recdata(CMD_ID).Length Then
                        Dim b As Boolean = True
                        For i = 0 To UBound(tar)
                            If tar(i) <> ParValue(i) Then b = False
                        Next
                        Return b
                    End If
                End If
                Return False
            Else
                Return False
            End If
        End Function



        Public Function CMD_GET_SERV_PAR(ByVal Address As UInteger, ByVal ParInd As Byte, ByRef ParValue As Byte()) As Boolean
            Dim CMD_ID As Byte = &HA
            Dim CMD_DATA As Byte() = {ParInd}
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(1000)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            Dim pc As New Pack(CMD_ID, False, Address, CMD_DATA)
            pc.IsService = True
            If Not WriteCMD(pc) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop

            If cmd_recflag(CMD_ID) Then
                ParValue = cmd_recdata(CMD_ID)
                Return True
            Else
                Return False
            End If
        End Function

        Public Function CMD_SET_SERV_PAR(ByVal Address As UInteger, ByVal ParInd As Byte, ByVal ParValue As Byte()) As Boolean
            Dim CMD_ID As Byte = &HB
            Dim CMD_DATA(ParValue.Length) As Byte
            CMD_DATA(0) = ParInd
            For i = 0 To UBound(ParValue)
                CMD_DATA(i + 1) = ParValue(i)
            Next
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(2000)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            Dim pc As New Pack(CMD_ID, False, Address, CMD_DATA)
            pc.IsService = True
            If Not WriteCMD(pc) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop
            If cmd_recflag(CMD_ID) Then
                If cmd_recdata(CMD_ID) IsNot Nothing Then
                    Dim tar() As Byte = CType(cmd_recdata(CMD_ID), Byte())
                    If tar.Length = cmd_recdata(CMD_ID).Length Then
                        Dim b As Boolean = True
                        For i = 0 To UBound(tar)
                            If tar(i) <> ParValue(i) Then b = False
                        Next
                        Return b
                    End If
                End If
                Return False
            Else
                Return False
            End If
        End Function

        Public Function CMD_GET_ROAD_SITUATION(ByVal Address As UInteger, ByVal ZoneInd As Byte) As Boolean
            Dim CMD_ID As Byte = 5
            Dim CMD_DATA As Byte() = {ZoneInd}
            Dim wt As TimeSpan = TimeSpan.FromMilliseconds(500)
            Dim tm As Date = Now
            cmd_recflag(CMD_ID) = False
            cmd_recdata(CMD_ID) = Nothing
            If Not WriteCMD(New Pack(CMD_ID, False, Address, CMD_DATA)) Then Return False
            Do Until cmd_recflag(CMD_ID) Or ((Now - tm) > wt)
                System.Threading.Thread.Sleep(1)
            Loop
            If cmd_recflag(CMD_ID) Then


                Return True
            Else
                Return False
            End If
        End Function


#End Region









        Private Sub Comunicator_PackReceived(ByVal Pack As Pack) Handles Me.PackReceived
            '  Console.WriteLine(Pack.CMD)
            Dim IsData As Boolean = Pack.Data IsNot Nothing
            If IsData Then IsData = Pack.Data.Length > 0
            If Pack.IsService Then
                Select Case Pack.CMD
                    Case &HA
                        If IsData Then
                            cmd_recdata(Pack.CMD) = Pack.Data
                            cmd_recflag(Pack.CMD) = True
                        End If
                    Case &HB
                        If IsData Then
                            cmd_recdata(Pack.CMD) = Pack.Data
                            cmd_recflag(Pack.CMD) = True
                        End If
                End Select

            Else
                Select Case Pack.CMD
                    Case 1
                        If IsData Then
                            Dim ts As String = ""
                            For i = 0 To UBound(Pack.Data)
                                ts &= Chr(Pack.Data(i))
                            Next
                            cmd_recdata(Pack.CMD) = ts
                            cmd_recflag(Pack.CMD) = True
                        End If
                    Case 2
                        If Not IsData Then cmd_recflag(Pack.CMD) = True
                    Case 3
                        If IsData Then
                            cmd_recdata(Pack.CMD) = Pack.Data
                            cmd_recflag(Pack.CMD) = True
                        End If
                    Case 4
                        If IsData Then
                            If Pack.Data.Length <> 50 Then
                                cmd_recdata(Pack.CMD) = Pack.Data
                                cmd_recflag(Pack.CMD) = True
                            End If
                        End If
                    Case 5
                        If IsData Then
                            cmd_recdata(Pack.CMD) = Pack.Data
                            cmd_recflag(Pack.CMD) = True
                        End If




                End Select
            End If


        End Sub

        Private Sub SP_ErrorReceived(ByVal sender As Object, ByVal e As System.IO.Ports.SerialErrorReceivedEventArgs) Handles SP.ErrorReceived

        End Sub
    End Class

End Namespace