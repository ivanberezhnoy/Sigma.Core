using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sigma.Core.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Sigma.Core
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        { 
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddHttpContextAccessor();

            services.AddSingleton<SOAP1CCleintProviderController>();
            services.AddSingleton<ProductController>();
            services.AddSingleton<OrganizationController>();
            services.AddSingleton<MoneyStoreController>();
            services.AddSingleton<StoreController>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/login");
        }
    }
}
