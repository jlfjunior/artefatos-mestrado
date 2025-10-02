using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace cashflow_seeder
{
    public static class KeycloakSeeder
    {
        public static async Task SeedAsync()
        {
            var host = "http://localhost:8080";
            var kuser = "admin";
            var kpass = "Passw0rd";

            using var httpClient = new HttpClient();

            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{host}/realms/master/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["username"] = kuser,
                    ["password"] = kpass,
                    ["grant_type"] = "password",
                    ["client_id"] = "admin-cli"
                })
            };

            var tokenResponse = await httpClient.SendAsync(tokenRequest);
            tokenResponse.EnsureSuccessStatusCode();

            var tJson = await tokenResponse.Content.ReadAsStringAsync();
            var _token = JsonDocument.Parse(tJson).RootElement.GetProperty("access_token").GetString();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            // Create user
            var userEmployee = new
            {
                username = "cashflow.employee",
                firstName = "Cashflow",
                lastName = "Employee",
                email = "cashflow.employee@teste.com",
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = "Passw0rd",
                        temporary = false
                    }
                },
                realmRoles = new[] { "employee" }
            };

            var usersEmployeeResponse = await httpClient.GetAsync($"{host}/admin/realms/cashflow/users?email={userEmployee.email}");
            var userEmployeeExists = usersEmployeeResponse.IsSuccessStatusCode && (await usersEmployeeResponse.Content.ReadAsStringAsync()).Contains(userEmployee.email);
            if (!userEmployeeExists)
            {
                var userEmployeeRequest = new HttpRequestMessage(HttpMethod.Post, $"{host}/admin/realms/cashflow/users")
                {
                    Content = new StringContent(JsonSerializer.Serialize(userEmployee), Encoding.UTF8, "application/json")
                };

                var userEmployeeResponse = await httpClient.SendAsync(userEmployeeRequest);
                userEmployeeResponse.EnsureSuccessStatusCode();

                if (userEmployeeResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Usuário criado...");

                    // Add realmRole
                    await AddRoleToUser(httpClient, host, userEmployee.email, "employee");
                }
                else
                {
                    Console.WriteLine($"Erro criar usuário: {userEmployeeResponse.StatusCode}");
                }
            }
            else
            {
                Console.WriteLine("Usuário já existe.");
            }

            await TestAuth(httpClient, host, userEmployee.email, "Passw0rd");

            // Create userManager
            var userManager = new
            {
                username = "cashflow.manager",
                firstName = "Cashflow",
                lastName = "Manager",
                email = "cashflow.manager@teste.com",
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new
                    {
                        type = "password",
                        value = "Passw0rd",
                        temporary = false
                    }
                },
                realmRoles = new[] { "manager", "employee" }
            };

            var usersManagerResponse = await httpClient.GetAsync($"{host}/admin/realms/cashflow/users?email={userManager.email}");
            var userManagerExists = usersEmployeeResponse.IsSuccessStatusCode && (await usersEmployeeResponse.Content.ReadAsStringAsync()).Contains(userEmployee.email);
            if (!userManagerExists)
            {
                var userManagerRequest = new HttpRequestMessage(HttpMethod.Post, $"{host}/admin/realms/cashflow/users")
                {
                    Content = new StringContent(JsonSerializer.Serialize(userManager), Encoding.UTF8, "application/json")
                };

                var userManagerResponse = await httpClient.SendAsync(userManagerRequest);
                userManagerResponse.EnsureSuccessStatusCode();

                if (userManagerResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Usuário Manager criado...");

                    // Add realmRole
                    await AddRoleToUser(httpClient, host, userManager.email, "employee");
                    await AddRoleToUser(httpClient, host, userManager.email, "manager");
                }
                else
                {
                    Console.WriteLine($"Erro criar usuário Manager: {userManagerResponse.StatusCode}");
                }
            }
            else
            {
                Console.WriteLine("Usuário Manager já existe.");
            }

            await TestAuth(httpClient, host, userManager.email, "Passw0rd");
        }

        private static async Task TestAuth(HttpClient httpClient, string host, string username, string password)
        {
            // Test auth
            var authRequest = new HttpRequestMessage(HttpMethod.Post, $"{host}/realms/cashflow/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "cashflow-web",
                    ["username"] = username,
                    ["password"] = password
                })
            };

            var authResponse = await httpClient.SendAsync(authRequest);
            if (authResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Usuário autenticado com sucesso!");
            }
            else
            {
                Console.WriteLine($"Erro ao autenticar usuário: {authResponse.StatusCode} - {await authResponse.Content.ReadAsStringAsync()}");
            }
        }

        private static async Task AddRoleToUser(HttpClient httpClient, string host, string userEmail, string roleName)
        {
            var _user = await httpClient.GetAsync($"{host}/admin/realms/cashflow/users?email={userEmail}");
            _user.EnsureSuccessStatusCode();

            var userJson = await _user.Content.ReadAsStringAsync();
            var users = JsonDocument.Parse(userJson).RootElement;

            var userId = users[0].GetProperty("id").GetString();

            var roleResponse = await httpClient.GetAsync($"{host}/admin/realms/cashflow/roles?search={roleName}");
            roleResponse.EnsureSuccessStatusCode();

            var roleJson = await roleResponse.Content.ReadAsStringAsync();
            var roleDoc = JsonDocument.Parse(roleJson).RootElement;

            if (roleDoc.GetArrayLength() == 0)
            {
                Console.WriteLine($"Role ({roleName}) não encontrada.");
                return;
            }

            var roleId = roleDoc[0].GetProperty("id").GetString();
            var assignRoleUrl = $"{host}/admin/realms/cashflow/users/{userId}/role-mappings/realm";
            var rolePayload = new[]
            {
                new
                {
                    id = roleId,
                    name = roleName
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(rolePayload), Encoding.UTF8, "application/json");

            var assignResponse = await httpClient.PostAsync(assignRoleUrl, content);
            if (assignResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Role ({roleName}) atribuída ao usuário {userEmail} com sucesso!");
            }
            else
            {
                Console.WriteLine($"Erro ao atribuir role: {assignResponse.StatusCode}");
                Console.WriteLine(await assignResponse.Content.ReadAsStringAsync());
            }
        }
    }
}
