# CloudPlayoutChannelOverlayUpdateRequest

At least one field must be provided.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `CustomResolution` | *bool* | :heavy_minus_sign: | Set to true to give the overlay your own `width` and `height`. When it is false, `width` and `height` must be left out. | true |
| `Width` | *long* | :heavy_minus_sign: | A new width for the overlay in pixels. Required when `customResolution` is true, and must not exceed the channel's own width. | 320 |
| `Height` | *long* | :heavy_minus_sign: | A new height for the overlay in pixels. Required when `customResolution` is true, and must not exceed the channel's own height. | 180 |
| `Position` | *bool* | :heavy_minus_sign: | Set to true to give the overlay your own `positionX` and `positionY`. When it is false, `positionX` and `positionY` must be left out. | true |
| `PositionX` | *long* | :heavy_minus_sign: | A new distance of the overlay's left edge from the left edge of the frame, in pixels. Required when `position` is true. | 60 |
| `PositionY` | *long* | :heavy_minus_sign: | A new distance of the overlay's top edge from the top edge of the frame, in pixels. Required when `position` is true. | 60 |
| `Opacity` | *long* | :heavy_minus_sign: | A new opacity for the overlay, from 1 to 100. | 80 |
| `Repeat` | [CloudPlayoutOverlayRepeat](../../Models/Components/CloudPlayoutOverlayRepeat.md) | :heavy_minus_sign: | Optional recurrence for a `static` overlay placement. Without it the overlay stays on air for the whole placement window. | {<br/>  "everySeconds": 300,<br/>  "showForSeconds": 30<br/>} |
| `RemoveRepeat` | *bool* | :heavy_minus_sign: | Set to true to stop the overlay repeating and leave it on air for the whole placement. It cannot be sent together with `repeat`. | false |
