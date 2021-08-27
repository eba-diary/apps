xquery version "3.1";

(:~
 : This is the place to import your own XQuery modules for either:
 :
 : 1. custom API request handling functions
 : 2. custom templating functions to be called from one of the HTML templates
 :)
module namespace api="http://teipublisher.com/api/custom";

(: Add your own module imports here :)
import module namespace rutil="http://exist-db.org/xquery/router/util";
import module namespace app="teipublisher.com/app" at "app.xql";
import module namespace githubxq="http://exist-db.org/lib/githubxq";

(:~
 : Keep this. This function does the actual lookup in the imported modules.
 :)
declare function api:lookup($name as xs:string, $arity as xs:integer) {
    try {
        function-lookup(xs:QName($name), $arity)
    } catch * {
        ()
    }
};

declare function api:git-sync($request as map(*)) {
    let $data := request:get-data()
    return
        githubxq:execute-webhook($data,
            '/db/apps', 
            'https://github.com/eba-diary/apps',
            'main',
            doc('../access-config.xml')//private-key/text(),
            '')
};
