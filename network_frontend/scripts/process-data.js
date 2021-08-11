//EBA Network Analysis Group
//Vincent Wilson
//processed data from CSV
//format [time,source,target] --> [{times,[[edges],[nodes]]}]

//convert 0 to 1 value to hsl color
const getColor = (value) => {
    //value from 0 to 1
    var hue=(value*120).toString(10);
    return ["hsl(",hue,",100%,70%)"].join("");
}

const processData = (raw, entries_json) => {
  let max_score = Number.MIN_SAFE_INTEGER
  let min_score = Number.MAX_SAFE_INTEGER
  let cleaned_data = raw
    .trim()
    .split(/\n/) //split by line
    .map( (line,index) => {
      arr = line.split(/,/) //split by comma
      result = arr.splice(0,6) //only keep the first 6 elements
      //now, get min and max score to use later for color scaling
      score = parseFloat(entries_json[result[2]].entry_sentiment)

      if(score > max_score) {max_score = score}
      if(score < min_score) {min_score = score}

      return result
    })
    .sort(([a], [b]) => a - b)

  //run a map reduce function to group nodes and links by a given time
  let emma = "Emma B. Andrews"
  let bare_nodes = new Array(emma) //this array keeps track of unique node keys
  links_and_nodes_by_date = cleaned_data.reduce((obj,line, index) => {
    date = line[2]
    //need to remove # from target
    //a # in the url tells the browser to go to a specific place on the page
    //#Person_Name can't work as an API Param
    tei_target = line[3].replace('#', '')
    target = line[4]
    entry = entries_json[date].entry_txt
    tei = entries_json[date].entry_tei
    sentiment = entries_json[date].entry_sentiment

    //create new entry whenever a new time is found
    if(!obj[date]) {
      adjusted_sentiment = (sentiment - min_score)/(max_score-min_score)
      color = getColor(adjusted_sentiment)
      bare_nodes = new Array(emma)
      obj[date] = [[],[{
        key: emma,
        date: date,
        entry: entry,
        tei: tei,
        sentiment: adjusted_sentiment,
        color: color,
        weight: 10
      }]]
    }

    //add node if not already in bare_nodes
    //this will need to be done for sources later on
    //there might be connections that don't contain Emma
    if(!(bare_nodes.includes(target)) && target != "None") {
      obj[date][1].push({key: target, tei_target: tei_target, date: date, entry: entry, tei: tei, weight: 5})
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
