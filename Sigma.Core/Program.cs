
using Microsoft.Extensions.Options;
using HotelManager;
using System;
using Sigma.Core.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.AllowSynchronousIO = true;
});

Sigma.Core.Startup startup = new Sigma.Core.Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.UseDeveloperExceptionPage();

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
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.Services.GetService<SOAP1CCleintProviderController>();
app.Services.GetService<ProductController>();
app.Services.GetService<OrganizationController>();
app.Services.GetService<MoneyStoreController>();
app.Services.GetService<StoreController>();

/*var dbContext = app.Services.GetService<DatabaseContext>();
if (dbContext != null)
{
    var city = dbContext.Cities.Find(1);
}*/

/*app.Run(async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";

    // если обращение идет по адресу "/postuser", получаем данные формы
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
