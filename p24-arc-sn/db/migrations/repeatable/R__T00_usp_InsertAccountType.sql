CREATE OR ALTER PROCEDURE usp_InsertAccountType @AccountTypeName VARCHAR(50),
                                                @BaseFee DECIMAL(10, 2),
                                                @PaymentDueDays INT
AS
BEGIN

    INSERT INTO tbl_AccountType (AccountTypeName,
                                 BaseFee,
                                 PaymentDueDays)
    VALUES (@AccountTypeName,
            @BaseFee,
            @PaymentDueDays);

    SELECT SCOPE_IDENTITY() AS AccountTypeId;

END
