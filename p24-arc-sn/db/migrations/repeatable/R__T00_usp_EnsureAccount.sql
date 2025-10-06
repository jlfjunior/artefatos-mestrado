CREATE
OR
ALTER PROCEDURE usp_EnsureAccount
    @UserId INT,
    @AccountTypeId INT,
    @AccountName VARCHAR (100),
    @InitialBalance DECIMAL (18, 2),
    @CurrentBalance DECIMAL (18, 2),
    @BalanceUpdatedAt DATETIME = NULL,
    @CreatedAt DATETIME = NULL,
    @Id INT OUTPUT
    AS
BEGIN

    IF
@CreatedAt IS NULL
BEGIN
        SET
@CreatedAt = GETDATE();
END

    IF
@BalanceUpdatedAt IS NULL
BEGIN
        SET
@BalanceUpdatedAt = GETDATE();
END

    IF
NOT EXISTS (
       SELECT 1
       FROM tbl_Account
       WHERE UserId = @UserId
       AND AccountTypeId = @AccountTypeId
       AND AccountName = @AccountName)
BEGIN

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

SELECT @Id = SCOPE_IDENTITY();

END
ELSE
BEGIN

SELECT TOP 1
            @Id = AccountId
FROM tbl_Account
WHERE UserId = @UserId
  AND AccountTypeId = @AccountTypeId
  AND AccountName = @AccountName

END

END
