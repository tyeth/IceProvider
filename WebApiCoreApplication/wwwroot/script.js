// console.log("IQS loaded");

var version = "7.1";

// defaults
var w = 638;
var h = 388;
var downloadlink = 0;
var flashlink = 0;
var timer = 0;

function iceVerify(url){
    if (typeof url === 'undefined') url = location.href;
    var con = IQS_container(),
        iframe = document.createElement("iframe");
    iframe.src = document.location.protocol+'//' + Location.host +'/membersonly/components/com_iceplayer/verify.php?url='+escape(url);
    iframe.style.width = w+'px';
    iframe.style.height= h+'px';
    iframe.style.border= "0";
    iframe.style.margin= "0";
    iframe.style.marginRight= "100%";
    iframe.style.position="relative";
    iframe.setAttribute("scrolling","no");
    iframe.setAttribute("id","iceVerify");
    iframe.style.setProperty("display","block","important");
    iframe.style.backgroundColor= "#333";
    con.appendChild(iframe);
    IQSToggleButton();
}

function hostLove(){
    var con = IQS_container(),
        box = document.createElement("div");
    box.style.width = '300px';
    box.style.height= '0px';
    box.style.border= "0";
    box.style.margin= "0";
    box.style.marginRight= "100%";
    box.style.position="absolute";
    box.style.left = (w+50)+"px";
    box.style.top = (h/2-110)+"px";
    box.style.textAlign = "center";
    box.setAttribute("id","hostLove");
    box.style.setProperty("display","block","important");
    box.innerHTML = "<h1 style='font-size:34px;line-height:40px;margin:0;padding:0;'>Enjoying this host? Give back by clicking an ad or paying for premium.</h1>";
    con.appendChild(box);
}

function iceEmbed(){
    var con = IQS_container(),
        iframe = document.createElement("iframe");
    iframe.src = document.location.protocol+'//www.icedivx.com/video.php?w='+w+'&h='+(h-18)+'&vurl='+escape(downloadlink)+'&flash='+flashlink+'&t='+timer+'&v='+version;
    iframe.style.width = w+'px';
    iframe.style.height= h+'px';
    iframe.style.border= "0";
    iframe.style.margin= "0";
    iframe.style.marginRight= "100%";
    iframe.style.position="relative";
    iframe.setAttribute("scrolling","no");
    iframe.setAttribute("id","iceQuickStream");
    iframe.setAttribute("allowfullscreen","true");
    iframe.style.setProperty("display","block","important");
    iframe.style.backgroundColor= "#333";
    con.appendChild(iframe);
    IQSToggleButton();
}

function IQSToggleButton(){
    var con = IQS_container(),
        box = document.createElement("div");
    box.style.width = '20px';
    box.style.height= '20px';
    box.style.border= "0";
    box.style.margin= "0";
    box.style.position="absolute";
    box.style.left = w+"px";
    box.style.top = "0";
    box.setAttribute("id","toggleIQS");
    box.setAttribute("title","Close ICE Quick Stream");
    box.style.setProperty("display","block","important");
    box.style.cursor= "pointer";
    box.innerHTML = "&times;";
    box.style.fontSize= "17px";
    box.style.fontWeight= "bold";
    box.style.color= "#777";
    box.style.backgroundColor= "#333";
    box.style.textAlign= "center";
    box.style.lineHeight= "20px";
    box.style.borderRadius= "0 3px 3px 0";
    box.style.fontFamily= "monospace";
    con.appendChild(box);
    document.getElementById('toggleIQS').addEventListener('click', function(){
        document.body.removeChild(document.getElementById('IQS_container'));
    });
}

function IQS_container(){
    var el = document.getElementById("IQS_container");
    if (!el){
        var box = document.createElement("div");
        box.style.height= h+'px';
        box.style.border= "0";
        box.style.margin= "0";
        box.style.setProperty("display","block","important");
        box.style.position="relative";
        box.style.zIndex= "9999999999";
        box.setAttribute("id","IQS_container");
        document.body.insertBefore(box, document.body.firstChild);
        el = box;
    }
    return el;
}


