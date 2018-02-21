using System;
using System.Collections.Generic;
using System.Linq;
using VORBS.Models;
using VORBS.Utils;

namespace VORBS.DAL.Repositories
{
    public class EFAdminRepository :  IAdminRepository
    {
        private VORBSContext db;
        private NLog.Logger _logger;

        public EFAdminRepository(VORBSContext context)
        {
            db = context;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        public List<Admin> GetAll()
        {
            List<Admin> admins = db.Admins.ToList();
            
            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(admins));

            return admins;
        }

        public Admin GetAdminById(int ID)
        {
            Admin admin = db.Admins.Where(a => a.ID == ID).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(admin, ID));
            
            return admin;
        }

        public Admin GetAdminByPid(string PID)
        {
            Admin result = db.Admins.Where(x => x.PID == PID).FirstOrDefault();

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(result, PID));

            return result;
        }

        public void UpdateAdmin(Admin admin)
        {
            try
            {
                Admin existingAdmin = GetAdminById(admin.ID);
                db.Entry(existingAdmin).CurrentValues.SetValues(admin);

                db.SaveChanges();
            }
            catch (Exception exn)
            {
                _logger.ErrorException("Unable to update admin: " + admin.PID, exn);
                throw exn;
            }
            finally
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, admin));
            }
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
            finally
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, admin));
            }
        }

        public void DeleteAdmin(Admin admin)
        {
            try
            {
                db.Admins.Remove(admin);
                db.SaveChanges();
            }
            catch (Exception exn)
            {
                _logger.ErrorException("Unable to delete admin: " + admin.PID, exn);
                throw exn;
            }
            finally
            {
                _logger.Trace(LoggerHelper.ExecutedFunctionMessage(null, admin));
            }
        }
    }
}