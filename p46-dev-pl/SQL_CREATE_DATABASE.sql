-- Criar banco de dados se não existir
IF DB_ID('CaixaDb') IS NULL
BEGIN
    CREATE DATABASE CaixaDb;
END
GO

USE CaixaDb;
GO


IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'dbo')
BEGIN
    EXEC('CREATE SCHEMA dbo');
END
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Criar tabela Consolidations
IF OBJECT_ID('dbo.Consolidations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Consolidations (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Lojista INT NOT NULL,
        ConsolidationDate DATETIME2 NOT NULL,
        DebitTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Consolidations_DebitTotal DEFAULT(0),
        CreditTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Consolidations_CreditTotal DEFAULT(0),
        DailyBalance DECIMAL(18,2) NOT NULL CONSTRAINT DF_Consolidations_DailyBalance DEFAULT(0),
        ProcessedCount INT NOT NULL CONSTRAINT DF_Consolidations_ProcessedCount DEFAULT(0),
        LastUpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Consolidations_LastUpdatedAt DEFAULT(GETUTCDATE())
    );

    
    CREATE UNIQUE INDEX IX_Consolidations_Date_Lojista ON dbo.Consolidations(ConsolidationDate, Lojista);
END
GO

-- Criar tabela Transactions
IF OBJECT_ID('dbo.Transactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Lojista INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Description NVARCHAR(500) NULL,
        TransactionDate DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT(GETUTCDATE()),
        IsProcessed BIT NOT NULL CONSTRAINT DF_Transactions_IsProcessed DEFAULT(0)
    );

    -- Índices
    CREATE INDEX IX_Transactions_TransactionDate ON dbo.Transactions(TransactionDate);
    CREATE INDEX IX_Transactions_CreatedAt ON dbo.Transactions(CreatedAt);
END
GO



IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'caixa')
BEGIN
    CREATE LOGIN caixa WITH PASSWORD = 'Caixa1234!';
END
GO

USE CaixaDb;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'caixa')
BEGIN
    CREATE USER caixa FOR LOGIN caixa;
    ALTER ROLE db_owner ADD MEMBER caixa;
END
GO
