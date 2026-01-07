!include "MUI2.nsh"
!insertmacro MUI_LANGUAGE "Russian"

Name "AdvertControl"
Icon "AdControl.ScreenClient\AdvertControl_Icon.ico"
OutFile "Setup.exe"

InstallDir "$PROGRAMFILES\AdControl.ScreenClient"
InstallDirRegKey HKLM "Software\AdControl.ScreenClient" "Install_Dir"
RequestExecutionLevel admin

!define MIN_FREE_MB 400

; -------------------------
; Страницы
Page components
!include LogicLib.nsh
PageEx directory
  DirVerify leave
  PageCallbacks "" "" dirLeave
PageExEnd

Function dirLeave
  GetInstDirError $0
  ${Switch} $0
    ${Case} 0
      MessageBox MB_OK "valid installation directory"
      ${Break}
    ${Case} 1
      MessageBox MB_OK "invalid installation directory!"
      Abort
      ${Break}
    ${Case} 2
      MessageBox MB_OK "not enough free space!"
      Abort
      ${Break}
  ${EndSwitch}
FunctionEnd

Page instfiles
UninstPage uninstConfirm
UninstPage instfiles

Function .onInit
  # set required size of section 'test' to 100 bytes
  SectionSetSize ${firstSection} 409600
FunctionEnd

; -------------------------
Section "AdControl.ScreenClient (required)" firstSection
  SectionIn RO
  
  SetOutPath $INSTDIR

  ; Копирование всех файлов через xcopy
  nsExec::ExecToLog 'cmd /C xcopy /E /I /Y "$EXEDIR\AdControl.ScreenClient\bin\Release\net9.0\win-x64\publish\*" "$INSTDIR\"'

  ; Ярлык на рабочем столе
  CreateShortcut "$DESKTOP\AdControl.ScreenClient.lnk" "$INSTDIR\AdControl.ScreenClient.exe"

  ; Реестр
  WriteRegStr HKLM "Software\AdControl.ScreenClient" "Install_Dir" "$INSTDIR"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AdControl.ScreenClient" "DisplayName" "AdControl.ScreenClient"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AdControl.ScreenClient" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AdControl.ScreenClient" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AdControl.ScreenClient" "NoRepair" 1

  WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

Section "Start Menu Shortcuts (required)"
  SectionIn RO
  CreateDirectory "$SMPROGRAMS\AdControl.ScreenClient"
  CreateShortcut "$SMPROGRAMS\AdControl.ScreenClient\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortcut "$SMPROGRAMS\AdControl.ScreenClient\AdControl.ScreenClient.lnk" "$INSTDIR\AdControl.ScreenClient.exe" "" "$INSTDIR\AdControl.ScreenClient.exe" 0
SectionEnd

Section "Uninstall"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\AdControl.ScreenClient"
  DeleteRegKey HKLM "Software\AdControl.ScreenClient"
  Delete "$INSTDIR\AdControl.ScreenClient.exe"
  Delete "$INSTDIR\uninstall.exe"
  Delete "$DESKTOP\AdControl.ScreenClient.lnk"
  Delete "$SMPROGRAMS\AdControl.ScreenClient\*.*"
  RMDir "$SMPROGRAMS\AdControl.ScreenClient"
  RMDir "$INSTDIR"
SectionEnd
