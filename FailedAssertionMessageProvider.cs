using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assertive
{
  internal class FailedAssertionMessageProvider
  {
    private readonly Expression<Func<bool>> _assertion;

    public FailedAssertionMessageProvider(Expression<Func<bool>> assertion)
    {
      this._assertion = assertion;
    }

    private static readonly Regex _closureCleanup = new Regex(@"(value\(.+<>.+\)\.)", RegexOptions.Compiled);
    private static readonly Regex _indexPropertyRewrite = new Regex(@".get_Item\((\d)\)", RegexOptions.Compiled);

    private string SanitizeMessage(string str)
    {
      var result = _closureCleanup.Replace(str, "");

      result = _indexPropertyRewrite.Replace(result, "[$1]");

      return result;
    }

    internal string GetExceptionMessage()
    {
      var partsProvider = new AssertionPartProvider(_assertion);

      var failedParts = partsProvider.GetFailedAssertionParts();

      var messages = new List<string>(failedParts.Count());

      foreach (var part in failedParts)
      {
        var friendlyMessageProvider = new FriendlyMessageProvider(part);

        var friendlyMessage = friendlyMessageProvider.TryGetFriendlyMessage();

        friendlyMessage = SanitizeMessage(friendlyMessage ?? part.Assertion.ToString());

        messages.Add(friendlyMessage);
      }

      return
$@"Assertion failed:

{ string.Join(Environment.NewLine, messages) }";
    }
  }
}
