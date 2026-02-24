using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Model
{
    public static class StatusValues
    {
        public static IList<StatusTrans> All { get; } = Enum.GetValues<StatusTrans>();
    }
}
