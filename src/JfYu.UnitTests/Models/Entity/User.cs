#if NET8_0_OR_GREATER
using JfYu.Data.Model; 
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
}
#endif