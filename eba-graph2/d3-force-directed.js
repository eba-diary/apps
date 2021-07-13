let width = window.screen.width,
    height = window.screen.height,
    nodes, links, oldNodes, links_and_nodes_by_time, // data
    svg, node, link, // d3 selections
    force = d3.layout.force()
      .charge(-100)
      .linkDistance(50)
      .size([width, height]);


//function from recycled code that generates random data to graph
// will remove in later version
const randomData = () => {
  //oldNodes = nodes;

  //parse out the large object created in processData()
  //_.random(0, 100) -- sometimes these datasets are undefined
  links = links_and_nodes_by_time[Object.keys(links_and_nodes_by_time)[100]][0]
  nodes = links_and_nodes_by_time[Object.keys(links_and_nodes_by_time)[100]][1]

  console.log(nodes)
  console.log(links)


  /*

  //This code hasn't been implemented yet
  //Its goal is to keep old nodes in the same place that they were in before an update

  if (oldNodes) {
    var add = _.initial(oldNodes, _.random(0, oldNodes.length));
    add = _.rest(add, _.random(0, add.length));

    nodes = _.union(nodes, add);
  }
  maintainNodePositions();
  */
}

//this function will replace update(). It will increment the data in question by 1
const nextData = () => {

}

//called when the page loads
function render() {

  randomData(links_and_nodes_by_time);
  force.nodes(nodes).links(links);

  svg = d3.select("body").append("svg")
    .attr("width", width)
    .attr("height", height);

  var l = svg.selectAll(".link")
    .data(links, function(d) {return d.source + "," + d.target});
  var n = svg.selectAll(".node")
    .data(nodes, function(d) {return d.key});

  enterLinks(l);
  enterNodes(n);

  link = svg.selectAll(".link");
  node = svg.selectAll(".node");

  force.start();
}

/*
function update() {
  randomData();
  force.nodes(nodes).links(links);


  var l = svg.selectAll(".link")
    .data(links, function(d) {return d.source + "," + d.target});
  var n = svg.selectAll(".node")
    .data(nodes, function(d) {return d.key});
  enterLinks(l);
  exitLinks(l);
  enterNodes(n);
  exitNodes(n);
  link = svg.selectAll(".link");
  node = svg.selectAll(".node");

  //link.style("stroke-width", function(d) { return d.weight; });
  link.style("stroke-width", function(d) { return d.weight; });

  //node.select("circle").attr("r", function(d) {return d.weight});
  node.select("circle").attr("r", function(d) {return d.weight});

  force.start();
}
*/


//format nodes
function enterNodes(n) {
  var g = n.enter().append("g")
    .attr("class", "node");

  g.append("circle")
    .attr("cx", 0)
    .attr("cy", 0)
    .attr("r", function(d) {return d.weight}) //d.weight
    .call(force.drag)
    .on('click', function(d, i) {
      alert("node " + d.key + " clicked")
    });

  g.append("text")
    .attr("x", function(d) {return d.weight + 5}) //d.weight + 5
    .attr("dy", ".35em")
    .text(function(d) {return d.key});
}

function exitNodes(n) {
  n.exit().remove();
}

//format links
function enterLinks(l) {
  l.enter().insert("line", ".node")
    .attr("class", "link")
    .style("stroke-width", function(d) { return d.weight; })
}

function exitLinks(l) {
  l.exit().remove();
}

//not yet implemented, will keep old nodes in correct position
function maintainNodePositions() {
  var kv = {};
  _.each(oldNodes, function(d) {
    kv[d.key] = d;
  });
  _.each(nodes, function(d) {
    if (kv[d.key]) {
      // if the node already exists, maintain current position
      d.x = kv[d.key].x;
      d.y = kv[d.key].y;
    } else {
      // else assign it a random position near the center
      d.x = width / 2 + _.random(-150, 150);
      d.y = height / 2 + _.random(-25, 25);
    }
  });
}

//handles every "tick" by updating the node and link positions
//this has a small bug of having everything shudder around
//because from what I understand, it is ticking the parent of the node class, not the node itself?
force.on("tick", function(e) {
  link.attr("x1", d => d.source.x)
      .attr("y1", d => d.source.y)
      .attr("x2", d => d.target.x)
      .attr("y2", d => d.target.y);

  node.attr("transform", function(d) { return "translate(" + d.x + "," + d.y + ")"; });
});

//cleans up data
function processData(raw) {
  let cleaned_data = raw
    .trim()
    .split(/\n/)
    .map(line => {
      const [t, i, j] = line.split(/,/);
      return [t, i, j];
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

//jquery function that handles the reading the file from the machine.
$(document).ready(function() {
  $.ajax({
      type: "GET",
      url: "dummy_data.csv",
      dataType: "text",
      success: function(raw) {processData(raw);}
   });
});
