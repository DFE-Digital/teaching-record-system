using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class TestFileStorageService : IImportFileStorageService
{
    private readonly List<UploadedFile> _uploadedFiles = [];

    public Task<Stream> GetFileAsync(string containerName, string fileName, CancellationToken cancellationToken)
    {
        var file = _uploadedFiles.Single(f => f.ContainerName == containerName && f.FileName == fileName);
        return Task.FromResult(file.Stream);
    }

    public Task<string[]> GetFileNamesAsync(string containerName, string folderName, CancellationToken cancellationToken)
    {
        var fileNames = _uploadedFiles
            .Where(f => f.ContainerName == containerName && f.FileName.StartsWith(folderName, StringComparison.InvariantCultureIgnoreCase))
            .Select(f => f.FileName)
            .ToArray();

        return Task.FromResult(fileNames);
    }

    public Task MoveFileAsync(string containerName, string fileName, string targetFolderName, CancellationToken cancellationToken)
    {
        var file = _uploadedFiles.Single(f => f.ContainerName == containerName && f.FileName == fileName);
        var fileNameOnly = file.FileName.Split('/').Last();
        file.FileName = $"{targetFolderName}/{fileNameOnly}";

        return Task.CompletedTask;
    }

    public Task<Stream> WriteFileAsync(string containerName, string fileName, CancellationToken cancellationToken)
    {
        var file = new UploadedFile(containerName, fileName);
        var stream = new UploadedFileStream(file);
        _uploadedFiles.Add(file);

        return Task.FromResult((Stream)stream);
    }

    public void WriteFile(string containerName, string fileName, string content)
    {
        var file = new UploadedFile(containerName, fileName);
        file.Content = content;
        _uploadedFiles.Add(file);
    }

    public void Clear()
    {
        _uploadedFiles.Clear();
    }

    public UploadedFile? GetLastUploadedFile() => _uploadedFiles.LastOrDefault();

    public class UploadedFile(string containerName, string fileName)
    {
        public string ContainerName { get; set; } = containerName;
        public string FileName { get; set; } = fileName;
        public string? Content { get; set; }

        public Stream Stream
        {
            get
            {
                var stream = new MemoryStream();
                using (var sw = new StreamWriter(stream, leaveOpen: true))
                {
                    sw.Write(Content);
                }
                stream.Position = 0;

                return stream;
            }
        }
    }

    public class UploadedFileStream(UploadedFile file) : MemoryStream
    {
        public override void Close()
        {
            if (this.CanRead)
            {
                this.Position = 0;
                using (var sr = new StreamReader(this, leaveOpen: true))
                {
                    file.Content = sr.ReadToEnd();
                }
            }
            base.Close();
        }
    }
}
