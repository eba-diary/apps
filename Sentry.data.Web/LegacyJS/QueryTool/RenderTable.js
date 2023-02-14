
function renderTable(input, queryTool, push) {

    var filters;

    if (!queryTool) {
        filters = input.split("\n");
    } else {
        filters = input;
    }


    if (filters[0].startsWith("+-")) {

        if (table) {
            table.destroy();
        }

        //Column information
        var columns = filters[1].split('|');
        var parsedColumns = [];
        var nullColumns = [];

        $('#tableColumnIDs').empty();
        $('#tableFirstRow').empty();
        $('#datasetRowTable tbody').empty();

        for (var i = 1; i < columns.length; i++) {
            if (columns[i]) {
                nullColumns.push(null);
                parsedColumns.push({ 'title': columns[i].trim(), 'width': "auto" });
                $('#tableColumnIDs').append("<th>" + columns[i].trim() + "</th>");
                $('#tableFirstRow').append("<td></td>");
            }
        }

        var parsedRows = [];

        //Data for Each Row in the Result Set
        for (var i = 3; i < filters.length; i++) {

            if (!filters[i].startsWith("+-")) {


                var cells = filters[i].split('|');
                var parsedCells = [];
                cells.splice(0, 1);
                cells.splice(cells.length, 1);
                for (var j = 0; j < cells.length - 1; j++) {
                    parsedCells.push(cells[j].trim());
                }

                if (parsedCells.length >= 1) {
                    parsedRows.push(parsedCells);
                }
            }
        }
        if (!queryTool) {
            if (!push) {
                table = $('#datasetRowTable').DataTable({
                    "scrollX": true,
                    data: parsedRows,
                    columns: parsedColumns
                });
            } else {
                table = $('#datasetRowTable').DataTable({ "scrollX": true }).rows.add(parsedRows).draw();
                $($('#datasetRowTable_info').children()[2]).text($('#rowCount').text());
            }
        } else {
            table = $('#datasetRowTable').DataTable({
                "scrollX": true,
                data: parsedRows,
                columns: parsedColumns
            });
        }


        $(".dataTables_filter").parent().addClass("text-right");
    }
}