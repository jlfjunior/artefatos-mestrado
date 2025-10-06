CREATE
OR ALTER
VIEW vw_TransactionType AS
SELECT 0       AS TransactionType,
       'Debit' AS TransactionTypeDescription
UNION ALL
SELECT 1        AS TransactionType,
       'Credit' AS TransactionTypeDescription;
