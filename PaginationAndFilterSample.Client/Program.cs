using System;
using System.Linq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaginationAndFilterSample.Domain;
using PaginationAndFilterSample.Persistence;
using PaginationAndFilterSample.Persistence.QueryObjects;

namespace PaginationAndFilterSample.Client
{
    static class Program
    {
        private static readonly CustomerDbContext Context = new CustomerDbContext();
        private static readonly Fixture Fixture = new Fixture();
        static void Main(string[] args)
        {
            Context.Database.EnsureDeleted();
            Context.Database.EnsureCreated();

            CreateAndEditCustomer();

            PaginateCustomers("name,lastModified");
        }

        private static void PaginateCustomers(string expression)
        {
            var result = Context.Customers
                .MultiColumnOrderBy(expression)
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
            Fixture.RepeatCount = 100;
            Fixture.Customize<Customer>(o => o
                    .Without(c => c.Id));

            var customersToCreate = Fixture.CreateMany<Customer>();

           Context.Customers.AddRange(customersToCreate);
           Context.SaveChanges();

        }
    }
}
