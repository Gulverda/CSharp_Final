﻿using AutoMapper;
using LibraryManagement.Application.DTOs.BookDtos;
using LibraryManagement.Application.Exceptions;
using LibraryManagement.Application.Services.Interfaces;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Interfaces;
using LibraryManagement.Application.Contracts.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagement.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<BookReadDto> GetBookByIdAsync(int id)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book == null)
                throw new NotFoundException(nameof(Book), id);

            await EnsureNavigationPropertiesLoadedAsync(book);
            return _mapper.Map<BookReadDto>(book);
        }

        public async Task<PaginatedBookResultDto> GetBooksAsync(BookFilterDto filter)
        {
            var (books, totalCount) = await _unitOfWork.Books.GetBooksAsync(
                filter.PageNumber,
                filter.PageSize,
                filter.MinPages,
                filter.MaxPages,
                filter.GenreId,
                filter.AuthorId,
                filter.SortBy,
                filter.SortDirection
            );

            var bookDtos = _mapper.Map<IEnumerable<BookReadDto>>(books);

            return new PaginatedBookResultDto
            {
                Items = bookDtos,
                TotalCount = totalCount,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<BookReadDto> CreateBookAsync(BookCreateDto bookCreateDto)
        {
            ValidatePublicationYear(bookCreateDto.PublicationYear);
            await ValidateAuthorAndGenreExistAsync(bookCreateDto.AuthorId, bookCreateDto.GenreId);

            var book = _mapper.Map<Book>(bookCreateDto);
            await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.CompleteAsync();

            var createdBook = await _unitOfWork.Books.GetByIdAsync(book.Id);
            await EnsureNavigationPropertiesLoadedAsync(createdBook);

            return _mapper.Map<BookReadDto>(createdBook);
        }

        public async Task<bool> UpdateBookAsync(BookUpdateDto bookUpdateDto)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(bookUpdateDto.Id);
            if (book == null)
                throw new NotFoundException(nameof(Book), bookUpdateDto.Id);

            ValidatePublicationYear(bookUpdateDto.PublicationYear);
            await ValidateAuthorAndGenreExistAsync(bookUpdateDto.AuthorId, bookUpdateDto.GenreId);

            _mapper.Map(bookUpdateDto, book);
            _unitOfWork.Books.Update(book);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            if (book == null)
                throw new NotFoundException(nameof(Book), id);

            _unitOfWork.Books.Remove(book);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        // Helper methods for cleaner code:

        private void ValidatePublicationYear(int year)
        {
            var currentYear = _dateTimeProvider.UtcNow.Year;
            if (year > currentYear || year < 1000)
                throw new ValidationException("Invalid Publication Year.");
        }

        private async Task ValidateAuthorAndGenreExistAsync(int authorId, int genreId)
        {
            if (!await _unitOfWork.Authors.ExistsAsync(a => a.Id == authorId))
                throw new ValidationException($"Author with ID {authorId} not found.");
            if (!await _unitOfWork.Genres.ExistsAsync(g => g.Id == genreId))
                throw new ValidationException($"Genre with ID {genreId} not found.");
        }

        private async Task EnsureNavigationPropertiesLoadedAsync(Book book)
        {
            if (book.Author == null && book.AuthorId > 0)
                book.Author = await _unitOfWork.Authors.GetByIdAsync(book.AuthorId);

            if (book.Genre == null && book.GenreId > 0)
                book.Genre = await _unitOfWork.Genres.GetByIdAsync(book.GenreId);
        }
    }
}
