/******************************************************************************************
 * Javascript methods for the Admin Manage Users-related pages
 ******************************************************************************************/

data.ManageUsers = {

    // this is an example of how to scope a javascript variable to a page namespace
    userTable: {},

    Init: function () {

        // NOTE:  To switch this grid to client-side processing, comment-out the serverSide: true and switch the
        // ajax url to "/Admin/GetAllUserInfoForGrid".
        data.ManageUsers.userTable = $("#userTable").DataTable({
            autoWidth: false,
            serverSide: true,
            processing: true,
            paging: false,
            ajax: {
                url: "/Admin/GetUserInfoForGrid",
                type: "POST"
            },
            columns: [
                        { data: "AssociateId", width: "20%"},
                        { data: "Name" },
                        { data: "Ranking", width: "20%", type: "num" },
                        { data: "Id", render: data.ManageUsers.make_action_links, sortable: false, searchable: false, width: "60px" }
            ],
            order: [1, 'asc']
            //stateSave: true,
            //stateDuration: -1  // indicates session storage, not local storage
        });
        $("#userTable").dataTable().columnFilter({
            sPlaceHolder: "head:after",
            aoColumns: [
                    { type: "text" },
                    { type: "text" },
                    { type: "number-range" },
                    null
            ]
        });

        $("#userTable_wrapper .dt-toolbar").html($("#userToolbar"));

        $("#exportToExcel").click(function () {

            var table = data.ManageUsers.userTable;
            
            var params = Sentry.GetDataTableParamsForExport(table);

            var paramsEncoded = $.param(params);
            window.open('UserExport.xlsx?' + paramsEncoded, '_blank');
            //window.open('http://localhost:21944?' + paramsEncoded, '_blank');
        });

    },

    make_action_links: function (cellData, type, rowData) {
        return String.format($("#userTableRowIcons").html(), rowData.Id);
    },

    showConfirmDeleteModal: function (cell) {
        var data = data.ManageUsers.userTable.row($(cell).closest("tr")).data();

        Sentry.ShowModalCustom("Confirm delete",
            "<p>Please confirm that you want to delete the following user from Sentry.Data</p>" +
            "<p>" + data.Name + " (Associate Id " + data.AssociateId + ")</p>" +
            "<input type='hidden' id='removeUserId'/>",
            {
                Delete: {
                    label: "Delete",
                    className: "btn-danger",
                    callback: function () {
                        data.ManageUsers.confirmDelete(data.Id, data.Name);
                        return false;
                    }
                },
                Cancel: {
                    label: "Cancel",
                    className: "btn-link"
                }
            });
    },

    confirmDelete: function (id, name) {
        $.post("DeleteUser/" + id)
            .done(function () {
                data.ManageUsers.userTable.ajax.reload(null, false);
            })
            .fail(function () {
                alert("Error when deleting user " + name)
            }).always(function () {
                Sentry.HideAllModals();
            });
        return false;
    }

}
