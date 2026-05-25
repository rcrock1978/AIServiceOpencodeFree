using AiService.Contracts;
using AiService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiService.Endpoints;

public static class VoiceEndpoints
{
    public static void MapVoiceEndpoints(this WebApplication app)
    {
        app.MapPost("/voice/stt", async (HttpRequest httpRequest, [FromServices] IVoiceService voiceService) =>
        {
            if (!httpRequest.HasFormContentType)
            {
                using var ms = new MemoryStream();
                await httpRequest.Body.CopyToAsync(ms);
                var audioData = ms.ToArray();
                var transcript = await voiceService.TranscribeAsync(audioData);
                return Results.Ok(new SttResponse(transcript));
            }

            var file = httpRequest.Form.Files.GetFile("audio");
            if (file == null)
            {
                return Results.BadRequest("No audio file provided.");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var audioBytes = stream.ToArray();
            var result = await voiceService.TranscribeAsync(audioBytes);
            return Results.Ok(new SttResponse(result));
        });

        app.MapPost("/voice/tts", async (TtsRequest request, [FromServices] IVoiceService voiceService) =>
        {
            var audioData = await voiceService.SynthesizeAsync(request.Text);
            return Results.File(audioData, "audio/wav");
        });
    }
}
