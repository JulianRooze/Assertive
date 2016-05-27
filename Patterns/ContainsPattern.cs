using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Assertive.Patterns
{
  public class ContainsPattern : IFriendlyMessagePattern
  {
    public bool IsMatch(Expression expression)
    {
      var callExpression = expression as MethodCallExpression;

      if (callExpression == null) return false;

      return callExpression.Method.Name == "Contains";
    }

    public string TryGetFriendlyMessage(Expression expression, Stack<object> values)
    {
      var callExpression = (MethodCallExpression)expression;

      var instance = callExpression.Object;

      var isExtensionMethod = callExpression.Method.IsDefined(typeof(ExtensionAttribute), false);

      if (isExtensionMethod)
      {
        instance = callExpression.Arguments.First();
      }

      return $"Expected {instance} to contain {string.Join(", ", callExpression.Arguments.Skip(isExtensionMethod ? 1 : 0).Select(a => a.ToString())) }, but it did not.";
    }
  }
}
