Public Class JsonRpcResponse(Of T)
    Public Property Jsonrpc As String
    Public Property Result As T
    Public Property Id As Integer
End Class