# ExportCloudPlayoutChannelScheduleResponse


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `Csv` | *string* | :heavy_minus_sign: | Channel timeline exported successfully. The body is the CSV file itself, with no byte-order mark, starting with the header row. Media type `text/csv`, returned unchanged. The `Content-Disposition` header (attachment filename) is on `HttpMeta.Response`. | Media ID,Duration (seconds),Start Time,End Time,Type<br/>1f7a3c92-5b6d-4e08-9a41-c2d8e5f7b310,5,2026-09-02T18:00:00Z,2026-09-02T18:00:05Z,bumper |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
