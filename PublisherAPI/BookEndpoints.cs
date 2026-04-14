using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using PublisherData;
using PublisherDomain;
namespace PublisherAPI;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Book").WithTags(nameof(Book));

        group.MapGet("/", async (PubContext db) =>
        {
            return await db.Books.Include(b => b.Author).AsNoTracking().ToListAsync();
        })
        .WithName("GetAllBooks");

        group.MapGet("/{bookid}", async Task<Results<Ok<Book>, NotFound>> (int bookid, PubContext db) =>
        {
            return await db.Books.AsNoTracking()
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BookId == bookid)
                is Book model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetBookById");

        group.MapPost("/", async (Book book, PubContext db) =>
        {
            db.Books.Add(book);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Book/{book.BookId}", book);
        })
        .WithName("CreateBook");

        group.MapPut("/{bookid}", async Task<Results<Ok, NotFound>> (int bookid, Book book, PubContext db) =>
        {
            var affected = await db.Books
                .Where(b => b.BookId == bookid)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.AuthorId, book.AuthorId)
                    .SetProperty(m => m.Title, book.Title)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateBook");
    }
    
}