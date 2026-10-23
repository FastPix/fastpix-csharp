# CloudPlayoutSimulcastListResponseData


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `DestinationLiveId` | *string* | :heavy_minus_sign: | The unique identifier of the stream behind the channel. Use it as the `livestreamId` when you remove a simulcast target. | 8717422d89288ad5958d4a86e9afe2a2 |
| `PlaybackId` | *string* | :heavy_minus_sign: | The unique identifier used to build the channel's own playback URL. | 4fd9d0a8-1c3e-4b2f-9a6d-7c5e8b0f2a41 |
| `SimulcastResponses` | List<[LiveSimulcast](../../Models/Components/LiveSimulcast.md)> | :heavy_minus_sign: | The third-party platforms the channel is being restreamed to. |  |
