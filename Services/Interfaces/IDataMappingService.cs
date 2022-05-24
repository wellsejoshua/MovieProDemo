using MovieProDemo.Models.Database;
using MovieProDemo.Models.TMDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Services.Interfaces
{
  // service that carries data from api into my database
  public interface IDataMappingService
  {
    Task<Movie> MapMovieDetailAsync(MovieDetail movie);
    ActorDetail MapActorDetail(ActorDetail actor);


  }
}
