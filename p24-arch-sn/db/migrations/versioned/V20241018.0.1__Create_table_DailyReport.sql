CREATE TABLE tbl_DailyReport
(
    ReportId     INT PRIMARY KEY IDENTITY(1,1),
    AccountId    INT NULL,
    ReportDate   DATE           NOT NULL,
    TotalDebits  DECIMAL(18, 2) NOT NULL,
    TotalCredits DECIMAL(18, 2) NOT NULL,
    Balance      DECIMAL(18, 2) NOT NULL,
    CreatedAt    DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AccountId) REFERENCES tbl_Account (AccountId)
);

CREATE INDEX IX_ReportDate ON tbl_DailyReport (ReportDate);
CREATE INDEX IX_AccountId ON tbl_DailyReport (AccountId);
