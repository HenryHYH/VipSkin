using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Tests;
using NUnit.Framework;
using Rhino.Mocks;

namespace Nop.Services.Tests.Customers
{
    [TestFixture]
    public class CustomerLoginServiceTests : ServiceTest
    {
        private IRepository<Customer> _customerRepo;
        private IRepository<GenericAttribute> _genericAttributeRepo;
        private ICustomerService _customerService;

        [SetUp]
        public new void SetUp()
        {
            _customerRepo = MockRepository.GenerateMock<IRepository<Customer>>();
            _customerRepo.Expect(x => x.Table).Return(CreateCustomers().AsQueryable());

            _genericAttributeRepo = MockRepository.GenerateMock<IRepository<GenericAttribute>>();
            _genericAttributeRepo.Expect(x => x.Table).Return(CreateGenericAttributes().AsQueryable());

            _customerService = new CustomerService(new NopNullCache(), _customerRepo, null,
                _genericAttributeRepo, null, null,
                null, null, null, null, null, null,
                null, null, null, null, null, null);
        }

        private IList<Customer> CreateCustomers()
        {
            var customers = new List<Customer>();

            customers.Add(new Customer
            {
                Id = 1,
                Username = "a",
                Email = "a@a.com"
            });
            customers.Add(new Customer
            {
                Id = 2,
                Username = "b",
                Email = "b@b.com"
            });

            return customers;
        }
        private IList<GenericAttribute> CreateGenericAttributes()
        {
            var attributes = new List<GenericAttribute>();

            attributes.Add(new GenericAttribute
            {
                Id = 1,
                EntityId = 1,
                KeyGroup = "Customer",
                Key = "Phone",
                Value = "13800138000",
                StoreId = 0
            });
            attributes.Add(new GenericAttribute
            {
                Id = 2,
                EntityId = 2,
                KeyGroup = "Customer",
                Key = "Phone",
                Value = "123456",
                StoreId = 0
            });

            return attributes;
        }

        [Test]
        [TestCase("13800138000", "a@a.com", "a")]
        [TestCase("123456", "b@b.com", "b")]
        public void Can_get_customer_by_phone(string phone, string expectedEmail, string expectedUsername)
        {
            Customer customer = _customerService.GetCustomerByPhone(phone);
            customer.ShouldNotBeNull();
            customer.Email.ShouldEqual(expectedEmail);
            customer.Username.ShouldEqual(expectedUsername);
        }

        [Test]
        [TestCase("1")]
        public void Can_get_null_customer_by_phone(string phone)
        {
            Customer customer = _customerService.GetCustomerByPhone(phone);
            customer.ShouldBeNull();
        }

        [Test]
        [TestCase("a", "a", "a@a.com")]
        [TestCase("a@a.com", "a", "a@a.com")]
        [TestCase("13800138000", "a", "a@a.com")]
        public void Can_get_customer_by_username_email_phone(string val, string expectedUsername, string expectedEmail)
        {
            Customer customer = _customerService.GetCustomerByUsernameOrEmailOrPhone(val);
            customer.ShouldNotBeNull();
            customer.Username.ShouldEqual(expectedUsername);
            customer.Email.ShouldEqual(expectedEmail);
        }

        [Test]
        [TestCase("1")]
        public void Can_get_null_customer_by_username_email_phone(string val)
        {
            Customer customer = _customerService.GetCustomerByUsernameOrEmailOrPhone(val);
            customer.ShouldBeNull();
        }
    }
}
