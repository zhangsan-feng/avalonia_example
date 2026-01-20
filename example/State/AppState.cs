using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace example.State;

public abstract class AppState{
    public static AppStateService Global { get; } = new AppStateService();
}

public static class Logger {
    public static void Log<T>(T message, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = ""){
        string fileName = Path.GetFileName(file);
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string messageStr = message?.ToString() ?? "null";
        Console.WriteLine($"[{timestamp}] {fileName}:{line} [{member}] {messageStr}");
    }
}