using System.Net.Http.Headers;

namespace cashflow_seeder
{
    public static class CashflowTransactionsSeeder
    {
        public static async Task SeedAsync()
        {
            var _apiTransactions = "http://localhost:5282/api";
            var _token = "";

            using var httpClient = new HttpClient();


            // Authenticate with the API
            var authRequest = new HttpRequestMessage(HttpMethod.Post, $"http://localhost:8080/realms/cashflow/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = "cashflow-web",
                    ["username"] = "cashflow.manager@teste.com",
                    ["password"] = "Passw0rd"
                })
            };

            var authResponse = await httpClient.SendAsync(authRequest);
            if (authResponse.IsSuccessStatusCode)
            {
                var res = await authResponse.Content.ReadAsStringAsync();
                _token = System.Text.Json.JsonDocument.Parse(res).RootElement.GetProperty("access_token").GetString();

                if(string.IsNullOrEmpty(_token))
                {
                    Console.WriteLine("Token vazio.");
                    return;
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
            else
            {
                Console.WriteLine($"Erro ao autenticar usuário: {authResponse.StatusCode} - {await authResponse.Content.ReadAsStringAsync()}");
            }

            // Create massive transactions with csv
            var file = "transactions_seed.csv";

            using var form = new MultipartFormDataContent();

            using var fileStream = File.OpenRead(file);
            var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            form.Add(streamContent, "file", "transactions_seed.csv");

            Console.WriteLine("Enviando requisição...");

            var response = await httpClient.PostAsync($"{_apiTransactions}/v1/transactions/massive-create", form);

            Console.WriteLine($"Importação transactions status: {response.StatusCode}");
        }
    }
}
