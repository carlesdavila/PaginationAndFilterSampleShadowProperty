using System;
using System.Linq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaginationAndFilterSample.Domain;
using PaginationAndFilterSample.Persistence;

namespace PaginationAndFilterSample.Client
{
    class Program
    {
        private static CustomerDbContext _context = new CustomerDbContext();
        private static Fixture fixture = new Fixture();
        static void Main(string[] args)
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            CreateAndEditCustomer();

            PaginateCustomers("expression");
        }

        private static void PaginateCustomers(string expression)
        {
            var result = _context.Customers
                .OrderBy(c => EF.Property<DateTime>(c, "LastModified"))
                .Select(x => new CustomerDto()
                {
                    Id = x.Id,
                    Organization = x.Organization,
                    Name = x.Name,
                    LastUpdated = EF.Property<DateTime>(x, "LastModified")
                })
                .Skip(0)
                .Take(10);

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));

        }

        private static void CreateAndEditCustomer()
        {
            fixture.RepeatCount = 100;
            fixture.Customize<Customer>(o => o
                    .Without(c => c.Id));

            var customersToCreate = fixture.CreateMany<Customer>();

           _context.Customers.AddRange(customersToCreate);
           _context.SaveChanges();

        }
    }
}
