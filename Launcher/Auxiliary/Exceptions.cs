/* 
 *  Exceptions.cs
 *  Author: RealNickk
*/

using System;

namespace Starlight
{
    public partial class BootstrapError : Exception
    {
        public BootstrapError() : base() { }
        public BootstrapError(string x) : base(x) { }
    }
}
