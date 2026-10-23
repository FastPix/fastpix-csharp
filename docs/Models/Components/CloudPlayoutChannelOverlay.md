# CloudPlayoutChannelOverlay

An overlay placed on a Cloud Playout channel.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier FastPix assigns to the placement. Use this identifier when you update or remove the overlay from the channel. | 5e8b2c31-7a49-4d62-b0f8-9c1e4a7d2b53 |
| `OverlayId` | *string* | :heavy_minus_sign: | The unique identifier of the overlay that was placed. | 2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16 |
| `ChannelId` | *string* | :heavy_minus_sign: | The unique identifier of the channel the overlay is placed on. | cc99cae6-56b4-46ab-9f4a-8b2462d8137c |
| `Name` | *string* | :heavy_minus_sign: | The name given to the overlay. | Channel logo |
| `Type` | [CloudPlayoutChannelOverlayType](../../Models/Components/CloudPlayoutChannelOverlayType.md) | :heavy_minus_sign: | The kind of overlay. A `static` overlay shows a fixed image or video, and a `dynamic` overlay renders content that changes while it is on air. | static |
| `OverlayLayoutType` | *string* | :heavy_minus_sign: | The layout the overlay was created with, such as a logo, an aston, or an L-band. | logo |
| `OverlayImageURL` | *string* | :heavy_minus_sign: | A URL to the overlay's source file. | https://images.fastpix.com/2f6c1d84-9b3a-4d0e-8f21-5c7e3a9b4d16/overlay.png |
| `StartTime` | *DateTime* | :heavy_minus_sign: | When the overlay comes on air, defined as a localDateTime (UTC Time). | 2026-09-02T18:00:00Z |
| `EndTime` | *DateTime* | :heavy_minus_sign: | When the overlay goes off air, defined as a localDateTime (UTC Time). | 2026-09-02T18:10:00Z |
| `Duration` | *string* | :heavy_minus_sign: | How long the overlay stays on air, formatted as `HH:MM:SS`. | 00:10:00 |
| `UntilChannelEnd` | *bool* | :heavy_minus_sign: | Whether the overlay stays on air until the channel's end time. | false |
| `CustomResolution` | *bool* | :heavy_minus_sign: | Whether the overlay was given its own width and height. | false |
| `Width` | *long* | :heavy_minus_sign: | The width of the overlay in pixels. | 320 |
| `Height` | *long* | :heavy_minus_sign: | The height of the overlay in pixels. | 180 |
| `Position` | *bool* | :heavy_minus_sign: | Whether the overlay was given its own position. | true |
| `PositionX` | *long* | :heavy_minus_sign: | The distance of the overlay's left edge from the left edge of the frame, in pixels. | 40 |
| `PositionY` | *long* | :heavy_minus_sign: | The distance of the overlay's top edge from the top edge of the frame, in pixels. | 40 |
| `Opacity` | *long* | :heavy_minus_sign: | How opaque the overlay is, from 1 to 100. | 100 |
| `Repeat` | [CloudPlayoutOverlayRepeat](../../Models/Components/CloudPlayoutOverlayRepeat.md) | :heavy_minus_sign: | Present only when the overlay repeats; absent means it shows once for the whole placement. | {<br/>  "everySeconds": 300,<br/>  "showForSeconds": 30<br/>} |
