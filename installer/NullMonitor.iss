; Null Monitor installer (Inno Setup 6). Built by GitHub Actions:
;   ISCC.exe /DMyAppVersion=1.0.N installer\NullMonitor.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "Null Monitor"
#define MyAppExe "NullMonitor.exe"
#define MyAppUrl "https://github.com/PainFrenemy2/null-code"

[Setup]
AppId={{8F4B2C1E-6A3D-4E5F-9B7A-2C1D3E4F5A6B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher=painfrenemy
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}/issues
AppUpdatesURL={#MyAppUrl}/releases
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
VersionInfoCompany=painfrenemy
VersionInfoDescription={#MyAppName} Setup
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=auto
DisableReadyPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir=..\dist
OutputBaseFilename=NullMonitor-Setup
SetupIconFile=..\src\NullMonitor\Assets\app.ico
UninstallDisplayIcon={app}\{#MyAppExe}
UninstallDisplayName={#MyAppName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=force
RestartApplications=no

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
russian.AutostartTask=Запускать вместе с Windows
english.AutostartTask=Start with Windows
russian.ExtraGroup=Дополнительно:
english.ExtraGroup=Additional options:

[Tasks]
Name: "autostart"; Description: "{cm:AutostartTask}"; GroupDescription: "{cm:ExtraGroup}"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\{#MyAppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExe}"; Tasks: desktopicon

[Run]
; Register or remove the elevated logon task according to the checkbox.
Filename: "{app}\{#MyAppExe}"; Parameters: "--register-autostart"; Flags: runhidden waituntilterminated; Tasks: autostart
Filename: "{app}\{#MyAppExe}"; Parameters: "--unregister-autostart"; Flags: runhidden waituntilterminated; Tasks: not autostart
; Also runs after silent auto-updates, so the app comes back by itself.
Filename: "{app}\{#MyAppExe}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall

[UninstallRun]
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM {#MyAppExe}"; Flags: runhidden waituntilterminated; RunOnceId: "KillApp"
Filename: "{sys}\schtasks.exe"; Parameters: "/Delete /TN NullMonitor /F"; Flags: runhidden waituntilterminated; RunOnceId: "DeleteTask"

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\NullMonitor"
Type: filesandordirs; Name: "{userappdata}\NullMonitor"
