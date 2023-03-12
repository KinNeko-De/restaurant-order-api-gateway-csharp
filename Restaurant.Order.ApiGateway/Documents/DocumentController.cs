extern alias ApiContract;
using ApiContract::Kinnekode.Document;
using ApiContract::Kinnekode.Document.Grpc.V1;
using Google.Protobuf;
using Grpc.Core;
using Kinnekode.Protobuf;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Order.ApiGateway.Documents;

[Route("[controller]")]
[ApiController]
public class DocumentController : ControllerBase
{
    private readonly TimeSpan deadline = TimeSpan.FromSeconds(60);
    private readonly ILogger<DocumentController> logger;
    private readonly DocumentService.DocumentServiceClient documentService;

    public DocumentController(
        ILogger<DocumentController> logger,
        DocumentService.DocumentServiceClient documentService
    )
    {
        this.logger = logger;
        this.documentService = documentService;
    }

    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentById(Guid documentId, CancellationToken cancellationToken)
    {
        if (documentId == Guid.Empty) { return BadRequest(); }

        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            [nameof(documentId)] = documentId
        });

        try
        {
            using var downloadDocumentStream = documentService.DownloadDocument(
                new DownloadDocumentRequest() { DocumentId = Uuid.FromGuid(documentId) },
                deadline: GetDeadline(),
                cancellationToken: cancellationToken);

            var metadata = EnsureFileMetadataWasSent(downloadDocumentStream);
            Response.ContentType = metadata.MediaType;
            Response.ContentLength = Convert.ToInt64(metadata.Size);

            while (await downloadDocumentStream.ResponseStream.MoveNext(cancellationToken))
            {
                var fileChunk = EnsurePayloadWasSent(downloadDocumentStream);
                await Response.BodyWriter.WriteAsync(fileChunk.Memory, cancellationToken);
                await Response.BodyWriter.FlushAsync(cancellationToken);
            }

            await Response.BodyWriter.CompleteAsync();

            return Ok();
        }
        catch (RpcException notFound) when (notFound.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound();
        }
    }

    private static ByteString EnsurePayloadWasSent(AsyncServerStreamingCall<DownloadDocumentResponse> responseStream)
    {
        if (responseStream.ResponseStream.Current.FileCase != DownloadDocumentResponse.FileOneofCase.Chunk)
        {
            throw new InvalidOperationException($"{nameof(responseStream.ResponseStream.Current.FileCase)} of type '{nameof(DownloadDocumentResponse.FileOneofCase.Chunk)}' expected. Actual value {responseStream.ResponseStream.Current.FileCase}.");
        }

        return responseStream.ResponseStream.Current.Chunk;
    }

    private static UploadedFileMetadata EnsureFileMetadataWasSent(AsyncServerStreamingCall<DownloadDocumentResponse> responseStream)
    {
        if (responseStream.ResponseStream.Current.FileCase != DownloadDocumentResponse.FileOneofCase.Metadata)
        {
            throw new InvalidOperationException($"{nameof(responseStream.ResponseStream.Current.FileCase)} of type '{nameof(DownloadDocumentResponse.FileOneofCase.Metadata)}'. Actual value {responseStream.ResponseStream.Current.FileCase}.");
        }

        return responseStream.ResponseStream.Current.Metadata;
    }

    private DateTime GetDeadline()
    {
        return DateTime.UtcNow.Add(deadline);
    }
}