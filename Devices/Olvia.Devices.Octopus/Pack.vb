Namespace Olvia.Devices.Octopus
    Public Class Pack
        Dim p_CMD As Byte
        Public Property CMD As Byte
            Get
                Return p_CMD
            End Get
            Set(ByVal value As Byte)
                p_CMD = value
            End Set
        End Property

        Dim p_IsIncoming As Boolean
        Public Property IsIncoming As Boolean
            Get
                Return p_IsIncoming
            End Get
            Set(ByVal value As Boolean)
                p_IsIncoming = value
            End Set
        End Property

        Dim p_IsService As Boolean = False
        Public Property IsService As Boolean
            Get
                Return p_IsService
            End Get
            Set(ByVal value As Boolean)
                p_IsService = value
            End Set
        End Property

        Dim p_Data As Byte()
        Public Property Data As Byte()
            Get
                Return p_Data
            End Get
            Set(ByVal value As Byte())
                p_Data = value
            End Set
        End Property

        Dim p_Address As UInteger
        Public Property Address As UInteger
            Get
                Return p_Address
            End Get
            Set(ByVal value As UInteger)
                p_Address = value
            End Set
        End Property

        Public ReadOnly Property Length As Integer
            Get
                If p_Data Is Nothing Then
                    Return 0
                Else
                    Return p_Data.Length
                End If
            End Get
        End Property

        Public ReadOnly Property CRC As Byte
            Get
                Dim tcrc As ULong = Me.CMD + Me.Length
                If Me.IsIncoming Then
                    If Me.p_IsService Then
                        tcrc += &HF1
                    Else
                        tcrc += &H1
                    End If
                Else
                    If Me.p_IsService Then
                        tcrc += &HF0
                    End If
                End If


                Dim addr() As Byte = BitConverter.GetBytes(Me.Address)
                For i = 0 To 3
                    tcrc += addr(i)
                Next
                If Me.Data IsNot Nothing Then '
                    If Me.Data.Length > 0 Then
                        For i = 0 To UBound(Me.Data)
                            tcrc += Me.Data(i)
                        Next
                    End If
                End If
                If tcrc > 255 Then
                    Do Until tcrc < 256
                        tcrc -= 256
                    Loop
                End If
                Return CByte(tcrc)
            End Get
        End Property





        Public Sub New()

        End Sub

        Public Sub New(ByVal Bytes As Byte())
            Dim pck As New Pack
            pck = FromBytes(Bytes)
            If pck Is Nothing Then
                Me.CMD = Nothing
                Me.Data = Nothing
                Me.Address = Nothing
                Me.IsIncoming = Nothing
                Me.IsService = Nothing
            Else
                Me.CMD = pck.CMD
                Me.Data = pck.Data
                Me.Address = pck.Address
                Me.IsIncoming = pck.IsIncoming
                Me.IsService = pck.IsService
            End If

        End Sub

        Public Sub New(ByVal CMD As Byte, ByVal IsIncoming As Boolean, ByVal Address As UInteger, ByVal Data As Byte())
            Me.Address = Address
            Me.IsIncoming = IsIncoming
            Me.CMD = CMD
            Me.Data = Data
        End Sub

        Public Sub New(ByVal CMD As Byte, ByVal Data As Byte())
            Me.CMD = CMD
            Me.Data = Data
        End Sub


        Public Shared Function FromBytes(ByVal bytes As Byte()) As Pack
            ' min size
            If bytes Is Nothing Then
                Return Nothing
            Else
                If bytes.Length < 10 Then Return Nothing
            End If

            Dim ret As New Pack
            Dim tmpI As Integer = 1 ' start from prefix
            ' start
            If bytes(0) <> &HDB Then Return Nothing
            ' end
            If bytes(UBound(bytes)) <> &HDC Then Return Nothing
            ' pack dir
            Dim pdir As Byte = GetNextByte(tmpI, bytes)
            Select Case pdir
                Case 0
                    ret.IsIncoming = False
                    ret.IsService = False
                Case 1
                    ret.IsIncoming = True
                    ret.IsService = False
                Case &HF0
                    ret.IsIncoming = False
                    ret.IsService = True
                Case &HF1
                    ret.IsIncoming = True
                    ret.IsService = True
            End Select

            Dim adar(3) As Byte
            For i = 0 To 3
                adar(i) = GetNextByte(tmpI, bytes)
            Next
            If BitConverter.IsLittleEndian Then Array.Reverse(adar)
            ret.Address = BitConverter.ToUInt32(adar, 0)
            ' cmd
            ret.CMD = GetNextByte(tmpI, bytes)
            ' len 
            Dim Len As Byte = GetNextByte(tmpI, bytes)
            ' data
            If Len > 0 Then
                Dim data(Len - 1) As Byte
                For i = 0 To Len - 1
                    Try
                        data(i) = GetNextByte(tmpI, bytes)
                    Catch ex As Exception
                        Return Nothing
                    End Try
                Next
                ret.Data = data
            Else
                ret.Data = Nothing
            End If

            Dim CS As Byte = GetNextByte(tmpI, bytes)
            If CS = ret.CRC Then
                Return ret
            End If
            Return Nothing
        End Function


        Private Shared Function GetNextByte(ByRef Index As Integer, ByVal bytes As Byte()) As Byte
            Dim ret As Byte = 0
            If Index >= bytes.Length Then Return 0
            If bytes(Index) = &H21 Then
                ret = _ByteStaff(bytes(Index + 1), True)
                Index += 2
            Else
                ret = bytes(Index)
                Index += 1
            End If
            Return ret
        End Function



        Public Overloads Shared Function ToBytes(ByVal Pack2Convert As Pack) As Byte()
            Dim bt() As Byte = {&HDB}
            If Pack2Convert.IsIncoming Then
                If Pack2Convert.p_IsService Then
                    AppEndToArray(bt, _ByteStaff(&HF1))
                Else
                    AppEndToArray(bt, _ByteStaff(&H1))
                End If
            Else
                If Pack2Convert.p_IsService Then
                    AppEndToArray(bt, _ByteStaff(&HF0))
                Else
                    AppEndToArray(bt, _ByteStaff(&H0))
                End If
            End If
            Dim addr() As Byte = BitConverter.GetBytes(Pack2Convert.Address)
            If BitConverter.IsLittleEndian Then Array.Reverse(addr)
            For i = 0 To 3
                AppEndToArray(bt, _ByteStaff(addr(i)))
            Next
            AppEndToArray(bt, _ByteStaff(Pack2Convert.CMD))
            AppEndToArray(bt, _ByteStaff(Pack2Convert.Length))
            If Pack2Convert.Data IsNot Nothing Then
                If Pack2Convert.Data.Length > 0 Then
                    For i = 0 To UBound(Pack2Convert.Data)
                        AppEndToArray(bt, _ByteStaff(Pack2Convert.Data(i)))
                    Next
                End If
            End If
            AppEndToArray(bt, _ByteStaff(Pack2Convert.CRC))
            AppEndToArray(bt, {&HDC})
            Return bt
        End Function

        Public Overloads Function ToBytes() As Byte()
            Return ToBytes(Me)
        End Function


        Private Shared Sub AppEndToArray(ByRef Arr As Byte(), ByVal bts As Byte())
            If Arr Is Nothing Then
                ReDim Arr(bts.Length - 1)
            Else
                ReDim Preserve Arr(UBound(Arr) + bts.Length)
            End If
            Array.Copy(bts, 0, Arr, Arr.Length - bts.Length, bts.Length)
        End Sub



        Private Overloads Shared Function _ByteStaff(ByVal ByteToConvert As Byte) As Byte()
            Dim ans() As Byte = {ByteToConvert}
            Select Case ByteToConvert
                Case &HDB
                    ReDim ans(1)
                    ans(0) = &H21
                    ans(1) = &HFA
                Case &HDC
                    ReDim ans(1)
                    ans(0) = &H21
                    ans(1) = &HFB
                Case &H21
                    ReDim ans(1)
                    ans(0) = &H21
                    ans(1) = &HFC
            End Select
            Return ans
        End Function

        Private Overloads Shared Function _ByteStaff(ByVal EscCode As Byte, ByVal isRetNothing As Boolean) As Byte
            Select Case EscCode
                Case &HFA
                    Return &HDB
                Case &HFB
                    Return &HDC
                Case &HFC
                    Return &H21
                Case Else
                    If isRetNothing = True Then
                        Return Nothing
                    Else
                        Return EscCode
                    End If
            End Select
        End Function


    End Class

