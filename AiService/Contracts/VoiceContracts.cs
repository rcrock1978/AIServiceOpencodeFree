namespace AiService.Contracts;

public record SttRequest(byte[] AudioData);

public record SttResponse(string Transcript);

public record TtsRequest(string Text);

public record TtsResponse(byte[] AudioData);
