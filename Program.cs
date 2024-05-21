using GZY.Quartz.MUI.Extensions;

namespace GithubTrendsScraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen();

            builder.Services.AddQuartzUI();
            builder.Services.AddQuartzClassJobs();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            app.UseQuartz(); //����ע��Quartz

            app.MapControllers();

            app.Run();
        }
    }
}
