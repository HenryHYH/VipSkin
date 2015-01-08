using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Nop.Plugin.Payments.Tenpay.Library
{
    /// <summary>
    /// RequestHandler ��ժҪ˵����
    /// </summary>
    public class RequestHandler
    {
        public RequestHandler(HttpContext httpContext)
        {
            parameters = new Dictionary<string, string>();

            this.httpContext = httpContext;
            this.SetGateUrl("https://www.tenpay.com/cgi-bin/v1.0/service_gate.cgi");
        }

        /** ����url��ַ */
        private string gateUrl;

        /** ��Կ */
        private string key;

        /** ����Ĳ��� */
        protected Dictionary<string, string> parameters;

        /** debug��Ϣ */
        private string debugInfo;

        protected HttpContext httpContext;

        /** ��ʼ��������*/
        public virtual void init()
        {
            //nothing to do
        }

        /** ��ȡ��ڵ�ַ,����������ֵ */
        public string GetGateUrl()
        {
            return gateUrl;
        }

        /** ������ڵ�ַ,����������ֵ */
        public void SetGateUrl(string gateUrl)
        {
            this.gateUrl = gateUrl;
        }

        /** ��ȡ��Կ */
        public string GetKey()
        {
            return key;
        }

        /** ������Կ */
        public void SetKey(string key)
        {
            this.key = key;
        }

        /** ��ȡ������������URL  @return String */
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

            //ȥ�����һ��&
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return this.GetGateUrl() + "?" + sb.ToString();
        }

        /**
        * ����md5ժҪ,������:����������a-z����,������ֵ�Ĳ������μ�ǩ����
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

            //debug��Ϣ
            this.SetDebugInfo(sb.ToString() + " => sign:" + sign);
        }

        /** ��ȡ����ֵ */
        public string GetParameter(string parameter)
        {
            string s = (string)parameters[parameter];
            return (null == s) ? "" : s;
        }

        /** ���ò���ֵ */
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

        /** ��ȡdebug��Ϣ */
        public String GetDebugInfo()
        {
            return debugInfo;
        }

        /** ����debug��Ϣ */
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