End Namespace

'Public Class Pack
'#Region "Propertys"
'    Dim p_CMD As Byte
'    Public Property CMD As Byte
'        Get
'            Return p_CMD
'        End Get
'        Set(ByVal value As Byte)
'            p_CMD = value
'        End Set
'    End Property


'    Dim p_Data As Byte()
'    Public Property Data As Byte()
'        Get
'            Return p_Data
'        End Get
'        Set(ByVal value As Byte())
'            p_Data = value
'        End Set
'    End Property

'    Public ReadOnly Property Length As Integer
'        Get
'            If p_Data Is Nothing Then
'                Return 0
'            Else
'                Return p_Data.Length
'            End If
'        End Get
'    End Property


'    Dim p_Address As UInteger = 0
'    Public Property Address As UInteger
'        Get
'            Return p_Address
'        End Get
'        Set(value As UInteger)
'            p_Address = value
'        End Set
'    End Property



'    Public ReadOnly Property CRC As Byte
'        Get
'            Dim tcrc As ULong = Me.CMD + Me.Length
'            If Me.Data IsNot Nothing Then '
'                If Me.Data.Length > 0 Then
'                    For i = 0 To UBound(Me.Data)
'                        tcrc += Me.Data(i)
'                    Next
'                End If
'            End If
'            If tcrc > 255 Then
'                Do Until tcrc < 256
'                    tcrc -= 256
'                Loop
'            End If
'            Return CByte(tcrc)
'        End Get
'    End Property
'#End Region

