CREATE
OR
ALTER PROCEDURE usp_SelectTransactionsOnProjectedRepaymentDate
    @AccountId INT = NULL,
    @ProjectedRepaymentDate DATE
    AS
BEGIN

SELECT TransactionId,
       UserId,
       AccountId,
       TransactionType,
       Amount,
       Description,
       TransactionDate,
       TransactionFee,
       ProjectedRepaymentDate
FROM tbl_Transaction
WHERE (@AccountId IS NULL OR AccountId = @AccountId)
  AND CAST(ProjectedRepaymentDate AS DATE) = @ProjectedRepaymentDate;

END
