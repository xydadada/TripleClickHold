#define AppName "TripleClickHold"
#define AppVersion "1.1.0"
#ifndef SourceDir
#define SourceDir "..\publish"
#endif
#ifndef OutputDir
#define OutputDir "."
#endif

[Setup]
AppId={{A3D5AF20-9E3C-4E02-8E6D-7CE8B1C32B2F}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=xydadada
AppPublisherURL=https://github.com/xydadada/TripleClickHold
AppSupportURL=https://github.com/xydadada/TripleClickHold/issues
DefaultDirName={localappdata}\Programs\TripleClickHold
DefaultGroupName={#AppName}
UninstallDisplayName={#AppName}
OutputDir={#OutputDir}
OutputBaseFilename=TripleClickHold-Setup-v{#AppVersion}
SetupIconFile={#SourceDir}\TripleClickHold.ico
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
DisableProgramGroupPage=yes
Uninstallable=yes

[Files]
Source: "{#SourceDir}\TripleClickHold.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\TripleClickHold.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
Source: "{#SourceDir}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autodesktop}\三倍点击保持器"; Filename: "{app}\TripleClickHold.exe"; WorkingDir: "{app}"; IconFilename: "{app}\TripleClickHold.ico"
Name: "{group}\三倍点击保持器"; Filename: "{app}\TripleClickHold.exe"; WorkingDir: "{app}"; IconFilename: "{app}\TripleClickHold.ico"
Name: "{group}\卸载三倍点击保持器"; Filename: "{uninstallexe}"

[UninstallDelete]
; 保留 %LocalAppData%\\TripleClickHold\\settings.json，卸载后重新安装可继续使用原设置。
Type: filesandordirs; Name: "{app}"
