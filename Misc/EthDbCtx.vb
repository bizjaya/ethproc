Imports System.IO
Imports Microsoft.EntityFrameworkCore
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.Options

Public Class EthDbCtx
    Inherits DbContext

    Public Property Blocks As DbSet(Of Block)
    Public Property Transactions As DbSet(Of Transaction)

    Private ReadOnly _connectionString As String

    Public Sub New(options As IOptions(Of AppStg))
        _connectionString = options.Value.ConnectionStrings.DefaultConnection
    End Sub

    Protected Overrides Sub OnConfiguring(optionsBuilder As DbContextOptionsBuilder)
        optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
    End Sub

    Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
        MyBase.OnModelCreating(modelBuilder)
        'SeedDatabase()
    End Sub

    Private Sub SeedDatabase()
        Using connection = Database.GetDbConnection()
            connection.Open()
            Using command = connection.CreateCommand()
                command.CommandText = "
                    CREATE TABLE IF NOT EXISTS `Block` (
                        `BlockID` INT AUTO_INCREMENT PRIMARY KEY,
                        `BlockNumber` BIGINT NOT NULL,
                        `Hash` VARCHAR(66),
                        `ParentHash` VARCHAR(66),
                        `Miner` VARCHAR(42),
                        `BlockReward` DECIMAL(50,0),
                        `GasLimit` DECIMAL(50,0),
                        `GasUsed` DECIMAL(50,0)
                    );
                    
                    CREATE TABLE IF NOT EXISTS `Transaction` (
                        `TransactionID` INT AUTO_INCREMENT PRIMARY KEY,
                        `BlockID` INT,
                        `Hash` VARCHAR(66),
                        `From` VARCHAR(42),
                        `To` VARCHAR(42),
                        `Value` DECIMAL(50,0),
                        `Gas` DECIMAL(50,0),
                        `GasPrice` DECIMAL(50,0),
                        `TransactionIndex` INT,
                        FOREIGN KEY (`BlockID`) REFERENCES `Block`(`BlockID`)
                    );
                "
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class