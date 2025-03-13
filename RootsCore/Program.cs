
using RootsCore.Models;
using RootsCore.Services;

namespace RootsCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddSingleton<Data.Neo4jContext>();

            builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("Ai"));
            var vendor = builder.Configuration["AiVendor"]?.ToLower() ?? "mock";
            switch (vendor)
            {
                case "grok":
                    builder.Services.AddSingleton<IAiService, GrokAiService>();
                    break;
                case "claude":
                    builder.Services.AddSingleton<IAiService, ClaudeAiService>();
                    break;
                case "gemini":
                    builder.Services.AddSingleton<IAiService, GeminiAiService>();
                    break;
                case "chatgpt":
                    builder.Services.AddSingleton<IAiService, ChatGptAiService>();
                    break;
                case "azure":
                    builder.Services.AddSingleton<IAiService, AzureAiService>();
                    break;
                default:
                    builder.Services.AddSingleton<IAiService, MockAiService>();
                    break;
            }

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
        }
    }
}
