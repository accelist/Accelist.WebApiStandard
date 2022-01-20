using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExplodeController : ControllerBase
    {
        // GET: api/<ExplodeController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            throw new Exception("Something happened!");
        }

        // GET api/<ExplodeController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ExplodeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ExplodeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ExplodeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
