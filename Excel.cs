using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;

namespace Yun.Common
{
    public class Excel
    {
        public Excel()
        {
        }

        public DataTable ExcelToDataTable(string path, string sheetName)
        {
            var work = GetWork(path);
            var sheet = work.GetSheet(sheetName);
            return this.ExcelToDataTable(sheet);
        }

        public DataTable GetDataTable(DataSet ds, string name)
        {
            return ds.Tables.Contains(name) ? ds.Tables[name] : null;
        }

        public DataSet ExcelToDataTable(string path, params string[] sheetName)
        {
            var work = GetWork(path);
            DataSet ds = new DataSet();
            foreach (var item in sheetName)
            {
                var sheet = work.GetSheet(item);
                try
                {
                    if (sheet != null)
                        ds.Tables.Add(this.ExcelToDataTable(sheet));
                }
                catch (Exception ex)
                {
                }
            }
            return ds;
        }

        public DataSet ExcelToDataTables(string path)
        {
            var work = GetWork(path);
            DataSet ds = new DataSet();
            var length = work.NumberOfSheets;
            for (int i = 0; i < length; i++)
            {
                var sheet = work.GetSheetAt(i);
                try
                {
                    if (sheet != null)
                        ds.Tables.Add(this.ExcelToDataTable(sheet));
                }
                catch (Exception)
                {
                }
            }
            return ds;
        }

        private IWorkbook GetWork(string path)
        {
            var ext = System.IO.Path.GetExtension(path);
            if (ext.ToUpper() == ".xls".ToUpper())
            {
                return new HSSFWorkbook(new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite));
            }
            else if (ext.ToUpper() == ".xlsx".ToUpper())
            {
                return new XSSFWorkbook(new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite));
            }
            else
                return null;
        }

        private DataTable ExcelToDataTable(ISheet sheet)
        {
            DataTable data = new DataTable();
            data.TableName = sheet.SheetName;
            IRow firstRow = sheet.GetRow(0);
            int cellCount = firstRow.LastCellNum;
            var isFirstRowColumn = true;
            int startRow = 0;
            if (isFirstRowColumn)
            {
                for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                {
                    ICell cell = firstRow.GetCell(i);
                    if (cell != null)
                    {
                        string cellValue = cell.StringCellValue;
                        if (cellValue != null)
                        {
                            DataColumn column = new DataColumn(cellValue);
                            data.Columns.Add(column);
                        }
                    }
                }
                startRow = sheet.FirstRowNum + 1;
            }
            else
            {
                startRow = sheet.FirstRowNum;
            }
            int rowCount = sheet.LastRowNum;
            for (int i = startRow; i <= rowCount; ++i)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue;

                DataRow dataRow = data.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; ++j)
                {
                    if (row.GetCell(j) != null)
                    {
                        var obj = row.GetCell(j);
                        switch (obj.CellType)
                        {
                            case CellType.Boolean:
                                dataRow[j] = obj.BooleanCellValue;
                                break;

                            case CellType.Error:
                                dataRow[j] = ErrorEval.GetText(obj.ErrorCellValue);
                                break;

                            case CellType.Formula:
                                switch (obj.CachedFormulaResultType)
                                {
                                    case CellType.Boolean:
                                        dataRow[j] = obj.BooleanCellValue;
                                        break;

                                    case CellType.Error:
                                        dataRow[j] = ErrorEval.GetText(obj.ErrorCellValue);
                                        break;

                                    case CellType.Numeric:
                                        if (DateUtil.IsCellDateFormatted(obj))
                                        {
                                            dataRow[j] = obj.DateCellValue.ToString("yyyy-MM-dd hh:MM:ss");
                                        }
                                        else
                                        {
                                            dataRow[j] = obj.NumericCellValue;
                                        }
                                        break;

                                    case CellType.String:
                                        string str = obj.StringCellValue;
                                        if (!string.IsNullOrEmpty(str))
                                        {
                                            dataRow[j] = str.ToString();
                                        }
                                        else
                                        {
                                            dataRow[j] = null;
                                        }
                                        break;

                                    case CellType.Unknown:
                                    case CellType.Blank:
                                    default:
                                        dataRow[j] = string.Empty;
                                        break;
                                }
                                break;

                            case CellType.Numeric:
                                if (DateUtil.IsCellDateFormatted(obj))
                                {
                                    dataRow[j] = obj.DateCellValue.ToString("yyyy-MM-dd hh:MM:ss");
                                }
                                else
                                {
                                    dataRow[j] = obj.NumericCellValue;
                                }
                                break;

                            case CellType.String:
                                string strValue = obj.StringCellValue;
                                if (!string.IsNullOrEmpty(strValue))
                                {
                                    dataRow[j] = strValue.ToString();
                                }
                                else
                                {
                                    dataRow[j] = null;
                                }
                                break;

                            case CellType.Unknown:
                            case CellType.Blank:
                            default:
                                dataRow[j] = string.Empty;
                                break;
                        }
                    }
                }
                data.Rows.Add(dataRow);
            }
            return data;
        }

        /// <summary>
        /// 将DataTable数据导入到excel中
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(string fileName, DataTable data, string sheetName, bool isColumnWritten)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            IWorkbook workbook = null;
            if (fileName.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (fileName.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();

            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }
    }
}