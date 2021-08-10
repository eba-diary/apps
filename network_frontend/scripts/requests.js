const loadData = () => {
  const xmlHttp = new XMLHttpRequest();
  const url = "https://l4acfene95.execute-api.us-west-2.amazonaws.com/api/diary_entries/"
  xmlHttp.onreadystatechange = () => {
    if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
      $.ajax({
          type: "GET",
          url: "data/networks.csv",
          dataType: "text",
          success: (raw) => {processData(raw, JSON.parse(xmlHttp.responseText))}
       })
    }
  }
  xmlHttp.open("GET", url);
  xmlHttp.send(null);
}
