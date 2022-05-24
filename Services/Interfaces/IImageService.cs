using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Services.Interfaces
{
  public interface IImageService
  {
    //first two make image data suitable for the database and the last one pulls from data base and makes byte array back into a picture
    Task<byte[]> EncodeImageAsync(IFormFile poster);
    Task<byte[]> EncodeImageURLAsync(string imageURL);
    string DecodeImage(byte[] poster, string contentType);

  }
}
