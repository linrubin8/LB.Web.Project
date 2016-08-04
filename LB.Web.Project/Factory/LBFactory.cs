using System;
using System.Collections.Generic;
using System.Web;
using LB.Web.Project.Function;

namespace LB.Web.Project.Factory
{
    public class LBFactory
    {
        public IBLLFunction GetAssemblyFunction(int iFunctionType)
        {
            switch (iFunctionType)
            {
                case 10000:
                case 10001:
                case 10002:
                case 10003:
                    return new DBUser();
                    break;
            }

            return null;
        }
    }
}