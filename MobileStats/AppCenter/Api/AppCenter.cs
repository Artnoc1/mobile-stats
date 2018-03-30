namespace MobileStats.AppCenter.Api
{
    class AppCenter
    {
        private readonly string authToken;

        public AppCenter(string apiToken)
        {
            authToken = apiToken;
        }

        public App App(string owner, string appName) => new App(authToken, owner, appName);
    }
}
