using Assertive.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assertive
{
  public class FriendlyMessageProvider
  {
    private readonly AssertionPart _assertionPart;

    private IFriendlyMessagePattern[] _patterns = new IFriendlyMessagePattern[]
    {
      new SimpleEqualsPattern(),
      new ContainsPattern()
    };

    public FriendlyMessageProvider(AssertionPart assertionPart)
    {
      _assertionPart = assertionPart;
    }

    public string TryGetFriendlyMessage()
    {
      foreach (var pattern in _patterns)
      {
        if (pattern.IsMatch(_assertionPart.Assertion))
        {
          try
          {
            var visitor = new ExpressionInterpreter(_assertionPart.Assertion, pattern);

            visitor.Visit(_assertionPart.Assertion);

            if (visitor.FriendlyMessage != null)
            {
              return visitor.FriendlyMessage;
            }
          }
          catch { }
        }
      }

      return null;
    }

    private class ExpressionInterpreter : ExpressionVisitor
    {
      public ExpressionInterpreter(Expression expression, IFriendlyMessagePattern pattern)
      {
        _expression = expression;
        _pattern = pattern;
      }

      private Stack<object> _stack = new Stack<object>();
      private readonly Expression _expression;
      private readonly IFriendlyMessagePattern _pattern;
      public string FriendlyMessage { get; private set; }

      private object GetValueFromMemberInfo(MemberInfo member, object instance)
      {
        if (member is PropertyInfo)
        {
          return ((PropertyInfo)member).GetValue(instance);
        }
        else if (member is FieldInfo)
        {
          return ((FieldInfo)member).GetValue(instance);
        }
        else
        {
          return null;
        }
      }

      protected override Expression VisitMethodCall(MethodCallExpression node)
      {
        if (object.ReferenceEquals(node, _expression))
        {
          FriendlyMessage = _pattern.TryGetFriendlyMessage(_expression, _stack);
          return node;
        }

        if (node.Object != null)
        {
          this.Visit(node.Object);
        }

        object instance = null;

        if (_stack.Count > 0)
        {
          instance = _stack.Pop();
        }

        var args = new List<object>();

        foreach (var arg in node.Arguments)
        {
          this.Visit(arg);

          args.Add(_stack.Pop());
        }

        var returnValue = node.Method.Invoke(instance, args.Count > 0 ? args.ToArray() : null);

        _stack.Push(returnValue);

        return node;
      }

      protected override Expression VisitMember(MemberExpression ex)
      {
        this.Visit(ex.Expression);

        var expVal = _stack.Pop();

        var accessMember = GetValueFromMemberInfo(ex.Member, expVal);

        _stack.Push(accessMember);

        return ex;
      }

      protected override Expression VisitConstant(ConstantExpression ex)
      {
        _stack.Push(ex.Value);

        return base.VisitConstant(ex);
      }

      protected override Expression VisitBinary(BinaryExpression ex)
      {
        var left = Visit(ex.Left);

        var right = Visit(ex.Right);

        if (object.ReferenceEquals(ex, _expression))
        {
          FriendlyMessage = _pattern.TryGetFriendlyMessage(_expression, _stack);
          return ex;
        }

        var leftVal = _stack.Pop();

        var rightVal = _stack.Pop();

        if (ex.NodeType == ExpressionType.Add)
        {
          _stack.Push((int)leftVal + (int)rightVal);
        }
        else if (ex.NodeType == ExpressionType.Equal)
        {
          var equals = leftVal.Equals(rightVal);

          _stack.Push(equals);
        }
        return ex;
      }
    }

    private Stack<object> _stack = new Stack<object>();
  }
}
