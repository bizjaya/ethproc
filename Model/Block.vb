Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Public Class Block
    <Key>
    <DatabaseGenerated(DatabaseGeneratedOption.Identity)>
    Public Property BlockID As Integer

    Public Property BlockNumber As Long

    <MaxLength(66)>
    Public Property Hash As String

    <MaxLength(66)>
    Public Property ParentHash As String

    <MaxLength(42)>
    Public Property Miner As String

    <Column(TypeName:="decimal(50,0)")>
    Public Property BlockReward As Decimal

    <Column(TypeName:="decimal(50,0)")>
    Public Property GasLimit As Decimal

    <Column(TypeName:="decimal(50,0)")>
    Public Property GasUsed As Decimal
End Class
