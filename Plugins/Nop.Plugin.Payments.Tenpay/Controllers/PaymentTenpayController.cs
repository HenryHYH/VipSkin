using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.Tenpay.Models;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.Tenpay.Controllers
{
    public class PaymentTenpayController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly TenpayPaymentSettings _tenpayPaymentSettings;
        private readonly PaymentSettings _paymentSettings;

        public PaymentTenpayController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService, 
            ILogger logger, IWebHelper webHelper,
            TenpayPaymentSettings tenpayPaymentSettings,
            PaymentSettings paymentSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._logger = logger;
            this._webHelper = webHelper;
            this._tenpayPaymentSettings = tenpayPaymentSettings;
            this._paymentSettings = paymentSettings;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.Key = _tenpayPaymentSettings.Key;
            model.Partner = _tenpayPaymentSettings.Partner;
            model.AdditionalFee = _tenpayPaymentSettings.AdditionalFee;

            return View("~/Plugins/Payments.Tenpay/Views/PaymentTenpay/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _tenpayPaymentSettings.Key = model.Key;
            _tenpayPaymentSettings.Partner = model.Partner;
            _tenpayPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_tenpayPaymentSettings);
            
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.Tenpay/Views/PaymentTenpay/PaymentInfo.cshtml", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult Notify(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Tenpay") as TenpayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Tenpay module cannot be loaded");


            string tenpayNotifyUrl = "https://www.tenpay.com/cooperate/gateway.do?service=notify_verify";
            string partner = _tenpayPaymentSettings.Partner;
            if (string.IsNullOrEmpty(partner))
                throw new Exception("Partner is not set");
            string key = _tenpayPaymentSettings.Key;
            if (string.IsNullOrEmpty(key))
                throw new Exception("Partner is not set");
            string _input_charset = "utf-8";

            tenpayNotifyUrl = tenpayNotifyUrl + "&partner=" + partner + "&notify_id=" + Request.Form["notify_id"];
            string responseTxt = processor.Get_Http(tenpayNotifyUrl, 120000);

            int i;
            NameValueCollection coll;
            coll = Request.Form;
            String[] requestarr = coll.AllKeys;
            string[] Sortedstr = processor.BubbleSort(requestarr);

            var prestr = new StringBuilder();
            for (i = 0; i < Sortedstr.Length; i++)
            {
                if (Request.Form[Sortedstr[i]] != "" && Sortedstr[i] != "sign" && Sortedstr[i] != "sign_type")
                {
                    if (i == Sortedstr.Length - 1)
                    {
                        prestr.Append(Sortedstr[i] + "=" + Request.Form[Sortedstr[i]]);
                    }
                    else
                    {
                        prestr.Append(Sortedstr[i] + "=" + Request.Form[Sortedstr[i]] + "&");

                    }
                }
            }

            prestr.Append(key);

            string mysign = processor.GetMD5(prestr.ToString(), _input_charset);

            string sign = Request.Form["sign"];

            if (mysign == sign && responseTxt == "true")
            {
                if (Request.Form["trade_status"] == "WAIT_BUYER_PAY")
                {
                    string strOrderNo = Request.Form["out_trade_no"];
                    string strPrice = Request.Form["total_fee"];
                }
                else if (Request.Form["trade_status"] == "TRADE_FINISHED" || Request.Form["trade_status"] == "TRADE_SUCCESS")
                {
                    string strOrderNo = Request.Form["out_trade_no"];
                    string strPrice = Request.Form["total_fee"];

                    int orderId = 0;
                    if (Int32.TryParse(strOrderNo, out orderId))
                    {
                        var order = _orderService.GetOrderById(orderId);
                        if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            _orderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }
                else
                {
                }

                Response.Write("success");
            }
            else
            {
                Response.Write("fail");
                string logStr = "MD5:mysign=" + mysign + ",sign=" + sign + ",responseTxt=" + responseTxt;
                _logger.Error(logStr);
            }

            return Content("");
        }

        [ValidateInput(false)]
        public ActionResult Return()
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Tenpay") as TenpayPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Tenpay module cannot be loaded");

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}