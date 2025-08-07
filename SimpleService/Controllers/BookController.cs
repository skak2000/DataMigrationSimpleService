using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleService.Request;
using SimpleService.Respons;
using SimpleService.Tools;
using System.Data;
using System.Diagnostics;

namespace SimpleService.Controllers
{
    [ApiController]
    [Route("api/Books")]
    public class BookController : ControllerBase
    {
        private readonly SimpleDbContext _context;
        private readonly IConfiguration _configuration;

        public BookController(SimpleDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid authorId)
        {
            var authors = await _context.Authors
                .Include(a => a.Books)
                .Where(x => x.PublicId == authorId)
                .ToListAsync();
            return Ok(authors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bookList"></param>
        /// <param name="tenantId"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("CreateBooks")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuthorRespons>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateBooks(List<BookRequest> bookList, Guid tenantId, Guid instanceId)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            BulkTool bt = new BulkTool(_configuration);

            // Get all internal id's for SimpleAuthors
            List<Guid> publicId = bookList.Select(x => x.AuthorId).ToList();
            List<KeyValue> keyValue = bt.GetMappings(publicId, "SimpleAuthors", tenantId, instanceId);
            Dictionary<Guid, KeyValue> mappingDict = keyValue.ToDictionary(x => x.PublicId);

            Console.WriteLine("GetMappings: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            DataTable table = bt.GetDataTableLayout("SimpleBooks");
            Console.WriteLine("GetDataTableLayout: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            foreach (BookRequest item in bookList)
            {
                mappingDict.TryGetValue(item.AuthorId, out KeyValue? temp);
                //KeyValue temp = keyValue.FirstOrDefault(x => x.PublicId == item.AuthorId);

                if (temp != null)
                {
                    DataRow newRow = table.NewRow();
                    newRow["PublicId"] = Guid.NewGuid();
                    newRow["Title"] = item.Title;
                    newRow["Description"] = item.Description;
                    newRow["ISBN"] = item.ISBN;
                    newRow["TraceId"] = item.TraceId;
                    newRow["TenantId"] = tenantId;
                    newRow["InstanceId"] = instanceId;
                    newRow["AuthorId"] = temp.Id;
                    table.Rows.Add(newRow);
                }
            }
            Console.WriteLine("Create DT: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            int valuex = table.Rows.Count;
            string[] keyColumns = new string[] { "TraceId", "InstanceId", "TenantId", "AuthorId" };
            string[] updateColumns = new string[] { "Title", "Description", "ISBN" };
            // string[] getColumns = new string[] { "PublicId", "Title", "Description", "ISBN", "TraceId" };
            string[] getColumns = new string[] { "PublicId", "TraceId" };

            DataTable res = await bt.BulkInsertUpdateAsync("SimpleBooks", table, keyColumns, updateColumns, getColumns, true);

            Console.WriteLine("BulkInsertUpdate: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            List<BookRespons> books = new List<BookRespons>();
                                    
            foreach (DataRow row in res.Rows)
            {
                var temp = new BookRespons
                {
                    PublicId = Guid.Parse(row["PublicId"].ToString()),
                    TraceId = row["TraceId"].ToString()
                };
                books.Add(temp);
            }

            Console.WriteLine("Map to DTO: " + stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            return Ok(books);
        }
    }
}
