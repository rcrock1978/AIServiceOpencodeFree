using AiService.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace AiService.Services;

public class VoiceService : IVoiceService
{
    private readonly AzureSpeechOptions _options;
    private readonly bool _isConfigured;

    public VoiceService(AzureSpeechOptions options)
    {
        _options = options;
        _isConfigured = !string.IsNullOrEmpty(options.ApiKey);
    }

    public async Task<string> TranscribeAsync(byte[] audioData)
    {
        if (!_isConfigured)
            return "Speech-to-text is not configured. Please configure Azure Speech services.";

        try
        {
            var speechConfig = SpeechConfig.FromSubscription(_options.ApiKey, _options.Region);
            using var audioStream = AudioInputStream.CreatePushStream();
            audioStream.Write(audioData, audioData.Length);
            audioStream.Close();

            using var recognizer = new SpeechRecognizer(speechConfig, AudioConfig.FromStreamInput(audioStream));
            var result = await recognizer.RecognizeOnceAsync();

            return result.Reason == ResultReason.RecognizedSpeech
                ? result.Text
                : "Could not recognize speech.";
        }
        catch (Exception ex)
        {
            return $"Speech recognition failed: {ex.Message}";
        }
    }

    public async Task<byte[]> SynthesizeAsync(string text)
    {
        if (!_isConfigured)
            return [];

        try
        {
            var speechConfig = SpeechConfig.FromSubscription(_options.ApiKey, _options.Region);
            using var synthesizer = new SpeechSynthesizer(speechConfig);
            var ssml = $"""
                <speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="en-US">
                    <voice name="en-US-AriaNeural">
                        <prosody rate="0%" pitch="0%">
                            {System.Security.SecurityElement.Escape(text)}
                        </prosody>
                    </voice>
                </speak>
                """;

            var result = await synthesizer.SpeakSsmlAsync(ssml);
            return result.Reason == ResultReason.SynthesizingAudioCompleted
                ? result.AudioData
                : [];
        }
        catch
        {
            return [];
        }
    }
}
