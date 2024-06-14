using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace DocToSomething;

/// <summary>
/// DocToSomethingConverter
/// </summary>
public class DocToSomethingConverter
{
    private readonly Channel<string> _channel;
    private readonly string _tempPath;
    private readonly ILogger _logger;
    private readonly string _sofficePath;


    public DocToSomethingConverter(string sofficePath, string tempPath, ILogger<DocToSomethingConverter> logger, int maxThreads = 1)
    {
        _tempPath = tempPath;
        _logger = logger;
        _sofficePath = sofficePath;

        _channel = Channel.CreateBounded<string>(maxThreads);
        Enumerable.Range(0, maxThreads).Select(o => $"converter{o}").ToList().ForEach(async o =>
        {
            await _channel.Writer.WriteAsync(o);
        });
    }


    /// <summary>
    /// Convert doc to docx
    /// </summary>
    public async Task<Stream?> ConvertAsync(Stream input, CancellationToken cancellationToken = default)
    {
        var user = await _channel.Reader.ReadAsync(cancellationToken);
        Console.WriteLine($"Running conversion with user {user}");

        try
        {
            var converterTempFolder = Path.Join(_tempPath, user);
            Directory.CreateDirectory(converterTempFolder);
            var tempInputFileName = Path.Join(converterTempFolder, $"{Guid.NewGuid()}.doc");
            var tempOutputFileName = tempInputFileName + "x";

            void CleanupTempFiles()
            {
                if (File.Exists(tempOutputFileName))
                {
                    File.Delete(tempOutputFileName);
                }

                if (File.Exists(tempInputFileName))
                {
                    File.Delete(tempInputFileName);
                }
            }

            try
            {
                _logger.LogInformation("Writing temp file");
                using var tempFileOutputStream = File.OpenWrite(tempInputFileName);
                await input.CopyToAsync(tempFileOutputStream, cancellationToken).ConfigureAwait(false);

                var processStartInfo = new ProcessStartInfo(_sofficePath)
                {
                    Arguments = $"-env:UserInstallation=file:///{converterTempFolder} --headless --norestore --nologo --convert-to docx --outdir {converterTempFolder} {tempInputFileName}",
                    CreateNoWindow = true,
                };

                _logger.LogInformation("Starting process");
                using var process = Process.Start(processStartInfo);
                try
                {
                    ArgumentNullException.ThrowIfNull(process);

                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("Something was written probably... open output file");
                        var outputStream = new MemoryStream();
                        using var tempOutputFileStream = File.OpenRead(tempOutputFileName);

                        await tempOutputFileStream.CopyToAsync(outputStream, cancellationToken);

                        outputStream.Position = 0;

                        _logger.LogInformation("All good");
                        return outputStream;
                    }
                }
                finally
                {
                    if (!process?.HasExited ?? false)
                    {
                        process?.Kill();
                    }

                    CleanupTempFiles();
                }
            }
            finally
            {
                CleanupTempFiles();
            }
        }
        finally
        {
            Console.WriteLine($"Returning user {user} to channel");
            await _channel.Writer.WriteAsync(user, cancellationToken);
        }

        return null;
    }
}
