#if NET8_0_OR_GREATER
using JfYu.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JfYu.UnitTests.Models.Entity
{
    public class User : BaseEntity
    {
        /// <summary>
        /// UserName
        /// </summary>
        [DisplayName("UserName"), Required, MaxLength(100)]
        public string UserName { get; set; } = "";

        /// <summary>
        /// NickName
        /// </summary>
        [DisplayName("NickName"), Required, MaxLength(100)]
        public string? NickName { get; set; }

        /// <summary>
        /// DepartmentId
        /// </summary>
        [DisplayName("DepartmentId")]
        public int? DepartmentId { get; set; }

        /// <summary>
        /// Department
        /// </summary>
        public virtual Department? Department { get; set; }
    }

    public class Department : BaseEntity
    {
        /// <summary>
        /// Name
        /// </summary>
        [DisplayName("Name"), Required]
        public string Name { get; set; } = "";

        /// <summary>
        /// SubName
        /// </summary>
        [DisplayName("SubName"), Required]
        public string SubName { get; set; } = "";

        /// <summary>
        /// SuperiorId
        /// </summary>
        [DisplayName("SuperiorId")]
        public int? SuperiorId { get; set; }

        /// <summary>
        /// Superior
        /// </summary>
        [DisplayName("Superior")]
        public virtual Department? Superior { get; set; }

        /// <summary>
        /// Users
        /// </summary>
        [DisplayName("Users")]
        public virtual List<User>? Users { get; set; }
    }

    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
    }
}
#endif