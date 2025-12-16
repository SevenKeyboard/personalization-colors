#Requires AutoHotkey v1.1.0+
;==============================================================
; PersonalizationColors — Windows personalization/theme settings reader (registry)
;
; GitHub: https://github.com/SevenKeyboard/personalization-colors
; Author: SevenKeyboard Ltd. (2025)
; License: The Unlicense
;==============================================================
class VersionManager_PersonalizationColors
{
    static _ := VersionManager_PersonalizationColors._init()
    _init()    {
        global
        PERSONALIZATIONCOLORS_VERSION := "1.0.0"
    }
}
class PersonalizationColors
{
    DefaultWindowsMode    { ;  Choose your default Windows mode    기본 Windows 모드 선택
        get  {
            return this._regReadThemes("SystemUsesLightTheme")?"Light":"Dark"
        }
    }
    DefaultAppMode    { ;  Choose your default app mode    기본 앱 모드 선택
        get  {
            return this._regReadThemes("AppsUseLightTheme")?"Light":"Dark"
        }
    }
    TransparencyEffects    { ;  Transparency effects    투명 효과
        get  {
            return this._regReadThemes("EnableTransparency")
        }
    }
    ShowAccentColorOnStartAndTaskbar    { ;  Show accent color on Start and taskbar    시작 및 작업 표시줄에 테마 컬러 표시
        get  {
            return this._regReadThemes("ColorPrevalence")
        }
    }
    ShowAccentColorOnTitleBarsAndWindowsBorders    { ;  Show accent color on title bars and windows borders    제목 표시줄 및 창 테두리에 테마 컬러 표시
        get  {
            regRead regValue, HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\DWM, ColorPrevalence
            return (ErrorLevel?false:regValue)
        }
    }
    _regReadThemes(valueName)    {
        static keyName:="HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        regRead regValue, % keyName, % valueName
        return (ErrorLevel?false:regValue)
    }
}