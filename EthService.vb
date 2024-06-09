Imports System.Globalization
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports AutoMapper
Imports Microsoft.EntityFrameworkCore
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Options
Imports Newtonsoft.Json

' Interface defining the Ethereum service
Public Interface IEthService
    ' Method to process blocks within a specified range
    Function ProcessBlocksAsync(startBlock As Integer, endBlock As Integer) As Task
End Interface

' Implementation of the Ethereum service
Public Class EthService
    Implements IEthService

    Private ReadOnly _httpClient As HttpClient
    Private ReadOnly _logger As ILogger(Of EthService)
    Private ReadOnly _mapper As IMapper
    Private ReadOnly _context As EthDbCtx
    Private ReadOnly _alchemyApiKey As String

    ' Constructor to initialize the service with dependencies
    Public Sub New(httpClient As HttpClient, logger As ILogger(Of EthService), mapper As IMapper, context As EthDbCtx, options As IOptions(Of AppStg))
        _httpClient = httpClient
        _logger = logger
        _mapper = mapper
        _context = context
        _alchemyApiKey = options.Value.AlchemyApiKey
    End Sub

    ' Method to process blocks within a specified range
    Public Async Function ProcessBlocksAsync(startBlock As Integer, endBlock As Integer) As Task Implements IEthService.ProcessBlocksAsync
        For blockNumber As Integer = startBlock To endBlock
            Dim stopwatch As Stopwatch = Stopwatch.StartNew()
            Await ProcessBlockAsync(blockNumber)
            stopwatch.Stop()
            _logger.LogInformation($"Processed block {blockNumber} in {stopwatch.ElapsedMilliseconds} ms")
        Next
    End Function

    ' Method to process a single block
    Private Async Function ProcessBlockAsync(blockNumber As Integer) As Task
        Dim hexBlockNumber As String = "0x" & blockNumber.ToString("X")

        Dim block = Await GetBlockByNumber(hexBlockNumber)
        If block Is Nothing Then
            _logger.LogError($"Block {blockNumber} not found.")
            Return
        End If

        Await InsertBlockToDatabase(block)

        Dim transactionCount As Integer = Await GetBlockTransactionCountByNumber(hexBlockNumber)
        If transactionCount > 0 Then
            Await ProcessTransactionsAsync(block, transactionCount)
        End If
    End Function

    ' Method to retrieve block information by block number
    Private Async Function GetBlockByNumber(hexBlockNumber As String) As Task(Of BlockDto)
        Dim response = Await _httpClient.PostAsync($"https://eth-mainnet.alchemyapi.io/v2/{_alchemyApiKey}",
            New StringContent(JsonConvert.SerializeObject(New With {
                .jsonrpc = "2.0",
                .method = "eth_getBlockByNumber",
                .params = {hexBlockNumber, True},
                .id = 1
            }), Encoding.UTF8, "application/json"))

        response.EnsureSuccessStatusCode()
        Dim responseContent = Await response.Content.ReadAsStringAsync()
        Dim result = JsonConvert.DeserializeObject(Of JsonRpcResponse(Of BlockDto))(responseContent)
        Return result.Result
    End Function

    ' Method to retrieve the transaction count for a block
    Private Async Function GetBlockTransactionCountByNumber(hexBlockNumber As String) As Task(Of Integer)
        Dim response = Await _httpClient.PostAsync($"https://eth-mainnet.alchemyapi.io/v2/{_alchemyApiKey}",
            New StringContent(JsonConvert.SerializeObject(New With {
                .jsonrpc = "2.0",
                .method = "eth_getBlockTransactionCountByNumber",
                .params = {hexBlockNumber},
                .id = 1
            }), Encoding.UTF8, "application/json"))

        response.EnsureSuccessStatusCode()
        Dim responseContent = Await response.Content.ReadAsStringAsync()
        Dim result = JsonConvert.DeserializeObject(Of JsonRpcResponse(Of String))(responseContent)
        Return Integer.Parse(result.Result.Substring(2), NumberStyles.HexNumber)
    End Function

    ' Method to process transactions for a block
    Private Async Function ProcessTransactionsAsync(block As BlockDto, transactionCount As Integer) As Task
        For i As Integer = 0 To transactionCount - 1
            Dim transaction = Await GetTransactionByBlockNumberAndIndex(block.Number, i)
            If transaction IsNot Nothing Then
                Await InsertTransactionToDatabase(block, transaction)
            End If
        Next
    End Function

    ' Method to retrieve transaction information by block number and index
    Private Async Function GetTransactionByBlockNumberAndIndex(hexBlockNumber As String, index As Integer) As Task(Of TransactionDto)
        Dim response = Await _httpClient.PostAsync($"https://eth-mainnet.alchemyapi.io/v2/{_alchemyApiKey}",
            New StringContent(JsonConvert.SerializeObject(New With {
                .jsonrpc = "2.0",
                .method = "eth_getTransactionByBlockNumberAndIndex",
                .params = {hexBlockNumber, "0x" & index.ToString("X")},
                .id = 1
            }), Encoding.UTF8, "application/json"))

        response.EnsureSuccessStatusCode()
        Dim responseContent = Await response.Content.ReadAsStringAsync()
        Dim result = JsonConvert.DeserializeObject(Of JsonRpcResponse(Of TransactionDto))(responseContent)
        Return result.Result
    End Function

    ' Method to insert block information into the database
    Private Async Function InsertBlockToDatabase(blockDto As BlockDto) As Task
        Dim blockEntity = _mapper.Map(Of Block)(blockDto)
        _context.Blocks.Add(blockEntity)
        Await _context.SaveChangesAsync()
    End Function

    ' Method to insert transaction information into the database
    Private Async Function InsertTransactionToDatabase(blockDto As BlockDto, transactionDto As TransactionDto) As Task
        Dim blockNumber As Long = Long.Parse(blockDto.Number.Substring(2), NumberStyles.HexNumber)
        Dim blockEntity = Await _context.Blocks.FirstOrDefaultAsync(Function(b) b.BlockNumber = blockNumber)
        If blockEntity Is Nothing Then
            Throw New Exception($"Block with number {blockDto.Number} not found in database.")
        End If
        Dim transactionEntity = _mapper.Map(Of Transaction)(transactionDto)
        transactionEntity.BlockID = blockEntity.BlockID
        _context.Transactions.Add(transactionEntity)
        Await _context.SaveChangesAsync()
    End Function
End Class
