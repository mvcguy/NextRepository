using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextRepository.Common;
using NextRepository.WebSample.Services;
using Repository.MsSql;
using Repository.MySql;


namespace NextRepository.WebSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();

            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<IMySqlRepository>(provider =>
            {
                var connectionString = Configuration["Data:MySqlDefault:ConnectionString"];
                return new MySqlRepository(connectionString);

            });

            services.AddSingleton(typeof(IMsSqlRepository), provider =>
            {
                var connectionString = Configuration["Data:MsSqlDefault:ConnectionString"];
                return new MsSqlRepository(connectionString);
            });

            services.AddSingleton<SeedDatabaseService>();
            services.AddSingleton<AppQueriesService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            //if this is the right way to initialize stuff?
            app.Use(async (ctx, next) =>
            {
                var provider = ctx.ApplicationServices;
                var seedService = provider.GetService<SeedDatabaseService>();

                seedService.DropCreateDatabaseMySql();
                seedService.DropCreateDatabaseMsSql();

                await next();
            });

            //if this is the right way to initialize stuff?
            app.Use(async (ctx, next) =>
            {
                var provider = ctx.ApplicationServices;
                var appQueriesService = provider.GetService<AppQueriesService>();
                appQueriesService.Init();

                await next();
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
