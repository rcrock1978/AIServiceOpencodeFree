namespace AiService.Services;

public interface IVoiceService
{
    Task<string> TranscribeAsync(byte[] audioData);
    Task<byte[]> SynthesizeAsync(string text);
}
