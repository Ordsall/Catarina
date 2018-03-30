Namespace Olvia.Devices.Octopus.Phasantlike
    Public Class pack
        Enum eDir
            FromHost
            FromModule
        End Enum
        Public Property Direction As eDir = eDir.FromHost
        Public Property CMD As Byte = 0
        Public Property ADDRESS As Byte = 0
        Public ReadOnly Property LEN As Byte
            Get
                If Me.DATA Is Nothing Then Return 0
                Return DATA.Length \ 2
            End Get
        End Property
        Public Property DATA As Byte()
        Public ReadOnly Property CRC As Byte
            Get
                Dim _CRC As Integer = &H21
                If Me.Direction = eDir.FromModule Then _CRC += 1
                _CRC += Me.CMD
                _CRC += Me.LEN
                If Me.DATA IsNot Nothing Then
                    If Me.DATA.Length > 0 Then
                        For i = 0 To UBound(Me.DATA)
                            _CRC += Me.DATA(i)
                        Next
                    End If
                End If

                If _CRC > 255 Then
                    Do Until _CRC < 256
                        _CRC -= 256
                    Loop
                End If

                Return CByte(_CRC)
            End Get
        End Property

        Sub New()

        End Sub

        Sub New(ByVal Direction As eDir, ByVal CMD As Byte, ByVal DATA As Byte())
            Me.Direction = Direction
            Me.CMD = CMD
            Me.DATA = DATA
        End Sub


        Public Function ToByte() As Byte()
            Dim bts(Me.LEN * 2 + 4) As Byte
            bts(0) = &H21
            If Me.Direction = eDir.FromModule Then bts(0) += 1
            bts(1) = Me.ADDRESS
            bts(2) = Me.CMD
            bts(3) = Me.LEN
            If Me.LEN > 0 Then
                For i = 0 To Me.LEN * 2 - 1
                    bts(4 + i) = Me.DATA(i)
                Next
            End If
            bts(UBound(bts)) = Me.CRC
            Return bts
        End Function

    End Class
End Namespace