﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Models.Settings
{
  public class AppSettings
  {
    public  MovieProSettings MovieProSettings { get; set; }
    public  TMDBSettings TMDBSettings { get; set; }
  }
}
