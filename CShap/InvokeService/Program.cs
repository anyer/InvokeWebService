using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InvokeService
{
    class Program
    {
        static void Main(string[] args)
        {
            MyWebServices.WebService1SoapClient service = new MyWebServices.WebService1SoapClient();
            MyWebServices.MySoapHeader header = new MyWebServices.MySoapHeader();

            //未设置SoapHeader的服务调用
            Console.WriteLine("未设置SoapHeader的服务调用:" + service.HelloWorld(header));
            Console.WriteLine();

            //将用户名与密码存入SoapHeader;
            header.UserName = "admin";
            header.PassWord = "admin123";

            ////设置SoapHeader的服务调用
            Console.WriteLine("未设置SoapHeader的服务调用:" + service.HelloWorld(header));
            Console.Read();
        }
    }
}
