/* Set the width of the sidebar to 250px and the left margin of the page content to 250px */
const openNav = (node) => {
  console.log(node)
  document.getElementById("node-info-sidebar").style.width = "250px";
  document.getElementById("node-name").innerHTML = node.key
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
const closeNav = () => {
  document.getElementById("node-info-sidebar").style.width = "0";
}
