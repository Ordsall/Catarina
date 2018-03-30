Namespace Olvia.Devices.Octopus
    Public Class TargetData
        Public Property Zone As Byte
        Public Property Counter As Byte
        Public Property Speed As Double
        Public Property Srv As Byte
        Public Property FixationDate As Date
        Public Property Direction As TargetDirection
        Enum TargetDirection
            none
            Incoming
            Outcoming
        End Enum



        Sub New()

        End Sub

        Public Overloads Shared Function GetFixationDate(ByVal Cntr As Byte, ByVal Ref As Date) As Date
            Return (Ref - TimeSpan.FromMilliseconds(2))
        End Function

        Sub New(ByVal Zone As Byte, ByVal Counter As Byte, ByVal Speed As Double, ByVal Dir As TargetDirection, ByVal Srv As Byte, ByVal Tmr As Byte)
            Me.New(Zone, Counter, Speed, Dir, Srv, gd(Tmr))
        End Sub

        Private Shared Function gd(ByVal cntr As Integer) As Date
            If cntr = 255 Then Return Nothing
            Return Now - TimeSpan.FromMilliseconds(cntr * 100)
        End Function

        Sub New(ByVal Zone As Byte, ByVal Counter As Byte, ByVal Speed As Double, ByVal Dir As TargetDirection, ByVal Srv As Byte, ByVal FixationDate As Date)
            Me.Zone = Zone
            Me.Counter = Counter
            Me.Speed = Speed
            Me.Direction = Dir
            Me.Srv = Srv
            Me.FixationDate = FixationDate
        End Sub

    End Class
End Namespace