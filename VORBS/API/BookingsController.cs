using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VORBS.Models;

namespace VORBS.API
{
    [RoutePrefix("api/bookings")]
    public class BookingsController : ApiController
    {
        [Route("")]
        [HttpPost]
        public List<Booking> GetRoomBookings(Location location, DateTime start, DateTime end, string room)
        {
            DateTime date = new DateTime();
            if (location.Name == "CEO")
            {
                date = DateTime.Now;
            }
            else if (location.Name == "Worthing")
            {
                date = DateTime.Now.AddMonths(-1);
            }
            else
            {
                date = DateTime.Now.AddMonths(1);
            }

            return new List<Booking>() { new Booking(){ Location = location, EndDate = date.AddDays(2), Owner = "Reece Bedding", StartDate = date} };
        }
    }
}
