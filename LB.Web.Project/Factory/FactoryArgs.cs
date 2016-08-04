using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

namespace LB.Web.Project.Factory
{
    public class FactoryArgs
    {
        private DataTable _selectResult = null;
        public DataTable SelectResult
        {
            get
            {
                return _selectResult;
            }
            set
            {
                _selectResult = value;
            }
        }

        private string _LoginName = "";
        public string LoginName
        {
            get
            {
                return _LoginName;
            }
            set
            {
                _LoginName = value;
            }
        }

        public FactoryArgs(string _LoginName)
        {
            this._LoginName = _LoginName;
        } 
    }
}