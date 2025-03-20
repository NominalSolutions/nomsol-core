using NotVisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace nomsol.core.utilities.converters
{
    public static class datatables
    {
        /// <summary>
        /// Sorts DataTable by column and direction
        /// </summary>
        /// <param name="dataTable">DataTable to sort</param>
        /// <param name="columnName">Column name</param>
        /// <param name="direction">ASC or DESC</param>
        public static DataTable SortDataTableByColumn(this DataTable dataTable, string columnName, string direction)
        {
            if (dataTable.Columns[columnName] == null)
            {
                throw new ArgumentException($"Column '{columnName}' does not exist in the DataTable.");
            }
            if (direction.ToUpper() != "ASC" && direction.ToUpper() != "DESC")
            {
                throw new ArgumentException("Direction must be 'ASC' or 'DESC'.");
            }
            dataTable.DefaultView.Sort = $"{columnName} {direction}";
            return dataTable.DefaultView.ToTable();
        }

        /// <summary>
        /// Convert DB List to DataTable
        /// </summary>
        /// <param name="dbList">Pass List()</param>
        public static DataTable ToDataTable<T>(this List<T> dbList)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (T item in dbList)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        /// <summary>
        /// Convert class to DataTable
        /// </summary>
        /// <param name="thisClass">Pass class || Usage: typeof(class).CreateDataTableFromType()</param>
        public static DataTable CreateDataTableFromType(this Type thisClass)
        {
            DataTable returnDataTable = new DataTable();
            PropertyInfo[] properties = thisClass.GetProperties();
            foreach (PropertyInfo info in properties)
            {
                returnDataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }
            return returnDataTable;
        }

        /// <summary>
        /// Convert list (e.g., from JSON) to DataTable
        /// </summary>
        /// <param name="data">Pass IList || Usage: .ListToDataTable()</param>
        public static DataTable ListToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        /// <summary>
        /// Convert DataTable to JSON
        /// </summary>
        /// <param name="table">Pass DataTable</param>
        public static string DataTableToJson(this DataTable table)
        {
            return JsonSerializer.Serialize(table);
        }

        /// <summary>
        /// Convert DataTable to List
        /// </summary>
        /// <param name="table">Pass DataTable || Usage: ConvertDataTableToList<ClassFileName>(DataTable)</param>
        public static List<T> ConvertDataTableToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();
                PropertyInfo[] properties = typeof(T).GetProperties();
                foreach (DataRow row in table.Rows)
                {
                    T obj = new T();
                    foreach (PropertyInfo prop in properties)
                    {
                        if (table.Columns.Contains(prop.Name))
                        {
                            try
                            {
                                object value = row[prop.Name];
                                if (value != DBNull.Value)
                                {
                                    prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType), null);
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException($"Error setting property '{prop.Name}': {ex.Message}", ex);
                            }
                        }
                    }
                    list.Add(obj);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to convert DataTable to List.", ex);
            }
        }

        /// <summary>
        /// Parse CSV file into DataTable (delimiter specified)
        /// </summary>
        /// <param name="csvFilePath">Path to CSV file to parse</param>
        /// <param name="delimiter">Delimiter character</param>
        public static DataTable CsvToDataTable(string csvFilePath, char delimiter)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (CsvTextFieldParser csvReader = new CsvTextFieldParser(csvFilePath))
                {
                    csvReader.SetDelimiter(delimiter);
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    if (colFields == null)
                    {
                        throw new InvalidOperationException("CSV file is empty or invalid.");
                    }
                    foreach (string column in colFields)
                    {
                        DataColumn dateColumn = new DataColumn(column)
                        {
                            AllowDBNull = true
                        };
                        csvData.Columns.Add(dateColumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException("CSV file not found.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse CSV file.", ex);
            }
            return csvData;
        }

        /// <summary>
        /// Convert source delimited text file to DataTable
        /// </summary>
        /// <param name="filePath">Pass file path</param>
        /// <param name="_class">Pass DataTable class definition (use typeof(class))</param>
        /// <param name="delimiter">Pass delimiter character</param>
        /// <param name="quoteEnclosed">Are strings enclosed in quotes?</param>
        public static DataTable FileToClassDataTable(string filePath, Type _class, char delimiter, bool quoteEnclosed)
        {
            DataTable dt = new DataTable();
            PropertyInfo[] properties = _class.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                dt.Columns.Add(pi.Name);
            }
            using (CsvTextFieldParser csvReader = new CsvTextFieldParser(filePath))
            {
                csvReader.SetDelimiter(delimiter);
                csvReader.HasFieldsEnclosedInQuotes = quoteEnclosed;
                while (!csvReader.EndOfData)
                {
                    string[] fieldData = csvReader.ReadFields();
                    for (int i = 0; i < fieldData.Length; i++)
                    {
                        if (fieldData[i] == "")
                        {
                            fieldData[i] = null;
                        }
                    }
                    dt.Rows.Add(fieldData);
                }
            }
            return dt;
        }

        /// <summary>
        /// Rename DataTable column
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public static void RenameColumn(this DataTable dataTable, string oldName, string newName)
        {
            if (dataTable != null && !string.IsNullOrEmpty(oldName) && !string.IsNullOrEmpty(newName) && oldName != newName)
            {
                int idx = dataTable.Columns.IndexOf(oldName);
                if (idx >= 0)
                {
                    dataTable.Columns[idx].ColumnName = newName;
                    dataTable.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Removes column from DataTable
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnName"></param>
        public static void RemoveColumn(this DataTable dataTable, string columnName)
        {
            if (dataTable != null && !string.IsNullOrEmpty(columnName))
            {
                int idx = dataTable.Columns.IndexOf(columnName);
                if (idx >= 0)
                {
                    dataTable.Columns.RemoveAt(idx);
                    dataTable.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Converts DataTable to HTML
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableClass">Optional CSS class for the table</param>
        /// <param name="rowClass">Optional CSS class for rows</param>
        /// <param name="cellClass">Optional CSS class for cells</param>
        /// <returns></returns>
        public static string ConvertDataTableToHtml(this DataTable dataTable, string tableClass = "", string rowClass = "", string cellClass = "")
        {
            string html = $"<table{(string.IsNullOrEmpty(tableClass) ? "" : $" class=\"{tableClass}\"")}>";
            html += $"<tr{(string.IsNullOrEmpty(rowClass) ? "" : $" class=\"{rowClass}\"")}>";
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                html += $"<td{(string.IsNullOrEmpty(cellClass) ? "" : $" class=\"{cellClass}\"")}>{dataTable.Columns[i].ColumnName}</td>";
            }
            html += "</tr>";
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                html += $"<tr{(string.IsNullOrEmpty(rowClass) ? "" : $" class=\"{rowClass}\"")}>";
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    html += $"<td{(string.IsNullOrEmpty(cellClass) ? "" : $" class=\"{cellClass}\"")}>{dataTable.Rows[i][j].ToString()}</td>";
                }
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
    }
}
