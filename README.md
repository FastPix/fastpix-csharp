# FastPix C# SDK

[![NuGet version](https://img.shields.io/nuget/v/Fastpix)](https://www.nuget.org/packages/Fastpix)
[![NuGet downloads](https://img.shields.io/nuget/dt/Fastpix)](https://www.nuget.org/packages/Fastpix)
[![license](https://img.shields.io/github/license/FastPix/fastpix-sdk-csharp)](https://github.com/FastPix/fastpix-sdk-csharp/blob/main/LICENSE)
[![.NET 8.0+](https://img.shields.io/badge/.NET-8.0%2B-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)

A robust, type-safe C# SDK designed for seamless integration with the FastPix API platform.

The FastPix C# SDK is a strongly-typed .NET client for the FastPix video API. From any .NET 8 app you can upload and manage videos, run live streams and simulcasts, create and secure playback IDs, manage playlists and signing keys, pull video analytics (views, metrics, dimensions, and errors), and drive in-video AI features such as subtitles, chapters, summaries, and content moderation.

**Works with:** .NET 8.0+ · C# · NuGet package `Fastpix` · ASP.NET Core, console, and worker apps

📖 **Docs:** https://fastpix.com/docs/language-sdks/csharp-sdk &nbsp;·&nbsp; 🚀 **Free account:** https://dashboard.fastpix.com

## Jump to

Skip straight to a section without scrolling:

| Get started | Reference | Help & more |
|---|---|---|
| [Start here](#start-here) | [Available resources & operations](#available-resources-and-operations) | [FAQ](#faq) |
| [Before you begin](#before-you-begin) | [Error handling](#error-handling) | [Which SDK?](#which-fastpix-sdk-should-i-use) |
| [Install the SDK](#3-install-the-fastpix-sdk) | [Server selection](#server-selection) | [Related tools](#related-fastpix-tools) |
| [Make your first API request](#7-make-your-first-api-request) | [Custom HTTP client](#custom-http-client) | [Development](#development) |
| [Media workflow](#understand-the-media-workflow) | [Retries](#retries) | [Examples](https://github.com/FastPix/fastpix-sdk-csharp/tree/main/examples) |

<br />

## Start here

If you are using the FastPix C# SDK for the first time, follow these steps in order:

1. [Check your .NET version](#1-check-your-net-version)
2. [Create a .NET project](#2-create-a-net-project)
3. [Install the FastPix SDK](#3-install-the-fastpix-sdk)
4. [Verify the SDK installation](#4-verify-the-sdk-installation)
5. [Configure authentication](#5-configure-authentication)
6. [Initialize the FastPix client](#6-initialize-the-fastpix-client)
7. [Make your first API request](#7-make-your-first-api-request)
8. [Retrieve a media asset](#8-retrieve-a-media-asset)
9. [Generate a playback ID](#9-generate-a-playback-id)

**Do not skip the verification steps.** If the SDK does not install or the project does not build, resolve that issue before making an API request.

---

### Before you begin

Make sure you have the following:

- .NET 8.0 or later.
- The .NET CLI.
- Internet access.
- A FastPix account.
- A FastPix Access Token.
- A FastPix Secret Key.
- A publicly accessible video URL for the first media-creation example.

#### Environment and Version Support

| Requirement | Version | Description |
|---|---:|---|
| .NET | `8.0+` | Core runtime environment |
| NuGet | `Latest` | Package manager for dependencies |
| Internet | `Required` | API communication and authentication |

> Pro Tip: We recommend using .NET 8.0+ for optimal performance and the latest language features.

FastPix uses HTTP Basic Authentication.

| SDK property | FastPix credential |
| --- | --- |
| `Username` | Access Token |
| `Password` | Secret Key |

Follow the steps in the [Authentication with Basic Auth](https://fastpix.com/docs/getting-started/activate-your-account) guide to obtain your credentials from the [FastPix Dashboard](https://dashboard.fastpix.com).

Optionally, store your credentials as environment variables:

```bash
# Set your FastPix credentials
export FASTPIX_USERNAME="your-access-token"
export FASTPIX_PASSWORD="your-secret-key"
```

> **Security:** Do not commit your Access Token or Secret Key to source control. Use environment variables or a secure secrets manager.

---

## 1. Check your .NET version

Run:

```bash
dotnet --version
```

The output must be **8.0 or later**.

For example:

```text
8.0.414
```

You can also check the installed SDKs:

```bash
dotnet --list-sdks
```

If .NET 8 or later is not installed, install a supported .NET SDK before continuing.

---

## 2. Create a .NET project

Create a new console application:

```bash
mkdir fastpix-csharp-demo
cd fastpix-csharp-demo
dotnet new console
```

This creates a new .NET console application containing files similar to:

```text
fastpix-csharp-demo/
├── fastpix-csharp-demo.csproj
├── Program.cs
└── obj/
```

Verify that the project builds before installing the FastPix SDK:

```bash
dotnet build
```

You should see output similar to:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

If the build fails, resolve the .NET project issue before continuing.

---

## 3. Install the FastPix SDK

Install the FastPix SDK from NuGet:

```bash
dotnet add package Fastpix
```

The SDK is added to your project as a NuGet dependency.

Verify the package:

```bash
dotnet list package
```

You should see an entry similar to:

```text
Fastpix    1.3.0
```

The exact version may be different if a newer version has been published.

---

## 4. Verify the SDK installation

Before making an API request, verify that your application can import and initialize the FastPix SDK.

Replace the contents of `Program.cs` with:

```csharp
using Fastpix;
using Fastpix.Models.Components;

var sdk = new FastpixSDK(
    security: new Security
    {
        Username = "test",
        Password = "test"
    }
);

Console.WriteLine("FastPix SDK initialized successfully");
```

Run:

```bash
dotnet run
```

Expected output:

```text
FastPix SDK initialized successfully
```

At this point you have verified that:

- .NET is installed.
- The project builds.
- The `Fastpix` package is installed.
- The required namespaces are available.
- The `FastpixSDK` client can be initialized.

The example uses placeholder credentials, so it does **not** make an API request.

---

## 5. Configure authentication

Do not put your FastPix credentials directly in `Program.cs`.

Set the credentials as environment variables instead.

### macOS and Linux

```bash
export FASTPIX_USERNAME="your-access-token"
export FASTPIX_PASSWORD="your-secret-key"
```

Verify that the variables are set without printing the actual credentials:

```bash
test -n "$FASTPIX_USERNAME" && echo "FASTPIX_USERNAME is set"
test -n "$FASTPIX_PASSWORD" && echo "FASTPIX_PASSWORD is set"
```

Expected output:

```text
FASTPIX_USERNAME is set
FASTPIX_PASSWORD is set
```

### Windows PowerShell

```powershell
$env:FASTPIX_USERNAME="your-access-token"
$env:FASTPIX_PASSWORD="your-secret-key"
```

Verify:

```powershell
if ($env:FASTPIX_USERNAME) { "FASTPIX_USERNAME is set" }
if ($env:FASTPIX_PASSWORD) { "FASTPIX_PASSWORD is set" }
```

Do not print the values of the credentials themselves.

---

## 6. Initialize the FastPix client

Update `Program.cs`:

```csharp
using Fastpix;
using Fastpix.Models.Components;

var username = Environment.GetEnvironmentVariable("FASTPIX_USERNAME");
var password = Environment.GetEnvironmentVariable("FASTPIX_PASSWORD");

if (string.IsNullOrWhiteSpace(username))
{
    throw new InvalidOperationException(
        "FASTPIX_USERNAME environment variable is not set."
    );
}

if (string.IsNullOrWhiteSpace(password))
{
    throw new InvalidOperationException(
        "FASTPIX_PASSWORD environment variable is not set."
    );
}

var sdk = new FastpixSDK(
    security: new Security
    {
        Username = username,
        Password = password
    }
);

Console.WriteLine("FastPix client initialized successfully");
```

Run:

```bash
dotnet run
```

Expected output:

```text
FastPix client initialized successfully
```

Initializing the SDK does not make an API request. The SDK contacts the FastPix API when you call an operation such as `CreateMediaAsync`.

---

## 7. Make your first API request

The simplest way to verify the complete integration is to create a media asset from a publicly accessible video URL.

Replace `Program.cs` with:

```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;
using System.Text.Json;

var username = Environment.GetEnvironmentVariable("FASTPIX_USERNAME");
var password = Environment.GetEnvironmentVariable("FASTPIX_PASSWORD");

if (string.IsNullOrWhiteSpace(username))
{
    throw new InvalidOperationException(
        "FASTPIX_USERNAME environment variable is not set."
    );
}

if (string.IsNullOrWhiteSpace(password))
{
    throw new InvalidOperationException(
        "FASTPIX_PASSWORD environment variable is not set."
    );
}

var sdk = new FastpixSDK(
    security: new Security()
    {
        Username = username,
        Password = password,
    }
);

var request = new CreateMediaRequest()
{
    Inputs = new List<Fastpix.Models.Components.Input>()
    {
        Fastpix.Models.Components.Input.CreatePullVideoInput(
            new PullVideoInput()
            {
                Url = "https://static.fastpix.com/fp-sample-video.mp4"
            }
        )
    },

    Metadata = new Dictionary<string, string>()
    {
        { "title", "My first FastPix video" },
        { "source", "csharp-demo" }
    }
};

try
{
    var response = await sdk.InputVideo.CreateMediaAsync(request);

    Console.WriteLine("Media creation request succeeded.");
    Console.WriteLine();
    Console.WriteLine("Response type:");
    Console.WriteLine(response.GetType().FullName);
    Console.WriteLine();
    Console.WriteLine("Response:");
    Console.WriteLine(
        JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        )
    );
}
catch (Exception ex)
{
    Console.Error.WriteLine("FastPix API request failed.");
    Console.Error.WriteLine(ex.Message);
}
```

Run:

```bash
dotnet run
```

If the request succeeds, FastPix returns the response from the media creation API.

> **Note:** The example URL above is a placeholder. You must replace it with a URL that points directly to a video file that FastPix can access.

---

## 8. Retrieve a media asset

After creating media, use the returned media ID to retrieve the media details.

The media ID returned by the create operation can be passed to the corresponding media retrieval operation.

For a media asset to become playable, wait until its processing status indicates that it is ready.

---

## 9. Generate a playback ID

Once your media is ready, you can create a playback ID for video playback.

Playback IDs can be used with FastPix playback clients such as the FastPix Web Player.

For secure content, use signed playback and the appropriate signing configuration.

See the [FastPix playback documentation](https://fastpix.com/docs) for details.

---

## Understand the media workflow

Creating media is usually the first operation in an on-demand video workflow. You carry the media ID from one call to the next, from upload through to playback.

<Image alt="FastPix C# media workflow: create media returns a media ID, get the media details, wait until the status is ready, create a playback ID, then play the video." border={false} src="https://static.fastpix.com/csharp-media-workflow.png" />

A playback ID is created separately, only when you need playback access.

> **More examples:** For runnable, end-to-end flows (media creation, live streaming, playlists, analytics, and more), see the [`examples/`](https://github.com/FastPix/fastpix-sdk-csharp/tree/main/examples) directory in the repo.

## Available Resources and Operations

Comprehensive C# SDK for FastPix platform integration with full API coverage.

### Media API

Upload, manage, and transform video content with comprehensive media management capabilities.

For detailed documentation, see [FastPix Video on Demand Overview](https://fastpix.com/docs/video-on-demand-api/overview).

#### Input Video
- [Create from URL](docs/sdks/inputvideo/README.md#createmedia) - Upload video content from external URL
- [Upload from Device](docs/sdks/inputvideo/README.md#upload) - Upload video files directly from device

#### Manage Videos
- [List All Media](docs/sdks/managevideos/README.md#list) - Retrieve complete list of all media files
- [Get Media by ID](docs/sdks/managevideos/README.md#getbyid) - Get detailed information for specific media
- [Update Media](docs/sdks/videos/README.md#update) - Modify media metadata and settings
- [Delete Media](docs/sdks/managevideos/README.md#deletemedia) - Remove media files from library
- [Cancel Upload](docs/sdks/managevideos/README.md#cancelupload) - Stop ongoing media upload process
- [Get Input Info](docs/sdks/videos/README.md#getinputinfo) - Retrieve detailed input information
- [List Uploads](docs/sdks/managevideos/README.md#listuploads) - Get all available upload URLs
- [List Clips](docs/sdks/managevideos/README.md#listclips) - Get all clips of a media

#### Playback
- [Create Playback ID](docs/sdks/playback/README.md#create) - Generate secure playback identifier
- [List Playback IDs](docs/sdks/playback/README.md#list) - Get all playback IDs for a media
- [Delete Playback ID](docs/sdks/playback/README.md#delete) - Remove playback access
- [Get Playback ID](docs/sdks/playbacks/README.md#get) - Retrieve playback configuration details
- [Update Domain Restrictions](docs/sdks/playback/README.md#updatedomainrestrictions) - Update domain restrictions for a playback ID
- [Update User-Agent Restrictions](docs/sdks/playback/README.md#updateuseragentrestrictions) - Update user-agent restrictions for a playback ID

#### Playlist
- [Create Playlist](docs/sdks/playlist/README.md#create) - Create new video playlist
- [List Playlists](docs/sdks/playlists/README.md#getall) - Get all available playlists
- [Get Playlist](docs/sdks/playlists/README.md#get) - Retrieve specific playlist details
- [Update Playlist](docs/sdks/playlists/README.md#update) - Modify playlist settings and metadata
- [Delete Playlist](docs/sdks/playlists/README.md#delete) - Remove playlist from library
- [Add Media](docs/sdks/playlists/README.md#addmedia) - Add media items to playlist
- [Reorder Media](docs/sdks/playlists/README.md#reordermedia) - Change order of media in playlist
- [Remove Media](docs/sdks/playlists/README.md#deletemedia) - Remove media from playlist

#### Signing Keys
- [Create Key](docs/sdks/signingkeys/README.md#create) - Generate new signing key pair
- [List Keys](docs/sdks/signingkeys/README.md#list) - Get all available signing keys
- [Delete Key](docs/sdks/signingkeys/README.md#delete) - Remove signing key from system
- [Get Key](docs/sdks/signingkeys/README.md#getbyid) - Retrieve specific signing key details

#### DRM Configurations
- [List DRM Configs](docs/sdks/drmconfigurations/README.md#list) - Get all DRM configuration options
- [Get DRM Config](docs/sdks/drmconfigurations/README.md#getbyid) - Retrieve specific DRM configuration

### Live API

Stream, manage, and transform live video content with real-time broadcasting capabilities.

For detailed documentation, see [FastPix Live Stream Overview](https://fastpix.com/docs/live-stream-api/overview).

#### Start Live Stream
- [Create Stream](docs/sdks/livestreams/README.md#create) - Initialize new live streaming session

#### Manage Live Stream
- [List Streams](docs/sdks/livestreams/README.md#getall) - Retrieve all active live streams
- [Get Viewer Count](docs/sdks/livestreams/README.md#getviewercount) - Get real-time viewer statistics
- [Get Stream](docs/sdks/livestreams/README.md#getbyid) - Retrieve detailed stream information
- [Delete Stream](docs/sdks/livestreams/README.md#delete) - Terminate and remove live stream
- [Update Stream](docs/sdks/streams/README.md#update) - Modify stream settings and configuration
- [Enable Stream](docs/sdks/managelivestream/README.md#enable) - Activate live streaming
- [Disable Stream](docs/sdks/managelivestream/README.md#disable) - Pause live streaming
- [Complete Stream](docs/sdks/livestreams/README.md#complete) - Finalize and archive stream

#### Live Playback
- [Create Playback ID](docs/sdks/liveplayback/README.md#create) - Generate secure live playback access
- [Delete Playback ID](docs/sdks/liveplayback/README.md#deleteplaybackid) - Revoke live playback access
- [Get Playback ID](docs/sdks/liveplayback/README.md#getplaybackdetails) - Retrieve live playback configuration
- [Update Domain Restrictions](docs/sdks/liveplayback/README.md#updatedomainrestrictions) - Update domain restrictions for a live playback ID
- [Update User-Agent Restrictions](docs/sdks/liveplayback/README.md#updateuseragentrestrictions) - Update user-agent restrictions for a live playback ID

#### Simulcast Stream
- [Create Simulcast](docs/sdks/simulcasts/README.md#create) - Set up multi-platform streaming
- [Delete Simulcast](docs/sdks/simulcasts/README.md#delete) - Remove simulcast configuration
- [Get Simulcast](docs/sdks/simulcaststream/README.md#getspecific) - Retrieve simulcast settings
- [Update Simulcast](docs/sdks/simulcasts/README.md#update) - Modify simulcast parameters

### Cloud Playout API

Run 24/7 linear channels from your media library, either as a looping program or as a timed schedule.

#### Channels
- [List Channels](docs/sdks/cloudplayoutchannels/README.md#list) - Retrieve all channels, filtered by type
- [Create Channel](docs/sdks/cloudplayoutchannels/README.md#create) - Create a loop or schedule channel
- [Get Channel](docs/sdks/cloudplayoutchannels/README.md#getbyid) - Retrieve a specific channel
- [Update Channel](docs/sdks/cloudplayoutchannels/README.md#update) - Modify channel settings
- [Delete Channel](docs/sdks/cloudplayoutchannels/README.md#delete) - Remove a channel
- [Activate Channel](docs/sdks/cloudplayoutchannels/README.md#activate) - Start a channel playing
- [Deactivate Channel](docs/sdks/cloudplayoutchannels/README.md#deactivate) - Stop a channel playing

#### Loop Programs
- [List Programs](docs/sdks/cloudplayoutloopprograms/README.md#list) - Retrieve all programs on a channel
- [Create Program](docs/sdks/cloudplayoutloopprograms/README.md#create) - Create a program on a loop channel
- [Get Program](docs/sdks/cloudplayoutloopprograms/README.md#getbyid) - Retrieve a specific program
- [Update Program](docs/sdks/cloudplayoutloopprograms/README.md#update) - Modify program settings
- [Delete Program](docs/sdks/cloudplayoutloopprograms/README.md#delete) - Remove a program
- [Add Media](docs/sdks/cloudplayoutloopprograms/README.md#addmedia) - Add media to a program
- [Add Playlist](docs/sdks/cloudplayoutloopprograms/README.md#addplaylist) - Add a playlist to a program
- [Get Media Details](docs/sdks/cloudplayoutloopprograms/README.md#getmediadetails) - Retrieve the items in a program
- [Delete Item](docs/sdks/cloudplayoutloopprograms/README.md#deleteitem) - Remove an item from a program
- [Delete Items](docs/sdks/cloudplayoutloopprograms/README.md#deleteitems) - Remove several items from a program
- [Add Bumper](docs/sdks/cloudplayoutloopprograms/README.md#addbumper) - Add a bumper to a program item

#### Schedule Channel Events
- [List Events](docs/sdks/cloudplayoutschedulechannel/README.md#listevents) - Retrieve the events on a schedule channel
- [Add Event](docs/sdks/cloudplayoutschedulechannel/README.md#addevent) - Add an event to a schedule channel
- [Update Event](docs/sdks/cloudplayoutschedulechannel/README.md#updateevent) - Modify an event on a schedule channel
- [Delete Event](docs/sdks/cloudplayoutschedulechannel/README.md#deleteevent) - Remove an event from a schedule channel
- [Add Events](docs/sdks/cloudplayoutschedulechannel/README.md#addevents) - Add several events in one request

#### Channel Overlays
- [List Overlays](docs/sdks/cloudplayoutchanneloverlays/README.md#list) - Retrieve all overlays on a channel
- [Add Overlay](docs/sdks/cloudplayoutchanneloverlays/README.md#add) - Add an overlay to a channel
- [Update Overlay](docs/sdks/cloudplayoutchanneloverlays/README.md#update) - Modify an overlay on a channel
- [Delete Overlay](docs/sdks/cloudplayoutchanneloverlays/README.md#delete) - Remove an overlay from a channel

#### Schedule and EPG
- [Get Schedule](docs/sdks/cloudplayoutscheduleandepg/README.md#getschedule) - Retrieve what a channel plays
- [Export Schedule](docs/sdks/cloudplayoutscheduleandepg/README.md#exportschedule) - Export what a channel plays as CSV
- [Get EPG](docs/sdks/cloudplayoutscheduleandepg/README.md#getepg) - Retrieve a channel's program guide as XML

#### Simulcast
- [List Simulcasts](docs/sdks/cloudplayoutsimulcast/README.md#list) - Retrieve a channel's simulcast targets
- [Create Simulcast](docs/sdks/cloudplayoutsimulcast/README.md#create) - Add a simulcast target to a channel
- [Delete Simulcast](docs/sdks/cloudplayoutsimulcast/README.md#delete) - Remove a simulcast target from a channel

### Video Data API

Monitor video performance and quality with comprehensive analytics and real-time metrics.

For detailed documentation, see [FastPix Video Data Overview](https://fastpix.com/docs/video-data-api/overview).

#### Metrics
- [List Breakdown Values](docs/sdks/metrics/README.md#listbreakdownvalues) - Get detailed breakdown of metrics by dimension
- [List Overall Values](docs/sdks/metrics/README.md#listoverallvalues) - Get aggregated metric values across all content
- [Get Timeseries Data](docs/sdks/metrics/README.md#gettimeseriesdata) - Retrieve time-based metric trends and patterns
- [Compare Values](docs/sdks/metrics/README.md#compare) - List comparison values

#### Views
- [List Video Views](docs/sdks/views/README.md#list) - Get comprehensive list of video viewing sessions
- [Get View Details](docs/sdks/views/README.md#getviewdetails) - Retrieve detailed information about specific video views
- [List Top Content](docs/sdks/views/README.md#listbytopcontent) - Find your most popular and engaging content

#### Dimensions
- [List Dimensions](docs/sdks/dimensions/README.md#list) - Get available data dimensions for filtering and analysis
- [List Filter Values](docs/sdks/dimensions/README.md#listfilters) - Get specific values for a particular dimension

#### Errors
- [List Errors](docs/sdks/errors/README.md#list) - Get list of playback errors

### Transformations

Transform and enhance your video content with powerful AI and editing capabilities.

#### In-Video AI Features

Enhance video content with AI-powered features including moderation, summarization, and intelligent categorization.

- [Update Summary](docs/sdks/invideoaifeatures/README.md#updatesummary) - Create AI-generated video summaries
- [Update Chapters](docs/sdks/invideoai/README.md#updatemediachapters) - Automatically generate video chapter markers
- [Extract Entities](docs/sdks/invideoai/README.md#updatenamedentities) - Identify and extract named entities from content
- [Enable Moderation](docs/sdks/moderations/README.md#update) - Activate content moderation and safety checks

#### Media Clips

- [List Live Clips](docs/sdks/videos/README.md#listliveclips) - Get all clips of a live stream
- [List Media Clips](docs/sdks/managevideos/README.md#listclips) - Retrieve all clips associated with a source media

#### Subtitles

- [Generate Subtitles](docs/sdks/managevideos/README.md#generatesubtitles) - Create automatic subtitles for media

#### Media Tracks

- [Add Track](docs/sdks/managevideos/README.md#addmediatrack) - Add audio or subtitle tracks to media
- [Update Track](docs/sdks/videos/README.md#updatetrack) - Modify existing audio or subtitle tracks
- [Delete Track](docs/sdks/tracks/README.md#delete) - Remove audio or subtitle tracks

#### Access Control

- [Update Source Access](docs/sdks/managevideos/README.md#updatesourceaccess) - Control access permissions for media source

#### Format Support

- [Update MP4 Support](docs/sdks/managevideos/README.md#updatemp4support) - Configure MP4 download capabilities

<!-- End Available Resources and Operations [operations] -->

<!-- Start Retries [retries] -->
## Retries

Some of the endpoints in this SDK support retries. If you use the SDK without any configuration, it will fall back to the default retry strategy provided by the API. However, the default retry strategy can be overridden on a per-operation basis, or across the entire SDK.

To change the default retry strategy for a single API call, simply pass a `RetryConfig` to the call:

```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils.Retries;
using System.Collections.Generic;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

CreateMediaRequest req = new CreateMediaRequest() {
    Inputs = new List<Fastpix.Models.Components.Input>() {
        Fastpix.Models.Components.Input.CreatePullVideoInput(
            new PullVideoInput() {}
        ),
    },
    Metadata = new Dictionary<string, string>() {
        { "<key>", "<value>" },
    },
};

var res = await sdk.InputVideo.CreateMediaAsync(
    retryConfig: new RetryConfig(
        strategy: RetryConfig.RetryStrategy.BACKOFF,
        backoff: new BackoffStrategy(
            initialIntervalMs: 1L,
            maxIntervalMs: 50L,
            maxElapsedTimeMs: 100L,
            exponent: 1.1
        ),
        retryConnectionErrors: false
    ),
    request: req
);

// handle response
```

If you'd like to override the default retry strategy for all operations that support retries, you can use the `RetryConfig` optional parameter when initializing the SDK:

```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Utils.Retries;
using System.Collections.Generic;

var sdk = new FastpixSDK(
    retryConfig: new RetryConfig(
        strategy: RetryConfig.RetryStrategy.BACKOFF,
        backoff: new BackoffStrategy(
            initialIntervalMs: 1L,
            maxIntervalMs: 50L,
            maxElapsedTimeMs: 100L,
            exponent: 1.1
        ),
        retryConnectionErrors: false
    ),
    security: new Security() {
        Username = "your-access-token",
        Password = "your-secret-key",
    }
);

CreateMediaRequest req = new CreateMediaRequest() {
    Inputs = new List<Fastpix.Models.Components.Input>() {
        Fastpix.Models.Components.Input.CreatePullVideoInput(
            new PullVideoInput() {}
        ),
    },
    Metadata = new Dictionary<string, string>() {
        { "<key>", "<value>" },
    },
};

var res = await sdk.InputVideo.CreateMediaAsync(req);

// handle response
```
<!-- End Retries [retries] -->

<!-- Start Error Handling [errors] -->
## Error Handling

[`FastpixException`](./src/Fastpix/Models/Errors/FastPixException.cs) is the base exception class for all HTTP error responses. It has the following properties:

| Property      | Type                  | Description           |
|---------------|-----------------------|-----------------------|
| `Message`     | *string*              | Error message         |
| `Request`     | *HttpRequestMessage*  | HTTP request object   |
| `Response`    | *HttpResponseMessage* | HTTP response object  |
| `Body`        | *string*              | HTTP response body    |

### Example

```csharp
using Fastpix;
using Fastpix.Models.Components;
using Fastpix.Models.Errors;
using System.Collections.Generic;

var sdk = new FastpixSDK(security: new Security() {
    Username = "your-access-token",
    Password = "your-secret-key",
});

try
{
    CreateMediaRequest req = new CreateMediaRequest() {
        Inputs = new List<Fastpix.Models.Components.Input>() {
            Fastpix.Models.Components.Input.CreatePullVideoInput(
                new PullVideoInput() {}
            ),
        },
        Metadata = new Dictionary<string, string>() {
            { "<key>", "<value>" },
        },
    };

    var res = await sdk.InputVideo.CreateMediaAsync(req);
    // handle response
}
catch (FastpixException ex)  // all SDK exceptions inherit from FastpixException
{
    // ex.ToString() provides a detailed error message
    System.Console.WriteLine(ex);

    // Base exception fields
    HttpRequestMessage request = ex.Request;
    HttpResponseMessage response = ex.Response;
    var statusCode = (int)response.StatusCode;
    var responseBody = ex.Body;
}
catch (OperationCanceledException ex)
{
    // CancellationToken was cancelled
}
catch (System.Net.Http.HttpRequestException ex)
{
    // Check ex.InnerException for Network connectivity errors
}
```

### Error Classes

**Primary exception:**
* [`FastpixException`](./src/Fastpix/Models/Errors/FastPixException.cs): The base class for HTTP error responses.

<details><summary>Less common exceptions (2)</summary>

* [`System.Net.Http.HttpRequestException`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httprequestexception): Network connectivity error. For more details about the underlying cause, inspect the `ex.InnerException`.

* Inheriting from [`FastpixException`](./src/Fastpix/Models/Errors/FastPixException.cs):
  * [`ResponseValidationError`](./src/Fastpix/Models/Errors/ResponseValidationException.cs): Thrown when the response data could not be deserialized into the expected type.
</details>
<!-- End Error Handling [errors] -->

<!-- Start Server Selection [server] -->
## Server Selection

### Override Server URL Per-Client

The default server can be overridden globally by passing a URL to the `serverUrl: string` optional parameter when initializing the SDK client instance. For example:

```csharp
using Fastpix;
using Fastpix.Models.Components;
using System.Collections.Generic;

var sdk = new FastpixSDK(
    serverUrl: "<server-url>",
    security: new Security() {
        Username = "your-access-token",
        Password = "your-secret-key",
    }
);

CreateMediaRequest req = new CreateMediaRequest() {
    Inputs = new List<Fastpix.Models.Components.Input>() {
        Fastpix.Models.Components.Input.CreatePullVideoInput(
            new PullVideoInput() {}
        ),
    },
    Metadata = new Dictionary<string, string>() {
        { "<key>", "<value>" },
    },
};

var res = await sdk.InputVideo.CreateMediaAsync(req);

// handle response
```
<!-- End Server Selection [server] -->

<!-- Start Custom HTTP Client [http-client] -->
## Custom HTTP Client

The C# SDK makes API calls using an `IFastpixHttpClient` that wraps the native
[HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient). This
client provides the ability to attach hooks around the request lifecycle that can be used to modify the request or handle
errors and response.

The `IFastpixHttpClient` interface allows you to either use the default `FastpixHttpClient` that comes with the SDK,
or provide your own custom implementation with customized configuration such as custom message handlers, timeouts,
connection pooling, and other HTTP client settings.

The following example shows how to create a custom HTTP client with request modification and error handling:

```csharp
using Fastpix;
using Fastpix.Utils;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// Create a custom HTTP client
public class CustomHttpClient : IFastpixHttpClient
{
    private readonly IFastpixHttpClient _defaultClient;

    public CustomHttpClient()
    {
        _defaultClient = new FastpixHttpClient();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
    {
        // Add custom header and timeout
        request.Headers.Add("x-custom-header", "custom value");
        request.Headers.Add("x-request-timeout", "30");
        
        try
        {
            var response = await _defaultClient.SendAsync(request, cancellationToken);
            // Log successful response
            Console.WriteLine($"Request successful: {response.StatusCode}");
            return response;
        }
        catch (Exception error)
        {
            // Log error
            Console.WriteLine($"Request failed: {error.Message}");
            throw;
        }
    }

    public void Dispose()
    {
        _defaultClient?.Dispose();
    }
}

// Use the custom HTTP client with the SDK
var customHttpClient = new CustomHttpClient();
var sdk = new FastpixSDK(client: customHttpClient);
```

<details>
<summary>You can also provide a completely custom HTTP client with your own configuration:</summary>

```csharp
using Fastpix.Utils;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// Custom HTTP client with custom configuration
public class AdvancedHttpClient : IFastpixHttpClient
{
    private readonly HttpClient _httpClient;

    public AdvancedHttpClient()
    {
        var handler = new HttpClientHandler()
        {
            MaxConnectionsPerServer = 10,
            // ServerCertificateCustomValidationCallback = customCertValidation, // Custom SSL validation if needed
        };

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
    {
        return await _httpClient.SendAsync(request, cancellationToken ?? CancellationToken.None);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

var sdk = new FastpixSDK(client: new AdvancedHttpClient());
```
</details>

<details>
<summary>For simple debugging, you can enable request/response logging by implementing a custom client:</summary>

```csharp
public class LoggingHttpClient : IFastpixHttpClient
{
    private readonly IFastpixHttpClient _innerClient;

    public LoggingHttpClient(IFastpixHttpClient innerClient = null)
    {
        _innerClient = innerClient ?? new FastpixHttpClient();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken? cancellationToken = null)
    {
        // Log request
        Console.WriteLine($"Sending {request.Method} request to {request.RequestUri}");
        
        var response = await _innerClient.SendAsync(request, cancellationToken);
        
        // Log response
        Console.WriteLine($"Received {response.StatusCode} response");
        
        return response;
    }

    public void Dispose() => _innerClient?.Dispose();
}

var sdk = new FastpixSDK(client: new LoggingHttpClient());
```
</details>

The SDK also provides built-in hook support through the `SDKConfiguration.Hooks` system, which automatically handles
`BeforeRequestAsync`, `AfterSuccessAsync`, and `AfterErrorAsync` hooks for advanced request lifecycle management.
<!-- End Custom HTTP Client [http-client] -->

<!-- Placeholder for Future Fastpix SDK Sections -->

## FAQ

**How do I install the FastPix C# SDK?**
Add the NuGet package with `dotnet add package Fastpix` (or `Install-Package Fastpix` in Visual Studio). See [Install the FastPix SDK](#3-install-the-fastpix-sdk).

**How do I authenticate the FastPix .NET SDK?**
FastPix uses Basic Auth: pass your access token as `Username` and your secret key as `Password` when constructing `FastpixSDK`. See [Initialize the FastPix client](#6-initialize-the-fastpix-client).

**How do I upload a video from C#?**
Create media from a URL or upload from a device through `sdk.InputVideo`, for example `await sdk.InputVideo.CreateMediaAsync(req)`. See [Make your first API request](#7-make-your-first-api-request) and [Available Resources and Operations](#available-resources-and-operations).

**How do I start a live stream in .NET?**
Use the Live API to create and manage streams, simulcasts, and live playback IDs. See [Available Resources and Operations](#available-resources-and-operations).

**How do I create a secure playback ID?**
Generate playback IDs and manage signing keys and DRM configurations through the Media API resources. See [Available Resources and Operations](#available-resources-and-operations).

**How do I get video analytics and metrics in C#?**
The Video Data API exposes metrics, views, dimensions, and errors for monitoring quality of experience. See [Available Resources and Operations](#available-resources-and-operations).

**How do I handle API errors?**
Catch `FastpixException` (the base class for all HTTP error responses); it exposes the request, response, status code, and body. See [Error Handling](#error-handling).

**How do I configure automatic retries?**
Pass a `RetryConfig` per call or when constructing the SDK to control the backoff strategy. See [Retries](#retries).

**How do I use a custom HttpClient, proxy, or timeout?**
Provide your own `IFastpixHttpClient` implementation (custom headers, handlers, timeouts, connection pooling) or use the built-in hooks. See [Custom HTTP Client](#custom-http-client).

**Which .NET versions are supported?**
The SDK targets .NET 8.0 and above. See [Before you begin](#before-you-begin).

**Is the SDK strongly typed?**
Yes - it is a strongly-typed client generated from the FastPix API specification, so requests and responses are fully typed. See [Development](#development).

## Which FastPix SDK should I use?

FastPix publishes a server SDK for every major backend language, each generated from the same API specification:

| Language | Repo | Install |
|---|---|---|
| **C# / .NET** (this repo) | [fastpix-sdk-csharp](https://github.com/FastPix/fastpix-sdk-csharp) | `dotnet add package Fastpix` |
| Node.js / TypeScript | [node-sdk](https://github.com/FastPix/node-sdk) | `npm install @fastpix/fastpix-node` |
| Python | [fastpix-python](https://github.com/FastPix/fastpix-python) | `pip install fastpix-python` |
| Go | [fastpix-go](https://github.com/FastPix/fastpix-go) | `go get github.com/FastPix/fastpix-go` |
| PHP | [fastpix-php](https://github.com/FastPix/fastpix-php) | `composer require fastpix/sdk` |
| Java | [fastpix-java](https://github.com/FastPix/fastpix-java) | `io.fastpix:sdk` (Maven/Gradle) |
| Ruby | [fastpix-ruby](https://github.com/FastPix/fastpix-ruby) | `gem install fastpixapi` |

## Related FastPix tools

The C# SDK manages media, live streams, and playback on the server. To upload and play that media in the browser, pair it with these FastPix client-side libraries:

- [web-uploads-sdk](https://github.com/FastPix/web-uploads-sdk) - resumable, chunked file uploads from the browser (`@fastpix/resumable-uploads`)
- [react-web-uploader](https://github.com/FastPix/react-web-uploader) - a drop-in React upload component (`@fastpix/fp-react-uploader`)
- [web-player-component](https://github.com/FastPix/web-player-component) - the FastPix HLS video player web component (`@fastpix/fp-player`)

Browse every SDK and tool in the [FastPix organization](https://github.com/orgs/FastPix/repositories).

## Development

This C# SDK is programmatically generated from our API specifications. Any manual modifications to internal files will be overwritten during subsequent generation cycles. 

We value community contributions and feedback. Feel free to submit pull requests or open issues with your suggestions, and we'll do our best to include them in future releases.

## Detailed Usage

For comprehensive understanding of each API's functionality, including detailed request and response specifications, parameter descriptions, and additional examples, please refer to the [FastPix API Reference](https://fastpix.com/docs/product-os-api/overview).

The API reference offers complete documentation for all available endpoints and features, enabling developers to integrate and leverage FastPix APIs effectively.
