using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customers
{
    public partial class CustomerService : ICustomerService
    {
        /// <summary>
        /// Get customer by phone
        /// </summary>
        /// <param name="phone">Phone</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var query = from c in _customerRepository.Table
                        join a in _gaRepository.Table
                        on new { Id = c.Id, KeyGroup = "Customer", Key = "Phone", Value = phone }
                        equals new { Id = a.EntityId, KeyGroup = a.KeyGroup, Key = a.Key, Value = a.Value } into c_a
                        where c_a.Any()
                        orderby c.Id
                        select c;
            var customer = query.FirstOrDefault();

            return customer;
        }

        /// <summary>
        /// Get customer by username or email or phone
        /// </summary>
        /// <param name="val">Username/Email/Phone</param>
        /// <returns>Customer</returns>
        public virtual Customer GetCustomerByUsernameOrEmailOrPhone(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return null;

            var customer = GetCustomerByUsername(val);
            if (null == customer)
                customer = GetCustomerByEmail(val);
            if (null == customer)
                customer = GetCustomerByPhone(val);

            return customer;
        }
    }
}