'#Region "Constructors"
'    Public Sub New()

'    End Sub

'    Public Sub New(ByVal Bytes As Byte())
'        Dim pck As New Pack
'        pck = FromBytes(Bytes)
'        If pck Is Nothing Then
'            Me.CMD = Nothing
'            Me.Data = Nothing
'        Else
'            Me.CMD = pck.CMD
'            Me.Data = pck.Data
'        End If

'    End Sub

'    Public Sub New(ByVal CMD As Byte, ByVal Address As UInteger, ByVal Data As Byte())
'        Me.Address = Address
'        Me.CMD = CMD
'        Me.Data = Data
'    End Sub 
'#End Region



'    Public Shared Function FromBytes(ByVal bytes As Byte()) As Pack
'        min size
'        If bytes Is Nothing Then
'            Return Nothing
'        Else
'            If bytes.Length < 5 Then Return Nothing
'        End If

'        Dim ret As New Pack
'        Dim tmpI As Integer = 1 ' start from prefix
'        start()
'        If bytes(0) <> &HDB Then Return Nothing
'        End
'        If bytes(UBound(bytes)) <> &HDC Then Return Nothing
'        CMD
'        ret.CMD = GetNextByte(tmpI, bytes)
'        Len()
'        Dim Len As Byte = GetNextByte(tmpI, bytes)
'        data()
'        Dim data(Len - 1) As Byte
'        Try
'            For i = 0 To Len - 1
'                data(i) = GetNextByte(tmpI, bytes)
'            Next
'            ret.Data = data
'            CS()
'            Dim CS As Byte = GetNextByte(tmpI, bytes)
'            Dim tCS As Integer = 0
'        Catch ex As Exception
'            ret = Nothing
'        End Try

'        Return ret

'    End Function


'    Private Shared Function GetNextByte(ByRef Index As Integer, ByVal bytes As Byte()) As Byte
'        Dim ret As Byte = 0
'        If bytes(Index) = &H21 Then
'            ret = _ByteStaff(bytes(Index + 1), True)
'            Index += 2
'        Else
'            ret = bytes(Index)
'            Index += 1
'        End If
'        Return ret
'    End Function



'    Public Overloads Shared Function ToBytes(ByVal Pack2Convert As Pack) As Byte()
'        Dim bt() As Byte = {&HDB}

'        Dim ab As Byte() = AddressToByte(Pack2Convert.Address)
'        For i = 0 To 3
'            AppEndToArray(bt, _ByteStaff(ab(i)))
'        Next
'        AppEndToArray(bt, _ByteStaff(Pack2Convert.CMD))
'        AppEndToArray(bt, _ByteStaff(Pack2Convert.Length))
'        If Pack2Convert IsNot Nothing Then
'            If Pack2Convert.Length > 0 Then
'                For i = 0 To UBound(Pack2Convert.Data)
'                    AppEndToArray(bt, _ByteStaff(Pack2Convert.Data(i)))
'                Next
'            End If
'        End If
'        AppEndToArray(bt, _ByteStaff(Pack2Convert.CRC))
'        AppEndToArray(bt, {&HDC})
'        Return bt
'    End Function

'    Public Overloads Function ToBytes() As Byte()
'        Return ToBytes(Me)
'    End Function


'    Private Shared Sub AppEndToArray(ByRef Arr As Byte(), ByVal bts As Byte())
'        If Arr Is Nothing Then
'            ReDim Arr(bts.Length - 1)
'        Else
'            ReDim Preserve Arr(UBound(Arr) + bts.Length)
'        End If
'        Array.Copy(bts, 0, Arr, Arr.Length - bts.Length, bts.Length)
'    End Sub



'    Private Overloads Shared Function _ByteStaff(ByVal ByteToConvert As Byte) As Byte()
'        Dim ans() As Byte = {ByteToConvert}
'        Select Case ByteToConvert
'            Case &HDB
'                ReDim ans(1)
'                ans(0) = &H21
'                ans(1) = &HFA
'            Case &HDC
'                ReDim ans(1)
'                ans(0) = &H21
'                ans(1) = &HFB
'            Case &H21
'                ReDim ans(1)
'                ans(0) = &H21
'                ans(1) = &HFC
'        End Select
'        Return ans
'    End Function

'    Private Overloads Shared Function _ByteStaff(ByVal EscCode As Byte, ByVal isRetNothing As Boolean) As Byte
'        Select Case EscCode
'            Case &HFA
'                Return &HDB
'            Case &HFB
'                Return &HDC
'            Case &HFC
'                Return &H21
'            Case Else
'                If isRetNothing = True Then
'                    Return Nothing
'                Else
'                    Return EscCode
'                End If
'        End Select
'    End Function

'    Private Shared Function AddressToByte(ByVal Address As UInteger) As Byte()
'        Dim b As Byte() = BitConverter.GetBytes(Address)
'        If BitConverter.IsLittleEndian Then Array.Reverse(b) 
'        Return b
'    End Function

'End Class

