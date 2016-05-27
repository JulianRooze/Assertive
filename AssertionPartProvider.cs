using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Assertive
{
  internal class AssertionPartProvider : ExpressionVisitor
  {
    private readonly Expression<Func<bool>> _assertion;
    private readonly List<Expression> _failedAssertions;

    private bool _binaryExpressionsEncountered = false;

    public AssertionPartProvider(Expression<Func<bool>> assertion)
    {
      _assertion = assertion;
      _failedAssertions = new List<Expression>();
    }

    internal IEnumerable<AssertionPart> GetFailedAssertionParts()
    {
      this.Visit(_assertion.Body);

      if (_failedAssertions.Count == 0 && !_binaryExpressionsEncountered)
      {
        _failedAssertions.Add(_assertion.Body);
      }

      var failedAssertionParts = (from f in _failedAssertions
                                  select new AssertionPart(f)).ToList();

      return failedAssertionParts;
    }

    private bool TestAssertion(Expression expression)
    {
      var lambda = Expression.Lambda<Func<bool>>(expression);

      var compiled = lambda.Compile();

      return compiled();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
      if (node.Type == typeof(bool))
      {
        bool b;
        ProcessBinaryExpression(node, out b);
      }

      return node;
    }

    private bool ProcessBinaryExpression(Expression node, out bool addedFailure)
    {
      addedFailure = false;

      var binaryExpression = node as BinaryExpression;

      if (binaryExpression != null)
      {
        switch (binaryExpression.NodeType)
        {
          case ExpressionType.And: return ProcessAnd(out addedFailure, binaryExpression);
          case ExpressionType.AndAlso: return ProcessAndAlso(out addedFailure, binaryExpression);
          case ExpressionType.Or: return ProcessOr(out addedFailure, binaryExpression);
          case ExpressionType.OrElse: return ProcessOrElse(out addedFailure, binaryExpression);
          default:

            var passed = TestAssertion(binaryExpression);

            return passed;
        }
      }
      else
      {
        var passed = TestAssertion(node);

        return passed;
      }
    }

    private bool ProcessOr(out bool addedFailure, BinaryExpression binaryExpression)
    {
      _binaryExpressionsEncountered = true;

      var leftPassed = ProcessBinaryExpression(binaryExpression.Left, out addedFailure);
      var rightPassed = ProcessBinaryExpression(binaryExpression.Right, out addedFailure);

      if (!leftPassed && !rightPassed)
      {
        if (addedFailure)
        {
          _failedAssertions.Add(binaryExpression.Left);
          _failedAssertions.Add(binaryExpression.Right);
          addedFailure = true;
        }
      }

      return true;
    }

    private bool ProcessOrElse(out bool addedFailure, BinaryExpression binaryExpression)
    {
      _binaryExpressionsEncountered = true;

      var leftPassed = ProcessBinaryExpression(binaryExpression.Left, out addedFailure);

      if (!leftPassed)
      {
        var rightPassed = ProcessBinaryExpression(binaryExpression.Right, out addedFailure);

        if (!rightPassed)
        {
          if (!addedFailure)
          {
            _failedAssertions.Add(binaryExpression.Left);
            _failedAssertions.Add(binaryExpression.Right);
            addedFailure = true;
          }

          return false;
        }
      }

      return true;
    }

    private bool ProcessAnd(out bool addedFailure, BinaryExpression binaryExpression)
    {
      _binaryExpressionsEncountered = true;

      var leftPassed = ProcessBinaryExpression(binaryExpression.Left, out addedFailure);

      if (!leftPassed)
      {
        if (!addedFailure)
        {
          _failedAssertions.Add(binaryExpression.Left);
          addedFailure = true;
        }
      }

      var rightPassed = ProcessBinaryExpression(binaryExpression.Right, out addedFailure);

      if (!rightPassed)
      {
        if (!addedFailure)
        {
          _failedAssertions.Add(binaryExpression.Right);
          addedFailure = true;
        }
      }

      if (!leftPassed || !rightPassed)
      {
        return false;
      }

      return true;
    }

    private bool ProcessAndAlso(out bool addedFailure, BinaryExpression binaryExpression)
    {
      _binaryExpressionsEncountered = true;

      var leftPassed = ProcessBinaryExpression(binaryExpression.Left, out addedFailure);

      if (!leftPassed)
      {
        if (!addedFailure)
        {
          _failedAssertions.Add(binaryExpression.Left);
          addedFailure = true;
        }

        return false;
      }

      var rightPassed = ProcessBinaryExpression(binaryExpression.Right, out addedFailure);

      if (!rightPassed)
      {
        if (!addedFailure)
        {
          _failedAssertions.Add(binaryExpression.Right);
          addedFailure = true;
        }

        return false;
      }

      return true;
    }

    public override Expression Visit(Expression node)
    {
      return base.Visit(node);
    }
  }
}
