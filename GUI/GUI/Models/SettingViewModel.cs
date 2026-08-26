using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace GUI.Models;

public partial class SettingViewModel : INotifyPropertyChanged
{
    private readonly ApplicationDataContainer _settings = ApplicationData.Current.LocalSettings;

    private string _selectedLanguage = string.Empty;
    private string _cliExecutablePath = string.Empty;
    private string _defaultNbtEdition = "java";
    private bool _needRestartAfterLanguageChange;

    public List<string> AvailableLanguages { get; } = ["en‑US", "zh‑CN"];
    public List<string> EditionOptions { get; } = ["java", "bedrock"];

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage == value) return;
            _selectedLanguage = value;
            _needRestartAfterLanguageChange = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NeedRestartAfterLanguageChange));
            SaveToLocalSettings();
        }
    }

    public bool NeedRestartAfterLanguageChange => _needRestartAfterLanguageChange;

    public string CliExecutablePath
    {
        get => _cliExecutablePath;
        set
        {
            _cliExecutablePath = value;
            OnPropertyChanged();
            SaveToLocalSettings();
        }
    }

    public string DefaultNbtEdition
    {
        get => _defaultNbtEdition;
        set
        {
            _defaultNbtEdition = value;
            OnPropertyChanged();
            SaveToLocalSettings();
        }
    }

    public SettingViewModel()
    {
        LoadFromLocalSettings();
    }

    public void LoadFromLocalSettings()
    {
        _selectedLanguage = _settings.Values.TryGetValue(nameof(SelectedLanguage), out var langObj)
            ? (string)langObj
            : "en‑US";

        _cliExecutablePath = _settings.Values.TryGetValue(nameof(CliExecutablePath), out var cliObj)
            ? (string)cliObj
            : "MCNbtCli.exe";

        _defaultNbtEdition = _settings.Values.TryGetValue(nameof(DefaultNbtEdition), out var edObj)
            ? (string)edObj
            : "java";

        _needRestartAfterLanguageChange = false;
    }

    public void SaveToLocalSettings()
    {
        _settings.Values[nameof(SelectedLanguage)] = SelectedLanguage;
        _settings.Values[nameof(CliExecutablePath)] = CliExecutablePath;
        _settings.Values[nameof(DefaultNbtEdition)] = DefaultNbtEdition;
    }

    public void ResetAll()
    {
        _settings.Values.Clear();
        LoadFromLocalSettings();
        _needRestartAfterLanguageChange = false;
        OnPropertyChanged(string.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
