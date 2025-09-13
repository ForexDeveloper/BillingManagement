namespace Shared.MinIO.Models
{
    public class DeleteResponse
    {
        public string? Message { get; set; } = null;
        public bool Result { get; set; }

        public DeleteResponse(bool result, string? message = null)
        {
            Result = result;
            Message = message;
        }
    }

}
