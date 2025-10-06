CREATE TABLE tbl_Transaction
(
    TransactionId          INT PRIMARY KEY IDENTITY(1,1),
    UserId                 INT            NOT NULL,
    AccountId              INT            NOT NULL,
    TransactionType        BIT            NOT NULL,
    Amount                 DECIMAL(18, 2) NOT NULL,
    Description            VARCHAR(255),
    TransactionDate        DATETIME       NOT NULL,
    TransactionFee         DECIMAL(18, 2),
    ProjectedRepaymentDate DATETIME,
    FOREIGN KEY (UserId) REFERENCES tbl_User (UserId),
    FOREIGN KEY (AccountId) REFERENCES tbl_Account (AccountId)
);
