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

        public bool ShowAll = false;
        public bool ShowAvailable = false;

        public int MaxBookID = 0;
        public int MaxRecID = 0;
        public int MaxUserID = 0;
        public int MaxFicBookID = 0;
        public int MaxAuthorID = 0;

        public List<Book> MyBooks = new List<Book>();
        public List<Record> MyRecords = new List<Record>();
        public List<User> MyUsers = new List<User>();

        public dbBooks()
        {
            /*   MaxBookID = GetMaxID("Books");
               MaxRecID = GetMaxID("Records");
               MaxUserID = GetMaxID("Users");
               MaxFicBookID = GetMaxID("FicBooks");
               MaxAuthorID = GetMaxID("Authors");*/
            FetchBooks();
            FetchUserRecords();
           // Book b = new Book { Quantity = 23, Name = "Fucking shoes", Author = "Александр Рудазов, Стивен Кинг" };
            //MAddBook(b);
           // FetchBooks();

            
        }


        public void MergeAuthors()
        {
            List<int> nums = GetMultipleAuthors();
            foreach (int i in nums)
            {
                Merge(i);
            }
        }

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
            MaxBookID = MyBooks[MyBooks.Count - 1].ID;
            MergeAuthors();
        }

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

        public void FetchUsers()
        {
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
                        MyUsers.Add(new User { ID = Convert.ToInt32(dr[0]), Email = dr[1].ToString(), IsAdmin = (bool)dr[2] });
                    }
                }

            }
            MaxUserID = MyUsers[MyUsers.Count - 1].ID;
        }



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



        public void BookAdd(string name, int quantity)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Books(Name, Quantity) values('" + name + "', " + quantity.ToString() + ")";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            MaxBookID++;
        }

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

        public void FicBookAdd(int bookID, int authorID)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into FicBooks(BookID, AuthorID) values(" + bookID + ", " + authorID + ")";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            MaxFicBookID++;
            FetchBooks();
        }

        public void FicBookDelete(int bookID)
        {
            //Удаляет по признаку книги.
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Delete from FicBooks where BookID =" + bookID.ToString();
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        public void FicBookIDDelete(int id)
        {
            //Удаляет по признаку ID
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "Delete from FicBooks where ID =" + id;
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            FetchBooks();
        }

        public void AuthorAdd(string name)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Authors(Name) values('" + name + "' )";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            MaxAuthorID++;
            FetchBooks();
        }

        public void AuthorDelete(int id)
        {
            //Let it be empty for now
        }

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

        public void RecAdd(int bookID, int userID)
        {
            using (SqlConnection con = new SqlConnection(strConnection))
            {
                string sql = "insert into Records values(" + bookID + ", " + userID + ", '" + DateTime.Now.ToString() + "')";
                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            MaxRecID++;
        }


        public void MAddBook(Book b)
        {
            this.BookAdd(b.Name,b.Quantity);
            int? tmp;
            //tmp = this.GetAuthorID(b.Author)
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

        public void MDelBook(int bID)
        {
            FicBookDelete(bID);
            BookDelete(bID);
        }

        public void MUpdateBook(Book b)
        {
            List<int> tmp = FetchFicBooks(b.ID);
            //List<string> stmp = SplitAuthors(b.Author);
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



        public List<string> SplitAuthors(string authors)
        {
            string[] res;
            res = authors.Split(',');
            List<string> r = res.ToList<string>();
            return r;
        }





    }
}