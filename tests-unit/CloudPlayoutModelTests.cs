using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Fastpix.Models.Components;
using Fastpix.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Fastpix.UnitTests;

// Cloud Playout models: every spec example survives deserialize then SerializeJSON with the same wire keys,
// and every envelope reads absent or empty data without throwing. Examples are copied from openapi.yaml.
public class CloudPlayoutModelTests
{
    private static readonly Assembly Sdk = typeof(CloudPlayoutChannel).Assembly;
    private static readonly Regex IsoTime = new(@"^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d(\.\d+)?Z?$");

    // (model type name, example JSON): schema examples, plus the nested parts of response examples for models without one.
    // Null-valued example keys are dropped on serialize by design, so they are not expected back.
    public static IEnumerable<object[]> Examples() => new[]
    {
        new object[] { "CloudPlayoutChannelRequest", """{"name":"News 24x7","type":"loop","startTime":"2026-11-01T00:00:00","endTime":"2026-11-08T00:00:00","liveSupport":true,"slateId":"default"}""" }, // CloudPlayoutChannelRequest
        new object[] { "CloudPlayoutChannelResponse", """{"success":true,"data":{"id":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"News 24x7","ingestionPoint":"rtmps://live.fastpix.com:443/live/493e43e13ac83bb49672403f0fba52b8k17eed9dd1c9c18fe8177d5b547007ef4","startTime":"2026-11-01T00:00:00Z","endTime":"2026-11-08T00:00:00Z","liveSupport":true,"status":"draft","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2026-10-12T06:17:26.403017Z","updatedAt":"2026-10-12T06:17:26.403017Z","slateId":"default","type":"loop","captions":false,"encodingTier":"standard","playbackUrl":"https://stream.fastpix.com/d6bf9101-7d5f-4247-b5a4-0ffc91896ba3.m3u8","inputPlaybackUrl":"https://stream.fastpix.com/396ce915-0978-4d49-a662-06c7f975dd6b.m3u8","destinationUrl":"rtmps://live.fastpix.com:443/live/9150bf73432f52483b4b819237eab3afk41b0002bba7a1f971fbada1cc746a04f","destinationLiveId":"41b0002bba7a1f971fbada1cc746a04f","inputLiveId":"17eed9dd1c9c18fe8177d5b547007ef4"}}""" }, // CloudPlayoutChannelResponse
        new object[] { "CloudPlayoutChannelListResponse", """{"success":true,"data":[{"id":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"News 24x7","ingestionPoint":"rtmps://live.fastpix.com:443/live/493e43e13ac83bb49672403f0fba52b8k17eed9dd1c9c18fe8177d5b547007ef4","startTime":"2026-11-01T00:00:00Z","endTime":"2026-11-08T00:00:00Z","liveSupport":true,"status":"running","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2026-10-12T06:17:26.403017Z","updatedAt":"2026-11-01T00:00:04.118220Z","slateId":"default","type":"loop","captions":false,"encodingTier":"standard","playbackUrl":"https://stream.fastpix.com/d6bf9101-7d5f-4247-b5a4-0ffc91896ba3.m3u8","inputPlaybackUrl":"https://stream.fastpix.com/396ce915-0978-4d49-a662-06c7f975dd6b.m3u8","destinationUrl":"rtmps://live.fastpix.com:443/live/9150bf73432f52483b4b819237eab3afk41b0002bba7a1f971fbada1cc746a04f","destinationLiveId":"41b0002bba7a1f971fbada1cc746a04f","inputLiveId":"17eed9dd1c9c18fe8177d5b547007ef4"},{"id":"f4e1db13-9a2c-4e18-8f3d-6b2c9d0a1e77","name":"Weekend movies","ingestionPoint":"rtmps://live.fastpix.com:443/live/d106fc2e0b72de27c37e15504ece74f1k78477058813be9429b2c44748a254be6","startTime":"2026-11-07T18:00:00Z","endTime":"2026-11-08T02:00:00Z","status":"draft","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2026-10-11T09:02:11.114530Z","updatedAt":"2026-10-11T09:02:11.114530Z","slateId":"default","type":"schedule","genres":["movies"],"encodingTier":"standard","playbackUrl":"https://stream.fastpix.com/348aec86-7196-4ca6-a398-b68a713cf8c4.m3u8","inputPlaybackUrl":"https://stream.fastpix.com/e6f130fa-5992-4429-8209-27e42200de14.m3u8","destinationUrl":"rtmps://live.fastpix.com:443/live/92020929721d6575d756ecb5176a4198k82aecaa8e4aff1cd3546a73d0a8556d1","destinationLiveId":"82aecaa8e4aff1cd3546a73d0a8556d1","inputLiveId":"78477058813be9429b2c44748a254be6"}],"pagination":{"totalRecords":2,"currentOffset":1,"offsetCount":1}}""" }, // CloudPlayoutChannelListResponse
        new object[] { "CloudPlayoutChannelUpdateRequest", """{"endTime":"2026-11-09T00:00:00"}""" }, // CloudPlayoutChannelUpdateRequest
        new object[] { "CloudPlayoutChannelDeleteResponse", """{"success":true}""" }, // CloudPlayoutChannelDeleteResponse
        new object[] { "CloudPlayoutProgramRequest", """{"name":"Morning block"}""" }, // CloudPlayoutProgramRequest
        new object[] { "CloudPlayoutProgramResponse", """{"success":true,"data":{"id":"d85083a0-e935-4c29-9c32-6720bdc9b22f","index":10,"name":"Morning block","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","mediaCount":0,"duration":"00:00:00","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2025-03-12T06:17:26.403017Z","updatedAt":"2025-03-12T06:17:26.403017Z"}}""" }, // CloudPlayoutProgramResponse
        new object[] { "CloudPlayoutProgramListResponse", """{"success":true,"data":[{"id":"d85083a0-e935-4c29-9c32-6720bdc9b22f","index":10,"name":"Morning block","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","mediaCount":4,"duration":"01:12:30.480","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2025-03-12T06:17:26.403017Z","updatedAt":"2025-03-12T08:41:02.117220Z"},{"id":"1ac8ddbd-fb22-46d5-a407-b5238ecaa9a6","index":20,"name":"Evening block","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","mediaCount":3,"duration":"00:48:00","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2025-03-12T06:19:44.882014Z","updatedAt":"2025-03-12T06:19:44.882014Z"}]}""" }, // CloudPlayoutProgramListResponse
        new object[] { "CloudPlayoutProgramDeleteResponse", """{"success":true}""" }, // CloudPlayoutProgramDeleteResponse
        new object[] { "CloudPlayoutProgramPlaylistRequest", """{"playlistId":"pl1"}""" }, // no spec example
        new object[] { "CloudPlayoutProgramMediaRequest", """{"mediaIds":["398edd49-322c-4f06-b596-37ba7ddff0be","075bf22c-6261-4dc4-9795-83374ff83495"]}""" }, // CloudPlayoutProgramMediaRequest
        new object[] { "CloudPlayoutProgramMediaListResponse", """{"success":true,"data":[{"id":"4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90","index":10,"programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","duration":"00:10:30.500","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","segments":[{"id":"6f0b6a3c-6c1e-4c15-9a2e-58b1b0a3c5d1","segmentMediaId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","startTime":0,"duration":300,"bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5,"durationMillis":5040,"thumbnail":"https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png"}}}]},{"id":"7b4d9a12-5f3c-4c8e-9d21-3a7e2c5b1f04","index":20,"programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","referenceId":"Evening match","type":"live","duration":"02:00:00","thumbnail":null,"sourceResolution":null,"segments":[]}]}""" }, // CloudPlayoutProgramMediaListResponse
        new object[] { "CloudPlayoutSuccessResponse", """{"success":true}""" }, // CloudPlayoutSuccessResponse
        new object[] { "CloudPlayoutProgramItemDeleteRequest", """{"eventIds":["4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90","7b4d9a12-5f3c-4c8e-9d21-3a7e2c5b1f04"]}""" }, // CloudPlayoutProgramItemDeleteRequest
        new object[] { "CloudPlayoutProgramBumperRequest", """{"bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","eventId":"4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90","bumperType":"preroll"}""" }, // CloudPlayoutProgramBumperRequest
        new object[] { "CloudPlayoutChannelOverlayRequest", """{"overlayId":"2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16","startTime":"2026-09-02T18:00:00","duration":600,"position":true,"positionX":40,"positionY":40,"opacity":100}""" }, // CloudPlayoutChannelOverlayRequest
        new object[] { "CloudPlayoutOverlayRepeat", """{"everySeconds":300,"showForSeconds":30}""" }, // CloudPlayoutOverlayRepeat
        new object[] { "CloudPlayoutChannelOverlayUpdateRequest", """{"position":true,"positionX":60,"positionY":60,"opacity":80}""" }, // CloudPlayoutChannelOverlayUpdateRequest
        new object[] { "CloudPlayoutChannelOverlaySingleResponse", """{"success":true,"data":{"id":"5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53","overlayId":"2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"Channel logo","type":"static","overlayLayoutType":"logo","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:00Z","duration":"00:10:00","untilChannelEnd":false,"customResolution":false,"width":100,"height":100,"position":true,"positionX":60,"positionY":60,"opacity":80,"repeat":{"everySeconds":300,"showForSeconds":30}}}""" }, // CloudPlayoutChannelOverlaySingleResponse
        new object[] { "CloudPlayoutChannelOverlayListResponse", """{"success":true,"data":[{"id":"5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53","overlayId":"2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"Channel logo","type":"static","overlayLayoutType":"logo","overlayImageURL":"https://images.fastpix.com/2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16/overlay.png","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:00Z","duration":"00:10:00","untilChannelEnd":false,"customResolution":false,"width":100,"height":100,"position":true,"positionX":40,"positionY":40,"opacity":100},{"id":"7c2d9e45-1b38-4f6a-a0d2-3e9f8b7c6a21","overlayId":"0a4b8c6d-2e1f-4a3b-9c5d-7e8f9a0b1c2d","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"Sponsor bug","type":"static","overlayLayoutType":"logo","overlayImageURL":"https://images.fastpix.com/0a4b8c6d-2e1f-4a3b-9c5d-7e8f9a0b1c2d/overlay.png","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T20:00:00Z","duration":"02:00:00","untilChannelEnd":false,"customResolution":false,"width":100,"height":100,"position":false,"positionX":0,"positionY":0,"opacity":100,"repeat":{"everySeconds":300,"showForSeconds":30}}]}""" }, // CloudPlayoutChannelOverlayListResponse
        new object[] { "CloudPlayoutScheduleEventRequest", """{"event":{"type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","startTime":"2026-09-02T18:00:00"}}""" }, // CloudPlayoutScheduleEventRequest
        new object[] { "CloudPlayoutScheduleEventCreate", """{"type":"live","startTime":"2026-09-02T19:00:00","endTime":"2026-09-02T20:00:00","name":"Evening press conference"}""" }, // CloudPlayoutScheduleEventCreate
        new object[] { "CloudPlayoutScheduleEventUpdateRequest", """{"event":{"startTime":"2026-09-02T18:15:00","endTime":"2026-09-02T19:15:00","bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}}}}""" }, // CloudPlayoutScheduleEventUpdateRequest
        new object[] { "CloudPlayoutScheduleEventBumper", """{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}}""" }, // CloudPlayoutScheduleEventBumper
        new object[] { "CloudPlayoutScheduleEventResponse", """{"success":true,"data":{"id":"b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","startTime":"2026-09-02T18:15:00Z","endTime":"2026-09-02T19:15:00Z","duration":"01:00:00","sourceResolution":"1080p","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}},"createdAt":"2026-08-20T09:02:11.114530Z","updatedAt":"2026-08-20T09:14:52.905117Z"}}""" }, // CloudPlayoutScheduleEventResponse
        new object[] { "CloudPlayoutScheduleEventListResponse", """{"success":true,"data":[{"id":"b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:30Z","duration":"00:10:30","sourceResolution":"1080p","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","segments":[],"createdAt":"2026-08-20T09:02:11.114530Z","updatedAt":"2026-08-20T09:02:11.114530Z"},{"id":"3e5c7a90-8b2d-4f61-9c07-6d4a1b8e2f53","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","type":"live","name":"Evening press conference","startTime":"2026-09-02T19:00:00Z","endTime":"2026-09-02T20:00:00Z","duration":"01:00:00","sourceResolution":"1080p","segments":[],"createdAt":"2026-08-20T09:05:44.210338Z","updatedAt":"2026-08-20T09:05:44.210338Z"}]}""" }, // CloudPlayoutScheduleEventListResponse
        new object[] { "CloudPlayoutScheduleItemBumper", """{"preroll":{"id":"1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5}}""" }, // CloudPlayoutScheduleItemBumper
        new object[] { "CloudPlayoutChannelScheduleResponse", """{"success":true,"data":{"schedule":[{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","type":"bumper","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperName":"Ad break intro","startTime":"2026-09-02T17:59:55Z","endTime":"2026-09-02T18:00:00Z","duration":"00:00:05","programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","programName":"Morning block"},{"id":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:30Z","duration":"00:10:30","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","programName":"Morning block","programMediaId":"4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90"}],"videoCount":2}}""" }, // CloudPlayoutChannelScheduleResponse
        new object[] { "CloudPlayoutSimulcastRequest", """{"url":"rtmp://a.rtmp.youtube.com/live2/abcd-1234-efgh-5678"}""" }, // CloudPlayoutSimulcastRequest
        new object[] { "CloudPlayoutSimulcastCreateResponse", """{"success":true,"data":{"simulcastId":"5f2c8e1b7a934d06b8e4c1a3d9f07e52","url":"rtmp://a.rtmp.youtube.com/live2/","streamKey":"abcd-1234-efgh-5678","isEnabled":true}}""" }, // CloudPlayoutSimulcastCreateResponse
        new object[] { "CloudPlayoutSimulcastListResponse", """{"success":true,"data":{"destinationLiveId":"8717422d89288ad5958d4a86e9afe2a2","playbackId":"4fd9d0a8-1c3e-4b2f-9a6d-7c5e8b0f2a41","simulcastResponses":[{"simulcastId":"5f2c8e1b7a934d06b8e4c1a3d9f07e52","url":"rtmp://a.rtmp.youtube.com/live2/","streamKey":"abcd-1234-efgh-5678","isEnabled":true}]}}""" }, // CloudPlayoutSimulcastListResponse
        new object[] { "CloudPlayoutScheduleEventBatchRequest", """{"events":[{"type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","startTime":"2026-09-02T18:00:00"},{"type":"live","name":"Morning show","startTime":"2026-09-03T07:00:00","endTime":"2026-09-03T09:00:00"},{"type":"vod","referenceId":"3e5c7a90-8b2d-4f61-9c07-6d4a1b8e2f53","startTime":"2026-09-03T09:00:00"}]}""" }, // CloudPlayoutScheduleEventBatchRequest
        new object[] { "CloudPlayoutChannel", """{"id":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"News 24x7","ingestionPoint":"rtmps://live.fastpix.com:443/live/493e43e13ac83bb49672403f0fba52b8k17eed9dd1c9c18fe8177d5b547007ef4","startTime":"2026-11-01T00:00:00Z","endTime":"2026-11-08T00:00:00Z","liveSupport":true,"status":"draft","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2026-10-12T06:17:26.403017Z","updatedAt":"2026-10-12T06:17:26.403017Z","slateId":"default","type":"loop","captions":false,"encodingTier":"standard","playbackUrl":"https://stream.fastpix.com/d6bf9101-7d5f-4247-b5a4-0ffc91896ba3.m3u8","inputPlaybackUrl":"https://stream.fastpix.com/396ce915-0978-4d49-a662-06c7f975dd6b.m3u8","destinationUrl":"rtmps://live.fastpix.com:443/live/9150bf73432f52483b4b819237eab3afk41b0002bba7a1f971fbada1cc746a04f","destinationLiveId":"41b0002bba7a1f971fbada1cc746a04f","inputLiveId":"17eed9dd1c9c18fe8177d5b547007ef4"}""" }, // ChannelResponse.data
        new object[] { "CloudPlayoutChannel", """{"id":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"News 24x7","ingestionPoint":"rtmps://live.fastpix.com:443/live/493e43e13ac83bb49672403f0fba52b8k17eed9dd1c9c18fe8177d5b547007ef4","startTime":"2026-11-01T00:00:00Z","endTime":"2026-11-08T00:00:00Z","liveSupport":true,"status":"running","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2026-10-12T06:17:26.403017Z","updatedAt":"2026-11-01T00:00:04.118220Z","slateId":"default","type":"loop","captions":false,"encodingTier":"standard","playbackUrl":"https://stream.fastpix.com/d6bf9101-7d5f-4247-b5a4-0ffc91896ba3.m3u8","inputPlaybackUrl":"https://stream.fastpix.com/396ce915-0978-4d49-a662-06c7f975dd6b.m3u8","destinationUrl":"rtmps://live.fastpix.com:443/live/9150bf73432f52483b4b819237eab3afk41b0002bba7a1f971fbada1cc746a04f","destinationLiveId":"41b0002bba7a1f971fbada1cc746a04f","inputLiveId":"17eed9dd1c9c18fe8177d5b547007ef4"}""" }, // ChannelList.data[0]
        new object[] { "CloudPlayoutChannel", """{"id":"f4e1db13-9a2c-4e18-8f3d-6b2c9d0a1e77","name":"Weekend movies","ingestionPoint":"rtmps://live.fastpix.com:443/live/d106fc2e0b72de27c37e15504ece74f1k78477058813be9429b2c44748a254be6","startTime":"2026-11-07T18:00:00Z","endTime":"2026-11-08T02:00:00Z","status":"draft","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2026-10-11T09:02:11.114530Z","updatedAt":"2026-10-11T09:02:11.114530Z","slateId":"default","type":"schedule","genres":["movies"],"encodingTier":"standard","playbackUrl":"https://stream.fastpix.com/348aec86-7196-4ca6-a398-b68a713cf8c4.m3u8","inputPlaybackUrl":"https://stream.fastpix.com/e6f130fa-5992-4429-8209-27e42200de14.m3u8","destinationUrl":"rtmps://live.fastpix.com:443/live/92020929721d6575d756ecb5176a4198k82aecaa8e4aff1cd3546a73d0a8556d1","destinationLiveId":"82aecaa8e4aff1cd3546a73d0a8556d1","inputLiveId":"78477058813be9429b2c44748a254be6"}""" }, // ChannelList.data[1]
        new object[] { "CloudPlayoutProgram", """{"id":"d85083a0-e935-4c29-9c32-6720bdc9b22f","index":10,"name":"Morning block","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","mediaCount":0,"duration":"00:00:00","workspaceId":"37d62550-e2a5-4827-b16a-3826e66d77ad","createdAt":"2025-03-12T06:17:26.403017Z","updatedAt":"2025-03-12T06:17:26.403017Z"}""" }, // ProgramResponse.data
        new object[] { "CloudPlayoutProgramMedia", """{"id":"4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90","index":10,"programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","duration":"00:10:30.500","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","segments":[{"id":"6f0b6a3c-6c1e-4c15-9a2e-58b1b0a3c5d1","segmentMediaId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","startTime":0,"duration":300,"bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5,"durationMillis":5040,"thumbnail":"https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png"}}}]}""" }, // MediaList.data[0]
        new object[] { "CloudPlayoutProgramMedia", """{"id":"7b4d9a12-5f3c-4c8e-9d21-3a7e2c5b1f04","index":20,"programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","referenceId":"Evening match","type":"live","duration":"02:00:00","thumbnail":null,"sourceResolution":null,"segments":[]}""" }, // MediaList.data[1]
        new object[] { "CloudPlayoutSegment", """{"id":"6f0b6a3c-6c1e-4c15-9a2e-58b1b0a3c5d1","segmentMediaId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","startTime":0,"duration":300,"bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5,"durationMillis":5040,"thumbnail":"https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png"}}}""" }, // MediaList.data[0].segments[0]
        new object[] { "CloudPlayoutSegmentBumper", """{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5,"durationMillis":5040,"thumbnail":"https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png"}}""" }, // segments[0].bumper
        new object[] { "CloudPlayoutSegmentRoll", """{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5,"durationMillis":5040,"thumbnail":"https://images.fastpix.com/9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88/thumbnail.png"}""" }, // segments[0].bumper.preroll
        new object[] { "CloudPlayoutChannelOverlay", """{"id":"5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53","overlayId":"2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"Channel logo","type":"static","overlayLayoutType":"logo","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:00Z","duration":"00:10:00","untilChannelEnd":false,"customResolution":false,"width":100,"height":100,"position":true,"positionX":60,"positionY":60,"opacity":80,"repeat":{"everySeconds":300,"showForSeconds":30}}""" }, // OverlaySingle.data
        new object[] { "CloudPlayoutChannelOverlay", """{"id":"5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53","overlayId":"2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"Channel logo","type":"static","overlayLayoutType":"logo","overlayImageURL":"https://images.fastpix.com/2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16/overlay.png","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:00Z","duration":"00:10:00","untilChannelEnd":false,"customResolution":false,"width":100,"height":100,"position":true,"positionX":40,"positionY":40,"opacity":100}""" }, // OverlayList.data[0]
        new object[] { "CloudPlayoutChannelOverlay", """{"id":"7c2d9e45-1b38-4f6a-a0d2-3e9f8b7c6a21","overlayId":"0a4b8c6d-2e1f-4a3b-9c5d-7e8f9a0b1c2d","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","name":"Sponsor bug","type":"static","overlayLayoutType":"logo","overlayImageURL":"https://images.fastpix.com/0a4b8c6d-2e1f-4a3b-9c5d-7e8f9a0b1c2d/overlay.png","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T20:00:00Z","duration":"02:00:00","untilChannelEnd":false,"customResolution":false,"width":100,"height":100,"position":false,"positionX":0,"positionY":0,"opacity":100,"repeat":{"everySeconds":300,"showForSeconds":30}}""" }, // OverlayList.data[1]
        new object[] { "CloudPlayoutScheduleEvent", """{"id":"b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","startTime":"2026-09-02T18:15:00Z","endTime":"2026-09-02T19:15:00Z","duration":"01:00:00","sourceResolution":"1080p","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}},"createdAt":"2026-08-20T09:02:11.114530Z","updatedAt":"2026-08-20T09:14:52.905117Z"}""" }, // EventResponse.data
        new object[] { "CloudPlayoutScheduleEvent", """{"id":"b1d6f0a7-4e28-4b93-90c5-7f2a8c1d3e64","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:30Z","duration":"00:10:30","sourceResolution":"1080p","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","segments":[],"createdAt":"2026-08-20T09:02:11.114530Z","updatedAt":"2026-08-20T09:02:11.114530Z"}""" }, // EventList.data[0]
        new object[] { "CloudPlayoutScheduleEvent", """{"id":"3e5c7a90-8b2d-4f61-9c07-6d4a1b8e2f53","channelId":"cc99cae6-56b4-46ab-9f4a-8b2462d8137c","type":"live","name":"Evening press conference","startTime":"2026-09-02T19:00:00Z","endTime":"2026-09-02T20:00:00Z","duration":"01:00:00","sourceResolution":"1080p","segments":[],"createdAt":"2026-08-20T09:05:44.210338Z","updatedAt":"2026-08-20T09:05:44.210338Z"}""" }, // EventList.data[1]
        new object[] { "CloudPlayoutScheduleEventRollResponse", """{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}""" }, // EventResponse.data.bumper.preroll
        new object[] { "CloudPlayoutChannelScheduleResponseData", """{"schedule":[{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","type":"bumper","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperName":"Ad break intro","startTime":"2026-09-02T17:59:55Z","endTime":"2026-09-02T18:00:00Z","duration":"00:00:05","programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","programName":"Morning block"},{"id":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:30Z","duration":"00:10:30","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","programName":"Morning block","programMediaId":"4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90"}],"videoCount":2}""" }, // ChannelSchedule.data
        new object[] { "CloudPlayoutChannelScheduleResponse", """{"success":true,"data":{"schedule":[{"id":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:40Z","duration":"00:10:40","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","bumper":{"preroll":{"id":"1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5},"postroll":{"id":"6b2e9d14-8c3f-4a7b-b5e1-0d4c7f2a9e63","bumperId":"2c8e4f61-9a3b-4d7e-8f05-1b6c3d9e7a42","name":"Ad break outro","duration":5}}},{"id":"3e5c7a90-8b2d-4f61-9c07-6d4a1b8e2f53","name":"Evening press conference","type":"live","startTime":"2026-09-02T19:00:00Z","endTime":"2026-09-02T20:00:00Z","duration":"01:00:00","sourceResolution":"1080p"}],"videoCount":2}}""" }, // path scheduleChannel
        new object[] { "CloudPlayoutScheduleItem", """{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","type":"bumper","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","bumperName":"Ad break intro","startTime":"2026-09-02T17:59:55Z","endTime":"2026-09-02T18:00:00Z","duration":"00:00:05","programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","programName":"Morning block"}""" }, // loopChannel.schedule[0]
        new object[] { "CloudPlayoutScheduleItem", """{"id":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:30Z","duration":"00:10:30","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","programId":"d85083a0-e935-4c29-9c32-6720bdc9b22f","programName":"Morning block","programMediaId":"4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90"}""" }, // loopChannel.schedule[1]
        new object[] { "CloudPlayoutScheduleItem", """{"id":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","name":"Morning headlines","type":"vod","startTime":"2026-09-02T18:00:00Z","endTime":"2026-09-02T18:10:40Z","duration":"00:10:40","thumbnail":"https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png","sourceResolution":"1080p","bumper":{"preroll":{"id":"1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5},"postroll":{"id":"6b2e9d14-8c3f-4a7b-b5e1-0d4c7f2a9e63","bumperId":"2c8e4f61-9a3b-4d7e-8f05-1b6c3d9e7a42","name":"Ad break outro","duration":5}}}""" }, // scheduleChannel.schedule[0]
        new object[] { "CloudPlayoutScheduleItem", """{"id":"3e5c7a90-8b2d-4f61-9c07-6d4a1b8e2f53","name":"Evening press conference","type":"live","startTime":"2026-09-02T19:00:00Z","endTime":"2026-09-02T20:00:00Z","duration":"01:00:00","sourceResolution":"1080p"}""" }, // scheduleChannel.schedule[1]
        new object[] { "CloudPlayoutScheduleItemRoll", """{"id":"1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310","bumperId":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","name":"Ad break intro","duration":5}""" }, // scheduleChannel.schedule[0].bumper.preroll
        new object[] { "CloudPlayoutScheduleItemRoll", """{"id":"6b2e9d14-8c3f-4a7b-b5e1-0d4c7f2a9e63","bumperId":"2c8e4f61-9a3b-4d7e-8f05-1b6c3d9e7a42","name":"Ad break outro","duration":5}""" }, // scheduleChannel.schedule[0].bumper.postroll
        new object[] { "CloudPlayoutSimulcastListResponseData", """{"destinationLiveId":"8717422d89288ad5958d4a86e9afe2a2","playbackId":"4fd9d0a8-1c3e-4b2f-9a6d-7c5e8b0f2a41","simulcastResponses":[{"simulcastId":"5f2c8e1b7a934d06b8e4c1a3d9f07e52","url":"rtmp://a.rtmp.youtube.com/live2/","streamKey":"abcd-1234-efgh-5678","isEnabled":true}]}""" }, // SimulcastList.data
        new object[] { "CloudPlayoutScheduleEventUpdate", """{"startTime":"2026-09-02T18:15:00","endTime":"2026-09-02T19:15:00","bumper":{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}}}""" }, // UpdateRequest.event
        new object[] { "CloudPlayoutScheduleEventBumperRequest", """{"preroll":{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}}""" }, // UpdateRequest.event.bumper
        new object[] { "CloudPlayoutScheduleEventRoll", """{"id":"9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88","duration":5}""" }, // UpdateRequest.event.bumper.preroll
        new object[] { "CloudPlayoutScheduleEventBatchItem", """{"type":"vod","referenceId":"8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21","startTime":"2026-09-02T18:00:00"}""" }, // BatchRequest.events[0]
        new object[] { "CloudPlayoutScheduleEventBatchItem", """{"type":"live","name":"Morning show","startTime":"2026-09-03T07:00:00","endTime":"2026-09-03T09:00:00"}""" }, // BatchRequest.events[1]
        new object[] { "CloudPlayoutScheduleEventBatchItem", """{"type":"vod","referenceId":"3e5c7a90-8b2d-4f61-9c07-6d4a1b8e2f53","startTime":"2026-09-03T09:00:00"}""" }, // BatchRequest.events[2]
    };

