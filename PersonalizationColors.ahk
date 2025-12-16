#Requires AutoHotkey v2.0.0+
;==============================================================
; PersonalizationColors — Windows personalization/theme settings reader (registry)
;
; GitHub: https://github.com/SevenKeyboard/personalization-colors
; Author: SevenKeyboard Ltd. (2025)
; License: The Unlicense
;==============================================================
class VersionManager_PersonalizationColors
{
    static _ := this._init()
    static _init()    {
        global
        PERSONALIZATIONCOLORS_VERSION := "1.0.0"
    }
}
class PersonalizationColors
{
    static DefaultWindowsMode => this._regReadThemes("SystemUsesLightTheme")?"Light":"Dark" ;  Choose your default Windows mode    기본 Windows 모드 선택
    static DefaultAppMode => this._regReadThemes("AppsUseLightTheme")?"Light":"Dark"        ;  Choose your default app mode    기본 앱 모드 선택
    static TransparencyEffects => this._regReadThemes("EnableTransparency")                 ;  Transparency effects    투명 효과
    static ShowAccentColorOnStartAndTaskbar => this._regReadThemes("ColorPrevalence")       ;  Show accent color on Start and taskbar    시작 및 작업 표시줄에 테마 컬러 표시
    static ShowAccentColorOnTitleBarsAndWindowsBorders    {                                 ;  Show accent color on title bars and windows borders    제목 표시줄 및 창 테두리에 테마 컬러 표시
        get  {
            try  {
                regValue:=regRead("HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\DWM", "ColorPrevalence")
            }  catch  {
                regValue:=false
            }
            return regValue
        }
    }
    static _regReadThemes(valueName)    {
        static keyName:="HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        try  {
            regValue := regRead(keyName, valueName)
        }  catch  {
            regValue := false
        }
        return regValue
    }
}