using RowdyHacks25.Data;
using RowdyHacks25.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// JSON “DB” path and registrations
var contentRoot = builder.Environment.ContentRootPath;
var dbPath = Path.Combine(contentRoot, "App_Data", "db.json");

builder.Services.AddSingleton(new JsonFileDatabase(dbPath));
builder.Services.AddSingleton<IBountyRepository, BountyRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

// Gemini summarizer
builder.Services.AddHttpClient<IGeminiSummarizer, GeminiSummarizer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages().WithStaticAssets();

app.Run();