﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClosedXML.Excel
{
    internal class XLColumns: IXLColumns
    {
        private Boolean entireWorksheet;
        private XLWorksheet worksheet;
        public XLColumns(XLWorksheet worksheet, Boolean entireWorksheet = false)
        {
            this.worksheet = worksheet;
            this.entireWorksheet = entireWorksheet;
            Style = worksheet.Style;
        }

        List<XLColumn> columns = new List<XLColumn>();
        public IEnumerator<IXLColumn> GetEnumerator()
        {
            return columns.ToList<IXLColumn>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IXLStylized Members

        private IXLStyle style;
        public IXLStyle Style
        {
            get
            {
                return style;
            }
            set
            {
                style = new XLStyle(this, value);
                //Styles.ForEach(s => s = new XLStyle(this, value));

                if (entireWorksheet)
                {
                    worksheet.Style = value;
                }
                else
                {
                    var maxRow = 0;
                    if (worksheet.Internals.RowsCollection.Count > 0)
                        maxRow = worksheet.Internals.RowsCollection.Keys.Max();
                    foreach (var col in columns)
                    {
                        col.Style = value;
                        foreach (var c in col.Worksheet.Internals.CellsCollection.Values.Where(c => c.Address.ColumnNumber == col.FirstAddressInSheet.ColumnNumber))
                        {
                            c.Style = value;
                        }

                        for (var ro = 1; ro <= maxRow; ro++)
                        {
                            worksheet.Cell(ro, col.ColumnNumber()).Style = value;
                        }
                    }
                }
            }
        }

        public IEnumerable<IXLStyle> Styles
        {
            get
            {
                UpdatingStyle = true;
                yield return style;
                if (entireWorksheet)
                {
                    yield return worksheet.Style;
                }
                else
                {
                    var maxRow = 0;
                    if (worksheet.Internals.RowsCollection.Count > 0)
                        maxRow = worksheet.Internals.RowsCollection.Keys.Max();
                    foreach (var col in columns)
                    {
                        yield return col.Style;
                        foreach (var c in col.Worksheet.Internals.CellsCollection.Values.Where(c => c.Address.ColumnNumber == col.FirstAddressInSheet.ColumnNumber))
                        {
                            yield return c.Style;
                        }

                        for (var ro = 1; ro <= maxRow; ro++)
                        {
                            yield return worksheet.Cell(ro, col.ColumnNumber()).Style;
                        }
                    }
                }
                UpdatingStyle = false;
            }
        }

        public Boolean UpdatingStyle { get; set; }

        #endregion

        public Double Width
        {
            set
            {
                columns.ForEach(c => c.Width = value);

                if (entireWorksheet)
                {
                    worksheet.ColumnWidth = value;
                    worksheet.Internals.ColumnsCollection.ForEach(c => c.Value.Width = value);
                }
            }
        }

        public void Delete()
        {
            if (entireWorksheet)
            {
                worksheet.Internals.ColumnsCollection.Clear();
                worksheet.Internals.CellsCollection.Clear();
            }
            else
            {
                var toDelete = new List<Int32>();
                foreach (var c in columns)
                    toDelete.Add(c.ColumnNumber());

                foreach(var c in toDelete.OrderByDescending(c=>c))
                    worksheet.Column(c).Delete();
            }
        }

        public void Add(XLColumn column)
        {
            columns.Add(column);
        }
    }
}
