/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
//TODO: make this thing open and close better
// text shouldn't wrap during close animation
// when a new node is clicked, the nav should first close then open
// when the date changes, the sidebar should close if the node is no longer present
const openNodeInfo = (node, biography) => {
  document.getElementById("node-info-sidebar").style.width = "339px";
  document.getElementById("node-name").innerHTML = node.key
  document.getElementById("node-date").innerHTML = node.date
  document.getElementById("node-entry").innerHTML = node.tei
  document.getElementById("node-person-birth-date").innerHTML = biography.birth
  document.getElementById("node-person-birth-place").innerHTML = biography.birth_place
  document.getElementById("node-person-death-date").innerHTML = biography.death
  document.getElementById("node-person-death-place").innerHTML = biography.death_place
  document.getElementById("node-person-bio").innerHTML = biography.biography
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
const closeNodeInfo = () => {
  document.getElementById("node-info-sidebar").style.width = "0";
}

const openSearchInfo = (search_results) => {
  document.getElementById("search-info-sidebar").style.width = "260px";
  let search_view = document.getElementById("search-view")
  search_view.innerHTML = ""
  if(search_results.length < 1) {
    let result_div = document.createElement("div")
    result_div.innerHTML = "Sorry, we couldn't find any results"
    search_view.appendChild(result_div)
  } else {
    for(result in search_results){
          let result_div = document.createElement("div")
          let date = document.createElement("div")
          let snippet = document.createElement("div")
          let space = document.createElement("br")
          date.innerHTML = search_results[result].date
          snippet.innerHTML = search_results[result].snippet
          result_div.appendChild(date)
          result_div.appendChild(snippet)
          result_div.appendChild(space)
          search_view.appendChild(result_div)
      }
  }

}

const closeSearchInfo = () => {
  document.getElementById("search-info-sidebar").style.width = "0";
}
