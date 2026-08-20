# KeyPressMirror
Quick and Dirty UI to send a string to the cursor as if it came from the keyboard (useful for anti-copy/paste apps/sites)

## Run

This is a .NET 10 Avalonia desktop app. Run it with:

```bash
dotnet run
```

Saved phrases are stored in the user's application data folder. Windows uses native Unicode keyboard input. Linux requires `xdotool` to be installed and available on `PATH`.