function scrapeURL(src){
    var packed = src.match(/\(function\(p,a,c,k,e,d\).+\)/gi);
    for (var x in packed){
        src += eval(packed[x]);
    }

    if (!document.getElementById("js_vardump")){
        var ss = document.createElement("script");
        ss.text = "var js_vardump='';Object.getOwnPropertyNames(window).forEach(function(val) {if (typeof(window[val])=='string') js_vardump += '\"'+window[val]+'\"';}); var box = document.createElement('div');box.setAttribute('id','js_vardump');box.style.setProperty('display','none','important');box.innerHTML=js_vardump;document.body.appendChild(box);";
        var hh = document.getElementsByTagName('head')[0];
        hh.appendChild(ss);
        src += document.getElementById("js_vardump").innerHTML;
    }

    var fv = document.getElementsByName("flashvars");
    for (var i=0;i<fv.length;i++){
        src += decodeURIComponent(fv[i].value);
    }

    var match = src.match(/((?:https?:)?\/\/[0-9a-z\:\._-]{5,50}\/+(?:files\/+)?(?:[0-9a-z]{1,2}\/+)?[0-9a-z]{14,}\/+(?!video\.|vid\.|v\.).+?(?:\.(?:mkv|ogm|divx|avi|mp4|flv|webm|mov))+)/i);
    if (!match){
        match = src.match(/((?:https?:)?\/\/[0-9a-z\:\._-]{5,50}\/+(?:files\/+)?(?:[0-9a-z]{1,2}\/+)?[0-9a-z]{14,}\/+.+?(?:\.(?:mkv|ogm|divx|avi|mp4|flv|webm|mov))+)/i);
    }
    if (!match){
        match = src.match(/((?:https?:)?\/\/[0-9a-z\:\._-]{5,50}\/+(?:stream\/+)?[0-9a-z_-]{11}~[0-9]+~[0-9\.]+~[0-9a-z_-]+)/ig);
    }
    if (match){
        return match[match.length-1];
    }
}

function bad_content(content){
    // Check for bad content
    var isBad = content.match(/(does not exist|has been removed|has been deleted|is no longer available|Copyright Infringement|deleted for DMCA|due to inactivity or DMCA|file expired or deleted|File Not Found|File was removed|No such file with this filename|No such user exist|Reason of deletion|Reason for deletion|The file expired|The file was deleted|file deleted\.)/i);
    if (isBad) {
        console.log('Bad link detected');
        console.log(isBad);
    }
    return isBad;
}

function bad_filetype(dlurl){
    // Check for bad content
    var isBad = dlurl.match(/\.(001|002|003|rar|zip)$/i);
    if (isBad) {
        console.log('Bad filetype detected');
        console.log(isBad);
    }
    return isBad;
}

function get_html(){
    return document.getElementsByTagName('head')[0].innerHTML + document.body.innerHTML;
}

function get_visible_text(){
    var clone = document.body.cloneNode(true);
    var el = clone.querySelectorAll('*');
    for (var x=0; x<el.length; x++){
        if (el[x].nodeName == "SCRIPT" || el[x].nodeName == "STYLE" || el[x].nodeName == "NOSCRIPT" || el[x].style.color == "transparent" || el[x].style.display == "none" || el[x].classList.contains("hidden") || el[x].style.visibility == "hidden") el[x].parentNode.removeChild(el[x]);
    }
    return clone.textContent+document.title;
}




// icefilms
if (location.host.match('icefilms') && location.href.match('video.php') && !location.href.match('&sourceid=')){

    document.getElementById('iqs').value=1;
}


