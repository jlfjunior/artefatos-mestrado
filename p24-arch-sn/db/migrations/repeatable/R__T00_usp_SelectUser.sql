CREATE
OR
ALTER PROCEDURE usp_SelectUser
    @UserId INT
    AS
BEGIN

SELECT UserId,
       Username,
       PasswordSalt,
       PasswordHash,
       CreatedAt
FROM tbl_User
WHERE UserId = @UserId;

END
