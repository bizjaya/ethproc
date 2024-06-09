Imports MySqlConnector

Public Module DbInit
    Public Sub SeedDatabase(connectionString As String)
        Using connection = New MySqlConnection(connectionString)
            connection.Open()
            Using command = connection.CreateCommand()
                command.CommandText = "
                    CREATE TABLE IF NOT EXISTS `Blocks` (
                        `BlockID` INT AUTO_INCREMENT PRIMARY KEY,
                        `BlockNumber` BIGINT NOT NULL,
                        `Hash` VARCHAR(66),
                        `ParentHash` VARCHAR(66),
                        `Miner` VARCHAR(42),
                        `BlockReward` DECIMAL(50,0),
                        `GasLimit` DECIMAL(50,0),
                        `GasUsed` DECIMAL(50,0)
                    );

                    CREATE TABLE IF NOT EXISTS `Transactions` (
                        `TransactionID` INT AUTO_INCREMENT PRIMARY KEY,
                        `BlockID` INT,
                        `Hash` VARCHAR(66),
                        `From` VARCHAR(42),
                        `To` VARCHAR(42),
                        `Value` DECIMAL(50,0),
                        `Gas` DECIMAL(50,0),
                        `GasPrice` DECIMAL(50,0),
                        `TransactionIndex` INT,
                        FOREIGN KEY (`BlockID`) REFERENCES `Blocks`(`BlockID`)
                    );
                "
                command.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Module
