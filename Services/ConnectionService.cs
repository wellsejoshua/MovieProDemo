using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieProDemo.Services
{
  //builds connection string to communicate with database...dont use to many static classes due to using long term heap memory
  public class ConnectionService
  {
    public static string GetConnectionString(IConfiguration configuration)
    {
      //this may need to be altered to work with user secrets
      var connectionString = configuration.GetConnectionString("DefaultConnection");
      var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
      return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
    }

    private static string BuildConnectionString(string databaseUrl)
    {
      //does best to take in database url string and turn it into a viable connection string
      //incoming url string is turned into an instantce of the uri class
      //because this is static it doesn't have to be registered as a service 
      
      var databaseUri = new Uri(databaseUrl);
      var userInfo = databaseUri.UserInfo.Split(';');
      var builder = new NpgsqlConnectionStringBuilder
      {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true


      };
      return builder.ToString();
    }
  }
}
