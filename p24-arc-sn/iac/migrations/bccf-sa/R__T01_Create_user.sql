IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '${user_login}')
BEGIN

    CREATE
        LOGIN [${user_login}]
    WITH
        PASSWORD = '${user_password}',
        CHECK_POLICY = OFF;

END

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '${user_login}')
BEGIN

    CREATE USER
        [${user_login}]
    FOR
        LOGIN [${user_login}];

END

ALTER
    LOGIN [${user_login}]
WITH
    PASSWORD = '${user_password}',
    CHECK_POLICY = OFF;

GO;
