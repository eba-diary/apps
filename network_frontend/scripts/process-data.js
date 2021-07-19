//EBA Network Analysis Group
//Vincent Wilson
//processed data from CSV
//format [time,source,target] --> [{times,[[edges],[nodes]]}]


//sloppy code to handle commas in xml and journal entries
//TODO: take in json rather than CSVs
const handleCommas = (last_three) => {
  split_by_div = last_three.split("<div")
  entry = split_by_div[0]
  xml = "<div" + split_by_div[1]
  score = last_three.split("/div>")[1] + "/div>"
  try {
    score = score.split(/,/)[1]
    score = score.split("\r")[0]
    score = score.split("/")[1]
  } catch (err) {
    score = "0"
  }
  return [entry, xml, score]
}

const getColor = (value) => {
    //value from 0 to 1
    var hue=(value*120).toString(10);
    return ["hsl(",hue,",100%,50%)"].join("");
}

const processData = (raw) => {
  let max_score = Number.MIN_SAFE_INTEGER
  let min_score = Number.MAX_SAFE_INTEGER
  let cleaned_data = raw
    .trim()
    .split(/\n/) //split by line
    .map(line => {
      arr = line.split(/,/) //split by comma
      result = arr.splice(0,6) //only keep the first 6 elements
      last_three = arr.join(/,/) //put the last 3 elements back
      last_three = handleCommas(last_three)
      result = result.concat(last_three) //handle commas and put arrays back together

      //now, get min and max score to use later for color scaling
      score = parseFloat(last_three[2])
      if(score > max_score) {max_score = score}
      if(score < min_score) {min_score = score}

      return result
    })
    .sort(([a], [b]) => a - b)
  cleaned_data.shift() //remove row headers

  //run a map reduce function to group nodes and links by a given time
  let emma = "Emma B. Andrews"
  let bare_nodes = new Array(emma) //this array keeps track of unique node keys
  links_and_nodes_by_date = cleaned_data.reduce((obj,line) => {
    date = line[2]
    source = "Emma B. Andrews"
    target = line[4]
    entry = line[6]
    sentiment = line[8]

    //create new entry whenever a new time is found
    if(!obj[date]) {
      adjusted_sentiment = (sentiment - min_score)/(max_score-min_score)
      color = getColor(adjusted_sentiment)
      console.log(adjusted_sentiment+":"+color)
      bare_nodes = new Array(emma)
      obj[date] = [[],[{
        key: emma,
        weight: 10,
        entry: entry,
        sentiment: adjusted_sentiment,
        color: color
      }]]
    }

    //add node if not already in bare_nodes
    if(!(bare_nodes.includes(target)) && target != "None") {
      obj[date][1].push({key: target, weight: 5, entry:  entry})
      bare_nodes.push(target)
    }

    //add link
    //temporary error handling: why is the link sometimes undefined?
    if(target != "None") {
      obj[date][0].push({source: 0, target: bare_nodes.indexOf(target)})
    }

    return obj
  }, {})

  //pass data and render the first data
  console.log(links_and_nodes_by_date)
  recieveData(links_and_nodes_by_date)
}

//jquery function that handles the reading the file from the machine.
//this is where the program enters
window.onload = (event) => {
  $.ajax({
      type: "GET",
      url: "data/networks.csv",
      dataType: "text",
      success: (raw) => {processData(raw)}
   })
}
