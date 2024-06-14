using DocToSomething;

const string sofficePath = "soffice";
const string tempPath = "/tmp/";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddHealthChecks();

var app = builder.Build();


app.UseOpenApi();
app.UseSwaggerUi();

var converter = new DocToSomethingConverter(sofficePath, tempPath, app.Services.GetRequiredService<ILogger<DocToSomethingConverter>>(), 4);


app.MapHealthChecks("/health").WithOpenApi();

app.MapPost("/upload", async (IFormFile file) =>
{
    var outputStream = await converter.ConvertAsync(file.OpenReadStream()) ?? throw new ArgumentNullException("uh oh");

    return Results.File(outputStream, fileDownloadName: file.FileName + "x");
}).DisableAntiforgery();

app.Run();
