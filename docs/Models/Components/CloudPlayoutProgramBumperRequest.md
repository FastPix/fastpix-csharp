# CloudPlayoutProgramBumperRequest


## Fields

| Field | Type | Required | Description | Example |
| --- | --- | --- | --- | --- |
| `BumperId` | *string* | :heavy_check_mark: | The unique identifier of the bumper to place, as returned by the create a filler endpoint. It must belong to the same workspace as the channel. | 9d9f2c67-0a25-4b3e-9a70-1a5c1d2b6f88 |
| `EventId` | *string* | :heavy_check_mark: | The unique identifier of the item the bumper is placed against, as returned in the `id` field by the get items in a program endpoint. | 4a1e1f3b-2c0d-4c4a-9a02-1d7e5f6b8c90 |
| `BumperType` | [CloudPlayoutProgramBumperRequestBumperType](../../Models/Components/CloudPlayoutProgramBumperRequestBumperType.md) | :heavy_check_mark: | Where the bumper is placed. `preroll` plays it immediately before the item, and `postroll` immediately after it. | preroll |
