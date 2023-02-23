
using Sigma.Core.DatabaseEntity;

var builder = WebApplication.CreateBuilder(args);


Sigma.Core.Startup startup = new Sigma.Core.Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

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
