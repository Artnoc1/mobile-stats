namespace MobileStats.AppCenter.Api
{
    class AppCenter
    {
        private readonly BaseApi api;

        public AppCenter(string apiToken)
        {
            api = new BaseApi(apiToken);
        }

        public App App(string owner, string appName) => new App(api, owner, appName);
    }
}
