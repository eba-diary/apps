//EBA Network Analysis Group
//Vincent Wilson
//processed data from CSV
//format [time,source,target] --> [{times,[[edges],[nodes]]}]

const processData = (raw) => {
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
      obj[time][1].push({key: source})
      bare_nodes.push(source)
    }
    if(!(bare_nodes.includes(target))) {
      obj[time][1].push({key: target})
      bare_nodes.push(target)
    }

    //add link
    //temporary error handling: why is the link sometimes undefined?
    obj[time][0].push({source: bare_nodes.indexOf(source), target: bare_nodes.indexOf(target)})

    return obj
  }, {})

  //pass data and render the first data
  recieveData(links_and_nodes_by_time)
}

//jquery function that handles the reading the file from the machine.
window.onload = (event) => {
  $.ajax({
      type: "GET",
      url: "data/dummy_data.csv",
      dataType: "text",
      success: (raw) => {processData(raw)}
   })
}
