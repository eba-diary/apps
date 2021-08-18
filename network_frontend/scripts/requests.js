const loadData = () => {
  const xmlHttp = new XMLHttpRequest();
  const url = "https://l4acfene95.execute-api.us-west-2.amazonaws.com/api/diary_entries/"
  xmlHttp.onreadystatechange = () => {
    if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
      window.entries_json = JSON.parse(xmlHttp.responseText)
      $.ajax({
          type: "GET",
          url: "data/networks.csv",
          dataType: "text",
          success: (raw) => {processData(raw)}
       })
    }
  }
  xmlHttp.open("GET", url);
  xmlHttp.send(null);
}

//500 error handling needed for this function when bio not in db
const getBio = (node) => {
  const xmlHttp = new XMLHttpRequest();
  const url = "https://l4acfene95.execute-api.us-west-2.amazonaws.com/api/bios/" + node.tei_target
  xmlHttp.onreadystatechange = () => {
    if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
      //is somewhat dizzying to close then open animate
      //closeNodeInfo()
      //setTimeout(function(){openNav(node, JSON.parse(xmlHttp.responseText))}, 600)
      openNodeInfo(node, JSON.parse(xmlHttp.responseText))
    }
  }
  xmlHttp.open("GET", url);
  xmlHttp.send(null);
}
