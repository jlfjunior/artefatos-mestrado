CREATE
OR
ALTER PROCEDURE usp_EnsureAccountType
    @AccountTypeName VARCHAR (50),
    @BaseFee DECIMAL (10, 2) = 0,
    @PaymentDueDays INT = 0,
    @Id INT OUTPUT
    AS
BEGIN

    IF
NOT EXISTS (
       SELECT 1
       FROM tbl_AccountType
       WHERE AccountTypeName = @AccountTypeName)
BEGIN

INSERT INTO tbl_AccountType (AccountTypeName,
                             BaseFee,
                             PaymentDueDays)
VALUES (@AccountTypeName,
        @BaseFee,
        @PaymentDueDays);

SELECT @Id = SCOPE_IDENTITY();

END
ELSE
BEGIN

SELECT TOP 1
            @Id = AccountTypeId
FROM tbl_AccountType
WHERE AccountTypeName = @AccountTypeName

END

END
