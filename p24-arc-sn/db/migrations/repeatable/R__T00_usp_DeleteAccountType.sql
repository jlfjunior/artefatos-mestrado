CREATE OR ALTER PROCEDURE usp_DeleteAccountType @AccountTypeId INT
AS
BEGIN

    DELETE
    FROM tbl_AccountType
    WHERE AccountTypeId = @AccountTypeId;

END
