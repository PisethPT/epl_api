using System;
using System.Threading.Tasks;

namespace epl_api.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment environment;

    public FileService(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }
    public async Task<Tuple<string>> UploadFile(IFormFile file, string directory)
    {
        if (file.Length > 0)
        {
            try
            {
                var fileName = file.FileName;
                var path = Path.Combine("wwwroot", directory);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using (var steam = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(steam);
                    steam.Flush();
                    return new Tuple<string>(fileName);
                }
            }
            catch (Exception ex)
            {
                return new Tuple<string>(ex.Message);
            }
        }
        else
            return new Tuple<string>(string.Empty);

    }
}
