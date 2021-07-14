//EBA Network Analysis Group
//Vincent Wilson
//processes csv data and creates force directed graph

// TODO:
//slider through each time
//maintain node positions, make everything stick around the center etc
//clickable nodes display modal with random info
//clicking nodes queries a demo db

//Bugs:
//when update is clicked, sometimes the network renders with tiny nodes

let width = window.screen.width,
    height = window.screen.height,
    nodes, links, oldNodes, links_and_nodes_by_time, // data
    svg, node, link, // d3 selections
    force = d3.layout.force()
      .charge(-100)
      .linkDistance(50)
      .size([width, height])


//function from recycled code that generates random data to graph
// will remove in later version
const parseData = () => {
  oldNodes = nodes

  //parse out the large object created in processData()
  //random timestamp for now
  random_index = _.random(0,50)

  links = links_and_nodes_by_time[Object.keys(links_and_nodes_by_time)[random_index]][0]
  nodes = links_and_nodes_by_time[Object.keys(links_and_nodes_by_time)[random_index]][1]

  console.log(nodes)
  console.log(links)

  maintainNodePositions()
}

//called when the page loads
function render() {

  parseData(links_and_nodes_by_time)
  force.nodes(nodes).links(links)

  svg = d3.select("body").append("svg")
    .attr("width", width)
    .attr("height", height)


  var l = svg.selectAll(".link")
    .data(links, function(d) {return d.source + "," + d.target})
  var n = svg.selectAll(".node")
    .data(nodes, function(d) {return d.key})

  enterLinks(l)
  enterNodes(n)


  link = svg.selectAll(".link")
  node = svg.selectAll(".node")

  force.start()
}

function update() {
  parseData()

  force.nodes(nodes).links(links)

  var l = svg.selectAll(".link")
    .data(links, function(d) {return d.source + "," + d.target})
  var n = svg.selectAll(".node")
    .data(nodes, function(d) {return d.key})

  enterLinks(l)
  exitLinks(l)
  enterNodes(n)
  exitNodes(n)

  link = svg.selectAll(".link")
  node = svg.selectAll(".node")

  force.start()
}


//format nodes
function enterNodes(n) {
  var g = n.enter().append("g")
    .attr("class", "node")
    .call(force.drag)

  g.append("circle")
    .attr("cx", 0)
    .attr("cy", 0)
    .attr("r", function(d) {return d.weight})
    .on('click', function(d, i) {
      document.getElementById('which-node').innerHTML = "Selected Node: " + d.key
    })
    .on('mouseover', function(d, i){
      d3.select(this).style("fill", 'magenta')
    })
    .on('mouseout', function(d, i){
      d3.select(this).style("fill", 'black')
    })

  g.append("text")
    .attr("x", function(d) {return d.weight + 5}) //offset text
    .attr("dy", ".35em")
    .text(function(d) {return d.key})
}

function exitNodes(n) {
  n.exit().remove()
}

//format links
function enterLinks(l) {
  l.enter().insert("line", ".node")
    .attr("class", "link")
    .style("stroke-width", function(d) { return d.weight })
}

function exitLinks(l) {
  l.exit().remove()
}

//not yet implemented, will keep old nodes in correct position
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
    } else {
      // else assign it a random position near the center
      d.x = width / 2 + _.random(-150, 150)
      d.y = height / 2 + _.random(-25, 25)
    }
  })
}

//handles every "tick" by updating the node and link positions
//this has a small bug of having everything shudder around
//because from what I understand, it is ticking the parent of the node class, not the node itself?
force.on("tick", function(e) {
  link.attr("x1", d => d.source.x)
      .attr("y1", d => d.source.y)
      .attr("x2", d => d.target.x)
      .attr("y2", d => d.target.y)

  node.attr("transform", function(d) { return "translate(" + d.x + "," + d.y + ")" })
})

//cleans up data
function processData(raw) {
  let cleaned_data = raw
    .trim()
    .split(/\n/)
    .map(line => {
      const [t, i, j] = line.split(/,/)
      return [t, i, j]
    })
    .sort(([a], [b]) => a - b)

  //run a map reduce function to group nodes and links by a given time
  let bare_nodes = [] //this array keeps track of unique node keys
  links_and_nodes_by_time = cleaned_data.reduce((obj,link) => {
    let time = link[0]
    let source = link[1]
    let target = link[2]

    //create new entry whenever a new time is found
    if(!obj[time]) {
      bare_nodes = []
      obj[time] = [[],[]]
    }

    //add node if not already in bare_nodes
    if(!(bare_nodes.includes(source))) {
      obj[time][1].push({key: source, weight: 5})
      bare_nodes.push(source)
    }
    if(!(bare_nodes.includes(target))) {
      obj[time][1].push({key: target, weight: 5})
      bare_nodes.push(target)
    }

    //add link
    //temporary error handling: why is the link sometimes undefined?
    obj[time][0].push({source: bare_nodes.indexOf(source), target: bare_nodes.indexOf(target), weight: 1})

    return obj
  }, {})

  render()
}

abrah


//jquery function that handles the reading the file from the machine.
$(document).ready(function() {
  $.ajax({
      type: "GET",
      url: "data/dummy_data.csv",
      dataType: "text",
      success: function(raw) {processData(raw)}
   })
})
