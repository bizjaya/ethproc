Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Public Class Transaction
    <Key>
    <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
    Public Property TransactionID As Integer

    Public Property BlockID As Integer

    <MaxLength(66)>
    Public Property Hash As String

    <MaxLength(42)>
    Public Property From As String

    <MaxLength(42)>
    Public Property [To] As String

    <Column(TypeName:="decimal(50,0)")>
    Public Property Value As Decimal

    <Column(TypeName:="decimal(50,0)")>
    Public Property Gas As Decimal

    <Column(TypeName:="decimal(50,0)")>
    Public Property GasPrice As Decimal

    Public Property TransactionIndex As Integer
End Class
