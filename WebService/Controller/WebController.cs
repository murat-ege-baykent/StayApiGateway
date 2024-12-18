 using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class WebController : ControllerBase
    {
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<string>>> GetAll()
        {
            return new string[] { "Dhananjay", "Nidhish", "Vijay","Nazim","Alpesh" };
        }
    }