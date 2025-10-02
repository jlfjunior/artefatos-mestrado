namespace CashFlowControl.ApiGateway.API.Extensions
{
    public class CustomDelegatingHandler : DelegatingHandler
    {
        private readonly HttpClientHandler _handler;

        public CustomDelegatingHandler()
        {
            _handler = new HttpClientHandler();
            _handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var client = new HttpClient(_handler);
            return await client.SendAsync(request, cancellationToken);
        }
    }
}
