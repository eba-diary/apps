import { Common } from "../../Common";
import { ShowModalWithSpinner, ShowModalCustom } from '@sentry-insurance/InternalFrontendTemplate/dist/Sentry.Common.js';
import SentryModal from '@sentry-insurance/InternalFrontendTemplate/dist/Interfaces/SentryModal'; 
import "bootstrap/js/dist/collapse";
import "bootstrap/js/dist/tooltip";
import "bootstrap/js/dist/popover"
import "bootstrap/js/dist/modal"

export namespace DatasetAuthorization {

    export async function InitForDataset(datasetId: number): Promise<void> {
        //try and axe the old modal
        $("#RequestAccessModal").remove();

        let modal: SentryModal = ShowModalWithSpinner("Request Dataset Access");
        
        $(modal).attr("id", "RequestAccessModal"); 

        let createRequestUrl: string = "/Dataset/AccessRequest/?datasetId=" + encodeURI(datasetId.toString());

        await $.get(createRequestUrl, function (e) {
            modal.ReplaceModalBody(e);
            $("#RequestAccessFormSection select").materialSelect();
            initRequestAccessWorkflow().catch(function (err) {
                console.log(err);
            });
        });

    }

    function validateInheritanceModal(): boolean {
        return ($("#Inheritance_BusinessReason").val() != '' && $("#Inheritance_SelectedApprover").val() != '')
    }

    function validateRemovePermissionModal(): boolean {
        return ($("#RemovePermission_BusinessReason").val() != '' && $("#RemovePermission_SelectedApprover").val() != '')
    }

    function permissionInheritanceSwitchInit(result: SecurityTicketSimple): void {
        let inheritance: string = $("#inheritanceSwitch").attr("value");
        if (inheritance == "Completed" && result.InheritanceActive) {
            $('#inheritanceSwitchInput').prop('checked', true);
            $("#addRemoveInheritanceMessage").text("Request Remove Inheritance");
            $("#Inheritance_IsAddingPermission").val('false');
        }
        else if (inheritance == 'Pending') {
            $("#inheritanceSwitch").html('<p>Inheritance change pending. See ticket ' + result.TicketId + '.</p>');
        }
        else {
            $("#addRemoveInheritanceMessage").text("Request Add Inheritance");
            $('#inheritanceSwitchInput').prop('checked', false);
            $("#Inheritance_IsAddingPermission").val('true');
        }
    }

    async function updateInheritanceStatus(): Promise<void> {
        await $.ajax({
            type: "GET",
            url: '/Dataset/Detail/' + $("#DatasetHeader").attr("value") + '/Permissions/GetLatestInheritanceTicket',
            success: function (result: SecurityTicketSimple) {
                $("#inheritanceSwitch").attr("value", result.TicketStatus);
                permissionInheritanceSwitchInit(result);
            }
        });
    }

