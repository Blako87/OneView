using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneView.Models
{
    public class User
    {
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _nickName = string.Empty;

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }  // Changed to public setter for JSON serialization
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }  // Changed to public setter for JSON serialization
        }

        public string NickName
        {
            get { return _nickName; }
            set { _nickName = value; }  // Changed to public setter for JSON serialization
        }
    }
}
