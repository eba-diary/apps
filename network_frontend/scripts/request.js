const authorRequest = (datum) => {
  const xmlHttp = new XMLHttpRequest();
  const url = "https://l4acfene95.execute-api.us-west-2.amazonaws.com/api/authors/Andrews"
  xmlHttp.onreadystatechange = () => {
    if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
      openNav(datum, JSON.parse(xmlHttp.responseText))
    }
  }
  xmlHttp.open("GET", url);
  xmlHttp.send(null);
}
