CREATE
OR
ALTER PROCEDURE usp_InsertTransaction
    @UserId INT = NULL,
    @AccountId INT,
    @TransactionType BIT,
    @Amount DECIMAL (18, 2),
    @Description VARCHAR (255) = NULL,
    @TransactionDate DATETIME,
    @TransactionFee DECIMAL (18, 2) = NULL,
    @ProjectedRepaymentDate DATETIME = NULL
    AS
BEGIN

INSERT INTO tbl_Transaction (UserId,
                             AccountId,
                             TransactionType,
                             Amount,
                             Description,
                             TransactionDate,
                             TransactionFee,
                             ProjectedRepaymentDate)
VALUES (@UserId,
        @AccountId,
        @TransactionType,
        @Amount,
        @Description,
        @TransactionDate,
        @TransactionFee,
        @ProjectedRepaymentDate);

SELECT SCOPE_IDENTITY() AS TransactionId;

END
