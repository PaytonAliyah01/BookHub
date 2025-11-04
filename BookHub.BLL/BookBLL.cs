using System.Data;
using BookHub.DAL;

namespace BookHub.BLL
{
    public class BookBLL
    {
        private readonly BookDAL bookDAL = new BookDAL();

        public DataTable GetAllBooks()
        {
            return bookDAL.GetAllBooks();
        }
    }
}
