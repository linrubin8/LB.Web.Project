using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Services;
using LB.Web.Project.DBConnect;
using LB.Web.Project.Factory;
using MySql.Data.MySqlClient;

namespace LB.Web.Project
{
    /// <summary>
    /// LBWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class LBWebService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public DataSet RunProcedure(int ProcedureType,string strLoginName, DataTable dtParmValue,
            out DataTable dtOut,out string ErrorMsg,out bool bolIsError)
        {
            dtOut = null;
            bolIsError = false;
            DataSet dsReturn = null;
            ErrorMsg = "";
            try
            {
                LBFactory factory = new LBFactory();
                IBLLFunction function = factory.GetAssemblyFunction(ProcedureType);

                if (function == null)
                {
                    #region -- 调用存储过程 --

                    DataTable dtView = SQLServerDAL.Query("select * from dbo.SysSPType where SysSPType=" + ProcedureType);
                    if (dtView.Rows.Count > 0)
                    {
                        DataRow drView = dtView.Rows[0];
                        string strSysSPName = drView["SysSPName"].ToString().TrimEnd();
                        SQLServerDAL.ExecuteProcedure(strSysSPName, dtParmValue, out dtOut, out dsReturn);
                    }
                    else
                    {
                        throw new Exception("存储过程号【" + ProcedureType + "】不存在！");
                    }

                    #endregion
                }
                else
                {
                    #region -- 调用中间层程序方法 --


                    string strMethod = function.GetFunctionName(ProcedureType);
                    string str = function.ToString();

                    Assembly s = Assembly.Load("LB.Web.Project");
                    Type tpe = s.GetType(str);


                    //调用GetName方法
                    MethodInfo method = tpe.GetMethod(strMethod);
                    int iRowIndex = 0;
                    foreach (DataRow drParmValue in dtParmValue.Rows)
                    {
                        //获取需要传入的参数
                        ParameterInfo[] parms = method.GetParameters();

                        FactoryArgs factoryArgs = new FactoryArgs(strLoginName);
                        Dictionary<int, string> dictOutFieldName = new Dictionary<int, string>();
                        object[] objValue = new object[parms.Length];
                        int iParmIndex = 0;
                        foreach (ParameterInfo ss in parms)
                        {
                            string strParmName = ss.Name;
                            if (ss.ParameterType == typeof(FactoryArgs))
                            {
                                objValue[iParmIndex] = factoryArgs;
                            }
                            else if (ss.Attributes != ParameterAttributes.Out)
                            {
                                if (dtParmValue.Columns.Contains(strParmName))
                                {
                                    objValue[iParmIndex] = drParmValue[strParmName];

                                }
                            }
                            else
                            {
                                if (dtOut == null)
                                {
                                    dtOut = new DataTable("Out");
                                }
                                if (!dtOut.Columns.Contains(strParmName))
                                {
                                    dtOut.Columns.Add(strParmName, typeof(object));
                                }
                                dictOutFieldName.Add(iParmIndex, strParmName);
                            }

                            iParmIndex++;
                        }

                        if (dtOut != null)
                        {
                            dtOut.Rows.Add(dtOut.NewRow());
                        }

                        //获取Car对象
                        object obj = s.CreateInstance(str);

                        //如果有返回值接收下
                        method.Invoke(obj, objValue);
                        int iobjReturnIndex = 0;
                        foreach (object objReturn in objValue)
                        {
                            if (objReturn is FactoryArgs)
                            {
                                FactoryArgs args = (FactoryArgs)objReturn;
                                if (args.SelectResult != null)
                                {
                                    if (dsReturn == null)
                                    {
                                        dsReturn = new DataSet("DSResult");
                                    }
                                    args.SelectResult.TableName = "Return" + iRowIndex.ToString();
                                    dsReturn.Tables.Add(args.SelectResult.Copy());
                                }
                            }
                            if (dictOutFieldName.ContainsKey(iobjReturnIndex))
                            {
                                dtOut.Rows[0][dictOutFieldName[iobjReturnIndex]] = objReturn;
                            }
                            iobjReturnIndex++;
                        }
                        iRowIndex++;
                    }

                    #endregion -- 调用中间层程序方法 --
                }
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.InnerException.Message;
                bolIsError = true;
            }
            return dsReturn;
        }

        [WebMethod]
        public DataTable RunView(int iViewType, string strLoginName, string strFieldNames, string strWhere, string strOrderBy,
            out string ErrorMsg, out bool bolIsError)
        {
            DataTable dtReturn = null;
            bolIsError = false;
            ErrorMsg = "";

            try
            {
                DataTable dtView = SQLServerDAL.Query("select * from dbo.SysViewType where SysViewType=" + iViewType);
                if (dtView.Rows.Count == 0)
                {
                    throw new Exception("查询出错！视图号：【" + iViewType+"】不存在！");
                }
                string strSysViewName = dtView.Rows[0]["SysViewName"].ToString().TrimEnd();
                DataTable dtViewExists = SQLServerDAL.Query(@"
select * from sysobjects 
where id = object_id(N'["+strSysViewName+@"]')
");
                if (dtViewExists.Rows.Count == 0)
                {
                    throw new Exception("查询出错！视图名称：【" + strSysViewName + "】不存在！");
                }

                string strFields = string.IsNullOrEmpty(strFieldNames) ? "*" : strFieldNames;
                strWhere =  string.IsNullOrEmpty(strWhere)?"":"where "+strWhere;
                strOrderBy =  string.IsNullOrEmpty(strOrderBy)?"":"Order By "+strOrderBy;
                string strSQL = @"
select {0}
from {1}
{2}
{3}
";
                strSQL = string.Format(strSQL, strFields, strSysViewName, strWhere, strOrderBy);
                dtReturn = SQLServerDAL.Query(strSQL);
                dtReturn.TableName = "Result";
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.InnerException.Message;
                bolIsError = true;
            }
            return dtReturn;
        }

        [WebMethod]
        public void User_Insert(string strAccount,string strPassword,string strName)
        {
            /*StringBuilder sb = new StringBuilder();
            sb.Append("INSERT INTO DBUser(UserAccount,UserPassword,UserName) ");
            sb.Append("VALUES(?UserAccount,?UserPassword,?UserName) ");
            MySqlParameter[] parameters = {
                                     new MySqlParameter("?UserAccount", MySqlDbType.String),
                                     new MySqlParameter("?UserPassword", MySqlDbType.String),
                                     new MySqlParameter("?UserName", MySqlDbType.String)
                                 };
            parameters[0].Value = strAccount;
            parameters[1].Value = strPassword;
            parameters[2].Value = strName;
            DBConn.ExecuteNonQuery(sb.ToString(), CommandType.Text, parameters);*/

            /*DataTable dtSP = new DataTable();
            dtSP.Columns.Add("UserID",typeof(long));
            dtSP.Columns.Add("UserAccount",typeof(string));
            dtSP.Columns.Add("UserPassword",typeof(string));
            dtSP.Columns.Add("UserName",typeof(string));

            DataRow drNew = dtSP.NewRow();
            drNew["UserAccount"] = "林汝斌";
            drNew["UserPassword"] = "林汝斌";
            drNew["UserName"] = "林汝斌";
            dtSP.Rows.Add(drNew);
            drNew = dtSP.NewRow();
            drNew["UserAccount"] = "林汝斌1";
            drNew["UserPassword"] = "林汝斌1";
            drNew["UserName"] = "林汝斌1";
            dtSP.Rows.Add(drNew);

            DataTable dtOut;
            DataSet dsReturn;
            SQLServerDAL.ExecuteProcedure("DBUser_Insert111", dtSP, out dtOut, out dsReturn);
            //IDataParameter[] parameters = new 
            throw new Exception("eeee");*/

            LBFactory factory = new LBFactory();
            IBLLFunction function = factory.GetAssemblyFunction(10000);
            string strMethod = function.GetFunctionName(10000);
            string str = function.ToString();

            Assembly s = Assembly.Load("LB.Web.Project");


            Type tpe = s.GetType(str);

            //调用GetName方法
            MethodInfo method = tpe.GetMethod(strMethod);

            //获取需要传入的参数
            ParameterInfo[] parms = method.GetParameters();

            //这里是判断参数类型
            foreach (ParameterInfo ss in parms)
            {
                if (ss.ParameterType == typeof(string))
                {
                    Console.WriteLine("Yes");
                }
            }

            //获取Car对象
            object obj = s.CreateInstance("RecleTest.Car");

            //如果有返回值接收下
            method.Invoke(obj, new object[] { "小小" });
        }
    }
}
