using DocToSomething;
using Microsoft.Extensions.Logging.Abstractions;

// const string inputFile = "./helloworld.doc";
// const string outputFile = "./helloworld.docx";
// const string sofficePath = "soffice";
// const string tempPath = "/tmp/";

const string inputFile = "../../helloworld.doc";
const string outputDirectoryPath = "../../test/";
const string sofficePath = "/Applications/LibreOffice.app/Contents/MacOS/soffice";
const string tempPath = "/tmp/";

Directory.CreateDirectory(outputDirectoryPath);

var converter = new DocToSomethingConverter(sofficePath, tempPath, NullLogger<DocToSomethingConverter>.Instance, 4);

await Parallel.ForAsync(0, 100, async (i, cancellationToken) =>
{
    using var inputStream = File.OpenRead(inputFile);
    using var convertedStream = await converter.ConvertAsync(inputStream, cancellationToken);

    if (convertedStream != null)
    {
        var outputFilePath = Path.Join(outputDirectoryPath, $"helloworld_{i}.docx");
        Console.WriteLine($"Writing converted file to {outputFilePath}");
        using var outputFileStream = File.OpenWrite(outputFilePath);
        await convertedStream.CopyToAsync(outputFileStream);
    }
});
