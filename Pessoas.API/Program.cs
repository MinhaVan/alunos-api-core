using System;
using System.IO;
using AspNetCoreRateLimit;
using Aluno.Core.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Aluno.Core.Data.Context;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var environment = builder.Environment.EnvironmentName;
        Console.WriteLine($"Iniciando a API no ambiente '{environment}'");

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Adiciona as configurações do Secrets Manager
        var secretManager = builder.Services.AddSecretManager(builder.Configuration);

        // Configura os serviços
        builder.Services.AddCustomAuthentication(secretManager)
                        .AddCustomAuthorization()
                        .AddCustomDbContext(secretManager)
                        .AddCustomSwagger()
                        .AddCustomRateLimiting(secretManager)
                        .AddCustomResponseCompression()
                        .AddCustomCors()
                        .AddCustomServices(secretManager)
                        .AddCustomRepository(secretManager)
                        .AddCustomMapper()
                        .AddControllersWithFilters()
                        .AddCustomHttp(secretManager);

        // Configura o loggerbuilder.Logging.AddConsole();
        builder.Logging.AddConsole().AddDebug();

        var app = builder.Build();

        // Configurações específicas para desenvolvimento
        if (environment == "local")
        {
            app.UseDeveloperExceptionPage();
            app.UsePathBase("/pessoas");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/pessoas/swagger/v1/swagger.json", "Pessoas.API v1");
                c.RoutePrefix = "swagger";
            });
        }
        else
        {
            using (var scope = app.Services.CreateScope())
            {
                Console.WriteLine($"Rodando migrations '{environment}'");
                var db = scope.ServiceProvider.GetRequiredService<APIContext>();
                db.Database.Migrate();
                Console.WriteLine($"Migrations '{environment}' executadas com sucesso");
            }

            app.UsePathBase("/auth");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pessoas.API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseResponseCompression();
        app.UseRouting();
        app.UseIpRateLimiting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("CorsPolicy");
        app.UseWebSockets();
        app.MapControllers();

        Console.WriteLine("Configuração de API finalizada com sucesso!");

        app.Run();
    }
}