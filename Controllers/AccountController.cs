using System.Text.RegularExpressions;
using ISocialBlog.Data;
using ISocialBlog.Extensions;
using ISocialBlog.Models;
using ISocialBlog.Services;
using ISocialBlog.ViewModels;
using ISocialBlog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace ISocialBlog.Controllers
{

    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts/")]
        public async Task<IActionResult> Post(
            [FromBody] RegisterViewModel model,
            [FromServices] EmailService emailService,
            [FromServices] ISocialBlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = new User() {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            var password = PasswordGenerator.Generate(25);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(user.Name, user.Email,
                    subject: "Bem vindo",
                    body: $"Sua senha é: >  {password}  < ");

                return Ok(new ResultViewModel<dynamic>(new {
                    user = user.Email,
                    password
                }));
            } catch (DbUpdateException)
            {
                return StatusCode(400, new ResultViewModel<string>("085X99 - Este E-mail já está em uso."));
            } catch
            {
                return StatusCode(500, new ResultViewModel<string>("085X04 - Falha interna no Servidor."));
            }
        }


        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model,
            [FromServices] ISocialBlogDataContext context,
            [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = await context.Users.AsNoTracking().Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token, null));
            } catch
            {
                return StatusCode(500, new ResultViewModel<string>("05X84 - Falha interna no servidor"));
            }
        }


        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage(
            [FromBody] UploadImageViewModel model,
            [FromServices] ISocialBlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image \/ [a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            } catch (Exception e)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna no servidor"));
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user == null)
                return NotFound(new ResultViewModel<Category>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            } catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha Interna no Servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso", null));
        }

    }
}
