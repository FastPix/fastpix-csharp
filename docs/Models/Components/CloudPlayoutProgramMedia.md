# CloudPlayoutProgramMedia

An item in a program on a Cloud Playout loop channel.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier FastPix assigns to the item within the program. Use this identifier when you remove the item from the program. | 4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90 |
| `Index` | *double* | :heavy_minus_sign: | The item's position in the program. Items are played in ascending order of index. | 10 |
| `ProgramId` | *string* | :heavy_minus_sign: | The unique identifier of the program the item belongs to. | d85083a0-e935-4c29-9c32-6720bdc9b22f |
| `ReferenceId` | *string* | :heavy_minus_sign: | The source the item plays. For a `vod` item this is the unique identifier of the media, for a `bumper` item it is the unique identifier of the bumper, and for a `live` item it is the name given to the live event. | 8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21 |
| `Name` | *string* | :heavy_minus_sign: | The name of the item. It is returned for `vod` and `bumper` items; for a `live` item the name is returned in `referenceId`. For a `vod` item it is the media's `title`, as set with `title` on the create media or update media endpoints; `metadata.name` is not used, and when the media has no title its id is shown instead. | Morning headlines |
| `Type` | [CloudPlayoutProgramMediaType](../../Models/Components/CloudPlayoutProgramMediaType.md) | :heavy_minus_sign: | The kind of item. `vod` plays a media item from your library, `live` reserves a slot for the channel's live input, and `bumper` plays a short clip between items. | vod |
| `Duration` | *string* | :heavy_minus_sign: | The duration of the item, formatted as `HH:MM:SS`, or `HH:MM:SS.mmm` when it includes a fraction of a second. | 00:10:30 |
| `Thumbnail` | *string* | :heavy_minus_sign: | A URL to the item's thumbnail image. | https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png |
| `SourceResolution` | *string* | :heavy_minus_sign: | The resolution of the source the item plays. | 1080p |
| `Segments` | List<[CloudPlayoutSegment](../../Models/Components/CloudPlayoutSegment.md)> | :heavy_minus_sign: | The segments marked on the item, in the order they occur. Segments can only be marked on `vod` items. |  |
