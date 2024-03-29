﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    // GET: Temporary/Create
    public IActionResult Create()
    {
      ViewData["CollectionId"] = new SelectList(_context.Collection, "Id", "Name");
      return View();
    }

    // POST: Temporary/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie, int collectionId)
    {
      if (ModelState.IsValid)
      {

        movie.PosterType = movie.PosterFile?.ContentType;
        movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);

        movie.BackdropType = movie.BackdropFile?.ContentType;
        movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);

        _context.Add(movie);
        await _context.SaveChangesAsync();

        await AddToMovieCollection(movie.Id, collectionId);

        return RedirectToAction("Index", "MovieCollections");
      }
      return View(movie);
    }

    // GET: Temporary/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var movie = await _context.Movie.FindAsync(id);
      if (movie == null)
      {
        return NotFound();
      }
      return View(movie);
    }

    // POST: Temporary/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie)
    {
      if (id != movie.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          if(movie.PosterFile is not null)
          {
            movie.PosterType = movie.PosterFile?.ContentType;
            movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);
          }
          if(movie.BackdropFile is not null)
          {
            movie.BackdropType = movie.BackdropFile?.ContentType;
            movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);
          }

          _context.Update(movie);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!MovieExists(movie.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        //details action of movies controller with route of movie id
        return RedirectToAction("Details", "Movies", new { id = movie.Id, local = true });
      }
      return View(movie);
    }

    // GET: Temporary/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var movie = await _context.Movie
          .FirstOrDefaultAsync(m => m.Id == id);
      if (movie == null)
      {
        return NotFound();
      }

      return View(movie);
    }

    // POST: Temporary/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var movie = await _context.Movie.FindAsync(id);
      _context.Movie.Remove(movie);
      await _context.SaveChangesAsync();
      return RedirectToAction("Library", "Movies");
    }

    private bool MovieExists(int id)
    {
      return _context.Movie.Any(e => e.Id == id);
    }

    public async Task<IActionResult> Details(int? id, bool local = false)
    {
      if (id == null)
      {
        return NotFound();
      }
      //if movie is not null try to find in database
      //incoming bool tells you if you should look for it locally or grab it from the database

      Movie movie = new();
      if(local)
      {
        movie = await _context.Movie.Include(m => m.Cast).Include(m => m.Crew).FirstOrDefaultAsync(m => m.Id == id);
      }
      else
      {
        //if bool false data is coming from the api  (casting nullable int to int
        var movieDetail = await _tmdbMovieService.MovieDetailAsync((int)id);
        movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);
      }
      if(movie == null)
      {
        return NotFound();
      }

      ViewData["Local"] = local;
      return View(movie);
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
