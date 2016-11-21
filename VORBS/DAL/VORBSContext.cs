using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;
using VORBS.API;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Transactions;
using VORBS.Services;

namespace VORBS.DAL
{
    public class VORBSContext : DbContext
    {
        private BookingService _bookingService;

        public VORBSContext() : base("VORBSContext")
        {
            _bookingService = new BookingService(this);
        }

        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<LocationCredentials> LocationCredentials { get; set; }
        public virtual DbSet<ExternalAttendees> ExternalAttendees { get; set; }

        public virtual int SaveChanges(Booking booking, bool dontCheckClash)
        {
            List<Booking> clashedBookings;
            int objectsWritten = 0;
            if (!dontCheckClash)
            {
                using (var scope = TransactionUtils.CreateTransactionScope())
                {
                    AvailabilityController aC = new AvailabilityController();

                    bool invalid = aC.DoesMeetingClash(booking, out clashedBookings);
                    //Checks if the booking that clashed is the current booking being saved, this allows us to edit bookings.
                    if (clashedBookings.Count == 1)
                    {
                        if (invalid && booking.ID != 0)
                            invalid = !(booking.ID == clashedBookings[0].ID);
                    }

                    if (invalid)
                        throw new BookingConflictException("Simultaneous booking conflict, please try again.");

                    objectsWritten = base.SaveChanges();
                    scope.Complete();
                }
            }
            else
            {
                objectsWritten = base.SaveChanges();
            }
            return objectsWritten;
        }

        public virtual int SaveChanges(Booking booking)
        {
            return SaveChanges(booking, false);
        }

        public virtual int SaveChanges(List<Booking> bookings)
        {
            return SaveChanges(bookings, false);
        }

        public virtual int SaveChanges(List<Booking> bookings, bool dontCheckClash)
        {
            int objectsWritten = 0;

            if (!dontCheckClash)
            {
                AvailabilityController aC = new AvailabilityController();
                List<Booking> clashedBookings;

                using (var scope = TransactionUtils.CreateTransactionScope())
                {
                    foreach (var b in bookings)
                    {

                        bool invalid = aC.DoesMeetingClash(b, out clashedBookings);
                        //Checks if the booking that clashed is the current booking being saved, this allows us to edit bookings.
                        if (clashedBookings.Count == 1)
                        {
                            if (invalid && b.ID != 0)
                                invalid = !(b.ID == clashedBookings[0].ID);
                        }

                        if (invalid)
                            throw new BookingConflictException("Simultaneous booking conflict, please try again.");
                    }

                    //We dont mess around with the recurrence ID's after they are first created. So if the first booking in the batch has an ID, it exists in the DB. So dont go on further.
                    if (bookings.First().ID == 0)
                    {
                        int? recurrenceId = bookings.Select(x => x.RecurrenceId).FirstOrDefault();
                        //Check if the recurrence id is infact the next id in the DB (could potentially not be if a recurrence was made during execution of this request)
                        int nextRecurrenceId = _bookingService.GetNextRecurrenceId();
                        if (recurrenceId != null && recurrenceId != nextRecurrenceId)
                            //if the bookings recurrenceid is not the next, then set it.
                            bookings.ForEach(x => x.RecurrenceId = nextRecurrenceId);
                    }
                    objectsWritten = base.SaveChanges();
                    scope.Complete();
                }
            }
            else
            {
                objectsWritten = base.SaveChanges();
            }
            return objectsWritten;
        }
    }

    public class TransactionUtils
    {
        public static TransactionScope CreateTransactionScope()
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };

            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }

    public class BookingConflictException : Exception
    {
        public BookingConflictException(string message)
            : base(message)
        {

        }
    }
}