using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Library2.Models
{
    public class dbBooks
    {
        string strConnection = ConfigurationManager.ConnectionStrings["LibraryDb"].ConnectionString.ToString();

        
        public List<Book> MyBooks = new List<Book>();
        public List<Record> MyRecords = new List<Record>();

        public dbBooks()
        {
            FetchBooks();
            FetchUserRecords();
        }

        /// <summary>
        /// Method for correct display
        /// </summary>
        public void MergeAuthors()
        {
            List<int> nums = GetMultipleAuthors();
            foreach (int i in nums)
            {
                Merge(i);
            }
        }

        /// <summary>
        /// Suppurt method for correct display
        /// </summary>
        public void Merge(int i)
        {
            string aus = "";
            List<Book> tmp = new List<Book>();
            Book btmp = new Book();
            tmp = MyBooks.FindAll(p => p.ID == i);
            foreach(Book b in tmp)
            {
                aus = aus + b.Author + ", ";
            }
            aus = aus.Remove(aus.Length - 2);
            btmp.Author = aus;
            btmp.ID = tmp[0].ID;
            btmp.Quantity = tmp[0].Quantity;
            btmp.Name = tmp[0].Name;

            MyBooks.RemoveAll(p=>p.ID == i);
            MyBooks.Add(btmp);
        }

        /// <summary>
        /// Returns list of books IDs with multiple authors
        /// </summary>
        /// <returns></returns>
        public List<int> GetMultipleAuthors()
        {
            List<int> res = new List<int>();
            foreach (Book b in MyBooks)
            {
                if (MyBooks.FindAll(p => p.ID == b.ID).Count > 1)
                {
                    if(!res.Contains(b.ID))
                    res.Add(b.ID);
                }
            }
            return res;
        }

        /// <summary>
        /// Fetches books records from the base
        /// </summary>
        public void FetchBooks()
        {
            MyBooks.Clear();
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select Books.ID as'BookID', Authors.Name, Books.Name, Books.Quantity from Authors join FicBooks on FicBooks.AuthorID=Authors.ID join Books on FicBooks.BookID = Books.ID  order by (BookID)";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();


                SqlDataReader dr;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        MyBooks.Add(new Book { ID = Convert.ToInt32(dr[0]), Author = dr[1].ToString(), Name = dr[2].ToString(), Quantity = Convert.ToInt32(dr[3]) });
                    }
                }
            }
            MergeAuthors();
        }

        /// <summary>
        /// Fetches Records records from the base
        /// </summary>
        public void FetchUserRecords()
        {
            MyRecords.Clear();
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select Records.ID, Users.[E-mail], Books.Name, Records.Date from Records join Users on users.ID=Records.UserID join Books on Books.ID=Records.BookID";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
                SqlDataReader dr;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        MyRecords.Add(new Record { ID = Convert.ToInt32(dr[0]), User = dr[1].ToString(), Book = dr[2].ToString(), Date =dr[3].ToString() });
                    }
                }

            }
        }

        /// <summary>
        /// Returns Max ID of the rerords from the table.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <returns>ID</returns>
        public int GetMaxID(string table)
        {
            int? res;
            string str;
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select Max(ID) from " + table;
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                str = cmd.ExecuteScalar().ToString();
                if (cmd.ExecuteScalar().ToString() != "")
                    res = (int)cmd.ExecuteScalar();
                else
                    res = null;
            }
            if (res == null)
                return 0;
            else
                return Convert.ToInt32(res);
        }

        /// <summary>
        /// Returns ID of an author.
        /// </summary>
        /// <param name="name">Name of authir</param>
        /// <returns>ID</returns>
        public int? GetAuthorID(string name)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select ID From Authors where Name = '"+ name +"'";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();


                SqlDataReader dr;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        return Convert.ToInt32(dr[0]);
                    }
                }

            }
            return null;
        }

        /// <summary>
        /// Adds a book in the table of database (support)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="quantity"></param>
        public void BookAdd(string name, int quantity)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Books(Name, Quantity) values('" + name + "', " + quantity.ToString() + ")";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes a book from database (support)
        /// </summary>
        /// <param name="id"></param>
        public void BookDelete(int id)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Delete from Books where ID=" + id.ToString();
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Changes quantity of the book in the case, someone took it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool BookTake(int id)
        {
            int q = 0;
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select Quantity From Books where ID = "+ id;
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
                SqlDataReader dr;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                    while (dr.Read())
                        q = (int)dr[0];
            }
            if (q == 0) return false;
            BookUpdate(id, q-1);
            RecAdd(id,UserBase.CurrentUser.ID);
            return true;
        }

        /// <summary>
        /// Updates information about a book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void BookUpdate(int id, int quantity)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Update Books set Quantity = " + quantity.ToString() + " where ID=" + id.ToString();

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        /// <summary>
        /// Updates information about a book 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        /// <param name="name"></param>
        public void BookUpdate(int id, int quantity, string name)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Update Books set Quantity = " + quantity.ToString() + ", Name = '"+ name +"'" + " where ID=" + id.ToString();

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        /// <summary>
        /// Adds a book to the database (support)
        /// </summary>
        /// <param name="bookID"></param>
        /// <param name="authorID"></param>
        public void FicBookAdd(int bookID, int authorID)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into FicBooks(BookID, AuthorID) values(" + bookID + ", " + authorID + ")";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        /// <summary>
        /// Deletes a book from database by the name (support)
        /// </summary>
        /// <param name="bookID"></param>
        public void FicBookDelete(int bookID)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Delete from FicBooks where BookID =" + bookID.ToString();
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        /// <summary>
        /// Deletes a book from the library by the ID (support)
        /// </summary>
        /// <param name="id"></param>
        public void FicBookIDDelete(int id)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Delete from FicBooks where ID =" + id;
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        /// <summary>
        /// Adds an author to the database
        /// </summary>
        /// <param name="name"></param>
        public void AuthorAdd(string name)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Authors(Name) values('" + name + "' )";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        /// <summary>
        /// Adds a user to the database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="isAdmin"></param>
        public void UserAdd(string email, bool isAdmin = false)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Users values(" + email + ", " + isAdmin + ")";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds a record (book tracking) to the database
        /// </summary>
        /// <param name="bookID"></param>
        /// <param name="userID"></param>
        public void RecAdd(int bookID, int userID)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Records values(" + bookID + ", " + userID + ", '" + DateTime.Now.ToString() + "')";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds a book to the database (full record)
        /// </summary>
        /// <param name="b"></param>
        public void MAddBook(Book b)
        {
            this.BookAdd(b.Name,b.Quantity);
            int? tmp;
            foreach (string x in SplitAuthors(b.Author))
            {
                tmp = GetAuthorID(x);
                if (tmp != null)
                {
                    FicBookAdd(GetMaxID("Books"), (int)tmp);
                }
                else
                {
                    AuthorAdd(x);
                    FicBookAdd(GetMaxID("Books"), (int)GetAuthorID(x));
                }
            }
            b.ID = GetMaxID("Books");
            MyBooks.Add(b);
            
        }

        /// <summary>
        /// Deletes a book from database (full record)
        /// </summary>
        /// <param name="bID"></param>
        public void MDelBook(int bID)
        {
            FicBookDelete(bID);
            BookDelete(bID);
        }

        /// <summary>
        /// Updates book data in the database (full record)
        /// </summary>
        /// <param name="b"></param>
        public void MUpdateBook(Book b)
        {
            List<int> tmp = FetchFicBooks(b.ID);
            List<int> AuthorsID = new List<int>();

            foreach (string x in SplitAuthors(b.Author))
            {
                if (GetAuthorID(x) == null)
                    AuthorAdd(x);
                AuthorsID.Add((int)GetAuthorID(x));
            }

            int switcher;
            switcher = (tmp.Count - AuthorsID.Count) > 0 ? 1 : -1;
            if (tmp.Count == AuthorsID.Count) switcher = 0;

            
            switch (switcher)
            {
                case -1: 
                    {
                        for (int i = 0; i < AuthorsID.Count - tmp.Count; i++)
                        {
                            FicBookAdd(b.ID, AuthorsID[0]);
                        }
                        tmp = FetchFicBooks(b.ID);
                        for (int i = 0; i < tmp.Count; i++ )
                        {
                            FicBAUpdate(tmp[i], AuthorsID[i]);
                        }

                            break; 
                    }
                case 0:
                    {
                        for (int i = 0; i < tmp.Count; i++)
                        {
                            FicBAUpdate(tmp[i], AuthorsID[i]);
                        }
                        break;
                    }
                case 1: 
                    {
                        for (int i = 0; i < tmp.Count - AuthorsID.Count; i++)
                        {
                            FicBookIDDelete(tmp[i]);
                        }
                        tmp = FetchFicBooks(b.ID);

                        for (int i = 0; i < tmp.Count; i++)
                        {
                            FicBAUpdate(tmp[i], AuthorsID[i]);
                        }
                        break; 
                    }
            }

            BookUpdate(b.ID, b.Quantity, b.Name);


        }

        /// <summary>
        /// Updates data in the support table of the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="aid"></param>
        public void FicBAUpdate(int id, int aid)
        {
              using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Update FicBooks Set AuthorID="+ aid +" where ID ="+id;

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Fetches records from the support book table of the database with simillar IDs
        /// </summary>
        /// <param name="bID"></param>
        /// <returns></returns>
        public List<int> FetchFicBooks(int bID)
        {
            List<int> tmp = new List<int>();
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Select ID from FicBooks where BookID = "+bID;
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
                SqlDataReader dr;
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        tmp.Add((int)dr[0]);
                    }
                }

            }
            return tmp;
        }

        /// <summary>
        /// Splits authors line by comas
        /// </summary>
        /// <param name="authors"></param>
        /// <returns></returns>
        public List<string> SplitAuthors(string authors)
        {
            string[] res;
            res = authors.Split(',');
            List<string> r = res.ToList<string>();
            return r;
        }





    }
}