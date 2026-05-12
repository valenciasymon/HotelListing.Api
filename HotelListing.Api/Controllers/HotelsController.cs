using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HotelListing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
            new Hotel(1,"Monarch Hotel","Calasiao",.3),
            new Hotel(2,"Azalea Hotel","Baguio",7.6),
            new Hotel(3,"Lavander Hotel","Singapore",8.2)
        };
        // GET: api/<HotelsController>
        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> Get()
        {
            return Ok(hotels);
        }

        // GET api/<HotelsController>/5
        [HttpGet("{id}")]
        public ActionResult<Hotel> Get(int id)
        {
            Hotel hotel = hotels.FirstOrDefault(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound(new {message = "Hotel Not Found"});
            }
            return Ok(hotel);
        }

        // POST api/<HotelsController>
        [HttpPost]
        public ActionResult Post([FromBody] Hotel newHotel)
        {
            Hotel existingHotel = hotels.FirstOrDefault(h => h.Id == newHotel.Id);

            if (existingHotel != null)
            {
                return BadRequest(new {message ="Hotel With This Id Already Exist"});
            }

            hotels.Add(newHotel);
            return CreatedAtAction(nameof(Get),new { id = newHotel.Id},newHotel);
        }

        // PUT api/<HotelsController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Hotel updatedHotel)
        {
            Hotel oldHotel = hotels.FirstOrDefault(h => h.Id == id);
            if (oldHotel == null)
            {
                return NotFound(new { message = "Hotel Not Found"});
            }

            oldHotel.Name = updatedHotel.Name;
            oldHotel.Address = updatedHotel.Address;
            oldHotel.Rating = updatedHotel.Rating;

            return NoContent();
        }

        // DELETE api/<HotelsController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            Hotel existingHotel = hotels.FirstOrDefault(h=> h.Id == id);

            if (existingHotel == null)
            {
                return NotFound(new {message = "Hotel Not found"});
            }

            hotels.Remove(existingHotel);
            return NoContent();
        }
    }
}
