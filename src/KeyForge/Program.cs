using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────

builder.Services.Configure<LessonCatalogOptions>(
    builder.Configuration.GetSection(LessonCatalogOptions.SectionName));

// ── Infrastructure ─────────────────────────────────────────────────

builder.Services.AddSingleton<IYamlLessonParser, YamlLessonParser>();
builder.Services.AddSingleton<ILessonCatalog, FileSystemLessonCatalog>();
builder.Services.AddSingleton<IProgressStore, InMemoryProgressStore>();
builder.Services.AddSingleton<IExerciseAttemptRecorder, InMemoryExerciseAttemptRecorder>();

// ── Application Services ───────────────────────────────────────────

builder.Services.AddSingleton<IExerciseCompletionEvaluator, ExerciseCompletionEvaluator>();
builder.Services.AddSingleton<ILessonProgressionService, LessonProgressionService>();
builder.Services.AddSingleton<ILessonProgressQueryService, LessonProgressQueryService>();
builder.Services.AddSingleton<IExerciseEvaluator, ExerciseEvaluator>();
builder.Services.AddSingleton<IExerciseScorer, ExerciseScorer>();
builder.Services.AddSingleton<IPracticeSessionService, PracticeSessionService>();
builder.Services.AddSingleton<IProgressUpdateService, ProgressUpdateService>();

// ── Blazor ─────────────────────────────────────────────────────────

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var supportedCultures = new[] { new CultureInfo("fa"), new CultureInfo("en") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("fa");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = [new CookieRequestCultureProvider()];
});

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAntiforgery();

// ── Endpoints ──────────────────────────────────────────────────────

app.MapStaticAssets();
app.MapGet("/language/{language}", IResult (
    string language,
    string? returnUrl,
    HttpContext context) =>
{
    var normalizedLanguage = language.ToLowerInvariant();
    if (!UiText.IsSupportedLanguage(normalizedLanguage))
    {
        return Results.BadRequest();
    }

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(normalizedLanguage)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps
        });

    return Results.LocalRedirect(IsLocalReturnUrl(returnUrl) ? returnUrl! : "/");
}).ExcludeFromDescription();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
return;

static bool IsLocalReturnUrl(string? returnUrl) =>
    !string.IsNullOrWhiteSpace(returnUrl) &&
    returnUrl.StartsWith('/') &&
    !returnUrl.StartsWith("//", StringComparison.Ordinal) &&
    !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
