using Kid_Guard_part1.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class GrowthMeasurement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ChildId { get; set; }

    [Required]
    public double Weight { get; set; } // الوزن المدخل

    [Required]
    public double Height { get; set; } // الطول المدخل

    [Required]
    public int AgeInMonths { get; set; } // العمر وقت القياس

    public string WeightStatus { get; set; } // نتيجة تحليل الوزن

    public string HeightStatus { get; set; } // نتيجة تحليل الطول

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // الربط بجدول الأطفال
    [ForeignKey("ChildId")]
    public virtual Child Child { get; set; }
}