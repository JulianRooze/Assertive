using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Assertive
{
  public class AssertionPart
  {
    public AssertionPart(Expression assertion)
    {
      Assertion = assertion;
    }

    public Expression Assertion { get; }
  }
}
