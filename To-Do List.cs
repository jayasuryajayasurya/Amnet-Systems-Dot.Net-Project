using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var todos = new List<TodoItem>();

public record TodoItem(int Id, string Task, bool IsCompleted);

app.MapGet("/", async (HttpContext context) =>
{
    string todoListHtml = string.Join("", todos.Select(todo =>
        $@"
        <li>
            <span style='text-decoration: {(todo.IsCompleted ? "line-through" : "none")};'>
                {todo.Task}
            </span>
            <form method='post' action='/toggle/{todo.Id}' style='display:inline;'>
                <button type='submit'>Toggle</button>
            </form>
            <form method='post' action='/delete/{todo.Id}' style='display:inline;'>
                <button type='submit'>Delete</button>
            </form>
        </li>
        "));

    string html = $@"
        <html>
        <head>
            <title>To-Do List</title>
        </head>
        <body>
            <h1>To-Do List</h1>
            <form method='post' action='/add'>
                <input type='text' name='task' placeholder='New Task' required>
                <button type='submit'>Add</button>
            </form>
            <ul>
                {todoListHtml}
            </ul>
        </body>
        </html>";

    await context.Response.WriteAsync(html);
});
app.MapPost("/add", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    string task = form["task"];

    if (!string.IsNullOrEmpty(task))
    {
        int nextId = todos.Any() ? todos.Max(t => t.Id) + 1 : 1;
        todos.Add(new TodoItem(nextId, task, false));
    }

    context.Response.Redirect("/");
});
app.MapPost("/toggle/{id:int}", (HttpContext context, int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo != null)
    {
        todos.Remove(todo);
        todos.Add(todo with { IsCompleted = !todo.IsCompleted });
    }

    context.Response.Redirect("/");
});

app.MapPost("/delete/{id:int}", (HttpContext context, int id) =>
{
    var todo = todos.FirstOrDefault(t => t.Id == id);
    if (todo != null)
    {
        todos.Remove(todo);
    }

    context.Response.Redirect("/");
});
app.Run();
