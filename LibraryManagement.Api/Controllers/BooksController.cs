using LibraryManagement.Application.DTOs.BookDtos;
using LibraryManagement.Application.Services.Interfaces;
using LibraryManagement.Infrastructure.Identity;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace LibraryManagement.Api.Controllers
{
    [RoutePrefix("api/books")]
    public class BooksController : ApiController
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        }

        /// <summary>
        /// Get a paginated list of books with optional filters.
        /// </summary>
        /// <param name="filter">Filter and pagination parameters.</param>
        /// <returns>Paginated list of books.</returns>
        [HttpGet]
        [Route("")]
        [Authorize(Roles = Roles.User + "," + Roles.Admin)]
        [ResponseType(typeof(PaginatedBookResultDto))]
        public async Task<IHttpActionResult> GetBooks([FromUri] BookFilterDto filter)
        {
            filter = new BookFilterDto();

            var paginatedResult = await _bookService.GetBooksAsync(filter);
            return Ok(paginatedResult);
        }

        /// <summary>
        /// Get a book by its ID.
        /// </summary>
        /// <param name="id">Book ID.</param>
        /// <returns>The book details.</returns>
        [HttpGet]
        [Route("{id:int}", Name = "GetBookById")]
        [Authorize(Roles = Roles.User + "," + Roles.Admin)]
        [ResponseType(typeof(BookReadDto))]
        public async Task<IHttpActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            return Ok(book);
        }

        /// <summary>
        /// Create a new book entry.
        /// </summary>
        /// <param name="bookCreateDto">Book creation data.</param>
        /// <returns>Created book details.</returns>
        [HttpPost]
        [Route("")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(BookReadDto))]
        public async Task<IHttpActionResult> CreateBook([FromBody] BookCreateDto bookCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdBook = await _bookService.CreateBookAsync(bookCreateDto);
            return CreatedAtRoute("GetBookById", new { id = createdBook.Id }, createdBook);
        }

        /// <summary>
        /// Update an existing book.
        /// </summary>
        /// <param name="id">ID of the book to update.</param>
        /// <param name="bookUpdateDto">Updated book data.</param>
        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateBook(int id, [FromBody] BookUpdateDto bookUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != bookUpdateDto.Id)
                return BadRequest("ID mismatch in route and body.");

            await _bookService.UpdateBookAsync(bookUpdateDto);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete a book by its ID.
        /// </summary>
        /// <param name="id">ID of the book to delete.</param>
        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> DeleteBook(int id)
        {
            await _bookService.DeleteBookAsync(id);
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}
