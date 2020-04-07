namespace MobileStats
{
    interface IConfiguration
    {
        string BitriseApiToken { get; }
        string BitriseAppSlugs { get; }
        
        string AppCenterApiToken { get; }
        string AppCenterOwner { get; }
        string AppCenterApps { get; }
        
        string AppFiguresUser { get; }
        string AppFiguresPassword { get; }
        string AppFiguresClientKey { get; }
        string AppFiguresProductIds { get; }
    }
}