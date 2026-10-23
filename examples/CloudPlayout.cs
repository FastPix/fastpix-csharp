using System.Globalization;
using Fastpix.Models.Components;
using Fastpix.Models.Errors;
using Fastpix.Models.Requests;

namespace FastPix.Examples;

// Walk through a Cloud Playout loop channel: create it, add a program with one
// media, read its loop schedule, activate and deactivate it, then delete it.
// Needs FASTPIX_READY_MEDIA_ID set to a Ready media, and a free Cloud Playout
// channel slot on your account.
internal static class CloudPlayout
{
    public static async Task Run()
    {
        var sdk = Env.CreateSdk();
        var mediaId = Env.Require("FASTPIX_READY_MEDIA_ID");

        // Start times are UTC with no "Z"; a channel must start at least 3 minutes out.
        var start = DateTime.UtcNow.AddMinutes(5);
        var end = start.AddHours(1);
        const string format = "yyyy-MM-ddTHH:mm:ss";

        string? channelId = null;
        var activated = false;
        var deactivated = false;
        try
        {
            // 1. Create a loop channel. LiveSupport=false means no slate is needed.
            try
            {
                var created = await sdk.CloudPlayoutChannels.CreateAsync(new CloudPlayoutChannelRequest
                {
                    Name = "fastpix_example",
                    Type = CloudPlayoutChannelRequestType.Loop,
                    StartTime = start.ToString(format, CultureInfo.InvariantCulture),
                    EndTime = end.ToString(format, CultureInfo.InvariantCulture),
                    LiveSupport = false,
                });
                channelId = created.CloudPlayoutChannelResponse?.Data?.Id;
                Json.Print(created.CloudPlayoutChannelResponse);
            }
            catch (ApiException ex)
            {
                PrintError("create channel", ex);
                return;
            }
            if (channelId is null) return;

            // 2. Create a program, the unit a loop channel repeats.
            string? programId = null;
            try
            {
                var program = await sdk.CloudPlayoutLoopPrograms.CreateAsync(channelId, new CloudPlayoutProgramRequest
                {
                    Name = "Morning block",
                });
                programId = program.CloudPlayoutProgramResponse?.Data?.Id;
                Json.Print(program.CloudPlayoutProgramResponse);
            }
            catch (ApiException ex)
            {
                PrintError("create program", ex);
            }

            // 3. Add the media to the program.
            if (programId is not null)
            {
                try
                {
                    var added = await sdk.CloudPlayoutLoopPrograms.AddMediaAsync(channelId, programId, new CloudPlayoutProgramMediaRequest
                    {
                        MediaIds = new() { mediaId },
                    });
                    Json.Print(added.CloudPlayoutProgramResponse);
                }
                catch (ApiException ex)
                {
                    PrintError("add media", ex);
                }
            }

            // 4. Read the loop schedule the channel will play.
            try
            {
                var schedule = await sdk.CloudPlayoutScheduleAndEpg.GetScheduleAsync(channelId, GetCloudPlayoutChannelScheduleType.Loop);
                Json.Print(schedule.CloudPlayoutChannelScheduleResponse);
            }
            catch (ApiException ex)
            {
                PrintError("get schedule", ex);
            }

            // 5. Put the channel on air, then take it off.
            // Assumes deactivating a channel that is still `scheduled` (not yet running) is
            // immediate. A running channel moves through `stopping` to `idle` and may need a
            // short wait before it can be deleted.
            try
            {
                var activate = await sdk.CloudPlayoutChannels.ActivateAsync(channelId);
                activated = true;
                Json.Print(activate.CloudPlayoutSuccessResponse);
            }
            catch (ApiException ex)
            {
                PrintError("activate channel", ex);
            }

            try
            {
                var deactivate = await sdk.CloudPlayoutChannels.DeactivateAsync(channelId);
                deactivated = true;
                Json.Print(deactivate.CloudPlayoutSuccessResponse);
            }
            catch (ApiException ex)
            {
                PrintError("deactivate channel", ex);
            }
        }
        finally
        {
            // 6. Clean up so reruns stay tidy; deleting the channel removes its program too.
            if (channelId is not null)
            {
                // The API refuses to delete a scheduled channel, so take it off air first
                // if the deactivate above did not succeed. A 409 means it is already inactive.
                if (activated && !deactivated)
                {
                    try
                    {
                        await sdk.CloudPlayoutChannels.DeactivateAsync(channelId);
                    }
                    catch (ApiException ex) when ((int)ex.Response.StatusCode == 409)
                    {
                    }
                    catch (ApiException ex)
                    {
                        PrintError("deactivate channel", ex);
                    }
                    catch (Exception ex)
                    {
                        // A transport failure must not skip the delete below.
                        Console.WriteLine($"deactivate channel failed: {ex.Message}");
                    }
                }

                try
                {
                    var deleted = await sdk.CloudPlayoutChannels.DeleteAsync(channelId);
                    Json.Print(deleted.CloudPlayoutChannelDeleteResponse);
                }
                catch (ApiException ex)
                {
                    PrintError($"delete channel; remove channel {channelId} manually", ex);
                }
            }
        }
    }

    static void PrintError(string step, ApiException ex)
    {
        Console.WriteLine($"{step} failed");
        Console.WriteLine($"status: {(int)ex.Response.StatusCode}");
        Console.WriteLine($"body:   {ex.Body}");
    }
}