    async function initRequestAccessWorkflow(): Promise<void> {
        let consumeDatasetGroupName = $("#consumeDatasetGroupName").text();
        let producerDatasetGroupName = $("#producerDatasetGroupName").text();
        let consumeAssetGroupName = $("#consumeAssetGroupName").text();
        let producerAssetGroupName = $("#producerAssetGroupName").text();

        $("#InheritanceWarningWrapper").tooltip({
            placement: 'bottom'
        });

        addRequestAccessBreadcrumb("Access To", "#RequestAccessToSection")
        $("#RequestAccessToDatasetBtn").on('click', function (e) {
            let datasetName: string = $("#RequestAccessDatasetName").text();
            $("#RequestAccess_Scope").val('0')
            editActiveRequestAccessBreadcrumb(datasetName);
            $("#RequestAccessConsumeEntitlement").text(consumeDatasetGroupName);
            $("#RequestAccessManageEntitlement").text(producerDatasetGroupName);
            onAccessToSelection(e);
        });
        $("#RequestAccessToAssetBtn").on ('click', function (e) {
            $("#RequestAccess_Scope").val('1')
            let assetName: string = $("#RequestAccessAssetName").text();

            editActiveRequestAccessBreadcrumb(assetName);
            $("#RequestAccessConsumeEntitlement").text(consumeAssetGroupName);
            $("#RequestAccessManageEntitlement").text(producerAssetGroupName);
            onAccessToSelection(e);
        });
        $("#RequestAccessTypeConsumeBtn").on('click', function (e) {
            editActiveRequestAccessBreadcrumb("Consumer");
            requestAccessCleanActiveBreadcrumb();
            addRequestAccessBreadcrumb("Consumer Type", "#RequestAccessConsumerTypeSection");
            $("#RequestAccessTypeSection").addClass("d-none");
            $("#RequestAccessConsumerTypeSection").removeClass("d-none");
        });
        $("#RequestAccessTypeManageBtn").on('click', function (e) {
            editActiveRequestAccessBreadcrumb("Producer");
            requestAccessCleanActiveBreadcrumb();
            addRequestAccessBreadcrumb("Producer Request", "#RequestAccessManageTypeSection");
            $("#RequestAccessTypeSection").addClass("d-none");
            $("#RequestAccessManageTypeSection").removeClass("d-none");
        });
        $("#RequestAccessConsumeSnowflakeBtn").on('click', function (e) {
            editActiveRequestAccessBreadcrumb("Snowflake Account");
            requestAccessCleanActiveBreadcrumb();
            addRequestAccessBreadcrumb("Create Request", "#RequestAccessFormSection");
            $("#RequestAccessConsumerTypeSection").addClass("d-none");
            $("#RequestAccessFormSection").removeClass("d-none");
            setupFormSnowflakeAccount();
        });
        $("#RequestAccessConsumeAwsBtn").on('click', function (e) {
            editActiveRequestAccessBreadcrumb("AWS IAM");
            requestAccessCleanActiveBreadcrumb();
            addRequestAccessBreadcrumb("Create Request", "#RequestAccessFormSection");
            $("#RequestAccessConsumerTypeSection").addClass("d-none");
            $("#RequestAccessFormSection").removeClass("d-none");
            setupFormAwsIam();
        });
        $("#RequestAccess_SelectedApprover").materialSelect();
        $("#RequestAccessSubmit").on('click', function () {
            if (validateRequestAccessModal()) {
                $("#RequestAccessLoading").removeClass('d-none');
                $("#RequestAccessBody").addClass('d-none');
                $.ajax({
                    type: 'POST',
                    data: $("#AccessRequestForm").serialize(),
                    url: '/Dataset/SubmitAccessRequestCLA3723',
                    success: function (data) {
                        $("#RequestAccessLoading").addClass('d-none');
                        $("#RequestAccessBody").removeClass('d-none');
                        $("#RequestAccessBody").html(data);
                    }
                }).catch(function (err) {
                    console.log(err);
                });
            }
            else {
                $("#AccessRequestValidationMessage").removeClass("d-none");
            }
        });
        $("#RequestAccessManageCopyBtn").on('click', function () {
            Common.copyTextToClipboard($("#RequestAccessManageEntitlement").text()).catch(function (err) {
                console.log(err);
            });
        });
        $("#RequestAccessConsumeCopyBtn").on('click', function () {
            Common.copyTextToClipboard($("#RequestAccessConsumeEntitlement").text()).catch(function (err) {
                console.log(err);
            });;
        });
    }

    export function ManagePermissionsInit(): void {
        manageInheritanceInit();
        removePermissionModalInit();
        $("#RequestAccessButton").off('click').on('click', function (e) {
            e.preventDefault();
            InitForDataset($(this).data("id")).catch(function (err) {
                console.log(err);
            });
        });
    }

