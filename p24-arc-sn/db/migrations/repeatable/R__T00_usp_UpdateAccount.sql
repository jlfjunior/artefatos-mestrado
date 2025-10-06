CREATE
OR
ALTER PROCEDURE usp_UpdateAccount
    @AccountId INT,
    @UserId INT,
    @AccountTypeId INT,
    @AccountName VARCHAR (100),
    @InitialBalance DECIMAL (18, 2),
    @CurrentBalance DECIMAL (18, 2),
    @BalanceUpdatedAt DATETIME
    AS
BEGIN

UPDATE tbl_Account
SET UserId           = @UserId,
    AccountTypeId    = @AccountTypeId,
    AccountName      = @AccountName,
    InitialBalance   = @InitialBalance,
    CurrentBalance   = @CurrentBalance,
    BalanceUpdatedAt = @BalanceUpdatedAt
WHERE AccountId = @AccountId;

END
