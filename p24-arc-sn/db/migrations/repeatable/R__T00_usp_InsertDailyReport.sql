CREATE
OR
ALTER PROCEDURE usp_InsertDailyReport
    @AccountId INT = NULL,
    @ReportDate DATE,
    @TotalDebits DECIMAL (18, 2),
    @TotalCredits DECIMAL (18, 2),
    @TotalFee DECIMAL (18, 2),
    @Balance DECIMAL (18, 2),
    @CreatedAt DATETIME = NULL
    AS
BEGIN

    IF
@CreatedAt IS NULL
BEGIN
        SET
@CreatedAt = GETDATE();
END

INSERT INTO tbl_DailyReport (AccountId,
                             ReportDate,
                             TotalDebits,
                             TotalCredits,
                             TotalFee,
                             Balance,
                             CreatedAt)
VALUES (@AccountId,
        @ReportDate,
        @TotalDebits,
        @TotalCredits,
        @TotalFee,
        @Balance,
        @CreatedAt);

SELECT SCOPE_IDENTITY() AS ReportId;

END
