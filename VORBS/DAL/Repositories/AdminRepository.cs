using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public class AdminRepository
    {
        private VORBSContext db;
        private NLog.Logger _logger;

        public AdminRepository(VORBSContext context)
        {
            db = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public List<Admin> GetAll()
        {
            return db.Admins.ToList();
        }

        public Admin GetAdminById(int ID)
        {
            return db.Admins.Single(a => a.ID == ID);
        }

        public Admin GetAdminByPid(string PID)
        {
            return db.Admins.Single(x => x.PID == PID);
        }

        public void UpdateAdmin(Admin admin)
        {
            Admin existingAdmin = GetAdminById(admin.ID);
            db.Entry(existingAdmin).CurrentValues.SetValues(admin);

            db.SaveChanges();
        }

        public void SaveNewAdmin(Admin admin)
        {
            try
            {
                db.Admins.Add(admin);
                db.SaveChanges();
            }
            catch (Exception exn)
            {
                _logger.ErrorException("Unable to save new admin: " + admin.PID, exn);
                throw exn;
            }
        }

        public void DeleteAdmin(Admin admin)
        {
            db.Admins.Remove(admin);
            db.SaveChanges();
        }
    }
}