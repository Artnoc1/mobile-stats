using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileStats.Formatting
{
    static class TableFormatter
    {
        public static TableFormatter<TRow> WithColumns<TRow>(
            params (string Title, Func<TRow, object> Selector, TextAlignMode TextAlignment)[] columns)
        {
            return WithColumns(columns.AsEnumerable());
        }
        public static TableFormatter<TRow> WithColumns<TRow>(
            IEnumerable<(string Title, Func<TRow, object> Selector, TextAlignMode TextAlignment)> columns)
        {
            return new TableFormatter<TRow>(columns.Select(c => (TableFormatter<TRow>.ColumnSelector)c));
        }
    }
    
    class TableFormatter<TRow>
    {
        private readonly ColumnSelector[] columns;

        public class ColumnSelector
        {
            public string Title { get; }
            public Func<TRow, object> Selector { get; }
            public TextAlignMode TextAlignment { get; }
            
            public ColumnSelector(string title, Func<TRow, object> selector, TextAlignMode textAlignment)
            {
                Title = title;
                Selector = selector;
                TextAlignment = textAlignment;
            }
            
            public static implicit operator ColumnSelector(
                (string Title, Func<TRow, object> Selector, TextAlignMode TextAlignment) column
                )
                => new ColumnSelector(column.Title, column.Selector, column.TextAlignment);
        }
        
        public TableFormatter(IEnumerable<ColumnSelector> columns)
            : this(columns.ToArray())
        {
        }
        
        public TableFormatter(params ColumnSelector[] columns)
        {
            this.columns = columns;
        }

        public string Format(IEnumerable<TRow> rows)
        {
            return "```\n" + string.Join("\n", FormatLines(rows)) + "\n```\n";
        }
        
        public List<string> FormatLines(IEnumerable<TRow> rows)
        {
            var rowsAsCells = getTableCells(rows);

            var columnWidths = getColumnWidths(rowsAsCells);

            var lines = writeCellsToLines(rowsAsCells, columnWidths);

            return lines;
        }

        private List<List<string>> getTableCells(IEnumerable<TRow> rows)
        {
            var cells = new List<List<string>>();

            cells.Add(columns.Select(c => c.Title).ToList());
            cells.AddRange(rows.Select(row => columns.Select(c => c.Selector(row).ToString()).ToList()));

            return cells;
        }

        private List<int> getColumnWidths(List<List<string>> rowsAsCells)
        {
            return Enumerable.Range(0, columns.Length)
                .Select(maxRowWidthOfColumn)
                .ToList();

            int maxRowWidthOfColumn(int i) => rowsAsCells.Max(row => row[i].Length);
        }

        private List<string> writeCellsToLines(List<List<string>> rowsAsCells, List<int> columnWidths)
        {
            return rowsAsCells.Select(row => writeRow(row, columnWidths)).ToList();
        }

        private string writeRow(List<string> row, List<int> columnWidths)
        {
            var builder = new StringBuilder();

            foreach (var (cell, column) in row.Select((c, i) => (c, i)))
            {
                builder.Append("| ");

                var paddedCell = columns[column].TextAlignment == TextAlignMode.Left
                    ? cell.PadRight(columnWidths[column])
                    : cell.PadLeft(columnWidths[column]);

                builder.Append(paddedCell);
                
                builder.Append(' ');
            }
            builder.Append("|");
            return builder.ToString();
        }
    }
}