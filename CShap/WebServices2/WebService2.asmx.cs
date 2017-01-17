using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebServices2
{
    /// <summary>
    /// 自定义MySoapHeader类
    /// </summary>
    public class MySoapHeader : System.Web.Services.Protocols.SoapHeader
    {

        public MySoapHeader() { }

        public MySoapHeader(string userName, string passWord)
        {
            this.userName = userName;
            this.passWord = passWord;
        }
        
        /// <summary>
        /// 验证用户SoapHeader
        /// </summary>
        /// <returns></returns>
        public Boolean Auth()
        {
            return ("admin".Equals(UserName) & "admin12345".Equals(PassWord)) ? true : false;
        }


        #region 属性定义
        private string userName = string.Empty;
        private string passWord = string.Empty;

        public string UserName
        {
            set
            {
                userName = value;
            }
            get
            {
                return userName;
            }
        }
        public string PassWord
        {
            set
            {
                passWord = value;
            }
            get
            {
                return passWord;
            }
        }
        #endregion
    }

    /// <summary>
    /// WebService2 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class WebService2 : System.Web.Services.WebService
    {

        public MySoapHeader mySoapHeader = new MySoapHeader();

        [WebMethod(Description = "SoapHeader验证")]
        [System.Web.Services.Protocols.SoapHeader("mySoapHeader")]
        public string HelloWorld()
        {
            //简单验证用户信息
            //可以通过数据库或其他方式验证
            if (mySoapHeader.Auth())
            {
                return "用户" + mySoapHeader.UserName + "验证通过！";
            }
            else
            {
                return "对不起，您没有访问权限！";
            }
        }

        [WebMethod(Description = "SoapHeader验证")]
        [System.Web.Services.Protocols.SoapHeader("mySoapHeader")]
        public string getNum(int a, int b)
        {
            //简单验证用户信息
            //可以通过数据库或其他方式验证
            if (mySoapHeader.Auth())
            {
                return (a * b).ToString();
            }
            else
            {
                return "对不起，您没有访问权限！";
            }
        }
        [WebMethod]
        public String getList()
        {
            return "list";
        }
    }
}
