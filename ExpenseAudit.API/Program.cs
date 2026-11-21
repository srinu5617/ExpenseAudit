using ExpenseAudit.API.Data;
using ExpenseAudit.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ExpenseDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "ExpenseDatabase")));

builder.Services.AddScoped<IBlobStorageService,
    BlobStorageService>();

builder.Services.AddHttpClient<IReceiptAIService,
    AzureOpenAIService>();

builder.Services.AddScoped<IPolicyEngine,
    PolicyEngine>();

builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();