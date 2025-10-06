CREATE
OR
ALTER PROCEDURE usp_InsertAccount
    @UserId INT,
    @AccountTypeId INT,
    @AccountName VARCHAR (100),
    @InitialBalance DECIMAL (18, 2),
    @CurrentBalance DECIMAL (18, 2),
    @BalanceUpdatedAt DATETIME,
    @CreatedAt DATETIME = NULL
    AS
BEGIN

    IF
@CreatedAt IS NULL

BEGIN
        SET
@CreatedAt = GETDATE();
END

INSERT INTO tbl_Account (UserId,
                         AccountTypeId,
                         AccountName,
                         InitialBalance,
                         CurrentBalance,
                         BalanceUpdatedAt,
                         CreatedAt)
VALUES (@UserId,
        @AccountTypeId,
        @AccountName,
        @InitialBalance,
        @CurrentBalance,
        @BalanceUpdatedAt,
        @CreatedAt);

SELECT SCOPE_IDENTITY() AS AccountId;

END
