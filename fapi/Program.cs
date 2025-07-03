using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FruitDB>(options => options.UseInMemoryDatabase("FruitList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Fruit API", Version = "v1", Description = "An API to manage fruits in stock" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<FruitDB>();
    dbContext.Database.EnsureCreated();
}

app.MapGet("/fruitlist", async (FruitDB db) => await db.Fruits.ToListAsync())
    .WithTags("Get all fruit");

app.MapGet("/fruitlist/instock", async (FruitDB db) => await db.Fruits.Where(t => t.InStock).ToListAsync())
    .WithTags("Get all fruit that is in stock");

app.MapGet("/fruitlist/{id}", async (int id, FruitDB db) => await db.Fruits.FindAsync(id) is Fruit fruit
    ? Results.Ok(fruit ) : Results.NotFound())
    .WithTags("Get fruit by id");

app.MapPost("/fruitlist", async (Fruit fruit, FruitDB db) =>
{
    db.Fruits.Add(fruit);
    await db.SaveChangesAsync();
    return Results.Created($"/fruitlist/{fruit.Id}", fruit);
})
.WithTags("Add a new fruit");

app.MapPut("/fruitlist/{id}", async (int id, Fruit inputFruit, FruitDB db) =>
{
    var fruit = await db.Fruits.FindAsync(id);
    if (fruit is null) return Results.NotFound();

    fruit.Name = inputFruit.Name;
    fruit.InStock = inputFruit.InStock;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithTags("Update an existing fruit");
app.MapDelete("/fruitlist/{id}", async (int id, FruitDB db) =>
{
    if (await db.Fruits.FindAsync(id) is Fruit fruit)
    {
        db.Fruits.Remove(fruit);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
})
.WithTags("Delete a fruit");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
