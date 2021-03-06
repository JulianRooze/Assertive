﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Assertive
{
  internal interface IFriendlyMessagePattern
  {
    bool IsMatch(Expression expression);
    string TryGetFriendlyMessage(Expression expression, Stack<object> values);
  }
}
