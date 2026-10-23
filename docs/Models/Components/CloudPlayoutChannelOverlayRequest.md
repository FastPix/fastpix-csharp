# CloudPlayoutChannelOverlayRequest

Places one overlay on a Cloud Playout channel. The same shape is used for loop and schedule channels.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `OverlayId` | *string* | :heavy_check_mark: | The unique identifier of the overlay to place, as returned by the create an overlay endpoint. It must belong to the same workspace as the channel. | 2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16 |
| `StartTime` | *string* | :heavy_check_mark: | When the overlay comes on air, in `yyyy-MM-dd'T'HH:mm:ss` format and interpreted as UTC. It cannot be earlier than the channel's start time. | 2026-09-02T18:00:00 |
| `Duration` | *long* | :heavy_minus_sign: | How long the overlay stays on air, in seconds. Required unless `untilChannelEnd` is true, in which case it must be left out. The overlay must finish before the channel's end time. | 600 |
| `UntilChannelEnd` | *bool* | :heavy_minus_sign: | Set to true to keep the overlay on air from `startTime` until the channel's end time. When it is true, `duration` must be left out. | false |
| `CustomResolution` | *bool* | :heavy_minus_sign: | Set to true to give the overlay your own `width` and `height`. When it is false, the overlay keeps the size it was created with, and `width` and `height` must be left out. Not supported for dynamic overlays. | false |
| `Width` | *long* | :heavy_minus_sign: | The width of the overlay in pixels. Required when `customResolution` is true, and must not exceed the channel's own width. | 320 |
| `Height` | *long* | :heavy_minus_sign: | The height of the overlay in pixels. Required when `customResolution` is true, and must not exceed the channel's own height. | 180 |
| `Position` | *bool* | :heavy_minus_sign: | Set to true to give the overlay your own `positionX` and `positionY`. When it is false, the overlay keeps the position it was created with, and `positionX` and `positionY` must be left out. Not supported for dynamic overlays. | false |
| `PositionX` | *long* | :heavy_minus_sign: | The distance of the overlay's left edge from the left edge of the frame, in pixels. Required when `position` is true. | 40 |
| `PositionY` | *long* | :heavy_minus_sign: | The distance of the overlay's top edge from the top edge of the frame, in pixels. Required when `position` is true. | 40 |
| `Opacity` | *long* | :heavy_minus_sign: | How opaque the overlay is, from 1 to 100. Defaults to 100. For dynamic overlays it stays at 100. | 100 |
| `Repeat` | [CloudPlayoutOverlayRepeat](../../Models/Components/CloudPlayoutOverlayRepeat.md) | :heavy_minus_sign: | Optional recurrence for a `static` overlay placement. Without it the overlay stays on air for the whole placement window. | {<br/>  "everySeconds": 300,<br/>  "showForSeconds": 30<br/>} |
