using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KeyPressMirror.Models;
using KeyPressMirror.Services;

namespace KeyPressMirror;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<NamedString> savedStrings = [];
    private readonly string storagePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "KeyPressMirror",
        "saved-strings.json");

    public MainWindow()
    {
        InitializeComponent();
        SavedPhrases.ItemsSource = savedStrings;
        LoadSavedStrings();
        PhraseSelector.Focus();
    }

    private void PhraseSelector_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PhraseSelector.SelectedItem is NamedString selected)
            PhraseSelector.Text = selected.Value;
    }

    private void SavedPhrases_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SavedPhrases.SelectedItem is NamedString selected)
            PhraseSelector.Text = selected.Value;
    }

    private async void SaveCurrent_OnClick(object? sender, RoutedEventArgs e)
    {
        var value = PhraseSelector.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            SetStatus("Enter a phrase before saving.");
            return;
        }

        var name = await PromptForNameAsync();
        if (string.IsNullOrWhiteSpace(name))
            return;

        var existing = savedStrings.FirstOrDefault(item =>
            string.Equals(item.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existing is null)
            savedStrings.Add(new NamedString { Name = name.Trim(), Value = value });
        else
            existing.Value = value;

        PersistSavedStrings();
        SetStatus($"Saved '{name.Trim()}'.");
    }

    private void Remove_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: NamedString item })
        {
            savedStrings.Remove(item);
            PersistSavedStrings();
            SetStatus($"Removed '{item.Name}'.");
        }
    }

    private async void SendToCursor_OnClick(object? sender, RoutedEventArgs e)
    {
        var value = PhraseSelector.Text ?? string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            SetStatus("Enter or select a phrase first.");
            return;
        }

        SetStatus("Sending...");
        await Task.Delay(150);
        try
        {
            await KeySender.SendAsync(value);
            SetStatus($"Sent {value.Length} {(value.Length == 1 ? "character" : "characters")}.");
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException or InvalidOperationException)
        {
            SetStatus(exception.Message);
        }
    }

    private async Task<string?> PromptForNameAsync()
    {
        var dialog = new TextInputDialog("Save phrase", "Name for this phrase:");
        await dialog.ShowDialog(this);
        return dialog.Result;
    }

    private void LoadSavedStrings()
    {
        try
        {
            if (File.Exists(storagePath))
            {
                var items = JsonSerializer.Deserialize<List<NamedString>>(File.ReadAllText(storagePath));
                if (items is not null)
                    foreach (var item in items.Where(item => !string.IsNullOrWhiteSpace(item.Name)))
                        savedStrings.Add(item);
            }
        }
        catch (JsonException)
        {
            SetStatus("Saved phrases could not be read and will be recreated.");
        }
    }

    private void PersistSavedStrings()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(storagePath)!);
        File.WriteAllText(storagePath, JsonSerializer.Serialize(savedStrings, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void SetStatus(string message) => StatusText.Text = message;
}