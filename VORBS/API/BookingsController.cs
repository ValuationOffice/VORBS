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
        [Route("{start:DateTime}/{end:DateTime}/")]
        [HttpPost]
        public List<Booking> GetRoomBookingsForLocation(Location location, DateTime start, DateTime end)
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

            return new List<Booking>() { new Booking() { Location = location, EndDate = date.AddDays(2), Owner = "Reece Bedding", StartDate = date, Room = "MR2" } };
        }

        [Route("{start:DateTime}/{end:DateTime}/{room}")]
        [HttpPost]
        public List<Booking> GetRoomBookingsForRoom(Location location, DateTime start, DateTime end, string room)
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

            return new List<Booking>() { new Booking(){ Location = location, EndDate = date.AddDays(2), Owner = "Reece Bedding", StartDate = date, Room = room} };
        }

        [Route("{start:DateTime}/{end:DateTime}/{room}/{person}")]
        [HttpPost]
        public List<Booking> GetRoomBookingsForRoomAndPerson(Location location, DateTime start, DateTime end, string room, string person)
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

            return new List<Booking>() { new Booking() { Location = location, EndDate = date.AddDays(2), Owner = "Reece Bedding", StartDate = date, Room = room } };
        }

        [Route("{start:DateTime}/{end:DateTime}/{person}")]
        [HttpPost]
        public List<Booking> GetRoomBookingsForPerson(DateTime start, DateTime end, string room, string person)
        {
            List<Booking> bookings = new List<Booking>()
            {
                new Booking(){ Location = new Location(){ Name = "CEO", Rooms = new string[0]}, StartDate = DateTime.Now.AddDays(0), EndDate = DateTime.Now.AddDays(0), Owner = "Reece Bedding", Room = "MR1"},
                new Booking(){ Location = new Location(){ Name = "Worthing", Rooms = new string[0]}, StartDate = DateTime.Now.AddDays(-1), EndDate = DateTime.Now.AddDays(-1), Owner = "Reece Bedding", Room = "MR5"},
                new Booking(){ Location = new Location(){ Name = "CEO", Rooms = new string[0]}, StartDate = DateTime.Now.AddDays(-2), EndDate = DateTime.Now.AddDays(-2), Owner = "Reece Bedding", Room = "MR1"},
                new Booking(){ Location = new Location(){ Name = "CEO", Rooms = new string[0]}, StartDate = DateTime.Now.AddDays(-3), EndDate = DateTime.Now.AddDays(-3), Owner = "Reece Bedding", Room = "MR2"},
                new Booking(){ Location = new Location(){ Name = "Thurrock", Rooms = new string[0]}, StartDate = DateTime.Now.AddDays(-4), EndDate = DateTime.Now.AddDays(-4), Owner = "Reece Bedding", Room = "MR14"},
                new Booking(){ Location = new Location(){ Name = "CEO", Rooms = new string[0]}, StartDate = DateTime.Now.AddDays(-5), EndDate = DateTime.Now.AddDays(-5), Owner = "Reece Bedding", Room = "MR3"},
            };
            return bookings;
        }
    }
}
