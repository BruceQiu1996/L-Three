using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ThreeL.ContextAPI.Application.Contract.Extensions;
using ThreeL.ContextAPI.Application.Contract.Services;
using ThreeL.ContextAPI.Application.Impl;
using ThreeL.ContextAPI.Application.Impl.Services.Grpc;
using ThreeL.Shared.Application.Contract.Extensions;

namespace ThreeL.ContextAPI;

internal class Program
{
    async static Task Main(string[] args)
    {
        WebApplication host = null;
        AppAssemblyInfo appAssemblyInfo = new AppAssemblyInfo();
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>((hcontext, builder) =>
        {
            builder.AddContextAPIApplicationContainer(appAssemblyInfo.ImplementAssembly);
        });

        builder.Host.ConfigureServices((hostContext, services) =>
        {
            services.AddGrpc();
            services.AddContextAPIApplicationService(hostContext.Configuration, appAssemblyInfo.ContractAssembly);
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

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                        {
                            new OpenApiSecurityScheme{Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme,Id = "Bearer"}},new string[] { }
                        }
                });
            });

            //注册jwt认证
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = hostContext.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudiences = hostContext.Configuration.GetSection("Jwt:Audiences").Get<string[]>(),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(int.Parse(hostContext.Configuration["Jwt:ClockSkew"]!)), //过期时间容错值，解决服务器端时间不同步问题（秒）
                    RequireExpirationTime = true,
                    IssuerSigningKeyResolver = host!.Services.GetRequiredService<IJwtService>().ValidateIssuerSigningKey
                };
            });
        });

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        //middleware
        host = builder.Build();
        await host.PreheatService();
        host.UseRouting();
        host.UseAuthentication();
        host.UseAuthorization();
        host.UseSwagger();
        host.UseSwaggerUI(option =>
        {
            option.SwaggerEndpoint($"/swagger/v1/swagger.json", "v1");
        });
        host.MapControllers();
        host.MapGrpcService<MySocketServerService>();
        await host.RunAsync();
    }
}
