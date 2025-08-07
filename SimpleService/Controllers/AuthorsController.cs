using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleService.Model;
using SimpleService.Request;
using SimpleService.Respons;
using SimpleService.Tools;
using System.Data;

namespace SimpleService.Controllers
{
    [ApiController]
    [Route("api/Authors")]
    public class AuthorsController : ControllerBase
    {
        private readonly SimpleDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthorsController(SimpleDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var authors = await _context.Authors.Take(10)               
                .ToListAsync();

            return Ok(authors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateAuthor")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuthorRespons>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateAuthors(List<AuthorRequest> author, Guid tenantId, Guid instanceId)
        {
            //var list = author.Where(x => x.AuthorName.Contains(' '));
            //if (list.Count() > 0)
            //{
            //    return BadRequest();
            //}

            BulkTool bt = new BulkTool(_configuration);
            DataTable table = bt.GetDataTableLayout("SimpleAuthors");

            foreach (var item in author)
            {
                DataRow newRow = table.NewRow();
                newRow["PublicId"] = Guid.NewGuid();
                newRow["Name"] = item.AuthorName;
                newRow["TraceId"] = item.TraceId;
                newRow["TenantId"] = tenantId;
                newRow["InstanceId"] = instanceId;
                table.Rows.Add(newRow);
            }

            string[] keyColumns = new string[] { "TraceId", "InstanceId", "TenantId" };
            string[] updateColumns = new string[] { "Name" };
            //string[] getColumns = new string[] { "PublicId", "Name", "TraceId" };
            string[] getColumns = new string[] { "PublicId", "TraceId" };
            DataTable res = await bt.BulkInsertUpdateAsync("SimpleAuthors", table, keyColumns, updateColumns, getColumns, true);

            List<AuthorRespons> authors = new List<AuthorRespons>();

            foreach (DataRow row in res.Rows)
            {
                var temp = new AuthorRespons
                {
                    PublicId = Guid.Parse(row["PublicId"].ToString()),
                    TraceId = row["TraceId"].ToString()
                };
                authors.Add(temp);
            }

            return Ok(authors);
        }
    }
}
