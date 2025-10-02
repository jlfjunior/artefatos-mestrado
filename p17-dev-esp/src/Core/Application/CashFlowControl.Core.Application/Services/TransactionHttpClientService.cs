using CashFlowControl.Core.Application.Interfaces.Services;
using CashFlowControl.Core.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CashFlowControl.Core.Application.Services
{
    public class TransactionHttpClientService : ITransactionHttpClientService
    {
        private readonly HttpClient _httpClient;
        private string TransactionApiUrl;

        public TransactionHttpClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("SemValidacaoSSL");
            TransactionApiUrl = configuration["TransactionApiUrl"] ?? throw new ArgumentNullException("TransactionApiUrl não configurada!");
        }

        public async Task<List<Transaction>?> GetTransactionsAsync()
        {
            var allTransactions = await _httpClient.GetFromJsonAsync<List<Transaction>>(TransactionApiUrl);
            return allTransactions;
        }
        public async Task<List<Transaction>?> GetTransactionsByDateAsync(DateTime date)
        {
            var requestUri = TransactionApiUrl + $"/date/{date.ToString("yyyy-MM-dd")}";
            var response = await _httpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao buscar transações: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            var allTransactions =  await response.Content.ReadFromJsonAsync<List<Transaction>>();

            return allTransactions;
        }
    }
}
