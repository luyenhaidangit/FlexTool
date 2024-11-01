using Dapper;
using FlexTool.Data;
using FlexTool.Entities;
using FlexTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FlexTool.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Deferror
        [HttpPost("export-deferror")]
        public async Task<IActionResult> ExportDeferror([FromBody] RenderDeferrorRequest request)
        {
            var query = "SELECT ERRNUM, ERRDESC, EN_ERRDESC ENERRDESC, MODCODE, CONFLVL FROM DEFERROR WHERE ERRNUM = :ErrNum";

            var deferror = new Deferror();

            using (var connection = _context.CreateConnection())
            {
                deferror = await connection.QuerySingleOrDefaultAsync<Deferror>(query, new { request.ErrNum });
            }

            if (deferror is null)
            {
                return NotFound($"Deferror với ErrNum {request.ErrNum} không tồn tại.");
            }

            var test = EscapeSingleQuotes(deferror.EnErrDesc);

            var contentBuilder = this.BuildContentDeferror(deferror);

            var content = Encoding.UTF8.GetBytes(contentBuilder.ToString());
            var contentType = "application/octet-stream";
            var fileName = $"deferror_{deferror.ErrNum}.sql";

            return File(content, contentType, fileName);
        }

        private StringBuilder BuildContentDeferror(Deferror deferror)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SET DEFINE OFF;");
            sb.AppendLine();

            sb.AppendLine($"DELETE FROM DEFERROR WHERE 1 = 1 AND ERRNUM = {deferror.ErrNum};");
            sb.AppendLine();
            sb.AppendLine("Insert into DEFERROR");
            sb.AppendLine("   (ERRNUM, ERRDESC, EN_ERRDESC, MODCODE, CONFLVL)");
            sb.AppendLine(" Values");
            sb.AppendLine($"   ({deferror.ErrNum}, '{FormatSqlString(deferror.ErrDesc)}', '{FormatSqlString(deferror.EnErrDesc)}', '{FormatSqlString(deferror.ModCode)}', {FormatSqlNumber(deferror.ConfLvl)});");
            sb.AppendLine();

            sb.Append("COMMIT;");

            return sb;
        }

        private string FormatSqlNumber(int? input)
        {
            return input.HasValue ? input.Value.ToString() : "NULL";
        }

        private string FormatSqlString(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : $"{EscapeSingleQuotes(input)}";
        }

        private string EscapeSingleQuotes(string input)
        {
            return input?.Replace("'", "''");
        }
        #endregion
    }
}
