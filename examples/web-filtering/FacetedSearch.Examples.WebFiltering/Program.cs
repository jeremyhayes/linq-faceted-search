using Microsoft.EntityFrameworkCore;
using FacetedSearch.Examples.WebFiltering.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<SpellsContext>(options =>
    options.UseSqlite("Data Source=spells.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// create a new database on startup
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
using var context = services.GetRequiredService<SpellsContext>();
context.Database.EnsureDeleted();
context.Database.EnsureCreated();
context.Spell.Count(); // force first use on startup

app.Run();
