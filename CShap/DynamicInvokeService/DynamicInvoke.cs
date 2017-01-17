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

namespace DynamicInvoke
{
    /// <summary>
    /// Web Service服务类
    /// </summary>
    public class WSHelper
    {
        /// < summary> 
        /// 动态调用web服务 （不含有SoapHeader）
        /// < /summary> 
        /// < param name="url">WSDL服务地址< /param> 
        /// < param name="methodname">方法名< /param> 
        /// < param name="args">参数< /param> 
        /// < returns>< /returns> 
        public static object InvokeWebService(string url, string methodname, object[] args)
        {
            return WSHelper.InvokeWebService(url, null, methodname, null, args);
        }
        /// <summary>
        /// 动态调用web服务（含有SoapHeader）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="methodname"></param>
        /// <param name="soapHeader"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object InvokeWebService(string url, string methodname, SoapHeader soapHeader, object[] args)
        {
            return WSHelper.InvokeWebService(url, null, methodname, soapHeader, args);
        }
        /// < summary> 
        /// 动态调用web服务 
        /// < /summary> 
        /// < param name="url">WSDL服务地址< /param> 
        /// < param name="classname">类名< /param> 
        /// < param name="methodname">方法名< /param> 
        /// < param name="args">参数< /param> 
        /// < returns>< /returns> 
        public static object InvokeWebService(string url, string classname, string methodname, SoapHeader soapHeader, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
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
                    {http://localhost:38076/Properties/
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
                        // typeClient.GetProperty(property.Key).SetValue(clientkey, property.Value, null);  
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
        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }

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
    }
}