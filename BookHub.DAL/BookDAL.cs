using System.Data;
using Microsoft.Data.SqlClient;

namespace BookHub.DAL
{
    public class BookDAL
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=BookHubDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        public DataTable GetAllBooks()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM Books", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
