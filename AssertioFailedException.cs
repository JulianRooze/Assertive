using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assertive
{
  internal class AssertionFailedException : Exception
  {
    public AssertionFailedException(string message) : base(message) { }
  }
}
