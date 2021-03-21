using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TwitchBot.Server.Auth;
using TwitchBot.Server.Hubs;
using TwitchBot.Server.Services;

namespace TwitchBot.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddControllers();

            services
                .AddAuthentication(EventTokenAuthenticationDefaults.AuthenticationScheme)
                .AddEventToken();

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddSignalR();

            // Inject settings
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Inject services
            services.AddTransient<ITwitchEventSubService, TwitchEventSubService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<EventHub>("/hubs/event");
                endpoints.MapControllers();
            });
        }
    }
}