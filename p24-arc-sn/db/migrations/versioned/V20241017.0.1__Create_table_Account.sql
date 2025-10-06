CREATE TABLE tbl_Account
(
    AccountId        INT PRIMARY KEY IDENTITY(1,1),
    UserId           INT                        NOT NULL,
    AccountTypeId    INT                        NOT NULL,
    AccountName      VARCHAR(100)               NOT NULL,
    InitialBalance   DECIMAL(18, 2)             NOT NULL,
    CurrentBalance   DECIMAL(18, 2)             NOT NULL,
    BalanceUpdatedAt DATETIME                   NOT NULL,
    CreatedAt        DATETIME DEFAULT GETDATE() NOT NULL,
    FOREIGN KEY (UserId) REFERENCES tbl_User (UserId),
    FOREIGN KEY (AccountTypeId) REFERENCES tbl_AccountType (AccountTypeId)
);
