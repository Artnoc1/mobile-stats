namespace MobileStats.Bitrise.Api
{
    class Bitrise
    {
        private readonly string authToken;

        public Bitrise(string apiToken)
        {
            authToken = $"token {apiToken}";
        }

        public App App(string appId) => new App(authToken, appId);
    }
}
