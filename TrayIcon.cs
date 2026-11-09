using System.Windows;
using Forms = System.Windows.Forms;

namespace PodcastDesktop;

internal sealed class TrayIcon : IDisposable
{
    private const string AppName = "Apple 播客";

    private readonly Forms.NotifyIcon _icon;
    private readonly Forms.ToolStripMenuItem _toggleItem;

    public event Action<string>? CommandInvoked;
    public event Action? ExitRequested;

    public TrayIcon()
    {
        var stream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/app.ico")).Stream;
        _toggleItem = new Forms.ToolStripMenuItem("播放", null, (_, _) => CommandInvoked?.Invoke(Commands.Toggle));

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add(_toggleItem);
        menu.Items.Add("后退 15 秒", null, (_, _) => CommandInvoked?.Invoke(Commands.Back));
        menu.Items.Add("前进 30 秒", null, (_, _) => CommandInvoked?.Invoke(Commands.Forward));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("显示窗口", null, (_, _) => CommandInvoked?.Invoke(Commands.Show));
        menu.Items.Add("退出", null, (_, _) => ExitRequested?.Invoke());

        _icon = new Forms.NotifyIcon
        {
            Icon = new System.Drawing.Icon(stream, Forms.SystemInformation.SmallIconSize),
            Text = AppName,
            ContextMenuStrip = menu,
            Visible = true,
        };
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                CommandInvoked?.Invoke(Commands.Show);
            }
        };
    }

    public void UpdateState(bool playing, string title)
    {
        _toggleItem.Text = playing ? "暂停" : "播放";
        var text = string.IsNullOrEmpty(title) ? AppName : $"{AppName} - {title}";
        _icon.Text = text.Length > 127 ? text[..127] : text;
    }

    public void ShowMessage(string message)
    {
        _icon.ShowBalloonTip(5000, AppName, message, Forms.ToolTipIcon.Warning);
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
