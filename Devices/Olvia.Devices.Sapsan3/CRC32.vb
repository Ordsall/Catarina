Imports System.Security.Cryptography

Public Class Crc32
    Inherits HashAlgorithm
    Public Const DefaultPolynomial As UInt32 = &HEDB88320UI
    Public Const DefaultSeed As UInt32 = &HFFFFFFFFUI

    Private hash32 As UInt32
    Private seed As UInt32
    Private table As UInt32()
    Private Shared defaultTable As UInt32()

    Public Sub New()
        table = InitializeTable(DefaultPolynomial)
        seed = DefaultSeed
        Initialize()
    End Sub

    Public Sub New(ByVal polynomial As UInt32, ByVal seed As UInt32)
        table = InitializeTable(polynomial)
        Me.seed = seed
        Initialize()
    End Sub

    Public Overrides Sub Initialize()
        hash32 = seed
    End Sub

    Protected Overrides Sub HashCore(ByVal buffer As Byte(), ByVal start As Integer, ByVal length As Integer)
        hash32 = CalculateHash(table, hash32, buffer, start, length)
    End Sub

    Protected Overrides Function HashFinal() As Byte()
        Dim hashBuffer As Byte() = UInt32ToBigEndianBytes(Not hash32)
        Me.HashValue = hashBuffer
        Return hashBuffer
    End Function

    Public Overrides ReadOnly Property HashSize() As Integer
        Get
            Return 32
        End Get
    End Property

    Public Shared Function Compute(ByVal buffer As Byte()) As UInt32
        Return Not CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length)
    End Function

    Public Shared Function Compute(ByVal seed As UInt32, ByVal buffer As Byte()) As UInt32
        Return Not CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length)
    End Function

    Public Shared Function Compute(ByVal polynomial As UInt32, ByVal seed As UInt32, ByVal buffer As Byte()) As UInt32
        Return Not CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length)
    End Function

    Private Shared Function InitializeTable(ByVal polynomial As UInt32) As UInt32()
        If polynomial = DefaultPolynomial AndAlso defaultTable IsNot Nothing Then
            Return defaultTable
        End If

        Dim createTable As UInt32() = New UInt32(255) {}
        For i As Integer = 0 To 255
            Dim entry As UInt32 = CType(i, UInt32)
            For j As Integer = 0 To 7
                If (entry And 1) = 1 Then
                    entry = (entry >> 1) Xor polynomial
                Else
                    entry = entry >> 1
                End If
            Next
            createTable(i) = entry
        Next

        If polynomial = DefaultPolynomial Then
            defaultTable = createTable
        End If

        Return createTable
    End Function

    Private Shared Function CalculateHash(ByVal table As UInt32(), ByVal seed As UInt32, ByVal buffer As Byte(), ByVal start As Integer, ByVal size As Integer) As UInt32
        Dim crc As UInt32 = seed
        For i As Integer = start To size - 1
            crc = (crc >> 8) Xor table(buffer(i) Xor crc And &HFF)

        Next
        Return crc
    End Function

    Private Function UInt32ToBigEndianBytes(ByVal x As UInt32) As Byte()
        Return New Byte() {CByte((x >> 24) And &HFF), CByte((x >> 16) And &HFF), CByte((x >> 8) And &HFF), CByte(x And &HFF)}
    End Function
End Class
