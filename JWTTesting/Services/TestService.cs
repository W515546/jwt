using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JWTTesting.Services
{
    public class TestService : ITestService {    
        public string GetTestMessages() {

            return "TestService.GetMessages() - Happy New Year!";

        }
    }
}