//EBA Network Analysis Group
//Vincent Wilson
//renders and updates force directed graph

//global vars
let width = window.screen.width - 400,
    height = window.screen.height - 400,
    nodes, links, oldNodes, data, dates

//keeps track of data index
let date_counter = 0

//force simulation and svg where graph is rendered
let svg = d3.select("body")
  .append("svg")
  .attr("viewBox", [-width/2, -height/2, width, height])

//force simulation and parameters
let simulation = d3.forceSimulation()
  .force("link", d3.forceLink())
  .force("charge", d3.forceManyBody().strength(-190))
  .force("x", d3.forceX())
  .force("y", d3.forceY())

//called on page load and when user updates
const render = (update) => {

  //TODO: why does this work and exitNodes doesn't?
  d3.selectAll(".node").remove();
  simulation.nodes(nodes)
  simulation.force("link").links(links).distance(250)
  simulation.alpha(0.3).restart();

  let l = svg.selectAll(".link")
    .data(links, function(d) {return d.source + "," + d.target})
  let n = svg.selectAll(".node")
    .data(nodes, function(d) {return d.key})

  enterLinks(l)
  if(update) exitLinks(l)
  enterNodes(n)
  //if(update) exitNodes(n) -- why does this not re-render color?

  link = svg.selectAll(".link")
  node = svg.selectAll(".node")


  //handles every simulation "tick" (updates nodes and links)
  simulation.on("tick", function() {
    node.attr("transform", function(d) { return "translate(" + d.x + "," + d.y + ")"; });

    link.attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);
  })
}

//format and add links
const enterLinks = (l) => {
  l.enter().insert("line", ".node")
    .attr("class", "link")
    .style("stroke-width", function(d) { return 2 })
}

//remove links when done
const exitLinks = (l) => {
  l.exit().remove()
}

//format and add nodes
const enterNodes = (n) => {
  var g = n.enter().append("g")
    .attr("class", "node")
    .call(drag(simulation))

  g.append("circle")
    .attr("cx", 0)
    .attr("cy", 0)
    .attr("r", (d) => {return d.weight})
    .attr("fill", (d) => {return d.color})
    .attr("stroke", '#000000')
    .on('click', (event,datum) => {
      openNav(datum)
    })
    /* TODO: highlight the most recently clicked node
    .on('mouseover', function(d, i){
      d3.select(this).style("stroke", '#87ceeb')
    })
    .on('mouseout', function(d, i){
      d3.select(this).style("stroke", 'none')
    })
    */


  g.append("text")
    .attr("x", function(d) {return 15}) //offset text
    .attr("dy", ".35em")
    .text(function(d) {return d.key})
}

//remove nodes when done
const exitNodes = (n) => {
  n.exit().remove()
}


//recieve and run data called once when page loads
const recieveData = (links_and_nodes_by_time) => {


  data = links_and_nodes_by_time
  dates = Object.keys(data)
  links = data[dates[0]][0]
  nodes = data[dates[0]][1]

  //updates date index depending on slider input
  let slider = document.getElementById("date-slider")
  let slider_amount_div = document.getElementById("slider-amount")
  slider_amount_div.innerHTML = dates[0]
  slider.addEventListener('input', function() {
    slider_amount_div.innerHTML = update(slider.value)
  })


  render(false)
}

//called onclick()
function update(date_index) {
  oldNodes = nodes
  links = data[dates[date_index]][0]
  nodes = data[dates[date_index]][1]
  maintainNodePositions()
  render(true)

  return dates[date_index]
}

//keeps existing nodes in the same place that they were
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
