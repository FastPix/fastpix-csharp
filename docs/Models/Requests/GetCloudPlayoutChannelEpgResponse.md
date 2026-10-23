# GetCloudPlayoutChannelEpgResponse


## Fields

| Field | Type | Required | Description | Example |
| -------- | -------- | -------- | -------- | -------- |
| `HttpMeta` | [HTTPMetadata](../../Models/Components/HTTPMetadata.md) | :heavy_check_mark: | N/A |  |
| `Xml` | *string* | :heavy_minus_sign: | Programme guide retrieved successfully. An XMLTV document (media type `application/xml`), returned unchanged and unparsed. The `X-Total-Count` header (number of `<programme>` elements) is on `HttpMeta.Response`. |  |
| `DefaultError` | [DefaultError](../../Models/Components/DefaultError.md) | :heavy_minus_sign: | See the range of possible <a href="https://fastpix.com/docs/error-codes/error-codes">error</a> responses and their status codes. |  |
