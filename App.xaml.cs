using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace PodcastDesktop;

public partial class App : Application
{
    private SingleInstance? _singleInstance;
    private MainWindow? _window;
    private TrayIcon? _tray;
    private GlobalHotkeys? _hotkeys;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var command = Commands.Parse(e.Args);
        _singleInstance = new SingleInstance();
        if (!_singleInstance.IsPrimary)
        {
            SingleInstance.SendToPrimary(command);
            Shutdown();
            return;
        }

        _window = new MainWindow();
        _window.Show();

        _tray = new TrayIcon();
        _tray.CommandInvoked += cmd => _ = _window.ExecuteAsync(cmd);
        _tray.ExitRequested += ExitApplication;
        _window.PlaybackStateChanged += _tray.UpdateState;

        RegisterHotkeys();
        ApplyJumpList();

        _singleInstance.CommandReceived += cmd => Dispatcher.InvokeAsync(() => _window.ExecuteAsync(cmd));
        _singleInstance.StartListening();

        await _window.InitializeWebViewAsync(command);
    }

    private void RegisterHotkeys()
    {
        _hotkeys = new GlobalHotkeys(_window!);
        _hotkeys.Pressed += cmd => _ = _window!.ExecuteAsync(cmd);

        var failed = new List<string>();
        if (!_hotkeys.RegisterCtrlAlt(Key.Space, Commands.Toggle)) failed.Add("Ctrl+Alt+空格");
        if (!_hotkeys.RegisterCtrlAlt(Key.Left, Commands.Back)) failed.Add("Ctrl+Alt+←");
        if (!_hotkeys.RegisterCtrlAlt(Key.Right, Commands.Forward)) failed.Add("Ctrl+Alt+→");

        if (failed.Count > 0)
        {
            _tray!.ShowMessage($"以下快捷键已被其他程序占用：{string.Join("、", failed)}");
        }
    }

    private void ApplyJumpList()
    {
        var exe = Environment.ProcessPath!;
        JumpTask CreateTask(string title, string command, string category) => new()
        {
            Title = title,
            Arguments = $"--cmd {command}",
            ApplicationPath = exe,
            IconResourcePath = exe,
            CustomCategory = category,
        };

        var jumpList = new JumpList();
        jumpList.JumpItems.Add(CreateTask("播放/暂停", Commands.Toggle, "播放控制"));
        jumpList.JumpItems.Add(CreateTask("后退 15 秒", Commands.Back, "播放控制"));
        jumpList.JumpItems.Add(CreateTask("前进 30 秒", Commands.Forward, "播放控制"));
        jumpList.JumpItems.Add(CreateTask("主页", Commands.Home, "快速打开"));
        jumpList.JumpItems.Add(CreateTask("最新单集", Commands.Latest, "快速打开"));
        jumpList.JumpItems.Add(CreateTask("节目", Commands.Shows, "快速打开"));
        JumpList.SetJumpList(this, jumpList);
        jumpList.Apply();
    }

    private void ExitApplication()
    {
        _window!.AllowClose = true;
        _window.Close();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotkeys?.Dispose();
        _tray?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
