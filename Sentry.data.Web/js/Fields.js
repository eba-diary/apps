data.Fields = {
    Init: function () {
        
    },

    //This function is based on adding child as last item within schemaRowContainer, therefore,
    //  it will traverse schemaRowContainer to file the last child and return it's row position.
    //If this schemaRowContainer does not contain children, it will return the row position
    //  of the schemaRowContainer passed in
    GetLastRowPositionOfSchemaRowContainer: function (e) {
        //Determine if schemaRowContainer has children
        // Get children container
        var childContainer = $(e).children('.childrenContainer').last();

        //Determine if childrenContainer has children or if this we are adding the first child
        var childrenCnt = $(childContainer).children('.schemaRowContainer').length;

        //If there are no children, return rowPosition from incomming schemaRowObject
        if (childrenCnt === undefined || childrenCnt === 0) {
            return $(e).find('.schemaRow').find('.rowDetails').data('id');
        }
        else if (childrenCnt > 0) {
            return data.Fields.GetLastRowPositionOfSchemaRowContainer($(childContainer).children('.schemaRowContainer').last())
        }
    },

    IncrementAssociatedRowErrors: function (item, newIndex) {
        //find associated errors and save off
        var errs = vm.Errors().filter(f => f.DataObjectField_ID == item.DataObjectField_ID());

        //remove associated errors
        vm.Errors.remove(function (err) {
            return err.DataObjectField_ID == item.DataObjectField_ID();
        });

        //change the Id element of error record
        for (var i in errs) {
            errs[i].Id = (newIndex).toString();
        }

        //Push adjusted error records back onto vm.Errors
        ko.utils.arrayPushAll(vm.Errors, errs);
        vm.Errors.valueHasMutated();
    }
}