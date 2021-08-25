xquery version "3.1";

(: Webhook endpoint for EBA Diaries Web Application 
 : XQuery endpoint to respond to Github webhook requests.
 :
 : Secret can be stored as environmental variable.
 : Will need to be run with administrative privileges, suggest creating a git user with privileges only to relevant app.
 :
 : @Notes 
 : This module is for the PRODUCTION server and picks up calls from refs/heads/master
 : This version uses eXistdb's native JSON parser elminating the need for the xqjson library
 :
 : @author Winona Salesky
 : @version 2.0
 : 
 :)

import module namespace githubxq="http://exist-db.org/lib/githubxq";
 
import module namespace xmldb="http://exist-db.org/xquery/xmldb";
import module namespace templates="http://exist-db.org/xquery/templates" ;

import module namespace crypto="http://expath.org/ns/crypto";
import module namespace http="http://expath.org/ns/http-client";
declare namespace tei = "http://www.tei-c.org/ns/1.0";

declare option exist:serialize "method=xml media-type=text/xml indent=yes";

(: Access git-api configuration file :)
declare variable $githubxq:git-config := if(doc('../access-config.xml')) then doc('../access-config.xml') else <response status="fail"><message>Load config.xml file please.</message></response>;

(: Private key for authentication :)
declare variable $githubxq:private-key := if($githubxq:git-config//private-key-variable != '') then
                                    environment-variable($githubxq:git-config//private-key-variable/text())
                              else $githubxq:git-config//private-key/text();

declare variable $githubxq:gitToken := if($githubxq:git-config//gitToken-variable != '') then 
                                    environment-variable($githubxq:git-config//gitToken-variable/text())
                              else $githubxq:git-config//gitToken/text();

(: eXist db collection location :)
declare variable $githubxq:exist-collection := $githubxq:git-config//exist-collection/text();

(: Github repository :)
declare variable $githubxq:repo-name := $githubxq:git-config//repo-name/text();

(:~
 : Recursively creates new collections if necessary
 : @param $uri url to resource being added to db 
 :)
declare function githubxq:create-collections($uri as xs:string){
let $collection-uri := substring($uri,1)
for $collections in tokenize($collection-uri, '/')
let $current-path := concat('/',substring-before($collection-uri, $collections),$collections)
let $parent-collection := substring($current-path, 1, string-length($current-path) - string-length(tokenize($current-path, '/')[last()]))
return 
    if (xmldb:collection-available($current-path)) then ()
    else xmldb:create-collection($parent-collection, $collections)
};

declare function githubxq:get-file-data($file-name, $contents-url){
let $url := concat($contents-url,'/',$file-name)         
let $raw-url := concat(replace(replace($contents-url,'https://api.github.com/repos/','https://raw.githubusercontent.com/'),'/contents','/master'),$file-name)            
return 
        http:send-request(<http:request http-version="1.1" href="{xs:anyURI($raw-url)}" method="get">
                            {if($githubxq:gitToken != '') then
                                <http:header name="Authorization" value="{concat('token ',$githubxq:gitToken)}"/>
                            else() }
                            <http:header name="Connection" value="close"/>
                        </http:request>)[2]
};

(:~
 : Updates files in eXistdb with github data 
 : @param $commits serialized json data
 : @param $contents-url string pointing to resource on github
:)
declare function githubxq:do-update($commits as xs:string*, $contents-url as xs:string?){
    for $file in $commits
    let $file-name := tokenize($file,'/')[last()]
    let $file-data := 
        if(contains($file-name,'.xar')) then ()
        else githubxq:get-file-data($file,$contents-url)
    let $resource-path := substring-before(replace($file,$githubxq:repo-name,''),$file-name)
    let $exist-collection-url := xs:anyURI(replace(concat($githubxq:exist-collection,'/',$resource-path),'/$',''))        
    return 
        try {
             if(contains($file-name,'.xar')) then ()
             else if(xmldb:collection-available($exist-collection-url)) then 
                <response status="okay">
                    <message>{xmldb:store($exist-collection-url, xmldb:encode-uri($file-name), $file-data)}</message>
                </response>
             else
                <response status="okay">
                    {(githubxq:create-collections($exist-collection-url),xmldb:store($exist-collection-url, xmldb:encode-uri($file-name), $file-data))}
               </response>  
        } catch * {
        (response:set-status-code( 500 ),
            <response status="fail">
                <message>Failed to update resource {xs:anyURI(concat($exist-collection-url,'/',$file-name))}: {concat($err:code, ": ", $err:description)}</message>
            </response>)
        }
};

(:~
 : Adds new files to eXistdb. 
 : Pulls data from github repository, parses file information and passes data to xmldb:store
 : @param $commits serilized json data
 : @param $contents-url string pointing to resource on github
 : NOTE permission changes could happen in a db trigger after files are created
:)
declare function githubxq:do-add($commits as xs:string*, $contents-url as xs:string?){
    for $file in $commits
    let $file-name := tokenize($file,'/')[last()]
    let $file-data := 
        if(contains($file-name,'.xar')) then ()
        else githubxq:get-file-data($file,$contents-url)
    let $resource-path := substring-before(replace($file,$githubxq:repo-name,''),$file-name)
    let $exist-collection-url := xs:anyURI(replace(concat($githubxq:exist-collection,'/',$resource-path),'/$',''))
    return
        try {
             if(contains($file-name,'.xar')) then ()
             else if(xmldb:collection-available($exist-collection-url)) then 
                <response status="okay">
                    <message>{xmldb:store($exist-collection-url, xmldb:encode-uri($file-name), xs:base64Binary($file-data))}</message>
                </response>
             else
                <response status="okay">
                 {(githubxq:create-collections($exist-collection-url),xmldb:store($exist-collection-url, xmldb:encode-uri($file-name), xs:base64Binary($file-data)))}
               </response>  
               } catch * {
               (response:set-status-code( 500 ),
            <response status="fail">
                <message>Failed to add resource {xs:anyURI(concat($exist-collection-url,$file-name))}: {concat($err:code, ": ", $err:description)}</message>
            </response>)
        }
};

(:~
 : Removes files from the database uses xmldb:remove
 : Pulls data from github repository, parses file information and passes data to xmldb:store
 : @param $commits serilized json data
 : @param $contents-url string pointing to resource on github
:)
declare function githubxq:do-delete($commits as xs:string*, $contents-url as xs:string?){
    for $file in $commits
    let $file-name := tokenize($file,'/')[last()]
    let $resource-path := substring-before(replace($file,$githubxq:repo-name,''),$file-name)
    let $exist-collection-url := xs:anyURI(replace(concat($githubxq:exist-collection,'/',$resource-path),'/$',''))
    return
        if(contains($file-name,'.xar')) then ()
        else 
            try {
                <response status="okay">
                    <message>{xmldb:remove($exist-collection-url, $file-name)}</message>
                </response>
            } catch * {
            (response:set-status-code( 500 ),
                <response status="fail">
                    <message>Failed to remove resource {xs:anyURI(concat($exist-collection-url,$file-name))}: {concat($err:code, ": ", $err:description)}</message>
                </response>)
            }
   
};

(:~
 : Parse request data and pass to appropriate local functions
 : @param $json-data github response serializing as xml xqjson:parse-json()  
 :)
declare function githubxq:parse-request($json-data as item()*){
let $contents-url := substring-before($json-data?repository?contents_url,'{')
return 
    try {
      (
            githubxq:do-update(distinct-values($json-data?commits?*?modified?*), $contents-url),  
            githubxq:do-add(distinct-values($json-data?commits?*?added?*), $contents-url),
            githubxq:do-delete(distinct-values($json-data?commits?*?removed?*), $contents-url))   
    } catch * {
    (response:set-status-code( 500 ),
        <response status="fail">
            <message>Failed to parse JSON {concat($err:code, ": ", $err:description)}</message>
        </response>)
    }
};

(:~
 : Validate github post request.
 : Check user agent and github event, only accept push events from master branch.
 : Check git hook secret against secret stored in environmental variable
 : @param $GIT_TOKEN environment variable storing github secret
 :)

declare function githubxq:execute-webhook($post-data){
if(not(empty($post-data))) then 
    let $payload := util:base64-decode($post-data)
    let $json-data := parse-json($payload)
    let $branch := if($githubxq:git-config//github-branch/text() != '') then $githubxq:git-config//github-branch/text() else 'refs/heads/master'
    return
        if($json-data?ref[. = $branch]) then 
             try {
                if(matches(request:get-header('User-Agent'), '^GitHub-Hookshot/')) then
                    if(request:get-header('X-GitHub-Event') = 'push') then 
                        let $signiture := request:get-header('X-Hub-Signature')
                        let $expected-result := <expected-result>{request:get-header('X-Hub-Signature')}</expected-result>
                        let $actual-result :=
                            <actual-result>
                                {crypto:hmac($payload, string($githubxq:private-key), "HMAC-SHA-1", "hex")}
                            </actual-result>
                        let $condition := contains(normalize-space($expected-result/text()),normalize-space($actual-result/text()))                	
                        return
                            if ($condition) then 
                                githubxq:parse-request($json-data)
            			    else 
            			     (response:set-status-code( 401 ),<response status="fail"><message>Invalid secret. </message></response>)
                    else (response:set-status-code( 401 ),<response status="fail"><message>Invalid trigger.</message></response>)
                else (response:set-status-code( 401 ),<response status="fail"><message>This is not a GitHub request.</message></response>)    
            } catch * {
                (response:set-status-code( 401 ),
                <response status="fail">
                    <message>Unacceptable headers {concat($err:code, ": ", $err:description)}</message>
                </response>)
            }
        else (response:set-status-code( 401 ),<response status="fail"><message>Not from the master branch.</message></response>)
else    
            (response:set-status-code( 401 ),
            <response status="fail">
                <message>No post data recieved</message>
            </response>)   
};

declare function githubxq:git-sync(){
    let $post-data := request:get-data()
    return githubxq:execute-webhook($post-data)
};

return
