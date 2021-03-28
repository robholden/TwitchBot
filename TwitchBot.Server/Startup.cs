using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TwitchBot.Server.Auth;
using TwitchBot.Server.Hubs;
using TwitchBot.Server.TwitchCode.Chatbot;
using TwitchBot.Server.TwitchCode.EventSub;

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
            services.AddControllers();

            services
                .AddAuthentication(EventTokenAuthenticationDefaults.AuthenticationScheme)
                .AddEventToken();

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddSignalR();

            services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");

            // Inject settings
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Inject services
            services.AddTransient<ITwitchEventSubService, TwitchEventSubService>();
            services.AddTransient<ITwitchChatService, TwitchChatService>();

            // Register background tasks
            services.AddHostedService<TwitchChatTask>();
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
                app.UseSpaStaticFiles();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<EventHub>("/hubs/event");
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}