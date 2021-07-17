//EBA Network Analysis Group
//Vincent Wilson
//renders and updates force directed graph

//global vars
let width = window.screen.width,
    height = window.screen.height,
    nodes, links, oldNodes, data

//keeps track of data index
let date_counter = 0

//force simulation and svg where graph is rendered
let svg = d3.select("body")
  .append("svg")
  .attr("viewBox", [-width/2, -height/2, width, height])

//link and node "list" within SVG

let link = svg.append("g")
        .attr("class", "link")
      .selectAll("line")
//let node_parent = svg.append("g")
let node = svg.append("g")
        .attr("class", "node")
      .selectAll("circle")


//handles every simulation "tick" (updates nodes and links)
const ticked = () => {
  node.attr("cx", d => d.x)
        .attr("cy", d => d.y);

  link.attr("x1", d => d.source.x)
      .attr("y1", d => d.source.y)
      .attr("x2", d => d.target.x)
      .attr("y2", d => d.target.y);
}


//force simulation and parameters
let simulation = d3.forceSimulation()
  .force("link", d3.forceLink())
  .force("charge", d3.forceManyBody().strength(-100))
  .force("x", d3.forceX())
  .force("y", d3.forceY())
  .on("tick", ticked)

//recieve and run data called once when page loads
const recieveData = (links_and_nodes_by_time) => {

  let slider = document.getElementById("date-slider")
  let sliderDiv = document.getElementById("sliderAmount")

  slider.addEventListener('input', function() {
    sliderDiv.innerHTML = slider.value
    update(slider.value)
  })


  data = links_and_nodes_by_time
  links = data[Object.keys(data)[0]][0]
  nodes = data[Object.keys(data)[0]][1]
  render()
}

//called on page load and when user updates
const render = () => {
  simulation.nodes(nodes)
  simulation.force("link").links(links)
  simulation.alpha(0.3).restart();

  node = node
    .data(nodes)
    .join(enter => enter.append("circle").attr("r",5)
      .call(node => node.append("text").text(d => d.key))
    )
    .on('mouseover', function(d, i){
      d3.select(this).style("fill", 'magenta')
    })
    .on('mouseout', function(d, i){
      d3.select(this).style("fill", 'black')
    })
    .call(drag(simulation))

  link = link
        .data(links)
        .join("line")

}

//called onclick()
function update(date_index) {
  oldNodes = nodes
  links = data[Object.keys(data)[date_index]][0]
  nodes = data[Object.keys(data)[date_index]][1]
  maintainNodePositions()
  render()
}

//keeps existing nodes in the same place they should be
function maintainNodePositions() {
  var kv = {}
  _.each(oldNodes, function(d) {
    kv[d.key] = d
  })
  _.each(nodes, function(d) {
    if (kv[d.key]) {
      // if the node already exists, maintain current position
      d.x = kv[d.key].x
      d.y = kv[d.key].y
    }
  })
}

//handles drag events
const drag = (simulation) => {

  function dragstarted(event) {
    if (!event.active) simulation.alphaTarget(0.3).restart();
    event.subject.fx = event.subject.x;
    event.subject.fy = event.subject.y;
  }

  function dragged(event) {
    event.subject.fx = event.x;
    event.subject.fy = event.y;
  }

  function dragended(event) {
    if (!event.active) simulation.alphaTarget(0);
    event.subject.fx = null;
    event.subject.fy = null;
  }

  return d3.drag()
      .on("start", dragstarted)
      .on("drag", dragged)
      .on("end", dragended);
}
