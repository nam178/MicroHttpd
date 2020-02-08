using System;
using System.Collections.Generic;
using System.Text;

namespace MicroHttpd.Core
{
    static class UnixTimestamp
    {
		public static double FromDateTime(DateTime dateTime)
		{
			return Math.Floor(
				dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
				);
		}
    }
}
