using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Scanlink.Services;

public class AuthService
{
    private const string ApiUrl = "https://scanlink-d0ed5.web.app/api/verify";
    private static readonly string TokenPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Scanlink", ".auth");

    private static readonly HttpClient Http = new();

    /// <summary>
    /// 서버에 인증코드 검증 요청
    /// </summary>
    public async Task<(bool valid, string message)> VerifyAsync(string code)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { code });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await Http.PostAsync(ApiUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<VerifyResponse>(body);
            return (result?.valid ?? false, result?.message ?? "알 수 없는 오류");
        }
        catch (HttpRequestException)
        {
            return (false, "서버에 연결할 수 없습니다.");
        }
        catch (TaskCanceledException)
        {
            return (false, "서버 응답 시간이 초과되었습니다.");
        }
    }

    /// <summary>
    /// 로컬에 인증코드 저장
    /// </summary>
    public void SaveToken(string code)
    {
        var dir = Path.GetDirectoryName(TokenPath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
        File.WriteAllText(TokenPath, encoded);
        File.SetAttributes(TokenPath, FileAttributes.Hidden);
    }

    /// <summary>
    /// 로컬에 저장된 인증코드 읽기
    /// </summary>
    public string? LoadToken()
    {
        if (!File.Exists(TokenPath)) return null;

        try
        {
            var encoded = File.ReadAllText(TokenPath).Trim();
            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 저장된 인증코드 삭제
    /// </summary>
    public void ClearToken()
    {
        if (File.Exists(TokenPath))
            File.Delete(TokenPath);
    }

    private class VerifyResponse
    {
        public bool valid { get; set; }
        public string message { get; set; } = "";
    }
}
