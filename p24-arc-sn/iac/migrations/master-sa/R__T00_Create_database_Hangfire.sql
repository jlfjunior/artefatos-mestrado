COMMIT;

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'Hangfire')
BEGIN
    CREATE DATABASE Hangfire
END
