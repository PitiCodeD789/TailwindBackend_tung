using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TailwindBackend.Commond
{
    public class CommondData
    {
        public static readonly string authSecret = "7gX4iRjWMRGWoqezlS1Xt2ZtQyZQQxuMIJRt8ws4";
        public static readonly string baseUrl = "https://biddingauction-4edd5.firebaseio.com/";
        public FirebaseClient Firebase { get; set; }
        private readonly IConfiguration _configuration;

        public CommondData()
        {
            Firebase = new FirebaseClient(baseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(authSecret) });
        }
    }
}