using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Tenpay.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using System.Web;
using Nop.Plugin.Payments.Tenpay.Library;

namespace Nop.Plugin.Payments.Tenpay
{
    /// <summary>
    /// Tenpay payment processor
    /// </summary>
    public class TenpayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly TenpayPaymentSettings _tenpayPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly HttpContextBase _httpContext;

        #endregion

        #region Ctor

        public TenpayPaymentProcessor(TenpayPaymentSettings tenpayPaymentSettings,
            ISettingService settingService, IWebHelper webHelper,
            IStoreContext storeContext,
            HttpContextBase httpContext)
        {
            this._tenpayPaymentSettings = tenpayPaymentSettings;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._httpContext = httpContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets MD5 hash
        /// </summary>
        /// <param name="Input">Input</param>
        /// <param name="Input_charset">Input charset</param>
        /// <returns>Result</returns>
        public string GetMD5(string Input, string Input_charset)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(Input_charset).GetBytes(Input));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Bubble sort
        /// </summary>
        /// <param name="Input">Input</param>
        /// <returns>Result</returns>
        public string[] BubbleSort(string[] Input)
        {
            int i, j;
            string temp;

            bool exchange;

            for (i = 0; i < Input.Length; i++)
            {
                exchange = false;

                for (j = Input.Length - 2; j >= i; j--)
                {
                    if (System.String.CompareOrdinal(Input[j + 1], Input[j]) < 0)
                    {
                        temp = Input[j + 1];
                        Input[j + 1] = Input[j];
                        Input[j] = temp;

                        exchange = true;
                    }
                }

                if (!exchange)
                {
                    break;
                }
            }
            return Input;
        }

        /// <summary>
        /// Create URL
        /// </summary>
        /// <param name="Para">Para</param>
        /// <param name="InputCharset">Input charset</param>
        /// <param name="Key">Key</param>
        /// <returns>Result</returns>
        public string CreatUrl(string[] Para, string InputCharset, string Key)
        {
            int i;
            string[] Sortedstr = BubbleSort(Para);
            StringBuilder prestr = new StringBuilder();

            for (i = 0; i < Sortedstr.Length; i++)
            {
                if (i == Sortedstr.Length - 1)
                {
                    prestr.Append(Sortedstr[i]);

                }
                else
                {
                    prestr.Append(Sortedstr[i] + "&");
                }

            }

            prestr.Append(Key);
            string sign = GetMD5(prestr.ToString(), InputCharset);
            return sign;
        }

