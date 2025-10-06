CREATE
OR
ALTER PROCEDURE usp_SelectDailyReport
    @ReportId INT
    AS
BEGIN

SELECT ReportId,
       AccountId,
       ReportDate,
       TotalDebits,
       TotalCredits,
       TotalFee,
       Balance,
       CreatedAt
FROM tbl_DailyReport
WHERE ReportId = @ReportId;

END