// 2shared
else if (location.host.match('2shared.com') && (location.href.match('/file/') || location.href.match('/video/')) ){

    // 200mb filesize limit means not many useful links anymore

    console.log('2Shared detected');

    document.body.style.margin = '0';
    document.getElementById("topNav").style.top = h+'px';

    // check for bad link
    var content = get_html();
    var bad = content.match("The file link that you requested is not valid") ||
        content.match("file is suspected of illegal or copyrighted content") ||
        content.match("VGhlIGZpbGUgbGluayB0aGF0IHlvdSByZXF1ZXN0ZWQgaXMgbm90IHZhbGlkLiBQbGVhc2UgY29udGFjdCBsaW5rIHB1Ymxpc2hlciBvciB0cnkgdG8gbWFrZSBhIHNlYXJjaC4") ||
        content.match(/\.(001|002|003|rar|zip)<\/title>/i);

    if (bad && !document.getElementById('iceVerify')){
        iceVerify();
    }

    // display embed
    else if (!document.getElementById('iceQuickStream'))
    {

        function matchLoop(){
            var matched = 0;
            matched = content.match(/['">](https?:\/\/dc.+)['"<]/i);
            if (!matched){
                setTimeout(function(){matchLoop();},100);
            }else{
                downloadlink = matched[1];
                iceEmbed();
                hostLove();
            }
        }
        matchLoop();

    }
}


// mediafire
else if (location.href.match('www.mediafire.com/download/.+')){

    // unable to identify original URL when 302 bad link is detected, so icefilms cant use mediafire :(
    // this may have changed, but also 200mb filesize limit means not many useful links.

    console.log('Mediafire detected');

    document.getElementById('header').style.position='relative';
    document.getElementById('container').style.paddingTop='0';

    // check for bad link
    var content = get_html();
    var bad = location.href.match("error.php\\?errno=") ||
        content.match(/\.(001|002|003|rar|zip)<\/title>/i);

    // display embed
    if (!document.getElementById('iceQuickStream'))
    {
        var match = document.body.innerHTML.match(/[\'\">](https?:\/\/[0-9a-z\:\._-]{5,50}\/[0-9a-z]{10,16}\/[0-9a-z]{10,16}\/.+?\.(?:mkv|ogm|divx|avi|mp4|flv|webm|mov))[\'\"<]/i);
        if (match){
            downloadlink = match[1];
            iceEmbed();
            hostLove();
        }else{
            console.log('dl not found');
        }
    }
}


// All Xfilesharing-based hosts
else if (
    (location.pathname.match('^/[0-9a-z]{12}(/.*)?(\\.html?)?$')) ||
    (location.pathname.match('^/[0-9a-zA-Z]/[0-9a-zA-Z_-]{11}(/.*)?(\\.html?)?$')) ||
    (document.title.match(/Easy way to share your files/i))
){

    console.log('Xfilesharing detected');

    // unhide url on some hosts
    var inputs = document.getElementsByTagName("input");
    for (var i = 0; i < inputs.length; i++){
        if (inputs[i].type == 'checkbox' && inputs[i].checked === true)
            inputs[i].click();
    }

    // prevent multiple embeds with manual run script
    if (!document.getElementById('iceQuickStream') && !document.getElementById('iceVerify')){
        // check for bad link
        var bad = bad_content(get_visible_text());

        if (bad){
            iceVerify();
        }

        // display embed
        else
        {
            function lookfor_dl_then_embed(){
                console.log('looking for dl');
                downloadlink = scrapeURL(document.body.innerHTML);
                if (downloadlink){
                    document.removeEventListener("click", lookfor_dl_then_embed);
                    if (bad_filetype(downloadlink)){
                        iceVerify();
                    }else{
                        iceEmbed();
                        hostLove();
                    }
                }
            }

            lookfor_dl_then_embed();

            if (!downloadlink){
                console.log('dl not found, adding click listener');
                document.addEventListener("click", lookfor_dl_then_embed);
            }
        }
    }
}


// run on rare 404.html dead link redirect
else if (location.pathname.match('^/404\\.html?$') && document.referrer.match('^https?://[^/]{5,30}/[0-9a-z]{12}(/.+)?(\\.html?)?$')){

    // check for bad link
    var bad = bad_content(get_visible_text());

    if (bad && !document.getElementById('iceVerify')){
        iceVerify(document.referrer);
    }
}