        /// <summary>
        /// Gets HTTP
        /// </summary>
        /// <param name="StrUrl">Url</param>
        /// <param name="Timeout">Timeout</param>
        /// <returns>Result</returns>
        public string Get_Http(string StrUrl, int Timeout)
        {
            string strResult = string.Empty;
            try
            {
                HttpWebRequest myReq = (HttpWebRequest)HttpWebRequest.Create(StrUrl);
                myReq.Timeout = Timeout;
                HttpWebResponse HttpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = HttpWResp.GetResponseStream();
                StreamReader sr = new StreamReader(myStream, Encoding.Default);
                StringBuilder strBuilder = new StringBuilder();
                while (-1 != sr.Peek())
                {
                    strBuilder.Append(sr.ReadLine());
                }

                strResult = strBuilder.ToString();
            }
            catch (Exception exc)
            {
                strResult = "Error: " + exc.Message;
            }
            return strResult;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            string out_trade_no = postProcessPaymentRequest.Order.Id.ToString();
            decimal total_fee = postProcessPaymentRequest.Order.OrderTotal;
            string subject = _storeContext.CurrentStore.Name;
            string body = string.Format("{0}����", _storeContext.CurrentStore.Name);

            string notify_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentTenpay/Notify";
            string return_url = _webHelper.GetStoreLocation(false) + "Plugins/PaymentTenpay/Return";

            //����RequestHandlerʵ��
            RequestHandler reqHandler = new RequestHandler(System.Web.HttpContext.Current);

            //��ʼ��
            reqHandler.init();

            //������Կ
            reqHandler.SetKey(_tenpayPaymentSettings.Key);
            reqHandler.SetGateUrl("https://gw.tenpay.com/gateway/pay.htm");

            //����֧������
            reqHandler.SetParameter("partner", _tenpayPaymentSettings.Partner);		        //�̻���
            reqHandler.SetParameter("out_trade_no", out_trade_no);                          //�̼Ҷ�����
            reqHandler.SetParameter("total_fee", (total_fee * 100).ToString("N0").Replace(",", ""));//��Ʒ���,�Է�Ϊ��λ
            reqHandler.SetParameter("return_url", return_url);                              //������ɺ���ת��URL
            reqHandler.SetParameter("notify_url", notify_url);                              //���ղƸ�֪ͨͨ��URL
            reqHandler.SetParameter("body", body);                                          //��Ʒ����
            reqHandler.SetParameter("bank_type", "DEFAULT");                                //��������(�н鵣��ʱ�˲�����Ч)
            reqHandler.SetParameter("spbill_create_ip", _webHelper.GetCurrentIpAddress());  //�û��Ĺ���ip�������̻�������IP
            reqHandler.SetParameter("fee_type", "1");                                       //���֣�1�����
            reqHandler.SetParameter("subject", subject);                                    //��Ʒ����(�н齻��ʱ����)

            //ϵͳ��ѡ����
            reqHandler.SetParameter("sign_type", "MD5");
            reqHandler.SetParameter("service_version", "1.0");
            reqHandler.SetParameter("input_charset", "UTF-8");
            reqHandler.SetParameter("sign_key_index", "1");

            //ҵ���ѡ����
            reqHandler.SetParameter("attach", "");                                          //�������ݣ�ԭ������
            reqHandler.SetParameter("product_fee", "0");                                    //��Ʒ���ã����뱣֤transport_fee + product_fee=total_fee
            reqHandler.SetParameter("transport_fee", "0");                                  //�������ã����뱣֤transport_fee + product_fee=total_fee
            reqHandler.SetParameter("time_start", DateTime.Now.ToString("yyyyMMddHHmmss")); //��������ʱ�䣬��ʽΪyyyymmddhhmmss
            reqHandler.SetParameter("time_expire", "");                                     //����ʧЧʱ�䣬��ʽΪyyyymmddhhmmss
            reqHandler.SetParameter("buyer_id", "");                                        //�򷽲Ƹ�ͨ�˺�
            reqHandler.SetParameter("goods_tag", "");                                       //��Ʒ���
            reqHandler.SetParameter("trade_mode", "1");                                     //����ģʽ��1��ʱ����(Ĭ��)��2�н鵣����3��̨ѡ����ҽ�֧�������б�ѡ��
            reqHandler.SetParameter("transport_desc", "");                                  //����˵��
            reqHandler.SetParameter("trans_type", "1");                                     //�������ͣ�1ʵ�ｻ�ף�2���⽻��
            reqHandler.SetParameter("agentid", "");                                         //ƽ̨ID
            reqHandler.SetParameter("agent_type", "");                                      //����ģʽ��0�޴���(Ĭ��)��1��ʾ������ģʽ��2��ʾ����ģʽ
            reqHandler.SetParameter("seller_id", "");                                       //�����̻��ţ�Ϊ�����ͬ��partner

            //��ȡ�����������url
            string requestUrl = reqHandler.GetRequestURL();
            _httpContext.Response.Redirect(requestUrl);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _tenpayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //Tenpay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentTenpay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Tenpay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentTenpay";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Tenpay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentTenpayController);
        }

        public override void Install()
        {
            //settings
            var settings = new TenpayPaymentSettings()
            {
                Key = "",
                Partner = "",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.RedirectionTip", "You will be redirected to Tenpay site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.SellerEmail", "Seller email");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.SellerEmail.Hint", "Enter seller email.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.Key.Hint", "Enter key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.Partner", "Partner");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.Partner.Hint", "Enter partner.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Tenpay.AdditionalFee.Hint", "Enter additional fee to charge your customers.");

            base.Install();
        }


        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.SellerEmail.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.SellerEmail");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.SellerEmail.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.Partner");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.Partner.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Tenpay.AdditionalFee.Hint");

            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        #endregion
    }
}
