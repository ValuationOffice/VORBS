using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VORBS.Models;

namespace VORBS.Services
{
    public class StubbedDirectoryService : IDirectoryService
    {

        private List<User> stubbedUsers
        {
            get
            {
                List<User> users = new List<User>()
                {
                    new User(){ FirstName = "User1", LastName = "Lastname", EmailAddress = "user1@email.com", PayId = new User.Pid("0000001") },
                    new User(){ FirstName = "User2", LastName = "Lastname", EmailAddress = "user2@email.com", PayId = new User.Pid("0000002") },
                    new User(){ FirstName = "User3", LastName = "Lastname", EmailAddress = "user3@email.com", PayId = new User.Pid("0000003") },
                    new User(){ FirstName = "User4", LastName = "Lastname", EmailAddress = "user4@email.com", PayId = new User.Pid("0000004") },
                    new User(){ FirstName = "User5", LastName = "Lastname", EmailAddress = "user5@email.com", PayId = new User.Pid("0000005") },
                    new User(){ FirstName = "User6", LastName = "Lastname", EmailAddress = "user6@email.com", PayId = new User.Pid("0000006") },
                    new User(){ FirstName = "User7", LastName = "Lastname", EmailAddress = "user7@email.com", PayId = new User.Pid("0000007") },
                    new User(){ FirstName = "User8", LastName = "Lastname", EmailAddress = "user8@email.com", PayId = new User.Pid("0000008") }
                };
                string userIdentity = Environment.UserName.Substring(Environment.UserName.IndexOf("\\") + 1);
                User currentUser = new User()
                {
                    FirstName = "Current",
                    LastName = "User",
                    EmailAddress = $"{userIdentity}@email.com",
                    PayId = new User.Pid(userIdentity)
                };

                users.Add(currentUser);

                return users;
            }
        }


        public List<User> GetAllUsers()
        {
            return stubbedUsers;
        }

        public List<User> GetBySurname(string surname)
        {
            return stubbedUsers.Where(x => x.LastName.StartsWith(surname)).ToList();
        }

        public User GetCurrentUser(string identity)
        {
            string pid = identity.Substring(identity.IndexOf("\\") + 1);
            return stubbedUsers.Where(x => x.PayId.Identity == pid).First();
        }

        public User GetUser(User.Pid pid)
        {
            return stubbedUsers.Where(x => x.PayId.Identity == pid.Identity).First();
        }
    }
}