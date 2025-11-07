namespace Accenture_Assesment.Web
{
    class ApplicationApiClient(HttpClient httpClient)
    {
        public async Task<TResponse?> GetAsync<TResponse>(string uri, CancellationToken cancellationToken = default)
        {
            return await httpClient.GetFromJsonAsync<TResponse>(uri, cancellationToken);
        }
    }
}
