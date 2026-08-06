using AgenticTesting.Engine.Skills;
using Microsoft.SemanticKernel;

// Load .env file if it exists
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AI Testing Assistant API",
        Version = "v1",
        Description = "Agentic AI-powered test generation and validation system with 40% quality reduction"
    });
});

// Configure Semantic Kernel with Azure OpenAI
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    
    // Azure OpenAI configuration - uses .env file or environment variables
    var endpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"] 
        ?? "https://aoai-ai-testing-assistant.openai.azure.com/";
    var apiKey = builder.Configuration["AZURE_OPENAI_API_KEY"] ?? "your-api-key";
    var deploymentName = builder.Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-4o";

    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: deploymentName,
        endpoint: endpoint,
        apiKey: apiKey);

    return kernelBuilder.Build();
});

// Register the Skill Orchestrator (simplified without memory for now)
builder.Services.AddSingleton<SkillOrchestrator>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    // Pass null for memory - skills will handle null gracefully
    return new SkillOrchestrator(kernel, null!);
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Testing Assistant API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();
