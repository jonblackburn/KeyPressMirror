using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyPressMirror.Services;

public static class KeySender
{
    public static async Task SendAsync(string text)
    {
        if (OperatingSystem.IsWindows())
        {
            SendWindows(text);
            return;
        }

        if (OperatingSystem.IsLinux() && await TrySendWithXdotoolAsync(text))
            return;

        throw new PlatformNotSupportedException(
            "Sending keys is supported on Windows and on Linux systems with xdotool installed.");
    }

    private static void SendWindows(string text)
    {
        var inputs = new INPUT[text.Length * 2];
        var index = 0;
        foreach (var character in text)
        {
            inputs[index++] = UnicodeInput(character, false);
            inputs[index++] = UnicodeInput(character, true);
        }

        if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) != inputs.Length)
            throw new InvalidOperationException("Windows did not accept the keyboard input.");
    }

    private static async Task<bool> TrySendWithXdotoolAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        var escaped = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        var startInfo = new ProcessStartInfo("sh", $"-c \"printf '%s' '{escaped}' | base64 -d | xdotool type --clearmodifiers --delay 1 --file -\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true
        };
        using var process = Process.Start(startInfo);
        if (process is null)
            return false;
        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    private static INPUT UnicodeInput(char character, bool keyUp) => new()
    {
        type = 1,
        U = new INPUTUNION
        {
            ki = new KEYBDINPUT
            {
                wVk = 0,
                wScan = character,
                dwFlags = 0x0004 | (keyUp ? 0x0002u : 0u)
            }
        }
    };

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInput);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }
}