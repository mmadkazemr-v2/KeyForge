var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<LessonCatalogOptions>(
    builder.Configuration.GetSection(LessonCatalogOptions.SectionName));

builder.Services.AddSingleton<IYamlLessonParser, YamlLessonParser>();
builder.Services.AddSingleton<ILessonCatalog, FileSystemLessonCatalog>();
builder.Services.AddSingleton<IProgressStore, InMemoryProgressStore>();
builder.Services.AddSingleton<ILessonProgressionService, LessonProgressionService>();
builder.Services.AddSingleton<ILessonProgressQueryService, LessonProgressQueryService>();
builder.Services.AddSingleton<IExerciseAttemptRecorder, InMemoryExerciseAttemptRecorder>();
builder.Services.AddSingleton<IExerciseEvaluator, ExerciseEvaluator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
