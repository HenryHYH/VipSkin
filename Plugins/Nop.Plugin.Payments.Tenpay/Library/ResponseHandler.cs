using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Nop.Plugin.Payments.Tenpay.Library
{
    /// <summary>
    /// ResponseHandler ��ժҪ˵����
    /// </summary>
    public class ResponseHandler
    {
        /** ��Կ */
        private string key;

        /** Ӧ��Ĳ��� */
        protected Hashtable parameters;

        /** debug��Ϣ */
        private string debugInfo;

        protected HttpContext httpContext;

        //��ȡ������֪ͨ���ݷ�ʽ�����в�����ȡ
        public ResponseHandler(HttpContext httpContext)
        {
            parameters = new Hashtable();

            this.httpContext = httpContext;
            NameValueCollection collection;
            if (this.httpContext.Request.HttpMethod == "POST")
            {
                collection = this.httpContext.Request.Form;
            }
            else
            {
                collection = this.httpContext.Request.QueryString;
            }

            foreach (string k in collection)
            {
                string v = (string)collection[k];
                this.SetParameter(k, v);
            }
        }

        /** ��ȡ��Կ */
        public string GetKey() { return key; }

        /** ������Կ */
        public void SetKey(string key) { this.key = key; }

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
                if (parameters.Contains(parameter))
                {
                    parameters.Remove(parameter);
                }

                parameters.Add(parameter, parameterValue);
            }
        }

        /** �Ƿ�Ƹ�ͨǩ��,������:����������a-z����,������ֵ�Ĳ������μ�ǩ���� 
         * @return boolean */
        public virtual bool IsTenpaySign()
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

            //debug��Ϣ
            this.SetDebugInfo(sb.ToString() + " => sign:" + sign);
            return GetParameter("sign").ToLower().Equals(sign);
        }

        /**
        * ��ʾ��������
        * @param show_url ��ʾ��������url��ַ,����url��ַ����ʽ(http://www.xxx.com/xxx.aspx)��
        * @throws IOException 
        */
        public void DoShow(string show_url)
        {
            string strHtml = "<html><head>\r\n" +
                "<meta name=\"TENCENT_ONLINE_PAYMENT\" content=\"China TENCENT\">\r\n" +
                "<script language=\"javascript\">\r\n" +
                "window.location.href='" + show_url + "';\r\n" +
                "</script>\r\n" +
                "</head><body></body></html>";

            this.httpContext.Response.Write(strHtml);

            this.httpContext.Response.End();
        }

        /** ��ȡdebug��Ϣ */
        public string GetDebugInfo()
        { return debugInfo; }

        /** ����debug��Ϣ */
        protected void SetDebugInfo(string debugInfo)
        { this.debugInfo = debugInfo; }

        protected virtual string GetCharset()
        {
            return this.httpContext.Request.ContentEncoding.BodyName;

        }

        /** �Ƿ�Ƹ�ͨǩ��,������:����������a-z����,������ֵ�Ĳ������μ�ǩ���� 
         * @return boolean */
        public virtual bool _isTenpaySign(ArrayList akeys)
        {
            StringBuilder sb = new StringBuilder();

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

            //debug��Ϣ
            this.SetDebugInfo(sb.ToString() + " => sign:" + sign);
            return GetParameter("sign").ToLower().Equals(sign);
        }
    }
}
