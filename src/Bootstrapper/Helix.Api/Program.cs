using Shared.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCarterWithAssemblies(ChatAssemblies.All);

builder.Services
    .AddMediatRWithAssemblies(ChatAssemblies.All);

builder.Services
    .AddChatModule(builder.Configuration);

builder.Services
    .AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

app.MapCarter();
app.UseExceptionHandler(options => { });

app.UseChatModule();

app.Run();
