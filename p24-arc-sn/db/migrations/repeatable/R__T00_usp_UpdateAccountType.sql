CREATE OR ALTER PROCEDURE usp_UpdateAccountType @AccountTypeId INT,
                                                @AccountTypeName VARCHAR(50),
                                                @BaseFee DECIMAL(10, 2),
                                                @PaymentDueDays INT
AS
BEGIN

    UPDATE
        tbl_AccountType
    SET AccountTypeName = @AccountTypeName,
        BaseFee         = @BaseFee,
        PaymentDueDays  = @PaymentDueDays
    WHERE AccountTypeId = @AccountTypeId;

END
