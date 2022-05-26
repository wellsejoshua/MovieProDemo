using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProDemo.Data;
using MovieProDemo.Models.Database;
using MovieProDemo.Models.Settings;
using MovieProDemo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//Import Function 1(get):Gets movies from TMDB api and stores in custom database - responsible for getting previously imported movies from database to be displayed on view
//Import Function 2(post) display movies that have already been imported  - responsible for importing new movies based on user input passed from a form
namespace MovieProDemo.Controllers
{
  public class MoviesController : Controller
  {
    private readonly AppSettings _appSettings;
    private readonly ApplicationDbContext _context;
    private readonly IImageService _imageService;
    private readonly IRemoteMovieService _tmdbMovieService;
    private readonly IDataMappingService _tmdbMappingService;


    public MoviesController(IOptions<AppSettings> appSettings, ApplicationDbContext context, IImageService imageService, IRemoteMovieService tmdbMovieService, IDataMappingService tmdbMappingService)
    {
      _appSettings = appSettings.Value;
      _context = context;
      _imageService = imageService;
      _tmdbMovieService = tmdbMovieService;
      _tmdbMappingService = tmdbMappingService;
    }


    public async Task<IActionResult> Import()
    {
      var movies = await _context.Movie.ToListAsync();
      return View(movies);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(int id)
    {
      //if movie exists already warn user instead of importing it again...redirect user to Details page for that movie
      if (_context.Movie.Any(m => m.MovieId == id))
      {
        var localMovie = await _context.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
        return RedirectToAction("Details", "Movies", new { id = localMovie.Id, local = true });
      }

      //Step 1: Get the raw data from the API
      var movieDetail = await _tmdbMovieService.MovieDetailAsync(id);

      //Step 2: Run the data through a mapping procedure
      var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);

      //Step 3: Add the new movie
      _context.Add(movie);
      await _context.SaveChangesAsync();

      //Step 4: Assign it to the default All Collection
      await AddToMovieCollection(movie.Id, _appSettings.MovieProSettings.DefaultCollection.Name);

      return RedirectToAction("Import");

    }

    public async Task<IActionResult> Library()
    {
      var movies = await _context.Movie.ToListAsync();
      return View(movies);
    }

    private async Task AddToMovieCollection(int movieId, string collectionName)
    {
      var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);
      _context.Add(
          new MovieCollection()
          {
            CollectionId = collection.Id,
            MovieId = movieId
          }
       );
      //persists movie record to database
      await _context.SaveChangesAsync();

      
    }

    private async Task AddToMovieCollection(int movieId, int collectionId)
    {
      _context.Add(
        new MovieCollection()
        {
          CollectionId = collectionId,
          MovieId = movieId
        }
       );

      await _context.SaveChangesAsync();


    }
  }
}
