using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Añadimos autenticacion
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcion => {
        opcion.LoginPath = "/Auth/Index";
        opcion.LoginPath = "/Auth/Registrar";
        opcion.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        opcion.AccessDeniedPath = "/Auth/Privacy";
    });

builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();
app.UseSession();

app.MapControllerRoute(

  name: "default",

  pattern: "{controller=ECommerce}/{action=Portal}/{id?}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ecommerce}/{action=Index}/{id?}");

app.Run();
