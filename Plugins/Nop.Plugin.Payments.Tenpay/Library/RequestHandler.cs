using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Nop.Plugin.Payments.Tenpay.Library
{
    /// <summary>
    /// RequestHandler 的摘要说明。
    /// </summary>
    public class RequestHandler
    {
        public RequestHandler(HttpContext httpContext)
        {
            parameters = new Dictionary<string, string>();

            this.httpContext = httpContext;
            this.SetGateUrl("https://www.tenpay.com/cgi-bin/v1.0/service_gate.cgi");
        }

        /** 网关url地址 */
        private string gateUrl;

        /** 密钥 */
        private string key;

        /** 请求的参数 */
        protected Dictionary<string, string> parameters;

        /** debug信息 */
        private string debugInfo;

        protected HttpContext httpContext;

        /** 初始化函数。*/
        public virtual void init()
        {
            //nothing to do
        }

        /** 获取入口地址,不包含参数值 */
        public string GetGateUrl()
        {
            return gateUrl;
        }

        /** 设置入口地址,不包含参数值 */
        public void SetGateUrl(string gateUrl)
        {
            this.gateUrl = gateUrl;
        }

        /** 获取密钥 */
        public string GetKey()
        {
            return key;
        }

        /** 设置密钥 */
        public void SetKey(string key)
        {
            this.key = key;
        }

        /** 获取带参数的请求URL  @return String */
        public virtual string GetRequestURL()
        {
            this.CreateSign();

            StringBuilder sb = new StringBuilder();
            ArrayList akeys = new ArrayList(parameters.Keys);
            akeys.Sort();
            foreach (string k in akeys)
            {
                string v = (string)parameters[k];
                if (null != v && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + TenpayUtil.UrlEncode(v, GetCharset()) + "&");
                }
            }

            //去掉最后一个&
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return this.GetGateUrl() + "?" + sb.ToString();
        }

        /**
        * 创建md5摘要,规则是:按参数名称a-z排序,遇到空值的参数不参加签名。
        */
        protected virtual void CreateSign()
        {
            StringBuilder sb = new StringBuilder();

            ArrayList akeys = new ArrayList(parameters.Keys);
            akeys.Sort();

            foreach (string k in akeys)
            {
                string v = (string)parameters[k];
                if (null != v && "".CompareTo(v) != 0
                    && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + v + "&");
                }
            }

            sb.Append("key=" + this.GetKey());
            string sign = MD5Util.GetMD5(sb.ToString(), GetCharset()).ToLower();

            this.SetParameter("sign", sign);

            //debug信息
            this.SetDebugInfo(sb.ToString() + " => sign:" + sign);
        }

        /** 获取参数值 */
        public string GetParameter(string parameter)
        {
            string s = (string)parameters[parameter];
            return (null == s) ? "" : s;
        }

        /** 设置参数值 */
        public void SetParameter(string parameter, string parameterValue)
        {
            if (parameter != null && parameter != "")
            {
                if (parameters.ContainsKey(parameter))
                {
                    parameters.Remove(parameter);
                }

                parameters.Add(parameter, parameterValue);
            }
        }

        public void DoSend()
        {
            this.httpContext.Response.Redirect(this.GetRequestURL());
        }

        /** 获取debug信息 */
        public String GetDebugInfo()
        {
            return debugInfo;
        }

        /** 设置debug信息 */
        public void SetDebugInfo(string debugInfo)
        {
            this.debugInfo = debugInfo;
        }

        public Dictionary<string, string> GetAllParameters()
        {
            return this.parameters;
        }

        protected virtual string GetCharset()
        {
            return this.httpContext.Request.ContentEncoding.BodyName;
        }
    }
}
