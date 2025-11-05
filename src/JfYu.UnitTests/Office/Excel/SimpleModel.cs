using System.ComponentModel;

namespace JfYu.UnitTests.Office.Excel
{
    public class SimpleModel<T>
    {
        [DisplayName("A")]
        public T? Value { get; set; }
    }
}
