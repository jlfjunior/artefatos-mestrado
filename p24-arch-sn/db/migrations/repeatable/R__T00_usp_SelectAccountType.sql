CREATE OR ALTER PROCEDURE usp_SelectAccountType @AccountTypeId INT
AS
BEGIN

    SELECT AccountTypeId,
           AccountTypeName,
           BaseFee,
           PaymentDueDays
    FROM tbl_AccountType
    WHERE AccountTypeId = @AccountTypeId;

END
