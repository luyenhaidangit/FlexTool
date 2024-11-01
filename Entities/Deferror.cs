using System.ComponentModel.DataAnnotations.Schema;

namespace FlexTool.Entities
{
    public class Deferror
    {
        public int ErrNum { get; set; }

        public string? ErrDesc { get; set; }

        public string? EnErrDesc { get; set; }

        public string? ModCode { get; set; }

        public int? ConfLvl { get; set; }
    }
}