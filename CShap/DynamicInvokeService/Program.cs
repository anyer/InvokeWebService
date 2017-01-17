using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace DynamicInvokeService
{
    class Program
    {
        static void Main(string[] args)
        {
            //string url = "http://192.168.1.109:81/WebService1.asmx";
            ////string url = "http://192.168.1.109:82/WebService2.asmx";

            //Dictionary<string, object> properties = new Dictionary<string, object>();
            //properties.Add("UserName", "admin");
            //properties.Add("PassWord", "admin123");

            //DynamicInvoke.WSHelper.SoapHeader soapHeader = new DynamicInvoke.WSHelper.SoapHeader("MySoapHeader", properties);

            //Object obj1 = DynamicInvoke.WSHelper.InvokeWebService(url, "HelloWorld", null);
            //Object obj2 = DynamicInvoke.WSHelper.InvokeWebService(url, "HelloWorld", soapHeader, null);
            //Object obj3 = DynamicInvoke.WSHelper.InvokeWebService(url, "getNum", new Object[] { 1, 2 });
            //Object obj4 = DynamicInvoke.WSHelper.InvokeWebService(url, "getNum", soapHeader, new Object[] { 1, 2 });
            //Object obj5 = DynamicInvoke.WSHelper.InvokeWebService(url, "getList", null);
            //Object obj6 = DynamicInvoke.WSHelper.InvokeWebService(url, "getList", soapHeader, null);

            //Console.WriteLine("无SoapHeader调用HelloWorld：" + obj1);
            //Console.WriteLine("有SoapHeader调用HelloWorld：" + obj2);
            //Console.WriteLine("无SoapHeader调用getNum：" + obj3);
            //Console.WriteLine("有SoapHeader调用getNum：" + obj4);
            //Console.WriteLine("无SoapHeader调用无验证方法getList：" + obj5.GetType().IsArray);
            //Console.WriteLine("有SoapHeader调用无验证方法getList：" + obj6);

            Console.WriteLine("AppSettings:"+ConfigurationManager.AppSettings["URL"]);

            Console.ReadKey();
        }
    }
}
