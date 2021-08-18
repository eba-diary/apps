//EBA Network Analysis Group
//Vincent Wilson
//renders and updates force directed graph

//global vars
let width = window.screen.width - 400,
    height = window.screen.height - 450,
    nodes, links, oldNodes, data, dates

//keeps track of data index
let date_counter = 0

//force simulation and svg where graph is rendered
let svg = d3.select("#render-here")
  .append("svg")
  .attr("viewBox", [-width/2, -height/2, width, height])

//force simulation and parameters
let simulation = d3.forceSimulation()
  .force("link", d3.forceLink())
  .force("charge", d3.forceManyBody().strength(-150))
  .force("x", d3.forceX())
  .force("y", d3.forceY())

//called on page load and when user updates
const render = (update) => {

  //TODO: why does this work and exitNodes doesn't?
  d3.selectAll(".node").remove();
  simulation.nodes(nodes)
  simulation.force("link").links(links).distance(250)
  simulation.alpha(0.3).restart()
  simulation.velocityDecay(0.9)

  let l = svg.selectAll(".link")
    .data(links, function(d) {return d.source + "," + d.target})
  let n = svg.selectAll(".node")
    .data(nodes, function(d) {return d.key})

  enterLinks(l)
  if(update) exitLinks(l)
  enterNodes(n)
  //if(update) exitNodes(n) --doesn't update color for some reason

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
    .attr("stroke", 'white')
    .on('click', (e,d) => {
      getBio(d)
    })
    /* TODO: HIGHLIGHT the most recently clicked node
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

  //move this to its own file
  let search = document.getElementById("search");
  let token = document.getElementById("token")
  search.addEventListener('submit', function() {
    event.preventDefault();

    const options = {
      includeScore: true,
      includeMatches: true,
      ignoreLocation: true,
      minMatchCharLength: token.value.length,
      keys: ['entry_txt']
    }
    const fuse = new Fuse(search_list, options)
    const results = fuse.search(token.value)

    search_results = new Array()
    for(result in results) {
      close_span = "</span>"
      open_span = "<span class=\"token\">"
      span_length = close_span.length + open_span.length
      entry = results[result].item.entry_txt
      start = results[result].matches[0].indices[0][0]
      end = results[result].matches[0].indices[0][1]+1

      head = Math.floor(start/3)
      distance_to_end = entry.length - 1 - end - span_length
      tail = end + span_length + Math.floor(distance_to_end/3)

      let first_span = [entry.slice(0, end), close_span, entry.slice(end)].join('');
      let second_span = [first_span.slice(0, start), open_span, first_span.slice(start)].join('');
      let snippet =  "..." + second_span.substring(head,tail) + "..."
      let date = dates[results[result].refIndex]
      search_results.push({date: date, snippet: snippet})
    }
    openSearchInfo(search_results)
  });


  render(false)
}

//called on slider update
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
  //set emma's position to be fixed at 0,0
  emma = nodes[0]
  emma.fx = 0
  emma.fy = 0
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
