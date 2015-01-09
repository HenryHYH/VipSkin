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
using Nop.Plugin.Payments.Tenpay.Library;

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

            //创建ResponseHandler实例
            ResponseHandler resHandler = new ResponseHandler(System.Web.HttpContext.Current);
            resHandler.SetKey(_tenpayPaymentSettings.Key);

            //判断签名
            if (resHandler.IsTenpaySign())
            {
                ///通知id
                string notify_id = resHandler.GetParameter("notify_id");
                //通过通知ID查询，确保通知来至财付通
                //创建查询请求
                RequestHandler queryReq = new RequestHandler(System.Web.HttpContext.Current);
                queryReq.Init();
                queryReq.SetKey(_tenpayPaymentSettings.Key);
                queryReq.SetGateUrl("https://gw.tenpay.com/gateway/simpleverifynotifyid.xml");
                queryReq.SetParameter("partner", _tenpayPaymentSettings.Partner);
                queryReq.SetParameter("notify_id", notify_id);

                //通信对象
                TenpayHttpClient httpClient = new TenpayHttpClient();
                httpClient.SetTimeOut(5);
                //设置请求内容
                httpClient.SetReqContent(queryReq.GetRequestURL());
                //后台调用
                if (httpClient.Call())
                {
                    //设置结果参数
                    ClientResponseHandler queryRes = new ClientResponseHandler();
                    queryRes.SetContent(httpClient.GetResContent());
                    queryRes.SetKey(_tenpayPaymentSettings.Key);
                    //判断签名及结果
                    //只有签名正确,retcode为0，trade_state为0才是支付成功
                    if (queryRes.IsTenpaySign())
                    {
                        //取结果参数做业务处理
                        string out_trade_no = queryRes.GetParameter("out_trade_no");
                        //财付通订单号
                        string transaction_id = queryRes.GetParameter("transaction_id");
                        //金额,以分为单位
                        string total_fee = queryRes.GetParameter("total_fee");
                        //如果有使用折扣券，discount有值，total_fee+discount=原请求的total_fee
                        string discount = queryRes.GetParameter("discount");
                        //支付结果
                        string trade_state = resHandler.GetParameter("trade_state");
                        //交易模式，1即时到帐 2中介担保
                        string trade_mode = resHandler.GetParameter("trade_mode");
                        #region
                        //判断签名及结果
                        if ("0".Equals(queryRes.GetParameter("retcode")))
                        {
                            //Response.Write("id验证成功");

                            if ("1".Equals(trade_mode))
                            {       //即时到账 
                                if ("0".Equals(trade_state))
                                {
                                    //------------------------------
                                    //即时到账处理业务开始
                                    //------------------------------

                                    //处理数据库逻辑
                                    //注意交易单不要重复处理
                                    //注意判断返回金额

                                    int orderId = 0;
                                    if (int.TryParse(out_trade_no, out orderId))
                                    {
                                        var order = _orderService.GetOrderById(orderId);
                                        if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                                        {
                                            _orderProcessingService.MarkOrderAsPaid(order);
                                        }
                                    }

                                    //------------------------------
                                    //即时到账处理业务完毕
                                    //------------------------------

                                    //给财付通系统发送成功信息，财付通系统收到此结果后不再进行后续通知
                                    Response.Write("即时到账支付成功");
                                }
                                else
                                {
                                    Response.Write("即时到账支付失败");
                                }
                            }
                            else if ("2".Equals(trade_mode))
                            {
                                //中介担保
                                //给财付通系统发送成功信息，财付通系统收到此结果后不再进行后续通知
                                Response.Write("success");
                            }
                        }
                        else
                        {
                            //错误时，返回结果可能没有签名，写日志trade_state、retcode、retmsg看失败详情。
                            //通知财付通处理失败，需要重新通知
                            Response.Write("查询验证签名失败或id验证失败");
                            _logger.Error("retcode:" + queryRes.GetParameter("retcode"));
                        }
                        #endregion
                    }
                    else
                    {
                        _logger.Error("通知ID查询签名验证失败");
                    }
                }
                else
                {
                    //通知财付通处理失败，需要重新通知
                    _logger.Error("后台调用通信失败");
                    //写错误日志
                    _logger.Error("call err:" + httpClient.GetErrInfo() + "|" + httpClient.GetResponseCode() + "");
                }
            }
            else
            {
                _logger.Error("签名验证失败");
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

            //创建ResponseHandler实例
            ResponseHandler resHandler = new ResponseHandler(System.Web.HttpContext.Current);
            resHandler.SetKey(_tenpayPaymentSettings.Key);

            //判断签名
            if (resHandler.IsTenpaySign())
            {
                //通知id
                string notify_id = resHandler.GetParameter("notify_id");
                //商户订单号
                string out_trade_no = resHandler.GetParameter("out_trade_no");
                //财付通订单号
                string transaction_id = resHandler.GetParameter("transaction_id");
                //金额,以分为单位
                string total_fee = resHandler.GetParameter("total_fee");
                //如果有使用折扣券，discount有值，total_fee+discount=原请求的total_fee
                string discount = resHandler.GetParameter("discount");
                //支付结果
                string trade_state = resHandler.GetParameter("trade_state");
                //交易模式，1即时到账，2中介担保
                string trade_mode = resHandler.GetParameter("trade_mode");

                if ("1".Equals(trade_mode))
                {
                    //即时到账 
                    if ("0".Equals(trade_state))
                    {
                        _logger.Debug(string.Format("订单{0}即时到帐付款成功", out_trade_no));
                        //给财付通系统发送成功信息，财付通系统收到此结果后不再进行后续通知
                    }
                    else
                    {
                        _logger.Error(string.Format("订单{0}即时到账支付失败", out_trade_no));
                    }
                }
                else if ("2".Equals(trade_mode))
                {
                    //中介担保
                    if ("0".Equals(trade_state))
                    {
                        _logger.Debug(string.Format("订单{0}中介担保付款成功", out_trade_no));
                        //给财付通系统发送成功信息，财付通系统收到此结果后不再进行后续通知
                    }
                    else
                    {
                        _logger.Error(string.Format("订单{0}中介担保付款失败", out_trade_no));
                    }
                }
            }
            else
            {
                _logger.Error("财付通Return认证签名失败");
            }

            //获取debug信息,建议把debug信息写入日志，方便定位问题
            string debuginfo = resHandler.GetDebugInfo();
            _logger.Debug(debuginfo);

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}