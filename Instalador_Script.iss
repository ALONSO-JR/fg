; Script de Inno Setup para Sistema de Gestión CRA Chile

[Setup]
AppId={{2A89E624-918E-4A9B-B85E-5D3E836D6F41}
AppName=Sistema de Gestión CRA
AppVersion=1.0
AppPublisher=CRA Chile
DefaultDirName={autopf}\SistemaGestionCRA
DefaultGroupName=Sistema de Gestión CRA
OutputDir=.
OutputBaseFilename=Setup_SistemaCRA
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net9.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTA: Asegúrese de compilar en modo Release con --self-contained true antes de ejecutar este script.

[Icons]
Name: "{group}\Sistema de Gestión CRA"; Filename: "{app}\SistemaGestionCRA.exe"
Name: "{autodesktop}\Sistema de Gestión CRA"; Filename: "{app}\SistemaGestionCRA.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SistemaGestionCRA.exe"; Description: "{cm:LaunchProgram,Sistema de Gestión CRA}"; Flags: nowait postinstall skipifsilent
