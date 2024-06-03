using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace project1_online_selling.Models
{
    public class DBLayer
    {
        SqlConnection conn;
        public DBLayer()
        {
            conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connection"].ConnectionString);
        }

        //method to execute a stored procedure that return number of rows effects
        public int ExecuteDML(string procname, SqlParameter[] prms)
        {
            SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            foreach (SqlParameter p in prms)
            {
                if (p.Value != null)
                {
                    cmd.Parameters.Add(p);
                }
            }
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            int res = cmd.ExecuteNonQuery();
            conn.Close();
            return res;
        }

        //method with two parameter to execute a stored procedure that return datatable as a response 
        public DataTable ExecuteSelect(string procname, SqlParameter[] prms)
        {
            SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            foreach (SqlParameter p in prms)
            {
                if (p.Value != null)
                {
                    cmd.Parameters.Add(p);
                }
            }
         DataTable dataTable = new DataTable();
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dataTable);
            return dataTable;

        }
        //method with single parameter to execute a stored procedure that return datatable as a response 

        public DataTable ExecuteSelect(string procname)
        {
            SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            DataTable dataTable = new DataTable();
            SqlDataAdapter adp = new SqlDataAdapter(cmd);
            adp.Fill(dataTable);
            return dataTable;

        }

        //Method to execute stored procedure that return one row and only one colunm as a response

        public object ExecuteScalar(string procname, SqlParameter[] prms)
        {
            SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            foreach (SqlParameter p in prms)
            {
                if (p.Value != null)
                {
                    cmd.Parameters.Add(p);
                }
            }

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            object res = cmd.ExecuteScalar();
            conn.Close();
            return res;
        }

        //Method to execute stored procedure that return one row and only one colunm as a response for those who does not need any parameter

        public object ExecuteScalar(string procname)
        {
            SqlCommand cmd = new SqlCommand(procname, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            object res = cmd.ExecuteScalar();
            conn.Close();
            return res;
        }

    }
}