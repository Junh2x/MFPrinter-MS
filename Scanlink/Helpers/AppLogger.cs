using System.Diagnostics;
using System.IO;

namespace Scanlink.Helpers;

/// <summary>
/// 앱 전역 파일 로거.
/// %LocalAppData%\Scanlink\logs\ 에 날짜별 로그 파일 생성.
/// </summary>
public static class AppLogger
{
    private static readonly string LogDir = Path.Combine(
        Directory.GetCurrentDirectory(), "logs");

    private static readonly object Lock = new();

    private static string LogFile => Path.Combine(LogDir, $"{DateTime.Now:yyyy-MM-dd}.log");

    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        Debug.WriteLine(line);

        lock (Lock)
        {
            try
            {
                if (!Directory.Exists(LogDir))
                    Directory.CreateDirectory(LogDir);
                File.AppendAllText(LogFile, line + Environment.NewLine);
            }
            catch { /* 로그 실패는 무시 */ }
        }
    }

    public static void Log(string tag, string message) => Log($"[{tag}] {message}");

    public static void Error(string tag, string message, Exception? ex = null)
    {
        Log($"[{tag}][ERROR] {message}");
        if (ex != null)
            Log($"[{tag}][ERROR] {ex.GetType().Name}: {ex.Message}");
    }
}
