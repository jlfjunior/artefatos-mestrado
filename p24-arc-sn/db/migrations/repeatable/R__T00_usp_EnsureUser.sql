CREATE
OR
ALTER PROCEDURE usp_EnsureUser
    @Username VARCHAR (32),
    @Password NVARCHAR(128),
    @CreatedAt DATETIME = NULL,
    @Id INT OUTPUT
    AS
BEGIN

    DECLARE
@PasswordSalt VARCHAR(16);
    DECLARE
@PasswordHash VARCHAR(64);

    IF
NOT EXISTS (
       SELECT 1
       FROM tbl_User
       WHERE Username = @Username)
BEGIN

        SET
@PasswordSalt = LEFT(CONVERT(VARCHAR(36), NEWID()), 16);
        SET
@PasswordHash = CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @Password + @PasswordSalt), 2);

        IF
@CreatedAt IS NULL
BEGIN
            SET
@CreatedAt = GETDATE();
END

INSERT INTO tbl_User (Username,
                      PasswordSalt,
                      PasswordHash,
                      CreatedAt)
VALUES (@Username,
        @PasswordSalt,
        @PasswordHash,
        @CreatedAt);

SELECT @Id = SCOPE_IDENTITY();

END
ELSE
BEGIN

SELECT TOP 1
            @Id = UserId
FROM tbl_User
WHERE Username = @Username

END

END
