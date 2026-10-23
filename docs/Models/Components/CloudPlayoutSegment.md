# CloudPlayoutSegment

A time window marked on an item, which can carry bumpers around it.

This is the loop-channel segment-bumper shape.


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `Id` | *string* | :heavy_minus_sign: | The unique identifier FastPix assigns to the segment. | 6f0b6a3c-6c1e-4c15-9a2e-58b1b0a3c5d1 |
| `SegmentMediaId` | *string* | :heavy_minus_sign: | The unique identifier of the media the segment is marked on. | 8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21 |
| `StartTime` | *long* | :heavy_minus_sign: | The point in the media where the segment begins, in seconds from the start of the media. | 0 |
| `Duration` | *long* | :heavy_minus_sign: | The length of the segment in seconds. | 300 |
| `Bumper` | [CloudPlayoutSegmentBumper](../../Models/Components/CloudPlayoutSegmentBumper.md) | :heavy_minus_sign: | The bumpers attached to this segment. Each key is present only when a bumper of that kind has been added. |  |
| `Thumbnail` | *string* | :heavy_minus_sign: | A URL to the segment's thumbnail image. | https://images.fastpix.com/8c2c4ef1-1c2a-4a49-9c5b-8f9c0a7c3d21/thumbnail.png |
| `Status` | *string* | :heavy_minus_sign: | The current state of the segment. | ready |
