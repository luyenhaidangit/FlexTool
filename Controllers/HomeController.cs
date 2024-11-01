using Dapper;
using FlexTool.Data;
using FlexTool.Entities;
using FlexTool.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using FileIO = System.IO.File;

namespace FlexTool.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            var contentBuilder = BuildContentDeferror(deferror);

            var content = Encoding.UTF8.GetBytes(contentBuilder.ToString());
            var contentType = "application/octet-stream";
            var fileName = $"DEFERROR.{deferror.ErrNum}.sql";

            if (request.IsUploadGit != true)
            {
                return File(content, contentType, fileName);
            }

            var baseFolderPath = _configuration["ConfigPath:DatabaseFolder"];
            var folderPath = Path.Combine(baseFolderPath, "1_parameter", "DEFERROR");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var filePath = Path.Combine(folderPath, fileName);

            await FileIO.WriteAllTextAsync(filePath, contentBuilder.ToString(), Encoding.UTF8);

            return Ok(Result.Success());
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
