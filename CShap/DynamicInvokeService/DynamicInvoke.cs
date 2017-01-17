using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Web.Services.Description;
using System.CodeDom;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Configuration;

namespace DynamicInvoke
{
    /// <summary>
    /// WebService操作类 
    /// </summary>
    public class WSHelper
    {
        /// <summary>
        /// 最终封装调用WebService方法 
        /// 方法中通过InitSoapHeader()方法给定SoapHeader
        /// </summary>
        /// <param name="methodname">方法名称</param>
        /// <param name="args">方法参数</param>
        /// <returns></returns>
        public static object InvokeWebService(string methodname, object[] args)
        {
            return InvokeWebService(null, null, methodname, InitSoapHeader(), args);
        }

        /// <summary>
        /// 调用Webservice方法，不含SoapHeader验证
        /// </summary>
        /// <param name="url">WSDL服务地址</param>
        /// <param name="methodname">方法名称</param>
        /// <param name="args">方法参数</param>
        /// <returns></returns>
        //public static object InvokeWebService(string url, string methodname, object[] args)
        //{
        //    return InvokeWebService(url, null, methodname, null, args);
        //}

        /// <summary>
        /// 调用Webservice方法，含SoapHeader
        /// </summary>
        /// <param name="url">WSDL服务地址</param>
        /// <param name="methodname">方法名称</param>
        /// <param name="args">方法参数</param>
        /// <returns></returns>
        //public static object InvokeWebService(string url, string methodname, SoapHeader soapheader, object[] args)
        //{
        //    return InvokeWebService(url, null, methodname, soapheader, args);
        //}

        /// <summary>
        /// 调用WebService方法
        /// </summary>
        /// <param name="url">WSDL服务地址</param>
        /// <param name="classname">类名</param>
        /// <param name="methodname">方法名称</param>
        /// <param name="soapHeader">soap头信息</param>
        /// <param name="args">方法参数</param>
        /// <returns></returns>
        public static object InvokeWebService(string url, string classname, string methodname, SoapHeader soapHeader, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";

            if (null == url)
            {
                if (null == WSHelper.GetServiceUrl())
                {
                    return "检查App.config配置文件中是否配置了WebService的URL";
                }
                else
                {
                    url = WSHelper.GetServiceUrl();
                }
            }
            if ((classname == null) || (classname == ""))
            {
                classname = WSHelper.GetWsClassName(url);
            }
            try
            {
                //获取WSDL 
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");
                ServiceDescription sd = ServiceDescription.Read(stream);
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                CodeNamespace cn = new CodeNamespace(@namespace);

                //生成客户端代理类代码 
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider icc = new CSharpCodeProvider();

                //设定编译参数 
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                //编译代理类 
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }

                //保存生产的代理类，默认是保存在bin目录下面  
                TextWriter writer = File.CreateText("MyWebServices.cs");
                icc.GenerateCodeFromCompileUnit(ccu, writer, null);
                writer.Flush();
                writer.Close();

                //生成代理实例 
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);

                #region 设置SoapHeader
                FieldInfo client = null;
                object clientkey = null;
                if (soapHeader != null)
                {
                    client = t.GetField(soapHeader.ClassName + "Value");
                    //获取客户端验证对象    soap类  
                    Type typeClient = assembly.GetType(@namespace + "." + soapHeader.ClassName);
                    //为验证对象赋值  soap实例    
                    clientkey = Activator.CreateInstance(typeClient);
                    //遍历属性  
                    foreach (KeyValuePair<string, object> property in soapHeader.Properties)
                    {
                        typeClient.GetField(property.Key).SetValue(clientkey, property.Value);
                        //typeClient.GetProperty(property.Key).SetValue(clientkey, property.Value, null);  
                    }
                }
                #endregion

                if (soapHeader != null)
                {
                    //设置Soap头  
                    client.SetValue(obj, clientkey);
                    //pro.SetValue(obj, soapHeader, null);  
                }

