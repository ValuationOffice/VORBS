using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Models;
using VORBS.Utils;

namespace VORBS.Services
{
    public class AdminService
    {
        IAdminRepository _adminRepository;

        private NLog.Logger _logger;

        public AdminService(NLog.Logger logger, IAdminRepository adminRepository)
        {
            _logger = logger;
            _adminRepository = adminRepository;

            _logger.Trace(LoggerHelper.InitializeClassMessage());
        }

        public void AddNewAdmin(Admin newAdmin)
        {
            if (_adminRepository.GetAdminByPid(newAdmin.PID) != null)
                throw new AdminExistsException();

            _adminRepository.SaveNewAdmin(newAdmin);
            _logger.Info("Admin successfully added: " + newAdmin.PID);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, newAdmin));
        }

        public void EditExistingAdmin(Admin existingAdmin, Admin newAdmin)
        {
            existingAdmin.PermissionLevel = newAdmin.PermissionLevel;
            existingAdmin.LocationID = newAdmin.LocationID;
            _adminRepository.UpdateAdmin(existingAdmin);

            _logger.Info("Admin successfully Edited: " + newAdmin.PID);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, existingAdmin, newAdmin));
        }

        public void DeleteExistingAdmin(Admin admin)
        {
            _adminRepository.DeleteAdmin(admin);

            _logger.Info("Admin successfully deleted: " + admin.ID);

            _logger.Trace(LoggerHelper.ExecutedFunctionMessage(LoggerHelper.VOID_TYPE, admin));
        }

        public class AdminExistsException : Exception { }
    }
}