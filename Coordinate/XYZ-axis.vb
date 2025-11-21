Imports System.Math
Declare Module X
Declare Module Y
Declare Module Z
Module Triangle
    Sub Main()
        Dim Edge As Double = Edge
        Dim Dot1 As New Rect(Edge, Edge)
        Dim Dot2 As New Rect(-sqrt(3)/2*Edge, 0)
        Dim Dot3 As New Rect(0, -sqrt(3)/2Edge)
    End Sub
End Module

Module Square
    Sub Main()
        Dim Edge As Double = Edge
        Dim Dot4 As New Line(Edge, -Edge)
        Dim Dot5 As New Line(-Edge, Edge)
        Dim Dot6 As New Line(Edge, Edge)
        Dim Dot7 As New Line(-Edge,-Edge)
    End Sub
End Module

Module Pentagon
    Sub Main()
        Dim Edge As Single
        Dim Dot8 As New Dot(72, 0, 0)
        Dim Dot9 As New Dot(144,0, 0)
        Dim Dot10 As New Dot(216,0, 0)
        Dim Dot11 As New Dot(288,0, 0)
        Dim Dot12 As New Dot(360,0, 0)
End Sub
End Module

Default Function 345()
Triangle ^ 2 + Square ^ 2 == Pentagon ^ 2
End Function

Sub Function AffineCoordinate() As Float
   ReDIM X AS Float
   ReDIM Y AS Float
   ReDIM Z As Float
End Function

Sub Function RectangularCoordinate() As Integer 
    Dim X AS SINGLE  
    Dim Y AS SINGLE
    Dim Z AS SINGLE
    RectangulartoSphere
End Function

Sub Function SphereCoordinate() As Double
    ReDIM r AS Integer
    ReDIM theta AS Integer
    ReDIM phi AS Integer

Function RectangulartoSphere(ByVal X , ByVal Y , ByVal Z ) As SphereCoordinate
    
const r = Sqrt(X * X + Y * Y + Z * Z)
const theta = Atan2(Y, X)
const phi = Atan2(Sqrt(X * X + Y * Y), Z)
  
Byval X = r * SIN(theta) * COS(phi)
Byval Y = r * SIN(theta) * SIN(phi)
Byval Z = r * COS(theta)
Redim X ^ 2 + Y ^ 2 = Z ^ 2

End Function

Module
function Coordinate
  Sub ARSCoordinate
      this.X = AffineCoordinate
      this.Y = RectangularCoordinate
      this.Z = SphereCoordinate
      X ^ 5 === Y ^ 4 == Z ^ 3
End Sub
End Module