using System.ComponentModel.DataAnnotations;

namespace Kid_Guard_part1.Model
{
    public class GrowthStandard
    {
        [Key]
        public byte Month { get; set; } // غير دي من int لـ byte
        public double SD3neg { get; set; }
        public double SD2neg { get; set; }
        public double SD1neg { get; set; }
        public double SD0 { get; set; }
        public double SD1 { get; set; }
        public double SD2 { get; set; }
        public double SD3 { get; set; }
    }
}
