using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VORBS.Models;

namespace VORBS.Services
{
    interface IDirectoryService
    {
        User GetCurrentUser(string identity);
        User GetUser(User.Pid pid);
        List<User> GetBySurname(string surname);
        List<User> GetAllUsers();
    }
}
