using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using System.Data;


namespace BookHub.Presentation.Pages
{
    public class BooksModel : PageModel
    {
        private readonly BookBLL bookBLL = new BookBLL();
        public DataTable BooksTable { get; set; } = new DataTable();

        public void OnGet()
        {
            BooksTable = bookBLL.GetAllBooks();
        }
    }
}
