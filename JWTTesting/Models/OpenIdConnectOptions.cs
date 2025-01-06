using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JWTTesting.Models
{
    public class OpenIdConnectOptions
    {
        public string Authority { get; set; }

        public string Audience { get; set; }

        public string Scope { get; set; }

        public string RequireHttpsMetadata { get; set; }
    }
}