    private static object Read(System.Type type, string json) =>
        typeof(ResponseBodyDeserializer).GetMethod("Deserialize")!.MakeGenericMethod(type)
            .Invoke(null, new object[] { json, NullValueHandling.Ignore, MissingMemberHandling.Ignore })!;

    private static JToken Parse(string json) =>
        JsonConvert.DeserializeObject<JToken>(json, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None })!;

    // RoundtripKind: a trailing Z reads as Utc, none as Unspecified, so a Z added or dropped changes Kind.
    private static DateTime Instant(string s) =>
        DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    private static void AssertSame(JToken want, JToken got, string path)
    {
        switch (want)
        {
            case JObject wo:
                Assert.True(got is JObject, $"{path}: expected an object, got {got.Type}");
                var go = (JObject)got;
                var wantKeys = wo.Properties().Where(p => p.Value.Type != JTokenType.Null).Select(p => p.Name).ToList();
                var gotKeys = go.Properties().Select(p => p.Name).ToList();
                var missing = wantKeys.Except(gotKeys).ToList();
                var extra = gotKeys.Except(wantKeys).ToList();
                Assert.True(!missing.Any() && !extra.Any(), $"{path}: missing [{string.Join(",", missing)}] extra [{string.Join(",", extra)}]");
                foreach (var k in wantKeys) AssertSame(wo[k]!, go[k]!, $"{path}.{k}");
                break;
            case JArray wa:
                Assert.True(got is JArray, $"{path}: expected an array, got {got.Type}");
                var ga = (JArray)got;
                Assert.True(wa.Count == ga.Count, $"{path}: expected {wa.Count} items, got {ga.Count}");
                for (var i = 0; i < wa.Count; i++) AssertSame(wa[i], ga[i], $"{path}[{i}]");
                break;
            case JValue wv when wv.Type is JTokenType.Integer or JTokenType.Float:
                Assert.True(got.Type is JTokenType.Integer or JTokenType.Float && (double)wv == (double)got, $"{path}: expected {wv}, got {got}");
                break;
            case JValue wv when wv.Type == JTokenType.String && IsoTime.IsMatch((string)wv!):
                var w = (string)wv!;
                var g = got.Type == JTokenType.String ? (string)got! : null;
                Assert.True(g != null && IsoTime.IsMatch(g) && w.EndsWith("Z") == g.EndsWith("Z")
                    && Instant(w) == Instant(g) && Instant(w).Kind == Instant(g).Kind, $"{path}: expected instant {wv}, got {got}");
                break;
            default:
                Assert.True(JToken.DeepEquals(want, got), $"{path}: expected {want}, got {got}");
                break;
        }
    }

    [Theory]
    [MemberData(nameof(Examples))]
    public void SpecExample_RoundTripsWithSameWireKeys(string typeName, string example)
    {
        var type = Sdk.GetType("Fastpix.Models.Components." + typeName, throwOnError: true)!;

        var model = Read(type, example);

        AssertSame(Parse(example), Parse(Utilities.SerializeJSON(model)), typeName);
    }

    // CloudPlayout*Response types with a Success property: every data envelope plus the two deletes and CloudPlayoutSuccessResponse.
    public static IEnumerable<object[]> Envelopes() =>
        typeof(CloudPlayoutChannel).Assembly.GetTypes()
            .Where(t => t.IsClass && t.Namespace == "Fastpix.Models.Components"
                && t.Name.StartsWith("CloudPlayout") && t.Name.EndsWith("Response") && t.GetProperty("Success") != null)
            .OrderBy(t => t.Name)
            .Select(t => new object[] { t });

    [Fact]
    public void EnvelopeScan_FindsEveryResponseType()
    {
        // 12 data envelopes + CloudPlayoutChannelDeleteResponse, CloudPlayoutProgramDeleteResponse, CloudPlayoutSuccessResponse
        Assert.True(Envelopes().Count() >= 15, $"scan found {Envelopes().Count()} envelope types, expected at least 15");
    }

    [Theory]
    [MemberData(nameof(Envelopes))]
    public void Envelope_WithoutData_Deserializes(System.Type type)
    {
        var obj = Read(type, """{"success":true}""");

        Assert.Equal(true, (bool?)type.GetProperty("Success")!.GetValue(obj));
        Assert.Null(type.GetProperty("Data")?.GetValue(obj));
    }

    [Theory]
    [MemberData(nameof(Envelopes))]
    public void Envelope_WithEmptyData_Deserializes(System.Type type)
    {
        var dataProp = type.GetProperty("Data");
        var isList = dataProp != null && typeof(System.Collections.IList).IsAssignableFrom(dataProp.PropertyType);

        var obj = Read(type, isList ? """{"success":true,"data":[]}""" : """{"success":true,"data":{}}""");

        Assert.Equal(true, (bool?)type.GetProperty("Success")!.GetValue(obj));
        var data = dataProp?.GetValue(obj);
        if (dataProp != null) Assert.NotNull(data);
        if (data is System.Collections.IList list) Assert.Empty(list);
        else if (data != null)
            foreach (var p in data.GetType().GetProperties())
                Assert.True(p.GetValue(data) == null, $"{type.Name}.Data.{p.Name} should read as null for an empty data object");
    }
}
