#define AppName "Apple 播客"
#define AppVersion "0.2.0"
#define AppExe "PodcastDesktop.exe"
#define RunValueName "ApplePodcastsDesktop"

[Setup]
AppId={{6893E350-C97A-4ECA-B627-A037524E1CE3}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=FanLu1994
AppPublisherURL=https://github.com/FanLu1994/apple-podcasts-desktop
DefaultDirName={autopf}\ApplePodcastsDesktop
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\publish\installer
OutputBaseFilename=ApplePodcastsDesktop-Setup-v{#AppVersion}
SetupIconFile=..\Assets\app.ico
UninstallDisplayIcon={app}\{#AppExe}
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
CloseApplications=yes

[Languages]
Name: "chs"; MessagesFile: "ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "开机自动启动"; GroupDescription: "其他选项："; Flags: unchecked

[Files]
Source: "..\publish\self-contained\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#RunValueName}"; ValueData: """{app}\{#AppExe}"""; Flags: uninsdeletevalue; Tasks: autostart
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "{#RunValueName}"; Flags: deletevalue; Tasks: not autostart

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
