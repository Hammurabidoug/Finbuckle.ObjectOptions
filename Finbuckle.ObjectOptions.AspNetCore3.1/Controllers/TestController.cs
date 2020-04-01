using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finbuckle.ObjectOptions
{
    public class TestController : Controller
    {
        private readonly IOptions<MySubOptions> mySubOptionsAccessor;

        public TestController(IOptions<MySubOptions> mySubOptionsAccessor)
        {
            this.mySubOptionsAccessor = mySubOptionsAccessor;
        }

        public IActionResult Index()
        {
            var options = this.mySubOptionsAccessor.Value;
            return new OkResult();
        }

    }
}
