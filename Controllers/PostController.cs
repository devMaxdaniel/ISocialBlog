using ISocialBlog.Data;
using ISocialBlog.Models;
using ISocialBlog.ViewModels;
using ISocialBlog.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISocialBlog.Controllers;

[ApiController]
public class PostController : ControllerBase
{

    [HttpGet("v1/posts")]
    public async Task<IActionResult> GetAsync(
        [FromServices] ISocialBlogDataContext context,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25)
    {
        var count = await context.Posts.AsNoTracking().CountAsync();
        var posts = await context
            .Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Select(x => new ListPostsViewModel {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                LastUpdateDate = x.LastUpdateDate,
                Category = x.Category.Name,
                Author = $"{x.Author.Name} ({x.Author.Email})"
            })
            .Skip(page * pageSize)
            .Take(pageSize)
            .OrderByDescending(x => x.LastUpdateDate)
            .ToListAsync();

        try
        {
            return Ok(new ResultViewModel<dynamic>(new {
                total = count,
                page,
                pageSize,
                posts
            }));
        } catch
        {
            return StatusCode(500, new ResultViewModel<List<Post>>("Falha interna no Servidor"));
        }
    }

    [HttpGet("v1posts/{id:int}")]
    public async Task<IActionResult> DetailAsync(
        [FromServices] ISocialBlogDataContext context,
        [FromRoute] int id)
    {
        try
        {
            var post = await context.Posts
                .AsNoTracking()
                .Include(x => x.Author)
                .ThenInclude(x => x.Roles)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
                return NotFound(new ResultViewModel<Post>("Conteudo não encontrado"));
            return Ok(new ResultViewModel<Post>(post));

        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResultViewModel<Post>("Falha interna no Servidor"));
        }
    }
}

