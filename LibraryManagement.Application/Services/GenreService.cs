using AutoMapper;
using LibraryManagement.Application.DTOs.GenreDtos;
using LibraryManagement.Application.Exceptions;
using LibraryManagement.Application.Services.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagement.Application.Services
{
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GenreService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GenreReadDto> GetGenreByIdAsync(int id)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(id);
            if (genre == null)
                throw new NotFoundException(nameof(Genre), id);

            return _mapper.Map<GenreReadDto>(genre);
        }

        public async Task<IEnumerable<GenreReadDto>> GetAllGenresAsync()
        {
            var genres = await _unitOfWork.Genres.GetAllAsync();
            return _mapper.Map<IEnumerable<GenreReadDto>>(genres);
        }

        public async Task<GenreReadDto> CreateGenreAsync(GenreCreateDto genreCreateDto)
        {
            await EnsureGenreNameIsUniqueAsync(genreCreateDto.Name);

            var genre = _mapper.Map<Genre>(genreCreateDto);
            await _unitOfWork.Genres.AddAsync(genre);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<GenreReadDto>(genre);
        }

        public async Task<bool> UpdateGenreAsync(GenreUpdateDto genreUpdateDto)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(genreUpdateDto.Id);
            if (genre == null)
                throw new NotFoundException(nameof(Genre), genreUpdateDto.Id);

            await EnsureGenreNameIsUniqueAsync(genreUpdateDto.Name, genreUpdateDto.Id);

            _mapper.Map(genreUpdateDto, genre);
            _unitOfWork.Genres.Update(genre);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteGenreAsync(int id)
        {
            var genre = await _unitOfWork.Genres.GetByIdAsync(id);
            if (genre == null)
                throw new NotFoundException(nameof(Genre), id);

            // Optional: Check if genre is associated with any books before deletion
            // var booksWithGenre = await _unitOfWork.Books.FindAsync(b => b.GenreId == id);
            // if (booksWithGenre.Any())
            // {
            //     throw new ValidationException("Cannot delete genre. It is currently associated with one or more books.");
            // }

            _unitOfWork.Genres.Remove(genre);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        // Helper method to check genre name uniqueness
        private async Task EnsureGenreNameIsUniqueAsync(string name, int? excludeId = null)
        {
            var normalizedName = name.ToLower();
            var existingGenres = await _unitOfWork.Genres.FindAsync(g => g.Name.ToLower() == normalizedName);

            var genreExists = excludeId.HasValue
                ? existingGenres.Any(g => g.Id != excludeId.Value)
                : existingGenres.Any();

            if (genreExists)
            {
                throw new ValidationException($"Genre with the name '{name}' already exists.");
            }
        }
    }
}
