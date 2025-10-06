CREATE
OR
ALTER PROCEDURE usp_SelectUsers
    @Username VARCHAR (32) = NULL,
    @CreatedSince DATETIME = NULL,
    @CreatedUntil DATETIME = NULL
    AS
BEGIN

SELECT UserId,
       Username,
       PasswordSalt,
       PasswordHash,
       CreatedAt
FROM tbl_User
WHERE (@Username IS NULL OR Username LIKE '%' + @Username + '%')
  AND (@CreatedSince IS NULL OR CreatedAt >= @CreatedSince)
  AND (@CreatedUntil IS NULL OR CreatedAt <= @CreatedUntil);

END
