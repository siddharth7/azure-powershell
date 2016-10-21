using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Commands.RecoveryServices.Backup.Cmdlets.ServiceClientAdapterNS
{
    public class QueryBuilder
    {
        public static QueryBuilder Instance
        {
            get
            {
                return new QueryBuilder();
            }
        }

        public string GetQueryString(object queryObject)
        {
            var props = queryObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string queryString = string.Empty;

            foreach (var property in props)
            {
                var lhs = property.Name;

                var rhs = string.Empty;

                if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var genarg = property.PropertyType.GetGenericArguments();
                    if (genarg.Any(type => type == typeof(DateTime)))
                    {
                        var dt = (DateTime)property.GetValue(queryObject);
                        DateTimeFormatInfo dateFormat = new CultureInfo("en-US").DateTimeFormat;
                        rhs = string.Format("'{0}'", dt.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss tt", dateFormat));
                    }
                    else if (genarg.Any(type => type.IsEnum))
                    {
                        rhs = property.GetValue(queryObject).ToString();
                    }
                }
                else
                {
                    rhs = property.GetValue(queryObject).ToString();
                }

                if (!string.IsNullOrEmpty(queryString))
                {
                    queryString += " and ";
                }

                queryString += lhs + " eq " + rhs;
            }

            return queryString;
        }
    }
}
