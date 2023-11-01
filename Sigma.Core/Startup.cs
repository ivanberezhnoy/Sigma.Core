using Sigma.Core.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sigma.Core.DataStorage;

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

            services.AddSingleton<AgreementDataStorage>();
            services.AddSingleton<ClientDataStorage>();
            services.AddSingleton<DocumentDataStorage>();
            services.AddSingleton<MoneyStoreDataStorage>();
            services.AddSingleton<OrganizationDataStorage>();
            services.AddSingleton<ProductDataStorage>();
            services.AddSingleton<SessionDataStorage>();
            services.AddSingleton<StorageProvider>();
            services.AddSingleton<StoreDataStorage>();

            services.AddTransient<ClientController>();
            services.AddTransient<DocumentController>();
            services.AddTransient<LoginController>();
            services.AddTransient<MoneyStoreController>();
            services.AddTransient<OrganizationController>();
            services.AddTransient<ProductController>();
            services.AddTransient<StoreController>();
            services.AddTransient<UserController>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/login");
        }
    }
}
