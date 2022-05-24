using MovieProDemo.Models.Database;
using MovieProDemo.Models.TMDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Models.ViewModels
{
  //this class is not turned into a table it is used to aggregate data many times from many sources...it helps a view work with complex data
  public class LandingPageVM
  {
    public List<Collection> CustomCollections { get; set; }
    public MovieSearch NowPlaying { get; set; }
    public MovieSearch Popular { get; set; }
    public MovieSearch TopRated { get; set; }
    public MovieSearch Upcoming { get; set; }
  }
}
