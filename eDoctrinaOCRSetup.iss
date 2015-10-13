; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "eDoctrina OCR"
#define MyAppVersion "1.0.0.271"
#define MyAppPublisher "eDoctrina Corp"
#define MyAppURL "http://www.edoctrina.org/"
;#define MyAppExeName "eDoctrinaOcr.exe"
#define MyAppExeName "eDoctrinaOcrWPF.exe"
#define MySourcePath "D:\eDoctrina\eDoctrinaOcr"
#define MyOutputDir "D:\eDoctrina\publish\"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{2408BB48-8F37-4342-BE5B-69E3B0812E05}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName}.{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=true
OutputDir={#MyOutputDir}
OutputBaseFilename=eDoctrinaOCRSetup.{#MyAppVersion}
SetupIconFile={#MySourcePath}\ico.ico
Compression=lzma
SolidCompression=true

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Dirs]
Name: {app}; Permissions: everyone-full

[Files]
;Source: {#MySourcePath}\Release\eDoctrinaOcr.exe; DestDir: {app}; Flags: replacesameversion restartreplace
;Source: {#MySourcePath}\Release\eDoctrinaOcr.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\eDoctrinaOcrWPF.exe; DestDir: {app}; Flags: replacesameversion restartreplace
Source: {#MySourcePath}\Release\eDoctrinaOcrWPF.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\eDoctrinaOcrEd.exe; DestDir: {app}; Flags: replacesameversion restartreplace
Source: {#MySourcePath}\Release\eDoctrinaOcrEd.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\eDoctrinaUtils.dll; DestDir: {app}; Flags: replacesameversion restartreplace
Source: {#MySourcePath}\Release\eDoctrinaUtils.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\eDoctrinaUtilsWPF.dll; DestDir: {app}; Flags: replacesameversion restartreplace
Source: {#MySourcePath}\Release\eDoctrinaUtilsWPF.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\PDFLibNet.dll; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\itextsharp.dll; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\BitMiracle.LibTiff.NET.dll; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\BitMiracle.LibTiff.NET.xml; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\zxing.dll; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\zxing.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\zxing.presentation.dll; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\zxing.presentation.pdb; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\zxing.presentation.xml; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\zxing.xml; DestDir: {app}; Flags: replacesameversion
Source: {#MySourcePath}\Release\Miniatures\*; DestDir: {app}\Miniatures\; Flags: replacesameversion
Source: {#MySourcePath}\Release\Configs\*; DestDir: {app}\Configs\; Flags: replacesameversion confirmoverwrite uninsneveruninstall onlyifdoesntexist
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\eDoctrina OCR Service; Filename: {app}\{#MyAppExeName}
Name: {group}\eDoctrina OCR; Filename: {app}\eDoctrinaOcrEd.exe
;Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {#MyAppURL}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}
Name: {commondesktop}\eDoctrina OCR Service; Filename: {app}\{#MyAppExeName}; Tasks: desktopicon
Name: {commondesktop}\eDoctrina OCR; Filename: {app}\eDoctrinaOcrEd.exe; Tasks: desktopicon
;Name: {commondesktop}\eDoctrina OCR Uninstall; Filename: {uninstallexe}; Tasks: desktopicon
;Name: {app}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {#MyAppURL}
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\eDoctrina OCR Service"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon
;Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\eDoctrina OCR"; Filename: "{app}\eDoctrinaOcrEd.exe"; Tasks: quicklaunchicon

[Run]
Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}; Flags: nowait postinstall skipifsilent
;Filename: "{app}\eDoctrinaOcrEd.exe"; Description: "{cm:LaunchProgram,eDoctrinaOcr Editor}"; Flags: nowait postinstall skipifsilent
[InstallDelete]
Name: {app}\Microsoft.VisualBasic.PowerPacks.Vs.dll; Type: files
Name: {app}\Microsoft.VisualBasic.PowerPacks.Vs.xml; Type: files
Name: {app}\eDoctrinaOcr.exe.config; Type: files
Name: {app}\eDoctrinaOcrEd.exe.config; Type: files
Name: {app}\*.bmp; Type: files
Name: {app}\*.tiff; Type: files
Name: {app}\*.url; Type: files
Name: {app}\eDoctrinaOcr.exe; Type: files
Name: {app}\eDoctrinaOcr.pdb; Type: files