using MyEvBackend;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.IgnoreObsoleteActions();
    c.IgnoreObsoleteProperties();
    c.CustomSchemaIds(type => type.FullName);
});
builder.Services.AddSingleton<ICosmosDbService>(InitializeCosmosClientInstanceAsync(builder.Configuration).GetAwaiter().GetResult());
builder.Services.AddSingleton<ISmsApi, SmsService>();
builder.Services.AddMvc().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(ConfigurationManager Configuration)
{
    var databaseName = Configuration.GetSection("ConnectionStrings").GetSection("CosmosDbName").Value;
    var evUserContainer = Configuration.GetSection("ConnectionStrings").GetSection("EvUserContainer").Value;
    var evUserHistoryContainer = Configuration.GetSection("ConnectionStrings").GetSection("EvUserHistoryContainer").Value;
    var account = Configuration.GetSection("ConnectionStrings").GetSection("CosmosDbEndpoint").Value;
    var key = Configuration.GetSection("ConnectionStrings").GetSection("CosmosKey").Value;
    var client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
    var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync(evUserContainer, "/VehicleNumber");
    await database.Database.CreateContainerIfNotExistsAsync(evUserHistoryContainer, "/id");
    var cosmosDbService = new CosmosDbService(databaseName,client);
    return cosmosDbService;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();