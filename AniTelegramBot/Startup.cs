using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AniTelegramBot
{
    class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AniTelegramBotSettings>(
                Configuration.GetSection(nameof(AniTelegramBotSettings)));

            services.AddSingleton<IAniTelegramBotSettings>(sp =>
                sp.GetRequiredService<IOptions<AniTelegramBotSettings>>().Value);
            services.AddSingleton<Commands>();

        }
    }
}
