var builder = WebApplication.CreateBuilder(args);

// Adiciona CORS com política nomeada
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ativa o uso da política CORS
app.UseCors("PermitirFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();