                //调用指定的方法
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                //方法名错误（找不到方法），给出提示
                if (null == mi)
                {
                    return "方法名不存在，请检查方法名是否正确！";
                }
                return mi.Invoke(obj, args);
                // PropertyInfo propertyInfo = type.GetProperty(propertyname); 
                //return propertyInfo.GetValue(obj, null); 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message, new Exception(ex.InnerException.StackTrace));
            }
        }

        /// <summary>
        /// 通过判断是否带有SoapHeader验证，初始化SoapHeader
        /// </summary>
        /// <returns></returns>
        private static SoapHeader InitSoapHeader()
        {
            SoapHeader soapHeader = null;
            if (IsSoapHeaderInvoke())
            { //带有SoapHeader验证调用时，传SoapHeader的username和password值
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add("UserName", GetSHUserName());
                properties.Add("PassWord", GetSHPassWord());
                soapHeader = new SoapHeader("MySoapHeader", properties);
            }

            return soapHeader;
        }

        #region 读取App.config 获取url、userName、passWord
        /// <summary>
        /// 读取App.config文件中的url
        /// </summary>
        /// <returns></returns>
        private static string GetServiceUrl()
        {
            return ConfigurationManager.AppSettings["URL"];
        }

        /// <summary>
        /// 读取App.config文件中的userName
        /// </summary>
        /// <returns></returns>
        private static string GetSHUserName()
        {
            return ConfigurationManager.AppSettings["UserName"];
        }

        /// <summary>
        /// 读取App.config文件中的passWord
        /// </summary>
        /// <returns></returns>
        private static string GetSHPassWord()
        {
            return ConfigurationManager.AppSettings["PassWord"];
        }

        /// <summary>
        /// 读取App.config文件，获取URL、UserName、PassWord
        /// </summary>
        /// <returns></returns>
        private static String[] ReadAppConfig()
        {

            String[] configValue = new String[3];
            String url = ConfigurationManager.AppSettings["URL"];
            String userName = ConfigurationManager.AppSettings["UserName"];
            String passWord = ConfigurationManager.AppSettings["PassWord"];

            configValue[0] = url;
            configValue[1] = userName;
            configValue[2] = passWord;

            return configValue;
        }
        #endregion


        /// <summary>
        /// 通过验证App.config中是否配置UserName及PassWord来判断是否SoapHeader请求
        /// 返回true表示带有SoapHeader验证，返回false表示不带有SoapHeader验证
        /// </summary>
        /// <returns></returns>        
        private static Boolean IsSoapHeaderInvoke()
        {
            //获取值中有空，返回false，表示不带SoapHeader验证调用
            return (null == GetSHUserName() || null == GetSHPassWord()) ? false : true;
        }

        /// <summary>
        /// 通过WSDL服务地址获取类名
        /// </summary>
        /// <param name="wsUrl">WSDL服务地址</param>
        /// <returns></returns>
        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }



        #region 构建SOAP头，用于SoapHeader的验证
        /// <summary>  
        /// 构建SOAP头，用于SoapHeader验证  
        /// </summary>  
        public class SoapHeader
        {
            /// <summary>  
            /// 构造一个SOAP头  
            /// </summary>  
            public SoapHeader()
            {
                this.Properties = new Dictionary<string, object>();
            }
            /// <summary>  
            /// 构造一个SOAP头  
            /// </summary>  
            /// <param name="className">SOAP头的类名</param>  
            public SoapHeader(string className)
            {
                this.ClassName = className;
                this.Properties = new Dictionary<string, object>();
            }
            /// <summary>  
            /// 构造一个SOAP头  
            /// </summary>  
            /// <param name="className">SOAP头的类名</param>  
            /// <param name="properties">SOAP头的类属性名及属性值</param>  
            public SoapHeader(string className, Dictionary<string, object> properties)
            {
                this.ClassName = className;
                this.Properties = properties;
            }
            /// <summary>  
            /// SOAP头的类名  
            /// </summary>  
            public string ClassName { get; set; }
            /// <summary>  
            /// SOAP头的类属性名及属性值  
            /// </summary>  
            public Dictionary<string, object> Properties { get; set; }
            /// <summary>  
            /// 为SOAP头增加一个属性及值  
            /// </summary>  
            /// <param name="name">SOAP头的类属性名</param>  
            /// <param name="value">SOAP头的类属性值</param>  
            public void AddProperty(string name, object value)
            {
                if (this.Properties == null)
                {
                    this.Properties = new Dictionary<string, object>();
                }
                Properties.Add(name, value);
            }
        }
        #endregion
    }
}