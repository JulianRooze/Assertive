using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Assertive.Patterns
{
  class SimpleEqualsPattern : IFriendlyMessagePattern
  {
    public bool IsMatch(Expression expression)
    {
      return expression.NodeType == ExpressionType.Equal;
    }

    public string TryGetFriendlyMessage(Expression expression, Stack<object> values)
    {
      var binaryExpression = (BinaryExpression)expression;

      var left = binaryExpression.Left;
      var right = binaryExpression.Right;

      var leftVal = values.Pop();
      var rightVal = values.Pop();

      if (binaryExpression.Right is ConstantExpression)
      {
        return $"Expected {left} to equal {right} but it was {rightVal} instead.";
      }
      else
      {
        return $"Expected {left} to equal {right} but left was {rightVal} while right was {leftVal}.";
      }
    }
  }
}
