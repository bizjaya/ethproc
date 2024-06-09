Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging
Imports AutoMapper
Imports System.IO
Imports System.Net.Http
Imports Serilog
Imports Microsoft.Extensions.Options

Module Program
    Sub Main(args As String())
        ' Load configuration from appsettings.json
        Dim configuration = New ConfigurationBuilder() _
            .SetBasePath(Directory.GetCurrentDirectory()) _
            .AddJsonFile("appsettings.json", optional:=False, reloadOnChange:=True) _
            .Build()

        ' Configure Serilog for file logging
        Log.Logger = New LoggerConfiguration() _
            .ReadFrom.Configuration(configuration) _
            .WriteTo.Console() _
            .WriteTo.File("Logs/eth_indexer_.txt", rollingInterval:=RollingInterval.Day) _
            .CreateLogger()

        ' Configure services and build the service provider
        Dim serviceProvider = ConfigureServices(configuration)

        ' Seed the database tables if they do not exist
        SeedDatabase(serviceProvider)

        ' Get the EthService and start processing blocks
        Dim ethService = serviceProvider.GetService(Of IEthService)()
        ethService.ProcessBlocksAsync(12100001, 12100500).GetAwaiter().GetResult()
    End Sub

    Private Function ConfigureServices(configuration As IConfiguration) As IServiceProvider
        ' Configure services
        Dim services = New ServiceCollection()

        ' Add logging
        services.AddLogging(Sub(configure)
                                configure.AddSerilog()
                            End Sub)

        ' Add AutoMapper
        services.AddAutoMapper(GetType(MappingProfile))

        ' Add HttpClient
        services.AddSingleton(New HttpClient())

        ' Add the configuration
        services.AddSingleton(Of IConfiguration)(configuration)

        ' Add AppSettings
        services.Configure(Of AppStg)(configuration.GetSection("AppStg"))

        ' Add database context
        services.AddDbContext(Of EthDbCtx)()

        ' Add EthService
        services.AddScoped(Of IEthService, EthService)()

        Return services.BuildServiceProvider()
    End Function

    Private Sub SeedDatabase(serviceProvider As IServiceProvider)
        Using scope = serviceProvider.CreateScope()
            Dim options = scope.ServiceProvider.GetService(Of IOptions(Of AppStg))()
            DbInit.SeedDatabase(options.Value.ConnectionStrings.DefaultConnection)
        End Using
    End Sub
End Module
