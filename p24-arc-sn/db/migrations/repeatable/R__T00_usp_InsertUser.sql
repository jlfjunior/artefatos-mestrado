CREATE
OR
ALTER PROCEDURE usp_InsertUser
    @Username VARCHAR (32),
    @PasswordSalt VARCHAR (16),
    @PasswordHash VARCHAR (64),
    @CreatedAt DATETIME = NULL
    AS
BEGIN

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

SELECT SCOPE_IDENTITY() AS UserId;

END
