using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using GeekBurger.StoreCatalog.Api.Services;
using GeekBurger.StoreCatalog.Api.Services.interfaces;
using GeekBurger.StoreCatalog.Api.Services.ServiceBus.Topic;
using AutoMapper;
using GeekBurger.StoreCatalog.Api.Configuration;

namespace GeekBurger.StoreCatalog.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Configuration  = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json").Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<ServiceBusConfiguration>(opt => Configuration.GetSection("ServiceBus").Bind(opt));

            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                            new OpenApiInfo
                            {
                                Title = "StoreCatalog",
                                Version = "v1",
                                Description = "GeekBurger StoreCatalog Microservice Web API",
                                Contact = new OpenApiContact
                                {
                                    Name = "Maxwell Freitas",
                                    Email = "maxfrts@gmail.com",
                                    Url = null,
                                },
                            });
            });

            services.AddAutoMapper();

            services.AddControllers();

            services.AddSingleton<IReceiveMessagesFactory, ReceiveMessagesFactory>();
            services.AddSingleton<IReceiveMessagesApiFactory, ReceiveMessagesApiFactory>();
            services.AddSingleton<ITopicBus, TopicBus>();
            services.AddSingleton<IAreasService, AreasService>();
            services.AddSingleton<IProductsService, ProductsService>();

            services.AddMemoryCache();

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });

            var mvcCoreBuilder = services.AddMvcCore();

            mvcCoreBuilder
                .AddFormatterMappings();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Store Catalog V1");
            });

            app.UseCors(options => options.AllowAnyOrigin());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.ApplicationServices.CreateScope().ServiceProvider.GetService<IReceiveMessagesApiFactory>();
            app.ApplicationServices.CreateScope().ServiceProvider.GetService<IReceiveMessagesFactory>();

            app.Run(async (context) =>
            {
                await context.Response
                   .WriteAsync("GeekBurger.StoreCatalog is running");
            });
        }
    }
}
