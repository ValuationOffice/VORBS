using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.DAL.Repositories;
using VORBS.Models;

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
        }

        public void AddNewAdmin(Admin newAdmin)
        {
            if (_adminRepository.GetAdminByPid(newAdmin.PID) != null)
                throw new AdminExistsException();

            _adminRepository.SaveNewAdmin(newAdmin);
            _logger.Info("Admin successfully added: " + newAdmin.PID);
        }

        public void EditExistingAdmin(Admin existingAdmin, Admin newAdmin)
        {
            existingAdmin.PermissionLevel = newAdmin.PermissionLevel;
            existingAdmin.LocationID = newAdmin.LocationID;
            _adminRepository.UpdateAdmin(existingAdmin);

            _logger.Info("Admin successfully Edited: " + newAdmin.PID);
        }

        public void DeleteExistingAdmin(Admin admin)
        {
            _adminRepository.DeleteAdmin(admin);

            _logger.Info("Admin successfully deleted: " + admin.ID);
        }

        public class AdminExistsException : Exception { }
    }
}