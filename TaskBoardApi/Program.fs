namespace TaskBoardApi
#nowarn "20"
open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open TaskBoardApi.Storage
open TaskBoardApi.Services

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder = WebApplication.CreateBuilder(args)

        // Add services to the container
        builder.Services.AddControllers()
            .AddJsonOptions(fun options ->
                options.JsonSerializerOptions.Converters.Add(System.Text.Json.Serialization.JsonStringEnumConverter())
                options.JsonSerializerOptions.Converters.Add(TaskStatusConverter())
            ) |> ignore

        // Register application services
        builder.Services.AddSingleton<InMemoryTaskStore>() |> ignore
        builder.Services.AddSingleton<TaskService>() |> ignore
        builder.Services.AddHostedService<JsonPersistenceService>() |> ignore

        // Add Swagger/OpenAPI
        builder.Services.AddSwaggerGen(fun c ->
            c.CustomSchemaIds(fun t -> t.FullName)
        ) |> ignore

        let app = builder.Build()

        // Configure the HTTP request pipeline
        if app.Environment.IsDevelopment() then
            app.UseSwagger() |> ignore
            app.UseSwaggerUI() |> ignore

        app.UseHttpsRedirection() |> ignore

        app.UseAuthorization() |> ignore
        app.MapControllers() |> ignore

        app.Run()

        exitCode
