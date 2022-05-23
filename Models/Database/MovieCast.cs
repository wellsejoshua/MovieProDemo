using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Models.Database
{
  public class MovieCast
  {
    public int Id { get; set; }
    public int MovieId { get; set; }

    //not a foreign key it is to store a cast id supplied bi tmdb
    public int CastID { get; set; }
    public string Department { get; set; }
    public string Name { get; set; }
    public string Character { get; set; }
    public string ImageUrl { get; set; }

    public Movie Movie { get; set; }
  }
}
