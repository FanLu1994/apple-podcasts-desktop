using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PodcastDesktop;

internal sealed class GlobalHotkeys : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;

    private readonly HwndSource _source;
    private readonly Dictionary<int, string> _commands = new();

    public event Action<string>? Pressed;

    public GlobalHotkeys(Window window)
    {
        var handle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(handle);
        _source.AddHook(WndProc);
    }

    public bool RegisterCtrlAlt(Key key, string command)
    {
        var id = _commands.Count + 1;
        var vk = (uint)KeyInterop.VirtualKeyFromKey(key);
        if (!RegisterHotKey(_source.Handle, id, ModControl | ModAlt | ModNoRepeat, vk))
        {
            return false;
        }
        _commands[id] = command;
        return true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && _commands.TryGetValue(wParam.ToInt32(), out var command))
        {
            Pressed?.Invoke(command);
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        foreach (var id in _commands.Keys)
        {
            UnregisterHotKey(_source.Handle, id);
        }
        _source.RemoveHook(WndProc);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
