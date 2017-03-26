using System.Collections.Generic;
using VORBS.Models;

namespace VORBS.DAL.Repositories
{
    public interface IAdminRepository
    {
        void DeleteAdmin(Admin admin);
        Admin GetAdminById(int ID);
        Admin GetAdminByPid(string PID);
        List<Admin> GetAll();
        void SaveNewAdmin(Admin admin);
        void UpdateAdmin(Admin admin);
    }
}