    function manageInheritanceInit(): void {
        $("#DSCPermissions").DataTable({
            orderCellsTop: true,
            language: {
                emptyTable: 'No Permissions Found'
            },
            columns: [
                { data: "Scope", className: "", orderable: true },
                { data: "Identity", className: "", orderable: true },
                { data: "Permission", className: "", orderable: true },
                { data: "Status", className: "", orderable: true },
                { data: "Code", className: "", orderable: true },
                { data: "ApprovalTicket", className: "", orderable: true },
                { data: "ExternalTicket", className: "d-none", orderable: true },
                { data: "Actions", className: "", orderable: false }
            ],
            order: [1, 'asc']
        });

        $("#SnowflakePermissions").DataTable({
            orderCellsTop: true,
            language: {
                emptyTable: 'No Permissions Found'
            },
            columns: [
                { data: "Scope", className: "", orderable: true },
                { data: "Identity", className: "", orderable: true },
                { data: "Permission", className: "", orderable: true },
                { data: "Status", className: "", orderable: true },
                { data: "Code", className: "", orderable: true },
                { data: "ApprovalTicket", className: "", orderable: true },
                { data: "ExternalTicket", className: "", orderable: true },
                { data: "Actions", className: "", orderable: false }
            ],
            order: [1, 'asc']
        });

        $("#S3Permissions").DataTable({
            orderCellsTop: true,
            language: {
                emptyTable: 'No Permissions Found'
            },
            columns: [
                { data: "Scope", className: "", orderable: true },
                { data: "Identity", className: "", orderable: true },
                { data: "Permission", className: "", orderable: true },
                { data: "Status", className: "", orderable: true },
                { data: "Code", className: "", orderable: true },
                { data: "ApprovalTicket", className: "", orderable: true },
                { data: "JiraTicket", className: "", orderable: true },
                { data: "Actions", className: "", orderable: false },
            ],
            order: [1, 'asc']
        });


        $("#Inheritance_SelectedApprover").materialSelect();
        $("#inheritanceSwitch label").on('click', function () {
            $("#inheritanceModal").modal('show');
        });
        updateInheritanceStatus().catch(function (err) {
            console.log(err);
        });
        //Event to refresh inheritance switch on modal close
        $("#inheritanceModal").on('hide.bs.modal', function () {
            updateInheritanceStatus().catch(function (err) {
                console.log(err);
            });
        });
        $("#inheritanceModalSubmit").on('click', function () {
            if (validateInheritanceModal()) {
                $("#InheritanceLoading").removeClass('d-none');
                $("#InheritanceModalBody").addClass('d-none');
                $("#InheritanceModalFooter").addClass('d-none');
                $.ajax({
                    type: 'POST',
                    data: $("#InheritanceRequestForm").serialize(),
                    url: '/Dataset/SubmitInheritanceRequest',
                    success: function (data) {
                        //handle result data
                        $("#inheritanceModal").modal('hide');
                        $("#InheritanceLoading").addClass('d-none');
                        $("#InheritanceModalBody").removeClass('d-none');
                        $("#InheritanceModalFooter").removeClass('d-none');
                        //show the user if their request was submitted successfully
                        ShowModalCustom("", data);
                    }
                }).catch(function (err) {
                    console.log(err);
                });
            }
            else {
                $("#inheritanceValidationMessage").removeClass("d-none");
            }
        });

    }

    function removePermissionModalInit(): void {
        $(".removePermissionIcon").on('click', function (e) {
            let cells = $(e.target).parent().parent().children();
            let scope: string = $(cells[0]).text();
            let identity: string = $(cells[1]).text();
            let permission: string = $(cells[2]).text();
            let code: string = $(cells[4]).text();
            let ticketId: string = cells.parent().attr("id");
            removePermissionModalOnOpen(scope, identity, permission, code, ticketId);
            $("#removePermissionModal").modal('show');
        });
        $("#removePermissionModal").on('hide.bs.modal', function () {
            removePermissionModalOnClose();
        });
        $("#removePermissionModalSubmit").on('click', function () {
            if (validateRemovePermissionModal()) {
                $("#RemovePermissionLoading").removeClass("d-none");
                $("#RemovePermissionModalForm").addClass("d-none");
                $("#RemovePermissionModalButtons").addClass("d-none");

                $.ajax({
                    type: 'POST',
                    data: $("#RemovePermissionRequestForm").serialize(),
                    url: '/Dataset/SubmitRemovePermissionRequest',
                    success: function (data) {
                        $("#RemovePermissionLoading").addClass("d-none");
                        $("#RemovePermissionRequestResult").html(data);
                        $("#RemovePermissionRequestResult").removeClass("d-none");
                    }
                }).catch(function (err) {
                    console.log(err);
                });
            }
            else {
                $("#removePermissionValidationMessage").removeClass("d-none");
            }
        });
    }

    function removePermissionModalOnClose(): void {
        $("#RemovePermission_Identity").val("");
        $("#RemovePermission_Scope").val("");
        $("#RemovePermission_Permission").val("");
        $("#RemovePermission_BusinessReason").val("");
        $("#RemovePermission_TicketId").val("");
        $("#RemovePermission_Code").val("");

        $("#RemovePermission_SelectedApprover").materialSelect({ destroy: true });

        $("#identityLabel").removeClass("active");
        $("#scopeLabel").removeClass("active");
        $("#permissionLabel").removeClass("active");
        $("#RemovePermissionRequestResult").html("");
        $("#RemovePermissionRequestResult").addClass("d-none");
        $("#removePermissionValidationMessage").addClass("d-none");
    }

