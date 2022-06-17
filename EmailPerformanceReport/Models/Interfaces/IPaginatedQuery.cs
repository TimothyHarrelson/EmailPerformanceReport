using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailPerformanceReport.Models
{
    /// <summary>
    /// Used for queries to the NGP 7 API that return a list of records.
    /// </summary>
    /// <typeparam name="T">The model for the record there will be a list of</typeparam>
    internal interface IPaginatedQuery<T>
    {
        public IEnumerable<T> items { get; set; }

        public int count { get; set; }
        public string? nextPageLink { get; set; }
    }
}
