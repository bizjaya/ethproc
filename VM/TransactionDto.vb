Imports Newtonsoft.Json

Public Class TransactionDto
    Public Property Hash As String

    <JsonProperty("from")>
    Public Property FromAddress As String

    <JsonProperty("to")>
    Public Property ToAddress As String

    Public Property Value As String
    Public Property Gas As String
    Public Property GasPrice As String
    Public Property TransactionIndex As String
End Class
