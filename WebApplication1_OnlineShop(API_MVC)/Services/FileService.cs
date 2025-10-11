namespace WebApplication1_API_MVC_.Services
{
    public interface IFileService
    {
        string SaveProfileImage(IFormFile file);
        string RemoveProfileImage(string profilePhotoName);
        string SaveProductImage(IFormFile file, List<string> categoryParentsName, string categoryName);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string SaveProfileImage(IFormFile file)
        {
            if(file.ContentType != "image/jpeg")
            {
                throw new InvalidOperationException("File should be JPG or JPEG format.");
            }
            if (file.Length > 10 * 1024 * 1024)
            {
                var errorMessage = "file should have a size less than 10 MB.";
                return errorMessage;
            }
            var folder = Path.Combine(_env.WebRootPath, "images/admin_profile_photos/");
            Directory.CreateDirectory(folder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return uniqueFileName;
        }
        public string RemoveProfileImage(string profilePhotoName)
        {
            var folder = Path.Combine(_env.WebRootPath, "images/admin_profile_photos/");
            var files = Directory.GetFiles(folder);
            foreach( var photo in files)
            {
                if (Path.GetFileName(photo) == profilePhotoName)
                {
                    File.Delete(photo.ToString());
                    break;
                }
            }
            return "default-avatar-icon-of-social-media-user-vector.jpg";

        }
        public string SaveProductImage(IFormFile file , List<string> categoryParentsName , string categoryName)
        {
            if (file.ContentType != "image/jpeg" && file.ContentType != "image/webp")
            {
                throw new InvalidOperationException("File should be JPG or JPEG format.");
            }
            if (file.Length > 10 * 1024 * 1024)
            {
                var errorMessage = "file should have a size less than 10 MB.";
                return errorMessage;
            }
            string folder;
            folder = Path.Combine(_env.WebRootPath, $"images/");
            if (!string.IsNullOrEmpty(categoryName) && categoryParentsName.Count > 0) { //category has one or more parents

                for(int i=categoryParentsName.Count - 1 ; i >= 0 ; i--)
                {
                    folder += categoryParentsName[i] + "/";
                }
                folder += categoryName;
                Directory.CreateDirectory(folder);
            }else if (!string.IsNullOrEmpty(categoryName)) // category doesn't have any parents
            {
                folder = Path.Combine(_env.WebRootPath, $"images/{categoryName}/");
                Directory.CreateDirectory(folder);
            }
            else
            {
                throw new ArgumentException("category name or subcategory required.");
            }
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return uniqueFileName;
        }
    }

}
