using JfYu.Data.Model;
using System.ComponentModel.DataAnnotations;

namespace JfYu.Benchmark.Models;

public class BenchmarkUser : BaseEntity
{
    [MaxLength(100)]
    public string UserName { get; set; } = "";

    [MaxLength(200)]
    public string Email { get; set; } = "";
}
