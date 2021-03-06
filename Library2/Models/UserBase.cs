﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Library2.Models
{
    public static class UserBase
    {
        public static List<User> Users = new List<User>();

        public static string strConnection = ConfigurationManager.ConnectionStrings["LibraryDb"].ConnectionString.ToString();

        public static User CurrentUser;

        static UserBase()
        {
            FetchUsers();
        }

        /// <summary>
        /// Fetches Users records from the database
        /// </summary>
        public static void FetchUsers()
        {
            Users.Clear();
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select * from Users";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();


                SqlDataReader dr;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Users.Add(new User { ID = Convert.ToInt32(dr[0]), Email = dr[1].ToString(), IsAdmin = (bool)dr[2] });
                    }
                }

            }
        }

        /// <summary>
        /// Checkes if user is registered in the base
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static bool IsRegistered(ref User u)
        {
            FetchUsers();
            string tmp = u.Email;
            var f = Users.Find(p => p.Email==tmp);
            List<User> nl = Users;
            if (f == null)
                return false;
            else
            {
                u.ID = f.ID;
                u.IsAdmin = f.IsAdmin;
                return true;
            }
        }

        /// <summary>
        /// Adds a new user to the base
        /// </summary>
        /// <param name="email"></param>
        /// <param name="isAdmin"></param>
        public static void UserAdd(string email, bool isAdmin = false)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Users values('" + email + "', " + (isAdmin ? 1 : 0) + ")";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchUsers();
        }
        
    }
}