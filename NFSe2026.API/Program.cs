using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NFSe2026.API;
using NFSe2026.API.Configurations;
using NFSe2026.API.Data;
using NFSe2026.API.Mappings;
using NFSe2026.API.Models;
using NFSe2026.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/nfse-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Manter PascalCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Aceitar case-insensitive
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never; // Não ignorar propriedades
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NFSe 2026 API",
        Version = "v1",
        Description = "API para emissão de Nota Fiscal de Serviços Eletrônica (NFS-e)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Suporte",
            Email = "suporte@example.com"
        }
    });

    // Configuração para JWT no Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Entity Framework with MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Usa versão MySQL 8.0 diretamente (evita erro de AutoDetect que tenta conectar antes)
    // Se o servidor for MySQL 5.7, altere para ServerVersion.Parse("5.7.0")
    var serverVersion = ServerVersion.Parse("8.0.0-mysql");
    options.UseMySql(connectionString, serverVersion, mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure API Nacional NFSe Settings
builder.Services.Configure<ApiNacionalNFSeSettings>(
    builder.Configuration.GetSection(ApiNacionalNFSeSettings.SectionName));
builder.Services.AddSingleton(builder.Configuration.GetSection(ApiNacionalNFSeSettings.SectionName)
    .Get<ApiNacionalNFSeSettings>() ?? new ApiNacionalNFSeSettings());

// Configure NS Tecnologia Settings
builder.Services.Configure<NSTecnologiaSettings>(
    builder.Configuration.GetSection(NSTecnologiaSettings.SectionName));
builder.Services.AddSingleton(builder.Configuration.GetSection(NSTecnologiaSettings.SectionName)
    .Get<NSTecnologiaSettings>() ?? new NSTecnologiaSettings());

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "sua-chave-secreta-super-longa-e-complexa-para-producao-mude-isso-minimo-32-caracteres";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NFSe2026";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "NFSe2026";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Register Services
// Registra serviços de NFSe
builder.Services.AddHttpClient<INFSeAPIService, NFSeAPIService>();
builder.Services.AddHttpClient<NSTecnologiaAPIService>();

// Registra factory de provedores
builder.Services.AddScoped<IProvedorNFSeFactory, ProvedorNFSeFactory>();
builder.Services.AddHttpClient<IConsultaCNPJService, ConsultaCNPJService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICriptografiaService, CriptografiaService>();
builder.Services.AddScoped<IAssinaturaXMLService, AssinaturaXMLService>();
builder.Services.AddScoped<IGeradorXMLDPSService, GeradorXMLDPSService>();
builder.Services.AddScoped<IGeradorJWTService, GeradorJWTService>();
builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<NSTecnologiaEmissaoLogger>();
builder.Services.AddSingleton<TomadorEditLogger>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NFSe 2026 API V1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created (only in development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    try
    {
        // Executar teste detalhado de conexão
        await TestConnectionDetailed.TestAsync(configuration);
        
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            dbContext.Database.EnsureCreated();
            Log.Information("Database verified/created successfully");

            // Seed inicial da configuração da API
            try
            {
                // Verifica se já existe configuração para Homologacao
                var configHomologacao = await dbContext.ConfiguracoesAPI
                    .FirstOrDefaultAsync(c => c.Ambiente == Ambiente.Homologacao);

                if (configHomologacao == null)
                {
                    var configHomolog = new ConfiguracaoAPI
                    {
                        Ambiente = Ambiente.Homologacao,
                        UrlBase = "https://www.producaorestrita.nfse.gov.br",
                        ClientId = "your_client_id", // TODO: Configurar com valores reais
                        ClientSecret = "your_client_secret", // TODO: Configurar com valores reais
                        Scope = "nfse",
                        Timeout = 30,
                        DataCriacao = DateTime.UtcNow
                    };
                    dbContext.ConfiguracoesAPI.Add(configHomolog);
                    await dbContext.SaveChangesAsync();
                    Log.Information("Configuração da API para Homologação criada automaticamente");
                }

                // Verifica se já existe configuração para Producao
                var configProducao = await dbContext.ConfiguracoesAPI
                    .FirstOrDefaultAsync(c => c.Ambiente == Ambiente.Producao);

                if (configProducao == null)
                {
                    var configProd = new ConfiguracaoAPI
                    {
                        Ambiente = Ambiente.Producao,
                        UrlBase = "https://api.nfse.gov.br",
                        ClientId = "your_production_client_id", // TODO: Configurar com valores reais
                        ClientSecret = "your_production_client_secret", // TODO: Configurar com valores reais
                        Scope = "nfse",
                        Timeout = 30,
                        DataCriacao = DateTime.UtcNow
                    };
                    dbContext.ConfiguracoesAPI.Add(configProd);
                    await dbContext.SaveChangesAsync();
                    Log.Information("Configuração da API para Produção criada automaticamente");
                }
            }
            catch (Exception exSeed)
            {
                Log.Warning(exSeed, "Erro ao fazer seed da configuração da API: {Message}", exSeed.Message);
            }
        }
        else
        {
            Log.Warning("Não foi possível conectar ao banco de dados. Verifique a connection string.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Erro ao conectar/criar banco de dados: {Message}", ex.Message);
        Log.Warning("A aplicação continuará iniciando, mas operações de banco falharão.");
        Log.Warning("Verifique: Connection string, servidor acessível, firewall, whitelist de IP");
    }
}

try
{
    Log.Information("Starting NFSe 2026 API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
