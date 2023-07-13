using ISocialBlog.ViewModels;
using ISocialBlog.Data;
using ISocialBlog.Extensions;
using ISocialBlog.Models;
using ISocialBlog.ViewModels;
using ISocialBlog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ISocialBlog.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {

        // GET ALL

        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices] IMemoryCache cache,
            [FromServices] ISocialBlogDataContext context)
        {
            try
            {
                var categories = cache.GetOrCreate("CategoriesCache", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });

                return Ok(new ResultViewModel<List<Category>>(categories));

            } catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("08XK15 - Falha Interna no servidor"));
            }
        }

        private List<Category> GetCategories(ISocialBlogDataContext context)
        {
            return context.Categories.ToList();
        }

        // GET BY ID
        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id,
            [FromServices] ISocialBlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
                if (category == null)
                {
                    return NotFound(new ResultViewModel<Category>("Contéudo não encontrado."));
                }

                return Ok(new ResultViewModel<Category>(category));
            } catch
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK12 - Falha Interna no servidor"));
            }
        }

        // POST
        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorCategoryViewModel model,
        [FromServices] ISocialBlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
            try
            {
                var category = new Category {
                    Id = 0,
                    Name = model.Name,
                    Slug = model.Slug.ToLower()
                };
                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
            } catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK9 - Não foi possível incluir a categoria "));
            } catch
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK10 - Falha Interna no servidor"));
            }
        }

        // PUT

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditorCategoryViewModel model,
            [FromServices] ISocialBlogDataContext context)
        {
            try
            {
                var category = await context.
                    Categories.
                    FirstOrDefaultAsync(x => x.Id == id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteudo não encontrado"));

                category.Name = model.Name;
                category.Slug = model.Slug;

                context.Categories.Update(category);
                await context.SaveChangesAsync();


                return Ok(new ResultViewModel<Category>(category));
            } catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK8 - Não foi possível alterar a categoria "));
            } catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK11 - Falha Interna no servidor"));
            }
        }

        // DELETE

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromServices] ISocialBlogDataContext context)
        {
            try
            {
                var category = await context.
                    Categories.
                    FirstOrDefaultAsync(x => x.Id == id);
                if (category == null)
                    return NotFound(new ResultViewModel<Category>("Conteúdo não encontrado."));

                context.Categories.Remove(category);
                await context.SaveChangesAsync();


                return Ok(new ResultViewModel<Category>(category));
            } catch (DbUpdateException ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK7 - Não foi possível excluir a categoria "));
            } catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<Category>("08XK14 - Falha Interna no servidor"));
            }
        }
    }

}



