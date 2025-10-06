DROP PROCEDURE IF EXISTS usp_GetAccount;
GO;

CREATE
OR
ALTER PROCEDURE usp_SelectAccount
    @AccountId INT
    AS
BEGIN

SELECT AccountId,
       UserId,
       AccountTypeId,
       AccountName,
       InitialBalance,
       CurrentBalance,
       BalanceUpdatedAt,
       CreatedAt
FROM tbl_Account
WHERE AccountId = @AccountId;

END
