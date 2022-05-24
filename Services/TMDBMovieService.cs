using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MovieProDemo.Enums;
using MovieProDemo.Models.Settings;
using MovieProDemo.Models.TMDB;
using MovieProDemo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace MovieProDemo.Services
{
  public class TMDBMovieService : IRemoteMovieService
  {

    private readonly AppSettings _appSettings;
    private readonly IHttpClientFactory _httpClient;

    public TMDBMovieService(IOptions<AppSettings> appSettings, IHttpClientFactory httpClient)
    {
      _appSettings = appSettings.Value;
      _httpClient = httpClient;
    }

    public async Task<ActorDetail> ActorDetailAsync(int id)
    {
      //Step 1: Setup a default return object
      ActorDetail actorDetail = new();

      //Step 2: Assemble the full request uri string
      var query = $"{_appSettings.TMDBSettings.BaseUrl}/person/{id}";
      var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieProSettings.TmDbApiKey },
                { "language", _appSettings.TMDBSettings.QueryOptions.Language}
            };
      var requestUri = QueryHelpers.AddQueryString(query, queryParams);

      //Step 3: Create a client and execute the request
      var client = _httpClient.CreateClient();
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      var response = await client.SendAsync(request);

      //Step 4: Return the ActorDetail object
      if (response.IsSuccessStatusCode)
      {
        using var responseStream = await response.Content.ReadAsStreamAsync();

        var dcjs = new DataContractJsonSerializer(typeof(ActorDetail));
        actorDetail = (ActorDetail)dcjs.ReadObject(responseStream);
      }

      return actorDetail;
    }

    public async Task<MovieDetail> MovieDetailAsync(int id)
    {
      //Step 1: Setup default return object
      MovieDetail movieDetail = new();

      //Step 2: Assemble the request
      var query = $"{_appSettings.TMDBSettings.BaseUrl}/movie/{id}";
      var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieProSettings.TmDbApiKey },
                { "language", _appSettings.TMDBSettings.QueryOptions.Language},
                { "append_to_response", _appSettings.TMDBSettings.QueryOptions.AppendToResponse}
            };
      var requestUri = QueryHelpers.AddQueryString(query, queryParams);

      //Step 3: Create client and execute request
      var client = _httpClient.CreateClient();
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      var response = await client.SendAsync(request);

      //Step 4: Deserialize into Moviedetail 
      if (response.IsSuccessStatusCode)
      {
        using var responseStream = await response.Content.ReadAsStreamAsync();
        var dcjs = new DataContractJsonSerializer(typeof(MovieDetail));
        movieDetail = dcjs.ReadObject(responseStream) as MovieDetail;
      }
      return movieDetail;
    }

    public async Task<MovieSearch> SearchMoviesAsync(MovieCategory category, int count)
    {
      //Step 1: Setup a default instance of MovieSearch
      MovieSearch movieSearch = new();

      // Step 2: assemble the full request uri string (to TMDB endpoint)
      var query = $"{_appSettings.TMDBSettings.BaseUrl}/movie/{category}";
      //adding to end of query string
      var queryParams = new Dictionary<string, string>()
      {
        {"api_key", _appSettings.MovieProSettings.TmDbApiKey },
        {"language", _appSettings.TMDBSettings.QueryOptions.Language },
        {"page", _appSettings.TMDBSettings.QueryOptions.Page}
      };

      var requestUri = QueryHelpers.AddQueryString(query, queryParams);

      //Step 3 create an Http client and execute the request
      var client = _httpClient.CreateClient();
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      //sends off request asynchronously and stores response which is incoming json object from tmdb
      var response = await client.SendAsync(request);

      //Step 4: return the MovieSearch object (deserialize the response)
      if (response.IsSuccessStatusCode)
      {
        var dcjs = new DataContractJsonSerializer(typeof(MovieSearch));
        //to manage garbage collection more aggressively
        using var responseStream = await response.Content.ReadAsStreamAsync();
        //update value of movieSearch object at top of code
        movieSearch = (MovieSearch)dcjs.ReadObject(responseStream);
        movieSearch.results = movieSearch.results.Take(count).ToArray();
        movieSearch.results.ToList().ForEach(r => r.poster_path = $"{_appSettings.TMDBSettings.BaseImagePath}/{_appSettings.MovieProSettings.DefaultPosterSize}/{r.poster_path}");
      }

      return movieSearch;
    }
  }
}
