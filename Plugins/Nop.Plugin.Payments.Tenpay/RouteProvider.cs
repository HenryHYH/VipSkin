using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Tenpay
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //Notify
            routes.MapRoute("Plugin.Payments.Tenpay.Notify",
                 "Plugins/PaymentTenpay/Notify",
                 new { controller = "PaymentTenpay", action = "Notify" },
                 new[] { "Nop.Plugin.Payments.Tenpay.Controllers" }
            );

            //Return
            routes.MapRoute("Plugin.Payments.Tenpay.Return",
                 "Plugins/PaymentTenpay/Return",
                 new { controller = "PaymentTenpay", action = "Return" },
                 new[] { "Nop.Plugin.Payments.Tenpay.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
