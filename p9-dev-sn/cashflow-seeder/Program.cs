using cashflow_seeder;

// Seed Keycloak(Realm, Client, Roles, Users)
await KeycloakSeeder.SeedAsync();

// Seed Cashflow Transactions
await CashflowTransactionsSeeder.SeedAsync();