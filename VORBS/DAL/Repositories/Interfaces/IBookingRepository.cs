using System;
using System.Collections.Generic;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public interface IBookingRepository
    {
        bool Delete(List<Booking> bookings);
        bool DeleteById(int Id);
        List<Booking> GetAvailableSmartRoomBookings(Booking newBooking, out List<Booking> clashedBookings);
        List<Booking> GetBookingsInRecurrence(int recurrenceId, bool noTracking = false);
        List<Booking> GetByDateAndLocation(DateTime startDate, Location location);
        List<Booking> GetByDateAndLocation(DateTime startDate, DateTime endDate, Location location);
        List<Booking> GetByDateAndOwner(DateTime startDate, string owner);
        List<Booking> GetByDateAndOwnerForPeriod(int period, string owner, DateTime startDate);
        List<Booking> GetByDateAndPid(DateTime startDate, string pid);
        List<Booking> GetByDateAndPidForPeriod(int period, string pid, DateTime startDate);
        List<Booking> GetByDateAndRoom(DateTime startDate, Room room);
        List<Booking> GetByDateAndRoom(DateTime startDate, DateTime endDate, Room room);
        List<Booking> GetByDateRoomAndOwner(DateTime startDate, DateTime endDate, Room room, string owner);
        List<Booking> GetByDateRoomAndPid(DateTime startDate, DateTime endDate, Room room, string pid);
        List<Booking> GetById(List<int> ids);
        Booking GetById(int id);
        List<Booking> GetByOwner(string owner);
        List<Booking> GetByPartialDateRoomLocationSmartAndOwner(DateTime? startDate, DateTime? endDate, string owner, bool? smartRoom, Room room, Location location);
        List<Booking> GetByPartialDateRoomSmartLocationAndPid(DateTime? startDate, DateTime? endDate, string Pid, bool? smartRoom, Room room, Location location);
        List<string> GetDistinctListOfOwners();
        int GetNextRecurrenceId();
        void SaveNewBookings(List<Booking> bookings, bool checkForClash = true);
        Booking UpdateExistingBooking(Booking existingBooking, Booking editBooking);
        List<Booking> UpdateExistingBookings(List<Booking> existingBookings, List<Booking> editBookings);
    }
}