using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Assertive
{
  public static class Assert
  {
    public static void That(Expression<Func<bool>> assertion)
    {
      var assertionResult = ExecuteAssertion(assertion);

      if (!assertionResult)
      {
        throw GetFailedAssertionException(assertion);
      }
    }

    private static bool ExecuteAssertion(Expression<Func<bool>> assertion)
    {
      return assertion.Compile()();
    }

    private static Exception GetFailedAssertionException(Expression<Func<bool>> assertion)
    {
      var message = GetFailedAssertionExceptionMessage(assertion);

      return new AssertionFailedException(message);
    }

    private static string GetFailedAssertionExceptionMessage(Expression<Func<bool>> assertion)
    {
      var messageProvider = new FailedAssertionMessageProvider(assertion);

      return messageProvider.GetExceptionMessage();
    }
  }
}
