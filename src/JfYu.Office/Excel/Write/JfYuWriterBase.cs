using JfYu.Office.Excel.Constant;
using JfYu.Office.Excel.Extensions;
using JfYu.Office.Excel.Write.Interface;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace JfYu.Office.Excel.Write
{
    /// <summary>
    /// Abstract base class for writing data to an Excel workbook.
    /// </summary>
    /// <typeparam name="T">The type of data to be written to Excel.</typeparam>
    public abstract class JfYuWriterBase<T> : IJfYuExcelWrite<T> where T : notnull
    {
        /// <inheritdoc/>
        protected abstract void WriteDataToWorkbook(IWorkbook workbook, T source, JfYuExcelOptions writeOperation, Dictionary<string, string>? titles = null, Action<int>? callback = null);

        /// <inheritdoc/>
        public void Write(T source, string filePath, JfYuExcelOptions writeOperation, Dictionary<string, string>? titles = null, Action<int>? callback = null)
        {
            IWorkbook wb;
            if (File.Exists(filePath))
            {
                if (writeOperation.AllowAppend == WriteOperation.None)
                    throw new FileLoadException(nameof(filePath), "cannot create file,file is existing.");
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                wb = WorkbookFactory.Create(fs);
            }
            else
            {
                if (filePath.EndsWith(".xlsx"))
                    wb = JfYuExcelExtension.CreateExcel(JfYuExcelVersion.Xlsx);
                else if (filePath.EndsWith(".xls"))
                    wb = JfYuExcelExtension.CreateExcel(JfYuExcelVersion.Xls);
                else
                    throw new InvalidOperationException("Unsupported file format.");
            }
            WriteDataToWorkbook(wb, source, writeOperation, titles, callback);
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir) && !string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            using (var savefs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                wb.Write(savefs);
            wb.Dispose();
        }

        /// <inheritdoc/>
        public void SetValue(Type type, object? value, ICell cell)
        {
            if (value == null || value == DBNull.Value)
            {
                cell.SetCellType(CellType.Blank);
                return;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    cell.SetCellValue(Convert.ToDouble(value));
                    break;
                //Store values in text format to ensure accuracy
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    cell.SetCellValue(value.ToString());
                    break;
                case TypeCode.Double:
                    cell.SetCellValue(((double)value).ToString("G17", CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Single:
                    cell.SetCellValue(((float)value).ToString("G9", CultureInfo.InvariantCulture));
                    break;
                case TypeCode.Decimal:
                    cell.SetCellValue(((decimal)value).ToString("G29", CultureInfo.InvariantCulture));
                    break;

                case TypeCode.Boolean:
                    cell.SetCellValue(Convert.ToBoolean(value));
                    break;

                case TypeCode.DateTime:
                    cell.SetCellValue(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    break;

                case TypeCode.SByte:
                    cell.SetCellValue(Convert.ToSByte(value));
                    break;

                case TypeCode.Byte:
                    cell.SetCellValue(Convert.ToByte(value));
                    break;

                default:
                    cell.SetCellValue(Convert.ToString(value));
                    break;
            }
        }
    }
}