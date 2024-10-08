
using Microsoft.Extensions.Options;
using HotelManager;
using System;
using Sigma.Core.Controllers;
using Sigma.Core.DataStorage;
using System.Web.Services.Description;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".MyApp.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(360000);
    options.Cookie.IsEssential = true;
});

Sigma.Core.Startup startup = new Sigma.Core.Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader());

app.UseDeveloperExceptionPage();

app.UseSession();

/*System.ServiceModel.BasicHttpBinding hotelManagerBinding = new System.ServiceModel.BasicHttpBinding();
hotelManagerBinding.MaxBufferSize = int.MaxValue;
hotelManagerBinding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
hotelManagerBinding.MaxReceivedMessageSize = int.MaxValue;
hotelManagerBinding.AllowCookies = true;
hotelManagerBinding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
hotelManagerBinding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;

var hotelManagerEndpoint = new System.ServiceModel.EndpointAddress("http://192.168.1.152/HotelManager/ws/ws2.1cws");

var client = new HotelManagerPortTypeClient(hotelManagerBinding, hotelManagerEndpoint);
client.ClientCredentials.UserName.UserName = "Service";
client.ClientCredentials.UserName.Password = "yandex";

var helloResult = await client.getHelloAsync();*/


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())

app.Use(async (context, next) =>
{
    // �������� ����� �������� ������� � ��������� middleware
    await next.Invoke();
    // �������� ����� ��������� ������� ��������� middleware
});
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCookiePolicy();


app.Services.GetService<StorageProvider>();

app.Services.GetService<AgreementDataStorage>();
app.Services.GetService<ClientDataStorage>();
app.Services.GetService<DocumentDataStorage>();
app.Services.GetService<LoginController>();
app.Services.GetService<MoneyStoreDataStorage>();
app.Services.GetService<OrganizationDataStorage>();
app.Services.GetService<ProductDataStorage>();
app.Services.GetService<SessionDataStorage>();
app.Services.GetService<StoreDataStorage>();

/*var dbContext = app.Services.GetService<DatabaseContext>();
if (dbContext != null)
{
    var city = dbContext.Cities.Find(1);
}*/

/*app.Run(async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    // ���� ��������� ���� �� ������ "/postuser", �������� ������ �����
    if (context.Request.Path == "/postuser")
    {
        var form = context.Request.Form;
        string name = form["name"];
        string age = form["age"];
        await context.Response.WriteAsync($"<div><p>Name: {name}</p><p>Age: {age}</p></div>");
    }
    else
    {
        await context.Response.SendFileAsync("html/index.html");
    }
});*/

app.MapControllers();

app.Run();
