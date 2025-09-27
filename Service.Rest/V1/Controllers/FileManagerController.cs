using Application.Command.FileManagerCommands;
using Application.Query.Queries.Attachments;
using Application.Query.ViewModels.Attachments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Rest.V1.RequestModels.FileManagers;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Service.Rest.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/file-managers")]
    [ApiController]
    [Authorize]
    public class FileManagerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FileManagerController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost("upload")]
        [ActionName(nameof(Upload))]
        [SwaggerOperation("Upload new document")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Created", typeof(int))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "some validation or business error")]
        public async Task<ActionResult<string>> Upload([FromForm] UploadDocumentModel request)
        {
            var fileReference = await _mediator.Send(new UploadDocumentCommand(
                request.File,
                request.FolderName,
                request.AttachmentCategory
            ));

            return fileReference;
        }

        [HttpGet("download/{fileReference:guid}")]
        [ActionName(nameof(Download))]
        [SwaggerOperation("Download document")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Document returned", typeof(DownloadDocumentVm))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Document not found", typeof(void))]
        public async Task<ActionResult<DownloadDocumentVm>> Download(Guid fileReference)
        {
            var result = await _mediator.Send(new DownloadDocumentQuery(fileReference));
            return Ok(result);
        }
    }
}
