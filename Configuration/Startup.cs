using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using react_app.Allegro;
using react_app.Apaczka;
using react_app.Apilo;
using react_app.BackgroundTasks;
using react_app.Lomag;
using react_app.Services;
using react_app.Utils;
using react_app.Wmprojack;
using Serilog;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace react_app.Configuration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.Configure<ApiloSettings>(Configuration.GetSection("apilo"));
            services.Configure<ApaczkaSettings>(Configuration.GetSection("apaczka"));
            services.Configure<AllegroSettings>(Configuration.GetSection("allegro"));
            services.Configure<Settings>(Configuration.GetSection("settings"));
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));

            var lomagSettings = new LomagSettings();
            Configuration.GetSection("lomag").Bind(lomagSettings);
            services.AddDbContext<LomagDbContext>(
                o => o.UseSqlServer(lomagSettings.ConnectionString,
                options => options.EnableRetryOnFailure())
            );

            services.AddDbContext<WmprojackDbContext>(
                o => o. UseSqlServer(Configuration.GetConnectionString("wmprojack"),
                options => options.EnableRetryOnFailure())
            );

            //services.AddTransient<AllegroOfferService>();
            //services.AddTransient<AllegroOrderService>();
            //services.AddTransient<IOrderProvider, AllegroOrderProvider>();
            //services.AddTransient<IOrderProvider, ApaczkaOrderProvider>();
            services.AddTransient<IOrderProvider, ApiloOrderProvider>();
            services.AddTransient<OrderService>();
            services.AddTransient<LomagService>();
            services.AddTransient<EmailService>();

            services.AddTransient<CommandExecutor>();

            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddHostedService<QuartzHostedService>();

            //services.AddTransient<OrdersSyncBackgroundJob>();
            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(OrdersSyncBackgroundJob),
            //    cronExpression: "0 0/1 * * * ?"));

            services.AddTransient<RefreshApiloTokenBackgroundJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(RefreshApiloTokenBackgroundJob),
                cronExpression: "0 0/1 * * * ?"));
            services.AddSingleton(new JobSchedule(
                jobType: typeof(RefreshApiloTokenBackgroundJob)));

            services.AddTransient<TruncateLogsBackgroundJob>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(TruncateLogsBackgroundJob),
                cronExpression: "0 0 0/23 * * ?"));





            //services.AddTransient<EmailMinimumInventoryBackgroundJob>();
            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(EmailMinimumInventoryBackgroundJob),
            //    cronExpression: "0 0 0/23 * * ?"));

            //services.AddTransient<ActivateInactiveAllegroOffersJob>(); //TODO to remove for sure
            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(ActivateInactiveAllegroOffersJob),
            //    cronExpression: "0 0/1 * * * ?"));

            //services.AddTransient<BackupAndSendDatabasesJob>();
            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(BackupAndSendDatabasesJob),
            //    cronExpression: "0 0 0/23 * * ?"));
        }

        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                var wmprojackDbContext = services.GetRequiredService<WmprojackDbContext>();

                wmprojackDbContext.Database.EnsureCreated();
            }
        }
    }
}
