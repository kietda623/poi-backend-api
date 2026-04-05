using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace PoiApi.Services;

public class AzureSpeechService
{
    private readonly string _key;
    private readonly string _region;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AzureSpeechService> _logger;

    public AzureSpeechService(
        IConfiguration config,
        IWebHostEnvironment env,
        ILogger<AzureSpeechService> logger)
    {
        _key    = config["Azure:SpeechKey"]!;
        _region = config["Azure:SpeechRegion"]!;
        _env    = env;
        _logger = logger;
    }

    private static string GetVoiceName(string langCode) => langCode.ToLower() switch
    {
        "vi" => "vi-VN-HoaiMyNeural",
        "en" => "en-US-JennyNeural",
        "zh" => "zh-CN-XiaoxiaoNeural",
        _    => "en-US-JennyNeural" // Default to English for unknown
    };

    public async Task<string?> GenerateAudioAsync(int poiId, string langCode, string text)
    {
        try
        {
            var speechConfig = SpeechConfig.FromSubscription(_key, _region);
            speechConfig.SpeechSynthesisVoiceName = GetVoiceName(langCode);
            speechConfig.SetSpeechSynthesisOutputFormat(
                SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

            var audioDir = Path.Combine(_env.WebRootPath, "audio");
            if (!Directory.Exists(audioDir)) Directory.CreateDirectory(audioDir);

            // 1. Dọn dẹp: Tìm các file cũ của POI này và Ngôn ngữ này để xóa
            // Format cũ: poi_{id}_{lang}.mp3
            // Format mới: poi_{id}_{lang}_*.mp3
            var searchPattern = $"poi_{poiId}_{langCode}*.mp3";
            var existingFiles = Directory.GetFiles(audioDir, searchPattern);
            foreach (var file in existingFiles)
            {
                try { File.Delete(file); } catch { /* Ignore */ }
            }

            // 2. Tạo tên file duy nhất (Unique) để vượt qua cache trình duyệt triệt để
            var fileName = $"poi_{poiId}_{langCode}_{Guid.NewGuid().ToString("N")[..8]}.mp3";
            var filePath = Path.Combine(audioDir, fileName);

            using var audioConfig = AudioConfig.FromWavFileOutput(filePath);
            using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

            var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                _logger.LogInformation("Audio generated: {File}", fileName);
                return $"/audio/{fileName}";
            }

            _logger.LogError("TTS failed: {Reason}", result.Reason);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AzureSpeechService error");
            return null;
        }
    }
}