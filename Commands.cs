namespace PodcastDesktop;

internal static class Commands
{
    public const string Toggle = "toggle";
    public const string Back = "back";
    public const string Forward = "forward";
    public const string Home = "home";
    public const string Latest = "latest";
    public const string Shows = "shows";
    public const string Show = "show";

    public static readonly IReadOnlyDictionary<string, string> PageUrls = new Dictionary<string, string>
    {
        [Home] = "https://podcasts.apple.com/cn/home",
        [Latest] = "https://podcasts.apple.com/cn/library/latest-episodes",
        [Shows] = "https://podcasts.apple.com/cn/library/shows",
    };

    public static string Parse(string[] args)
    {
        var index = Array.IndexOf(args, "--cmd");
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : Show;
    }
}
