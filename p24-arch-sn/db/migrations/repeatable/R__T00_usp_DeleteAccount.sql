CREATE
OR
ALTER PROCEDURE usp_DeleteAccount
    @AccountId INT
    AS
BEGIN

DELETE
FROM tbl_Account
WHERE AccountId = @AccountId;

END
