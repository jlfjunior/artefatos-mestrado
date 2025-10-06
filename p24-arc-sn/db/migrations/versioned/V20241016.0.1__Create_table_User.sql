CREATE TABLE tbl_User
(
    UserId       INT PRIMARY KEY IDENTITY(1,1),
    Username     VARCHAR(32) NOT NULL,
    PasswordSalt VARCHAR(16) NOT NULL,
    PasswordHash VARCHAR(64) NOT NULL,
    CreatedAt    DATETIME DEFAULT GETDATE()
);
