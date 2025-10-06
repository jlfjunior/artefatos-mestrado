CREATE TABLE tbl_AccountType
(
    AccountTypeId   INT PRIMARY KEY IDENTITY (1,1),
    AccountTypeName VARCHAR(50)    NOT NULL,
    BaseFee         DECIMAL(10, 2) NOT NULL,
    PaymentDueDays  INT            NOT NULL
);
