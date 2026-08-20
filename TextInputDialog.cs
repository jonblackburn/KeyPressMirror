using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Interactivity;

namespace KeyPressMirror;

internal sealed class TextInputDialog : Window
{
    private readonly TextBox input = new() { MinWidth = 300 };
    private readonly Button saveButton = new() { Content = "Save", IsDefault = true };
    public string? Result { get; private set; }

    public TextInputDialog(string title, string prompt)
    {
        Title = title;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Padding = new Avalonia.Thickness(24);
        Content = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                new TextBlock { Text = prompt },
                input,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children =
                    {
                        new Button { Content = "Cancel", IsCancel = true },
                        saveButton
                    }
                }
            }
        };

        saveButton.Click += Save_OnClick;
        Opened += (_, _) => input.Focus();
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = input.Text;
        Close();
    }
}