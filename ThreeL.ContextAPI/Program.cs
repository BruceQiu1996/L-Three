using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ThreeL.ContextAPI;

internal class Program
{
    async static Task Main(string[] args)
    {
       var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>((hcontext, builder) =>
        {

        });

        builder.Host.ConfigureServices((hostContext, services) =>
        {
            services.AddMemoryCache();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Three_L v1",
                    Description = "Three_L v1版本接口"
                });
            });
        });

        //middleware
        var host = builder.Build();
        host.UseRouting();
        host.UseAuthentication();
        host.UseAuthorization();
        host.UseSwagger();
        host.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
        });
        host.MapControllers();

        await host.RunAsync();
    }
}
