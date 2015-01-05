using Nop.Core.Domain.Customers;

namespace Nop.Services.Customers
{
    public partial interface ICustomerService
    {
        #region Customer

        /// <summary>
        /// Get customer by phone
        /// </summary>
        /// <param name="phone">Phone</param>
        /// <returns>Customer</returns>
        Customer GetCustomerByPhone(string phone);

        /// <summary>
        /// Get customer by username or email or phone
        /// </summary>
        /// <param name="val">Username/Email/Phone</param>
        /// <returns>Customer</returns>
        Customer GetCustomerByUsernameOrEmailOrPhone(string val);

        #endregion
    }
}
