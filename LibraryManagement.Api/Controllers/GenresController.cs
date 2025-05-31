using LibraryManagement.Application.DTOs.GenreDtos;
using LibraryManagement.Application.Services.Interfaces;
using LibraryManagement.Infrastructure.Identity;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace LibraryManagement.Api.Controllers
{
    [RoutePrefix("api/genres")]
    public class GenresController : ApiController
    {
        private readonly IGenreService _genreService;

        public GenresController(IGenreService genreService)
        {
            _genreService = genreService ?? throw new System.ArgumentNullException(nameof(genreService));
        }

        /// <summary>
        /// Get all genres.
        /// </summary>
        /// <returns>List of all genres.</returns>
        [HttpGet]
        [Route("")]
        [Authorize(Roles = Roles.User + "," + Roles.Admin)]
        [ResponseType(typeof(IEnumerable<GenreReadDto>))]
        public async Task<IHttpActionResult> GetAllGenres()
        {
            var genres = await _genreService.GetAllGenresAsync();
            return Ok(genres);
        }

        /// <summary>
        /// Get genre by ID.
        /// </summary>
        /// <param name="id">Genre ID.</param>
        /// <returns>Genre details.</returns>
        [HttpGet]
        [Route("{id:int}", Name = "GetGenreById")]
        [Authorize(Roles = Roles.User + "," + Roles.Admin)]
        [ResponseType(typeof(GenreReadDto))]
        public async Task<IHttpActionResult> GetGenre(int id)
        {
            var genre = await _genreService.GetGenreByIdAsync(id);
            return Ok(genre);
        }

        /// <summary>
        /// Create a new genre.
        /// </summary>
        /// <param name="genreCreateDto">Genre creation data.</param>
        /// <returns>Created genre details.</returns>
        [HttpPost]
        [Route("")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(GenreReadDto))]
        public async Task<IHttpActionResult> CreateGenre([FromBody] GenreCreateDto genreCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdGenre = await _genreService.CreateGenreAsync(genreCreateDto);
            return CreatedAtRoute("GetGenreById", new { id = createdGenre.Id }, createdGenre);
        }

        /// <summary>
        /// Update an existing genre.
        /// </summary>
        /// <param name="id">Genre ID to update.</param>
        /// <param name="genreUpdateDto">Updated genre data.</param>
        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateGenre(int id, [FromBody] GenreUpdateDto genreUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != genreUpdateDto.Id)
                return BadRequest("ID mismatch in route and body.");

            await _genreService.UpdateGenreAsync(genreUpdateDto);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete a genre by ID.
        /// </summary>
        /// <param name="id">Genre ID to delete.</param>
        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteGenre(int id)
        {
            await _genreService.DeleteGenreAsync(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
