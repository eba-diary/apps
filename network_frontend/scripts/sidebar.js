/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
//TODO: make this thing open and close better
// text shouldn't wrap during close animation
// when a new node is clicked, the nav should first close then open
// when the date changes, the sidebar should close if the node is no longer present
const openNav = (node) => {
  document.getElementById("node-info-sidebar").style.width = "250px";
  document.getElementById("node-name").innerHTML = node.key
  document.getElementById("node-date").innerHTML = node.date
  document.getElementById("node-entry").innerHTML = node.entry
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
const closeNav = () => {
  document.getElementById("node-info-sidebar").style.width = "0";
}
