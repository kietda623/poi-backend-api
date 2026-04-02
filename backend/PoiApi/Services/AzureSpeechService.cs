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
        _key = config["Azure:SpeechKey"]!;
        _region = config["Azure:SpeechRegion"]!;
        _env = env;
        _logger = logger;
    }

    // Map language code → Azure voice name
    private static string GetVoiceName(string langCode) => langCode.ToLower() switch
    {
        "vi" => "vi-VN-HoaiMyNeural",   // Vietnamese female
        "en" => "en-US-JennyNeural",    // English female
        "fr" => "fr-FR-DeniseNeural",   // French female
        "ja" => "ja-JP-NanamiNeural",   // Japanese female
        _ => "vi-VN-HoaiMyNeural"    // default Vietnamese
    };

    public async Task<string?> GenerateAudioAsync(
        int poiId, string langCode, string text)
    {
        try
        {
            var config = SpeechConfig.FromSubscription(_key, _region);
            config.SpeechSynthesisVoiceName = GetVoiceName(langCode);

            // Save to wwwroot/audio/
            var audioDir = Path.Combine(_env.WebRootPath, "audio");
            Directory.CreateDirectory(audioDir);

            var fileName = $"poi_{poiId}_{langCode}.mp3";
            var filePath = Path.Combine(audioDir, fileName);

            using var audioConfig = AudioConfig.FromWavFileOutput(filePath);
            using var synthesizer = new SpeechSynthesizer(config, audioConfig);

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