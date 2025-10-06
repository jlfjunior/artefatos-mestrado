CREATE
OR
ALTER PROCEDURE usp_SelectTransaction
    @TransactionId INT
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
WHERE TransactionId = @TransactionId;

END
