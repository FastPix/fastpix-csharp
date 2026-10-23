using FastPix.Examples;

// Runs one example by name: `dotnet run -- <example>`.
// Most examples call the live API and need FASTPIX_USERNAME / FASTPIX_PASSWORD
// (see README.md). verify-webhook is the exception: it runs fully offline.

var examples = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
{
    ["create-upload"]     = CreateUpload.Run,
    ["verify-webhook"]    = VerifyWebhook.Run,
    ["sdk-init"]          = SdkInitialization.Run,
    ["video-management"]  = VideoManagement.Run,
    ["playback"]          = PlaybackManagement.Run,
    ["live-streaming"]    = LiveStreaming.Run,
    ["ai-features"]       = AiFeatures.Run,
    ["playlist"]          = PlaylistManagement.Run,
    ["cloud-playout"]     = CloudPlayout.Run,
    ["error-handling"]    = ErrorHandling.Run,
};

if (args.Length == 0 || !examples.TryGetValue(args[0], out var run))
{
    Console.WriteLine("Usage: dotnet run -- <example>\n\nExamples:");
    foreach (var name in examples.Keys)
    {
        Console.WriteLine($"  {name}");
    }
    return args.Length == 0 ? 0 : 1;
}

await run();
return 0;
