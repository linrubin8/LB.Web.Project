using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using LB.Web.Project.Factory;

namespace LB.Web.Project.Function
{
    public class DBUser : IBLLFunction
    {
        public override string GetFunctionName(int iFunctionType)
        {
            string strFunName = "";
            switch (iFunctionType)
            {
                case 10000:
                    strFunName = "DBUser_Insert";
                    break;

                case 10001:
                    strFunName = "DBUser_Update";
                    break;

                case 10002:
                    strFunName = "DBUser_Delete";
                    break;

                case 10003:
                    strFunName = "DBUser_ChangePassword";
                    break;
            }
            return strFunName;
        }

        public void DBUser_Insert(FactoryArgs args, out long UserID,string UserAccount,string UserPassword,string UserName)
        {
            UserID = 0;

            DataTable dtSP = new DataTable();
            dtSP.Columns.Add("UserID", typeof(long));
            dtSP.Columns.Add("UserAccount", typeof(string));
            dtSP.Columns.Add("UserPassword", typeof(string));
            dtSP.Columns.Add("UserName", typeof(string));

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

            args.SelectResult = dtSP;
            throw new Exception("dasfasfd");
        }

        //public void DBUser_ChangePassword(

    }
}