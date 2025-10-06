DECLARE
@AccountType_Cash INT;
DECLARE
@AccountType_BelloDebit INT;
DECLARE
@AccountType_BelloCredit INT;
DECLARE
@AccountType_NetCardDebit INT;
DECLARE
@AccountType_NetCardCredit INT;

EXEC usp_EnsureAccountType @AccountTypeName = 'Cash', @Id = @AccountType_Cash OUTPUT;
EXEC usp_EnsureAccountType @AccountTypeName = 'Bello Debit', @BaseFee = 1.75, @PaymentDueDays = 2, @Id = @AccountType_BelloDebit OUTPUT;
EXEC usp_EnsureAccountType @AccountTypeName = 'Bello Credit', @BaseFee = 2.50, @PaymentDueDays = 12, @Id = @AccountType_BelloCredit OUTPUT;
EXEC usp_EnsureAccountType @AccountTypeName = 'NetCard Debit', @BaseFee = 1.15, @PaymentDueDays = 3, @Id = @AccountType_NetCardDebit OUTPUT;
EXEC usp_EnsureAccountType @AccountTypeName = 'NetCard Credit', @BaseFee = 4.00, @PaymentDueDays = 6, @Id = @AccountType_NetCardCredit OUTPUT;

DECLARE
@Admin_UserId INT;

EXEC usp_EnsureUser @Username = 'admin', @Password = 'wi$eLunch63', @CreatedAt = NULL, @Id = @Admin_UserId OUTPUT;

DECLARE
@Admin_Cash_AccountId INT;

EXEC usp_EnsureAccount @UserId = @Admin_UserId, @AccountTypeId = @AccountType_Cash, @AccountName = 'Cash', @InitialBalance = 0.00, @CurrentBalance = 0.00, @BalanceUpdatedAt = NULL, @Id = @Admin_Cash_AccountId OUTPUT;
EXEC usp_EnsureAccount @UserId = @Admin_UserId, @AccountTypeId = @AccountType_BelloDebit, @AccountName = 'Bello Debit', @InitialBalance = 1.04, @CurrentBalance = 0.00, @BalanceUpdatedAt = NULL, @Id = @Admin_Cash_AccountId OUTPUT;
EXEC usp_EnsureAccount @UserId = @Admin_UserId, @AccountTypeId = @AccountType_BelloCredit, @AccountName = 'Bello Credit', @InitialBalance = 2.03, @CurrentBalance = 0.00, @BalanceUpdatedAt = NULL, @Id = @Admin_Cash_AccountId OUTPUT;
EXEC usp_EnsureAccount @UserId = @Admin_UserId, @AccountTypeId = @AccountType_NetCardDebit, @AccountName = 'NetCard Debit', @InitialBalance = 3.02, @CurrentBalance = 0.00, @BalanceUpdatedAt = NULL, @Id = @Admin_Cash_AccountId OUTPUT;
EXEC usp_EnsureAccount @UserId = @Admin_UserId, @AccountTypeId = @AccountType_NetCardCredit, @AccountName = 'NetCard Credit', @InitialBalance = 4.01, @CurrentBalance = 0.00, @BalanceUpdatedAt = NULL, @Id = @Admin_Cash_AccountId OUTPUT;
