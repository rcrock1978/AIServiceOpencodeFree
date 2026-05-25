using AiService.Configuration;
using AiService.Endpoints;
using AiService.Providers;
using AiService.Repositories;
using AiService.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration binding
builder.Services.Configure<AiOptions>(builder.Configuration.GetSection("AI"));
builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));
builder.Services.Configure<OpenAIOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<AzureOpenAIOptions>(builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<AzureSpeechOptions>(builder.Configuration.GetSection("AzureSpeech"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// AI Providers via factory
builder.Services.AddAIProviders(builder.Configuration);

// HttpClient for AI providers (Ollama, OpenAI, etc.)
builder.Services.AddSingleton<HttpClient>();

// Services
builder.Services.AddSingleton<ISearchService, SearchService>();
builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IVoiceService, VoiceService>();
builder.Services.AddSingleton<IWebSearchProvider, WebSearchProvider>();

// Repositories
var connectionString = builder.Configuration.GetConnectionString("PgVector")
    ?? throw new InvalidOperationException("Connection string 'PgVector' not found.");
builder.Services.AddSingleton<IPgVectorRepository>(_ => new PgVectorRepository(connectionString));
builder.Services.AddSingleton<IConversationRepository>(_ => new ConversationRepository(connectionString));
builder.Services.AddSingleton<ICartRepository>(_ => new CartRepository(connectionString));
builder.Services.AddSingleton<IOrderRepository>(_ => new OrderRepository(connectionString));
builder.Services.AddSingleton<IUserRepository>(_ => new UserRepository(connectionString));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? "SuperSecretKeyForDevelopment12345678!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// HttpContext accessor for CQRS handlers
builder.Services.AddHttpContextAccessor();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health check
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Map endpoints
app.MapSearchEndpoints();
app.MapChatEndpoints();
app.MapEmbeddingsEndpoints();
app.MapCartEndpoints();
app.MapOrderEndpoints();
app.MapVoiceEndpoints();
app.MapAuthEndpoints();

app.Run();
