namespace FlexTool.Models
{
    public class Result
    {
        public string Status { get; set; }

        public string Desc { get; set; }

        public Result(string status, string message)
        {
            Status = status;
            Desc = message;
        }

        public static Result Success()
        {
            return new Result("0", "Thành công!");
        }

        public static Result Error(string status, string message)
        {
            return new Result(status, message);
        }
    }
}
