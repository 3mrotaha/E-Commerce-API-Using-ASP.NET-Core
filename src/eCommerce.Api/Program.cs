using eCommerce.Persistence;
using eCommerce.Application;
using eCommerce.Api.Middlewares;
using eCommerce.Infrastructure.DependencyInjection;
using eCommerce.Api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddSwagger(builder.Configuration);
builder.Services.AddApiVersioningConfig();


builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

var app = builder.Build();

app.UseExceptionMiddleware();
// app.UseHttpLogging();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API v1.0");
    });

    // app.UseDeveloperExceptionPage();
}

app.Use(async (context, next) =>
  {
      context.Request.EnableBuffering(); // enable buffering to allow reading the request body multiple times in the filters and controllers
      await next();
  });

// app.UseHttpsRedirection();
// app.UseHsts();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthorizeAdminAccountActivated(); // authorize admin accounts
app.MapControllers();

app.Run();
