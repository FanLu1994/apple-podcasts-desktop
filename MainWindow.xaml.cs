using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace PodcastDesktop;

public partial class MainWindow : Window
{
    private readonly ThumbButtonInfo _backButton;
    private readonly ThumbButtonInfo _toggleButton;
    private readonly ThumbButtonInfo _forwardButton;
    private readonly ImageSource _playIcon;
    private readonly ImageSource _pauseIcon;

    public bool AllowClose { get; set; }

    public event Action<bool, string>? PlaybackStateChanged;

    public MainWindow()
    {
        InitializeComponent();

        var brush = SystemUsesLightTheme() ? new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)) : Brushes.White;
        _playIcon = CreateIcon("M4,2 L14,8 L4,14 Z", brush);
        _pauseIcon = CreateIcon("M3,2 H7 V14 H3 Z M9,2 H13 V14 H9 Z", brush);

        _backButton = CreateThumbButton("后退 15 秒", CreateIcon("M8,3 L1,8 L8,13 Z M15,3 L8,8 L15,13 Z", brush), Commands.Back);
        _toggleButton = CreateThumbButton("播放", _playIcon, Commands.Toggle);
        _forwardButton = CreateThumbButton("前进 30 秒", CreateIcon("M1,3 L8,8 L1,13 Z M8,3 L15,8 L8,13 Z", brush), Commands.Forward);

        TaskbarItemInfo = new TaskbarItemInfo
        {
            ThumbButtonInfos = new ThumbButtonInfoCollection { _backButton, _toggleButton, _forwardButton },
        };
    }

    public async Task InitializeWebViewAsync(string startCommand)
    {
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PodcastDesktop", "WebView2");
        var options = new CoreWebView2EnvironmentOptions("--autoplay-policy=no-user-gesture-required");
        var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
        await WebView.EnsureCoreWebView2Async(environment);

        await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(LoadPlayerScript());
        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

        var url = Commands.PageUrls.TryGetValue(startCommand, out var pageUrl) ? pageUrl : Commands.PageUrls[Commands.Home];
        WebView.CoreWebView2.Navigate(url);
    }

    public async Task ExecuteAsync(string command)
    {
        switch (command)
        {
            case Commands.Toggle:
            case Commands.Back:
            case Commands.Forward:
                await RunPlayerScriptAsync($"window.__podcastDesktop && window.__podcastDesktop.{command}()");
                break;
            case Commands.Home:
            case Commands.Latest:
            case Commands.Shows:
                BringToFront();
                var handled = await RunPlayerScriptAsync($"window.__podcastDesktop && window.__podcastDesktop.navigate('{command}')");
                if (handled != "true")
                {
                    WebView.CoreWebView2?.Navigate(Commands.PageUrls[command]);
                }
                break;
            case Commands.Show:
                BringToFront();
                break;
        }
    }

    public void BringToFront()
    {
        if (!IsVisible)
        {
            Show();
        }
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }
        Activate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!AllowClose)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnClosing(e);
    }

    private async Task<string> RunPlayerScriptAsync(string script)
    {
        if (WebView.CoreWebView2 == null)
        {
            return "null";
        }
        return await WebView.CoreWebView2.ExecuteScriptAsync(script);
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        using var doc = JsonDocument.Parse(e.WebMessageAsJson);
        var root = doc.RootElement;
        if (root.GetProperty("type").GetString() != "state")
        {
            return;
        }

        var playing = root.GetProperty("playing").GetBoolean();
        var title = root.GetProperty("title").GetString() ?? string.Empty;

        _toggleButton.ImageSource = playing ? _pauseIcon : _playIcon;
        _toggleButton.Description = playing ? "暂停" : "播放";
        PlaybackStateChanged?.Invoke(playing, title);
    }

    private ThumbButtonInfo CreateThumbButton(string description, ImageSource icon, string command)
    {
        var button = new ThumbButtonInfo { Description = description, ImageSource = icon };
        button.Click += async (_, _) => await ExecuteAsync(command);
        return button;
    }

    private static ImageSource CreateIcon(string pathData, Brush brush)
    {
        var group = new DrawingGroup();
        group.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, 16, 16))));
        group.Children.Add(new GeometryDrawing(brush, null, Geometry.Parse(pathData)));
        var image = new DrawingImage(group);
        image.Freeze();
        return image;
    }

    private static string LoadPlayerScript()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("player.js")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static bool SystemUsesLightTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("SystemUsesLightTheme") is int value && value == 1;
    }
}
