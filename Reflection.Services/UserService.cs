using System;
using System.Collections.Generic;
using System.Text;

namespace Reflection.Services
{
    public class UserService
    {
        public int PrintUser(UserModel userModel, int index)
        {
            Console.WriteLine(string.Format("User Model {2}:{0} {1}", userModel.UserId, userModel.UserName, index));

            return index;
        }
    }

    public class UserModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; }
    }
}
