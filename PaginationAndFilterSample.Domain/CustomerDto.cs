using System;

namespace PaginationAndFilterSample.Domain
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }

        public DateTime LastUpdated{ get; set; }
    }
}