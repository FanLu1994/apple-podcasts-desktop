# Apple 播客桌面版

把 [Apple 播客网页版](https://podcasts.apple.com/cn/home) 封装成 Windows 桌面应用。播放控制走网页播放器自己的按钮，选集仍在窗口里完成。

这是非官方封装，与 Apple 无关。

## 功能

- 任务栏预览按钮：后退 15 秒、播放/暂停、前进 30 秒
- 托盘图标和右键菜单；关闭窗口时缩到托盘，播放继续
- 全局快捷键：`Ctrl+Alt+空格` 播放/暂停，`Ctrl+Alt+←` 后退 15 秒，`Ctrl+Alt+→` 前进 30 秒
- 任务栏跳转列表：播放控制，以及快速打开主页、最新单集、节目

## 下载

安装包在 [Releases](https://github.com/FanLu1994/apple-podcasts-desktop/releases)。当前版本是 [v0.2.0](https://github.com/FanLu1994/apple-podcasts-desktop/releases/tag/v0.2.0)。

`ApplePodcastsDesktop-Setup-v0.2.0.exe` 是中文安装向导，只装给当前用户，不需要管理员权限。安装时可选：

- 创建桌面快捷方式
- 开机自动启动
- 安装完成后立即运行

装好后可在「设置 → 应用」里卸载。卸载不会删除登录数据。

需要 64 位 Windows，以及 Microsoft Edge WebView2 Runtime（Windows 11 自带）。安装包已包含 .NET 运行时。首次使用要在应用内登录 Apple ID。

## 本地构建

需要 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)。

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o publish\self-contained
```

生成安装包还需要 [Inno Setup 6](https://jrsoftware.org/isinfo.php)：

```powershell
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" installer\PodcastDesktop.iss
```

安装包输出到 `publish\installer`。
