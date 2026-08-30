#define MyAppName "Tabularius AI"
#ifndef MyAppVersion
  #define MyAppVersion "0.2.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\release\TabulariusAI-" + MyAppVersion + "-win-x64"
#endif
#ifndef OutputDir
  #define OutputDir "..\artifacts\release"
#endif

[Setup]
AppId={{5D7D1DBB-BFC0-4A3D-9D9A-87E11C30B6F7}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Rui Ribeiro
DefaultDirName={localappdata}\Programs\TabulariusAI
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir={#OutputDir}
OutputBaseFilename=TabulariusAI-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName={#MyAppName}

[Tasks]
Name: "desktopicon"; Description: "Criar atalho no Ambiente de Trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Dirs]
Name: "{app}\data"
Name: "{app}\logs"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\TabulariusAI.Web.exe"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\TabulariusAI.Web.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\TabulariusAI.Web.exe"; Description: "Iniciar {#MyAppName}"; Flags: nowait postinstall skipifsilent
