To use this WebAPI versioning package, do the following:

1. In your WebApiConfig.Register method, add the following code (be sure to interweave any existing routing and/or swagger configuration as appropriate)

            var constraintResolver = new DefaultInlineConstraintResolver();
            Sentry.WebAPI.Versioning.Register.ConstraintMap(constraintResolver);
            Sentry.WebAPI.Versioning.Register.ApiVersions(config, "1");   // Replace "1" with a different default API version if you desire
            config.MapHttpAttributeRoutes(constraintResolver);

            // Configure swagger
            config
               .EnableSwagger("{apiVersion}/swagger", c =>
               {
                   // The following line replaces the typical call to SingleApiVersion.  It auto-wires the multiple versions into the 
                   // swagger API descriptions
                   Sentry.WebAPI.Versioning.Register.SwaggerMultipleApiVersions(config, c, "Test API");  
               })
               .EnableSwaggerUi(c =>
               {
                   // The following is necessary to allow the user to choose from a multitude of swagger definitions in a drop-down,
                   // one definition per API version
                   c.EnableDiscoveryUrlSelector();
               });

2. Remove your existing SwaggerConfig file, if it exists.  It is important that you configure Swagger AFTER you register ApiVersions.  
   If you keep the existing SwaggerConfig file, Swagger may be configured before ApiVersions registration, because SwaggerConfig uses 
   WebActivatorEx.

3. Update existing RoutePrefix attributes on your various controllers to include the version in the URL, like this:

      [RoutePrefix("api/" + Sentry.WebAPI.Versioning.Constants.VERSION + "/SomeResourceName")]

  (this will result in a URL for your API like this:  /api/v1/SomeResourceName )

That's all you need to do to support a default version on your existing Web API.  When you are ready to make a breaking change and add a new version
to your API, you have a choice to make. You can either add additional methods to an existing controller, or create a whole new controller.  Here is how
to handle each scenario:


ADDING A NEW VERSION USING AN ADDITIONAL METHOD ON EXISTING CONTROLLER

1. On the "old" method that you are removing from the API, add the following attribute, including the version number that should no longer include the 
   method:

     [ApiVersionEnd("2")]

2. Create a "new" method (with a different name if necessary, such as Get_V2 to replace Get), and add the following attribute to indicate that this
   method should begin being a part of the API starting with the given version:

     [ApiVersionBegin("2")]

This will result in a new version automatically becoming a part of your api, living at a URL such as /api/v2/SomeResourceName.


ADDING A NEW VERSION USING A NEW CONTROLLER

1. On the "old" controller that you are removing from the API, add the following attribute, including the version number that should no longer include the 
   controller:

     [ApiVersionEnd("2")]

2. Create a "new" controller (if you want to use the same name for the controller, you can put this new controller in a new namespace, such as
   Sentry.MyApplication.Web.V2), and add the following attribute to the controller (in addition to the RoutePrefix attribute as noted above):

     [ApiVersionBegin("2")]

This will result in a new version automatically becoming a part of your api, living at a URL such as /api/v2/SomeResourceName.


HOW TO CHOOSE BETWEEN THE ABOVE TWO SCENARIOS

If you are tweaking the behavior of one or two methods on a controller, it is probably easier to just add new methods to the existing controller, 
and add the ApiVersionEnd attribute to the "old" method and the ApiVersionBegin attribute to the "new" method.

If you are making a larger change to your API, such as changing the shape of the data that are you returning from a GET or accepting on a POST/PUT, then
you'll probably find it easier to add a whole new controller that is a copy of the old controller, but with the new data types as input/output parameters.

In either case, be sure to use refactoring techniques as appropriate to avoid having duplicate copies of code with just minor variations.  For example,
both the old and new methods (or old and new controllers) could both call to a common set of code to do the common work, while only keeping the code that
varies between versions in the separate old/new methods/controllers.  Using design patterns such as Template Method or Strategy may be helpful.