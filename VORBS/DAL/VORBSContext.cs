using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;
using VORBS.API;

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Transactions;

namespace VORBS.DAL
{
    public class VORBSContext : DbContext
    {
        public VORBSContext() : base("VORBSContext") { }

        public DbSet<Location> Locations { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<LocationCredentials> LocationCredentials { get; set; }

        public virtual int SaveChanges(Booking booking)
        {
            Booking clashedBooking;
            int objectsWritten;

            using (var scope = TransactionUtils.CreateTransactionScope())
            {
                if (new AvailabilityController().DoesMeetingClash(booking, out clashedBooking))
                    throw new BookingConflictException("Simultaneous booking conflict, please try again.");

                objectsWritten = base.SaveChanges();
                scope.Complete();
            }

            return objectsWritten;
        }

        public virtual int SaveChanges(List<Booking> bookings)
        {
            AvailabilityController aC = new AvailabilityController();
            Booking clashedBooking;

            int objectsWritten;

            using (var scope = TransactionUtils.CreateTransactionScope())
            {
                foreach (var b in bookings)
                {
                    if (aC.DoesMeetingClash(b, out clashedBooking))
                        throw new BookingConflictException("Simultaneous booking conflict, please try again.");
                }

                objectsWritten = base.SaveChanges();
                scope.Complete();
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