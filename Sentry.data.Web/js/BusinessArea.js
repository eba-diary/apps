data.BusinessArea =
{

   PersonalLinesInit: function () {


        


        
        //$.get(getAustin, function (e) {

        //    console.log("method 1");
        //    console.log(e);
        //    console.log(e.CriticalNotifications[0].Message);
        //    console.log(e.StandardNotifications[0].Message);
        //});  


        //attempt #2
        $.get("/Notification/GetAustin", this.DisplayNotifications);

       
        //toastr.options = {
        //    "closeButton": false,
        //    "debug": false,
        //    "newestOnTop": false,
        //    "progressBar": false,
        //    "positionClass": "toast-top-right",
        //    "preventDuplicates": false,
        //    "onclick": null,
        //    "showDuration": "300",
        //    "hideDuration": "1000",
        //    "timeOut": "5000",
        //    "extendedTimeOut": "1000",
        //    "showEasing": "swing",
        //    "hideEasing": "linear",
        //    "showMethod": "fadeIn",
        //    "hideMethod": "fadeOut"
        //}
        //toastr["error"]("Message Test1", "Austin");
        //toastr["error"]("Message Test2", "Austin2");

        
    },


    DisplayNotifications: function (e)
    {
        
        console.log("LoadNotifications:  START");
        console.log(e);
        //console.log(e.CriticalNotifications[0].Message);
        //console.log(e.StandardNotifications[0].Message);


        toastr.options = {
            "closeButton": false,
            "debug": false,
            "newestOnTop": false,
            "progressBar": false,
            "positionClass": "toast-top-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "3000",
            "hideDuration": "10",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }


        
        for (i = 0; i < e.CriticalNotifications.length; i++)
        {
            toastr["error"](e.CriticalNotifications[i].Message, e.CriticalNotifications[i].Title);
        }

        for (i = 0; i < e.StandardNotifications.length; i++)
        {
            if (e.StandardNotifications[i].MessageSeverity == "Warning")
                toastr["warning"](e.StandardNotifications[i].Message, e.StandardNotifications[i].Title);
            else 
                toastr["info"](e.StandardNotifications[i].Message, e.StandardNotifications[i].Title);
            


            
        }



    },

    

    
    


}   