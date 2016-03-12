Imports Storm
Imports Storm.ExternalEvent
Imports Storm.StardewValley.Event

Imports System.IO
Imports System.Environment

Imports Newtonsoft.Json

Namespace CheatersToggles
    <[Mod]>
    Public Class CheatersToggles
        Inherits DiskResource

        Dim TimePause As Boolean = True 'When True, Time Passes
        Dim GameLoaded As Boolean = False 'Lets not do anything until a Save is loaded.

        Dim Settings As New Settings

        Dim CurrentSettingsVer As Integer = 2

        <Subscribe>
        Public Sub LoadConfigEvent([Event] As PostGameLoadedEvent)
            GameLoaded = True                       'We don't want to run any cheats outside of the main game (on the main menu)
            WriteLog("Game Loaded, Mod Enabled.")   'No idea if it has side effects, but better safe than sorry imo.
        End Sub

        <Subscribe>
        Public Sub GameStartedEvent([Event] As InitializeEvent)
            LoadConfig()                            'Loading config when the game first loads is good practice imo.
        End Sub

        Public Sub LoadConfig(Optional ByVal isReload As Boolean = False)
            Dim ConfigPath As String = GetFolderPath(SpecialFolder.ApplicationData)
            ConfigPath += "\StardewValley\Mods\CheatersToggles\config.json"
            If File.Exists(ConfigPath) = False Then
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Settings, Formatting.Indented))
                WriteLog("Created Config File, Please edit and restart the game.")
            Else
                Settings = JsonConvert.DeserializeObject(Of Settings)(File.ReadAllText(ConfigPath))
                If isReload = True Then
                    WriteLog("Config File Reloaded")
                Else
                    WriteLog("Config File Loaded")
                End If
            End If
            If Settings.ConfigVersionDontChange <> CurrentSettingsVer Then
                File.Delete(ConfigPath)
                Settings.ConfigVersionDontChange = CurrentSettingsVer
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Settings, Formatting.Indented))
                WriteLog("Updated Config File, please edit and restart the game.")
            End If
        End Sub

        <Subscribe>
        Public Sub StopTime([Event] As ShouldTimePassEvent)
            If GameLoaded = True Then
                [Event].ReturnValue = TimePause
            End If
        End Sub

        Public Sub WriteLog(ByVal Message As String)
            Logging.Log("[Cheaters Toggles] " & Message)
        End Sub


        <Subscribe>
        Public Sub HoldHealth([Event] As FarmerDamageEvent)
            If Settings.MaxedHealth.Enabled Then
                [Event].ReturnEarly = True
            End If
        End Sub

        <Subscribe>
        Public Sub HoldStamina([Event] As PostRenderEvent)
            If GameLoaded = True Then
                Try
                    If Settings.MaxedStamina.Enabled = True Then
                        [Event].LocalPlayer.Stamina = [Event].LocalPlayer.MaxStamina
                    End If
                    If Settings.SpeedMultiplier.Enabled = True Then
                        [Event].LocalPlayer.AddedSpeed = Settings.SpeedMultiplier.SpeedMultiplier
                    End If
                Catch ex As Exception
                    WriteLog(ex.ToString)
                End Try
            End If
        End Sub

        <Subscribe>
        Public Sub GetKeys([Event] As KeyPressedEvent)
            If GameLoaded = True Then
                'Logging.Log([Event].Key)
                '==========================Time Pause==================================
                If Settings.Timepause.Enabled = True Then
                    If [Event].Key = Settings.Timepause.ToggleKey Then
                        If TimePause = True Then
                            TimePause = False
                            WriteLog("Pausing time...")
                        Else
                            TimePause = True
                            WriteLog("Unpausing time...")
                        End If
                    End If
                End If
                '=========================Weather Control=============================
                If Settings.WeatherControl.Enabled = True Then
                    If [Event].Key = Settings.WeatherControl.NextWeatherKey Then
                        If [Event].Root.WeatherForTomorrow = 3 Then
                            [Event].Root.WeatherForTomorrow = 0
                        Else
                            [Event].Root.WeatherForTomorrow += 1
                        End If
                        Dim Weather As String = ""
                        Select Case [Event].Root.WeatherForTomorrow
                            Case 0
                                Weather = "Sunny"
                            Case 1
                                Weather = "Rainy"
                            Case 2
                                Weather = "Partially Cloudy, Expect Pollen"
                            Case 3
                                Weather = "Storm, Thunder and Lightning"
                        End Select
                        WriteLog("Set weather for tomorow to: " & Weather)
                    End If
                End If

                '==========================Money Manager===========================
                If Settings.MoneyManager.Enabled = True Then
                    'Logging.Log([Event].Root.Player.Money)
                    If [Event].Key = Settings.MoneyManager.GiveMoneyKey Then
                        [Event].Root.Player.Money += Settings.MoneyManager.Amount
                        [Event].Root.Player.TotalMoneyEarned += Settings.MoneyManager.Amount
                        WriteLog("Money Added. New total: " & [Event].Root.Player.Money)
                    ElseIf [Event].Key = Settings.MoneyManager.RemoveMoneyKey Then
                        Try
                            [Event].Root.Player.Money += (Settings.MoneyManager.Amount - (Settings.MoneyManager.Amount * 2))
                        Catch ex As Exception
                            [Event].Root.Player.Money = 0
                        End Try
                        Try
                            [Event].Root.Player.TotalMoneyEarned += (Settings.MoneyManager.Amount - (Settings.MoneyManager.Amount * 2))
                        Catch ex As Exception
                            [Event].Root.Player.Money = 0
                        End Try
                        WriteLog("Money Removed.")
                    End If
                End If
                '==========================Luck Manager!==========================
                'If Settings.Luck.Enabled = True Then
                '    If [Event].Key = Settings.Luck.NextLuckKey Then
                '        'Logging.Log([Event].Root.DailyLuck)
                '        [Event].Root.DailyLuck += 0.1
                '        Logging.Log([Event].Root.DailyLuck)
                '    End If
                'End If
                If Settings.LiveReloadConfig = True Then
                    If [Event].Key = Settings.ReloadKey Then
                        LoadConfig(True)
                    End If
                End If
            End If
        End Sub
    End Class

    Public Class Settings
        Public Property Timepause As New TimePause
        Public Property WeatherControl As New WeatherSet
        Public Property MoneyManager As New GiveMoney
        'Public Property Luck As New Luck
        Public Property MaxedHealth As New Health
        Public Property MaxedStamina As New Stamina
        Public Property SpeedMultiplier As New SpeedRunning
        Public Property LiveReloadConfig As Boolean = True
        Public Property ReloadKey As Integer = 82
        Public Property ConfigVersionDontChange As Integer = 0
    End Class

    Public Class TimePause
        Public Property Enabled As Boolean = False
        Public Property ToggleKey As Integer = 32           'Defaults to Space
    End Class

    Public Class WeatherSet
        Public Property Enabled As Boolean = False
        Public Property NextWeatherKey As Integer = 78      'Defaults to N
    End Class

    Public Class GiveMoney
        Public Property Enabled As Boolean = False
        Public Property GiveMoneyKey As Integer = 107       'Numpad +
        Public Property RemoveMoneyKey As Integer = 109     'Numpad -
        Public Property Amount As Integer = 5000
    End Class

    Public Class Luck
        Public Property Enabled As Boolean = False
        Public Property NextLuckKey As Integer = 76         'L key
    End Class

    Public Class Health
        Public Property Enabled As Boolean = False
    End Class

    Public Class Stamina
        Public Property Enabled As Boolean = False
    End Class

    Public Class SpeedRunning
        Public Property Enabled As Boolean = False
        Public Property SpeedMultiplier As Integer = 2
    End Class
End Namespace

'Things to do:

'Increased Speed
