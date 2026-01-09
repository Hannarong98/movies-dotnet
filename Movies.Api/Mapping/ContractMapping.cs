using Movies.Application.Model;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{

    extension(CreateMovieRequest request)
    {
        public Movie MapToMovie()
        {
            return new Movie
            {
                Id = Guid.NewGuid(),
                Genres = request.Genres.ToList(),
                Title = request.Title,
                YearOfRelease = request.YearOfRelease
            };
        }
    }

    extension(UpdateMovieRequest request)
    {
        public Movie MapToMovie(Guid id)
        {
            return new Movie
            {
                Id = id,
                Genres = request.Genres.ToList(),
                Title = request.Title,
                YearOfRelease = request.YearOfRelease
            };
        }
    }

    extension(Movie movie)
    {
        public MovieResponse MapToResponse()
        {
            return new MovieResponse
            {
                Genres = movie.Genres,
                Id = movie.Id,
                Title = movie.Title,
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                YearOfRelease = movie.YearOfRelease,
                Slug = movie.Slug
            };
        }
    }

    extension(IEnumerable<Movie> movies)
    {
        public MoviesResponse MapToResponse(int page, int pageSize, int totalCount)
        {
            return new MoviesResponse
            {
                Items = movies.Select(MapToResponse),
                Page = page,
                PageSize = pageSize,
                Total = totalCount
            };
        }
    }

    extension(IEnumerable<MovieRating> ratings)
    {
        public IEnumerable<MovieRatingResponse> MapToResponse()
        {
            return ratings.Select(x => new MovieRatingResponse
            {
                Rating = x.Rating,
                Slug = x.Slug,
                MovieId = x.MovieId
            });
        }
    }

    extension(GetAllMoviesRequest request)
    {
        public GetAllMoviesOption MapToOptions()
        {
            return new GetAllMoviesOption
            {
                Title = request.Title,
                YearOfRelease = request.Year,
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ? SortOrder.Unsorted :
                    request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }

    extension(GetAllMoviesOption option)
    {
        public GetAllMoviesOption WithUser(Guid? userId)
        {
            option.UserId = userId;
            return option;
        }
    }

   
}