using LibraryManagement.Application.DTOs.AuthorDtos;
using LibraryManagement.Application.Services.Interfaces;
using LibraryManagement.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace LibraryManagement.Api.Controllers
{
    [RoutePrefix("api/authors")]
    public class AuthorsController : ApiController
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService ?? throw new ArgumentNullException(nameof(authorService));
        }

        // GET: api/authors
        [HttpGet]
        [Route("")]
        [Authorize(Roles = Roles.User + "," + Roles.Admin)]
        [ResponseType(typeof(IEnumerable<AuthorReadDto>))]
        public async Task<IHttpActionResult> GetAllAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(authors);
        }

        // GET: api/authors/{id}
        [HttpGet]
        [Route("{id:int}", Name = "GetAuthorById")]
        [Authorize(Roles = Roles.User + "," + Roles.Admin)]
        [ResponseType(typeof(AuthorReadDto))]
        public async Task<IHttpActionResult> GetAuthor(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
                return NotFound();

            return Ok(author);
        }

        // POST: api/authors
        [HttpPost]
        [Route("")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(AuthorReadDto))]
        public async Task<IHttpActionResult> CreateAuthor([FromBody] AuthorCreateDto authorCreateDto)
        {
            if (authorCreateDto == null)
                return BadRequest("Author data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdAuthor = await _authorService.CreateAuthorAsync(authorCreateDto);
                return CreatedAtRoute("GetAuthorById", new { id = createdAuthor.Id }, createdAuthor);
            }
            catch (NotImplementedException)
            {
                return InternalServerError(new Exception("Author creation logic is not implemented."));
            }
        }

        // PUT: api/authors/{id}
        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateAuthor(int id, [FromBody] AuthorUpdateDto authorUpdateDto)
        {
            if (authorUpdateDto == null)
                return BadRequest("Author data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != authorUpdateDto.Id)
                return BadRequest("ID mismatch between route and payload.");

            await _authorService.UpdateAuthorAsync(authorUpdateDto);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/authors/{id}
        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteAuthor(int id)
        {
            await _authorService.DeleteAuthorAsync(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
