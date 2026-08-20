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

builder.Services.AddSingleton<ILessonProgressionService, LessonProgressionService>();
builder.Services.AddSingleton<ILessonProgressQueryService, LessonProgressQueryService>();
builder.Services.AddSingleton<IExerciseEvaluator, ExerciseEvaluator>();
builder.Services.AddSingleton<IExerciseScorer, ExerciseScorer>();
builder.Services.AddSingleton<IPracticeSessionService, PracticeSessionService>();

// ── Blazor ─────────────────────────────────────────────────────────

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

// ── Endpoints ──────────────────────────────────────────────────────

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