    function removePermissionModalOnOpen(scope: string, identity: string, permission: string, code: string, ticketId: string): void {
        $("#RemovePermission_Identity").val(identity);
        $("#RemovePermission_Scope").val(scope);
        $("#RemovePermission_Permission").val(permission);
        $("#RemovePermission_TicketId").val(ticketId);
        $("#RemovePermission_Code").val(code);
        $("#RemovePermission_SelectedApprover").materialSelect();
        $("#identityLabel").addClass("active");
        $("#scopeLabel").addClass("active");
        $("#permissionLabel").addClass("active");
        $("#RemovePermissionModalForm").removeClass("d-none");
        $("#RemovePermissionModalButtons").removeClass("d-none");


        if (scope == $("#ManagePermissionDatasetName").text()) {
            $("#RemovePermissionScopeContainer").addClass("d-none");
        }
        else {
            $("#RemovePermissionScopeContainer").removeClass("d-none");
        }
    }

    function validateRequestAccessModal(): boolean {
        let valid;
        valid = $("#RequestAccess_BusinessReason").val() != '' && $("#RequestAccess_SelectedApprover").val() != ''
        if ($("#RequestAccess_Type").val() == "1") {
            valid = valid && requestAccessValidateAwsArnIam();
        }
        if ($("#RequestAccess_Type").val() == "4") {
            valid = valid && requestAccessValidateSnowflakeAccount();
        }
        return valid;
    }

    function onAccessToSelection(event): void { 
        requestAccessCleanActiveBreadcrumb();
        addRequestAccessBreadcrumb("Access Type", "#RequestAccessTypeSection");
        $("#RequestAccessToSection").addClass("d-none");
        $("#RequestAccessTypeSection").removeClass("d-none");
    }

    function buildBreadcrumbReturnToStepHandler(element: JQuery<HTMLElement>): void {
        element.on('click', function () {
            let jumpBackTo = element.attr("value");
            $('#AccessRequestForm div.requestAccessStage:not(.d-none)').addClass('d-none');
            $(jumpBackTo).removeClass('d-none');
            element.nextAll().remove();
            element.children(":first").text(element.children(":first").attr("value"));
            element.addClass('active');
            if (jumpBackTo != "#RequestAccessFormSection") {
                requestAccessHideSaveChanges();
            }
        });
    }

    function requestAccessCleanActiveBreadcrumb(): void {
        $("#RequestAccessBreadcrumb li").removeClass("active");
    }

    function addRequestAccessBreadcrumb(breadCrumbText, createdFrom): void {
        $("#RequestAccessBreadcrumb").append('<li class="breadcrumb-item active" value="' + createdFrom + '"><a href="#" value="' + breadCrumbText + '">' + breadCrumbText + '</a></li>');
        buildBreadcrumbReturnToStepHandler(requestAccessGetActiveBreadcrumb());
    }

    function editActiveRequestAccessBreadcrumb(breadCrumbText): void {
        $("#RequestAccessBreadcrumb li.active a").text(breadCrumbText);
    }

    function requestAccessGetActiveBreadcrumb(): JQuery<HTMLElement> {
        return $("#RequestAccessBreadcrumb li.active");
    }

    function setupFormAwsIam(): void {
        $("#SnowflakeAccountForm").addClass("d-none");
        $("#AwsArnForm").removeClass("d-none");
        $("#RequestAccess_Type").val("1")
        requestAccessShowSaveChanges();
    }

    function setupFormSnowflakeAccount(): void {
        $("#AwsArnForm").addClass("d-none");
        $("#SnowflakeAccountForm").removeClass("d-none");
        $("#RequestAccess_Type").val("4")
        requestAccessShowSaveChanges();
    }

    function requestAccessShowSaveChanges(): void {
        $("#RequestAccessSubmit").removeClass("d-none");
    }

    function requestAccessHideSaveChanges(): void {
        $("#RequestAccessSubmit").addClass("d-none");
    }

    function requestAccessValidateAwsArnIam(): boolean {
        let pattern: RegExp = /^arn:aws:iam::\d{12}:role\/+./;
        let valid: boolean = pattern.test($("#RequestAccess_AwsArn").val().toString());
        if (valid) {
            $("#AccessRequestAwsArnValidationMessage").addClass("d-none");
            return true;
        }
        else {
            $("#AccessRequestAwsArnValidationMessage").removeClass("d-none");
            return false;
        }
    }

    function requestAccessValidateSnowflakeAccount(): boolean {
        let valid = $("#RequestAccess_SnowflakeAccount").val() != '';
        valid = valid && $("#RequestAccess_SnowflakeAccount").val().toString().length < 255;
        if (valid) {
            $("#AccessRequestSnowflakeValidationMessage").addClass("d-none");
            return true;
        }
        else {
            $("#AccessRequestSnowflakeValidationMessage").removeClass("d-none");
            return false;
        }
    }


    interface SecurityTicketSimple {
        TicketId: string,
        TicketStatus: string,
        InheritanceActive: boolean
    }
}