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
    [Route("api/chapter")]
    public class ChapterController : ControllerBase
    {
        private readonly SimpleDbContext _context;
        private readonly IConfiguration _configuration;

        public ChapterController(SimpleDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="author"></param>
        /// <param name="tenantId"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("CreateChapters")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuthorRespons>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CreateChapters(List<ChapterRequest> input, Guid tenantId, Guid instanceId)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            BulkTool bt = new BulkTool(_configuration);

            List<Guid> publicId = input.Select(x => x.BookId).ToList();
            List<KeyValue> keyValue = bt.GetMappings(publicId, "SimpleBooks", tenantId, instanceId);
            Dictionary<Guid, KeyValue> mappingDict = keyValue.ToDictionary(x => x.PublicId);

            DataTable table = bt.GetDataTableLayout("SimpleChapters");

            foreach (var item in input)
            {
                mappingDict.TryGetValue(item.BookId, out KeyValue? temp);

                if (temp != null)
                {
                    DataRow newRow = table.NewRow();
                    newRow["PublicId"] = Guid.NewGuid();
                    newRow["Title"] = item.Title;
                    newRow["ChapterNumber"] = item.ChapterNumber;
                    newRow["Text"] = item.Text;
                    newRow["TraceId"] = item.TraceId;
                    newRow["TenantId"] = tenantId;
                    newRow["InstanceId"] = instanceId;
                    newRow["BookId"] = temp.Id;
                    table.Rows.Add(newRow);
                }
            }

            stopwatch.Stop();
            Console.WriteLine("Match key time: " + stopwatch.ElapsedMilliseconds);

            string[] keyColumns = new string[] { "TraceId", "InstanceId", "TenantId", "AuthorId" };
            string[] updateColumns = new string[] { "Title", "ChapterNumber", "Text" };
            //string[] getColumns = new string[] { "PublicId", "Title", "ChapterNumber", "Text", "TraceId" };
            string[] getColumns = new string[] { "PublicId", "TraceId" };
            
            DataTable res = await bt.BulkInsertUpdateAsync("SimpleChapters", table, keyColumns, updateColumns, getColumns, true);                       
            List<ChapterRespons> chapters = new List<ChapterRespons>();

            foreach (DataRow row in res.Rows)
            {
                var temp = new ChapterRespons
                {
                    PublicId = Guid.Parse(row["PublicId"].ToString()),
                    TraceId = row["TraceId"].ToString()
                };
                chapters.Add(temp);
            }

            return Ok(chapters);
        }
    }
}
