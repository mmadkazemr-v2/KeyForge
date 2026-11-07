using KeyForge.Infrastructure.Persistence.Progress;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────

builder.Services.Configure<LessonCatalogOptions>(
    builder.Configuration.GetSection(LessonCatalogOptions.SectionName));

// ── Infrastructure ─────────────────────────────────────────────────

builder.Services.AddSingleton<IYamlLessonParser, YamlLessonParser>();
builder.Services.AddSingleton<ILessonCatalog, FileSystemLessonCatalog>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JsonPracticeStore>(services =>
{
    var context = services.GetRequiredService<IHttpContextAccessor>().HttpContext;
    if (context?.Items["PracticeProfile"] is not string profile)
    {
        throw new InvalidOperationException("Practice profile unavailable. Reload the application.");
    }

    var root = builder.Configuration["PracticeStorage:Directory"]
               ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Practice");

    return new JsonPracticeStore(Path.Combine(root, profile + ".json"));
});
builder.Services.AddScoped<IProgressStore>(s => s.GetRequiredService<JsonPracticeStore>());
builder.Services.AddScoped<IExerciseAttemptRecorder>(s => s.GetRequiredService<JsonPracticeStore>());

// ── Application Services ───────────────────────────────────────────

builder.Services.AddScoped<IExerciseCompletionEvaluator, ExerciseCompletionEvaluator>();
builder.Services.AddScoped<ILessonProgressionService, LessonProgressionService>();
builder.Services.AddScoped<ILessonProgressQueryService, LessonProgressQueryService>();
builder.Services.AddScoped<IPracticeLibraryQueryService, PracticeLibraryQueryService>();
builder.Services.AddSingleton<IExerciseChallengeGenerator, ExerciseChallengeGenerator>();
builder.Services.AddSingleton<IExerciseEvaluator, ExerciseEvaluator>();
builder.Services.AddSingleton<IExerciseScorer, ExerciseScorer>();
builder.Services.AddScoped<IPracticeSessionService, PracticeSessionService>();
builder.Services.AddScoped<IProgressUpdateService, ProgressUpdateService>();
builder.Services.AddScoped<IAudioPlaybackService, AudioPlaybackService>();
builder.Services.AddScoped<MidiInputRouter>();
builder.Services.AddScoped<MidiPracticeInputState>();

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
app.Use(async (context, next) =>
{
    const string cookie = "KeyForge.PracticeProfile";
    if (!Guid.TryParseExact(context.Request.Cookies[cookie], "N", out var profileId))
    {
        profileId = Guid.NewGuid();
        context.Response.Cookies.Append(cookie, profileId.ToString("N"),
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = context.Request.IsHttps,
                MaxAge = TimeSpan.FromDays(3650)
            });
    }

    context.Items["PracticeProfile"] = profileId.ToString("N");
    await next();
});
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

static bool IsLocalReturnUrl(string? returnUrl)
    => !string.IsNullOrWhiteSpace(returnUrl)
       && returnUrl.StartsWith('/')
       && !returnUrl.StartsWith("//", StringComparison.Ordinal)
       && !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
