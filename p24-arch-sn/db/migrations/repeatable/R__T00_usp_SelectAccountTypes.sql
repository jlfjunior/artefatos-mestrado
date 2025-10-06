CREATE
OR
ALTER PROCEDURE usp_SelectAccountTypes
    @AccountTypeName VARCHAR (50) = NULL,
    @BaseFeeFrom DECIMAL (10, 2) = NULL,
    @BaseFeeTo DECIMAL (10, 2) = NULL,
    @PaymentDueDaysFrom INT = NULL,
    @PaymentDueDaysTo INT = NULL,
    @PagingSkip INT = NULL,
    @PagingLimit INT = NULL,
    @PagingTotal INT OUTPUT
    AS
BEGIN

    SET
NOCOUNT ON;

    DECLARE
@QueryResults TABLE (
        AccountTypeId INT
    );

INSERT INTO @QueryResults (AccountTypeId)
SELECT AccountTypeId
FROM tbl_AccountType
WHERE (@AccountTypeName IS NULL OR AccountTypeName LIKE '%' + @AccountTypeName + '%')
  AND (@BaseFeeFrom IS NULL OR BaseFee >= @BaseFeeFrom)
  AND (@BaseFeeTo IS NULL OR BaseFee <= @BaseFeeTo)
  AND (@PaymentDueDaysFrom IS NULL OR PaymentDueDays >= @PaymentDueDaysFrom)
  AND (@PaymentDueDaysTo IS NULL OR PaymentDueDays <= @PaymentDueDaysTo);

SELECT @PagingTotal = COUNT(*)
FROM @QueryResults;

SELECT AccountTypeId
FROM @QueryResults
ORDER BY AccountTypeId
OFFSET ISNULL(@PagingSkip, 0) ROWS FETCH NEXT ISNULL(@PagingLimit, CASE WHEN @PagingTotal = 0 THEN 1 ELSE @PagingTotal END) ROWS ONLY;
END
