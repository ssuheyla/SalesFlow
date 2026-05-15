using Microsoft.EntityFrameworkCore;
using SCProject.Models.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // JSON property isimlerini olduðu gibi býrakýr (camelCase dönüþümü yapmaz)
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddDbContext<TestDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"))); //db baðlantýsý

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
