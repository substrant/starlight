using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starlight.Apis
{
    public abstract class RbxUser
    {
        public abstract string UserId { get; protected set; }

        public abstract string Username { get; protected set; }
    }
}
