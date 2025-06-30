namespace epl_api.Services;

public interface IFileService
{
    Task<Tuple<string>> UploadFile(IFormFile file, string directory);
}
